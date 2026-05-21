using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Controllers
{
    public class WarehouseController : BaseController
    {
        public WarehouseController(AppDbContext context) : base(context) { }

        // POST /Warehouse/QuickAdd — inline hızlı depo ekleme
        [HttpPost]
        public async Task<IActionResult> QuickAdd(string Name)
        {
            if (string.IsNullOrWhiteSpace(Name))
                return Json(new { success = false, message = "Depo adı zorunludur." });
            var count = await _context.Warehouses.CountAsync();
            var w = new Warehouse
            {
                Name        = Name.Trim().ToUpper(new System.Globalization.CultureInfo("tr-TR")),
                Code        = $"DEP-{(count + 1):D2}",
                IsActive    = true,
                CreatedDate = DateTime.Now,
                CreatedBy   = User.Identity?.Name ?? "System"
            };
            _context.Warehouses.Add(w);
            await _context.SaveChangesAsync();
            return Json(new { success = true, id = w.Id, text = w.Name });
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Depo Tanımlamaları";
            var list = await _context.Warehouses.OrderBy(w => w.Code).ToListAsync();
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> Save(Warehouse model)
        {
            try
            {
                if (model.Id == 0)
                {
                    model.IsActive = true;
                    model.CreatedDate = DateTime.Now;
                    model.CreatedBy = User.Identity?.Name ?? "System";
                    _context.Warehouses.Add(model);
                }
                else
                {
                    var existing = await _context.Warehouses.FindAsync(model.Id);
                    if (existing == null)
                        return Json(new { success = false, message = "Kayıt bulunamadı." });

                    existing.Name = model.Name;
                    existing.Code = model.Code;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                    existing.UpdatedBy = User.Identity?.Name ?? "System";
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Depo kaydedildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var item = await _context.Warehouses.FindAsync(id);
            if (item == null) return Json(new { success = false });
            return Json(new { success = true, data = new { item.Id, item.Name, item.Code, item.IsActive } });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Warehouses.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Kayıt bulunamadı." });

            var used = await _context.StockMovements.AnyAsync(m => m.FromWarehouseId == id || m.ToWarehouseId == id);
            if (used)
                return Json(new { success = false, message = "Bu depo stok hareketlerinde kullanılmaktadır, silinemez." });

            _context.Warehouses.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Depo silindi." });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var item = await _context.Warehouses.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Kayıt bulunamadı." });

            item.IsActive = !item.IsActive;
            item.UpdatedDate = DateTime.Now;
            item.UpdatedBy = User.Identity?.Name ?? "System";

            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = item.IsActive });
        }

        public async Task<IActionResult> Export()
        {
            var data = await _context.Warehouses.OrderBy(w => w.Code).ToListAsync();
            var rows = data.Select(w => new {
                Kod    = w.Code,
                Adı    = w.Name,
                Durum  = w.IsActive ? "Aktif" : "Pasif"
            });
            return ExportToExcel(rows, "Depolar");
        }
    }
}
