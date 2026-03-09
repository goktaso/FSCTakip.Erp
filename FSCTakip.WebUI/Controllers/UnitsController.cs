using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FSC_ERP.Controllers
{
    public class UnitsController : BaseController
    {
        public UnitsController(AppDbContext context) : base(context) { }

        public async Task<IActionResult> Index()
        {
            var units = await _context.Units
                .OrderBy(u => u.Name)
                .ToListAsync();
            return View(units);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnit(int id)
        {
            var unit = await _context.Units.FindAsync(id);
            if (unit == null) return Json(new { success = false, message = "KAYIT BULUNAMADI." });

            return Json(new { success = true, id = unit.Id, name = unit.Name, shortCode = unit.ShortCode, isActive = unit.IsActive });
        }

        [HttpPost]
        public async Task<IActionResult> Save(Unit model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                TempData["Error"] = "BİRİM ADI ZORUNLUDUR.";
                return RedirectToAction(nameof(Index));
            }

            if (model.Id == 0)
            {
                model.CreatedBy = "SYSTEM";
                model.CreatedDate = DateTime.Now;
                _context.Units.Add(model);
            }
            else
            {
                var existing = await _context.Units.FindAsync(model.Id);
                if (existing != null)
                {
                    existing.Name = model.Name;
                    existing.ShortCode = model.ShortCode;
                    existing.UpdatedBy = "SYSTEM";
                    existing.UpdatedDate = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var unit = await _context.Units.FindAsync(id);
            if (unit == null) return Json(new { success = false });

            unit.IsActive = !unit.IsActive;
            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = unit.IsActive });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var unit = await _context.Units.FindAsync(id);
            if (unit == null) return Json(new { success = false, message = "KAYIT BULUNAMADI." });

            _context.Units.Remove(unit);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        public async Task<IActionResult> ExportIndex()
        {
            var data = await _context.Units
                .OrderBy(u => u.Name)
                .Select(u => new { Birim_Adi = u.Name, Kisa_Kod = u.ShortCode ?? "-", Durum = u.IsActive ? "AKTİF" : "PASİF" })
                .ToListAsync();

            return ExportToExcel(data, "BirimListesi");
        }

        [HttpPost]
        public async Task<IActionResult> QuickSave(string name)
        {
            try
            {
                var entity = new Unit { Name = name.ToUpper(), IsActive = true }; // Unit yerine ilgili sınıf
                _context.Units.Add(entity);
                await _context.SaveChangesAsync();
                return Json(new { success = true, id = entity.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}