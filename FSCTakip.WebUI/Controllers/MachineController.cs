using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FSC_ERP.Controllers;

namespace FSC_ERP.Controllers
{
    public class MachineController : BaseController
    {
        public MachineController(AppDbContext context) : base(context) { }

        // --- ÜRETİM TANIMLARI ---

        public async Task<IActionResult> Machines()
        {
            var list = await _context.Machines.ToListAsync();
            return View(list);
        }

        
        [HttpPost]
        public async Task<IActionResult> SaveMachine(Machine model)
        {
            if (model.Id == 0)
            {
                model.IsActive = true;
                model.CreatedDate = DateTime.Now;
                model.CreatedBy = User.Identity?.Name ?? "System";
                _context.Machines.Add(model);
            }
            else
            {
                var existing = await _context.Machines.FindAsync(model.Id);
                if (existing != null)
                {
                    existing.Name = model.Name;
                    existing.Code = model.Code;
                    existing.Type = model.Type;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                    existing.UpdatedBy = User.Identity?.Name ?? "System";
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Machines));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMachine(int id)
        {
            var machine = await _context.Machines.FindAsync(id);
            if (machine == null)
                return Json(new { success = false, message = "Makine bulunamadı." });

            _context.Machines.Remove(machine);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Makine silindi." });
        }

        public async Task<IActionResult> ExportMachines()
        {
            var data = await _context.Machines.ToListAsync();
            return ExportToExcel(data, "Makineler");
        }

        // --- [YENİ EKLENDİ] - Makine Durum Değiştirme Metodu ---
        [HttpPost]
        public async Task<IActionResult> ToggleMachineStatus(int id)
        {
            var item = await _context.Machines.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Kayıt bulunamadı." });

            item.IsActive = !item.IsActive;
            item.UpdatedDate = DateTime.Now;
            item.UpdatedBy = User.Identity?.Name ?? "System";

            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = item.IsActive });
        }
    }
}
