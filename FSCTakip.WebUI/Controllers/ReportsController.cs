using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public async Task<IActionResult> LotTrace(int? lotId, string? search)
        {
            var lots = await _context.FscLots
                .Include(l => l.Supplier)
                .Include(l => l.FscType)
                .Include(l => l.Product)
                .OrderByDescending(l => l.ArrivalDate)
                .ToListAsync();

            ViewBag.Lots   = lots;
            ViewBag.Search = search;

            // Metin araması ile parti/seri no'ya göre lot bul
            if (!lotId.HasValue && !string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToUpperInvariant();
                var matchLot = lots.FirstOrDefault(l =>
                    l.PartiNo.ToUpperInvariant().Contains(term) ||
                    (l.InvoiceNo?.ToUpperInvariant().Contains(term) ?? false));
                if (matchLot != null) lotId = matchLot.Id;
            }

            if (!lotId.HasValue)
                return View(null as LotTraceResult);

            var lot = lots.FirstOrDefault(l => l.Id == lotId.Value);
            if (lot == null) return View(null as LotTraceResult);

            var serials = await _context.FscSerials
                .Include(s => s.ProductionDetails)
                    .ThenInclude(pd => pd.WorkOrder)
                        .ThenInclude(w => w.Product)
                .Include(s => s.ProductionDetails)
                    .ThenInclude(pd => pd.WorkOrder)
                        .ThenInclude(w => w.Machine)
                .Where(s => s.LotId == lotId.Value)
                .OrderBy(s => s.SerialNo)
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

            // Üretim kullanım satırları: bu partiyi hangi iş emirleri, hangi gün, kaç kg kullandı
            var flatPd = serials
                .SelectMany(s => s.ProductionDetails.Select(pd => new { SerialNo = s.SerialNo, pd }))
                .ToList();

            var productionRows = flatPd
                .GroupBy(x => new
                {
                    x.pd.WorkOrderId,
                    WorkOrderNo = x.pd.WorkOrder?.WorkOrderNo ?? "—",
                    ProductName = x.pd.WorkOrder?.Product?.ProductName ?? "—",
                    ProductCode = x.pd.WorkOrder?.Product?.ProductCode ?? "",
                    MachineName = x.pd.WorkOrder?.Machine?.Name ?? "—",
                    Date        = x.pd.ProductionDate.Date
                })
                .Select(g => new LotTraceProductionRow
                {
                    WorkOrderId    = g.Key.WorkOrderId,
                    WorkOrderNo    = g.Key.WorkOrderNo,
                    ProductName    = g.Key.ProductName,
                    ProductCode    = g.Key.ProductCode,
                    MachineName    = g.Key.MachineName,
                    ProductionDate = g.Key.Date,
                    SerialNos      = string.Join(", ", g.Select(x => x.SerialNo).Distinct()),
                    ConsumedKg     = g.Sum(x => x.pd.ConsumedWeight),
                    WasteKg        = g.Sum(x => x.pd.WasteWeight),
                    ProducedQty    = g.Max(x => x.pd.ProducedQuantity)
                })
                .OrderBy(r => r.ProductionDate)
                .ThenBy(r => r.WorkOrderNo)
                .ToList();

            var result = new LotTraceResult
            {
                Lot            = lot,
                Serials        = serials,
                SalesLines     = salesLines,
                ProductionRows = productionRows
            };

            ViewBag.LotId = lotId;
            return View(result);
        }

        // GET /Reports/ExportLotTrace
        public async Task<IActionResult> ExportLotTrace(int lotId)
        {
            var lot = await _context.FscLots
                .Include(l => l.Supplier)
                .Include(l => l.FscType)
                .Include(l => l.Product)
                .FirstOrDefaultAsync(l => l.Id == lotId);

            if (lot == null) return NotFound();

            var serials = await _context.FscSerials
                .Include(s => s.ProductionDetails)
                    .ThenInclude(pd => pd.WorkOrder)
                        .ThenInclude(w => w.Product)
                .Include(s => s.ProductionDetails)
                    .ThenInclude(pd => pd.WorkOrder)
                        .ThenInclude(w => w.Machine)
                .Where(s => s.LotId == lotId)
                .OrderBy(s => s.SerialNo)
                .ToListAsync();

            var flatPd = serials
                .SelectMany(s => s.ProductionDetails.Select(pd => new { SerialNo = s.SerialNo, pd }))
                .ToList();

            var productionRows = flatPd
                .GroupBy(x => new
                {
                    x.pd.WorkOrderId,
                    WorkOrderNo = x.pd.WorkOrder?.WorkOrderNo ?? "—",
                    ProductName = x.pd.WorkOrder?.Product?.ProductName ?? "—",
                    MachineName = x.pd.WorkOrder?.Machine?.Name ?? "—",
                    Date        = x.pd.ProductionDate.Date
                })
                .Select(g => new
                {
                    UretimTarihi   = g.Key.Date.ToString("dd.MM.yyyy"),
                    IsEmriNo       = g.Key.WorkOrderNo,
                    Mamul          = g.Key.ProductName,
                    Makine         = g.Key.MachineName,
                    KullanilanSeri = string.Join(", ", g.Select(x => x.SerialNo).Distinct()),
                    TuketilenKg    = g.Sum(x => x.pd.ConsumedWeight),
                    FireKg         = g.Sum(x => x.pd.WasteWeight),
                    UretilenAdet   = g.Max(x => x.pd.ProducedQuantity)
                })
                .OrderBy(r => r.UretimTarihi)
                .ToList();

            return ExportToExcel(productionRows, $"LotKullanim_{lot.PartiNo}");
        }

        // ── 4. Denetim Özet Raporu ─────────────────────────────────────────────
        // GET /Reports/AuditReport
        public async Task<IActionResult> AuditReport(DateTime? startDate, DateTime? endDate)
        {
            var sd = startDate ?? new DateTime(DateTime.Today.Year, 1, 1);
            var ed = endDate   ?? DateTime.Today;

            // ── Ortak veri yükle ────────────────────────────────────────────
            var data        = await BuildAuditReportData(sd, ed);
            var lots        = data.Lots;
            var prodDetails = data.ProdDetails;
            var salesLines  = data.SalesLines;
            var preSerials  = data.PreSerials;
            var allSerials  = data.AllSerials;

            // ── A: Hammadde Girişleri ────────────────────────────────────────
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
            // preSerials / allSerials → data'dan gelir (BuildAuditReportData)
            var openingByType = preSerials
                .GroupBy(s => s.Lot?.FscType?.Name ?? "—")
                .ToDictionary(g => g.Key, g =>
                    g.Sum(s => s.InitialWeight
                        - s.ProductionDetails
                            .Where(d => d.ProductionDate < sd)
                            .Sum(d => d.ConsumedWeight + d.WasteWeight)));

            // Canlı stok (CurrentWeight — bugünkü gerçek değer, bilgi amaçlı)
            var currentStockByType = allSerials
                .GroupBy(s => s.Lot?.FscType?.Name ?? "—")
                .ToDictionary(g => g.Key, g => g.Sum(s => s.CurrentWeight));

            // Dönemde tüketilen miktar (üretim detaylarından)
            var consumedByType = prodDetails
                .GroupBy(d => d.FscSerial?.Lot?.FscType?.Name ?? "—")
                .ToDictionary(g => g.Key, g => (Consumed: g.Sum(d => d.ConsumedWeight), Waste: g.Sum(d => d.WasteWeight)));

            // YM donusum girisleri (FscLot.SourceSerialId != null): FSC tipine gore kg
            var ymInputByType = data.YmLots
                .GroupBy(l => l.FscType?.Name ?? "—")
                .ToDictionary(g => g.Key, g => g.Sum(l => l.Serials.Sum(s => s.InitialWeight)));

            // Donusum firesi: nullable ConversionFireKg toplami
            var convFireByType = data.YmLots
                .GroupBy(l => l.FscType?.Name ?? "—")
                .ToDictionary(g => g.Key, g => g.Sum(l => l.ConversionFireKg ?? 0m));

            ViewBag.YmInputByFscType       = ymInputByType;
            ViewBag.ConversionFireByFscType = convFireByType;

            // Tüm FSC tiplerini birleştir
            var allFscTypes = lots.Select(l => l.FscType?.Name ?? "—")
                .Union(preSerials.Select(s => s.Lot?.FscType?.Name ?? "—"))
                .Union(prodDetails.Select(d => d.FscSerial?.Lot?.FscType?.Name ?? "—"))
                .Union(data.YmLots.Select(l => l.FscType?.Name ?? "—"))
                .Distinct();

            var balanceRows = allFscTypes.Select(type => {
                    openingByType.TryGetValue(type, out var openingKg);
                    consumedByType.TryGetValue(type, out var cons);
                    currentStockByType.TryGetValue(type, out var currentStock);
                    ymInputByType.TryGetValue(type, out var ymInput);
                    convFireByType.TryGetValue(type, out var convFire);
                    // Sadece gercek satin alma lotlari (SourceSerialId null)
                    var inputKg   = lots.Where(l => l.FscType?.Name == type && l.SourceSerialId == null).Sum(l => l.Serials.Sum(s => s.InitialWeight));
                    // Kapanis = Acilis + HammaddeGirisi + YMDonusumGirisi - Tuketim - DonusumFire - UretimFire
                    var closingKg = openingKg + inputKg + ymInput - cons.Consumed - convFire - cons.Waste;
                    if (closingKg < 0) closingKg = 0;
                    return new AuditBalanceRow {
                        FscType          = type,
                        OpeningKg        = openingKg < 0 ? 0 : openingKg,
                        InputKg          = inputKg,
                        YmConversionKg   = ymInput,
                        ConversionFireKg = convFire,
                        ConsumedKg       = cons.Consumed,
                        WasteKg          = cons.Waste,
                        ClosingKg        = closingKg,
                        CurrentStockKg   = currentStock,
                        IsBalanced       = (cons.Consumed + cons.Waste + convFire) <= (openingKg + inputKg + ymInput) + 0.01m
                    };
                })
                .Where(r => r.OpeningKg > 0 || r.InputKg > 0 || r.ConsumedKg > 0 || r.YmConversionKg > 0)
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

            var data        = await BuildAuditReportData(sd, ed);
            var lots        = data.Lots;
            var prodDetails = data.ProdDetails;
            var salesLines  = data.SalesLines;

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
            // Açılış bakiyesi
            var openingMap2 = data.PreSerials
                .GroupBy(s => s.Lot?.FscType?.Name ?? "—")
                .ToDictionary(g => g.Key, g =>
                    g.Sum(s => s.InitialWeight - s.ProductionDetails
                        .Where(d => d.ProductionDate < sd)
                        .Sum(d => d.ConsumedWeight + d.WasteWeight)));
            var consumedMap2 = prodDetails.GroupBy(d => d.FscSerial?.Lot?.FscType?.Name ?? "—")
                .ToDictionary(g => g.Key, g => (Consumed: g.Sum(d => d.ConsumedWeight), Waste: g.Sum(d => d.WasteWeight)));
            var fscTypes2 = lots.Select(l => l.FscType?.Name ?? "—")
                .Union(data.PreSerials.Select(s => s.Lot?.FscType?.Name ?? "—"))
                .Union(prodDetails.Select(d => d.FscSerial?.Lot?.FscType?.Name ?? "—"))
                .Union(data.YmLots.Select(l => l.FscType?.Name ?? "—"))
                .Distinct();
            int r4 = 3;
            foreach (var ft in fscTypes2.OrderBy(x => x)) {
                var inputKg = lots.Where(l => l.FscType?.Name == ft && l.SourceSerialId == null).Sum(l => l.Serials.Sum(s => s.InitialWeight));
                openingMap2.TryGetValue(ft, out var openingKg2);
                if (openingKg2 < 0) openingKg2 = 0;
                consumedMap2.TryGetValue(ft, out var cons);
                var stockKg = data.AllSerials.Where(s => s.Lot?.FscType?.Name == ft).Sum(s => s.CurrentWeight);
                var ymInput2 = data.YmLots.Where(l => l.FscType?.Name == ft).Sum(l => l.Serials.Sum(s => s.InitialWeight));
                var convFire2 = data.YmLots.Where(l => l.FscType?.Name == ft).Sum(l => l.ConversionFireKg ?? 0m);
                var closingKg2 = openingKg2 + inputKg + ymInput2 - cons.Consumed - convFire2 - cons.Waste;
                if (closingKg2 < 0) closingKg2 = 0;
                var ok = (cons.Consumed + cons.Waste + convFire2) <= (openingKg2 + inputKg + ymInput2) + 0.01m;
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
                    // Her tarih grubunda Max al: aynı üretim günü içindeki tüm malzeme
                    // satırları aynı üretim adedini taşır → birden fazla güne bölünmüşse topla.
                    TotalProducedQty = w.ProductionDetails.Any()
                        ? w.ProductionDetails
                            .GroupBy(d => d.ProductionDate.Date)
                            .Sum(g => g.Max(d => d.ProducedQuantity))
                        : w.ActualQuantity,
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
                        ProducedQty   = unlinked
                            .GroupBy(d => d.ProductionDate.Date)
                            .Sum(g => g.Max(d => d.ProducedQuantity)),
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
                .Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.WorkOrderNo })
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
                    var totalProduced = w.ProductionDetails.Any()
                        ? w.ProductionDetails
                            .GroupBy(d => d.ProductionDate.Date)
                            .Sum(g => g.Max(d => d.ProducedQuantity))
                        : w.ActualQuantity;
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

        // ── Ortak veri yükleme yardımcısı ───────────────────────────────────────
        private async Task<AuditReportData> BuildAuditReportData(DateTime sd, DateTime ed)
        {
            var edNext = ed.AddDays(1);

            var lots = await _context.FscLots
                .Include(l => l.FscType)
                .Include(l => l.Supplier)
                .Include(l => l.Product)
                .Include(l => l.Serials)
                .Where(l => l.ArrivalDate >= sd && l.ArrivalDate < edNext)
                .OrderBy(l => l.FscType.Name).ThenBy(l => l.Supplier != null ? l.Supplier.Name : "").ThenBy(l => l.ArrivalDate)
                .ToListAsync();

            var prodDetails = await _context.ProductionDetails
                .Include(d => d.WorkOrder).ThenInclude(w => w.Product)
                .Include(d => d.FscSerial).ThenInclude(s => s.Lot).ThenInclude(l => l.FscType)
                .Include(d => d.FscSerial).ThenInclude(s => s.Lot).ThenInclude(l => l.Supplier)
                .Include(d => d.Machine)
                .Where(d => d.ProductionDate >= sd && d.ProductionDate < edNext)
                .OrderBy(d => d.ProductionDate)
                .ToListAsync();

            var salesLines = await _context.SalesOrderLines
                .Include(l => l.SalesOrder).ThenInclude(o => o.Customer)
                .Include(l => l.Product)
                .Include(l => l.WorkOrder)
                .Where(l => l.SalesOrder.Status == SalesOrderStatus.TeslimEdildi
                         && l.SalesOrder.DispatchDate >= sd
                         && l.SalesOrder.DispatchDate < edNext)
                .OrderBy(l => l.SalesOrder.DispatchDate)
                .ToListAsync();

            var preSerials = await _context.FscSerials
                .Include(s => s.Lot).ThenInclude(l => l.FscType)
                .Include(s => s.ProductionDetails)
                .Where(s => s.Lot.ArrivalDate < sd)
                .ToListAsync();

            var allSerials = await _context.FscSerials
                .Include(s => s.Lot).ThenInclude(l => l.FscType)
                .ToListAsync();

            // YM donusum cikti lotlari: SourceSerialId != null olan donem lotlari
            var ymLots = await _context.FscLots
                .Include(l => l.FscType)
                .Include(l => l.Serials)
                .Where(l => l.SourceSerialId != null && l.ArrivalDate >= sd && l.ArrivalDate < edNext)
                .ToListAsync();

            return new AuditReportData(lots, ymLots, prodDetails, salesLines, preSerials, allSerials);
        }

        // ── 9. Hammadde / Mamul / Lot İzleme ───────────────────────────────────
        // GET /Reports/MaterialTrace
        public async Task<IActionResult> MaterialTrace(
            string  mode      = "hammadde",
            int?    hammaddeId = null,
            int?    mamulId    = null,
            string? partiNo   = null,
            string? serialNo  = null,
            DateTime? startDate = null,
            DateTime? endDate   = null)
        {
            var sd = startDate ?? new DateTime(DateTime.Today.Year, 1, 1);
            var ed = endDate   ?? DateTime.Today;

            // Hammadde dropdown: FscLot.ProductId üzerinden gelen ürünler
            var hammaddeProductIds = await _context.FscLots
                .Where(l => l.ProductId != null)
                .Select(l => l.ProductId!.Value)
                .Distinct().ToListAsync();
            ViewBag.Hammaddeler = await _context.Products
                .Where(p => hammaddeProductIds.Contains(p.Id))
                .OrderBy(p => p.ProductName).ToListAsync();

            // Mamul dropdown: WorkOrder.ProductId üzerinden gelen ürünler
            var mamulProductIds = await _context.WorkOrders
                .Select(w => w.ProductId).Distinct().ToListAsync();
            ViewBag.Mamuller = await _context.Products
                .Where(p => mamulProductIds.Contains(p.Id))
                .OrderBy(p => p.ProductName).ToListAsync();

            ViewBag.Mode       = mode;
            ViewBag.HammaddeId = hammaddeId;
            ViewBag.MamulId    = mamulId;
            ViewBag.PartiNo    = partiNo?.Trim();
            ViewBag.SerialNo   = serialNo?.Trim();
            ViewBag.StartDate  = sd.ToString("yyyy-MM-dd");
            ViewBag.EndDate    = ed.ToString("yyyy-MM-dd");

            bool hasFilter = mode switch {
                "hammadde" => hammaddeId.HasValue,
                "mamul"    => mamulId.HasValue,
                "parti"    => !string.IsNullOrWhiteSpace(partiNo) || !string.IsNullOrWhiteSpace(serialNo),
                _          => false
            };

            ViewData["Title"] = mode switch {
                "mamul" => "Mamul Hammadde Analizi",
                "parti" => "Parti / Lot Kullanım İzleme",
                _       => "Hammadde Kullanım İzleme"
            };

            if (!hasFilter)
                return View(new MaterialTraceModel { Mode = mode, StartDate = sd, EndDate = ed });

            // ── Ana sorgu ──────────────────────────────────────────────────────
            var query = _context.ProductionDetails
                .Include(pd => pd.FscSerial).ThenInclude(s => s.Lot).ThenInclude(l => l.Product)
                .Include(pd => pd.FscSerial).ThenInclude(s => s.Lot).ThenInclude(l => l.FscType)
                .Include(pd => pd.FscSerial).ThenInclude(s => s.Lot).ThenInclude(l => l.Supplier)
                .Include(pd => pd.WorkOrder).ThenInclude(w => w.Product)
                .Include(pd => pd.WorkOrder).ThenInclude(w => w.Machine)
                .AsQueryable();

            switch (mode)
            {
                case "hammadde":
                    query = query.Where(pd => pd.FscSerial != null
                        && pd.FscSerial.Lot != null
                        && pd.FscSerial.Lot.ProductId == hammaddeId!.Value);
                    query = query.Where(pd => pd.ProductionDate >= sd && pd.ProductionDate < ed.AddDays(1));
                    if (!string.IsNullOrWhiteSpace(partiNo))
                        query = query.Where(pd => pd.FscSerial!.Lot.PartiNo.Contains(partiNo.Trim()));
                    break;

                case "mamul":
                    query = query.Where(pd => pd.WorkOrder != null
                        && pd.WorkOrder.ProductId == mamulId!.Value);
                    query = query.Where(pd => pd.ProductionDate >= sd && pd.ProductionDate < ed.AddDays(1));
                    break;

                case "parti":
                    if (!string.IsNullOrWhiteSpace(partiNo))
                    {
                        var t = partiNo.Trim();
                        query = query.Where(pd => pd.FscSerial != null
                            && pd.FscSerial.Lot != null
                            && pd.FscSerial.Lot.PartiNo.Contains(t));
                    }
                    if (!string.IsNullOrWhiteSpace(serialNo))
                    {
                        var t = serialNo.Trim();
                        query = query.Where(pd => pd.FscSerial != null
                            && pd.FscSerial.SerialNo.Contains(t));
                    }
                    if (startDate.HasValue) query = query.Where(pd => pd.ProductionDate >= sd);
                    if (endDate.HasValue)   query = query.Where(pd => pd.ProductionDate < ed.AddDays(1));
                    break;
            }

            var data = await query
                .OrderBy(pd => pd.ProductionDate)
                .ThenBy(pd => pd.WorkOrder!.WorkOrderNo)
                .ThenBy(pd => pd.FscSerial!.Lot.PartiNo)
                .ToListAsync();

            var rows = data.Select(pd => new MaterialTraceRow
            {
                HammaddeAdi  = pd.FscSerial?.Lot?.Product?.ProductName ?? "—",
                HammaddeKodu = pd.FscSerial?.Lot?.Product?.ProductCode ?? "",
                PartiNo      = pd.FscSerial?.Lot?.PartiNo ?? "—",
                SerialNo     = pd.FscSerial?.SerialNo ?? "—",
                FscTipi      = pd.FscSerial?.Lot?.FscType?.Name ?? "—",
                Tedarikci    = pd.FscSerial?.Lot?.Supplier?.Name ?? "—",
                GelisTarihi  = pd.FscSerial?.Lot?.ArrivalDate,
                WorkOrderId  = pd.WorkOrderId,
                WorkOrderNo  = pd.WorkOrder?.WorkOrderNo ?? "—",
                MamulAdi     = pd.WorkOrder?.Product?.ProductName ?? "—",
                MamulKodu    = pd.WorkOrder?.Product?.ProductCode ?? "",
                MakineName   = pd.WorkOrder?.Machine?.Name ?? "—",
                UretimTarihi = pd.ProductionDate,
                TuketilenKg  = pd.ConsumedWeight,
                FireKg       = pd.WasteWeight,
                UretilenAdet = pd.ProducedQuantity
            }).ToList();

            var model = new MaterialTraceModel
            {
                Mode       = mode,
                HammaddeId = hammaddeId,
                MamulId    = mamulId,
                PartiNo    = partiNo?.Trim(),
                SerialNo   = serialNo?.Trim(),
                StartDate  = sd,
                EndDate    = ed,
                Rows       = rows
            };
            return View(model);
        }

        // GET /Reports/ExportMaterialTrace
        public async Task<IActionResult> ExportMaterialTrace(
            string  mode      = "hammadde",
            int?    hammaddeId = null,
            int?    mamulId    = null,
            string? partiNo   = null,
            string? serialNo  = null,
            DateTime? startDate = null,
            DateTime? endDate   = null)
        {
            var sd = startDate ?? new DateTime(DateTime.Today.Year, 1, 1);
            var ed = endDate   ?? DateTime.Today;

            var query = _context.ProductionDetails
                .Include(pd => pd.FscSerial).ThenInclude(s => s.Lot).ThenInclude(l => l.Product)
                .Include(pd => pd.FscSerial).ThenInclude(s => s.Lot).ThenInclude(l => l.FscType)
                .Include(pd => pd.FscSerial).ThenInclude(s => s.Lot).ThenInclude(l => l.Supplier)
                .Include(pd => pd.WorkOrder).ThenInclude(w => w.Product)
                .Include(pd => pd.WorkOrder).ThenInclude(w => w.Machine)
                .AsQueryable();

            switch (mode)
            {
                case "hammadde":
                    if (!hammaddeId.HasValue) return BadRequest();
                    query = query.Where(pd => pd.FscSerial != null && pd.FscSerial.Lot != null && pd.FscSerial.Lot.ProductId == hammaddeId.Value);
                    query = query.Where(pd => pd.ProductionDate >= sd && pd.ProductionDate < ed.AddDays(1));
                    if (!string.IsNullOrWhiteSpace(partiNo)) query = query.Where(pd => pd.FscSerial!.Lot.PartiNo.Contains(partiNo.Trim()));
                    break;
                case "mamul":
                    if (!mamulId.HasValue) return BadRequest();
                    query = query.Where(pd => pd.WorkOrder != null && pd.WorkOrder.ProductId == mamulId.Value);
                    query = query.Where(pd => pd.ProductionDate >= sd && pd.ProductionDate < ed.AddDays(1));
                    break;
                case "parti":
                    if (!string.IsNullOrWhiteSpace(partiNo)) query = query.Where(pd => pd.FscSerial != null && pd.FscSerial.Lot != null && pd.FscSerial.Lot.PartiNo.Contains(partiNo.Trim()));
                    if (!string.IsNullOrWhiteSpace(serialNo)) query = query.Where(pd => pd.FscSerial != null && pd.FscSerial.SerialNo.Contains(serialNo.Trim()));
                    if (startDate.HasValue) query = query.Where(pd => pd.ProductionDate >= sd);
                    if (endDate.HasValue)   query = query.Where(pd => pd.ProductionDate < ed.AddDays(1));
                    break;
            }

            var data = await query
                .OrderBy(pd => pd.ProductionDate)
                .ThenBy(pd => pd.WorkOrder!.WorkOrderNo)
                .ToListAsync();

            var rows = data.Select(pd => new {
                UretimTarihi = pd.ProductionDate.ToString("dd.MM.yyyy"),
                IsEmriNo     = pd.WorkOrder?.WorkOrderNo ?? "",
                Mamul        = pd.WorkOrder?.Product?.ProductName ?? "",
                MamulKodu    = pd.WorkOrder?.Product?.ProductCode ?? "",
                Makine       = pd.WorkOrder?.Machine?.Name ?? "",
                Hammadde     = pd.FscSerial?.Lot?.Product?.ProductName ?? "",
                PartiNo      = pd.FscSerial?.Lot?.PartiNo ?? "",
                SeriNo       = pd.FscSerial?.SerialNo ?? "",
                FscTipi      = pd.FscSerial?.Lot?.FscType?.Name ?? "",
                Tedarikci    = pd.FscSerial?.Lot?.Supplier?.Name ?? "",
                GelisTarihi  = pd.FscSerial?.Lot?.ArrivalDate.ToString("dd.MM.yyyy") ?? "",
                TuketilenKg  = pd.ConsumedWeight,
                FireKg       = pd.WasteWeight,
                UretilenAdet = pd.ProducedQuantity
            }).ToList();

            var fileName = mode switch {
                "mamul" => "MamulHammaddeAnalizi",
                "parti" => $"PartiKullanim_{partiNo ?? ""}",
                _       => "HammaddeKullanimIzleme"
            };
            return ExportToExcel(rows, fileName);
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
                    // Her iş emrini × gün için Max al → toplam gerçek üretimi bul
                    TotalProducedQty = g
                        .GroupBy(d => new { d.WorkOrderId, d.ProductionDate.Date })
                        .Sum(wg => wg.Max(d => d.ProducedQuantity)),
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

        // ── Uyarı Paneli ────────────────────────────────────────────────────────
        // GET /Reports/Warnings
        public async Task<IActionResult> Warnings()
        {
            var today = DateTime.Today;
            var soon  = today.AddDays(30);

            // Eksik belgeli lotlar (fatura NO veya irsaliye NO eksik, VEYA her iki PDF de yok)
            var missingDocLots = await _context.FscLots
                .Include(l => l.Supplier)
                .Include(l => l.Product)
                .Where(l => string.IsNullOrEmpty(l.InvoiceNo) ||
                            string.IsNullOrEmpty(l.DispatchNo) ||
                            (l.InvoicePdfPath == null && l.DispatchPdfPath == null))
                .OrderByDescending(l => l.ArrivalDate)
                .ToListAsync();

            // Süresi dolmuş tedarikçi FSC sertifikaları
            var expiredSuppliers = await _context.Suppliers
                .Where(s => s.IsActive && s.FscExpiryDate != null && s.FscExpiryDate < today)
                .OrderBy(s => s.FscExpiryDate)
                .ToListAsync();

            // 30 gün içinde dolacak tedarikçi FSC sertifikaları
            var expiringSoonSuppliers = await _context.Suppliers
                .Where(s => s.IsActive && s.FscExpiryDate != null &&
                            s.FscExpiryDate >= today && s.FscExpiryDate <= soon)
                .OrderBy(s => s.FscExpiryDate)
                .ToListAsync();

            // FSC kodu tanımlanmamış aktif tedarikçiler
            var noFscSuppliers = await _context.Suppliers
                .Where(s => s.IsActive && string.IsNullOrEmpty(s.FscCode))
                .OrderBy(s => s.Name)
                .ToListAsync();

            // Süresi dolmuş müşteri FSC lisansları
            var expiredCustomers = await _context.Customers
                .Where(c => c.IsActive && c.FscExpiryDate != null && c.FscExpiryDate < today)
                .OrderBy(c => c.FscExpiryDate)
                .ToListAsync();

            // 30 gün içinde dolacak müşteri FSC lisansları
            var expiringSoonCustomers = await _context.Customers
                .Where(c => c.IsActive && c.FscExpiryDate != null &&
                            c.FscExpiryDate >= today && c.FscExpiryDate <= soon)
                .OrderBy(c => c.FscExpiryDate)
                .ToListAsync();

            ViewBag.MissingDocLots        = missingDocLots;
            ViewBag.ExpiredSuppliers      = expiredSuppliers;
            ViewBag.ExpiringSoonSuppliers = expiringSoonSuppliers;
            ViewBag.NoFscSuppliers        = noFscSuppliers;
            ViewBag.ExpiredCustomers      = expiredCustomers;
            ViewBag.ExpiringSoonCustomers = expiringSoonCustomers;

            int criticalCount = expiredSuppliers.Count + expiredCustomers.Count;
            int warningCount  = expiringSoonSuppliers.Count + expiringSoonCustomers.Count + noFscSuppliers.Count;
            ViewBag.CriticalCount   = criticalCount;
            ViewBag.WarningCount    = warningCount;
            ViewBag.MissingDocCount = missingDocLots.Count;
            ViewBag.TotalCount      = criticalCount + warningCount + missingDocLots.Count;

            ViewData["Title"] = "Uyarı Paneli";
            return View();
        }

        // GET /Reports/FscConsumption — Yıllık FSC CoC tüketim denetim raporu (mamul → kategori → bileşen)
        public async Task<IActionResult> FscConsumption(DateTime? startDate, DateTime? endDate, int? productId)
        {
            var sd = startDate ?? new DateTime(DateTime.Today.Year, 1, 1);
            var ed = endDate ?? DateTime.Today;
            var edNext = ed.AddDays(1);

            var q = _context.ProductionDetails
                .Include(d => d.WorkOrder).ThenInclude(w => w.Product)
                .Include(d => d.FscSerial).ThenInclude(s => s.Lot).ThenInclude(l => l.Product).ThenInclude(p => p!.ProductGroup)
                .Include(d => d.FscSerial).ThenInclude(s => s.Lot).ThenInclude(l => l.FscType)
                .Where(d => d.ProductionDate >= sd && d.ProductionDate < edNext && d.WorkOrder.Status != WorkOrderStatus.Taslak);
            if (productId.HasValue) q = q.Where(d => d.WorkOrder.ProductId == productId.Value);
            var details = await q.ToListAsync();

            var mamuller = details
                .GroupBy(d => new { d.WorkOrder.ProductId, d.WorkOrder.Product })
                .Select(mg =>
                {
                    var m = new FscConsMamulRow
                    {
                        MamulKod    = mg.Key.Product?.ExternalCode ?? mg.Key.Product?.ProductCode ?? "—",
                        MamulAd     = mg.Key.Product?.ProductName ?? "—",
                        ProducedQty = mg.GroupBy(d => d.WorkOrderId)
                            .Sum(wg => wg.GroupBy(d => d.ProductionDate.Date).Sum(dg => dg.Max(d => d.ProducedQuantity)))
                    };
                    m.Components = mg
                        .GroupBy(d => new { Comp = d.FscSerial?.Lot?.Product, Fsc = d.FscSerial?.Lot?.FscType?.Name })
                        .Select(cg => new FscConsComponentRow
                        {
                            Category     = CategorizeComponent(cg.Key.Comp),
                            ComponentKod = cg.Key.Comp?.ExternalCode ?? cg.Key.Comp?.ProductCode ?? "—",
                            ComponentAd  = cg.Key.Comp?.ProductName ?? "—",
                            FscType      = cg.Key.Fsc ?? "—",
                            ConsumedKg   = cg.Sum(d => d.ConsumedWeight),
                            WasteKg      = cg.Sum(d => d.WasteWeight)
                        })
                        .OrderBy(c => c.Category).ThenBy(c => c.ComponentAd).ToList();
                    return m;
                })
                .OrderBy(m => m.MamulAd).ToList();

            var rollup = mamuller.SelectMany(m => m.Components)
                .GroupBy(c => c.Category)
                .Select(g => new FscConsCategoryRow { Category = g.Key, ConsumedKg = g.Sum(c => c.ConsumedKg), WasteKg = g.Sum(c => c.WasteKg) })
                .OrderByDescending(r => r.ConsumedKg).ToList();

            var model = new FscConsumptionModel
            {
                StartDate     = sd,
                EndDate       = ed,
                Mamuller      = mamuller,
                CategoryRollup = rollup,
                TotalConsumed = mamuller.Sum(m => m.ConsumedKg),
                TotalWaste    = mamuller.Sum(m => m.WasteKg),
                TotalProduced = (int)mamuller.Sum(m => m.ProducedQty)
            };

            // Donem acilis/kapanis bakiyesi hesabi
            var _openingSerials = await _context.FscSerials
                .Include(s => s.Lot)
                .Where(s => s.Lot.ArrivalDate < sd && s.Lot.SourceSerialId == null)
                .ToListAsync();
            var _openingKg = _openingSerials.Sum(s => s.CurrentWeight);
            var _periodInputKg = await _context.FscSerials
                .Where(s => s.Lot.ArrivalDate >= sd && s.Lot.ArrivalDate < edNext && s.Lot.SourceSerialId == null)
                .SumAsync(s => s.InitialWeight);
            var _closingKg = await _context.FscSerials
                .Where(s => s.Lot.SourceSerialId == null)
                .SumAsync(s => s.CurrentWeight);
            ViewBag.OpeningKg        = _openingKg;
            ViewBag.PeriodInputKg    = _periodInputKg;
            ViewBag.PeriodConsumedKg = model.TotalConsumed;
            ViewBag.PeriodFireKg     = model.TotalWaste;
            ViewBag.ClosingKg        = _closingKg;

            ViewBag.StartDate = sd.ToString("yyyy-MM-dd");
            ViewBag.EndDate   = ed.ToString("yyyy-MM-dd");
            ViewBag.Products  = await _context.Products
                .Where(p => p.ExternalCode != null && p.ExternalCode.StartsWith("3"))
                .OrderBy(p => p.ProductName)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = (p.ExternalCode ?? "") + " — " + p.ProductName })
                .ToListAsync();
            ViewBag.ProductId = productId;
            ViewData["Title"] = "FSC Tüketim Denetim Raporu";
            return View(model);
        }

        // GET /Reports/ExportFscConsumption
        public async Task<IActionResult> ExportFscConsumption(DateTime? startDate, DateTime? endDate, int? productId)
        {
            var result = await FscConsumption(startDate, endDate, productId) as ViewResult;
            var model = result?.Model as FscConsumptionModel ?? new FscConsumptionModel();
            var rows = model.Mamuller.SelectMany(m => m.Components.Select(c => new
            {
                Mamul       = $"{m.MamulKod} — {m.MamulAd}",
                UretilenAdet = m.ProducedQty,
                Kategori    = c.Category,
                BilesenKod  = c.ComponentKod,
                Bilesen     = c.ComponentAd,
                FscTipi     = c.FscType,
                TuketilenKg = c.ConsumedKg,
                FireKg      = c.WasteKg
            })).ToList();
            return ExportToExcel(rows, "FSC_Tuketim_Raporu");
        }

        // Bileşen kategorisi sınıflandırıcı (genelleştirilebilir):
        //  1) Firma ProductGroup tanımladıysa onu kullan (her sektör kendi kategorisini tanımlar)
        //  2) Grup yoksa ad/kod deseninden tahmin (FSCTakip kraft torba varsayılanı)
        private static string CategorizeComponent(FSCTakip.Core.Entities.Product? p)
        {
            if (p == null) return "Diğer";
            if (!string.IsNullOrWhiteSpace(p.ProductGroup?.GroupName))
                return p.ProductGroup!.GroupName;
            var tr  = new System.Globalization.CultureInfo("tr-TR");
            var ad  = (p.ProductName ?? "").ToUpper(tr);
            var kod = p.ExternalCode ?? "";
            if (ad.Contains("ETİKET")) return "Etiket Bobini";
            if (ad.Contains("BURGU SAP")) return "Burgu Sap";
            if (ad.Contains("SAP BOB")) return "Sap Bobini";
            if (ad.Contains("TUTKAL")) return "Tutkal";
            if (ad.Contains("KOLİ")) return "Koli";
            if (ad.Contains("BOYA")) return "Boya";
            if (ad.StartsWith("BB ") || kod.StartsWith("23")) return "Gövde (BB)";
            if (kod.StartsWith("24") || ad.StartsWith("YM ")) return "Ara Yarı Mamül";
            if (kod.StartsWith("1")) return "Ham Kağıt";
            return "Diğer";
        }
    }

    // ── FSC Tüketim Denetim Raporu modelleri ───────────────────────────────────
    public class FscConsumptionModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate   { get; set; }
        public List<FscConsMamulRow>    Mamuller       { get; set; } = new();
        public List<FscConsCategoryRow> CategoryRollup { get; set; } = new();
        public decimal TotalConsumed { get; set; }
        public decimal TotalWaste    { get; set; }
        public int     TotalProduced { get; set; }
    }

    public class FscConsMamulRow
    {
        public string MamulKod { get; set; } = "";
        public string MamulAd  { get; set; } = "";
        public decimal ProducedQty { get; set; }
        public List<FscConsComponentRow> Components { get; set; } = new();
        public decimal ConsumedKg => Components.Sum(c => c.ConsumedKg);
        public decimal WasteKg    => Components.Sum(c => c.WasteKg);
    }

    public class FscConsComponentRow
    {
        public string Category     { get; set; } = "";
        public string ComponentKod { get; set; } = "";
        public string ComponentAd  { get; set; } = "";
        public string FscType      { get; set; } = "";
        public decimal ConsumedKg  { get; set; }
        public decimal WasteKg     { get; set; }
    }

    public class FscConsCategoryRow
    {
        public string Category   { get; set; } = "";
        public decimal ConsumedKg { get; set; }
        public decimal WasteKg    { get; set; }
    }

    // ── View Model Sınıfları ──────────────────────────────────────────────────

    /// <summary>
    /// AuditReport ve ExportAuditReport arasında paylaşılan ham veri seti.
    /// BuildAuditReportData() tarafından doldurulur.
    /// </summary>
    public record AuditReportData(
        List<FscLot>           Lots,
        List<FscLot>           YmLots,
        List<ProductionDetail> ProdDetails,
        List<SalesOrderLine>   SalesLines,
        List<FscSerial>        PreSerials,
        List<FscSerial>        AllSerials);

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
        public string  FscType          { get; set; } = "";
        public decimal OpeningKg        { get; set; }  // donem basi devir bakiye
        public decimal InputKg          { get; set; }  // donem hammadde girisi (satin alma)
        public decimal YmConversionKg   { get; set; }  // YM donusum girisi
        public decimal ConversionFireKg { get; set; }  // donusum firesi
        public decimal ConsumedKg       { get; set; }  // donem uretim tuketimi
        public decimal WasteKg          { get; set; }  // donem uretim firesi
        public decimal ClosingKg        { get; set; }  // donem sonu kapanis (hesaplanan)
        public decimal CurrentStockKg   { get; set; }  // canli stok (CurrentWeight)
        public bool    IsBalanced        { get; set; }
    }

    public class LotTraceResult
    {
        public FscLot Lot                                    { get; set; } = null!;
        public List<FscSerial> Serials                       { get; set; } = new();
        public List<SalesOrderLine> SalesLines               { get; set; } = new();
        public List<LotTraceProductionRow> ProductionRows    { get; set; } = new();

        public decimal TotalConsumedKg  => ProductionRows.Sum(r => r.ConsumedKg);
        public decimal TotalWasteKg     => ProductionRows.Sum(r => r.WasteKg);
        public int     TotalWorkOrders  => ProductionRows.Select(r => r.WorkOrderId).Distinct().Count();
    }

    public class LotTraceProductionRow
    {
        public int      WorkOrderId    { get; set; }
        public string   WorkOrderNo    { get; set; } = "";
        public string   ProductName    { get; set; } = "";
        public string   ProductCode    { get; set; } = "";
        public string   MachineName    { get; set; } = "";
        public DateTime ProductionDate { get; set; }
        public string   SerialNos      { get; set; } = "";
        public decimal  ConsumedKg     { get; set; }
        public decimal  WasteKg        { get; set; }
        public decimal  ProducedQty    { get; set; }
        public decimal  FireRate       => ConsumedKg > 0 ? WasteKg / ConsumedKg * 100 : 0;
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

    // ── MaterialTrace View Models ──────────────────────────────────────────────
    public class MaterialTraceModel
    {
        public string    Mode       { get; set; } = "hammadde";
        public int?      HammaddeId { get; set; }
        public int?      MamulId    { get; set; }
        public string?   PartiNo    { get; set; }
        public string?   SerialNo   { get; set; }
        public DateTime  StartDate  { get; set; }
        public DateTime  EndDate    { get; set; }
        public List<MaterialTraceRow> Rows { get; set; } = new();

        public decimal TotalConsumedKg => Rows.Sum(r => r.TuketilenKg);
        public decimal TotalWasteKg    => Rows.Sum(r => r.FireKg);
        public int     TotalWorkOrders => Rows.Select(r => r.WorkOrderId).Distinct().Count();
        public int     TotalLots       => Rows.Select(r => r.PartiNo).Distinct().Count();
        public int     TotalSerials    => Rows.Select(r => r.SerialNo).Distinct().Count();
    }

    public class MaterialTraceRow
    {
        public string    HammaddeAdi  { get; set; } = "";
        public string    HammaddeKodu { get; set; } = "";
        public string    PartiNo      { get; set; } = "";
        public string    SerialNo     { get; set; } = "";
        public string    FscTipi      { get; set; } = "";
        public string    Tedarikci    { get; set; } = "";
        public DateTime? GelisTarihi  { get; set; }
        public int       WorkOrderId  { get; set; }
        public string    WorkOrderNo  { get; set; } = "";
        public string    MamulAdi     { get; set; } = "";
        public string    MamulKodu    { get; set; } = "";
        public string    MakineName   { get; set; } = "";
        public DateTime  UretimTarihi { get; set; }
        public decimal   TuketilenKg  { get; set; }
        public decimal   FireKg       { get; set; }
        public decimal   UretilenAdet { get; set; }
        public decimal   FireRate     => TuketilenKg > 0 ? FireKg / TuketilenKg * 100 : 0;
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
