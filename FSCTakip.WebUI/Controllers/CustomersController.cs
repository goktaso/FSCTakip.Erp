using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Controllers
{
    public class CustomersController : BaseController
    {
        public CustomersController(AppDbContext context) : base(context) { }

        // POST /Customers/QuickAdd — inline hızlı müşteri ekleme
        [HttpPost]
        public async Task<IActionResult> QuickAdd(string Name, string? Phone)
        {
            if (string.IsNullOrWhiteSpace(Name))
                return Json(new { success = false, message = "Müşteri adı zorunludur." });

            var count = await _context.Customers.CountAsync();
            var c = new Customer
            {
                Name         = Name.Trim().ToUpper(new System.Globalization.CultureInfo("tr-TR")),
                CustomerCode = $"MHS-{(count + 1):D3}",
                Phone        = Phone?.Trim(),
                IsActive     = true,
                CreatedDate  = DateTime.Now,
                CreatedBy    = User.Identity?.Name ?? "System"
            };
            _context.Customers.Add(c);
            await _context.SaveChangesAsync();
            return Json(new { success = true, id = c.Id, text = c.Name });
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
            // Mail Adresi Temizliği: Büyük İ/I karakterlerini standart karakterlere dönüştürür [cite: 2026-03-04]
            if (!string.IsNullOrEmpty(model.Email))
            {
                model.Email = model.Email
                    .Replace("İ", "i")
                    .Replace("I", "ı")
                    .Trim()
                    .ToLowerInvariant();
            }

            if (model.Id == 0) // Yeni Kayıt
            {
                var count = await _context.Customers.CountAsync();
                model.CustomerCode = $"MHS-{(count + 1):D3}";
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
                    existing.UpdatedDate = DateTime.Now;
                    _context.Customers.Update(existing);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index)); // İşlem sonrası listeye dön [cite: 2026-03-04]
        }
    }
    }