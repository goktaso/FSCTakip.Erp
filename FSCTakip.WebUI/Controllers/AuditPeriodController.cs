using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Controllers
{
    public class AuditPeriodController : BaseController
    {
        public AuditPeriodController(AppDbContext context) : base(context) { }

        // GET /AuditPeriod
        public async Task<IActionResult> Index()
        {
            var list = await _context.AuditPeriods
                .OrderByDescending(x => x.Year)
                .ToListAsync();

            ViewData["Title"] = "Denetim Dönemleri";
            return View(list);
        }

        // GET /AuditPeriod/GetPeriod/{id}
        [HttpGet]
        public async Task<IActionResult> GetPeriod(int id)
        {
            var p = await _context.AuditPeriods.FindAsync(id);
            if (p == null) return Json(new { success = false });
            return Json(new
            {
                success     = true,
                id          = p.Id,
                year        = p.Year,
                startDate   = p.StartDate.ToString("yyyy-MM-dd"),
                endDate     = p.EndDate.ToString("yyyy-MM-dd"),
                description = p.Description
            });
        }

        // POST /AuditPeriod/Save
        [HttpPost]
        public async Task<IActionResult> Save(AuditPeriod model)
        {
            try
            {
                if (model.StartDate >= model.EndDate)
                    return Json(new { success = false, message = "Başlangıç tarihi bitiş tarihinden önce olmalıdır." });

                if (model.Id == 0)
                {
                    model.IsActive    = true;
                    model.CreatedDate = DateTime.Now;
                    model.CreatedBy   = User.Identity?.Name ?? "SYSTEM";
                    _context.AuditPeriods.Add(model);
                }
                else
                {
                    var existing = await _context.AuditPeriods.FindAsync(model.Id);
                    if (existing == null) return Json(new { success = false, message = "Dönem bulunamadı." });
                    existing.Year        = model.Year;
                    existing.StartDate   = model.StartDate;
                    existing.EndDate     = model.EndDate;
                    existing.Description = model.Description;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Dönem kaydedildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST /AuditPeriod/ToggleStatus
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var p = await _context.AuditPeriods.FindAsync(id);
            if (p == null) return Json(new { success = false });
            p.IsActive = !p.IsActive;
            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = p.IsActive });
        }

        // POST /AuditPeriod/Delete
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var p = await _context.AuditPeriods.FindAsync(id);
                if (p == null) return Json(new { success = false, message = "Dönem bulunamadı." });
                _context.AuditPeriods.Remove(p);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Dönem silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST /AuditPeriod/LockPeriod
        [HttpPost]
        public async Task<IActionResult> LockPeriod(int id)
        {
            if (!IsAdminUser) return Json(new { success = false, message = "Yetkisiz." });
            var p = await _context.AuditPeriods.FindAsync(id);
            if (p == null) return Json(new { success = false, message = "Dönem bulunamadı." });
            if (p.IsLocked)  return Json(new { success = false, message = "Dönem zaten kilitli." });

            p.IsLocked = true;
            p.LockedAt = DateTime.Now;
            p.LockedBy = HttpContext.Session.GetString("Username") ?? "ADMIN";

            // SaveChangesAsync kilit kontrolünü admin için atlar — güvenli
            await _context.SaveChangesAsync();
            return Json(new {
                success  = true,
                message  = $"{p.Year} dönemi kilitlendi.",
                lockedAt = p.LockedAt?.ToString("dd.MM.yyyy HH:mm"),
                lockedBy = p.LockedBy
            });
        }

        // POST /AuditPeriod/UnlockPeriod
        [HttpPost]
        public async Task<IActionResult> UnlockPeriod(int id)
        {
            if (!IsAdminUser) return Json(new { success = false, message = "Yetkisiz." });
            var p = await _context.AuditPeriods.FindAsync(id);
            if (p == null) return Json(new { success = false, message = "Dönem bulunamadı." });
            if (!p.IsLocked) return Json(new { success = false, message = "Dönem zaten açık." });

            p.IsLocked = false;
            p.LockedAt = null;
            p.LockedBy = null;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"{p.Year} dönemi kilidi açıldı." });
        }

        // GET /AuditPeriod/GetAll — AuditReport sayfasında dropdown için
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var periods = await _context.AuditPeriods
                .OrderByDescending(p => p.Year)
                .Select(p => new {
                    p.Id, p.Year, p.IsActive,
                    startDate = p.StartDate.ToString("yyyy-MM-dd"),
                    endDate   = p.EndDate.ToString("yyyy-MM-dd"),
                    isLocked  = p.IsLocked,
                    label     = $"{p.Year} — {p.StartDate:dd.MM.yyyy} / {p.EndDate:dd.MM.yyyy}{(p.IsLocked ? " 🔒" : "")}{(p.Description != null ? " · " + p.Description : "")}"
                })
                .ToListAsync();

            return Json(periods);
        }
    }
}
