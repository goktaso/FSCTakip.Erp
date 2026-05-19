using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Controllers
{
    public class SuppliersController : BaseController
    {
        public SuppliersController(AppDbContext context) : base(context) { }

        public async Task<IActionResult> Index() => View(await _context.Suppliers.ToListAsync());

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
            // --- Veri Temizleme ve Formatlama ---
            if (!string.IsNullOrEmpty(model.Email))
                model.Email = model.Email.Trim().ToLowerInvariant();

            if (!string.IsNullOrEmpty(model.Phone))
                model.Phone = new string(model.Phone.Where(char.IsDigit).ToArray());

            if (model.Id == 0) // Yeni Kayıt
            {
                var count = await _context.Suppliers.CountAsync();
                model.SupplierCode = $"TED-{(count + 1):D3}";
                model.CreatedDate = DateTime.Now;
                model.IsActive = true;
                _context.Suppliers.Add(model);
            }
            else // Güncelleme
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
                    existing.UpdatedDate   = DateTime.Now;
                    _context.Suppliers.Update(existing);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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
    }
}