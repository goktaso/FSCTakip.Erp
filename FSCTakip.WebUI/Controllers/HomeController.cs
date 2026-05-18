using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FSCTakip.WebUI.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(AppDbContext context) : base(context) { }

        public async Task<IActionResult> Index()
        {
            var today    = DateTime.Today;
            var warn60   = today.AddDays(60);
            var culture  = new System.Globalization.CultureInfo("tr-TR");

            // ── Özet sayılar ─────────────────────────────────────────────────────
            ViewBag.ActiveCustomers  = await _context.Customers.CountAsync(c => c.IsActive);
            ViewBag.ActiveSuppliers  = await _context.Suppliers.CountAsync(s => s.IsActive);
            ViewBag.ActiveProducts   = await _context.Products.CountAsync(p => p.IsActive);
            ViewBag.TotalLots        = await _context.FscLots.CountAsync();

            ViewBag.PendingWorkOrders   = await _context.WorkOrders.CountAsync(w => w.Status == WorkOrderStatus.Uretimde);
            ViewBag.CompletedWorkOrders = await _context.WorkOrders.CountAsync(w => w.Status == WorkOrderStatus.Tamamlandi);

            ViewBag.DraftSales     = await _context.SalesOrders.CountAsync(s => s.Status == SalesOrderStatus.Taslak);
            ViewBag.DeliveredSales = await _context.SalesOrders.CountAsync(s => s.Status == SalesOrderStatus.TeslimEdildi);

            // ── FSC Uyarılar ──────────────────────────────────────────────────────
            var expSuppliers = await _context.Suppliers
                .Where(s => s.IsActive && s.FscExpiryDate.HasValue && s.FscExpiryDate <= warn60)
                .OrderBy(s => s.FscExpiryDate)
                .ToListAsync();

            var expCustomers = await _context.Customers
                .Where(c => c.IsActive && c.IsFscActive && c.FscExpiryDate.HasValue && c.FscExpiryDate <= warn60)
                .OrderBy(c => c.FscExpiryDate)
                .ToListAsync();

            ViewBag.ExpiringSuppliers = expSuppliers;
            ViewBag.ExpiringCustomers = expCustomers;
            ViewBag.Today = today;

            // ── Son 8 stok hareketi ───────────────────────────────────────────────
            var recentMovements = await _context.StockMovements
                .Include(m => m.Product)
                .Include(m => m.Customer)
                .OrderByDescending(m => m.DocumentDate).ThenByDescending(m => m.Id)
                .Take(8)
                .ToListAsync();

            ViewBag.RecentMovements = recentMovements;

            // ── Stok özeti ───────────────────────────────────────────────────────
            var movements = await _context.StockMovements.ToListAsync();
            var stockSummary = movements
                .GroupBy(m => m.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Net = g.Where(m => m.Type == MovementType.ProductionEntry || m.Type == MovementType.PurchaseEntry).Sum(m => m.Quantity)
                        - g.Where(m => m.Type == MovementType.SalesDispatch).Sum(m => m.Quantity)
                })
                .OrderByDescending(x => x.Net)
                .ToList();

            ViewBag.StockItems = stockSummary.Count;
            ViewBag.InStock    = stockSummary.Count(x => x.Net > 0);

            // ── Son 6 Ay Trend Verisi ─────────────────────────────────────────────
            var sixMonthsAgo = new DateTime(today.Year, today.Month, 1).AddMonths(-5);

            var lotTrendRaw = await _context.FscSerials
                .Include(s => s.Lot)
                .Where(s => s.Lot.ArrivalDate >= sixMonthsAgo)
                .GroupBy(s => new { s.Lot.ArrivalDate.Year, s.Lot.ArrivalDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Kg = g.Sum(s => s.InitialWeight) })
                .ToListAsync();

            var prodTrendRaw = await _context.ProductionDetails
                .Where(d => d.ProductionDate >= sixMonthsAgo)
                .GroupBy(d => new { d.ProductionDate.Year, d.ProductionDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Kg = g.Sum(d => d.ConsumedWeight) })
                .ToListAsync();

            var salesTrendRaw = await _context.SalesOrderLines
                .Include(l => l.SalesOrder)
                .Where(l => l.SalesOrder.Status == SalesOrderStatus.TeslimEdildi
                         && l.SalesOrder.DispatchDate >= sixMonthsAgo)
                .GroupBy(l => new { l.SalesOrder.DispatchDate!.Value.Year, l.SalesOrder.DispatchDate!.Value.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Qty = g.Sum(l => l.Quantity) })
                .ToListAsync();

            var months = Enumerable.Range(0, 6).Select(i => sixMonthsAgo.AddMonths(i)).ToList();
            ViewBag.ChartMonths   = JsonSerializer.Serialize(months.Select(m => m.ToString("MMM yy", culture)).ToList());
            ViewBag.ChartLotKg    = JsonSerializer.Serialize(months.Select(m => (double)(lotTrendRaw.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month)?.Kg ?? 0)).ToList());
            ViewBag.ChartProdKg   = JsonSerializer.Serialize(months.Select(m => (double)(prodTrendRaw.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month)?.Kg ?? 0)).ToList());
            ViewBag.ChartSalesQty = JsonSerializer.Serialize(months.Select(m => (double)(salesTrendRaw.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month)?.Qty ?? 0)).ToList());

            // ── Düşük stok bobinleri (CurrentWeight < 500 kg) ─────────────────────
            var lowStockSerials = await _context.FscSerials
                .Include(s => s.Lot).ThenInclude(l => l.Supplier)
                .Include(s => s.Lot).ThenInclude(l => l.Product)
                .Where(s => s.CurrentWeight > 0 && s.CurrentWeight < 500)
                .OrderBy(s => s.CurrentWeight)
                .Take(6)
                .ToListAsync();

            ViewBag.LowStockSerials = lowStockSerials;

            // ── Aktif iş emirleri ────────────────────────────────────────────────
            var activeWorkOrders = await _context.WorkOrders
                .Include(w => w.Product)
                .Include(w => w.Machine)
                .Where(w => w.Status == WorkOrderStatus.Uretimde)
                .OrderBy(w => w.PlannedDate)
                .Take(5)
                .ToListAsync();

            ViewBag.ActiveWorkOrders = activeWorkOrders;

            return View();
        }

        public IActionResult Error() => View();
    }
}
