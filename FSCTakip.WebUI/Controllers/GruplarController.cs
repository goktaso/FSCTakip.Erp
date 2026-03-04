using FSCTakip.Core.Entities;
using FSCTakip.DataAc.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSC_ERP.Controllers
{
    public class GruplarController : BaseController
    {
        public GruplarController(AppDbContext context) : base(context) { }

        public async Task<IActionResult> Index()
        {
            var list = await _context.ProductGroups.ToListAsync();
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> Save(ProductGroup model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Index));

            if (model.Id == 0)
                _context.ProductGroups.Add(model);
            else
                _context.ProductGroups.Update(model);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.ProductGroups.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Kayıt bulunamadı." });

            _context.ProductGroups.Remove(entity);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        public async Task<IActionResult> Export()
        {
            var data = await _context.ProductGroups.ToListAsync();
            return ExportToExcel(data, "Gruplar");
        }
    }
}

