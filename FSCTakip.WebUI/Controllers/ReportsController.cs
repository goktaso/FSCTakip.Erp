using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Controllers
{
    public class ReportsController : BaseController
    {
        public ReportsController(AppDbContext context) : base(context) { }

        // ── 1. FSC Chain of Custody Raporu ─────────────────────────────────────
        // Her satış sevkiyatını tedarikçiye kadar izler.
        // GET /Reports/ChainOfCustody
        public async Task<IActionResult> ChainOfCustody(
            DateTime? startDate, DateTime? endDate,
            int? customerId, int? productId)
        {
            var query = _context.SalesOrderLines
                .Include(l => l.SalesOrder).ThenInclude(o => o.Customer)
                .Include(l => l.Product)
                .Include(l => l.WorkOrder)
                    .ThenInclude(w => w!.ProductionDetails)
                        .ThenInclude(pd => pd.FscSerial)
                            .ThenInclude(s => s.Lot)
                                .ThenInclude(lot => lot.Supplier)
                .Include(l => l.WorkOrder)
                    .ThenInclude(w => w!.ProductionDetails)
                        .ThenInclude(pd => pd.FscSerial)
                            .ThenInclude(s => s.Lot)
                                .ThenInclude(lot => lot.FscType)
                .Where(l => l.SalesOrder.Status == SalesOrderStatus.TeslimEdildi)
                .AsQueryable();

            if (startDate.HasValue) query = query.Where(l => l.SalesOrder.DispatchDate >= startDate.Value);
            if (endDate.HasValue)   query = query.Where(l => l.SalesOrder.DispatchDate <= endDate.Value.AddDays(1));
            if (customerId.HasValue) query = query.Where(l => l.SalesOrder.CustomerId == customerId.Value);
            if (productId.HasValue)  query = query.Where(l => l.ProductId == productId.Value);

            var lines = await query.OrderByDescending(l => l.SalesOrder.DispatchDate).ToListAsync();

            var rows = lines.Select(l => BuildCocRow(l)).ToList();

            ViewBag.Customers  = await _context.Customers.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
            ViewBag.Products   = await _context.Products.Where(p => p.IsActive).OrderBy(p => p.ProductName).ToListAsync();
            ViewBag.StartDate  = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate    = endDate?.ToString("yyyy-MM-dd");

            ViewBag.TotalLines    = rows.Count;
            ViewBag.FullChain     = rows.Count(r => r.ChainComplete);
            ViewBag.MissingChain  = rows.Count(r => !r.ChainComplete);

            return View(rows);
        }

        private static CocRow BuildCocRow(SalesOrderLine l)
        {
            var row = new CocRow
            {
                SalesOrderNo  = l.SalesOrder.SalesOrderNo,
                DispatchDate  = l.SalesOrder.DispatchDate,
                CustomerName  = l.SalesOrder.Customer?.Name ?? "—",
                CustomerFscLicense = l.SalesOrder.Customer?.FscLicenseCode,
                ProductName   = l.Product?.ProductName ?? "—",
                Quantity      = l.Quantity,
                Unit          = l.Unit,
                WorkOrderNo   = l.WorkOrder?.WorkOrderNo,
                SalesOrderLineId = l.Id
            };

            if (l.WorkOrder?.ProductionDetails != null && l.WorkOrder.ProductionDetails.Any())
            {
                var details = l.WorkOrder.ProductionDetails.ToList();
                var serials = details.Select(d => d.FscSerial).Where(s => s != null).ToList();
                var lots    = serials.Select(s => s!.Lot).Where(lot => lot != null).DistinctBy(lot => lot!.Id).ToList();

                row.Serials = serials.Select(s => s!.SerialNo).Distinct().ToList();
                row.LotNos  = lots.Select(lot => lot!.LotNo).ToList();
                row.SupplierNames = lots.Select(lot => lot!.Supplier?.Name ?? "").Distinct().ToList();
                row.SupplierFscCodes = lots.Select(lot => lot!.Supplier?.FscCode ?? "").Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();
                row.FscTypes = lots.Select(lot => lot!.FscType?.Name ?? "").Distinct().ToList();

                row.ChainComplete = lots.Any() && lots.All(lot =>
                    lot!.Supplier != null &&
                    !string.IsNullOrEmpty(lot.Supplier.FscCode) &&
                    (lot.Supplier.FscExpiryDate == null || lot.Supplier.FscExpiryDate >= DateTime.Today));
            }
            else
            {
                row.Serials = new();
                row.LotNos  = new();
                row.SupplierNames = new();
                row.SupplierFscCodes = new();
                row.FscTypes = new();
                row.ChainComplete = false;
            }

            return row;
        }

        // GET /Reports/ExportCoc
        public async Task<IActionResult> ExportCoc(
            DateTime? startDate, DateTime? endDate, int? customerId, int? productId)
        {
            var query = _context.SalesOrderLines
                .Include(l => l.SalesOrder).ThenInclude(o => o.Customer)
                .Include(l => l.Product)
                .Include(l => l.WorkOrder)
                    .ThenInclude(w => w!.ProductionDetails)
                        .ThenInclude(pd => pd.FscSerial)
                            .ThenInclude(s => s.Lot)
                                .ThenInclude(lot => lot.Supplier)
                .Include(l => l.WorkOrder)
                    .ThenInclude(w => w!.ProductionDetails)
                        .ThenInclude(pd => pd.FscSerial)
                            .ThenInclude(s => s.Lot)
                                .ThenInclude(lot => lot.FscType)
                .Where(l => l.SalesOrder.Status == SalesOrderStatus.TeslimEdildi);

            if (startDate.HasValue) query = query.Where(l => l.SalesOrder.DispatchDate >= startDate.Value);
            if (endDate.HasValue)   query = query.Where(l => l.SalesOrder.DispatchDate <= endDate.Value.AddDays(1));
            if (customerId.HasValue) query = query.Where(l => l.SalesOrder.CustomerId == customerId.Value);
            if (productId.HasValue)  query = query.Where(l => l.ProductId == productId.Value);

            var lines = await query.OrderByDescending(l => l.SalesOrder.DispatchDate).ToListAsync();
            var rows = lines.Select(l => BuildCocRow(l)).Select(r => new
            {
                SevkTarihi    = r.DispatchDate?.ToString("dd.MM.yyyy") ?? "",
                SiparisNo     = r.SalesOrderNo,
                Musteri       = r.CustomerName,
                MusteriFSC    = r.CustomerFscLicense ?? "",
                Urun          = r.ProductName,
                Miktar        = r.Quantity,
                Birim         = r.Unit,
                IsEmri        = r.WorkOrderNo ?? "",
                Lotlar        = string.Join(", ", r.LotNos),
                Seriler       = string.Join(", ", r.Serials),
                Tedarikciler  = string.Join(", ", r.SupplierNames),
                TedarikFSC    = string.Join(", ", r.SupplierFscCodes),
                FSCTipleri    = string.Join(", ", r.FscTypes),
                ZincirDurumu  = r.ChainComplete ? "Tam" : "Eksik"
            }).ToList();

            return ExportToExcel(rows, "FSC_CoCRaporu");
        }

        // ── 2. Lot Takip Raporu ─────────────────────────────────────────────────
        // GET /Reports/LotTrace
        public async Task<IActionResult> LotTrace(int? lotId)
        {
            var lots = await _context.FscLots
                .Include(l => l.Supplier)
                .Include(l => l.FscType)
                .Include(l => l.Product)
                .OrderByDescending(l => l.ArrivalDate)
                .ToListAsync();

            ViewBag.Lots = lots;

            if (!lotId.HasValue)
                return View(null as LotTraceResult);

            var lot = lots.FirstOrDefault(l => l.Id == lotId.Value);
            if (lot == null) return View(null as LotTraceResult);

            var serials = await _context.FscSerials
                .Include(s => s.ProductionDetails)
                    .ThenInclude(pd => pd.WorkOrder)
                        .ThenInclude(w => w.Product)
                .Where(s => s.LotId == lotId.Value)
                .ToListAsync();

            var workOrderIds = serials
                .SelectMany(s => s.ProductionDetails)
                .Select(pd => pd.WorkOrderId)
                .Distinct()
                .ToList();

            var salesLines = await _context.SalesOrderLines
                .Include(l => l.SalesOrder).ThenInclude(o => o.Customer)
                .Include(l => l.Product)
                .Where(l => l.WorkOrderId.HasValue && workOrderIds.Contains(l.WorkOrderId!.Value))
                .ToListAsync();

            var result = new LotTraceResult
            {
                Lot       = lot,
                Serials   = serials,
                SalesLines = salesLines
            };

            return View(result);
        }

        // ── 3. Tedarikçi FSC Sertifika Durumu ──────────────────────────────────
        // GET /Reports/SupplierFsc
        public async Task<IActionResult> SupplierFsc()
        {
            var suppliers = await _context.Suppliers
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            var today = DateTime.Today;
            ViewBag.Valid    = suppliers.Count(s => s.IsFscActive && (s.FscExpiryDate == null || s.FscExpiryDate > today.AddDays(30)));
            ViewBag.Expiring = suppliers.Count(s => s.IsFscActive && s.FscExpiryDate.HasValue && s.FscExpiryDate <= today.AddDays(30) && s.FscExpiryDate > today);
            ViewBag.Expired  = suppliers.Count(s => !s.IsFscActive || (s.FscExpiryDate.HasValue && s.FscExpiryDate <= today));
            ViewBag.Today    = today;

            return View(suppliers);
        }
    }

    // ── View Model Sınıfları ──────────────────────────────────────────────────

    public class CocRow
    {
        public string SalesOrderNo     { get; set; } = "";
        public DateTime? DispatchDate  { get; set; }
        public string CustomerName     { get; set; } = "";
        public string? CustomerFscLicense { get; set; }
        public string ProductName      { get; set; } = "";
        public decimal Quantity        { get; set; }
        public string Unit             { get; set; } = "";
        public string? WorkOrderNo     { get; set; }
        public int SalesOrderLineId    { get; set; }
        public List<string> Serials    { get; set; } = new();
        public List<string> LotNos     { get; set; } = new();
        public List<string> SupplierNames { get; set; } = new();
        public List<string> SupplierFscCodes { get; set; } = new();
        public List<string> FscTypes   { get; set; } = new();
        public bool ChainComplete      { get; set; }
    }

    public class LotTraceResult
    {
        public FscLot Lot                         { get; set; } = null!;
        public List<FscSerial> Serials            { get; set; } = new();
        public List<SalesOrderLine> SalesLines    { get; set; } = new();
    }
}
