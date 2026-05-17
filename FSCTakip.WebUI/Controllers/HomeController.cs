using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(AppDbContext context) : base(context) { }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var warn60 = today.AddDays(60);

            // ── Özet sayılar ─────────────────────────────────────────────────────
            ViewBag.ActiveCustomers  = await _context.Customers.CountAsync(c => c.IsActive);
            ViewBag.ActiveSuppliers  = await _context.Suppliers.CountAsync(s => s.IsActive);
            ViewBag.ActiveProducts   = await _context.Products.CountAsync(p => p.IsActive);
            ViewBag.TotalLots        = await _context.FscLots.CountAsync();

            ViewBag.PendingWorkOrders    = await _context.WorkOrders.CountAsync(w => w.Status == WorkOrderStatus.Uretimde);
            ViewBag.CompletedWorkOrders  = await _context.WorkOrders.CountAsync(w => w.Status == WorkOrderStatus.Tamamlandi);

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

            // ── Stok özeti (en çok/en az stok) ───────────────────────────────────
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

            return View();
        }

        public IActionResult Error() => View();
    }
}
