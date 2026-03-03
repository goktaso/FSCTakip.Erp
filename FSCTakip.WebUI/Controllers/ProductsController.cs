using FSC_ERP.Controllers;
using FSCTakip.Core.Entities;
using FSCTakip.DataAc.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class ProductController : BaseController // Base'den türüyor
{
    public ProductController(AppDbContext context) : base(context) { }

    public async Task<IActionResult> BagTypes()
    {
        var list = await _context.BagTypes.ToListAsync();
        return View(list);
    }

    [HttpPost]
    public async Task<IActionResult> SaveBagType(BagType model)
    {
        if (model.Id == 0)
        {
            model.IsActive = true;
            _context.BagTypes.Add(model);
        }
        else
        {
            _context.BagTypes.Update(model);
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(BagTypes));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteBagType(int id)
    {
        try
        {
            var entity = await _context.BagTypes.FindAsync(id);
            if (entity == null)
                return Json(new { success = false, message = "Kayıt bulunamadı." });
            _context.BagTypes.Remove(entity);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Torba tipi başarıyla silindi." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Hata: " + ex.Message });
        }
    }

    // Excel için sadece tek satır!
    public async Task<IActionResult> ExportBagTypes()
    {
        var data = await _context.BagTypes.ToListAsync();
        return ExportToExcel(data, "TorbaTipleri");
    }
}