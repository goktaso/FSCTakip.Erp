using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
namespace FSCTakip.WebUI.Controllers
{
    public class MachineController : BaseController
    {
        public MachineController(AppDbContext context) : base(context) { }

        // POST /Machine/QuickAdd — inline hızlı makine ekleme
        [HttpPost]
        public async Task<IActionResult> QuickAdd(string Name)
        {
            if (string.IsNullOrWhiteSpace(Name))
                return Json(new { success = false, message = "Makine adı zorunludur." });
            var m = new Machine
            {
                Name        = Name.Trim().ToUpper(new System.Globalization.CultureInfo("tr-TR")),
                IsActive    = true,
                CreatedDate = DateTime.Now,
                CreatedBy   = User.Identity?.Name ?? "System"
            };
            _context.Machines.Add(m);
            await _context.SaveChangesAsync();
            return Json(new { success = true, id = m.Id, text = m.Name });
        }

        // --- ÜRETİM TANIMLARI ---

        public async Task<IActionResult> Machines()
        {
            var list = await _context.Machines.Include(m => m.MachineType).ToListAsync();
            ViewBag.MachineTypes = await _context.MachineTypes.Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync();
            return View(list);
        }

        // ── MAKİNE TÜRLERİ (müşteriye özel — sabit kodlanmadı) ──────────────

        public async Task<IActionResult> Types() => View(await _context.MachineTypes.ToListAsync());

        [HttpPost]
        public async Task<IActionResult> SaveType(MachineType model)
        {
            if (model.Id == 0)
            {
                model.IsActive = true;
                model.CreatedDate = DateTime.Now;
                model.CreatedBy = User.Identity?.Name ?? "System";
                _context.MachineTypes.Add(model);
            }
            else
            {
                var existing = await _context.MachineTypes.FindAsync(model.Id);
                if (existing != null)
                {
                    existing.Name = model.Name;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                    existing.UpdatedBy = User.Identity?.Name ?? "System";
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Types));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteType(int id)
        {
            var item = await _context.MachineTypes.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Kayıt bulunamadı." });

            var used = await _context.Machines.AnyAsync(m => m.MachineTypeId == id);
            if (used)
                return Json(new { success = false, message = "Bu makine türü makinelerde kullanılmaktadır. Silmek yerine pasife alabilirsiniz." });

            try
            {
                _context.MachineTypes.Remove(item);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Makine türü silindi." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Bu makine türü silinemez." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleTypeStatus(int id)
        {
            var item = await _context.MachineTypes.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Kayıt bulunamadı." });

            item.IsActive = !item.IsActive;
            item.UpdatedDate = DateTime.Now;
            item.UpdatedBy = User.Identity?.Name ?? "System";

            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = item.IsActive });
        }

        public async Task<IActionResult> ExportTypes() => ExportToExcel(await _context.MachineTypes.ToListAsync(), "MakineTurleri");

        
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
                    existing.MachineTypeId = model.MachineTypeId;
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

            var usedInWorkOrders = await _context.WorkOrders.AnyAsync(w => w.MachineId == id);
            var usedInProduction = await _context.ProductionDetails.AnyAsync(p => p.MachineId == id);
            if (usedInWorkOrders || usedInProduction)
                return Json(new { success = false, message = "Bu makine iş emirlerinde kullanılmaktadır. Silmek yerine pasife alabilirsiniz." });

            try
            {
                _context.Machines.Remove(machine);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Makine silindi." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Bu makine silinemez." });
            }
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
