using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Controllers
{
    public class CustomersController : BaseController
    {
        public CustomersController(AppDbContext context) : base(context) { }

        // POST /Customers/QuickAdd — tam form ile hızlı müşteri ekleme
        [HttpPost]
        public async Task<IActionResult> QuickAdd(
            string Name,
            string? Phone,
            string? Email,
            string? TaxOffice,
            string? TaxNumber,
            string? City,
            string? Address,
            string? FscLicenseCode,
            DateTime? FscExpiryDate,
            bool IsFscActive = false)
        {
            if (string.IsNullOrWhiteSpace(Name))
                return Json(new { success = false, message = "Müşteri adı zorunludur." });

            var tr = new System.Globalization.CultureInfo("tr-TR");
            var count = await _context.Customers.CountAsync();

            if (!string.IsNullOrEmpty(Email))
                Email = Email.Trim().Replace("İ", "i").Replace("I", "ı").ToLowerInvariant();

            if (!string.IsNullOrEmpty(Phone))
                Phone = new string(Phone.Where(char.IsDigit).ToArray());

            var c = new Customer
            {
                Name           = Name.Trim().ToUpper(tr),
                CustomerCode   = $"MHS-{(count + 1):D3}",
                Phone          = Phone,
                Email          = Email,
                TaxOffice      = TaxOffice?.Trim(),
                TaxNumber      = TaxNumber?.Trim(),
                City           = City?.Trim(),
                Address        = Address?.Trim(),
                FscLicenseCode = FscLicenseCode?.Trim().ToUpperInvariant(),
                FscExpiryDate  = FscExpiryDate,
                IsFscActive    = IsFscActive,
                IsActive       = true,
                CreatedDate    = DateTime.Now,
                CreatedBy      = User.Identity?.Name ?? "System"
            };
            _context.Customers.Add(c);
            await _context.SaveChangesAsync();
            return Json(new { success = true, id = c.Id, text = c.Name });
        }

        // GET /Customers/ExportCustomers
        public async Task<IActionResult> ExportCustomers()
        {
            var rows = await _context.Customers.OrderBy(c => c.Name).Select(c => new {
                Kod           = c.CustomerCode,
                HariciKod     = c.ExternalCode ?? "",
                Ad            = c.Name,
                Telefon       = c.Phone ?? "",
                Email         = c.Email ?? "",
                Adres         = c.Address ?? "",
                Sehir         = c.City ?? "",
                VergiDairesi  = c.TaxOffice ?? "",
                VergiNo       = c.TaxNumber ?? "",
                FscLisansKodu = c.FscLicenseCode ?? "",
                FscBitis      = c.FscExpiryDate != null ? c.FscExpiryDate.Value.ToString("dd.MM.yyyy") : "",
                FscAktif      = c.IsFscActive ? "Evet" : "Hayır",
                Durum         = c.IsActive ? "Aktif" : "Pasif"
            }).ToListAsync();

            return ExportToExcel(rows, "Musteriler");
        }

        // POST /Customers/ToggleStatus
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var item = await _context.Customers.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Kayıt bulunamadı." });
            item.IsActive    = !item.IsActive;
            item.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = item.IsActive });
        }

        // Müşteri Listesi - Verilerin gelmesini garanti eder
        public IActionResult Index()
        {
            // Veritabanından tüm aktif müşterileri en yeni en üstte olacak şekilde çekiyoruz [cite: 2026-03-04]
            var list = _context.Customers
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .ToList();

            return View(list ?? new List<Customer>());
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var item = await _context.Customers.FindAsync(id);
            if (item == null) return Json(new { success = false });
            return Json(new { success = true, data = item });
        }

        [HttpPost]
        public async Task<IActionResult> Save(Customer model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return Json(new { success = false, message = "Müşteri adı zorunludur." });

            // Düzenlemede mevcut e-posta/telefonu al — değişmemiş alanları yeniden doğrulama
            // (eski/geçersiz formatlı kayıtların başka alanları güncellenebilsin diye).
            string? storedEmail = null, storedPhone = null;
            if (model.Id != 0)
            {
                var stored = await _context.Customers.AsNoTracking()
                    .Where(c => c.Id == model.Id)
                    .Select(c => new { c.Email, c.Phone })
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

            if (model.Id == 0) // Yeni Kayıt
            {
                var count = await _context.Customers.CountAsync();
                model.CustomerCode = $"MHS-{(count + 1):D3}";
                model.ExternalCode = string.IsNullOrWhiteSpace(model.ExternalCode) ? null : model.ExternalCode.Trim().ToUpperInvariant();
                model.CreatedDate = DateTime.Now;
                model.IsActive = true;
                _context.Customers.Add(model);
            }
            else // Güncelleme - Tüm alanlar burada eşlenmelidir
            {
                var existing = await _context.Customers.FindAsync(model.Id);
                if (existing != null)
                {
                    existing.Name = model.Name;
                    existing.TaxNumber = model.TaxNumber;
                    existing.TaxOffice = model.TaxOffice;
                    existing.City = model.City;
                    existing.Address = model.Address;
                    existing.Email = model.Email;
                    existing.Phone = model.Phone;
                    existing.FscLicenseCode = model.FscLicenseCode;
                    existing.FscExpiryDate = model.FscExpiryDate;
                    existing.IsFscActive = model.IsFscActive;
                    existing.ExternalCode = string.IsNullOrWhiteSpace(model.ExternalCode) ? existing.ExternalCode : model.ExternalCode.Trim().ToUpperInvariant();
                    existing.UpdatedDate = DateTime.Now;
                    _context.Customers.Update(existing);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Müşteri kaydedildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}