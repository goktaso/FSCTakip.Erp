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

        // ── 4. Denetim Özet Raporu ─────────────────────────────────────────────
        // GET /Reports/AuditReport
        public async Task<IActionResult> AuditReport(DateTime? startDate, DateTime? endDate)
        {
            var sd = startDate ?? new DateTime(DateTime.Today.Year, 1, 1);
            var ed = endDate   ?? DateTime.Today;
            var edNext = ed.AddDays(1);

            // ── A: Hammadde Girişleri ────────────────────────────────────────
            var lots = await _context.FscLots
                .Include(l => l.FscType)
                .Include(l => l.Supplier)
                .Include(l => l.Product)
                .Include(l => l.Serials)
                .Where(l => l.ArrivalDate >= sd && l.ArrivalDate < edNext)
                .OrderBy(l => l.FscType.Name).ThenBy(l => l.Supplier.Name).ThenBy(l => l.ArrivalDate)
                .ToListAsync();

            var inputRows = lots.GroupBy(l => new { FscType = l.FscType?.Name ?? "—", FscCode = l.FscType?.Code ?? "—", Supplier = l.Supplier?.Name ?? "—", SupplierFsc = l.Supplier?.FscCode ?? "" })
                .Select(g => new AuditInputRow {
                    FscType         = g.Key.FscType,
                    FscCode         = g.Key.FscCode,
                    Supplier        = g.Key.Supplier,
                    SupplierFscCode = g.Key.SupplierFsc,
                    LotCount        = g.Count(),
                    SerialCount     = g.Sum(l => l.Serials.Count),
                    TotalWeightKg   = g.Sum(l => l.Serials.Sum(s => s.InitialWeight)),
                    InvoiceNos      = string.Join(", ", g.Select(l => l.InvoiceNo).Where(n => !string.IsNullOrEmpty(n)).Distinct()),
                    DispatchNos     = string.Join(", ", g.Select(l => l.DispatchNo).Where(n => !string.IsNullOrEmpty(n)).Distinct()),
                    LotNos          = string.Join(", ", g.Select(l => l.LotNo).Distinct())
                }).ToList();

            // ── B: Üretim Tüketimi ───────────────────────────────────────────
            var prodDetails = await _context.ProductionDetails
                .Include(d => d.WorkOrder).ThenInclude(w => w.Product)
                .Include(d => d.FscSerial).ThenInclude(s => s.Lot).ThenInclude(l => l.FscType)
                .Include(d => d.FscSerial).ThenInclude(s => s.Lot).ThenInclude(l => l.Supplier)
                .Include(d => d.Machine)
                .Where(d => d.ProductionDate >= sd && d.ProductionDate < edNext)
                .OrderBy(d => d.ProductionDate)
                .ToListAsync();

            var productionRows = prodDetails.GroupBy(d => new { WoNo = d.WorkOrder.WorkOrderNo, ProductName = d.WorkOrder.Product?.ProductName ?? "—", d.ProductionDate })
                .Select(g => new AuditProductionRow {
                    WorkOrderNo   = g.Key.WoNo,
                    ProductName   = g.Key.ProductName,
                    ProductionDate= g.Key.ProductionDate,
                    LotNos        = string.Join(", ", g.Select(d => d.FscSerial?.Lot?.LotNo).Where(s => s != null).Distinct()),
                    FscTypes      = string.Join(", ", g.Select(d => d.FscSerial?.Lot?.FscType?.Name).Where(s => s != null).Distinct()),
                    Suppliers     = string.Join(", ", g.Select(d => d.FscSerial?.Lot?.Supplier?.Name).Where(s => s != null).Distinct()),
                    ConsumedKg    = g.Sum(d => d.ConsumedWeight),
                    WasteKg       = g.Sum(d => d.WasteWeight),
                    ProducedQty   = g.Max(d => d.ProducedQuantity)
                }).ToList();

            // ── C: Satış Sevkiyatları ────────────────────────────────────────
            var salesLines = await _context.SalesOrderLines
                .Include(l => l.SalesOrder).ThenInclude(o => o.Customer)
                .Include(l => l.Product)
                .Include(l => l.WorkOrder)
                .Where(l => l.SalesOrder.Status == SalesOrderStatus.TeslimEdildi
                         && l.SalesOrder.DispatchDate >= sd
                         && l.SalesOrder.DispatchDate < edNext)
                .OrderBy(l => l.SalesOrder.DispatchDate)
                .ToListAsync();

            var salesRows = salesLines.Select(l => new AuditSalesRow {
                SalesOrderNo      = l.SalesOrder.SalesOrderNo,
                DispatchDate      = l.SalesOrder.DispatchDate ?? l.SalesOrder.OrderDate,
                Customer          = l.SalesOrder.Customer?.Name ?? "—",
                CustomerFscLicense= l.SalesOrder.Customer?.FscLicenseCode,
                CustomerIsFsc     = l.SalesOrder.Customer?.IsFscActive == true,
                ProductName       = l.Product?.ProductName ?? "—",
                Quantity          = l.Quantity,
                Unit              = l.Unit,
                UnitPrice         = l.UnitPrice,
                WorkOrderNo       = l.WorkOrder?.WorkOrderNo,
                CocComplete       = l.WorkOrderId.HasValue
            }).ToList();

            // ── D: FSC Tipi Denge Özeti ─────────────────────────────────────
            // Sistemdeki tüm seriler (dönemle sınırlı değil — stok pozisyonu için)
            var allSerials = await _context.FscSerials
                .Include(s => s.Lot).ThenInclude(l => l.FscType)
                .ToListAsync();

            // Dönemde tüketilen miktar (üretim detaylarından)
            var consumedByType = prodDetails
                .GroupBy(d => d.FscSerial?.Lot?.FscType?.Name ?? "—")
                .ToDictionary(g => g.Key, g => (Consumed: g.Sum(d => d.ConsumedWeight), Waste: g.Sum(d => d.WasteWeight)));

            var balanceRows = allSerials
                .GroupBy(s => s.Lot?.FscType?.Name ?? "—")
                .Select(g => {
                    var type = g.Key;
                    consumedByType.TryGetValue(type, out var cons);
                    var inputKg    = lots.Where(l => l.FscType?.Name == type).Sum(l => l.Serials.Sum(s => s.InitialWeight));
                    var consumedKg = cons.Consumed;
                    var wasteKg    = cons.Waste;
                    var stockKg    = g.Sum(s => s.CurrentWeight);
                    return new AuditBalanceRow {
                        FscType    = type,
                        InputKg    = inputKg,
                        ConsumedKg = consumedKg,
                        WasteKg    = wasteKg,
                        StockKg    = stockKg,
                        IsBalanced = (consumedKg + wasteKg) <= (inputKg + stockKg) + 0.01m
                    };
                })
                .Where(r => r.InputKg > 0 || r.ConsumedKg > 0 || r.StockKg > 0)
                .OrderBy(r => r.FscType)
                .ToList();

            var model = new AuditReportModel {
                StartDate       = sd,
                EndDate         = ed,
                InputRows       = inputRows,
                ProductionRows  = productionRows,
                SalesRows       = salesRows,
                BalanceRows     = balanceRows
            };

            ViewBag.StartDate = sd.ToString("yyyy-MM-dd");
            ViewBag.EndDate   = ed.ToString("yyyy-MM-dd");
            ViewData["Title"] = "Denetim Özet Raporu";
            return View(model);
        }

        // GET /Reports/ExportAuditReport
        public async Task<IActionResult> ExportAuditReport(DateTime? startDate, DateTime? endDate)
        {
            var sd = startDate ?? new DateTime(DateTime.Today.Year, 1, 1);
            var ed = endDate   ?? DateTime.Today;
            var edNext = ed.AddDays(1);

            // Aynı veri setini çek (DRY için idealde metot çıkarılabilir)
            var lots = await _context.FscLots
                .Include(l => l.FscType).Include(l => l.Supplier).Include(l => l.Serials)
                .Where(l => l.ArrivalDate >= sd && l.ArrivalDate < edNext)
                .ToListAsync();

            var prodDetails = await _context.ProductionDetails
                .Include(d => d.WorkOrder).ThenInclude(w => w.Product)
                .Include(d => d.FscSerial).ThenInclude(s => s.Lot).ThenInclude(l => l.FscType)
                .Include(d => d.FscSerial).ThenInclude(s => s.Lot).ThenInclude(l => l.Supplier)
                .Where(d => d.ProductionDate >= sd && d.ProductionDate < edNext)
                .ToListAsync();

            var salesLines = await _context.SalesOrderLines
                .Include(l => l.SalesOrder).ThenInclude(o => o.Customer)
                .Include(l => l.Product).Include(l => l.WorkOrder)
                .Where(l => l.SalesOrder.Status == SalesOrderStatus.TeslimEdildi
                         && l.SalesOrder.DispatchDate >= sd
                         && l.SalesOrder.DispatchDate < edNext)
                .ToListAsync();

            using var wb = new ClosedXML.Excel.XLWorkbook();
            var headerBg = ClosedXML.Excel.XLColor.FromHtml("#1e3d14");

            // ── Sayfa 1: Hammadde Girişleri ─────────────────────────────────
            var ws1 = wb.AddWorksheet("Hammadde Girişleri");
            string[] h1 = { "FSC Tipi", "Tedarikçi", "Tedarikçi FSC Kodu", "Lot Sayısı", "Bobin Sayısı", "Toplam Giriş (kg)", "Fatura No(ları)", "İrsaliye No(ları)" };
            for (int i = 0; i < h1.Length; i++) {
                ws1.Cell(1, i+1).Value = h1[i];
                ws1.Cell(1, i+1).Style.Font.Bold = true;
                ws1.Cell(1, i+1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                ws1.Cell(1, i+1).Style.Fill.BackgroundColor = headerBg;
            }
            var inputGroups = lots.GroupBy(l => new { FscType = l.FscType?.Name ?? "—", Supplier = l.Supplier?.Name ?? "—", SupplierFsc = l.Supplier?.FscCode ?? "" }).ToList();
            int r1 = 2;
            foreach (var g in inputGroups) {
                ws1.Cell(r1,1).Value = g.Key.FscType;
                ws1.Cell(r1,2).Value = g.Key.Supplier;
                ws1.Cell(r1,3).Value = g.Key.SupplierFsc;
                ws1.Cell(r1,4).Value = g.Count();
                ws1.Cell(r1,5).Value = g.Sum(l => l.Serials.Count);
                ws1.Cell(r1,6).Value = (double)g.Sum(l => l.Serials.Sum(s => s.InitialWeight));
                ws1.Cell(r1,7).Value = string.Join(", ", g.Select(l => l.InvoiceNo).Where(n => !string.IsNullOrEmpty(n)).Distinct());
                ws1.Cell(r1,8).Value = string.Join(", ", g.Select(l => l.DispatchNo).Where(n => !string.IsNullOrEmpty(n)).Distinct());
                r1++;
            }
            ws1.Columns().AdjustToContents();

            // ── Sayfa 2: Üretim Tüketimi ────────────────────────────────────
            var ws2 = wb.AddWorksheet("Üretim Tüketimi");
            string[] h2 = { "Tarih", "İş Emri No", "Mamul", "Kullanılan Lot(lar)", "FSC Tip(leri)", "Tedarikçi(ler)", "Tüketim (kg)", "Fire (kg)", "Üretim (adet)" };
            for (int i = 0; i < h2.Length; i++) {
                ws2.Cell(1, i+1).Value = h2[i];
                ws2.Cell(1, i+1).Style.Font.Bold = true;
                ws2.Cell(1, i+1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                ws2.Cell(1, i+1).Style.Fill.BackgroundColor = headerBg;
            }
            var prodGroups = prodDetails.GroupBy(d => new { WoNo = d.WorkOrder.WorkOrderNo, Product = d.WorkOrder.Product?.ProductName ?? "—", d.ProductionDate }).ToList();
            int r2 = 2;
            foreach (var g in prodGroups) {
                ws2.Cell(r2,1).Value = g.Key.ProductionDate.ToString("dd.MM.yyyy");
                ws2.Cell(r2,2).Value = g.Key.WoNo;
                ws2.Cell(r2,3).Value = g.Key.Product;
                ws2.Cell(r2,4).Value = string.Join(", ", g.Select(d => d.FscSerial?.Lot?.LotNo).Where(s => s != null).Distinct());
                ws2.Cell(r2,5).Value = string.Join(", ", g.Select(d => d.FscSerial?.Lot?.FscType?.Name).Where(s => s != null).Distinct());
                ws2.Cell(r2,6).Value = string.Join(", ", g.Select(d => d.FscSerial?.Lot?.Supplier?.Name).Where(s => s != null).Distinct());
                ws2.Cell(r2,7).Value = (double)g.Sum(d => d.ConsumedWeight);
                ws2.Cell(r2,8).Value = (double)g.Sum(d => d.WasteWeight);
                ws2.Cell(r2,9).Value = (double)g.Max(d => d.ProducedQuantity);
                r2++;
            }
            ws2.Columns().AdjustToContents();

            // ── Sayfa 3: Satış Sevkiyatları ─────────────────────────────────
            var ws3 = wb.AddWorksheet("Satış Sevkiyatları");
            string[] h3 = { "Sevk Tarihi", "Sipariş No", "Müşteri", "Müşteri FSC Lisans", "Ürün", "Miktar", "Birim", "İş Emri", "CoC Durumu" };
            for (int i = 0; i < h3.Length; i++) {
                ws3.Cell(1, i+1).Value = h3[i];
                ws3.Cell(1, i+1).Style.Font.Bold = true;
                ws3.Cell(1, i+1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                ws3.Cell(1, i+1).Style.Fill.BackgroundColor = headerBg;
            }
            int r3 = 2;
            foreach (var l in salesLines) {
                ws3.Cell(r3,1).Value = (l.SalesOrder.DispatchDate ?? l.SalesOrder.OrderDate).ToString("dd.MM.yyyy");
                ws3.Cell(r3,2).Value = l.SalesOrder.SalesOrderNo;
                ws3.Cell(r3,3).Value = l.SalesOrder.Customer?.Name ?? "—";
                ws3.Cell(r3,4).Value = l.SalesOrder.Customer?.FscLicenseCode ?? "";
                ws3.Cell(r3,5).Value = l.Product?.ProductName ?? "—";
                ws3.Cell(r3,6).Value = (double)l.Quantity;
                ws3.Cell(r3,7).Value = l.Unit;
                ws3.Cell(r3,8).Value = l.WorkOrder?.WorkOrderNo ?? "—";
                ws3.Cell(r3,9).Value = l.WorkOrderId.HasValue ? "Tam" : "Eksik";
                r3++;
            }
            ws3.Columns().AdjustToContents();

            // ── Sayfa 4: Denge Özeti ─────────────────────────────────────────
            var ws4 = wb.AddWorksheet("FSC Denge Özeti");
            ws4.Cell(1,1).Value = $"Dönem: {sd:dd.MM.yyyy} — {ed:dd.MM.yyyy}";
            ws4.Cell(1,1).Style.Font.Bold = true;
            string[] h4 = { "FSC Tipi", "Dönem Girişi (kg)", "Dönem Tüketimi (kg)", "Fire (kg)", "Mevcut Stok (kg)", "Durum" };
            for (int i = 0; i < h4.Length; i++) {
                ws4.Cell(2, i+1).Value = h4[i];
                ws4.Cell(2, i+1).Style.Font.Bold = true;
                ws4.Cell(2, i+1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                ws4.Cell(2, i+1).Style.Fill.BackgroundColor = headerBg;
            }
            var allSerials2 = await _context.FscSerials.Include(s => s.Lot).ThenInclude(l => l.FscType).ToListAsync();
            var consumedMap2 = prodDetails.GroupBy(d => d.FscSerial?.Lot?.FscType?.Name ?? "—")
                .ToDictionary(g => g.Key, g => (Consumed: g.Sum(d => d.ConsumedWeight), Waste: g.Sum(d => d.WasteWeight)));
            var fscTypes2 = allSerials2.Select(s => s.Lot?.FscType?.Name ?? "—").Union(lots.Select(l => l.FscType?.Name ?? "—")).Distinct();
            int r4 = 3;
            foreach (var ft in fscTypes2.OrderBy(x => x)) {
                var inputKg = lots.Where(l => l.FscType?.Name == ft).Sum(l => l.Serials.Sum(s => s.InitialWeight));
                consumedMap2.TryGetValue(ft, out var cons);
                var stockKg = allSerials2.Where(s => s.Lot?.FscType?.Name == ft).Sum(s => s.CurrentWeight);
                var ok = (cons.Consumed + cons.Waste) <= (inputKg + stockKg) + 0.01m;
                ws4.Cell(r4,1).Value = ft;
                ws4.Cell(r4,2).Value = (double)inputKg;
                ws4.Cell(r4,3).Value = (double)cons.Consumed;
                ws4.Cell(r4,4).Value = (double)cons.Waste;
                ws4.Cell(r4,5).Value = (double)stockKg;
                ws4.Cell(r4,6).Value = ok ? "✓ Dengeli" : "⚠ Kontrol Et";
                ws4.Cell(r4,6).Style.Font.FontColor = ok ? ClosedXML.Excel.XLColor.FromHtml("#166534") : ClosedXML.Excel.XLColor.FromHtml("#991b1b");
                r4++;
                if (inputKg > 0 || cons.Consumed > 0 || stockKg > 0) { /* skip empty */ }
            }
            ws4.Columns().AdjustToContents();

            using var ms = new System.IO.MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"DenetimRaporu_{sd:ddMMyyyy}_{ed:ddMMyyyy}.xlsx");
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

    public class AuditReportModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate   { get; set; }
        public List<AuditInputRow>      InputRows      { get; set; } = new();
        public List<AuditProductionRow> ProductionRows { get; set; } = new();
        public List<AuditSalesRow>      SalesRows      { get; set; } = new();
        public List<AuditBalanceRow>    BalanceRows    { get; set; } = new();
    }

    public class AuditInputRow
    {
        public string FscType         { get; set; } = "";
        public string FscCode         { get; set; } = "";
        public string Supplier        { get; set; } = "";
        public string SupplierFscCode { get; set; } = "";
        public int    LotCount        { get; set; }
        public int    SerialCount     { get; set; }
        public decimal TotalWeightKg  { get; set; }
        public string InvoiceNos      { get; set; } = "";
        public string DispatchNos     { get; set; } = "";
        public string LotNos          { get; set; } = "";
    }

    public class AuditProductionRow
    {
        public string   WorkOrderNo    { get; set; } = "";
        public string   ProductName    { get; set; } = "";
        public DateTime ProductionDate { get; set; }
        public string   LotNos        { get; set; } = "";
        public string   FscTypes      { get; set; } = "";
        public string   Suppliers     { get; set; } = "";
        public decimal  ConsumedKg    { get; set; }
        public decimal  WasteKg       { get; set; }
        public decimal  ProducedQty   { get; set; }
    }

    public class AuditSalesRow
    {
        public string   SalesOrderNo       { get; set; } = "";
        public DateTime DispatchDate       { get; set; }
        public string   Customer           { get; set; } = "";
        public string?  CustomerFscLicense { get; set; }
        public bool     CustomerIsFsc      { get; set; }
        public string   ProductName        { get; set; } = "";
        public decimal  Quantity           { get; set; }
        public string   Unit               { get; set; } = "";
        public decimal  UnitPrice          { get; set; }
        public string?  WorkOrderNo        { get; set; }
        public bool     CocComplete        { get; set; }
    }

    public class AuditBalanceRow
    {
        public string  FscType    { get; set; } = "";
        public decimal InputKg    { get; set; }
        public decimal ConsumedKg { get; set; }
        public decimal WasteKg    { get; set; }
        public decimal StockKg    { get; set; }
        public bool    IsBalanced { get; set; }
    }

    public class LotTraceResult
    {
        public FscLot Lot                         { get; set; } = null!;
        public List<FscSerial> Serials            { get; set; } = new();
        public List<SalesOrderLine> SalesLines    { get; set; } = new();
    }
}
