using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSC_ERP.Controllers
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

            // View tarafındaki JS doğrudan 'data.id' beklediği için nesneyi düzeltiyoruz
            return Json(new
            {
                success = true,
                id = item.Id,
                name = item.Name,
                contactPerson = item.ContactPerson,
                phone = item.Phone,
                email = item.Email,
                fscCode = item.FscCode,
                fscExpiryDate = item.FscExpiryDate?.ToString("yyyy-MM-dd") // HTML date input uyumu
            });
        }

        [HttpPost]
        public async Task<IActionResult> Save(Supplier model)
        {
            if (!string.IsNullOrEmpty(model.Email))
                model.Email = model.Email.Trim().ToLowerInvariant();

            if (model.Id == 0)
            {
                var count = await _context.Suppliers.CountAsync();
                model.SupplierCode = $"TED-{(count + 1):D3}";
                model.CreatedDate = DateTime.Now;
                model.IsActive = true;
                _context.Suppliers.Add(model);
            }
            else
            {
                var existing = await _context.Suppliers.FindAsync(model.Id);
                if (existing != null)
                {
                    existing.Name = model.Name;
                    existing.ContactPerson = model.ContactPerson;
                    existing.Email = model.Email;
                    existing.Phone = model.Phone;
                    existing.FscCode = model.FscCode;
                    existing.FscExpiryDate = model.FscExpiryDate;
                    existing.UpdatedDate = DateTime.Now;
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