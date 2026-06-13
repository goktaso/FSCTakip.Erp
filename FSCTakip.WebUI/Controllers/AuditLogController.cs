using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Controllers
{
    /// <summary>Değişiklik günlüğü — yalnızca admin.</summary>
    public class AuditLogController : BaseController
    {
        public AuditLogController(AppDbContext context) : base(context) { }

        private IActionResult? AdminOnly()
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            if (!IsAdminUser) return View("~/Views/Shared/AccessDenied.cshtml");
            return null;
        }

        // GET /AuditLog
        public async Task<IActionResult> Index(
            string? table, string? op, string? user,
            DateTime? dateFrom, DateTime? dateTo,
            int page = 1, int pageSize = 50)
        {
            var guard = AdminOnly(); if (guard != null) return guard;

            // "action" ASP.NET Core'un rezerve route parametresi olduğu için model binding
            // çakışabilir — query string'den doğrudan oku, op parametresine fallback yap
            if (string.IsNullOrWhiteSpace(op))
            {
                var actionQs = HttpContext.Request.Query["action"].ToString();
                if (!string.IsNullOrWhiteSpace(actionQs))
                    op = actionQs;
            }

            // Varsayılan tarih aralığı: filtre yoksa son 30 gün
            if (!dateFrom.HasValue && !dateTo.HasValue && string.IsNullOrWhiteSpace(table) &&
                string.IsNullOrWhiteSpace(op) && string.IsNullOrWhiteSpace(user))
            {
                dateFrom = DateTime.Today.AddDays(-30);
                dateTo   = DateTime.Today;
            }

            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(table))
                query = query.Where(a => a.TableName.Contains(table));
            if (!string.IsNullOrWhiteSpace(op))
                query = query.Where(a => a.Action == op);
            if (!string.IsNullOrWhiteSpace(user))
                query = query.Where(a => a.ChangedBy != null && a.ChangedBy.Contains(user));
            if (dateFrom.HasValue)
                query = query.Where(a => a.ChangedAt >= dateFrom.Value);
            if (dateTo.HasValue)
                query = query.Where(a => a.ChangedAt < dateTo.Value.AddDays(1));

            var total  = await query.CountAsync();
            var logs   = await query
                .OrderByDescending(a => a.ChangedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Filtrelerde kullanmak için benzersiz tablo adları ve kullanıcılar
            ViewBag.TableNames = await _context.AuditLogs
                .Select(a => a.TableName).Distinct().OrderBy(t => t).ToListAsync();
            ViewBag.Users = await _context.AuditLogs
                .Where(a => a.ChangedBy != null)
                .Select(a => a.ChangedBy!).Distinct().OrderBy(u => u).ToListAsync();

            ViewBag.Total    = total;
            ViewBag.Page     = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Pages    = (int)Math.Ceiling((double)total / pageSize);

            // Filtre değerleri (sayfada göstermek için)
            ViewBag.FilterTable    = table;
            ViewBag.FilterAction   = op;
            ViewBag.FilterUser     = user;
            ViewBag.FilterDateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.FilterDateTo   = dateTo?.ToString("yyyy-MM-dd");

            ViewData["Title"] = "Değişiklik Günlüğü";
            return View(logs);
        }

        // GET /AuditLog/Detail/{id}
        public async Task<IActionResult> Detail(long id)
        {
            var guard = AdminOnly(); if (guard != null) return guard;

            var log = await _context.AuditLogs.FindAsync(id);
            if (log == null) return NotFound();

            ViewData["Title"] = $"Günlük Detayı #{id}";
            return View(log);
        }

        // GET /AuditLog/GetStats — son 30 günün istatistiği (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            if (!IsAdminUser) return Json(new { });

            var since = DateTime.Today.AddDays(-30);
            var stats = await _context.AuditLogs
                .Where(a => a.ChangedAt >= since)
                .GroupBy(a => a.Action)
                .Select(g => new { action = g.Key, count = g.Count() })
                .ToListAsync();

            var daily = await _context.AuditLogs
                .Where(a => a.ChangedAt >= since)
                .GroupBy(a => a.ChangedAt.Date)
                .Select(g => new { date = g.Key, count = g.Count() })
                .OrderBy(g => g.date)
                .ToListAsync();

            return Json(new { stats, daily });
        }
    }
}
