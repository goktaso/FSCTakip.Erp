using FSCTakip.Core.Entities;
using FSCTakip.DataAc.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
namespace FSC_ERP.Controllers
{
    public class PaperController : BaseController
    {
        public PaperController(AppDbContext context) : base(context) { }

        // --- LİSTELEME METOTLARI ---
        public async Task<IActionResult> Types() => View(await _context.PaperTypes.ToListAsync());
        public async Task<IActionResult> Colors() => View(await _context.PaperColors.ToListAsync());
        public async Task<IActionResult> FscTypes() => View(await _context.FscTypes.ToListAsync());
        public async Task<IActionResult> Widths() => View(await _context.PaperWidths.ToListAsync());
        public async Task<IActionResult> Weights() => View(await _context.PaperWeights.ToListAsync());

        // --- KAYDETME METOTLARI ---

        [HttpPost]
        public async Task<IActionResult> SaveType(PaperType model)
        {
            if (model.Id == 0) { model.IsActive = true; _context.PaperTypes.Add(model); }
            else _context.PaperTypes.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Types));
        }

        [HttpPost]
        public async Task<IActionResult> SaveColor(PaperColor model)
        {
            if (model.Id == 0) { model.IsActive = true; _context.PaperColors.Add(model); }
            else _context.PaperColors.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Colors));
        }

        [HttpPost]
       
        public async Task<IActionResult> SaveFscType(FSCTakip.Core.Entities.FscType model)
        {
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
            return RedirectToAction(nameof(FscTypes));
        }

        [HttpPost]
        public async Task<IActionResult> SaveWidth(PaperWidth model)
        {
            if (model.Id == 0) { model.IsActive = true; _context.PaperWidths.Add(model); }
            else _context.PaperWidths.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Widths));
        }

        [HttpPost]
        public async Task<IActionResult> SaveWeight(PaperWeight model)
        {
            if (model.Id == 0) { model.IsActive = true; _context.PaperWeights.Add(model); }
            else _context.PaperWeights.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Weights));
        }

        // --- EXCEL EXPORT METOTLARI ---
        public async Task<IActionResult> ExportWeights() => ExportToExcel(await _context.PaperWeights.ToListAsync(), "Gramajlar");
        public async Task<IActionResult> ExportColors() => ExportToExcel(await _context.PaperColors.ToListAsync(), "Renkler");
        public async Task<IActionResult> ExportTypes() => ExportToExcel(await _context.PaperTypes.ToListAsync(), "KagitTipleri");
        public async Task<IActionResult> ExportWidths() => ExportToExcel(await _context.PaperWidths.ToListAsync(), "BobinEnleri");
        public async Task<IActionResult> ExportFscTypes() => ExportToExcel(await _context.FscTypes.ToListAsync(), "FSCTipleri");
    }
}