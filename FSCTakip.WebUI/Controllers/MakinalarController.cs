using FSCTakip.DataAc.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSC_ERP.Controllers
{
    public class MakinalarController : BaseController
    {
        public MakinalarController(AppDbContext context) : base(context) { }

        public async Task<IActionResult> Index()
        {
            var list = await _context.Machines.ToListAsync();
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> Save(FSCTakip.Core.Entities.Machine model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Index));

            if (model.Id == 0)
            {
                model.IsActive = true;
                _context.Machines.Add(model);
            }
            else
            {
                _context.Machines.Update(model);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Machines.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Kayıt bulunamadı." });

            _context.Machines.Remove(entity);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        public async Task<IActionResult> Export()
        {
            var data = await _context.Machines.ToListAsync();
            return ExportToExcel(data, "Makinalar");
        }
    }
}

