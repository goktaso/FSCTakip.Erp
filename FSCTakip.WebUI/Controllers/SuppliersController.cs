using FSCTakip.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FSCTakip.WebUI.Controllers
{
    public class SuppliersController : Controller
    {
        // Namespace belirsizliğini önlemek için tam yol kullanıldı
        private readonly FSCTakip.DataAccess.Data.AppDbContext _context;

        public SuppliersController(FSCTakip.DataAccess.Data.AppDbContext context)
        {
            _context = context;
        }

        // Tedarikçi Listesi (Müşteri sayfası formatında)
        public async Task<IActionResult> Index()
        {
            var suppliers = await _context.Suppliers
                .OrderBy(s => s.Name)
                .ToListAsync();
            return View(suppliers);
        }

        // Kaydetme ve Güncelleme İşlemi (ID Bazlı)
        [HttpPost]
        public async Task<IActionResult> Save(Supplier model)
        {
            if (model.Id == 0)
            {
                // Yeni Kayıt
                _context.Suppliers.Add(model);
            }
            else
            {
                // Güncelleme
                var existing = await _context.Suppliers.FindAsync(model.Id);
                if (existing != null)
                {
                    existing.SupplierCode = model.SupplierCode;
                    existing.Name = model.Name;
                    existing.FscCode = model.FscCode;
                    existing.FscExpiryDate = model.FscExpiryDate;
                    existing.IsFscActive = model.IsFscActive;
                    _context.Suppliers.Update(existing);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Modal için Tedarikçi Verisi Getirme
        [HttpGet]
        public async Task<IActionResult> GetSupplier(int id)
        {
            var s = await _context.Suppliers.FindAsync(id);
            if (s == null) return Json(new { success = false });

            return Json(new
            {
                success = true,
                id = s.Id,
                name = s.Name,
                fscCode = s.FscCode,
                supplierCode = s.SupplierCode,
                fscExpiryDate = s.FscExpiryDate?.ToString("yyyy-MM-dd"),
                isFscActive = s.IsFscActive
            });
        }

        // Excel Export (Müşteri sayfası ile aynı format)
        public IActionResult ExportExcel()
        {
            // İleride Excel kütüphanesi eklendiğinde burası doldurulacak
            return Content("Excel hazırlama altyapısı hazırlandı.");
        }
    }
}