using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Controllers
{
    /// <summary>
    /// Üretim planlama takvimi — iş emirlerini takvim görünümünde gösterir,
    /// planlı tarih ve makine ataması yapılabilir.
    /// </summary>
    public class PlanningController : BaseController
    {
        public PlanningController(AppDbContext context) : base(context) { }

        // GET /Planning — takvim görünümü
        public async Task<IActionResult> Index(int? year, int? month)
        {
            var today = DateTime.Today;
            int y = year  ?? today.Year;
            int m = month ?? today.Month;

            // Gezinme için önceki/sonraki ay
            var currentMonth = new DateTime(y, m, 1);
            ViewBag.CurrentMonth = currentMonth;
            ViewBag.PrevMonth    = currentMonth.AddMonths(-1);
            ViewBag.NextMonth    = currentMonth.AddMonths(1);

            var monthEnd = currentMonth.AddMonths(1);

            // Bu ay planlanan ve bu ay başlayan iş emirleri
            var workOrders = await _context.WorkOrders
                .Include(w => w.Product)
                .Include(w => w.Machine)
                .Where(w => w.PlannedDate >= currentMonth && w.PlannedDate < monthEnd)
                .OrderBy(w => w.PlannedDate)
                .ToListAsync();

            ViewBag.WorkOrders = workOrders;
            ViewBag.Machines   = await _context.Machines.Where(m2 => m2.IsActive).OrderBy(m2 => m2.Name).ToListAsync();

            // Makine doluluk özeti
            var machineLoad = workOrders
                .Where(w => w.MachineId > 0 && w.Machine != null)
                .GroupBy(w => w.Machine!.Name)
                .Select(g => new
                {
                    Machine = g.Key,
                    Count   = g.Count(),
                    Planned = g.Count(w => w.Status == WorkOrderStatus.Taslak),
                    Active  = g.Count(w => w.Status == WorkOrderStatus.Uretimde),
                    Done    = g.Count(w => w.Status == WorkOrderStatus.Tamamlandi)
                })
                .ToList();

            ViewBag.MachineLoad = machineLoad;

            // Gecikmiş iş emirleri (planlı tarih < bugün, henüz tamamlanmamış)
            var overdue = await _context.WorkOrders
                .Include(w => w.Product)
                .Include(w => w.Machine)
                .Where(w => w.PlannedDate < today
                         && w.Status != WorkOrderStatus.Tamamlandi
                         && w.Status != WorkOrderStatus.Iptal)
                .OrderBy(w => w.PlannedDate)
                .Take(10)
                .ToListAsync();

            ViewBag.OverdueOrders = overdue;

            ViewData["Title"] = $"Üretim Planı — {currentMonth.ToString("MMMM yyyy", new System.Globalization.CultureInfo("tr-TR"))}";
            return View();
        }

        // GET /Planning/GetCalendarData — FullCalendar JSON feed
        [HttpGet]
        public async Task<IActionResult> GetCalendarData(string start, string end)
        {
            if (!DateTime.TryParse(start, out var sd)) sd = DateTime.Today.AddDays(-30);
            if (!DateTime.TryParse(end,   out var ed)) ed = DateTime.Today.AddDays(60);

            var workOrders = await _context.WorkOrders
                .Include(w => w.Product)
                .Include(w => w.Machine)
                .Where(w => w.PlannedDate >= sd && w.PlannedDate < ed)
                .ToListAsync();

            var events = workOrders.Select(w => new
            {
                id         = w.Id,
                title      = $"{w.WorkOrderNo} — {w.Product?.ProductName ?? "?"} ({w.PlannedQuantity:N0} ad)",
                start      = w.PlannedDate.ToString("yyyy-MM-dd"),
                color      = w.Status switch
                {
                    WorkOrderStatus.Tamamlandi => "#16a34a",
                    WorkOrderStatus.Uretimde   => "#2563eb",
                    WorkOrderStatus.Iptal      => "#6b7280",
                    _                          => w.PlannedDate < DateTime.Today ? "#dc2626" : "#d97706"
                },
                extendedProps = new
                {
                    status    = w.Status.ToString(),
                    machine   = w.Machine != null ? w.Machine.Name : "—",
                    quantity  = w.PlannedQuantity,
                    workOrderNo = w.WorkOrderNo
                }
            }).ToList();

            return Json(events);
        }

        // POST /Planning/UpdatePlannedDate — sürükle-bırak tarih güncellemesi
        [HttpPost]
        public async Task<IActionResult> UpdatePlannedDate(int id, DateTime plannedDate)
        {
            var deny = RequireWrite("PLANNING");
            if (deny != null) return deny;

            var wo = await _context.WorkOrders.FindAsync(id);
            if (wo == null) return Json(new { success = false, message = "İş emri bulunamadı." });
            if (wo.Status == WorkOrderStatus.Tamamlandi)
                return Json(new { success = false, message = "Tamamlanmış iş emrinin tarihi değiştirilemez." });

            wo.PlannedDate = plannedDate;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Planlı tarih {plannedDate:dd.MM.yyyy} olarak güncellendi." });
        }

        // POST /Planning/AssignMachine
        [HttpPost]
        public async Task<IActionResult> AssignMachine(int workOrderId, int machineId)
        {
            var deny = RequireWrite("PLANNING");
            if (deny != null) return deny;

            var wo = await _context.WorkOrders.FindAsync(workOrderId);
            if (wo == null) return Json(new { success = false, message = "İş emri bulunamadı." });

            var machine = await _context.Machines.FindAsync(machineId);
            if (machine == null) return Json(new { success = false, message = "Makine bulunamadı." });

            wo.MachineId = machineId;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Makine '{machine.Name}' atandı." });
        }

        // GET /Planning/GetWorkOrderDetail/{id}
        [HttpGet]
        public async Task<IActionResult> GetWorkOrderDetail(int id)
        {
            var wo = await _context.WorkOrders
                .Include(w => w.Product).ThenInclude(p => p!.FscType)
                .Include(w => w.Machine)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (wo == null) return Json(new { success = false });

            return Json(new
            {
                success        = true,
                id             = wo.Id,
                workOrderNo    = wo.WorkOrderNo,
                productName    = wo.Product?.ProductName ?? "—",
                productCode    = wo.Product?.ProductCode ?? "—",
                fscType        = wo.Product?.FscType?.Name ?? "—",
                plannedQty     = wo.PlannedQuantity,
                actualQty      = wo.ActualQuantity,
                machineName    = wo.Machine != null ? wo.Machine.Name : "—",
                machineId      = wo.MachineId,
                plannedDate    = wo.PlannedDate.ToString("dd.MM.yyyy"),
                status         = wo.Status.ToString(),
                notes          = wo.Notes
            });
        }
    }
}
