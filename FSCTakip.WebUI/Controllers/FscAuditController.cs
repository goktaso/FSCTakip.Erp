using FSCTakip.Core.Entities;
using FSCTakip.DataAc.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;

namespace FSC_ERP.Controllers
{
    public class FscAuditController : BaseController
    {
        public FscAuditController(AppDbContext context) : base(context) { }

        public async Task<IActionResult> Index()
        {
            var list = await _context.FscAudits
                .OrderByDescending(a => a.AuditDate)
                .ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var audit = await _context.FscAudits
                .Include(a => a.Items)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (audit == null) return NotFound();
            return View(audit);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var count = await _context.FscAudits.CountAsync();
            return View(new FscAudit
            {
                AuditDate = DateTime.Today,
                AuditType = "İç Denetim",
                Status = "Planlandı",
                AuditCode = $"FSC-DNT-{DateTime.Now:yyyy}-{(count + 1):D2}"
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(FscAudit model)
        {
            if (string.IsNullOrWhiteSpace(model.AuditCode))
            {
                var count = await _context.FscAudits.CountAsync();
                model.AuditCode = $"FSC-DNT-{DateTime.Now:yyyy}-{(count + 1):D2}";
            }
            _context.FscAudits.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Checklist), new { id = model.Id });
        }

        public async Task<IActionResult> Checklist(int id)
        {
            var audit = await _context.FscAudits
                .Include(a => a.Items)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (audit == null) return NotFound();

            // Varsayılan checklist maddeleri yoksa ekle
            if (audit.Items.Count == 0)
            {
                var defaultItems = GetDefaultChecklist();
                foreach (var (cat, req, order) in defaultItems)
                {
                    audit.Items.Add(new FscAuditItem
                    {
                        Category = cat,
                        Requirement = req,
                        ConformStatus = "Değerlendirilmedi",
                        SortOrder = order
                    });
                }
                await _context.SaveChangesAsync();
            }
            return View(audit);
        }

        [HttpPost]
        public async Task<IActionResult> SaveChecklist(List<FscAuditItem> Items, int FscAuditId)
        {
            if (Items == null) return RedirectToAction(nameof(Checklist), new { id = FscAuditId });
            foreach (var model in Items)
            {
                model.FscAuditId = FscAuditId;
                var item = await _context.FscAuditItems.FindAsync(model.Id);
                if (item == null)
                {
                    _context.FscAuditItems.Add(model);
                }
                else
                {
                    item.ConformStatus = model.ConformStatus;
                    item.Evidence = model.Evidence;
                    item.NonConformity = model.NonConformity;
                    item.CorrectiveAction = model.CorrectiveAction;
                }
            }
            await _context.SaveChangesAsync();
            TempData["Success"] = "Checklist kaydedildi.";
            return RedirectToAction(nameof(Checklist), new { id = FscAuditId });
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ImportMasterData(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ImportError"] = "Lütfen bir Excel (.xlsx) dosyası seçin.";
                return RedirectToAction(nameof(Import));
            }
            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ImportError"] = "Sadece .xlsx formatı desteklenir. .xlsb dosyasını Excel'de açıp 'Farklı Kaydet' ile .xlsx olarak kaydedin.";
                return RedirectToAction(nameof(Import));
            }

            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);
                var results = new List<string>();

                foreach (var sheet in workbook.Worksheets)
                {
                    var (count, msg) = await TryImportSheet(sheet, results);
                    if (count > 0) results.Add($"{sheet.Name}: {count} kayıt - {msg}");
                }

                TempData["ImportSuccess"] = results.Count > 0 ? string.Join(" | ", results) : "İçe aktarılacak uyumlu veri bulunamadı.";
                if (results.Count == 0 && workbook.Worksheets.Count > 0)
                    TempData["ImportInfo"] = "Excel yapısını kontrol edin. Sütun başlıkları: FSC Tipi için 'Code/Name', Tedarikçi için 'SupplierCode/Name/FscCode', Lot için 'LotNo/FscType/Supplier' olmalı.";
            }
            catch (Exception ex)
            {
                TempData["ImportError"] = "İçe aktarma hatası: " + ex.Message;
            }
            return RedirectToAction(nameof(Import));
        }

        private async Task<(int count, string message)> TryImportSheet(IXLWorksheet sheet, List<string> logs)
        {
            var headerRow = sheet.FirstRow();
            if (headerRow == null) return (0, "");

            var headers = headerRow.Cells().Select(c => c.GetString()?.ToLowerInvariant() ?? "").ToList();
            int count = 0;
            int GetColIndex(params string[] parts)
            {
                var idx = headers.FindIndex(h => parts.Any(p => h.Contains(p)));
                return idx >= 0 ? idx + 1 : 0;
            }

            // FSC Types: Code, Name, Description
            int codeCol = GetColIndex("code", "kod");
            int nameCol = GetColIndex("name", "ad");
            if (codeCol > 0 && nameCol > 0)
            {
                var lastRow = sheet.LastRowUsed();
                var maxRow = lastRow != null ? lastRow.RowNumber() : 1;
                for (int row = 2; row <= maxRow; row++)
                {
                    var code = sheet.Cell(row, codeCol).GetString()?.Trim() ?? "";
                    var name = sheet.Cell(row, nameCol).GetString()?.Trim() ?? "";
                    if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(name)) continue;
                    if (await _context.FscTypes.AnyAsync(f => f.Code == code || f.Name == name)) continue;

                    _context.FscTypes.Add(new FSCTakip.Core.Entities.FscType
                    {
                        Code = code,
                        Name = name,
                        Description = GetCellByHeader(sheet, row, headers, "description", "açıklama"),
                        IsActive = true
                    });
                    count++;
                }
                if (count > 0) await _context.SaveChangesAsync();
                return (count, "FSC Tipi");
            }

            // Suppliers: SupplierCode, Name, FscCode
            int supCol = GetColIndex("supplier", "tedarik", "tedarikci");
            nameCol = GetColIndex("name", "firma", "ad");
            if (supCol > 0 || nameCol > 0)
            {
                var lastRow = sheet.LastRowUsed();
                var maxRow = lastRow != null ? lastRow.RowNumber() : 1;
                for (int row = 2; row <= maxRow; row++)
                {
                    var code = GetCellByHeader(sheet, row, headers, "suppliercode", "tedarikci", "code", "kod");
                    var name = GetCellByHeader(sheet, row, headers, "name", "firma", "ad");
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    if (await _context.Suppliers.AnyAsync(s => s.SupplierCode == code || s.Name == name)) continue;

                    _context.Suppliers.Add(new Supplier
                    {
                        SupplierCode = code ?? ("TED-" + (await _context.Suppliers.CountAsync() + 1)),
                        Name = name ?? "",
                        FscCode = GetCellByHeader(sheet, row, headers, "fsccode", "fsc"),
                        IsFscActive = true
                    });
                    count++;
                }
                if (count > 0) await _context.SaveChangesAsync();
                return (count, "Tedarikçi");
            }

            return (0, "");
        }

        private static string GetCellByHeader(IXLWorksheet sheet, int row, List<string> headers, params string[] searchTerms)
        {
            for (int i = 0; i < headers.Count; i++)
            {
                if (searchTerms.Any(t => headers[i].Contains(t)))
                    return sheet.Cell(row, i + 1).GetString() ?? "";
            }
            return "";
        }

        private static List<(string Category, string Requirement, int Order)> GetDefaultChecklist()
        {
            return new List<(string, string, int)>
            {
                ("Tedarik", "Tedarikçi FSC sertifikaları güncel mi?", 1),
                ("Tedarik", "FSC'li hammadde girişlerinde fatura/irsaliye eşleşmesi var mı?", 2),
                ("Tedarik", "Lot/Seri numaralandırma kurallarına uyuluyor mu?", 3),
                ("Stok", "FSC'li / FSC'siz malzeme fiziksel ayrımı yapılmış mı?", 4),
                ("Stok", "Stok kayıtları denetim izlenebilirliğine uygun mu?", 5),
                ("Üretim", "Üretim sürecinde FSC karışım oranları takip ediliyor mu?", 6),
                ("Üretim", "Fire/atık yönetimi FSC kurallarına uygun mu?", 7),
                ("Müşteri", "Müşteri FSC lisans kodları güncel mi?", 8),
                ("Müşteri", "Sevkiyat belgelerinde FSC bilgisi eksiksiz mi?", 9),
                ("Dokümantasyon", "Sertifika ve eğitim kayıtları mevcut mu?", 10),
                ("Dokümantasyon", "İç denetim planı uygulanıyor mu?", 11)
            };
        }

        public async Task<IActionResult> ExportAudits()
        {
            var data = await _context.FscAudits
                .Include(a => a.Items)
                .OrderByDescending(a => a.AuditDate)
                .ToListAsync();
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("FSC_Denetimler");
            ws.Cell(1, 1).Value = "Kod"; ws.Cell(1, 2).Value = "Başlık"; ws.Cell(1, 3).Value = "Tarih";
            ws.Cell(1, 4).Value = "Denetçi"; ws.Cell(1, 5).Value = "Tip"; ws.Cell(1, 6).Value = "Durum";
            int r = 2;
            foreach (var a in data)
            {
                ws.Cell(r, 1).Value = a.AuditCode; ws.Cell(r, 2).Value = a.Title;
                ws.Cell(r, 3).Value = a.AuditDate.ToString("dd.MM.yyyy");
                ws.Cell(r, 4).Value = a.AuditorName; ws.Cell(r, 5).Value = a.AuditType; ws.Cell(r, 6).Value = a.Status;
                r++;
            }
            ws.Columns().AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"FSC_Denetim_{DateTime.Now:ddMMyyyy}.xlsx");
        }
    }
}
