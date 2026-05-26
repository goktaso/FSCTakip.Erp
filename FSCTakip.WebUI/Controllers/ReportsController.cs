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

            ViewBag.Customers   = await _context.Customers.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
            ViewBag.Products    = await _context.Products.Where(p => p.IsActive).OrderBy(p => p.ProductName).ToListAsync();
            ViewBag.StartDate   = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate     = endDate?.ToString("yyyy-MM-dd");
            ViewBag.CustomerId  = customerId;
            ViewBag.ProductId   = productId;

            ViewBag.TotalLines    = rows.Count;
            ViewBag.FullChain     = rows.Count(r => r.ChainComplete);
            ViewBag.MissingChain  = rows.Count(r => !r.ChainComplete);

            return View(rows);
        }

        private static CocRow BuildCocRow(SalesOrderLine l)
        {
            var row = new CocRow
            {
                SalesOrderId  = l.SalesOrderId,
                SalesOrderNo  = l.SalesOrder.SalesOrderNo,
                DispatchDate  = l.SalesOrder.DispatchDate,
                CustomerName  = l.SalesOrder.Customer?.Name ?? "—",
                CustomerFscLicense = l.SalesOrder.Customer?.FscLicenseCode,
                ProductName   = l.Product?.ProductName ?? "—",
                Quantity      = l.Quantity,
                Unit          = l.Unit,
                WorkOrderNo   = l.WorkOrder?.WorkOrderNo,
                WorkOrderId   = l.WorkOrderId,
                SalesOrderLineId = l.Id
            };

            if (l.WorkOrder?.ProductionDetails != null && l.WorkOrder.ProductionDetails.Any())
            {
                var details = l.WorkOrder.ProductionDetails.ToList();
                var serials = details.Select(d => d.FscSerial).Where(s => s != null).ToList();
                var lots    = serials.Select(s => s!.Lot).Where(lot => lot != null).DistinctBy(lot => lot!.Id).ToList();

                row.Serials = serials.Select(s => s!.SerialNo).Distinct().ToList();
                row.LotNos  = lots.Select(lot => lot!.PartiNo).ToList();
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
                .OrderBy(l => l.FscType.Name).ThenBy(l => l.Supplier != null ? l.Supplier.Name : "").ThenBy(l => l.ArrivalDate)
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
                    LotNos          = string.Join(", ", g.Select(l => l.PartiNo).Distinct())
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
                    LotNos        = string.Join(", ", g.Select(d => d.FscSerial?.Lot?.PartiNo).Where(s => s != null).Distinct()),
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
            // Açılış bakiyesi: dönem BAŞINDAN önce gelen seriler, dönem öncesi tüketimle düzeltilmiş
            var preSerials = await _context.FscSerials
                .Include(s => s.Lot).ThenInclude(l => l.FscType)
                .Include(s => s.ProductionDetails)
                .Where(s => s.Lot.ArrivalDate < sd)
                .ToListAsync();

            var openingByType = preSerials
                .GroupBy(s => s.Lot?.FscType?.Name ?? "—")
                .ToDictionary(g => g.Key, g =>
                    g.Sum(s => s.InitialWeight
                        - s.ProductionDetails
                            .Where(d => d.ProductionDate < sd)
                            .Sum(d => d.ConsumedWeight + d.WasteWeight)));

            // Canlı stok (CurrentWeight — bugünkü gerçek değer, bilgi amaçlı)
            var allSerials = await _context.FscSerials
                .Include(s => s.Lot).ThenInclude(l => l.FscType)
                .ToListAsync();

            var currentStockByType = allSerials
                .GroupBy(s => s.Lot?.FscType?.Name ?? "—")
                .ToDictionary(g => g.Key, g => g.Sum(s => s.CurrentWeight));

            // Dönemde tüketilen miktar (üretim detaylarından)
            var consumedByType = prodDetails
                .GroupBy(d => d.FscSerial?.Lot?.FscType?.Name ?? "—")
                .ToDictionary(g => g.Key, g => (Consumed: g.Sum(d => d.ConsumedWeight), Waste: g.Sum(d => d.WasteWeight)));

            // Tüm FSC tiplerini birleştir
            var allFscTypes = lots.Select(l => l.FscType?.Name ?? "—")
                .Union(preSerials.Select(s => s.Lot?.FscType?.Name ?? "—"))
                .Union(prodDetails.Select(d => d.FscSerial?.Lot?.FscType?.Name ?? "—"))
                .Distinct();

            var balanceRows = allFscTypes.Select(type => {
                    openingByType.TryGetValue(type, out var openingKg);
                    consumedByType.TryGetValue(type, out var cons);
                    currentStockByType.TryGetValue(type, out var currentStock);
                    var inputKg   = lots.Where(l => l.FscType?.Name == type).Sum(l => l.Serials.Sum(s => s.InitialWeight));
                    var closingKg = openingKg + inputKg - cons.Consumed - cons.Waste;
                    if (closingKg < 0) closingKg = 0;
                    return new AuditBalanceRow {
                        FscType        = type,
                        OpeningKg      = openingKg < 0 ? 0 : openingKg,
                        InputKg        = inputKg,
                        ConsumedKg     = cons.Consumed,
                        WasteKg        = cons.Waste,
                        ClosingKg      = closingKg,
                        CurrentStockKg = currentStock,
                        IsBalanced     = (cons.Consumed + cons.Waste) <= (openingKg + inputKg) + 0.01m
                    };
                })
                .Where(r => r.OpeningKg > 0 || r.InputKg > 0 || r.ConsumedKg > 0)
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
                ws2.Cell(r2,4).Value = string.Join(", ", g.Select(d => d.FscSerial?.Lot?.PartiNo).Where(s => s != null).Distinct());
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
            string[] h4 = { "FSC Tipi", "Açılış Bakiyesi (kg)", "Dönem Girişi (kg)", "Dönem Tüketimi (kg)", "Fire (kg)", "Kapanış Bakiyesi (kg)", "Canlı Stok (kg)", "Durum" };
            for (int i = 0; i < h4.Length; i++) {
                ws4.Cell(2, i+1).Value = h4[i];
                ws4.Cell(2, i+1).Style.Font.Bold = true;
                ws4.Cell(2, i+1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                ws4.Cell(2, i+1).Style.Fill.BackgroundColor = headerBg;
            }
            // Açılış bakiyesi (Export için)
            var preSerials2 = await _context.FscSerials
                .Include(s => s.Lot).ThenInclude(l => l.FscType)
                .Include(s => s.ProductionDetails)
                .Where(s => s.Lot.ArrivalDate < sd)
                .ToListAsync();
            var openingMap2 = preSerials2
                .GroupBy(s => s.Lot?.FscType?.Name ?? "—")
                .ToDictionary(g => g.Key, g =>
                    g.Sum(s => s.InitialWeight - s.ProductionDetails
                        .Where(d => d.ProductionDate < sd)
                        .Sum(d => d.ConsumedWeight + d.WasteWeight)));
            var allSerials2 = await _context.FscSerials.Include(s => s.Lot).ThenInclude(l => l.FscType).ToListAsync();
            var consumedMap2 = prodDetails.GroupBy(d => d.FscSerial?.Lot?.FscType?.Name ?? "—")
                .ToDictionary(g => g.Key, g => (Consumed: g.Sum(d => d.ConsumedWeight), Waste: g.Sum(d => d.WasteWeight)));
            var fscTypes2 = lots.Select(l => l.FscType?.Name ?? "—")
                .Union(preSerials2.Select(s => s.Lot?.FscType?.Name ?? "—"))
                .Union(prodDetails.Select(d => d.FscSerial?.Lot?.FscType?.Name ?? "—"))
                .Distinct();
            int r4 = 3;
            foreach (var ft in fscTypes2.OrderBy(x => x)) {
                var inputKg = lots.Where(l => l.FscType?.Name == ft).Sum(l => l.Serials.Sum(s => s.InitialWeight));
                openingMap2.TryGetValue(ft, out var openingKg2);
                if (openingKg2 < 0) openingKg2 = 0;
                consumedMap2.TryGetValue(ft, out var cons);
                var stockKg = allSerials2.Where(s => s.Lot?.FscType?.Name == ft).Sum(s => s.CurrentWeight);
                var closingKg2 = openingKg2 + inputKg - cons.Consumed - cons.Waste;
                if (closingKg2 < 0) closingKg2 = 0;
                var ok = (cons.Consumed + cons.Waste) <= (openingKg2 + inputKg) + 0.01m;
                ws4.Cell(r4,1).Value = ft;
                ws4.Cell(r4,2).Value = (double)openingKg2;
                ws4.Cell(r4,3).Value = (double)inputKg;
                ws4.Cell(r4,4).Value = (double)cons.Consumed;
                ws4.Cell(r4,5).Value = (double)cons.Waste;
                ws4.Cell(r4,6).Value = (double)closingKg2;
                ws4.Cell(r4,7).Value = (double)stockKg;
                ws4.Cell(r4,8).Value = ok ? "✓ Dengeli" : "⚠ Kontrol Et";
                ws4.Cell(r4,8).Style.Font.FontColor = ok ? ClosedXML.Excel.XLColor.FromHtml("#166534") : ClosedXML.Excel.XLColor.FromHtml("#991b1b");
                r4++;
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

        // GET /Reports/CustomerFsc
        public async Task<IActionResult> CustomerFsc()
        {
            var customers = await _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var today = DateTime.Today;
            ViewBag.Valid    = customers.Count(c => c.IsFscActive && (c.FscExpiryDate == null || c.FscExpiryDate > today.AddDays(30)));
            ViewBag.Expiring = customers.Count(c => c.IsFscActive && c.FscExpiryDate.HasValue && c.FscExpiryDate <= today.AddDays(30) && c.FscExpiryDate > today);
            ViewBag.Expired  = customers.Count(c => !c.IsFscActive || (c.FscExpiryDate.HasValue && c.FscExpiryDate <= today));
            ViewBag.Today    = today;

            return View(customers);
        }

        // ── 6. BOM Bileşen Analizi ──────────────────────────────────────────────
        // GET /Reports/BomAnalysis
        public async Task<IActionResult> BomAnalysis(DateTime? startDate, DateTime? endDate, int? workOrderId)
        {
            var sd     = startDate ?? new DateTime(DateTime.Today.Year, 1, 1);
            var ed     = endDate   ?? DateTime.Today;
            var edNext = ed.AddDays(1);

            var woQuery = _context.WorkOrders
                .Include(w => w.Product).ThenInclude(p => p!.FscType)
                .Include(w => w.Machine)
                .Include(w => w.WorkOrderRecipes)
                    .ThenInclude(r => r.Product).ThenInclude(p => p!.FscType)
                .Include(w => w.ProductionDetails)
                    .ThenInclude(pd => pd.WorkOrderRecipe)
                        .ThenInclude(r => r!.Product)
                .Include(w => w.ProductionDetails)
                    .ThenInclude(pd => pd.FscSerial)
                        .ThenInclude(s => s.Lot)
                            .ThenInclude(l => l.FscType)
                .Include(w => w.ProductionDetails)
                    .ThenInclude(pd => pd.FscSerial)
                        .ThenInclude(s => s.Lot)
                            .ThenInclude(l => l.Supplier)
                .Where(w => w.PlannedDate >= sd && w.PlannedDate < edNext
                         && w.Status != WorkOrderStatus.Taslak)
                .AsQueryable();

            if (workOrderId.HasValue)
                woQuery = woQuery.Where(w => w.Id == workOrderId.Value);

            var workOrders = await woQuery.OrderByDescending(w => w.PlannedDate).ToListAsync();

            var rows = workOrders.Select(w =>
            {
                var woRow = new BomWorkOrderRow
                {
                    WorkOrderId    = w.Id,
                    WorkOrderNo    = w.WorkOrderNo,
                    ProductName    = w.Product?.ProductName ?? "—",
                    ProductCode    = w.Product?.ProductCode ?? "",
                    FscTypeName    = w.Product?.FscType?.Name,
                    MachineName    = w.Machine?.Name ?? "—",
                    PlannedDate    = w.PlannedDate,
                    Status         = w.Status,
                    PlannedQty     = w.PlannedQuantity,
                    TotalProducedQty = w.ProductionDetails.Sum(pd => pd.ProducedQuantity),
                    TotalConsumedKg  = w.ProductionDetails.Sum(pd => pd.ConsumedWeight),
                    TotalWasteKg     = w.ProductionDetails.Sum(pd => pd.WasteWeight)
                };

                // BOM bileşenleri: WorkOrderRecipe kayıtları
                woRow.Components = w.WorkOrderRecipes.Select(wr =>
                {
                    // Bu bileşene bağlı üretim detayları
                    var linked = w.ProductionDetails.Where(pd => pd.WorkOrderRecipeId == wr.Id).ToList();
                    var serials   = linked.Select(pd => pd.FscSerial?.SerialNo ?? "").Where(s => s != "").Distinct().ToList();
                    var lots      = linked.Select(pd => pd.FscSerial?.Lot?.PartiNo ?? "").Where(s => s != "").Distinct().ToList();
                    var suppliers = linked.Select(pd => pd.FscSerial?.Lot?.Supplier?.Name ?? "").Where(s => s != "").Distinct().ToList();
                    var fscTypes  = linked.Select(pd => pd.FscSerial?.Lot?.FscType?.Name ?? "").Where(s => s != "").Distinct().ToList();

                    return new BomComponentRow
                    {
                        ComponentName = wr.Product?.ProductName ?? "—",
                        ComponentCode = wr.Product?.ProductCode ?? "",
                        FscTypeName   = wr.Product?.FscType?.Name,
                        FscTypeCode   = wr.Product?.FscType?.Code,
                        PlannedKg     = wr.PlannedQuantity,
                        ConsumedKg    = wr.ActualConsumedQuantity,
                        WasteKg       = wr.WasteQuantity,
                        ProducedQty   = wr.ProducedQuantity,
                        SerialNos     = serials,
                        LotNos        = lots,
                        SupplierNames = suppliers,
                        InputFscTypes = fscTypes,
                        IsUnlinked    = false
                    };
                }).ToList();

                // Bileşene bağlanmamış serbest tüketimler
                var unlinked = w.ProductionDetails.Where(pd => pd.WorkOrderRecipeId == null).ToList();
                if (unlinked.Any())
                {
                    woRow.UnlinkedComponents.Add(new BomComponentRow
                    {
                        ComponentName = "— Serbest Tüketim (Reçete Dışı) —",
                        ComponentCode = "",
                        FscTypeName   = null,
                        PlannedKg     = 0,
                        ConsumedKg    = unlinked.Sum(pd => pd.ConsumedWeight),
                        WasteKg       = unlinked.Sum(pd => pd.WasteWeight),
                        ProducedQty   = unlinked.Sum(pd => pd.ProducedQuantity),
                        SerialNos     = unlinked.Select(pd => pd.FscSerial?.SerialNo ?? "").Where(s => s != "").Distinct().ToList(),
                        LotNos        = unlinked.Select(pd => pd.FscSerial?.Lot?.PartiNo ?? "").Where(s => s != "").Distinct().ToList(),
                        SupplierNames = unlinked.Select(pd => pd.FscSerial?.Lot?.Supplier?.Name ?? "").Where(s => s != "").Distinct().ToList(),
                        InputFscTypes = unlinked.Select(pd => pd.FscSerial?.Lot?.FscType?.Name ?? "").Where(s => s != "").Distinct().ToList(),
                        IsUnlinked    = true
                    });
                }

                return woRow;
            }).ToList();

            var model = new BomAnalysisModel
            {
                StartDate  = sd,
                EndDate    = ed,
                WorkOrders = rows
            };

            ViewBag.StartDate    = sd.ToString("yyyy-MM-dd");
            ViewBag.EndDate      = ed.ToString("yyyy-MM-dd");
            ViewBag.AllWorkOrders = await _context.WorkOrders
                .Where(w => w.Status != WorkOrderStatus.Taslak)
                .OrderByDescending(w => w.PlannedDate)
                .Select(w => new { w.Id, w.WorkOrderNo })
                .ToListAsync();
            ViewData["Title"] = "BOM Bileşen Analizi";
            return View(model);
        }

        // GET /Reports/ExportBomAnalysis
        public async Task<IActionResult> ExportBomAnalysis(DateTime? startDate, DateTime? endDate)
        {
            var sd     = startDate ?? new DateTime(DateTime.Today.Year, 1, 1);
            var ed     = endDate   ?? DateTime.Today;
            var edNext = ed.AddDays(1);

            var workOrders = await _context.WorkOrders
                .Include(w => w.Product).ThenInclude(p => p!.FscType)
                .Include(w => w.Machine)
                .Include(w => w.WorkOrderRecipes)
                    .ThenInclude(r => r.Product).ThenInclude(p => p!.FscType)
                .Include(w => w.ProductionDetails)
                    .ThenInclude(pd => pd.FscSerial)
                        .ThenInclude(s => s.Lot)
                            .ThenInclude(l => l.FscType)
                .Include(w => w.ProductionDetails)
                    .ThenInclude(pd => pd.FscSerial)
                        .ThenInclude(s => s.Lot)
                            .ThenInclude(l => l.Supplier)
                .Where(w => w.PlannedDate >= sd && w.PlannedDate < edNext
                         && w.Status != WorkOrderStatus.Taslak)
                .OrderByDescending(w => w.PlannedDate)
                .ToListAsync();

            using var wb = new ClosedXML.Excel.XLWorkbook();
            var headerBg = ClosedXML.Excel.XLColor.FromHtml("#1e3d14");
            var ws = wb.AddWorksheet("BOM Analizi");

            string[] headers = {
                "İş Emri No", "Mamul", "Durum", "Plan Tarihi",
                "Bileşen", "Bileşen FSC Tipi", "Planlanan (kg)",
                "Gerçek Tüketim (kg)", "Fire (kg)", "Üretilen (adet)",
                "Mass-Balance", "Kullanılan Lotlar", "Tedarikçiler"
            };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
                ws.Cell(1, i + 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                ws.Cell(1, i + 1).Style.Fill.BackgroundColor = headerBg;
            }

            int row = 2;
            foreach (var w in workOrders)
            {
                var statusLabel = w.Status switch {
                    WorkOrderStatus.Uretimde   => "Üretimde",
                    WorkOrderStatus.Tamamlandi => "Tamamlandı",
                    WorkOrderStatus.Iptal      => "İptal",
                    _                          => "—"
                };

                if (w.WorkOrderRecipes.Any())
                {
                    foreach (var wr in w.WorkOrderRecipes)
                    {
                        var linked = w.ProductionDetails.Where(pd => pd.WorkOrderRecipeId == wr.Id).ToList();
                        var lots = linked.Select(pd => pd.FscSerial?.Lot?.PartiNo ?? "").Where(s => s != "").Distinct();
                        var suppliers = linked.Select(pd => pd.FscSerial?.Lot?.Supplier?.Name ?? "").Where(s => s != "").Distinct();
                        var ok = wr.PlannedQuantity == 0 || wr.ActualConsumedQuantity <= wr.PlannedQuantity * 1.10m;

                        ws.Cell(row, 1).Value  = w.WorkOrderNo;
                        ws.Cell(row, 2).Value  = w.Product?.ProductName ?? "—";
                        ws.Cell(row, 3).Value  = statusLabel;
                        ws.Cell(row, 4).Value  = w.PlannedDate.ToString("dd.MM.yyyy");
                        ws.Cell(row, 5).Value  = wr.Product?.ProductName ?? "—";
                        ws.Cell(row, 6).Value  = wr.Product?.FscType?.Name ?? "—";
                        ws.Cell(row, 7).Value  = (double)wr.PlannedQuantity;
                        ws.Cell(row, 8).Value  = (double)wr.ActualConsumedQuantity;
                        ws.Cell(row, 9).Value  = (double)wr.WasteQuantity;
                        ws.Cell(row, 10).Value = (double)wr.ProducedQuantity;
                        ws.Cell(row, 11).Value = ok ? "✓ OK" : "⚠ Aşım";
                        ws.Cell(row, 11).Style.Font.FontColor = ok
                            ? ClosedXML.Excel.XLColor.FromHtml("#166534")
                            : ClosedXML.Excel.XLColor.FromHtml("#991b1b");
                        ws.Cell(row, 12).Value = string.Join(", ", lots);
                        ws.Cell(row, 13).Value = string.Join(", ", suppliers);
                        row++;
                    }
                }
                else
                {
                    // BOM tanımlı değil — sadece toplam
                    var totalConsumed = w.ProductionDetails.Sum(pd => pd.ConsumedWeight);
                    var totalWaste    = w.ProductionDetails.Sum(pd => pd.WasteWeight);
                    var totalProduced = w.ProductionDetails.Sum(pd => pd.ProducedQuantity);
                    ws.Cell(row, 1).Value  = w.WorkOrderNo;
                    ws.Cell(row, 2).Value  = w.Product?.ProductName ?? "—";
                    ws.Cell(row, 3).Value  = statusLabel;
                    ws.Cell(row, 4).Value  = w.PlannedDate.ToString("dd.MM.yyyy");
                    ws.Cell(row, 5).Value  = "(BOM tanımlı değil)";
                    ws.Cell(row, 7).Value  = (double)w.PlannedQuantity;
                    ws.Cell(row, 8).Value  = (double)totalConsumed;
                    ws.Cell(row, 9).Value  = (double)totalWaste;
                    ws.Cell(row, 10).Value = (double)totalProduced;
                    ws.Cell(row, 11).Value = "—";
                    row++;
                }
            }

            ws.Columns().AdjustToContents();

            using var ms = new System.IO.MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"BOM_Analizi_{sd:ddMMyyyy}_{ed:ddMMyyyy}.xlsx");
        }

        // ── 5. Tam İzlenebilirlik — Satış → Üretim → Lot ───────────────────────
        // GET /Reports/Traceability/{id}
        public async Task<IActionResult> Traceability(int id)
        {
            var order = await _context.SalesOrders
                .Include(o => o.Customer)
                .Include(o => o.Lines)
                    .ThenInclude(l => l.Product)
                .Include(o => o.Lines)
                    .ThenInclude(l => l.WorkOrder)
                        .ThenInclude(w => w!.Product)
                .Include(o => o.Lines)
                    .ThenInclude(l => l.WorkOrder)
                        .ThenInclude(w => w!.Machine)
                .Include(o => o.Lines)
                    .ThenInclude(l => l.WorkOrder)
                        .ThenInclude(w => w!.ProductionDetails)
                            .ThenInclude(pd => pd.FscSerial)
                                .ThenInclude(s => s.Lot)
                                    .ThenInclude(lot => lot.Supplier)
                .Include(o => o.Lines)
                    .ThenInclude(l => l.WorkOrder)
                        .ThenInclude(w => w!.ProductionDetails)
                            .ThenInclude(pd => pd.FscSerial)
                                .ThenInclude(s => s.Lot)
                                    .ThenInclude(lot => lot.FscType)
                .Include(o => o.Lines)
                    .ThenInclude(l => l.WorkOrder)
                        .ThenInclude(w => w!.ProductionDetails)
                            .ThenInclude(pd => pd.Machine)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            var lineModels = new List<TraceabilityLineModel>();

            foreach (var line in order.Lines)
            {
                var lm = new TraceabilityLineModel { Line = line, WorkOrder = line.WorkOrder };

                if (line.WorkOrder?.ProductionDetails != null && line.WorkOrder.ProductionDetails.Any())
                {
                    var lotGroups = line.WorkOrder.ProductionDetails
                        .GroupBy(pd => pd.FscSerial?.LotId ?? 0)
                        .Select(g =>
                        {
                            var lot = g.First().FscSerial?.Lot;
                            if (lot == null) return null;

                            var supplier = lot.Supplier;
                            var fscValid = supplier != null &&
                                           supplier.IsFscActive &&
                                           (supplier.FscExpiryDate == null || supplier.FscExpiryDate >= DateTime.Today);

                            return new TraceabilityConsumptionGroup
                            {
                                Lot            = lot,
                                ConsumedKg     = g.Sum(pd => pd.ConsumedWeight),
                                WasteKg        = g.Sum(pd => pd.WasteWeight),
                                SupplierFscValid = fscValid,
                                Serials = g.OrderBy(pd => pd.FscSerial?.SerialNo).Select(pd => new TraceabilitySerialRow
                                {
                                    Serial         = pd.FscSerial!,
                                    ConsumedKg     = pd.ConsumedWeight,
                                    WasteKg        = pd.WasteWeight,
                                    ProducedQty    = pd.ProducedQuantity,
                                    ProductionDate = pd.ProductionDate
                                }).ToList()
                            };
                        })
                        .Where(g => g != null)
                        .Cast<TraceabilityConsumptionGroup>()
                        .ToList();

                    lm.LotGroups       = lotGroups;
                    lm.TotalConsumedKg = lotGroups.Sum(g => g.ConsumedKg);
                    lm.TotalWasteKg    = lotGroups.Sum(g => g.WasteKg);
                    lm.HasFullChain    = lotGroups.Any() && lotGroups.All(g => g.SupplierFscValid);
                }

                lineModels.Add(lm);
            }

            var model = new TraceabilityModel
            {
                Order           = order,
                Lines           = lineModels,
                ChainComplete   = lineModels.Any() && lineModels.All(l => l.HasFullChain),
                TotalLots       = lineModels.SelectMany(l => l.LotGroups).Select(g => g.Lot.Id).Distinct().Count(),
                TotalSerials    = lineModels.SelectMany(l => l.LotGroups).SelectMany(g => g.Serials).Count(),
                TotalConsumedKg = lineModels.Sum(l => l.TotalConsumedKg),
                TotalWasteKg    = lineModels.Sum(l => l.TotalWasteKg)
            };

            ViewData["Title"] = $"Tam İzlenebilirlik — {order.SalesOrderNo}";
            return View(model);
        }

        // ── 8. Fire / Atık Derinlemesine Raporu ─────────────────────────────────
        // GET /Reports/WasteAnalysis
        public async Task<IActionResult> WasteAnalysis(DateTime? startDate, DateTime? endDate, int? machineId, int? productGroupId)
        {
            var sd     = startDate ?? new DateTime(DateTime.Today.Year, 1, 1);
            var ed     = endDate   ?? DateTime.Today;
            var edNext = ed.AddDays(1);

            // Üretim detayından fire verileri
            var prodDetails = await _context.ProductionDetails
                .Include(d => d.WorkOrder).ThenInclude(w => w!.Machine)
                .Include(d => d.WorkOrder).ThenInclude(w => w!.Product).ThenInclude(p => p!.ProductGroup)
                .Include(d => d.FscSerial).ThenInclude(s => s.Lot).ThenInclude(l => l.FscType)
                .Where(d => d.ProductionDate >= sd && d.ProductionDate < edNext)
                .ToListAsync();

            if (machineId.HasValue)
                prodDetails = prodDetails.Where(d => d.WorkOrder != null && d.WorkOrder.MachineId == machineId.Value).ToList();
            if (productGroupId.HasValue)
                prodDetails = prodDetails.Where(d => d.WorkOrder?.Product?.ProductGroupId == productGroupId.Value).ToList();

            // WasteManagement kayıtları
            var wasteRecords = await _context.WasteManagements
                .Include(w => w.WorkOrder).ThenInclude(wo => wo!.Machine)
                .Where(w => w.DisposalDate >= sd && w.DisposalDate < edNext)
                .ToListAsync();

            // Makine bazında fire özeti
            var byMachine = prodDetails
                .Where(d => d.WorkOrder?.Machine != null)
                .GroupBy(d => new { MachineId = d.WorkOrder!.MachineId, MachineName = d.WorkOrder!.Machine!.Name })
                .Select(g => new WasteByMachineRow
                {
                    MachineName     = g.Key.MachineName,
                    TotalConsumedKg = g.Sum(d => d.ConsumedWeight),
                    TotalWasteKg    = g.Sum(d => d.WasteWeight),
                    TotalProducedQty = g.Sum(d => d.ProducedQuantity),
                    RecordCount     = g.Count()
                })
                .OrderByDescending(r => r.FireRate)
                .ToList();

            // Aylık fire trendi
            var monthlyTrend = prodDetails
                .GroupBy(d => new { d.ProductionDate.Year, d.ProductionDate.Month })
                .Select(g => new WasteMonthlyRow
                {
                    Year            = g.Key.Year,
                    Month           = g.Key.Month,
                    TotalConsumedKg = g.Sum(d => d.ConsumedWeight),
                    TotalWasteKg    = g.Sum(d => d.WasteWeight)
                })
                .OrderBy(r => r.Year).ThenBy(r => r.Month)
                .ToList();

            // Kategori bazında atık özeti (WasteManagement tablosundan)
            var byCategory = wasteRecords
                .GroupBy(w => w.Category)
                .Select(g => new WasteByCategoryRow
                {
                    Category    = g.Key.ToString().Replace("Artigi","Artığı").Replace("Hasari","Hasarı").Replace("Hatasi","Hatası"),
                    TotalKg     = g.Sum(w => w.Quantity),
                    RecordCount = g.Count()
                })
                .OrderByDescending(r => r.TotalKg)
                .ToList();

            var model = new WasteAnalysisModel
            {
                StartDate      = sd,
                EndDate        = ed,
                TotalConsumedKg = prodDetails.Sum(d => d.ConsumedWeight),
                TotalWasteKg   = prodDetails.Sum(d => d.WasteWeight),
                TotalWasteRecordKg = wasteRecords.Sum(w => w.Quantity),
                ByMachine      = byMachine,
                MonthlyTrend   = monthlyTrend,
                ByCategory     = byCategory
            };

            ViewBag.Machines      = await _context.Machines.Where(m => m.IsActive).OrderBy(m => m.Name).ToListAsync();
            ViewBag.ProductGroups = await _context.ProductGroups.OrderBy(g => g.GroupName).ToListAsync();
            ViewBag.MachineId     = machineId;
            ViewBag.ProductGroupId = productGroupId;

            ViewData["Title"] = "Fire / Atık Analizi";
            return View(model);
        }
    }

    // ── View Model Sınıfları ──────────────────────────────────────────────────

    public class CocRow
    {
        public int    SalesOrderId     { get; set; }
        public string SalesOrderNo     { get; set; } = "";
        public DateTime? DispatchDate  { get; set; }
        public string CustomerName     { get; set; } = "";
        public string? CustomerFscLicense { get; set; }
        public string ProductName      { get; set; } = "";
        public decimal Quantity        { get; set; }
        public string Unit             { get; set; } = "";
        public string? WorkOrderNo     { get; set; }
        public int?   WorkOrderId      { get; set; }
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
        public string  FscType        { get; set; } = "";
        public decimal OpeningKg      { get; set; }  // dönem başı devir bakiye
        public decimal InputKg        { get; set; }  // dönem girişi
        public decimal ConsumedKg     { get; set; }  // dönem tüketimi
        public decimal WasteKg        { get; set; }  // dönem firesi
        public decimal ClosingKg      { get; set; }  // dönem sonu kapanış (hesaplanan)
        public decimal CurrentStockKg { get; set; }  // canlı stok (CurrentWeight)
        public bool    IsBalanced     { get; set; }
    }

    public class LotTraceResult
    {
        public FscLot Lot                         { get; set; } = null!;
        public List<FscSerial> Serials            { get; set; } = new();
        public List<SalesOrderLine> SalesLines    { get; set; } = new();
    }

    // ── Tam İzlenebilirlik View Modelleri ──────────────────────────────────────

    public class TraceabilityModel
    {
        public SalesOrder Order             { get; set; } = null!;
        public List<TraceabilityLineModel> Lines { get; set; } = new();
        public bool    ChainComplete        { get; set; }
        public int     TotalLots            { get; set; }
        public int     TotalSerials         { get; set; }
        public decimal TotalConsumedKg      { get; set; }
        public decimal TotalWasteKg         { get; set; }
    }

    public class TraceabilityLineModel
    {
        public SalesOrderLine Line          { get; set; } = null!;
        public WorkOrder?     WorkOrder     { get; set; }
        public List<TraceabilityConsumptionGroup> LotGroups { get; set; } = new();
        public bool    HasFullChain         { get; set; }
        public decimal TotalConsumedKg      { get; set; }
        public decimal TotalWasteKg         { get; set; }
    }

    public class TraceabilityConsumptionGroup
    {
        public FscLot  Lot                  { get; set; } = null!;
        public decimal ConsumedKg           { get; set; }
        public decimal WasteKg              { get; set; }
        public bool    SupplierFscValid     { get; set; }
        public List<TraceabilitySerialRow> Serials { get; set; } = new();
    }

    public class TraceabilitySerialRow
    {
        public FscSerial Serial             { get; set; } = null!;
        public decimal   ConsumedKg         { get; set; }
        public decimal   WasteKg            { get; set; }
        public decimal   ProducedQty        { get; set; }
        public DateTime  ProductionDate     { get; set; }
    }

    // ── BOM Analizi View Modelleri ─────────────────────────────────────────────

    public class BomAnalysisModel
    {
        public DateTime StartDate            { get; set; }
        public DateTime EndDate              { get; set; }
        public List<BomWorkOrderRow> WorkOrders { get; set; } = new();
    }

    public class BomWorkOrderRow
    {
        public int            WorkOrderId      { get; set; }
        public string         WorkOrderNo      { get; set; } = "";
        public string         ProductName      { get; set; } = "";
        public string         ProductCode      { get; set; } = "";
        public string?        FscTypeName      { get; set; }
        public string         MachineName      { get; set; } = "";
        public DateTime       PlannedDate      { get; set; }
        public WorkOrderStatus Status          { get; set; }
        public decimal        PlannedQty       { get; set; }
        public decimal        TotalProducedQty { get; set; }
        public decimal        TotalConsumedKg  { get; set; }
        public decimal        TotalWasteKg     { get; set; }
        public List<BomComponentRow> Components         { get; set; } = new();
        public List<BomComponentRow> UnlinkedComponents { get; set; } = new();

        public decimal FireRate => TotalConsumedKg > 0 ? TotalWasteKg / TotalConsumedKg * 100 : 0;
        public bool HasBom     => Components.Any();
    }

    public class BomComponentRow
    {
        public string        ComponentName  { get; set; } = "";
        public string        ComponentCode  { get; set; } = "";
        public string?       FscTypeName    { get; set; }
        public string?       FscTypeCode    { get; set; }
        public decimal       PlannedKg      { get; set; }
        public decimal       ConsumedKg     { get; set; }
        public decimal       WasteKg        { get; set; }
        public decimal       ProducedQty    { get; set; }
        public bool          IsUnlinked     { get; set; }
        public List<string>  SerialNos      { get; set; } = new();
        public List<string>  LotNos         { get; set; } = new();
        public List<string>  SupplierNames  { get; set; } = new();
        public List<string>  InputFscTypes  { get; set; } = new();

        public decimal FireRate      => ConsumedKg > 0 ? WasteKg / ConsumedKg * 100 : 0;
        public bool    IsOverPlan    => PlannedKg > 0 && ConsumedKg > PlannedKg * 1.10m;
        public bool    IsNearPlan    => PlannedKg > 0 && ConsumedKg >= PlannedKg * 0.90m;
    }

    // ── WasteAnalysis View Models ──────────────────────────────────────────────
    public class WasteAnalysisModel
    {
        public DateTime StartDate           { get; set; }
        public DateTime EndDate             { get; set; }
        public decimal  TotalConsumedKg     { get; set; }
        public decimal  TotalWasteKg        { get; set; }
        public decimal  TotalWasteRecordKg  { get; set; }
        public List<WasteByMachineRow>   ByMachine    { get; set; } = new();
        public List<WasteMonthlyRow>     MonthlyTrend { get; set; } = new();
        public List<WasteByCategoryRow>  ByCategory   { get; set; } = new();

        public decimal OverallFireRate => TotalConsumedKg > 0 ? TotalWasteKg / TotalConsumedKg * 100 : 0;
    }

    public class WasteByMachineRow
    {
        public string  MachineName      { get; set; } = "";
        public decimal TotalConsumedKg  { get; set; }
        public decimal TotalWasteKg     { get; set; }
        public decimal TotalProducedQty { get; set; }
        public int     RecordCount      { get; set; }
        public decimal FireRate         => TotalConsumedKg > 0 ? TotalWasteKg / TotalConsumedKg * 100 : 0;
    }

    public class WasteMonthlyRow
    {
        public int     Year            { get; set; }
        public int     Month           { get; set; }
        public decimal TotalConsumedKg { get; set; }
        public decimal TotalWasteKg    { get; set; }
        public decimal FireRate        => TotalConsumedKg > 0 ? TotalWasteKg / TotalConsumedKg * 100 : 0;
    }

    public class WasteByCategoryRow
    {
        public string  Category    { get; set; } = "";
        public decimal TotalKg     { get; set; }
        public int     RecordCount { get; set; }
    }
}
