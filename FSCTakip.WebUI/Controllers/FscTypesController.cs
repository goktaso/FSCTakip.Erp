using FSCTakip.DataAc.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSC_ERP.Controllers
{
    public class FscTypesController : BaseController
    {
        public FscTypesController(AppDbContext context) : base(context) { }

        public async Task<IActionResult> Index()
        {
            var list = await _context.FscTypes.ToListAsync();
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> Save(FSCTakip.Core.Entities.FscType model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Index));

            if (model.Id == 0)
            {
                model.IsActive = true;
                _context.FscTypes.Add(model);
            }
            else
            {
                _context.FscTypes.Update(model);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.FscTypes.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Kayıt bulunamadı." });

            _context.FscTypes.Remove(entity);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        public async Task<IActionResult> Export()
        {
            var data = await _context.FscTypes.ToListAsync();
            return ExportToExcel(data, "FSCTipleri");
        }
    }
}

