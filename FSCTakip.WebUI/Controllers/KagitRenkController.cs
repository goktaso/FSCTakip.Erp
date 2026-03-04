using FSCTakip.Core.Entities;
using FSCTakip.DataAc.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSC_ERP.Controllers
{
    public class KagitRenkController : BaseController
    {
        public KagitRenkController(AppDbContext context) : base(context) { }

        public async Task<IActionResult> Index()
        {
            var list = await _context.PaperColors.ToListAsync();
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> Save(PaperColor model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Index));

            if (model.Id == 0)
            {
                model.IsActive = true;
                _context.PaperColors.Add(model);
            }
            else
            {
                _context.PaperColors.Update(model);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.PaperColors.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Kayıt bulunamadı." });

            _context.PaperColors.Remove(entity);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        public async Task<IActionResult> Export()
        {
            var data = await _context.PaperColors.ToListAsync();
            return ExportToExcel(data, "KagitRenkleri");
        }
    }
}

