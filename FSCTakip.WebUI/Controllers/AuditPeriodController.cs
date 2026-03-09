using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class AuditPeriodController : Controller
{
    private readonly AppDbContext _context; // Kendi context adınızla güncelleyin

    public AuditPeriodController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _context.AuditPeriods.OrderByDescending(x => x.Year).ToListAsync();
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> GetPeriod(int id)
    {
        var period = await _context.AuditPeriods.FindAsync(id);
        if (period == null) return Json(new { success = false });
        return Json(new
        {
            success = true,
            id = period.Id,
            year = period.Year,
            startDate = period.StartDate.ToString("yyyy-MM-dd"),
            endDate = period.EndDate.ToString("yyyy-MM-dd"),
            description = period.Description
        });
    }

    [HttpPost]
    public async Task<IActionResult> Save(AuditPeriod model)
    {
        if (model.Id == 0)
        {
            model.IsActive = true;
            _context.AuditPeriods.Add(model);
        }
        else
        {
            _context.AuditPeriods.Update(model);
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var period = await _context.AuditPeriods.FindAsync(id);
        if (period == null) return Json(new { success = false });
        period.IsActive = !period.IsActive;
        await _context.SaveChangesAsync();
        return Json(new { success = true, isActive = period.IsActive });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var period = await _context.AuditPeriods.FindAsync(id);
        if (period == null) return Json(new { success = false });
        _context.AuditPeriods.Remove(period);
        await _context.SaveChangesAsync();
        return Json(new { success = true });
    }
}