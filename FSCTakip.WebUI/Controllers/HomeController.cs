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
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
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

            // ── Kilitli dönemler ─────────────────────────────────────────────────
            var lockedPeriods = await _context.AuditPeriods
                .Where(p => p.IsLocked)
                .OrderByDescending(p => p.Year)
                .ToListAsync();
            ViewBag.LockedPeriods = lockedPeriods;

            // ── Bu ay hammadde girişi (kg) ────────────────────────────────────────
            var monthStart = new DateTime(today.Year, today.Month, 1);
            ViewBag.ThisMonthInputKg = await _context.FscSerials
                .Include(s => s.Lot)
                .Where(s => s.Lot.ArrivalDate >= monthStart)
                .SumAsync(s => (double?)s.InitialWeight) ?? 0.0;

            ViewBag.ThisMonthProdKg = await _context.ProductionDetails
                .Where(d => d.ProductionDate >= monthStart)
                .SumAsync(d => (double?)d.ConsumedWeight) ?? 0.0;

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

        // Hata sayfaları giriş/lisans olmadan da açılabilmeli (hata her an oluşabilir).
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public IActionResult Error() => View();

        // HTTP durum sayfaları (404/403 vb.) — Program.cs UseStatusCodePagesWithReExecute buraya yönlendirir.
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public IActionResult HttpError(int code)
        {
            ViewData["StatusCode"] = code;
            return View("HttpError");
        }

        // ── Bildirim çanı (topbar — tüm sayfalarda) ──────────────────────────
        public async Task<IActionResult> GetNotifications()
        {
            if (CurrentUserId == 0) return Json(Array.Empty<object>());

            var today  = DateTime.Today;
            var warn60 = today.AddDays(60);
            var notifs = new List<object>();

            // FSC süresi dolan/dolacak tedarikçiler
            var expSuppliers = await _context.Suppliers
                .Where(s => s.IsActive && s.FscExpiryDate.HasValue && s.FscExpiryDate <= warn60)
                .OrderBy(s => s.FscExpiryDate)
                .Take(10)
                .ToListAsync();
            foreach (var s in expSuppliers)
            {
                var expired = s.FscExpiryDate!.Value.Date < today;
                notifs.Add(new
                {
                    urgent  = expired,
                    title   = $"Tedarikçi FSC {(expired ? "süresi doldu" : "yakında dolacak")}",
                    message = $"{s.Name} — {s.FscExpiryDate:dd.MM.yyyy}"
                });
            }

            // FSC süresi dolan/dolacak müşteriler
            var expCustomers = await _context.Customers
                .Where(c => c.IsActive && c.IsFscActive && c.FscExpiryDate.HasValue && c.FscExpiryDate <= warn60)
                .OrderBy(c => c.FscExpiryDate)
                .Take(10)
                .ToListAsync();
            foreach (var c in expCustomers)
            {
                var expired = c.FscExpiryDate!.Value.Date < today;
                notifs.Add(new
                {
                    urgent  = expired,
                    title   = $"Müşteri FSC {(expired ? "süresi doldu" : "yakında dolacak")}",
                    message = $"{c.Name} — {c.FscExpiryDate:dd.MM.yyyy}"
                });
            }

            // Düşük stok bobinleri
            var lowStock = await _context.FscSerials
                .Where(s => s.CurrentWeight > 0 && s.CurrentWeight < 500)
                .CountAsync();
            if (lowStock > 0)
                notifs.Add(new { urgent = false, title = "Düşük stok uyarısı", message = $"{lowStock} bobin 500 kg altında" });

            return Json(notifs);
        }

        // ── Global arama (topbar — tüm sayfalarda) ───────────────────────────
        public async Task<IActionResult> GlobalSearch(string q)
        {
            if (CurrentUserId == 0 || string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
                return Json(Array.Empty<object>());

            q = q.Trim();
            var results = new List<object>();

            var customers = await _context.Customers
                .Where(c => c.Name.Contains(q) || c.CustomerCode.Contains(q))
                .OrderBy(c => c.Name).Take(5).ToListAsync();
            foreach (var c in customers)
                results.Add(new { type = "Müşteri", title = c.Name, sub = c.CustomerCode, url = "/Customers/Index" });

            var suppliers = await _context.Suppliers
                .Where(s => s.Name.Contains(q) || s.SupplierCode.Contains(q))
                .OrderBy(s => s.Name).Take(5).ToListAsync();
            foreach (var s in suppliers)
                results.Add(new { type = "Tedarikçi", title = s.Name, sub = s.SupplierCode, url = "/Suppliers/Index" });

            var products = await _context.Products
                .Where(p => p.ProductName.Contains(q) || p.ProductCode.Contains(q))
                .OrderBy(p => p.ProductName).Take(5).ToListAsync();
            foreach (var p in products)
                results.Add(new { type = "Ürün", title = p.ProductName, sub = p.ProductCode, url = "/Products/Index" });

            var lots = await _context.FscLots
                .Where(l => l.PartiNo.Contains(q))
                .OrderByDescending(l => l.ArrivalDate).Take(5).ToListAsync();
            foreach (var l in lots)
                results.Add(new { type = "Lot", title = l.PartiNo, sub = l.ArrivalDate.ToString("dd.MM.yyyy"), url = $"/Purchase/Detail/{l.Id}" });

            return Json(results);
        }
    }
}
