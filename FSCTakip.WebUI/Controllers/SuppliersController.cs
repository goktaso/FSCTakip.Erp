using ClosedXML.Excel;
using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Controllers
{
    public class SuppliersController : BaseController
    {
        public SuppliersController(AppDbContext context) : base(context) { }

        public async Task<IActionResult> Index() => View(await _context.Suppliers.OrderBy(s => s.Name).ToListAsync());

        // POST /Suppliers/QuickAdd — tam form ile hızlı tedarikçi ekleme
        [HttpPost]
        public async Task<IActionResult> QuickAdd(
            string Name,
            string? ContactPerson,
            string? Phone,
            string? Email,
            string? City,
            string? Address,
            string? TaxOffice,
            string? TaxNumber,
            string? FscCode,
            DateTime? FscExpiryDate,
            bool IsFscActive = false)
        {
            if (string.IsNullOrWhiteSpace(Name))
                return Json(new { success = false, message = "Tedarikçi adı zorunludur." });

            var tr = new System.Globalization.CultureInfo("tr-TR");
            var count = await _context.Suppliers.CountAsync();

            // E-posta normalize
            if (!string.IsNullOrEmpty(Email))
                Email = Email.Trim().Replace("İ", "i").Replace("I", "ı").ToLowerInvariant();

            // Telefon yalnızca rakam
            if (!string.IsNullOrEmpty(Phone))
                Phone = new string(Phone.Where(char.IsDigit).ToArray());

            var s = new Supplier
            {
                Name          = Name.Trim().ToUpper(tr),
                SupplierCode  = $"TED-{(count + 1):D3}",
                ContactPerson = ContactPerson?.Trim(),
                Phone         = Phone,
                Email         = Email,
                City          = City?.Trim(),
                Address       = Address?.Trim(),
                TaxOffice     = TaxOffice?.Trim(),
                TaxNumber     = TaxNumber?.Trim(),
                FscCode       = FscCode?.Trim().ToUpperInvariant(),
                FscExpiryDate = FscExpiryDate,
                IsFscActive   = IsFscActive,
                IsActive      = true,
                CreatedDate   = DateTime.Now,
                CreatedBy     = User.Identity?.Name ?? "System"
            };
            _context.Suppliers.Add(s);
            await _context.SaveChangesAsync();
            return Json(new { success = true, id = s.Id, text = s.Name });
        }

        [HttpGet]
        public async Task<IActionResult> GetSupplier(int id)
        {
            var item = await _context.Suppliers.FindAsync(id);
            if (item == null) return Json(new { success = false });
            return Json(new { success = true, data = item });
        }

        [HttpPost]
        public async Task<IActionResult> Save(Supplier model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return Json(new { success = false, message = "Tedarikçi adı zorunludur." });

            // Düzenlemede mevcut e-posta/telefonu al — değişmemiş alanları yeniden doğrulama
            // (eski/geçersiz formatlı kayıtların başka alanları güncellenebilsin diye).
            string? storedEmail = null, storedPhone = null;
            if (model.Id != 0)
            {
                var stored = await _context.Suppliers.AsNoTracking()
                    .Where(s => s.Id == model.Id)
                    .Select(s => new { s.Email, s.Phone })
                    .FirstOrDefaultAsync();
                storedEmail = stored?.Email;
                storedPhone = stored?.Phone;
            }

            // Email formatı kontrolü — yalnızca yeni kayıt veya e-posta değiştiyse
            if (!string.IsNullOrEmpty(model.Email))
            {
                model.Email = model.Email.Replace("İ", "i").Replace("I", "ı").Trim().ToLowerInvariant();
                var emailChanged = storedEmail == null
                    || !string.Equals(storedEmail.Trim(), model.Email, StringComparison.OrdinalIgnoreCase);
                if (emailChanged && !System.Text.RegularExpressions.Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    return Json(new { success = false, message = "Geçerli bir e-posta adresi giriniz." });
            }

            // Telefon temizleme — sadece rakamlar; uzunluk kontrolü yalnızca telefon değiştiyse
            if (!string.IsNullOrEmpty(model.Phone))
            {
                model.Phone = new string(model.Phone.Where(char.IsDigit).ToArray());
                var storedPhoneDigits = storedPhone == null ? null : new string(storedPhone.Where(char.IsDigit).ToArray());
                var phoneChanged = storedPhoneDigits == null || storedPhoneDigits != model.Phone;
                if (phoneChanged && model.Phone.Length > 0 && (model.Phone.Length < 10 || model.Phone.Length > 15))
                    return Json(new { success = false, message = "Telefon numarası 10-15 rakam arasında olmalıdır." });
            }

            // FSC son geçerlilik tarihi mantık kontrolü
            if (model.IsFscActive && model.FscExpiryDate.HasValue && model.FscExpiryDate.Value < DateTime.Today.AddDays(-365))
                return Json(new { success = false, message = "FSC son geçerlilik tarihi çok eski görünüyor. Lütfen kontrol ediniz." });

            if (model.Id == 0)
            {
                var count = await _context.Suppliers.CountAsync();
                model.SupplierCode = $"TED-{(count + 1):D3}";
                model.ExternalCode = string.IsNullOrWhiteSpace(model.ExternalCode) ? null : model.ExternalCode.Trim().ToUpperInvariant();
                model.CreatedDate = DateTime.Now;
                model.IsActive = true;
                _context.Suppliers.Add(model);
            }
            else
            {
                var existing = await _context.Suppliers.FindAsync(model.Id);
                if (existing != null)
                {
                    existing.Name          = model.Name;
                    existing.ContactPerson = model.ContactPerson;
                    existing.Email         = model.Email;
                    existing.Phone         = model.Phone;
                    existing.Address       = model.Address;
                    existing.City          = model.City;
                    existing.TaxOffice     = model.TaxOffice;
                    existing.TaxNumber     = model.TaxNumber;
                    existing.FscCode       = model.FscCode;
                    existing.FscExpiryDate = model.FscExpiryDate;
                    existing.IsFscActive   = model.IsFscActive;
                    existing.ExternalCode  = string.IsNullOrWhiteSpace(model.ExternalCode) ? existing.ExternalCode : model.ExternalCode.Trim().ToUpperInvariant();
                    existing.UpdatedDate   = DateTime.Now;
                    _context.Suppliers.Update(existing);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Tedarikçi kaydedildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var item = await _context.Suppliers.FindAsync(id);
            if (item == null) return Json(new { success = false });

            item.IsActive = !item.IsActive;
            item.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = item.IsActive });
        }

        // GET /Suppliers/ExportSuppliers
        public async Task<IActionResult> ExportSuppliers()
        {
            var rows = await _context.Suppliers.OrderBy(s => s.Name).Select(s => new {
                Kod         = s.SupplierCode,
                HariciKod   = s.ExternalCode ?? "",
                Ad          = s.Name,
                Yetkili     = s.ContactPerson,
                Telefon     = s.Phone,
                Email       = s.Email,
                Adres       = s.Address,
                Sehir       = s.City,
                VergiDairesi = s.TaxOffice,
                VergiNo     = s.TaxNumber,
                FscKodu     = s.FscCode,
                FscBitis    = s.FscExpiryDate != null ? s.FscExpiryDate.Value.ToString("dd.MM.yyyy") : "",
                FscAktif    = s.IsFscActive ? "Evet" : "Hayır",
                Durum       = s.IsActive ? "Aktif" : "Pasif"
            }).ToListAsync();

            return ExportToExcel(rows, "Tedarikciler");
        }

        // GET /Suppliers/ExportFscTemplate
        // Mevcut tedarikçileri FSC sütunları sarı olarak export eder (doldurulacak alanlar vurgulanır)
        public async Task<IActionResult> ExportFscTemplate()
        {
            var suppliers = await _context.Suppliers
                .OrderBy(s => s.Name)
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("FSC_Kodlari");

            // ── Başlık satırı ──────────────────────────────────────────
            var headers = new[]
            {
                ("TedarikciKodu",   15, false),
                ("TedarikciAdi",    42, false),
                ("FscKodu",         26, true),
                ("FscBitisTarihi",  18, true),   // YYYY-AA-GG
                ("FscAktif",        10, true)     // EVET / HAYIR
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var (name, width, editable) = headers[i];
                var cell = ws.Cell(1, i + 1);
                cell.Value = name;
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = editable
                    ? XLColor.FromHtml("#1e3d14")   // koyu yeşil — doldurulacak
                    : XLColor.FromHtml("#374151");   // koyu gri   — referans
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Column(i + 1).Width = width;
            }

            // ── Veri satırları ─────────────────────────────────────────
            for (int r = 0; r < suppliers.Count; r++)
            {
                var s   = suppliers[r];
                var row = r + 2;
                var hasFsc = !string.IsNullOrWhiteSpace(s.FscCode);

                // Referans sütunlar (gri, değiştirme)
                ws.Cell(row, 1).Value = s.SupplierCode;
                ws.Cell(row, 2).Value = s.Name;
                ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#f3f4f6");
                ws.Cell(row, 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#f3f4f6");
                ws.Cell(row, 1).Style.Font.FontColor       = XLColor.FromHtml("#6b7280");
                ws.Cell(row, 2).Style.Font.FontColor       = XLColor.FromHtml("#6b7280");

                // FSC sütunlar
                ws.Cell(row, 3).Value = s.FscCode ?? "";
                ws.Cell(row, 4).Value = s.FscExpiryDate.HasValue
                    ? s.FscExpiryDate.Value.ToString("yyyy-MM-dd") : "";
                ws.Cell(row, 5).Value = s.IsFscActive ? "EVET" : "HAYIR";

                if (!hasFsc)
                {
                    // Boş FSC → sarı vurgu (doldurulması gerekiyor)
                    ws.Cell(row, 3).Style.Fill.BackgroundColor = XLColor.FromHtml("#fef9c3");
                    ws.Cell(row, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#fef9c3");
                    ws.Cell(row, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#fef9c3");
                }
            }

            // ── Dondurulan başlık + not satırı ────────────────────────
            ws.SheetView.FreezeRows(1);

            var noteRow = suppliers.Count + 3;
            var noteCell = ws.Cell(noteRow, 1);
            noteCell.Value = "NOT: TedarikciKodu ve TedarikciAdi sütunlarını değiştirmeyin. " +
                             "FscKodu doldurun, FscBitisTarihi için YYYY-AA-GG (ör. 2027-06-30) formatını kullanın. " +
                             "FscAktif: EVET veya HAYIR yazın.";
            noteCell.Style.Font.Italic = true;
            noteCell.Style.Font.FontColor = XLColor.FromHtml("#991b1b");
            ws.Range(noteRow, 1, noteRow, 5).Merge();

            var legendRow = suppliers.Count + 4;
            ws.Cell(legendRow, 1).Value = "■ Sarı satır = FSC kodu girilmemiş (doldurulması zorunlu değil ama tavsiye edilir)";
            ws.Cell(legendRow, 1).Style.Font.FontColor = XLColor.FromHtml("#92400e");
            ws.Range(legendRow, 1, legendRow, 5).Merge();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"FSC_Kodlari_{DateTime.Now:ddMMyyyy}.xlsx");
        }

        // POST /Suppliers/ImportFscCodes
        [HttpPost]
        public async Task<IActionResult> ImportFscCodes(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Dosya seçilmedi." });

            int updated = 0, skipped = 0;
            var errors  = new List<string>();

            try
            {
                using var stream = file.OpenReadStream();
                using var wb     = new XLWorkbook(stream);
                var ws = wb.Worksheets.First();

                var allRows = ws.RangeUsed()?.RowsUsed().ToList() ?? new();
                if (allRows.Count < 2)
                    return Json(new { success = false, message = "Dosyada veri satırı bulunamadı." });

                // Başlık haritası
                var headerRow = allRows[0];
                var hdrs = headerRow.CellsUsed()
                    .Select((c, i) => new { Col = i + 1, Name = c.GetString().Trim() })
                    .ToDictionary(x => x.Name, x => x.Col, StringComparer.OrdinalIgnoreCase);

                string ColVal(IXLRangeRow r, string name)
                    => hdrs.TryGetValue(name, out var c) ? r.Cell(c).GetString().Trim() : string.Empty;

                var dataRows = allRows.Skip(1).ToList();

                foreach (var row in dataRows)
                {
                    var kod   = ColVal(row, "TedarikciKodu");
                    var fsc   = ColVal(row, "FscKodu");
                    var tarih = ColVal(row, "FscBitisTarihi");
                    var aktif = ColVal(row, "FscAktif");

                    if (string.IsNullOrWhiteSpace(kod))  { skipped++; continue; }
                    if (string.IsNullOrWhiteSpace(fsc))  { skipped++; continue; }

                    var supplier = await _context.Suppliers
                        .FirstOrDefaultAsync(s => s.SupplierCode == kod);

                    if (supplier == null)
                    {
                        errors.Add($"{kod}: Tedarikçi bulunamadı (kod eşleşmedi).");
                        continue;
                    }

                    supplier.FscCode     = fsc.ToUpperInvariant();
                    supplier.IsFscActive = !aktif.Equals("HAYIR", StringComparison.OrdinalIgnoreCase);

                    if (!string.IsNullOrWhiteSpace(tarih) &&
                        DateTime.TryParse(tarih, out var dt))
                        supplier.FscExpiryDate = dt;

                    supplier.UpdatedDate = DateTime.Now;
                    supplier.UpdatedBy   = "FSC_IMPORT";
                    updated++;
                }

                await _context.SaveChangesAsync();

                return Json(new {
                    success  = true,
                    message  = $"{updated} tedarikçi FSC kodu güncellendi, {skipped} satır atlandı.",
                    updated, skipped,
                    errors
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Dosya okuma hatası: {ex.Message}" });
            }
        }
    }
}
