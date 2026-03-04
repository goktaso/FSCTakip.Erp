using FSCTakip.Core.Entities;
using FSCTakip.DataAc.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSC_ERP.Controllers
{
    public class KagitTipController : BaseController
    {
        public KagitTipController(AppDbContext context) : base(context) { }

        public async Task<IActionResult> Index()
        {
            var list = await _context.PaperTypes.ToListAsync();
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> Save(PaperType model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Index));

            if (model.Id == 0)
            {
                model.IsActive = true;
                _context.PaperTypes.Add(model);
            }
            else
            {
                _context.PaperTypes.Update(model);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.PaperTypes.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Kayıt bulunamadı." });

            _context.PaperTypes.Remove(entity);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        public async Task<IActionResult> Export()
        {
            var data = await _context.PaperTypes.ToListAsync();
            return ExportToExcel(data, "KagitTipleri");
        }
    }
}

