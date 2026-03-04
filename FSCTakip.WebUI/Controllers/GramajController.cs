using FSCTakip.Core.Entities;
using FSCTakip.DataAc.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSC_ERP.Controllers
{
    public class GramajController : BaseController
    {
        public GramajController(AppDbContext context) : base(context) { }

        public async Task<IActionResult> Index()
        {
            var list = await _context.PaperWeights.ToListAsync();
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> Save(PaperWeight model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Index));

            if (model.Id == 0)
            {
                model.IsActive = true;
                _context.PaperWeights.Add(model);
            }
            else
            {
                _context.PaperWeights.Update(model);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.PaperWeights.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Kayıt bulunamadı." });

            _context.PaperWeights.Remove(entity);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        public async Task<IActionResult> Export()
        {
            var data = await _context.PaperWeights.ToListAsync();
            return ExportToExcel(data, "Gramajlar");
        }
    }
}

