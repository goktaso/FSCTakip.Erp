using ClosedXML.Excel;
using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FSCTakip.WebUI.Controllers
{
    public class EtlController : BaseController
    {
        private readonly IConfiguration _cfg;
        public EtlController(AppDbContext context, IConfiguration cfg) : base(context) { _cfg = cfg; }

        // ─── Index — Dashboard ────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "ERP Entegrasyonu";
            ViewBag.TotalConnections  = await _context.EtlConnections.CountAsync();
            ViewBag.ActiveConnections = await _context.EtlConnections.CountAsync(c => c.IsActive);
            ViewBag.TotalJobs         = await _context.EtlJobs.CountAsync();
            ViewBag.SuccessJobs       = await _context.EtlJobs.CountAsync(j => j.Status == "Completed");
            ViewBag.RecentJobs        = await _context.EtlJobs
                .Include(j => j.EtlConnection)
                .OrderByDescending(j => j.StartedAt)
                .Take(5)
                .ToListAsync();
            return View();
        }

        // ─── Bağlantı Yönetimi ────────────────────────────────────────────────
        public async Task<IActionResult> Connections()
        {
            ViewData["Title"] = "Bağlantı Yönetimi";
            var list = await _context.EtlConnections.OrderByDescending(c => c.CreatedDate).ToListAsync();
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetConnection(int id)
        {
            var c = await _context.EtlConnections.FindAsync(id);
            if (c == null) return Json(new { success = false });
            return Json(new { success = true, data = new {
                c.Id, c.Name, c.Type, c.Description, c.Settings, c.IsActive
            }});
        }

        [HttpPost]
        public async Task<IActionResult> SaveConnection(EtlConnection model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return Json(new { success = false, message = "Bağlantı adı zorunludur." });

            if (model.Id == 0)
            {
                model.IsActive    = true;
                model.CreatedDate = DateTime.Now;
                model.CreatedBy   = "SYSTEM";
                _context.EtlConnections.Add(model);
            }
            else
            {
                var existing = await _context.EtlConnections.FindAsync(model.Id);
                if (existing == null) return Json(new { success = false, message = "Kayıt bulunamadı." });
                existing.Name        = model.Name;
                existing.Type        = model.Type;
                existing.Description = model.Description;
                existing.Settings    = model.Settings;
                existing.UpdatedDate = DateTime.Now;
                existing.UpdatedBy   = "SYSTEM";
            }
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleConnection(int id)
        {
            var c = await _context.EtlConnections.FindAsync(id);
            if (c == null) return Json(new { success = false, message = "Kayıt bulunamadı." });
            c.IsActive    = !c.IsActive;
            c.UpdatedDate = DateTime.Now;
            c.UpdatedBy   = "SYSTEM";
            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = c.IsActive });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConnection(int id)
        {
            var hasJobs = await _context.EtlJobs.AnyAsync(j => j.EtlConnectionId == id);
            if (hasJobs) return Json(new { success = false, message = "Bu bağlantıya ait aktarım geçmişi mevcut, silinemez." });
            var c = await _context.EtlConnections.FindAsync(id);
            if (c == null) return Json(new { success = false, message = "Kayıt bulunamadı." });
            _context.EtlConnections.Remove(c);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // ─── Aktarım Geçmişi ─────────────────────────────────────────────────
        public async Task<IActionResult> History()
        {
            ViewData["Title"] = "Aktarım Geçmişi";
            var list = await _context.EtlJobs
                .Include(j => j.EtlConnection)
                .OrderByDescending(j => j.StartedAt)
                .ToListAsync();
            return View(list);
        }

        // ─── Excel Aktarımı ───────────────────────────────────────────────────
        public async Task<IActionResult> Import()
        {
            ViewData["Title"] = "Excel Aktarımı";
            ViewBag.Connections = await _context.EtlConnections.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ImportPreview(IFormFile file, string jobType)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Dosya seçilmedi." });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls")
                return Json(new { success = false, message = "Yalnızca Excel dosyası (.xlsx) kabul edilir." });

            try
            {
                using var stream = file.OpenReadStream();
                using var wb     = new XLWorkbook(stream);
                var ws = wb.Worksheets.First();

                var rows    = ws.RangeUsed()?.RowsUsed().Skip(1).ToList() ?? new List<IXLRangeRow>();
                var headers = ws.Row(1).CellsUsed().Select(c => c.GetString().Trim()).ToList();
                var preview = rows.Take(10).Select(r =>
                    r.Cells().Select(c => c.GetString()).ToList()
                ).ToList();

                return Json(new { success = true, headers, preview, totalRows = rows.Count });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Dosya okunamadı: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ImportExecute(IFormFile file, string jobType, int? connectionId)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Dosya seçilmedi." });

            var job = new EtlJob
            {
                EtlConnectionId = connectionId,
                JobType         = jobType,
                Source          = "Manuel Excel",
                Status          = "Running",
                StartedAt       = DateTime.Now,
                SourceFile      = file.FileName,
                CreatedDate     = DateTime.Now,
                CreatedBy       = "SYSTEM"
            };
            _context.EtlJobs.Add(job);
            await _context.SaveChangesAsync();

            var errors = new List<string>();
            int inserted = 0, updated = 0, skipped = 0;

            try
            {
                using var stream = file.OpenReadStream();
                using var wb     = new XLWorkbook(stream);
                var ws   = wb.Worksheets.First();
                var rows = ws.RangeUsed()?.RowsUsed().Skip(1).ToList() ?? new List<IXLRangeRow>();

                job.TotalRecords = rows.Count;

                // Başlık satırını oku (ETL otomatik algılama için)
                var headerRow = ws.RangeUsed()?.RowsUsed().FirstOrDefault();
                var headers   = headerRow != null
                    ? headerRow.CellsUsed().Select((c, i) => new { Col = i + 1, Name = c.GetString().Trim() })
                              .ToDictionary(x => x.Name, x => x.Col, StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                if (jobType == "ProductImport")
                    (inserted, updated, skipped, errors) = await ImportProducts(rows);
                else if (jobType == "SupplierImport")
                    (inserted, updated, skipped, errors) = await ImportSuppliers(rows);
                else if (jobType == "CustomerImport")
                    (inserted, updated, skipped, errors) = await ImportCustomers(rows);
                else if (jobType == "LotImport")
                    (inserted, updated, skipped, errors) = await ImportLots(rows);
                else if (jobType == "UretimImport")
                    (inserted, updated, skipped, errors) = await ImportUretim(rows);
                else if (jobType == "SatisImport")
                    (inserted, updated, skipped, errors) = await ImportSatis(rows);
                else if (jobType == "EtlImport")
                    (inserted, updated, skipped, errors) = await ImportFromEtlFile(rows, headers);

                job.Status        = errors.Count == 0 ? "Completed" : (inserted + updated > 0 ? "Partial" : "Failed");
                job.InsertedCount = inserted;
                job.UpdatedCount  = updated;
                job.SkippedCount  = skipped;
                job.ErrorCount    = errors.Count;
                job.CompletedAt   = DateTime.Now;
                job.ErrorDetails  = errors.Count > 0 ? string.Join("\n", errors) : null;

                if (connectionId.HasValue)
                {
                    var conn = await _context.EtlConnections.FindAsync(connectionId.Value);
                    if (conn != null)
                    {
                        conn.LastSyncAt     = DateTime.Now;
                        conn.LastSyncStatus = job.Status;
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, inserted, updated, skipped, errorCount = errors.Count, errors, jobId = job.Id });
            }
            catch (Exception ex)
            {
                job.Status        = "Failed";
                job.ErrorDetails  = ex.Message;
                job.CompletedAt   = DateTime.Now;
                await _context.SaveChangesAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ─── Özel indir şablonları ────────────────────────────────────────────
        public async Task<IActionResult> DownloadTemplate(string type)
        {
            // DB'den referans veriler
            var fscTypes    = await _context.FscTypes.OrderBy(f => f.Name).ToListAsync();
            var suppliers   = await _context.Suppliers.Where(s => s.IsActive).OrderBy(s => s.SupplierCode).ToListAsync();
            var customers   = await _context.Customers.Where(c => c.IsActive).OrderBy(c => c.CustomerCode).ToListAsync();
            var products    = await _context.Products.Where(p => p.IsActive).OrderBy(p => p.ProductCode).ToListAsync();
            var machines    = await _context.Machines.OrderBy(m => m.Name).ToListAsync();
            var groups      = await _context.ProductGroups.OrderBy(g => g.GroupName).ToListAsync();

            using var wb = new XLWorkbook();

            // Renk sabitleri
            var headerBg    = XLColor.FromHtml("#1e3d14");
            var headerBg2   = XLColor.FromHtml("#2d5a1e");
            var reqBg       = XLColor.FromHtml("#fff7ed");
            var optBg       = XLColor.FromHtml("#f0fdf4");
            var noteColor   = XLColor.FromHtml("#6b7280");
            var exampleBg   = XLColor.FromHtml("#f8fafc");

            // Referans sayfası (gizli — dropdown kaynağı)
            var wsRef = wb.AddWorksheet("_Referans");
            wsRef.Visibility = XLWorksheetVisibility.Hidden;

            // FSC Tipleri (A sütunu)
            wsRef.Cell(1,1).Value = "FSC_Tipleri";
            for (int i = 0; i < fscTypes.Count; i++)
                wsRef.Cell(i+2, 1).Value = fscTypes[i].Name;

            // Tedarikçi Kodları (B sütunu)
            wsRef.Cell(1,2).Value = "Tedarikci_Kodlari";
            for (int i = 0; i < suppliers.Count; i++)
                wsRef.Cell(i+2, 2).Value = $"{suppliers[i].SupplierCode} - {suppliers[i].Name}";

            // Müşteri Kodları (C sütunu)
            wsRef.Cell(1,3).Value = "Musteri_Kodlari";
            for (int i = 0; i < customers.Count; i++)
                wsRef.Cell(i+2, 3).Value = $"{customers[i].CustomerCode} - {customers[i].Name}";

            // Ürün Kodları (D sütunu)
            wsRef.Cell(1,4).Value = "Urun_Kodlari";
            for (int i = 0; i < products.Count; i++)
                wsRef.Cell(i+2, 4).Value = $"{products[i].ProductCode} - {products[i].ProductName}";

            // Makine Adları (E sütunu)
            wsRef.Cell(1,5).Value = "Makineler";
            for (int i = 0; i < machines.Count; i++)
                wsRef.Cell(i+2, 5).Value = machines[i].Name;

            // Grup Adları (F sütunu)
            wsRef.Cell(1,6).Value = "Gruplar";
            for (int i = 0; i < groups.Count; i++)
                wsRef.Cell(i+2, 6).Value = groups[i].GroupName;

            // Adlandırılmış aralıklar
            int fscCount   = Math.Max(fscTypes.Count, 1);
            int supCount   = Math.Max(suppliers.Count, 1);
            int cusCount   = Math.Max(customers.Count, 1);
            int prdCount   = Math.Max(products.Count, 1);
            int mchCount   = Math.Max(machines.Count, 1);
            int grpCount   = Math.Max(groups.Count, 1);

            wb.NamedRanges.Add("FSC_Tipleri",   wsRef.Range(2, 1, fscCount+1, 1));
            wb.NamedRanges.Add("Tedarikci_Kod", wsRef.Range(2, 2, supCount+1, 2));
            wb.NamedRanges.Add("Musteri_Kod",   wsRef.Range(2, 3, cusCount+1, 3));
            wb.NamedRanges.Add("Urun_Kodu",     wsRef.Range(2, 4, prdCount+1, 4));
            wb.NamedRanges.Add("Makineler",     wsRef.Range(2, 5, mchCount+1, 5));
            wb.NamedRanges.Add("Gruplar",       wsRef.Range(2, 6, grpCount+1, 6));

            IXLWorksheet ws;

            // ── Yerel yardımcı: başlık satırı yaz ──────────────────────────
            void WriteHeaders(IXLWorksheet sheet, (string col, bool required, string hint)[] defs)
            {
                for (int i = 0; i < defs.Length; i++)
                {
                    var cell = sheet.Cell(1, i+1);
                    cell.Value = defs[i].required ? defs[i].col + " *" : defs[i].col;
                    cell.Style.Font.Bold      = true;
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Fill.BackgroundColor = defs[i].required ? headerBg : headerBg2;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    if (!string.IsNullOrWhiteSpace(defs[i].hint))
                        cell.GetComment().AddText(defs[i].hint);
                }
            }

            void StyleExampleRow(IXLWorksheet sheet, int row, int colCount)
            {
                var r = sheet.Range(row, 1, row, colCount);
                r.Style.Fill.BackgroundColor = exampleBg;
                r.Style.Font.Italic = true;
                r.Style.Font.FontColor = XLColor.FromHtml("#374151");
            }

            // ─── LotImport ────────────────────────────────────────────────
            if (type == "LotImport")
            {
                ws = wb.AddWorksheet("HammaddeLot");
                var defs = new (string, bool, string)[] {
                    ("LotNo",          true,  "Tedarikçi tarafından verilen lot/parti numarası.\nÖr: 24H0537\nAynı LotNo ile birden fazla seri (bobin) satırı ekleyebilirsiniz."),
                    ("TedarikciKodu",  true,  "Sistemdeki tedarikçi kodu (TED-XXX formatı).\nGeçerli kodlar için Referans sayfasına bakın.\nKod yerine tam isim de kabul edilir."),
                    ("UrunKodu",       true,  "Hammadde ürün kodu.\nÖr: 10255 veya HM-001\nGeçerli kodlar için Referans sayfasına bakın."),
                    ("FscTipi",        true,  "FSC sertifika tipi. Aşağıdaki listeden seçin:\n" + string.Join("\n", fscTypes.Select(f => "• " + f.Name))),
                    ("SeriNo",         false, "Bobin seri numarası. Boş bırakılırsa otomatik üretilir.\nÖrnek: 24H0537-01\nAynı lot için her satıra farklı seri numarası girin."),
                    ("Miktar",         true,  "Bobin ağırlığı KG cinsinden.\nOndalık ayırıcı: nokta (.) veya virgül (,)\nÖr: 1757.50 veya 1757,50"),
                    ("AlisIrsaliyeNo", false, "Alış irsaliye numarası. Ör: ORS202400001"),
                    ("AlisFaturaNo",   false, "Alış fatura numarası. Ör: FAT202400001"),
                    ("GirisTarihi",    false, "Giriş tarihi. Format: gg.AA.yyyy\nÖr: 15.05.2025"),
                    ("Plaka",          false, "Araç plakası. Ör: 34ABC123"),
                    ("Notlar",         false, "Ek notlar (opsiyonel)")
                };
                WriteHeaders(ws, defs);

                // Örnek satır
                ws.Cell(2,1).Value="24H0537"; ws.Cell(2,2).Value= suppliers.FirstOrDefault()?.SupplierCode ?? "TED-001";
                ws.Cell(2,3).Value= products.FirstOrDefault()?.ProductCode ?? "10255";
                ws.Cell(2,4).Value= fscTypes.FirstOrDefault()?.Name ?? "FSC_MIX";
                ws.Cell(2,5).Value="24H0537-01"; ws.Cell(2,6).Value=1757.50;
                ws.Cell(2,7).Value="ORS202400001"; ws.Cell(2,8).Value="FAT202400001";
                ws.Cell(2,9).Value="15.05.2025"; ws.Cell(2,10).Value="34ABC123";
                StyleExampleRow(ws, 2, defs.Length);

                ws.Cell(3,1).Value="24H0537"; ws.Cell(3,2).Value= suppliers.FirstOrDefault()?.SupplierCode ?? "TED-001";
                ws.Cell(3,3).Value= products.FirstOrDefault()?.ProductCode ?? "10255";
                ws.Cell(3,4).Value= fscTypes.FirstOrDefault()?.Name ?? "FSC_MIX";
                ws.Cell(3,5).Value="24H0537-02"; ws.Cell(3,6).Value=1820;
                ws.Cell(3,7).Value="ORS202400001"; ws.Cell(3,8).Value="FAT202400001";
                ws.Cell(3,9).Value="15.05.2025"; ws.Cell(3,10).Value="34ABC123";
                StyleExampleRow(ws, 3, defs.Length);

                // Tarih format sütunu
                ws.Column(9).Style.NumberFormat.Format = "@"; // metin olarak sakla

                // FscTipi dropdown (D sütunu)
                if (fscTypes.Any())
                    ws.Range("D4:D10000").SetDataValidation().List(wsRef.Range(2,1, fscCount+1,1), true);

                // Sayısal format (Miktar)
                ws.Range("F4:F10000").Style.NumberFormat.Format = "#,##0.00";
            }
            // ─── UretimImport ────────────────────────────────────────────
            else if (type == "UretimImport")
            {
                ws = wb.AddWorksheet("UretimKaydi");
                var defs = new (string, bool, string)[] {
                    ("UretimNo",       true,  "Üretim fişi numarası. Tekrarlanabilir (aynı üretim için birden fazla hammadde).\nÖr: URE-2025-001"),
                    ("Tarih",          true,  "Üretim tarihi. Format: gg.AA.yyyy\nÖr: 15.05.2025"),
                    ("Makine",         true,  "Makine adı veya kodu (sistemde tanımlı olmalı).\nGeçerli makineler için Referans sayfasına bakın."),
                    ("MamulKodu",      true,  "Üretilen mamul ürün kodu.\nÖr: 30001\nGeçerli kodlar için Referans sayfasına bakın."),
                    ("UretimMiktari",  true,  "Üretilen adet miktarı (tamsayı).\nÖr: 6600"),
                    ("LotNo",          true,  "Tüketilen hammaddenin LOT numarası (sistemde kayıtlı olmalı).\nÖr: 24H0537"),
                    ("HammaddeKodu",   true,  "Tüketilen hammadde ürün kodu.\nÖr: 10255"),
                    ("KullanilanMiktar", true, "Tüketilen hammadde miktarı KG.\nOndalık: nokta veya virgül.\nÖr: 1500.00"),
                    ("Fire",           false, "Fire/atık miktarı KG. Boş bırakılabilir.\nÖr: 12.50"),
                    ("Notlar",         false, "Ek notlar (opsiyonel)")
                };
                WriteHeaders(ws, defs);

                var machName  = machines.FirstOrDefault()?.Name ?? "DILME";
                var prodCode  = products.FirstOrDefault(p => !p.ProductCode.StartsWith("HM"))?.ProductCode ?? "30001";
                var hmCode    = products.FirstOrDefault()?.ProductCode ?? "10255";

                ws.Cell(2,1).Value="URE-2025-001"; ws.Cell(2,2).Value="15.05.2025"; ws.Cell(2,3).Value=machName;
                ws.Cell(2,4).Value=prodCode;        ws.Cell(2,5).Value=6600;         ws.Cell(2,6).Value="24H0537";
                ws.Cell(2,7).Value=hmCode;          ws.Cell(2,8).Value=1500;         ws.Cell(2,9).Value=12;
                StyleExampleRow(ws, 2, defs.Length);

                ws.Cell(3,1).Value="URE-2025-001"; ws.Cell(3,2).Value="15.05.2025"; ws.Cell(3,3).Value=machName;
                ws.Cell(3,4).Value=prodCode;        ws.Cell(3,5).Value=6600;         ws.Cell(3,6).Value="24H0538";
                ws.Cell(3,7).Value=hmCode;          ws.Cell(3,8).Value=980;          ws.Cell(3,9).Value=8;
                StyleExampleRow(ws, 3, defs.Length);

                ws.Column(2).Style.NumberFormat.Format = "@";
                ws.Range("H4:H10000").Style.NumberFormat.Format = "#,##0.00";
                ws.Range("I4:I10000").Style.NumberFormat.Format = "#,##0.00";

                // Makine dropdown
                if (machines.Any())
                    ws.Range("C4:C10000").SetDataValidation().List(wsRef.Range(2,5, mchCount+1,5), true);
            }
            // ─── SatisImport ──────────────────────────────────────────────
            else if (type == "SatisImport")
            {
                ws = wb.AddWorksheet("SatisKaydi");
                var defs = new (string, bool, string)[] {
                    ("SatisNo",     true,  "Satış irsaliye/fatura numarası. Tekrarlanabilir (aynı satış için birden fazla ürün).\nÖr: SAT-2025-001"),
                    ("Tarih",       true,  "Satış tarihi. Format: gg.AA.yyyy\nÖr: 15.05.2025"),
                    ("MusteriKodu", true,  "Sistemdeki müşteri kodu (MHS-XXX formatı).\nGeçerli kodlar için Referans sayfasına bakın.\nKod yerine tam isim de kabul edilir."),
                    ("UrunKodu",    true,  "Satılan mamul ürün kodu.\nÖr: 30001\nGeçerli kodlar için Referans sayfasına bakın."),
                    ("Miktar",      true,  "Satış miktarı (adet).\nÖr: 5000"),
                    ("BirimFiyat",  false, "Birim fiyat (opsiyonel). Ondalık: nokta veya virgül.\nÖr: 2.50"),
                    ("IrsaliyeNo",  false, "Satış irsaliye numarası. Ör: IRS202500001"),
                    ("FaturaNo",    false, "Satış fatura numarası. Ör: FAT202500001"),
                    ("Notlar",      false, "Ek notlar (opsiyonel)")
                };
                WriteHeaders(ws, defs);

                var cusCode  = customers.FirstOrDefault()?.CustomerCode ?? "MHS-001";
                var prodCode = products.FirstOrDefault()?.ProductCode ?? "30001";

                ws.Cell(2,1).Value="SAT-2025-001"; ws.Cell(2,2).Value="15.05.2025"; ws.Cell(2,3).Value=cusCode;
                ws.Cell(2,4).Value=prodCode;        ws.Cell(2,5).Value=5000;         ws.Cell(2,6).Value=2.50;
                ws.Cell(2,7).Value="IRS202500001";  ws.Cell(2,8).Value="FAT202500001";
                StyleExampleRow(ws, 2, defs.Length);

                ws.Cell(3,1).Value="SAT-2025-001"; ws.Cell(3,2).Value="15.05.2025"; ws.Cell(3,3).Value=cusCode;
                ws.Cell(3,4).Value=prodCode;        ws.Cell(3,5).Value=2000;         ws.Cell(3,6).Value=2.50;
                ws.Cell(3,7).Value="IRS202500001";  ws.Cell(3,8).Value="FAT202500001";
                StyleExampleRow(ws, 3, defs.Length);

                ws.Column(2).Style.NumberFormat.Format = "@";
                ws.Range("F4:F10000").Style.NumberFormat.Format = "#,##0.00";

                // Müşteri dropdown
                if (customers.Any())
                    ws.Range("C4:C10000").SetDataValidation().List(wsRef.Range(2,3, cusCount+1,3), true);
            }
            // ─── ProductImport ────────────────────────────────────────────
            else if (type == "ProductImport")
            {
                ws = wb.AddWorksheet("Urunler");
                var defs = new (string, bool, string)[] {
                    ("UrunKodu",  false, "Ürün kodu. Boş bırakılırsa otomatik üretilir.\nMevcut kod girilirse güncelleme yapılır."),
                    ("UrunAdi",   true,  "Ürün adı (zorunlu)."),
                    ("Birim",     true,  "Birim: Kg, Adet, Metre, Rulo vb."),
                    ("GrupAdi",   false, "Ürün grubu adı (sistemde tanımlı olmalı).\nGeçerli gruplar için Referans sayfasına bakın."),
                    ("IsActive",  false, "Aktif/Pasif: 1 = Aktif, 0 = Pasif. Boş = Aktif.")
                };
                WriteHeaders(ws, defs);

                ws.Cell(2,1).Value="HM-999"; ws.Cell(2,2).Value="Örnek Hammadde Kağıt"; ws.Cell(2,3).Value="Kg";
                ws.Cell(2,4).Value= groups.FirstOrDefault()?.GroupName ?? "Hammadde"; ws.Cell(2,5).Value="1";
                StyleExampleRow(ws, 2, defs.Length);

                if (groups.Any())
                    ws.Range("D3:D10000").SetDataValidation().List(wsRef.Range(2,6, grpCount+1,6), true);
            }
            // ─── SupplierImport ───────────────────────────────────────────
            else if (type == "SupplierImport")
            {
                ws = wb.AddWorksheet("Tedarikciler");
                var defs = new (string, bool, string)[] {
                    ("TedarikciKodu",  false, "Tedarikçi kodu. Boş bırakılırsa otomatik TED-NNN üretilir.\nMevcut kod girilirse güncelleme yapılır."),
                    ("TedarikciAdi",   true,  "Tedarikçi ticari ünvanı (zorunlu)."),
                    ("FscKodu",        false, "FSC sertifika kodu. Ör: FSC-C000001"),
                    ("ContactPerson",  false, "İletişim kurulacak kişi adı."),
                    ("Telefon",        false, "Telefon. Yalnızca rakamlar kullanılır.\nÖr: 02121234567 veya 05551234567"),
                    ("Email",          false, "E-posta adresi. Ör: info@firma.com.tr")
                };
                WriteHeaders(ws, defs);

                ws.Cell(2,1).Value="TED-999"; ws.Cell(2,2).Value="Örnek Tedarikçi Kağıt A.Ş.";
                ws.Cell(2,3).Value="FSC-C000001"; ws.Cell(2,4).Value="Ahmet Yılmaz";
                ws.Cell(2,5).Value="02121234567"; ws.Cell(2,6).Value="info@ornek.com";
                StyleExampleRow(ws, 2, defs.Length);
            }
            // ─── CustomerImport ───────────────────────────────────────────
            else
            {
                ws = wb.AddWorksheet("Musteriler");
                var defs = new (string, bool, string)[] {
                    ("MusteriKodu",   false, "Müşteri kodu. Boş bırakılırsa otomatik MHS-NNN üretilir.\nMevcut kod girilirse güncelleme yapılır."),
                    ("MusteriAdi",    true,  "Müşteri ticari ünvanı (zorunlu)."),
                    ("VergiNo",       false, "Vergi kimlik numarası (10 hane)."),
                    ("VergiDairesi",  false, "Bağlı olduğu vergi dairesi."),
                    ("Sehir",         false, "Şehir. Ör: İstanbul, Ankara"),
                    ("Telefon",       false, "Telefon. Ör: 02161234567"),
                    ("Email",         false, "E-posta. Ör: info@musteri.com.tr")
                };
                WriteHeaders(ws, defs);

                ws.Cell(2,1).Value="MHS-999"; ws.Cell(2,2).Value="Örnek Müşteri Ambalaj Ltd.";
                ws.Cell(2,3).Value="1234567890"; ws.Cell(2,4).Value="Kadıköy";
                ws.Cell(2,5).Value="İstanbul"; ws.Cell(2,6).Value="02161234567";
                ws.Cell(2,7).Value="info@musteri.com";
                StyleExampleRow(ws, 2, defs.Length);
            }

            // Açıklama sayfası (kullanıcıya görünür)
            var wsInfo = wb.AddWorksheet("OKUYUN");
            wsInfo.Cell("A1").Value = $"{type} — Dolum Kılavuzu";
            wsInfo.Cell("A1").Style.Font.Bold = true;
            wsInfo.Cell("A1").Style.Font.FontSize = 14;
            wsInfo.Cell("A1").Style.Font.FontColor = headerBg;
            wsInfo.Cell("A2").Value = $"Oluşturulma: {DateTime.Now:dd.MM.yyyy HH:mm}  |  Bu dosyayı silmeyin, doldurulmuş veri sayfasıyla birlikte yükleyin.";
            wsInfo.Cell("A2").Style.Font.FontColor = noteColor;

            var rules = new[] {
                ("GENEL KURALLAR", ""),
                ("1. Başlık satırı (1. satır)", "Değiştirmeyin. Sistem kolonları bu satırdan okur."),
                ("2. Zorunlu alanlar (*)", "Başlığında * işareti olan kolonlar boş bırakılamaz."),
                ("3. Tarih formatı", "gg.AA.yyyy — Ör: 15.05.2025  (yıl 4 haneli, ay ve gün 2 haneli)"),
                ("4. Ondalık sayılar", "Nokta (1757.50) veya virgül (1757,50) her ikisi de kabul edilir."),
                ("5. Kodlar", "Sistemde kayıtlı kodlarla eşleşmelidir. Yanlış kod = hata satırı."),
                ("6. Tekrarlı satırlar", "Lot/Üretim/Satış No aynı ise aynı fişe ait birden fazla kalem olarak işlenir."),
                ("7. Boş satır", "Ortada boş satır bırakmayın. Sistem ilk boş satırda okumayı durdurabilir."),
                ("", ""),
                ("SÜTUN REHBERİ", "Başlık hücresinin üzerine gelin — sarı not baloncuğunda açıklama görünür."),
                ("", ""),
                ("GEÇERLİ FSC TİPLERİ", string.Join(", ", fscTypes.Select(f => f.Name))),
                ("GEÇERLİ TEDARİKÇİ KODLARI", string.Join(", ", suppliers.Select(s => s.SupplierCode))),
                ("GEÇERLİ MÜŞTERİ KODLARI", string.Join(", ", customers.Select(c => c.CustomerCode))),
                ("GEÇERLİ MAKİNELER", string.Join(", ", machines.Select(m => m.Name))),
            };

            int row = 4;
            foreach (var (label, val) in rules)
            {
                if (string.IsNullOrEmpty(label)) { row++; continue; }
                wsInfo.Cell(row, 1).Value = label;
                wsInfo.Cell(row, 1).Style.Font.Bold = true;
                wsInfo.Cell(row, 2).Value = val;
                if (label.EndsWith("KURALLAR") || label.EndsWith("REHBERİ") || label.Contains("GEÇERLİ"))
                    wsInfo.Cell(row, 1).Style.Font.FontColor = headerBg;
                row++;
            }
            wsInfo.Column(1).Width = 35;
            wsInfo.Column(2).Width = 80;
            wsInfo.Column(2).Style.Alignment.WrapText = true;

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            // Örnek satırların ayrımı için kenarlık
            if (ws.LastRowUsed() != null)
            {
                var lastDataRow = ws.LastRowUsed()!.RowNumber();
                ws.Range(2, 1, lastDataRow, ws.LastColumnUsed()!.ColumnNumber())
                  .Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Range(2, 1, lastDataRow, ws.LastColumnUsed()!.ColumnNumber())
                  .Style.Border.BottomBorderColor = XLColor.FromHtml("#e5e7eb");
            }

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Sablon_{type}_{DateTime.Now:ddMMyyyy}.xlsx");
        }

        // ─── Netsis ETL Excel İndir ───────────────────────────────────────────
        public IActionResult DownloadNetsisEtl(string file)
        {
            var allowed = new[] { "ETL_Tedarikciler", "ETL_Musteriler", "ETL_HammaddeGirisleri", "ETL_SatisGirisleri", "ETL_FaturaListesi" };
            if (!allowed.Contains(file))
                return NotFound();

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "etl", $"{file}.xlsx");
            if (!System.IO.File.Exists(path))
                return NotFound("ETL dosyası bulunamadı. Lütfen yeniden oluşturun.");

            var bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{file}_{DateTime.Now:ddMMyyyy}.xlsx");
        }

        // ─── Netsis Senkronizasyon ────────────────────────────────────────────
        public async Task<IActionResult> NetsisSync()
        {
            ViewData["Title"] = "Netsis Senkronizasyonu";
            ViewBag.Connections = await _context.EtlConnections
                .Where(c => c.IsActive && c.Type == "Netsis")
                .OrderBy(c => c.Name)
                .ToListAsync();
            ViewBag.RecentJobs = await _context.EtlJobs
                .Where(j => j.Source == "Netsis")
                .Include(j => j.EtlConnection)
                .OrderByDescending(j => j.StartedAt)
                .Take(10)
                .ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> NetsisExecute(string syncType, int? connectionId)
        {
            var connStr = _cfg.GetConnectionString("NetsisConnection");

            if (connectionId.HasValue)
            {
                var etlConn = await _context.EtlConnections.FindAsync(connectionId.Value);
                if (etlConn?.Settings != null)
                {
                    try
                    {
                        var s = JsonSerializer.Deserialize<Dictionary<string, string>>(etlConn.Settings);
                        if (s != null && s.ContainsKey("Server") && s.ContainsKey("Database"))
                        {
                            var b = new SqlConnectionStringBuilder
                            {
                                DataSource         = s.GetValueOrDefault("Server", ""),
                                InitialCatalog     = s.GetValueOrDefault("Database", ""),
                                UserID             = s.GetValueOrDefault("UserId", ""),
                                Password           = s.GetValueOrDefault("Password", ""),
                                TrustServerCertificate = true
                            };
                            connStr = b.ConnectionString;
                        }
                    }
                    catch { }
                }
            }

            if (string.IsNullOrWhiteSpace(connStr))
                return Json(new { success = false, message = "Netsis bağlantı dizgisi tanımlı değil." });

            var job = new EtlJob
            {
                EtlConnectionId = connectionId,
                JobType         = syncType,
                Source          = "Netsis",
                Status          = "Running",
                StartedAt       = DateTime.Now,
                CreatedDate     = DateTime.Now,
                CreatedBy       = "SYSTEM"
            };
            _context.EtlJobs.Add(job);
            await _context.SaveChangesAsync();

            int inserted = 0, updated = 0, skipped = 0;
            var errors = new List<string>();

            try
            {
                using var netsisCon = new SqlConnection(connStr);
                await netsisCon.OpenAsync();

                (inserted, updated, skipped, errors) = syncType switch
                {
                    "ProductImport"  => await SyncNetsisProducts(netsisCon),
                    "SupplierImport" => await SyncNetsisSuppliers(netsisCon),
                    "CustomerImport" => await SyncNetsisCustomers(netsisCon),
                    _ => (0, 0, 0, new List<string> { "Bilinmeyen senkronizasyon türü." })
                };

                job.Status        = errors.Count == 0 ? "Completed" : (inserted + updated > 0 ? "Partial" : "Failed");
                job.InsertedCount = inserted;
                job.UpdatedCount  = updated;
                job.SkippedCount  = skipped;
                job.ErrorCount    = errors.Count;
                job.CompletedAt   = DateTime.Now;
                job.ErrorDetails  = errors.Count > 0 ? string.Join("\n", errors.Take(50)) : null;

                if (connectionId.HasValue)
                {
                    var conn = await _context.EtlConnections.FindAsync(connectionId.Value);
                    if (conn != null) { conn.LastSyncAt = DateTime.Now; conn.LastSyncStatus = job.Status; }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, inserted, updated, skipped, errorCount = errors.Count, errors = errors.Take(20), jobId = job.Id });
            }
            catch (Exception ex)
            {
                job.Status       = "Failed";
                job.ErrorDetails = ex.Message;
                job.CompletedAt  = DateTime.Now;
                await _context.SaveChangesAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ─── Netsis veri çekme yardımcıları ──────────────────────────────────
        private async Task<(int ins, int upd, int skp, List<string> errs)> SyncNetsisProducts(SqlConnection cn)
        {
            int ins = 0, upd = 0, skp = 0;
            var errors = new List<string>();
            var groups = await _context.ProductGroups.ToListAsync();

            const string sql = @"
                SELECT STOK_KODU, STOK_ADI, OLCU_BR1, GRUP_KODU, MAMULMU
                FROM TBLSTSABIT
                WHERE GRUP_KODU IN (10, 20, 30, 40, 50)
                ORDER BY STOK_KODU";

            using var cmd = new SqlCommand(sql, cn);
            using var rdr = await cmd.ExecuteReaderAsync();

            var rows = new List<(string Kod, string Adi, string Birim, int Grup, bool Mamul)>();
            while (await rdr.ReadAsync())
            {
                rows.Add((
                    rdr["STOK_KODU"]?.ToString()?.Trim() ?? "",
                    rdr["STOK_ADI"]?.ToString()?.Trim() ?? "",
                    rdr["OLCU_BR1"]?.ToString()?.Trim() ?? "AD",
                    Convert.ToInt32(rdr["GRUP_KODU"]),
                    rdr["MAMULMU"]?.ToString() == "1"
                ));
            }
            rdr.Close();

            foreach (var r in rows)
            {
                if (string.IsNullOrWhiteSpace(r.Kod) || string.IsNullOrWhiteSpace(r.Adi)) { skp++; continue; }
                try
                {
                    var grupAdi = r.Grup switch { 10 => "Hammadde", 20 => "Sap", 30 => "Mamul", 40 => "Kimyasal", _ => "Sarf" };
                    var group   = groups.FirstOrDefault(g => g.GroupName.Equals(grupAdi, StringComparison.OrdinalIgnoreCase));

                    var existing = await _context.Products.FirstOrDefaultAsync(p => p.ProductCode == r.Kod);
                    if (existing == null)
                    {
                        _context.Products.Add(new Product
                        {
                            ProductCode    = r.Kod,
                            ProductName    = r.Adi,
                            Unit           = r.Birim.ToUpperInvariant(),
                            ProductGroupId = group?.Id,
                            IsActive       = true,
                            CreatedDate    = DateTime.Now,
                            CreatedBy      = "NETSIS"
                        });
                        ins++;
                    }
                    else
                    {
                        existing.ProductName    = r.Adi;
                        existing.Unit           = r.Birim.ToUpperInvariant();
                        existing.ProductGroupId = group?.Id ?? existing.ProductGroupId;
                        existing.UpdatedDate    = DateTime.Now;
                        existing.UpdatedBy      = "NETSIS";
                        upd++;
                    }
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex) { errors.Add($"{r.Kod}: {ex.Message}"); }
            }
            return (ins, upd, skp, errors);
        }

        private async Task<(int ins, int upd, int skp, List<string> errs)> SyncNetsisSuppliers(SqlConnection cn)
        {
            int ins = 0, upd = 0, skp = 0;
            var errors = new List<string>();
            var count = await _context.Suppliers.CountAsync();

            const string sql = @"
                SELECT CARI_KOD, CARI_ISIM, CARI_TEL, EMAIL, VERGI_NUMARASI, VERGI_DAIRESI, CARI_IL
                FROM TBLCASABIT
                WHERE CARI_TIP = 'S'
                ORDER BY CARI_KOD";

            using var cmd = new SqlCommand(sql, cn);
            using var rdr = await cmd.ExecuteReaderAsync();

            var rows = new List<(string Kod, string Isim, string Tel, string Mail, string Vn, string Vd, string Il)>();
            while (await rdr.ReadAsync())
            {
                rows.Add((
                    rdr["CARI_KOD"]?.ToString()?.Trim() ?? "",
                    rdr["CARI_ISIM"]?.ToString()?.Trim() ?? "",
                    rdr["CARI_TEL"]?.ToString()?.Trim() ?? "",
                    rdr["EMAIL"]?.ToString()?.Trim() ?? "",
                    rdr["VERGI_NUMARASI"]?.ToString()?.Trim() ?? "",
                    rdr["VERGI_DAIRESI"]?.ToString()?.Trim() ?? "",
                    rdr["CARI_IL"]?.ToString()?.Trim() ?? ""
                ));
            }
            rdr.Close();

            foreach (var r in rows)
            {
                if (string.IsNullOrWhiteSpace(r.Isim)) { skp++; continue; }
                try
                {
                    var existing = await _context.Suppliers
                        .FirstOrDefaultAsync(s => s.SupplierCode == r.Kod || s.Name == r.Isim);

                    var email = NormalizeEmail(r.Mail);

                    if (existing == null)
                    {
                        count++;
                        _context.Suppliers.Add(new Supplier
                        {
                            SupplierCode  = string.IsNullOrWhiteSpace(r.Kod) ? $"TED-{count:D3}" : r.Kod,
                            Name          = r.Isim,
                            Phone         = r.Tel,
                            Email         = email,
                            IsActive      = true,
                            IsFscActive   = false,
                            CreatedDate   = DateTime.Now,
                            CreatedBy     = "NETSIS"
                        });
                        ins++;
                    }
                    else
                    {
                        existing.Name        = r.Isim;
                        existing.Phone       = r.Tel;
                        existing.Email       = email;
                        existing.UpdatedDate = DateTime.Now;
                        existing.UpdatedBy   = "NETSIS";
                        upd++;
                    }
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex) { errors.Add($"{r.Kod} - {r.Isim}: {ex.Message}"); }
            }
            return (ins, upd, skp, errors);
        }

        private async Task<(int ins, int upd, int skp, List<string> errs)> SyncNetsisCustomers(SqlConnection cn)
        {
            int ins = 0, upd = 0, skp = 0;
            var errors = new List<string>();
            var count = await _context.Customers.CountAsync();

            const string sql = @"
                SELECT CARI_KOD, CARI_ISIM, CARI_TEL, EMAIL, VERGI_NUMARASI, VERGI_DAIRESI, CARI_IL
                FROM TBLCASABIT
                WHERE CARI_TIP = 'A'
                ORDER BY CARI_KOD";

            using var cmd = new SqlCommand(sql, cn);
            using var rdr = await cmd.ExecuteReaderAsync();

            var rows = new List<(string Kod, string Isim, string Tel, string Mail, string Vn, string Vd, string Il)>();
            while (await rdr.ReadAsync())
            {
                rows.Add((
                    rdr["CARI_KOD"]?.ToString()?.Trim() ?? "",
                    rdr["CARI_ISIM"]?.ToString()?.Trim() ?? "",
                    rdr["CARI_TEL"]?.ToString()?.Trim() ?? "",
                    rdr["EMAIL"]?.ToString()?.Trim() ?? "",
                    rdr["VERGI_NUMARASI"]?.ToString()?.Trim() ?? "",
                    rdr["VERGI_DAIRESI"]?.ToString()?.Trim() ?? "",
                    rdr["CARI_IL"]?.ToString()?.Trim() ?? ""
                ));
            }
            rdr.Close();

            foreach (var r in rows)
            {
                if (string.IsNullOrWhiteSpace(r.Isim)) { skp++; continue; }
                try
                {
                    var existing = await _context.Customers
                        .FirstOrDefaultAsync(c => c.CustomerCode == r.Kod || c.Name == r.Isim);

                    var email = NormalizeEmail(r.Mail);

                    if (existing == null)
                    {
                        count++;
                        _context.Customers.Add(new Customer
                        {
                            CustomerCode = string.IsNullOrWhiteSpace(r.Kod) ? $"MHS-{count:D3}" : r.Kod,
                            Name         = r.Isim,
                            Phone        = r.Tel,
                            Email        = email,
                            TaxNumber    = r.Vn,
                            TaxOffice    = r.Vd,
                            City         = r.Il,
                            IsActive     = true,
                            CreatedDate  = DateTime.Now,
                            CreatedBy    = "NETSIS"
                        });
                        ins++;
                    }
                    else
                    {
                        existing.Name        = r.Isim;
                        existing.Phone       = r.Tel;
                        existing.Email       = email;
                        existing.TaxNumber   = r.Vn;
                        existing.TaxOffice   = r.Vd;
                        existing.City        = r.Il;
                        existing.UpdatedDate = DateTime.Now;
                        existing.UpdatedBy   = "NETSIS";
                        upd++;
                    }
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex) { errors.Add($"{r.Kod} - {r.Isim}: {ex.Message}"); }
            }
            return (ins, upd, skp, errors);
        }

        // ─── ETL Auto-Detect Import ──────────────────────────────────────────────
        // ETL_Tedarikciler / ETL_Musteriler / ETL_HammaddeGirisleri formatlarını
        // başlık satırı adlarına bakarak otomatik algılar.
        private async Task<(int ins, int upd, int skp, List<string> errs)> ImportFromEtlFile(
            List<IXLRangeRow> rows, Dictionary<string, int> hdrs)
        {
            string ColVal(IXLRangeRow r, string name)
                => hdrs.TryGetValue(name, out var c) ? r.Cell(c).GetString().Trim() : string.Empty;

            decimal ColDec(IXLRangeRow r, string name)
            {
                var s = ColVal(r, name).Replace(',', '.');
                return decimal.TryParse(s, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0;
            }

            DateTime? ColDate(IXLRangeRow r, string name)
            {
                var s = ColVal(r, name);
                if (string.IsNullOrWhiteSpace(s)) return null;
                if (DateTime.TryParseExact(s, new[] { "dd.MM.yyyy", "yyyy-MM-dd", "d.M.yyyy" },
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var d)) return d;
                if (DateTime.TryParse(s, out var d2)) return d2;
                return null;
            }

            // Algılama: başlık adlarına bak
            bool isSupplier  = hdrs.ContainsKey("TedarikciAdi");
            bool isCustomer  = hdrs.ContainsKey("MusteriAdi");
            bool isHammadde  = hdrs.ContainsKey("LotNo") && hdrs.ContainsKey("SeriNo");

            int ins = 0, upd = 0, skp = 0;
            var errors = new List<string>();

            if (!isSupplier && !isCustomer && !isHammadde)
            {
                errors.Add("ETL dosyası formatı tanınamadı. Başlık satırını kontrol edin (TedarikciAdi / MusteriAdi / LotNo+SeriNo bekleniyor).");
                return (ins, upd, skp, errors);
            }

            // ── Tedarikçi ETL ──────────────────────────────────────────────────
            if (isSupplier)
            {
                var count = await _context.Suppliers.CountAsync();
                for (int i = 0; i < rows.Count; i++)
                {
                    var row  = rows[i];
                    var name = ColVal(row, "TedarikciAdi");
                    if (string.IsNullOrWhiteSpace(name)) { skp++; continue; }

                    var phone     = ColVal(row, "Telefon");
                    var email     = NormalizeEmail(ColVal(row, "Email"));
                    var address   = ColVal(row, "Adres");
                    var city      = ColVal(row, "Sehir");
                    var taxOffice = ColVal(row, "VergiDairesi");
                    var taxNo     = ColVal(row, "VergiNo");
                    var fscCode   = ColVal(row, "FscKodu");
                    var fscExpStr = ColVal(row, "FscBitisTarihi");
                    DateTime? fscExp = null;
                    if (!string.IsNullOrWhiteSpace(fscExpStr))
                        if (DateTime.TryParseExact(fscExpStr, new[] { "dd.MM.yyyy", "yyyy-MM-dd" },
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out var fe)) fscExp = fe;

                    try
                    {
                        var existing = await _context.Suppliers
                            .FirstOrDefaultAsync(s => s.Name == name ||
                                (!string.IsNullOrWhiteSpace(taxNo) && s.TaxNumber == taxNo));
                        if (existing == null)
                        {
                            count++;
                            _context.Suppliers.Add(new Supplier
                            {
                                SupplierCode  = $"TED-{count:D3}",
                                Name          = name,
                                Phone         = phone,
                                Email         = email,
                                Address       = address,
                                City          = city,
                                TaxOffice     = taxOffice,
                                TaxNumber     = taxNo,
                                FscCode       = fscCode,
                                FscExpiryDate = fscExp,
                                IsActive      = true,
                                IsFscActive   = !string.IsNullOrWhiteSpace(fscCode),
                                CreatedDate   = DateTime.Now,
                                CreatedBy     = "ETL"
                            });
                            ins++;
                        }
                        else
                        {
                            existing.Phone         = phone;
                            existing.Email         = string.IsNullOrWhiteSpace(email) ? existing.Email : email;
                            existing.Address       = address;
                            existing.City          = city;
                            existing.TaxOffice     = taxOffice;
                            existing.TaxNumber     = string.IsNullOrWhiteSpace(taxNo) ? existing.TaxNumber : taxNo;
                            if (!string.IsNullOrWhiteSpace(fscCode)) { existing.FscCode = fscCode; existing.IsFscActive = true; }
                            if (fscExp.HasValue) existing.FscExpiryDate = fscExp;
                            existing.UpdatedDate = DateTime.Now;
                            existing.UpdatedBy   = "ETL";
                            upd++;
                        }
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex) { errors.Add($"Satır {i + 2} ({name}): {ex.Message}"); }
                }
                return (ins, upd, skp, errors);
            }

            // ── Müşteri ETL ──────────────────────────────────────────────────
            if (isCustomer)
            {
                var count = await _context.Customers.CountAsync();
                for (int i = 0; i < rows.Count; i++)
                {
                    var row  = rows[i];
                    var name = ColVal(row, "MusteriAdi");
                    if (string.IsNullOrWhiteSpace(name)) { skp++; continue; }

                    var phone     = ColVal(row, "Telefon");
                    var email     = NormalizeEmail(ColVal(row, "Email"));
                    var address   = ColVal(row, "Adres");
                    var city      = ColVal(row, "Sehir");
                    var taxOffice = ColVal(row, "VergiDairesi");
                    var taxNo     = ColVal(row, "VergiNo");
                    var fscLic    = ColVal(row, "FscLisansKodu");
                    var fscExpStr = ColVal(row, "FscBitisTarihi");
                    DateTime? fscExp = null;
                    if (!string.IsNullOrWhiteSpace(fscExpStr))
                        if (DateTime.TryParseExact(fscExpStr, new[] { "dd.MM.yyyy", "yyyy-MM-dd" },
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out var fe)) fscExp = fe;

                    try
                    {
                        var existing = await _context.Customers
                            .FirstOrDefaultAsync(c => c.Name == name ||
                                (!string.IsNullOrWhiteSpace(taxNo) && c.TaxNumber == taxNo));
                        if (existing == null)
                        {
                            count++;
                            _context.Customers.Add(new Customer
                            {
                                CustomerCode   = $"MHS-{count:D3}",
                                Name           = name,
                                Phone          = phone,
                                Email          = email,
                                Address        = address,
                                City           = city,
                                TaxOffice      = taxOffice,
                                TaxNumber      = taxNo,
                                FscLicenseCode = fscLic,
                                FscExpiryDate  = fscExp,
                                IsActive       = true,
                                IsFscActive    = !string.IsNullOrWhiteSpace(fscLic),
                                CreatedDate    = DateTime.Now,
                                CreatedBy      = "ETL"
                            });
                            ins++;
                        }
                        else
                        {
                            existing.Phone     = phone;
                            existing.Email     = string.IsNullOrWhiteSpace(email) ? existing.Email : email;
                            existing.Address   = address;
                            existing.City      = city;
                            existing.TaxOffice = taxOffice;
                            existing.TaxNumber = string.IsNullOrWhiteSpace(taxNo) ? existing.TaxNumber : taxNo;
                            if (!string.IsNullOrWhiteSpace(fscLic)) { existing.FscLicenseCode = fscLic; existing.IsFscActive = true; }
                            if (fscExp.HasValue) existing.FscExpiryDate = fscExp;
                            existing.UpdatedDate = DateTime.Now;
                            existing.UpdatedBy   = "ETL";
                            upd++;
                        }
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex) { errors.Add($"Satır {i + 2} ({name}): {ex.Message}"); }
                }
                return (ins, upd, skp, errors);
            }

            // ── Hammadde ETL ──────────────────────────────────────────────────
            // Sütunlar: LotNo, SeriNo, StokKodu, StokAdi, FscTipi, Tedarikci,
            //           Miktar_kg, Tarih, FisNo, IrsaliyeNo, DepoKodu
            {
                var fscTypes    = await _context.FscTypes.ToListAsync();
                var suppliers   = await _context.Suppliers.ToListAsync();
                var products    = await _context.Products.ToListAsync();
                var warehouses  = await _context.Warehouses.ToListAsync();
                var defWh       = warehouses.FirstOrDefault();

                var groups = rows
                    .Select((r, idx) => new { Row = r, Idx = idx })
                    .GroupBy(x => ColVal(x.Row, "LotNo"))
                    .Where(g => !string.IsNullOrWhiteSpace(g.Key))
                    .ToList();

                foreach (var group in groups)
                {
                    var lotNo    = group.Key;
                    var firstRow = group.First().Row;
                    var firstIdx = group.First().Idx;
                    try
                    {
                        var stokKodu     = ColVal(firstRow, "StokKodu");
                        var fscTipiStr   = ColVal(firstRow, "FscTipi");
                        var tedarikciAdi = ColVal(firstRow, "Tedarikci");
                        var irsNo        = ColVal(firstRow, "IrsaliyeNo");
                        var fisNo        = ColVal(firstRow, "FisNo");
                        var tarih        = ColDate(firstRow, "Tarih");

                        var product  = products.FirstOrDefault(p => p.ProductCode == stokKodu);
                        var supplier = suppliers.FirstOrDefault(s =>
                            s.Name.Equals(tedarikciAdi, StringComparison.OrdinalIgnoreCase) ||
                            (tedarikciAdi.Length > 3 && s.Name.Contains(tedarikciAdi.Substring(0, Math.Min(6, tedarikciAdi.Length)), StringComparison.OrdinalIgnoreCase)));
                        var fscType = fscTypes.FirstOrDefault(f =>
                            f.Name.Equals(fscTipiStr, StringComparison.OrdinalIgnoreCase) ||
                            f.Code.Equals(fscTipiStr, StringComparison.OrdinalIgnoreCase) ||
                            fscTipiStr.Contains(f.Code, StringComparison.OrdinalIgnoreCase));

                        if (product  == null) { errors.Add($"Lot {lotNo}: Ürün bulunamadı '{stokKodu}' — ürün kartı oluşturulup tekrar deneyin."); skp += group.Count(); continue; }
                        if (supplier == null) { errors.Add($"Lot {lotNo}: Tedarikçi bulunamadı '{tedarikciAdi}' — tedarikçi kaydı oluşturulup tekrar deneyin."); skp += group.Count(); continue; }
                        if (fscType  == null) { errors.Add($"Lot {lotNo}: FSC Tipi bulunamadı '{fscTipiStr}'."); skp += group.Count(); continue; }

                        var lot = await _context.FscLots.FirstOrDefaultAsync(l => l.LotNo == lotNo);
                        bool lotNew = lot == null;
                        if (lotNew)
                        {
                            lot = new FscLot
                            {
                                LotNo       = lotNo,
                                FscTypeId   = fscType.Id,
                                SupplierId  = supplier.Id,
                                ProductId   = product.Id,
                                DispatchNo  = irsNo,
                                InvoiceNo   = fisNo,
                                ArrivalDate = tarih ?? DateTime.Today,
                                CreatedDate = DateTime.Now,
                                CreatedBy   = "ETL"
                            };
                            _context.FscLots.Add(lot);
                            await _context.SaveChangesAsync();
                            ins++;
                        }
                        else { upd++; }

                        foreach (var item in group)
                        {
                            var row    = item.Row;
                            var rowNum = item.Idx + 2;
                            try
                            {
                                var seriNo = ColVal(row, "SeriNo").IfEmpty(lotNo);
                                var miktar = ColDec(row, "Miktar_kg");
                                if (miktar <= 0) { skp++; continue; }

                                var existingSerial = await _context.FscSerials
                                    .FirstOrDefaultAsync(s => s.LotId == lot!.Id && s.SerialNo == seriNo);
                                if (existingSerial == null)
                                {
                                    _context.FscSerials.Add(new FscSerial
                                    {
                                        LotId         = lot!.Id,
                                        SerialNo      = seriNo,
                                        InitialWeight = miktar,
                                        CurrentWeight = miktar,
                                        CreatedDate   = DateTime.Now,
                                        CreatedBy     = "ETL"
                                    });
                                    _context.StockMovements.Add(new StockMovement
                                    {
                                        Type          = MovementType.PurchaseEntry,
                                        DocumentNo    = fisNo.IfEmpty(lotNo),
                                        DocumentDate  = tarih ?? DateTime.Today,
                                        ProductId     = product.Id,
                                        Quantity      = miktar,
                                        Unit          = product.Unit ?? "KG",
                                        ToWarehouseId = defWh?.Id,
                                        Description   = $"ETL Lot: {lotNo} | Seri: {seriNo}",
                                        CreatedDate   = DateTime.Now,
                                        CreatedBy     = "ETL"
                                    });
                                    await _context.SaveChangesAsync();
                                }
                                // else: aynı seri zaten var, atla
                            }
                            catch (Exception ex) { errors.Add($"Satır {rowNum}: {ex.Message}"); }
                        }
                    }
                    catch (Exception ex) { errors.Add($"Lot {lotNo}: {ex.Message}"); }
                }
                return (ins, upd, skp, errors);
            }
        }

        private static string NormalizeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return string.Empty;
            return email.Replace("İ", "i").Replace("I", "ı").ToLowerInvariant();
        }

        // ─── Import yardımcıları ──────────────────────────────────────────────
        private async Task<(int ins, int upd, int skp, List<string> errs)> ImportProducts(List<IXLRangeRow> rows)
        {
            int ins = 0, upd = 0, skp = 0;
            var errors = new List<string>();
            var groups = await _context.ProductGroups.ToListAsync();

            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                try
                {
                    var code = row.Cell(1).GetString().Trim();
                    var name = row.Cell(2).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name)) { skp++; continue; }

                    var unit      = row.Cell(3).GetString().Trim().IfEmpty("Adet");
                    var groupName = row.Cell(4).GetString().Trim();
                    var isActive  = row.Cell(5).GetString().Trim() != "0";
                    var group     = groups.FirstOrDefault(g => g.GroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase));

                    var existing = await _context.Products.FirstOrDefaultAsync(p => p.ProductCode == code);
                    if (existing == null)
                    {
                        _context.Products.Add(new Product
                        {
                            ProductCode    = code,
                            ProductName    = name,
                            Unit           = unit,
                            ProductGroupId = group?.Id,
                            IsActive       = isActive,
                            CreatedDate    = DateTime.Now,
                            CreatedBy      = "ETL"
                        });
                        ins++;
                    }
                    else
                    {
                        existing.ProductName    = name;
                        existing.Unit           = unit;
                        existing.ProductGroupId = group?.Id ?? existing.ProductGroupId;
                        existing.IsActive       = isActive;
                        existing.UpdatedDate    = DateTime.Now;
                        existing.UpdatedBy      = "ETL";
                        upd++;
                    }
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex) { errors.Add($"Satır {i + 2}: {ex.Message}"); }
            }
            return (ins, upd, skp, errors);
        }

        private async Task<(int ins, int upd, int skp, List<string> errs)> ImportSuppliers(List<IXLRangeRow> rows)
        {
            int ins = 0, upd = 0, skp = 0;
            var errors = new List<string>();
            var count  = await _context.Suppliers.CountAsync();

            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                try
                {
                    var code = row.Cell(1).GetString().Trim();
                    var name = row.Cell(2).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(name)) { skp++; continue; }

                    var fscCode = row.Cell(3).GetString().Trim();
                    var contact = row.Cell(4).GetString().Trim();
                    var phone   = row.Cell(5).GetString().Trim();
                    var email   = row.Cell(6).GetString().Trim();

                    var existing = string.IsNullOrWhiteSpace(code)
                        ? null
                        : await _context.Suppliers.FirstOrDefaultAsync(s => s.SupplierCode == code);

                    if (existing == null)
                    {
                        count++;
                        _context.Suppliers.Add(new Supplier
                        {
                            SupplierCode   = string.IsNullOrWhiteSpace(code) ? $"TED-{count:D3}" : code,
                            Name           = name,
                            FscCode        = fscCode,
                            ContactPerson  = contact,
                            Phone          = phone,
                            Email          = email.ToLowerInvariant(),
                            IsActive       = true,
                            IsFscActive    = !string.IsNullOrWhiteSpace(fscCode),
                            CreatedDate    = DateTime.Now,
                            CreatedBy      = "ETL"
                        });
                        ins++;
                    }
                    else
                    {
                        existing.Name          = name;
                        existing.FscCode       = fscCode;
                        existing.ContactPerson = contact;
                        existing.Phone         = phone;
                        existing.Email         = email.ToLowerInvariant();
                        existing.UpdatedDate   = DateTime.Now;
                        existing.UpdatedBy     = "ETL";
                        upd++;
                    }
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex) { errors.Add($"Satır {i + 2}: {ex.Message}"); }
            }
            return (ins, upd, skp, errors);
        }

        private async Task<(int ins, int upd, int skp, List<string> errs)> ImportCustomers(List<IXLRangeRow> rows)
        {
            int ins = 0, upd = 0, skp = 0;
            var errors = new List<string>();
            var count  = await _context.Customers.CountAsync();

            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                try
                {
                    var code = row.Cell(1).GetString().Trim();
                    var name = row.Cell(2).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(name)) { skp++; continue; }

                    var taxNo     = row.Cell(3).GetString().Trim();
                    var taxOffice = row.Cell(4).GetString().Trim();
                    var city      = row.Cell(5).GetString().Trim();
                    var phone     = row.Cell(6).GetString().Trim();
                    var email     = row.Cell(7).GetString().Trim();

                    var existing = string.IsNullOrWhiteSpace(code)
                        ? null
                        : await _context.Customers.FirstOrDefaultAsync(c => c.CustomerCode == code);

                    if (existing == null)
                    {
                        count++;
                        _context.Customers.Add(new Customer
                        {
                            CustomerCode = string.IsNullOrWhiteSpace(code) ? $"MHS-{count:D3}" : code,
                            Name         = name,
                            TaxNumber    = taxNo,
                            TaxOffice    = taxOffice,
                            City         = city,
                            Phone        = phone,
                            Email        = email.ToLowerInvariant(),
                            IsActive     = true,
                            CreatedDate  = DateTime.Now,
                            CreatedBy    = "ETL"
                        });
                        ins++;
                    }
                    else
                    {
                        existing.Name      = name;
                        existing.TaxNumber = taxNo;
                        existing.TaxOffice = taxOffice;
                        existing.City      = city;
                        existing.Phone     = phone;
                        existing.Email     = email.ToLowerInvariant();
                        existing.UpdatedDate = DateTime.Now;
                        existing.UpdatedBy   = "ETL";
                        upd++;
                    }
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex) { errors.Add($"Satır {i + 2}: {ex.Message}"); }
            }
            return (ins, upd, skp, errors);
        }

        // ─── Lot / Hammadde Giriş Import ─────────────────────────────────────
        private async Task<(int ins, int upd, int skp, List<string> errs)> ImportLots(List<IXLRangeRow> rows)
        {
            int ins = 0, upd = 0, skp = 0;
            var errors  = new List<string>();
            var fscTypes = await _context.FscTypes.ToListAsync();
            var suppliers = await _context.Suppliers.ToListAsync();
            var products  = await _context.Products.ToListAsync();
            var warehouses = await _context.Warehouses.ToListAsync();
            var defaultWarehouse = warehouses.FirstOrDefault();

            // Satırları LotNo'ya göre grupla
            var groups = rows
                .Select((r, idx) => new { Row = r, Idx = idx })
                .GroupBy(x => x.Row.Cell(1).GetString().Trim())
                .Where(g => !string.IsNullOrWhiteSpace(g.Key))
                .ToList();

            foreach (var group in groups)
            {
                var lotNo    = group.Key;
                var firstRow = group.First().Row;
                var firstIdx = group.First().Idx;
                try
                {
                    var supplierCode = firstRow.Cell(2).GetString().Trim();
                    var productCode  = firstRow.Cell(3).GetString().Trim();
                    var fscTypeName  = firstRow.Cell(4).GetString().Trim();
                    var irsNo        = firstRow.Cell(7).GetString().Trim();
                    var fatNo        = firstRow.Cell(8).GetString().Trim();
                    var tarih        = ParseDate(firstRow.Cell(9));
                    var plaka        = firstRow.Cell(10).GetString().Trim();

                    var supplier = suppliers.FirstOrDefault(s => s.SupplierCode == supplierCode || s.Name.Equals(supplierCode, StringComparison.OrdinalIgnoreCase));
                    var product  = products.FirstOrDefault(p => p.ProductCode == productCode);
                    var fscType  = fscTypes.FirstOrDefault(f =>
                        f.Name.Contains(fscTypeName, StringComparison.OrdinalIgnoreCase) ||
                        fscTypeName.Contains(f.Code, StringComparison.OrdinalIgnoreCase) ||
                        f.Code.Equals(fscTypeName, StringComparison.OrdinalIgnoreCase));

                    if (supplier == null) { errors.Add($"Satır {firstIdx+2} (Lot {lotNo}): Tedarikçi bulunamadı — '{supplierCode}'"); skp += group.Count(); continue; }
                    if (product  == null) { errors.Add($"Satır {firstIdx+2} (Lot {lotNo}): Ürün bulunamadı — '{productCode}'");      skp += group.Count(); continue; }
                    if (fscType  == null) { errors.Add($"Satır {firstIdx+2} (Lot {lotNo}): FSC Tipi bulunamadı — '{fscTypeName}'");   skp += group.Count(); continue; }

                    // Lot bul veya oluştur
                    var lot = await _context.FscLots.FirstOrDefaultAsync(l => l.LotNo == lotNo);
                    bool lotNew = lot == null;
                    if (lotNew)
                    {
                        lot = new FscLot
                        {
                            LotNo        = lotNo,
                            FscTypeId    = fscType.Id,
                            SupplierId   = supplier.Id,
                            ProductId    = product.Id,
                            DispatchNo   = irsNo,
                            InvoiceNo    = fatNo,
                            ArrivalDate  = tarih ?? DateTime.Today,
                            TruckPlate   = plaka,
                            CreatedDate  = DateTime.Now,
                            CreatedBy    = "ETL"
                        };
                        _context.FscLots.Add(lot);
                        await _context.SaveChangesAsync();
                        ins++;
                    }
                    else { upd++; }

                    // Her satır için seri oluştur
                    foreach (var item in group)
                    {
                        var row    = item.Row;
                        var rowNum = item.Idx + 2;
                        try
                        {
                            var seriNo = row.Cell(5).GetString().Trim().IfEmpty(lotNo);
                            var miktar = ParseDecimal(row.Cell(6));
                            if (miktar <= 0) { skp++; continue; }

                            var existing = await _context.FscSerials
                                .FirstOrDefaultAsync(s => s.LotId == lot!.Id && s.SerialNo == seriNo);
                            if (existing == null)
                            {
                                _context.FscSerials.Add(new FscSerial
                                {
                                    LotId         = lot!.Id,
                                    SerialNo      = seriNo,
                                    InitialWeight = miktar,
                                    CurrentWeight = miktar,
                                    CreatedDate   = DateTime.Now,
                                    CreatedBy     = "ETL"
                                });
                                // Stok hareketi
                                _context.StockMovements.Add(new StockMovement
                                {
                                    Type          = MovementType.PurchaseEntry,
                                    DocumentNo    = irsNo.IfEmpty(lotNo),
                                    DocumentDate  = tarih ?? DateTime.Today,
                                    ProductId     = product.Id,
                                    Quantity      = miktar,
                                    Unit          = product.Unit ?? "KG",
                                    ToWarehouseId = defaultWarehouse?.Id,
                                    Description   = $"Lot: {lotNo} | Seri: {seriNo}",
                                    CreatedDate   = DateTime.Now,
                                    CreatedBy     = "ETL"
                                });
                                await _context.SaveChangesAsync();
                            }
                        }
                        catch (Exception ex) { errors.Add($"Satır {rowNum}: {ex.Message}"); }
                    }
                }
                catch (Exception ex) { errors.Add($"Lot {lotNo}: {ex.Message}"); }
            }
            return (ins, upd, skp, errors);
        }

        // ─── Üretim Kaydı Import ──────────────────────────────────────────────
        private async Task<(int ins, int upd, int skp, List<string> errs)> ImportUretim(List<IXLRangeRow> rows)
        {
            int ins = 0, upd = 0, skp = 0;
            var errors   = new List<string>();
            var machines = await _context.Machines.ToListAsync();
            var products = await _context.Products.ToListAsync();
            var warehouses = await _context.Warehouses.ToListAsync();
            var defaultWarehouse = warehouses.FirstOrDefault();

            var groups = rows
                .Select((r, idx) => new { Row = r, Idx = idx })
                .GroupBy(x => x.Row.Cell(1).GetString().Trim())
                .Where(g => !string.IsNullOrWhiteSpace(g.Key))
                .ToList();

            foreach (var group in groups)
            {
                var uretimNo = group.Key;
                var firstRow = group.First().Row;
                var firstIdx = group.First().Idx;
                try
                {
                    var tarih        = ParseDate(firstRow.Cell(2)) ?? DateTime.Today;
                    var makineName   = firstRow.Cell(3).GetString().Trim();
                    var mamulKodu    = firstRow.Cell(4).GetString().Trim();
                    var uretimMiktar = ParseDecimal(firstRow.Cell(5));

                    var machine = machines.FirstOrDefault(m =>
                        m.Code.Equals(makineName, StringComparison.OrdinalIgnoreCase) ||
                        m.Name.Equals(makineName, StringComparison.OrdinalIgnoreCase));
                    var mamul = products.FirstOrDefault(p => p.ProductCode == mamulKodu);

                    if (mamul == null) { errors.Add($"Satır {firstIdx+2} (Üretim {uretimNo}): Mamul ürün bulunamadı — '{mamulKodu}'"); skp += group.Count(); continue; }

                    // WorkOrder bul veya oluştur
                    var wo = await _context.WorkOrders.FirstOrDefaultAsync(w => w.WorkOrderNo == uretimNo);
                    bool woNew = wo == null;
                    if (woNew)
                    {
                        wo = new WorkOrder
                        {
                            WorkOrderNo      = uretimNo,
                            ProductId        = mamul.Id,
                            MachineId        = machine?.Id ?? (machines.FirstOrDefault()?.Id ?? 1),
                            PlannedDate      = tarih,
                            CompletedDate    = tarih,
                            PlannedQuantity  = uretimMiktar,
                            ActualQuantity   = uretimMiktar,
                            Status           = WorkOrderStatus.Tamamlandi,
                            CreatedDate      = DateTime.Now,
                            CreatedBy        = "ETL"
                        };
                        _context.WorkOrders.Add(wo);
                        await _context.SaveChangesAsync();
                        ins++;

                        // Mamul stok girişi
                        _context.StockMovements.Add(new StockMovement
                        {
                            Type          = MovementType.ProductionEntry,
                            DocumentNo    = uretimNo,
                            DocumentDate  = tarih,
                            ProductId     = mamul.Id,
                            Quantity      = uretimMiktar,
                            Unit          = mamul.Unit ?? "AD",
                            ToWarehouseId = defaultWarehouse?.Id,
                            WorkOrderId   = wo.Id,
                            Description   = $"Üretim: {uretimNo}",
                            CreatedDate   = DateTime.Now,
                            CreatedBy     = "ETL"
                        });
                    }
                    else { upd++; }

                    // Her satır = bir hammadde tüketimi
                    foreach (var item in group)
                    {
                        var row    = item.Row;
                        var rowNum = item.Idx + 2;
                        try
                        {
                            var lotNo        = row.Cell(6).GetString().Trim();
                            var hammaddeKodu = row.Cell(7).GetString().Trim();
                            var kullMiktar   = ParseDecimal(row.Cell(8));
                            var fire         = ParseDecimal(row.Cell(9));

                            if (string.IsNullOrWhiteSpace(lotNo) || kullMiktar <= 0) { skp++; continue; }

                            // Lot ve seri bul
                            var lot = await _context.FscLots
                                .Include(l => l.Serials)
                                .FirstOrDefaultAsync(l => l.LotNo == lotNo);

                            var serial = lot?.Serials.OrderByDescending(s => s.CurrentWeight).FirstOrDefault();

                            if (serial == null) { errors.Add($"Satır {rowNum}: Lot/Seri bulunamadı — '{lotNo}'"); skp++; continue; }

                            // ProductionDetail ekle
                            var alreadyExists = await _context.ProductionDetails
                                .AnyAsync(d => d.WorkOrderId == wo!.Id && d.FscSerialId == serial.Id);
                            if (!alreadyExists)
                            {
                                _context.ProductionDetails.Add(new ProductionDetail
                                {
                                    WorkOrderId     = wo!.Id,
                                    FscSerialId     = serial.Id,
                                    MachineId       = wo.MachineId,
                                    ProductionDate  = tarih,
                                    ConsumedWeight  = kullMiktar,
                                    WasteWeight     = fire,
                                    ProducedQuantity = uretimMiktar,
                                    Notes           = $"Excel: {hammaddeKodu}",
                                    CreatedDate     = DateTime.Now,
                                    CreatedBy       = "ETL"
                                });

                                // Seri ağırlığını güncelle
                                serial.CurrentWeight = Math.Max(0, serial.CurrentWeight - kullMiktar);
                                await _context.SaveChangesAsync();
                            }
                        }
                        catch (Exception ex) { errors.Add($"Satır {rowNum}: {ex.Message}"); }
                    }
                }
                catch (Exception ex) { errors.Add($"Üretim {uretimNo}: {ex.Message}"); }
            }
            return (ins, upd, skp, errors);
        }

        // ─── Satış Import ─────────────────────────────────────────────────────
        private async Task<(int ins, int upd, int skp, List<string> errs)> ImportSatis(List<IXLRangeRow> rows)
        {
            int ins = 0, upd = 0, skp = 0;
            var errors    = new List<string>();
            var customers = await _context.Customers.ToListAsync();
            var products  = await _context.Products.ToListAsync();
            var warehouses = await _context.Warehouses.ToListAsync();
            var defaultWarehouse = warehouses.FirstOrDefault();

            var groups = rows
                .Select((r, idx) => new { Row = r, Idx = idx })
                .GroupBy(x => x.Row.Cell(1).GetString().Trim())
                .Where(g => !string.IsNullOrWhiteSpace(g.Key))
                .ToList();

            foreach (var group in groups)
            {
                var satisNo  = group.Key;
                var firstRow = group.First().Row;
                var firstIdx = group.First().Idx;
                try
                {
                    var tarih       = ParseDate(firstRow.Cell(2)) ?? DateTime.Today;
                    var musteriKodu = firstRow.Cell(3).GetString().Trim();
                    var irsNo       = firstRow.Cell(7).GetString().Trim();
                    var fatNo       = firstRow.Cell(8).GetString().Trim();

                    var customer = customers.FirstOrDefault(c =>
                        c.CustomerCode == musteriKodu ||
                        c.Name.Equals(musteriKodu, StringComparison.OrdinalIgnoreCase));

                    if (customer == null) { errors.Add($"Satır {firstIdx+2} (Satış {satisNo}): Müşteri bulunamadı — '{musteriKodu}'"); skp += group.Count(); continue; }

                    // SalesOrder bul veya oluştur
                    var so = await _context.SalesOrders.FirstOrDefaultAsync(s => s.SalesOrderNo == satisNo);
                    bool soNew = so == null;
                    if (soNew)
                    {
                        so = new SalesOrder
                        {
                            SalesOrderNo   = satisNo,
                            CustomerId     = customer.Id,
                            OrderDate      = tarih,
                            DispatchDate   = tarih,
                            DispatchNo     = irsNo,
                            InvoiceNo      = fatNo,
                            Status         = SalesOrderStatus.TeslimEdildi,
                            CreatedDate    = DateTime.Now,
                            CreatedBy      = "ETL"
                        };
                        _context.SalesOrders.Add(so);
                        await _context.SaveChangesAsync();
                        ins++;
                    }
                    else { upd++; }

                    // Her satır = bir ürün kalemi
                    foreach (var item in group)
                    {
                        var row      = item.Row;
                        var rowNum   = item.Idx + 2;
                        try
                        {
                            var urunKodu   = row.Cell(4).GetString().Trim();
                            var miktar     = ParseDecimal(row.Cell(5));
                            var birimFiyat = ParseDecimal(row.Cell(6));

                            if (string.IsNullOrWhiteSpace(urunKodu) || miktar <= 0) { skp++; continue; }

                            var product = products.FirstOrDefault(p => p.ProductCode == urunKodu);
                            if (product == null) { errors.Add($"Satır {rowNum}: Ürün bulunamadı — '{urunKodu}'"); skp++; continue; }

                            var lineExists = await _context.SalesOrderLines
                                .AnyAsync(l => l.SalesOrderId == so!.Id && l.ProductId == product.Id);
                            if (!lineExists)
                            {
                                _context.SalesOrderLines.Add(new SalesOrderLine
                                {
                                    SalesOrderId = so!.Id,
                                    ProductId    = product.Id,
                                    Quantity     = miktar,
                                    UnitPrice    = birimFiyat,
                                    Unit         = product.Unit ?? "AD",
                                    CreatedDate  = DateTime.Now,
                                    CreatedBy    = "ETL"
                                });
                                // Stok hareketi
                                _context.StockMovements.Add(new StockMovement
                                {
                                    Type            = MovementType.SalesDispatch,
                                    DocumentNo      = irsNo.IfEmpty(satisNo),
                                    DocumentDate    = tarih,
                                    ProductId       = product.Id,
                                    Quantity        = miktar,
                                    Unit            = product.Unit ?? "AD",
                                    FromWarehouseId = defaultWarehouse?.Id,
                                    CustomerId      = customer.Id,
                                    Description     = $"Satış: {satisNo}",
                                    CreatedDate     = DateTime.Now,
                                    CreatedBy       = "ETL"
                                });
                                await _context.SaveChangesAsync();
                            }
                        }
                        catch (Exception ex) { errors.Add($"Satır {rowNum}: {ex.Message}"); }
                    }
                }
                catch (Exception ex) { errors.Add($"Satış {satisNo}: {ex.Message}"); }
            }
            return (ins, upd, skp, errors);
        }

        // ─── Yardımcı: Tarih ve Sayı parse ───────────────────────────────────
        private static DateTime? ParseDate(IXLCell cell)
        {
            try
            {
                if (cell.DataType == XLDataType.DateTime)
                    return cell.GetDateTime();
                var s = cell.GetString().Trim();
                if (string.IsNullOrWhiteSpace(s)) return null;
                string[] fmts = { "dd.MM.yyyy", "d.M.yyyy", "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy" };
                if (DateTime.TryParseExact(s, fmts, System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var dt))
                    return dt;
                if (DateTime.TryParse(s, out var dt2)) return dt2;
            }
            catch { }
            return null;
        }

        private static decimal ParseDecimal(IXLCell cell)
        {
            try
            {
                if (cell.DataType == XLDataType.Number) return (decimal)cell.GetDouble();
                var s = cell.GetString().Trim().Replace(',', '.');
                if (decimal.TryParse(s, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var d)) return d;
            }
            catch { }
            return 0;
        }
    }

    internal static class StringExt
    {
        public static string IfEmpty(this string s, string fallback) =>
            string.IsNullOrWhiteSpace(s) ? fallback : s;
    }
}
