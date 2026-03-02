using FSC_ERP.Controllers;
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

    // Excel için sadece tek satır!
    public async Task<IActionResult> ExportBagTypes()
    {
        var data = await _context.BagTypes.ToListAsync();
        return ExportToExcel(data, "TorbaTipleri");
    }
}