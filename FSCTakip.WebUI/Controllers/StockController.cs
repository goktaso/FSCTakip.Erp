using ClosedXML.Excel;
using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using FSCTakip.WebUI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Controllers
{
    public class StockController : BaseController
    {
        public StockController(AppDbContext context) : base(context) { }

        // GET /Stock/Summary — standart stok ozeti (HAMMADDE+BURGU SAP+YARI MAMUL, KG bazli)
        // Kaynak: StockMovements.QuantityKg — FscSerials.CurrentWeight kullanilmaz (eski kayitlar MT degerini KG olarak sakliyor)
        public async Task<IActionResult> Summary(int[]? groupIds, int? fscTypeId, string? search, int[]? productIds)
        {
            var allGroups = await _context.ProductGroups.OrderBy(g => g.GroupName).ToListAsync();

            // Default: HAMMADDE(1), BURGU SAP(4), YARI MAMUL(3)
            var defaultIds = new[] { 1, 4, 3 };
            var selectedIds = (groupIds != null && groupIds.Length > 0) ? groupIds : defaultIds;

            var mvQuery = _context.StockMovements
                .Include(m => m.Product).ThenInclude(p => p!.ProductGroup)
                .Where(m => m.Product != null &&
                            m.Product.ProductGroupId != null &&
                            selectedIds.Contains(m.Product.ProductGroupId!.Value));

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLower();
                mvQuery = mvQuery.Where(m => m.Product!.ProductCode.ToLower().Contains(q) ||
                    (m.Product.ExternalCode != null && m.Product.ExternalCode.ToLower().Contains(q)) ||
                    m.Product.ProductName.ToLower().Contains(q));
            }

            var movements = await mvQuery.ToListAsync();

            // Uretim tuketimi (consumed + fire) urun bazinda — StockMovements.Quantity'ye guvenme (eski kayitlar fire icermiyor)
            // ProductionDetails uzerinden dogru toplami hesapla
            var productionConsumptionByProduct = await _context.ProductionDetails
                .Include(d => d.FscSerial).ThenInclude(s => s.Lot)
                .Where(d => d.FscSerial != null && d.FscSerial.Lot.ProductId != null)
                .GroupBy(d => d.FscSerial!.Lot.ProductId!.Value)
                .Select(g => new { ProductId = g.Key, TotalKg = g.Sum(d => d.ConsumedWeight + d.WasteWeight) })
                .ToDictionaryAsync(x => x.ProductId, x => x.TotalKg);

            // FscLot sayilari (lot/serial count icin)
            var lotQuery = _context.FscLots
                .Include(l => l.Product)
                .Include(l => l.Serials)
                .Where(l => l.Product != null &&
                            l.Product.ProductGroupId != null &&
                            selectedIds.Contains(l.Product.ProductGroupId!.Value));
            if (!string.IsNullOrWhiteSpace(search))
            {
                var q2 = search.Trim().ToLower();
                lotQuery = lotQuery.Where(l => l.Product!.ProductCode.ToLower().Contains(q2) ||
                    (l.Product.ExternalCode != null && l.Product.ExternalCode.ToLower().Contains(q2)) ||
                    l.Product.ProductName.ToLower().Contains(q2));
            }
            var lots = await lotQuery.ToListAsync();
            var lotsByProduct   = lots.GroupBy(l => l.ProductId).ToDictionary(g => g.Key ?? 0, g => g.Count());
            var serialsByProduct = lots.GroupBy(l => l.ProductId).ToDictionary(g => g.Key ?? 0, g => g.Sum(l => l.Serials.Count));

            var rows = movements
                .GroupBy(m => m.ProductId)
                .Select(g =>
                {
                    var prod = g.First().Product!;
                    var unit = (prod.Unit ?? "KG").Trim().ToUpperInvariant();

                    // KG bazli net hesap
                    var inboundKg  = g.Where(m => m.Type == MovementType.PurchaseEntry || m.Type == MovementType.ProductionEntry)
                                      .Sum(m => m.QuantityKg ?? m.Quantity);
                    // Satis cikisi StockMovements'tan, uretim tuketimi ProductionDetails'tan (consumed+fire dogru toplam)
                    var salesOutKg = g.Where(m => m.Type == MovementType.SalesDispatch)
                                      .Sum(m => m.QuantityKg ?? m.Quantity);
                    var prodConsumKg = productionConsumptionByProduct.TryGetValue(g.Key, out var pc) ? pc : 0m;
                    var outboundKg = salesOutKg + prodConsumKg;
                    var netKg = inboundKg - outboundKg;

                    // Orijinal birimde giriş toplami (sadece MT/ADET gibi durumlar icin)
                    bool hasConv = unit != "KG";
                    decimal? origIn  = hasConv ? g.Where(m => m.Type == MovementType.PurchaseEntry || m.Type == MovementType.ProductionEntry)
                                                  .Sum(m => m.Quantity) : null;
                    decimal? origOut = hasConv ? g.Where(m => m.Type == MovementType.SalesDispatch || m.Type == MovementType.ProductionConsumption)
                                                  .Sum(m => m.Quantity) : null;
                    decimal? origNet = (origIn.HasValue) ? origIn - (origOut ?? 0) : null;

                    lotsByProduct.TryGetValue(g.Key, out int lotCnt);
                    serialsByProduct.TryGetValue(g.Key, out int serialCnt);

                    return new StockSummaryItem
                    {
                        ProductId     = g.Key,
                        ExternalCode  = prod.ExternalCode ?? "",
                        ProductCode   = prod.ProductCode,
                        ProductName   = prod.ProductName,
                        GroupName     = prod.ProductGroup?.GroupName ?? "—",
                        GroupId       = prod.ProductGroupId ?? 0,
                        Unit          = unit,
                        TotalKg       = netKg,
                        OriginalTotal = origNet,
                        LotCount      = lotCnt,
                        SerialCount   = serialCnt
                    };
                })
                .Where(r => r.TotalKg > 0)
                .OrderBy(r => r.GroupName).ThenBy(r => r.ProductName)
                .ToList();

            if (productIds != null && productIds.Length > 0)
                rows = rows.Where(r => productIds.Contains(r.ProductId)).ToList();

            ViewBag.ProductGroups    = allGroups;
            ViewBag.FscTypes         = await _context.FscTypes.Where(f => f.IsActive).ToListAsync();
            ViewBag.SelectedGroupIds = selectedIds;
            ViewBag.FscTypeId        = fscTypeId;
            ViewBag.Search           = search;
            ViewBag.ProductIds       = productIds ?? Array.Empty<int>();
            ViewBag.AllProducts      = await _context.Products
                .Where(p => p.IsActive && p.ProductGroupId != null && selectedIds.Contains(p.ProductGroupId!.Value))
                .OrderBy(p => p.ProductName)
                .ToListAsync();
            ViewBag.TotalKg          = rows.Sum(r => r.TotalKg);
            ViewBag.TotalProducts    = rows.Count;
            ViewBag.TotalLots        = rows.Sum(r => r.LotCount);
            ViewBag.TotalSerials     = rows.Sum(r => r.SerialCount);

            // FSC Kütle Dengesi — merkezi servis
            (await FscMassBalanceService.ComputeAsync(_context)).ApplyToViewData(ViewData);

            return View(rows);
        }

        // GET /Stock/AdminStock — TUM stok hareketleri, orijinal birimde (sadece admin)
        public async Task<IActionResult> AdminStock(int? productGroupId, string? search, int[]? productIds)
        {
            var allGroups = await _context.ProductGroups.OrderBy(g => g.GroupName).ToListAsync();

            var mvQuery = _context.StockMovements
                .Include(m => m.Product).ThenInclude(p => p!.ProductGroup)
                .AsQueryable();

            if (productGroupId.HasValue)
                mvQuery = mvQuery.Where(m => m.Product != null && m.Product.ProductGroupId == productGroupId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLower();
                mvQuery = mvQuery.Where(m => m.Product != null && (
                    m.Product.ProductCode.ToLower().Contains(q) ||
                    (m.Product.ExternalCode != null && m.Product.ExternalCode.ToLower().Contains(q)) ||
                    m.Product.ProductName.ToLower().Contains(q)));
            }

            if (productIds != null && productIds.Length > 0)
                mvQuery = mvQuery.Where(m => productIds.Contains(m.ProductId));

            var movements = await mvQuery.ToListAsync();

            // Uretim tuketimi + fire urun bazinda (eski SM kayitlari fire icermiyor)
            var adminProdConsByProduct = await _context.ProductionDetails
                .Include(d => d.FscSerial).ThenInclude(s => s.Lot)
                .Where(d => d.FscSerial != null && d.FscSerial.Lot.ProductId != null)
                .GroupBy(d => d.FscSerial!.Lot.ProductId!.Value)
                .Select(g => new { ProductId = g.Key, TotalKg = g.Sum(d => d.ConsumedWeight + d.WasteWeight) })
                .ToDictionaryAsync(x => x.ProductId, x => x.TotalKg);

            var rows = movements
                .GroupBy(m => m.ProductId)
                .Select(g =>
                {
                    var prod = g.First().Product!;
                    var unit = (prod.Unit ?? "KG").Trim().ToUpperInvariant();
                    bool hasConv = unit != "KG";

                    var inOrig  = g.Where(m => m.Type == MovementType.PurchaseEntry || m.Type == MovementType.ProductionEntry).Sum(m => m.Quantity);
                    var salesOut = g.Where(m => m.Type == MovementType.SalesDispatch).Sum(m => m.Quantity);
                    var prodConsumKg2 = adminProdConsByProduct.TryGetValue(g.Key, out var pc2) ? pc2 : 0m;
                    var outOrig = salesOut + prodConsumKg2;
                    var inKg    = g.Where(m => m.Type == MovementType.PurchaseEntry || m.Type == MovementType.ProductionEntry).Sum(m => m.QuantityKg ?? m.Quantity);
                    var outKg   = g.Where(m => m.Type == MovementType.SalesDispatch).Sum(m => m.QuantityKg ?? m.Quantity) + prodConsumKg2;

                    return new AdminStockItem
                    {
                        ProductId    = g.Key,
                        ExternalCode = prod.ExternalCode ?? "",
                        ProductCode  = prod.ProductCode,
                        ProductName  = prod.ProductName,
                        GroupName    = prod.ProductGroup?.GroupName ?? "—",
                        Unit         = unit,
                        InQty        = inOrig,
                        OutQty       = outOrig,
                        NetQty       = inOrig - outOrig,
                        InKg         = hasConv ? inKg  : (decimal?)null,
                        OutKg        = hasConv ? outKg : (decimal?)null,
                        NetKg        = hasConv ? inKg - outKg : (decimal?)null,
                        MovementCount = g.Count(),
                        LastDate      = g.Max(m => m.DocumentDate)
                    };
                })
                .OrderBy(r => r.GroupName).ThenBy(r => r.ProductName)
                .ToList();

            ViewBag.ProductGroups    = allGroups;
            ViewBag.ProductGroupId   = productGroupId;
            ViewBag.Search           = search;
            ViewBag.TotalProducts    = rows.Count;
            ViewBag.GrandTotalKg     = rows.Sum(r => r.NetKg ?? r.NetQty);
            ViewBag.ProductIds       = productIds ?? Array.Empty<int>();
            ViewBag.AllProducts      = await _context.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            // FSC Kütle Dengesi — merkezi servis
            (await FscMassBalanceService.ComputeAsync(_context)).ApplyToViewData(ViewData);

            return View(rows);
        }

        // GET /Stock/Index — ürün bazlı net stok özeti
        public async Task<IActionResult> Index(int? productGroupId, int[]? productIds, string? stockCode, string? erpCode)
        {
            // Ürün filtrelerini DB'de uygula — tüm hareketleri belleğe çekme
            var productQuery = _context.Products.AsQueryable();
            if (productGroupId.HasValue)
                productQuery = productQuery.Where(p => p.ProductGroupId == productGroupId.Value);
            if (productIds != null && productIds.Length > 0)
                productQuery = productQuery.Where(p => productIds.Contains(p.Id));
            if (!string.IsNullOrWhiteSpace(stockCode))
                productQuery = productQuery.Where(p => p.ProductCode.Contains(stockCode.Trim()));
            if (!string.IsNullOrWhiteSpace(erpCode))
                productQuery = productQuery.Where(p => p.ExternalCode != null && p.ExternalCode.Contains(erpCode.Trim()));

            var filteredProductIds = await productQuery.Select(p => p.Id).ToListAsync();

            var movements = await _context.StockMovements
                .Include(m => m.Product).ThenInclude(p => p!.ProductGroup)
                .Where(m => filteredProductIds.Contains(m.ProductId))
                .ToListAsync();

            // Uretim tuketimi + fire urun bazinda
            var indexProdConsByProduct = await _context.ProductionDetails
                .Include(d => d.FscSerial).ThenInclude(s => s.Lot)
                .Where(d => d.FscSerial != null && d.FscSerial.Lot.ProductId != null
                         && filteredProductIds.Contains(d.FscSerial.Lot.ProductId!.Value))
                .GroupBy(d => d.FscSerial!.Lot.ProductId!.Value)
                .Select(g => new { ProductId = g.Key, TotalKg = g.Sum(d => d.ConsumedWeight + d.WasteWeight) })
                .ToDictionaryAsync(x => x.ProductId, x => x.TotalKg);

            var grouped = movements
                .GroupBy(m => m.ProductId)
                .Select(g => new StockSummaryRow
                {
                    ProductId   = g.Key,
                    Product     = g.First().Product!,
                    GirisAdet   = g.Where(m => m.Type == MovementType.ProductionEntry || m.Type == MovementType.PurchaseEntry).Sum(m => m.Quantity),
                    CikisAdet   = g.Where(m => m.Type == MovementType.SalesDispatch).Sum(m => m.Quantity)
                                + (indexProdConsByProduct.TryGetValue(g.Key, out var ipc) ? ipc : 0m),
                    TransferAdet = g.Where(m => m.Type == MovementType.WarehouseTransfer).Sum(m => m.Quantity),
                    LastMovementDate = g.Max(m => m.DocumentDate)
                })
                .ToList();

            ViewBag.ProductGroups = await _context.ProductGroups.OrderBy(g => g.GroupName).ToListAsync();
            ViewBag.Products      = await _context.Products.Where(p => p.IsActive).OrderBy(p => p.ProductName).ToListAsync();
            ViewBag.Warehouses    = await _context.Warehouses.Where(w => w.IsActive).OrderBy(w => w.Name).ToListAsync();
            ViewBag.ProductGroupId = productGroupId;
            ViewBag.ProductIds     = productIds ?? Array.Empty<int>();
            ViewBag.StockCode      = stockCode;
            ViewBag.ErpCode        = erpCode;

            ViewBag.TotalProducts  = grouped.Count;
            ViewBag.InStock        = grouped.Count(r => r.NetAdet > 0);
            ViewBag.ZeroStock      = grouped.Count(r => r.NetAdet <= 0);
            ViewBag.TotalIn        = grouped.Sum(r => r.GirisAdet);
            ViewBag.TotalOut       = grouped.Sum(r => r.CikisAdet);

            return View(grouped.OrderBy(r => r.Product.ProductName).ToList());
        }

        // GET /Stock/Movements — tüm hareket geçmişi
        public async Task<IActionResult> Movements(
            int[]? productIds, MovementType? type,
            DateTime? startDate, DateTime? endDate,
            string? stockCode,
            int[]? supplierIds,
            int[]? fscTypeIds)
        {
            var query = _context.StockMovements
                .Include(m => m.Product)
                    .ThenInclude(p => p!.Supplier)
                .Include(m => m.Product)
                    .ThenInclude(p => p!.FscType)
                .Include(m => m.Customer)
                .Include(m => m.WorkOrder)
                .AsQueryable();

            if (productIds != null && productIds.Length > 0)
                query = query.Where(m => productIds.Contains(m.ProductId));
            if (type.HasValue)      query = query.Where(m => m.Type == type.Value);
            if (startDate.HasValue) query = query.Where(m => m.DocumentDate >= startDate.Value);
            if (endDate.HasValue)   query = query.Where(m => m.DocumentDate <= endDate.Value.AddDays(1));
            if (!string.IsNullOrWhiteSpace(stockCode))
            {
                var sc = stockCode.Trim();
                query = query.Where(m => m.Product != null && (
                    m.Product.ProductCode.Contains(sc) ||
                    (m.Product.ExternalCode != null && m.Product.ExternalCode.Contains(sc)) ||
                    m.Product.ProductName.Contains(sc)));
            }
            if (supplierIds != null && supplierIds.Length > 0)
                query = query.Where(m => m.Product != null && m.Product.SupplierId.HasValue && supplierIds.Contains(m.Product.SupplierId.Value));
            if (fscTypeIds != null && fscTypeIds.Length > 0)
            {
                // FSC tipi lot seviyesinde — belge numarası üzerinden eşleştir
                var matchingLots = await _context.FscLots
                    .Where(l => fscTypeIds.Contains(l.FscTypeId))
                    .Select(l => new { l.DispatchNo, l.InvoiceNo, l.PartiNo })
                    .ToListAsync();
                var lotDocNos = matchingLots
                    .SelectMany(l => new[] { l.DispatchNo, l.InvoiceNo, l.PartiNo })
                    .Where(n => !string.IsNullOrEmpty(n))
                    .Distinct()
                    .ToList();
                query = query.Where(m =>
                    (m.Product != null && m.Product.FscTypeId.HasValue && fscTypeIds.Contains(m.Product.FscTypeId.Value))
                    || (m.DocumentNo != null && lotDocNos.Contains(m.DocumentNo)));
            }

            var movements = await query.OrderByDescending(m => m.DocumentDate).ThenByDescending(m => m.Id).ToListAsync();

            // Alış hareketleri için FscLot PDF bilgileri (DocumentNo = irsaliye/fatura no)
            var purchaseDocNos = movements
                .Where(m => m.Type == MovementType.PurchaseEntry && m.DocumentNo != null)
                .Select(m => m.DocumentNo!).Distinct().ToList();
            var purchaseLots = await _context.FscLots
                .Where(l => (l.DispatchNo != null && purchaseDocNos.Contains(l.DispatchNo))
                         || (l.InvoiceNo  != null && purchaseDocNos.Contains(l.InvoiceNo)))
                .ToListAsync();
            // Belge no → lot eşleşmesi (önce irsaliye, yoksa fatura ile)
            var purchaseLotMap = new Dictionary<string, FscLot>(StringComparer.OrdinalIgnoreCase);
            foreach (var lot in purchaseLots)
            {
                if (!string.IsNullOrEmpty(lot.DispatchNo) && !purchaseLotMap.ContainsKey(lot.DispatchNo))
                    purchaseLotMap[lot.DispatchNo] = lot;
                if (!string.IsNullOrEmpty(lot.InvoiceNo) && !purchaseLotMap.ContainsKey(lot.InvoiceNo))
                    purchaseLotMap[lot.InvoiceNo] = lot;
            }
            ViewBag.PurchaseLotMap = purchaseLotMap;

            // Satış hareketleri için SalesOrder PDF bilgileri (DocumentNo = SalesOrderNo)
            var salesDocNos = movements
                .Where(m => m.Type == MovementType.SalesDispatch && m.DocumentNo != null)
                .Select(m => m.DocumentNo!).Distinct().ToList();
            var salesOrderMap = await _context.SalesOrders
                .Where(o => salesDocNos.Contains(o.SalesOrderNo))
                .ToDictionaryAsync(o => o.SalesOrderNo, StringComparer.OrdinalIgnoreCase);
            ViewBag.SalesOrderMap = salesOrderMap;

            // Uretim tuketim + fire toplami: ProductionDetails'tan (StockMovements eski kayitlarda fire icermiyor)
            var movProdIds = movements.Select(m => m.ProductId).Distinct().ToList();
            var pdMovQuery = _context.ProductionDetails
                .Include(d => d.FscSerial).ThenInclude(s => s.Lot)
                .Where(d => d.FscSerial != null && d.FscSerial.Lot.ProductId != null
                         && movProdIds.Contains(d.FscSerial.Lot.ProductId!.Value));
            if (startDate.HasValue) pdMovQuery = pdMovQuery.Where(d => d.ProductionDate >= startDate.Value);
            if (endDate.HasValue)   pdMovQuery = pdMovQuery.Where(d => d.ProductionDate <= endDate.Value.AddDays(1));
            if (productIds != null && productIds.Length > 0)
                pdMovQuery = pdMovQuery.Where(d => productIds.Contains(d.FscSerial!.Lot.ProductId!.Value));
            var totalProdConsumKg = await pdMovQuery.SumAsync(d => d.ConsumedWeight + d.WasteWeight);
            var totalSMProdConsumKg = movements.Where(m => m.Type == MovementType.ProductionConsumption).Sum(m => m.Quantity);
            ViewBag.TotalProdConsumKg  = totalProdConsumKg;
            ViewBag.TotalSMProdConsumKg = totalSMProdConsumKg;

            // Fire satirlari icin: WasteWeight > 0 olan ProductionDetail'lar (viewde ayri satir olarak gosterilecek)
            var fireDetails = await pdMovQuery
                .Where(d => d.WasteWeight > 0)
                .Include(d => d.WorkOrder)
                .Select(d => new FireMovementRow
                {
                    ProductId    = d.FscSerial!.Lot.ProductId!.Value,
                    ProductName  = d.FscSerial.Lot.Product != null ? d.FscSerial.Lot.Product.ProductName : "—",
                    ProductCode  = d.FscSerial.Lot.Product != null ? d.FscSerial.Lot.Product.ProductCode : "",
                    ExtCode      = d.FscSerial.Lot.Product != null ? d.FscSerial.Lot.Product.ExternalCode : null,
                    FireKg       = d.WasteWeight,
                    Date         = d.ProductionDate,
                    WorkOrderNo  = d.WorkOrder != null ? d.WorkOrder.WorkOrderNo : "",
                    SerialNo     = d.FscSerial.SerialNo
                })
                .OrderByDescending(r => r.Date)
                .ToListAsync();
            ViewBag.FireDetails = fireDetails;

            ViewBag.Products    = await _context.Products.Where(p => p.IsActive).OrderBy(p => p.ProductName).ToListAsync();
            ViewBag.Suppliers   = await _context.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
            ViewBag.FscTypes    = await _context.FscTypes.OrderBy(f => f.Name).ToListAsync();
            ViewBag.StartDate   = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate     = endDate?.ToString("yyyy-MM-dd");
            ViewBag.ProductIds  = productIds ?? Array.Empty<int>();
            ViewBag.StockCode   = stockCode;
            ViewBag.SupplierIds = supplierIds ?? Array.Empty<int>();
            ViewBag.FscTypeIds  = fscTypeIds  ?? Array.Empty<int>();

            return View(movements);
        }

        // POST /Stock/SaveTransfer
        [HttpPost]
        public async Task<IActionResult> SaveTransfer(
            int productId, int? fromWarehouseId, int? toWarehouseId,
            decimal quantity, string unit,
            string? documentNo, DateTime documentDate, string? notes)
        {
            if (quantity <= 0)
                return Json(new { success = false, message = "Miktar 0'dan büyük olmalıdır" });

            var docNo = string.IsNullOrWhiteSpace(documentNo)
                ? $"TRF{documentDate.Year}-{(await _context.StockMovements.CountAsync(m => m.Type == MovementType.WarehouseTransfer)) + 1:D3}"
                : documentNo;

            _context.StockMovements.Add(new StockMovement
            {
                Type             = MovementType.WarehouseTransfer,
                DocumentNo       = docNo,
                DocumentDate     = documentDate,
                ProductId        = productId,
                Quantity         = quantity,
                Unit             = string.IsNullOrWhiteSpace(unit) ? "Adet" : unit,
                FromWarehouseId  = fromWarehouseId == 0 ? null : fromWarehouseId,
                ToWarehouseId    = toWarehouseId == 0   ? null : toWarehouseId,
                Description      = notes
            });

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Transfer kaydedildi: {docNo}" });
        }

        // GET /Stock/ExportStock
        public async Task<IActionResult> ExportStock()
        {
            var movements = await _context.StockMovements
                .Include(m => m.Product).ThenInclude(p => p!.ProductGroup)
                .ToListAsync();

            var rows = movements
                .GroupBy(m => m.ProductId)
                .Select(g =>
                {
                    var p = g.First().Product!;
                    var giris = g.Where(m => m.Type == MovementType.ProductionEntry || m.Type == MovementType.PurchaseEntry).Sum(m => m.Quantity);
                    var cikis = g.Where(m => m.Type == MovementType.SalesDispatch || m.Type == MovementType.ProductionConsumption).Sum(m => m.Quantity);
                    return new
                    {
                        UrunKodu  = p.ProductCode,
                        UrunAdi   = p.ProductName,
                        Grup      = p.ProductGroup != null ? p.ProductGroup.GroupName : "",
                        Birim     = p.Unit,
                        Giris     = giris,
                        Cikis     = cikis,
                        NetStok   = giris - cikis,
                        SonHareket = g.Max(m => m.DocumentDate).ToString("dd.MM.yyyy")
                    };
                })
                .OrderBy(r => r.UrunAdi)
                .ToList();

            return ExportToExcel(rows, "StokDurumu");
        }

        // GET /Stock/RawMaterial — FscSerial bazlı hammadde stoğu
        public async Task<IActionResult> RawMaterial(
            int[]? fscTypeIds, int[]? supplierIds, int[]? productIds,
            int[]? productGroupIds,
            bool? showEmpty = false, bool showAll = false)
        {
            var query = _context.FscSerials
                .Include(s => s.Lot).ThenInclude(l => l.Supplier)
                .Include(s => s.Lot).ThenInclude(l => l.FscType)
                .Include(s => s.Lot).ThenInclude(l => l.Product).ThenInclude(p => p!.ProductGroup)
                .AsQueryable();

            if (showEmpty != true)
                query = query.Where(s => s.CurrentWeight > 0);

            // Kullanıcı herhangi bir filtre uyguladı mı?
            bool hasUserFilter = showAll
                              || (fscTypeIds?.Length > 0)
                              || (supplierIds?.Length > 0)
                              || (productIds?.Length > 0)
                              || (productGroupIds?.Length > 0)
                              || showEmpty == true;

            if (!hasUserFilter)
            {
                // Varsayılan: Ham+YM+BS — ToUpper() Türkçe Ü/U varyantlarını kapsamak için StartsWith kullan
                query = query.Where(s => s.Lot.Product != null
                    && s.Lot.Product.ProductGroup != null
                    && (s.Lot.Product.ProductGroup.GroupName.ToUpper() == "HAMMADDE"
                     || s.Lot.Product.ProductGroup.GroupName.ToUpper().StartsWith("YARI MA")
                     || s.Lot.Product.ProductGroup.GroupName.ToUpper() == "BURGU SAP"));
            }
            else
            {
                if (fscTypeIds != null && fscTypeIds.Length > 0)
                    query = query.Where(s => fscTypeIds.Contains(s.Lot.FscTypeId));
                if (supplierIds != null && supplierIds.Length > 0)
                    query = query.Where(s => s.Lot.SupplierId.HasValue && supplierIds.Contains(s.Lot.SupplierId.Value));
                if (productIds != null && productIds.Length > 0)
                    query = query.Where(s => s.Lot.ProductId.HasValue && productIds.Contains(s.Lot.ProductId.Value));
                if (productGroupIds != null && productGroupIds.Length > 0)
                    query = query.Where(s => s.Lot.Product != null && s.Lot.Product.ProductGroupId.HasValue
                        && productGroupIds.Contains(s.Lot.Product.ProductGroupId.Value));
            }

            ViewBag.IsDefaultFilter = !hasUserFilter;

            var serials = await query
                .OrderBy(s => s.Lot.FscType.Name)
                .ThenBy(s => s.Lot.Supplier.Name)
                .ThenBy(s => s.Lot.PartiNo)
                .ToListAsync();

            // Bobin başına fire = üretim firesi (ProductionDetail.WasteWeight) + dönüşüm firesi (kaynak olduğu YM lotları)
            var serialIds = serials.Select(s => s.Id).ToList();
            var prodFire = await _context.ProductionDetails
                .Where(d => serialIds.Contains(d.FscSerialId))
                .GroupBy(d => d.FscSerialId)
                .Select(g => new { SerialId = g.Key, Fire = g.Sum(x => x.WasteWeight) })
                .ToDictionaryAsync(x => x.SerialId, x => x.Fire);
            var convFire = await _context.FscLots
                .Where(l => l.SourceSerialId != null && serialIds.Contains(l.SourceSerialId.Value) && l.ConversionFireKg != null)
                .GroupBy(l => l.SourceSerialId!.Value)
                .Select(g => new { SerialId = g.Key, Fire = g.Sum(x => x.ConversionFireKg!.Value) })
                .ToDictionaryAsync(x => x.SerialId, x => x.Fire);
            var fireDict = new Dictionary<int, decimal>(prodFire);
            foreach (var kv in convFire)
                fireDict[kv.Key] = (fireDict.TryGetValue(kv.Key, out var pf) ? pf : 0m) + kv.Value;
            ViewBag.SerialFire = fireDict;

            // Bobin basina tuketim (ProductionDetail.ConsumedWeight toplami)
            var consumedDict = await _context.ProductionDetails
                .Where(d => serialIds.Contains(d.FscSerialId))
                .GroupBy(d => d.FscSerialId)
                .Select(g => new { SerialId = g.Key, Consumed = g.Sum(x => x.ConsumedWeight) })
                .ToDictionaryAsync(x => x.SerialId, x => x.Consumed);
            ViewBag.SerialConsumed = consumedDict;

            // Bu serilerden türeyen YM lotları (SourceSerialId → serial.Id)
            var ymRaw = await _context.FscLots
                .Include(l => l.Product)
                .Where(l => l.SourceSerialId != null && serialIds.Contains(l.SourceSerialId.Value))
                .ToListAsync();
            ViewBag.YmLotsBySerial = ymRaw
                .GroupBy(l => l.SourceSerialId!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(l => new YmLotInfo {
                        Id = l.Id, PartiNo = l.PartiNo,
                        ProductName = l.Product?.ProductName ?? "",
                        ConversionFireKg = l.ConversionFireKg
                    }).ToList()
                );

            // Özet kartlar — KALAN = InitialWeight - consumed - fire (CurrentWeight'e guvenme, eski kayitlar fire dusulmemis olabilir)
            ViewBag.TotalKg      = serials.Sum(s => {
                var c = consumedDict.TryGetValue(s.Id, out var cv) ? cv : 0m;
                var f = fireDict.TryGetValue(s.Id, out var fv) ? fv : 0m;
                var k = s.InitialWeight - c - f;
                return k < 0 ? 0m : k;
            });
            ViewBag.TotalBobins  = serials.Count;
            ViewBag.TotalLots    = serials.Select(s => s.LotId).Distinct().Count();
            ViewBag.ByFscType    = serials
                .GroupBy(s => s.Lot?.FscType?.Name ?? "—")
                .Select(g => new FscTypeStockSummary { FscType = g.Key, TotalKg = g.Sum(s => s.CurrentWeight), Count = g.Count() })
                .OrderByDescending(x => x.TotalKg)
                .ToList();

            ViewBag.FscTypes      = await _context.FscTypes.Where(f => f.IsActive).ToListAsync();
            ViewBag.Suppliers     = await _context.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
            ViewBag.Products      = await _context.Products.Where(p => p.IsActive).OrderBy(p => p.ProductName).ToListAsync();
            ViewBag.ProductGroups = await _context.ProductGroups.OrderBy(g => g.GroupName).ToListAsync();
            ViewBag.FscTypeIds       = fscTypeIds      ?? Array.Empty<int>();
            ViewBag.SupplierIds      = supplierIds     ?? Array.Empty<int>();
            ViewBag.ProductIds       = productIds      ?? Array.Empty<int>();
            ViewBag.ProductGroupIds  = productGroupIds ?? Array.Empty<int>();
            ViewBag.ShowEmpty        = showEmpty ?? false;

            // FSC Kütle Dengesi — merkezi servis
            (await FscMassBalanceService.ComputeAsync(_context)).ApplyToViewData(ViewData);

            return View(serials);
        }

        // GET /Stock/ExportRawMaterial
        public async Task<IActionResult> ExportRawMaterial(
            int[]? fscTypeIds, int[]? supplierIds, int[]? productIds,
            int[]? productGroupIds, bool? showEmpty = false)
        {
            var query = _context.FscSerials
                .Include(s => s.Lot).ThenInclude(l => l.Supplier)
                .Include(s => s.Lot).ThenInclude(l => l.FscType)
                .Include(s => s.Lot).ThenInclude(l => l.Product).ThenInclude(p => p!.ProductGroup)
                .AsQueryable();

            if (showEmpty != true)
                query = query.Where(s => s.CurrentWeight > 0);
            if (fscTypeIds != null && fscTypeIds.Length > 0)
                query = query.Where(s => fscTypeIds.Contains(s.Lot.FscTypeId));
            if (productGroupIds != null && productGroupIds.Length > 0)
                query = query.Where(s => s.Lot.Product != null && s.Lot.Product.ProductGroupId.HasValue
                    && productGroupIds.Contains(s.Lot.Product.ProductGroupId.Value));
            if (supplierIds != null && supplierIds.Length > 0)
                query = query.Where(s => s.Lot.SupplierId.HasValue && supplierIds.Contains(s.Lot.SupplierId.Value));
            if (productIds != null && productIds.Length > 0)
                query = query.Where(s => s.Lot.ProductId.HasValue && productIds.Contains(s.Lot.ProductId.Value));

            var rows = await query
                .OrderBy(s => s.Lot.FscType.Name)
                .ThenBy(s => s.Lot.Supplier.Name)
                .ThenBy(s => s.Lot.PartiNo)
                .Select(s => new
                {
                    SeriNo         = s.SerialNo,
                    PartiNo        = s.Lot.PartiNo,
                    DisKod         = s.Lot.Product != null ? s.Lot.Product.ExternalCode : "",
                    Urun           = s.Lot.Product != null ? s.Lot.Product.ProductName : "",
                    Tedarikci      = s.Lot.Supplier != null ? s.Lot.Supplier.Name : "",
                    FscTipi        = s.Lot.FscType != null ? s.Lot.FscType.Name : "",
                    GirisKg        = s.InitialWeight,
                    TuketimKg      = s.InitialWeight - s.CurrentWeight,
                    FireKg         = s.ProductionDetails.Sum(d => d.WasteWeight),
                    KalanKg        = s.CurrentWeight,
                    YuzdeKalan     = s.InitialWeight > 0 ? Math.Round(s.CurrentWeight / s.InitialWeight * 100, 1) : 0m,
                    AcilisDevir    = s.IsOpeningStock ? "Evet" : "Hayır",
                    LotTarihi      = s.Lot.ArrivalDate.ToString("dd.MM.yyyy"),
                    FaturaNo       = s.Lot.InvoiceNo ?? "",
                    IrsaliyeNo     = s.Lot.DispatchNo ?? "",
                    Notlar         = s.Notes ?? ""
                })
                .ToListAsync();

            return ExportToExcel(rows, "HammaddeStogu");
        }

        // GET /Stock/AnaOzet — Parti bazli hammadde ozet tablosu
        public async Task<IActionResult> AnaOzet(
            int[]? supplierIds, int[]? fscTypeIds, int[]? productIds, int[]? ymProductIds,
            string? search, DateTime? startDate, DateTime? endDate)
        {
            // 1. Tum hammadde lotlari — FscMassBalanceService ile ayni filtre (tek kaynak)
            var lotQuery = FscMassBalanceService.ApplyHamLotGirisFilter(
                _context.FscLots
                    .Include(l => l.Supplier)
                    .Include(l => l.FscType)
                    .Include(l => l.Product).ThenInclude(p => p!.ProductGroup),
                _context)
                .AsQueryable();

            if (startDate.HasValue) lotQuery = lotQuery.Where(l => l.ArrivalDate >= startDate.Value);
            if (endDate.HasValue)   lotQuery = lotQuery.Where(l => l.ArrivalDate <= endDate.Value.AddDays(1));
            if (supplierIds?.Length > 0)
                lotQuery = lotQuery.Where(l => l.SupplierId.HasValue && supplierIds.Contains(l.SupplierId.Value));
            if (fscTypeIds?.Length > 0)
                lotQuery = lotQuery.Where(l => fscTypeIds.Contains(l.FscTypeId));
            if (productIds?.Length > 0)
                lotQuery = lotQuery.Where(l => l.ProductId.HasValue && productIds.Contains(l.ProductId.Value));
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                lotQuery = lotQuery.Where(l =>
                    l.PartiNo.Contains(s) ||
                    (l.Product != null && (l.Product.ProductCode.Contains(s) ||
                     l.Product.ProductName.Contains(s) ||
                     (l.Product.ExternalCode != null && l.Product.ExternalCode.Contains(s)))));
            }

            var lots = await lotQuery.OrderByDescending(l => l.ArrivalDate).ToListAsync();
            var allLotIds = lots.Select(l => l.Id).ToList();

            // 2. Lot'lara ait serials
            var allSerials = await _context.FscSerials
                .Where(s => allLotIds.Contains(s.LotId))
                .ToListAsync();
            var serialsByLot = allSerials.GroupBy(s => s.LotId).ToDictionary(g => g.Key, g => g.ToList());
            var allSerialIds = allSerials.Select(s => s.Id).ToList();

            // 3. Tuketim ve fire (ProductionDetails)
            var consumedBySerial = await _context.ProductionDetails
                .Where(d => allSerialIds.Contains(d.FscSerialId))
                .GroupBy(d => d.FscSerialId)
                .Select(g => new { SerialId = g.Key, Consumed = g.Sum(x => x.ConsumedWeight), Fire = g.Sum(x => x.WasteWeight) })
                .ToListAsync();
            var consumedDict = consumedBySerial.ToDictionary(x => x.SerialId, x => x.Consumed);
            var fireDict     = consumedBySerial.ToDictionary(x => x.SerialId, x => x.Fire);

            // 4. YM donusum lotlari (SourceSerialId bu lot'larin seriallarindan biri)
            var ymLots = await _context.FscLots
                .Include(l => l.Product)
                .Where(l => l.SourceSerialId != null && allSerialIds.Contains(l.SourceSerialId.Value))
                .ToListAsync();

            if (ymProductIds?.Length > 0)
                ymLots = ymLots.Where(l => l.ProductId.HasValue && ymProductIds.Contains(l.ProductId.Value)).ToList();

            var ymLotIds = ymLots.Select(l => l.Id).ToList();
            var ymSerials = await _context.FscSerials
                .Where(s => ymLotIds.Contains(s.LotId))
                .ToListAsync();
            var ymSerialIds = ymSerials.Select(s => s.Id).ToList();
            var ymSerialsByLot = ymSerials.GroupBy(s => s.LotId).ToDictionary(g => g.Key, g => g.ToList());

            // YM seriallerinin uretimde tuketim ve fire'i
            var ymConsumedBySerial = await _context.ProductionDetails
                .Where(d => ymSerialIds.Contains(d.FscSerialId))
                .GroupBy(d => d.FscSerialId)
                .Select(g => new { SerialId = g.Key, Consumed = g.Sum(x => x.ConsumedWeight), Fire = g.Sum(x => x.WasteWeight) })
                .ToListAsync();
            var ymConsumedDict = ymConsumedBySerial.ToDictionary(x => x.SerialId, x => x.Consumed);
            var ymFireDict     = ymConsumedBySerial.ToDictionary(x => x.SerialId, x => x.Fire);

            var ymBySourceSerial = ymLots
                .GroupBy(l => l.SourceSerialId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 5. Satir olustur
            var rows = new List<AnaOzetRow>();
            foreach (var lot in lots)
            {
                var serials = serialsByLot.TryGetValue(lot.Id, out var sl) ? sl : new List<FscSerial>();

                decimal girisKg   = serials.Sum(s => s.InitialWeight);
                decimal tuketimKg = serials.Sum(s => consumedDict.TryGetValue(s.Id, out var c) ? c : 0m);
                decimal fireKg    = serials.Sum(s => fireDict.TryGetValue(s.Id, out var f) ? f : 0m);

                var ymInfoList = new List<AnaOzetYmInfo>();
                foreach (var serial in serials)
                {
                    if (!ymBySourceSerial.TryGetValue(serial.Id, out var ymList)) continue;
                    foreach (var ym in ymList)
                    {
                        var ymSerList = ymSerialsByLot.TryGetValue(ym.Id, out var ys) ? ys : new List<FscSerial>();
                        var ymKg      = ymSerList.Sum(s => s.InitialWeight);
                        var ymTuketim = ymSerList.Sum(s => ymConsumedDict.TryGetValue(s.Id, out var c) ? c : 0m);
                        var ymFire    = ymSerList.Sum(s => ymFireDict.TryGetValue(s.Id, out var f) ? f : 0m);
                        var ymKalan   = Math.Max(0m, ymKg - ymTuketim - ymFire);
                        // Her YM lot ayrı sub-row (gruplama yok)
                        ymInfoList.Add(new AnaOzetYmInfo {
                            LotId        = ym.Id,
                            PartiNo      = ym.PartiNo,
                            ArrivalDate  = ym.ArrivalDate,
                            ProductId    = ym.ProductId ?? 0,
                            ProductCode  = ym.Product?.ProductCode ?? "",
                            ExternalCode = ym.Product?.ExternalCode ?? "",
                            ProductName  = ym.Product?.ProductName ?? "",
                            YmKg         = ymKg,
                            YmTuketim    = ymTuketim,
                            YmFire       = ymFire,
                            YmKalan      = ymKalan,
                            ConvFireKg   = ym.ConversionFireKg ?? 0m
                        });
                    }
                }

                decimal ymToplam = ymInfoList.Sum(x => x.YmKg);
                // Kalan = Giriş - YM dönüşüm - direkt tüketim - direkt fire
                decimal kalanKg  = Math.Max(0m, girisKg - ymToplam - tuketimKg - fireKg);

                rows.Add(new AnaOzetRow {
                    LotId        = lot.Id,
                    PartiNo      = lot.PartiNo,
                    ArrivalDate  = lot.ArrivalDate,
                    Supplier     = lot.Supplier?.Name ?? "",
                    FscType      = lot.FscType?.Code ?? "",
                    ProductId    = lot.ProductId ?? 0,
                    ProductCode  = lot.Product?.ProductCode ?? "",
                    ExternalCode = lot.Product?.ExternalCode ?? "",
                    ProductName  = lot.Product?.ProductName ?? "",
                    SerialCount  = serials.Count,
                    GirisKg      = girisKg,
                    TuketimKg    = tuketimKg,
                    FireKg       = fireKg,
                    YmKg         = ymToplam,
                    KalanKg      = kalanKg,
                    YmList       = ymInfoList
                });
            }

            // ── YM lotlarini bagımsız satır olarak sona ekle ──────────────────
            // Tum YM lotlari (SourceSerialId IS NOT NULL) — ham filtresinden bagimsiz
            var allYmLots = await _context.FscLots
                .Include(l => l.Product).ThenInclude(p => p!.ProductGroup)
                .Include(l => l.FscType)
                .Where(l => l.SourceSerialId != null)
                .OrderBy(l => l.ArrivalDate)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s2 = search.Trim();
                allYmLots = allYmLots.Where(l =>
                    l.PartiNo.Contains(s2, StringComparison.OrdinalIgnoreCase) ||
                    (l.Product != null && (
                        l.Product.ProductCode.Contains(s2, StringComparison.OrdinalIgnoreCase) ||
                        l.Product.ProductName.Contains(s2, StringComparison.OrdinalIgnoreCase) ||
                        (l.Product.ExternalCode != null && l.Product.ExternalCode.Contains(s2, StringComparison.OrdinalIgnoreCase)))
                    )).ToList();
            }
            if (startDate.HasValue) allYmLots = allYmLots.Where(l => l.ArrivalDate >= startDate.Value).ToList();
            if (endDate.HasValue)   allYmLots = allYmLots.Where(l => l.ArrivalDate <= endDate.Value.AddDays(1)).ToList();

            var allYmLotIds     = allYmLots.Select(l => l.Id).ToList();
            var allYmSerials2   = await _context.FscSerials.Where(s => allYmLotIds.Contains(s.LotId)).ToListAsync();
            var allYmSerialIds2 = allYmSerials2.Select(s => s.Id).ToList();
            var ymByLot2        = allYmSerials2.GroupBy(s => s.LotId).ToDictionary(g => g.Key, g => g.ToList());
            var ymProd2         = await _context.ProductionDetails
                .Where(d => allYmSerialIds2.Contains(d.FscSerialId))
                .GroupBy(d => d.FscSerialId)
                .Select(g => new { SerialId = g.Key, Consumed = g.Sum(x => x.ConsumedWeight), Fire = g.Sum(x => x.WasteWeight) })
                .ToListAsync();
            var ymConsumed2 = ymProd2.ToDictionary(x => x.SerialId, x => x.Consumed);
            var ymFire2     = ymProd2.ToDictionary(x => x.SerialId, x => x.Fire);

            // Ham satırların source serial'larını bul → FSC tipi için
            var sourceSerialIds = allYmLots.Where(l => l.SourceSerialId.HasValue).Select(l => l.SourceSerialId!.Value).Distinct().ToList();
            var sourceLotFsc = await _context.FscSerials
                .Include(s => s.Lot).ThenInclude(l => l.FscType)
                .Where(s => sourceSerialIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Lot?.FscType?.Code ?? "");

            foreach (var yl in allYmLots)
            {
                var ymSers   = ymByLot2.TryGetValue(yl.Id, out var sl2) ? sl2 : new List<FscSerial>();
                var girisKg  = ymSers.Sum(s => s.InitialWeight);
                var tuketim2 = ymSers.Sum(s => ymConsumed2.TryGetValue(s.Id, out var c) ? c : 0m);
                var fire2    = ymSers.Sum(s => ymFire2.TryGetValue(s.Id, out var f) ? f : 0m);
                var kalan2   = Math.Max(0m, girisKg - tuketim2 - fire2);
                // FSC tipini kaynak serialden al
                var fscCode  = yl.SourceSerialId.HasValue && sourceLotFsc.TryGetValue(yl.SourceSerialId.Value, out var fc) ? fc : (yl.FscType?.Code ?? "");

                rows.Add(new AnaOzetRow {
                    LotId        = yl.Id,
                    PartiNo      = yl.PartiNo,
                    ArrivalDate  = yl.ArrivalDate,
                    Supplier     = "YM Dönüşüm",
                    FscType      = fscCode,
                    ProductId    = yl.ProductId ?? 0,
                    ProductCode  = yl.Product?.ProductCode ?? "",
                    ExternalCode = yl.Product?.ExternalCode ?? "",
                    ProductName  = yl.Product?.ProductName ?? "",
                    SerialCount  = ymSers.Count,
                    GirisKg      = girisKg,
                    TuketimKg    = tuketim2,
                    FireKg       = fire2,
                    KalanKg      = kalan2,
                    IsYm         = true,
                    YmList       = new()
                });
            }
            // ──────────────────────────────────────────────────────────────────

            ViewBag.TotalGiris   = rows.Where(r => !r.IsYm).Sum(r => r.GirisKg);  // YM satırları hariç
            ViewBag.TotalTuketim = rows.Sum(r => r.TuketimKg);
            ViewBag.TotalFire    = rows.Sum(r => r.FireKg);
            ViewBag.TotalYm      = rows.Where(r => r.IsYm).Sum(r => r.GirisKg);   // YM lot giriş toplamı
            ViewBag.TotalKalan   = rows.Sum(r => r.KalanKg);

            ViewBag.Suppliers  = await _context.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
            ViewBag.FscTypes   = await _context.FscTypes.Where(f => f.IsActive).ToListAsync();
            ViewBag.Products   = await _context.Products.Where(p => p.IsActive).OrderBy(p => p.ProductName).ToListAsync();
            ViewBag.YmProducts = await _context.Products
                .Where(p => p.IsActive && p.ProductGroup != null && p.ProductGroup.GroupName.ToUpper().StartsWith("YARI MA"))
                .OrderBy(p => p.ProductName).ToListAsync();

            ViewBag.SupplierIds  = supplierIds  ?? Array.Empty<int>();
            ViewBag.FscTypeIds   = fscTypeIds   ?? Array.Empty<int>();
            ViewBag.ProductIds   = productIds   ?? Array.Empty<int>();
            ViewBag.YmProductIds = ymProductIds ?? Array.Empty<int>();
            ViewBag.Search       = search ?? "";
            ViewBag.StartDate    = startDate?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.EndDate      = endDate?.ToString("yyyy-MM-dd") ?? "";

            return View(rows);
        }

        // GET /Stock/ExportAnaOzet
        public async Task<IActionResult> ExportAnaOzet(
            int[]? supplierIds, int[]? fscTypeIds, int[]? productIds, int[]? ymProductIds,
            string? search, DateTime? startDate, DateTime? endDate)
        {
            // AnaOzet ile aynı veri — redirect yerine direkt çağır
            var result = await AnaOzet(supplierIds, fscTypeIds, productIds, ymProductIds, search, startDate, endDate);
            var rows = (result as ViewResult)?.Model as List<AnaOzetRow> ?? new();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Ana Özet");

            // Başlık satırı
            var headers = new[] { "Tarih","Parti No","Tedarikçi","FSC","Stok Kodu","Dış Kodu","Stok Adı","Tip",
                                   "Giriş KG","YM KG","Tüketim KG","Fire KG","Kalan KG" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i+1).Value = headers[i];
                ws.Cell(1, i+1).Style.Font.Bold = true;
                ws.Cell(1, i+1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3d14");
                ws.Cell(1, i+1).Style.Font.FontColor = XLColor.White;
            }

            int row = 2;
            foreach (var r in rows)
            {
                ws.Cell(row, 1).Value  = r.ArrivalDate.ToString("dd.MM.yyyy");
                ws.Cell(row, 2).Value  = r.PartiNo;
                ws.Cell(row, 3).Value  = r.Supplier;
                ws.Cell(row, 4).Value  = r.FscType;
                ws.Cell(row, 5).Value  = r.ProductCode;
                ws.Cell(row, 6).Value  = r.ExternalCode;
                ws.Cell(row, 7).Value  = r.ProductName;
                ws.Cell(row, 8).Value  = r.IsYm ? "YM Dönüşüm" : "Hammadde";
                // is-ym: Giriş boş, YM KG = girisKg (pozitif); ham: Giriş = girisKg, YM KG = -ymKg (negatif)
                ws.Cell(row, 9).Value  = r.IsYm ? (decimal?)null : r.GirisKg;
                ws.Cell(row, 10).Value = r.IsYm
                    ? (r.GirisKg > 0 ? (decimal?)r.GirisKg : (decimal?)null)
                    : (r.YmKg > 0 ? (decimal?)-r.YmKg : (decimal?)null);
                ws.Cell(row, 11).Value = r.TuketimKg > 0 ? r.TuketimKg : (decimal?)null;
                ws.Cell(row, 12).Value = r.FireKg > 0 ? r.FireKg : (decimal?)null;
                ws.Cell(row, 13).Value = r.KalanKg;
                ws.Row(row).Style.Fill.BackgroundColor = r.IsYm ? XLColor.FromHtml("#fefce8") : XLColor.White;
                row++;

                // YM sub-satırlar
                foreach (var ym in r.YmList)
                {
                    ws.Cell(row, 1).Value  = ym.ArrivalDate.ToString("dd.MM.yyyy");
                    ws.Cell(row, 2).Value  = "  ↳ " + ym.PartiNo;
                    ws.Cell(row, 3).Value  = "";
                    ws.Cell(row, 4).Value  = "";
                    ws.Cell(row, 5).Value  = ym.ProductCode;
                    ws.Cell(row, 6).Value  = ym.ExternalCode;
                    ws.Cell(row, 7).Value  = ym.ProductName;
                    ws.Cell(row, 8).Value  = "YM Dönüşüm";
                    ws.Cell(row, 9).Value  = ym.YmKg;
                    ws.Cell(row, 10).Value = (decimal?)null;
                    ws.Cell(row, 11).Value = ym.YmTuketim > 0 ? ym.YmTuketim : (decimal?)null;
                    ws.Cell(row, 12).Value = ym.YmFire > 0 ? ym.YmFire : (decimal?)null;
                    ws.Cell(row, 13).Value = ym.YmKalan;
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#fefce8");
                    ws.Cell(row, 2).Style.Font.Italic = true;
                    row++;
                }
            }

            // Toplam satırı
            ws.Cell(row, 7).Value = "TOPLAM";
            ws.Cell(row, 7).Style.Font.Bold = true;
            ws.Cell(row, 9).FormulaA1  = $"=SUM(I2:I{row-1})";
            ws.Cell(row, 11).FormulaA1 = $"=SUM(K2:K{row-1})";
            ws.Cell(row, 12).FormulaA1 = $"=SUM(L2:L{row-1})";
            ws.Cell(row, 13).FormulaA1 = $"=SUM(M2:M{row-1})";
            ws.Row(row).Style.Font.Bold = true;
            ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#f0fdf4");

            // Sayı formatı
            for (int r2 = 2; r2 <= row; r2++)
                for (int c = 9; c <= 13; c++)
                    ws.Cell(r2, c).Style.NumberFormat.Format = "#,##0.00";

            ws.Columns().AdjustToContents();
            ws.Column(7).Width = Math.Min(ws.Column(7).Width, 40);

            using var ms = new System.IO.MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"AnaOzet_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        // GET /Stock/ExportMovements
        public async Task<IActionResult> ExportMovements()
        {
            var rows = await _context.StockMovements
                .Include(m => m.Product)
                .Include(m => m.Customer)
                .OrderByDescending(m => m.DocumentDate)
                .Select(m => new
                {
                    Tarih       = m.DocumentDate.ToString("dd.MM.yyyy"),
                    BelgeNo     = m.DocumentNo ?? "",
                    Tip         = m.Type == MovementType.ProductionEntry       ? "Üretim Girişi"
                                : m.Type == MovementType.PurchaseEntry         ? "Satın Alma Girişi"
                                : m.Type == MovementType.SalesDispatch         ? "Satış Çıkışı"
                                : m.Type == MovementType.ProductionConsumption ? "Üretim Tüketimi (Çıkış)"
                                : "Depo Transferi",
                    Urun        = m.Product != null ? m.Product.ProductName : "",
                    Miktar      = m.Quantity,
                    Birim       = m.Unit,
                    Musteri     = m.Customer != null ? m.Customer.Name : "",
                    Plaka       = m.PlateNumber ?? "",
                    Aciklama    = m.Description ?? ""
                })
                .ToListAsync();

            return ExportToExcel(rows, "StokHareketleri");
        }
    }

    public class FscTypeStockSummary
    {
        public string FscType { get; set; } = "";
        public decimal TotalKg { get; set; }
        public int Count { get; set; }
    }

    public class AnaOzetRow
    {
        public int      LotId        { get; set; }
        public string   PartiNo      { get; set; } = "";
        public DateTime ArrivalDate  { get; set; }
        public string   Supplier     { get; set; } = "";
        public string   FscType      { get; set; } = "";
        public int      ProductId    { get; set; }
        public string   ProductCode  { get; set; } = "";
        public string   ExternalCode { get; set; } = "";
        public string   ProductName  { get; set; } = "";
        public int      SerialCount  { get; set; }
        public decimal  GirisKg      { get; set; }
        public decimal  TuketimKg    { get; set; }
        public decimal  FireKg       { get; set; }
        public decimal  YmKg         { get; set; }
        public decimal  KalanKg      { get; set; }
        public bool     IsYm         { get; set; }  // YM dönüşüm lotu
        public List<AnaOzetYmInfo> YmList { get; set; } = new();
    }

    public class AnaOzetYmInfo
    {
        public int      LotId        { get; set; }
        public string   PartiNo      { get; set; } = "";
        public DateTime ArrivalDate  { get; set; }
        public int      ProductId    { get; set; }
        public string   ProductCode  { get; set; } = "";
        public string   ExternalCode { get; set; } = "";
        public string   ProductName  { get; set; } = "";
        public decimal  YmKg         { get; set; }
        public decimal  YmTuketim    { get; set; }
        public decimal  YmFire       { get; set; }
        public decimal  YmKalan      { get; set; }
        public decimal  ConvFireKg   { get; set; }
    }

    public class YmLotInfo
    {
        public int Id { get; set; }
        public string PartiNo { get; set; } = "";
        public string ProductName { get; set; } = "";
        public decimal? ConversionFireKg { get; set; }
    }

    public class StockSummaryItem
    {
        public int      ProductId     { get; set; }
        public int      GroupId       { get; set; }
        public string   ExternalCode  { get; set; } = "";
        public string   ProductCode   { get; set; } = "";
        public string   ProductName   { get; set; } = "";
        public string   GroupName     { get; set; } = "";
        public string   Unit          { get; set; } = "KG";
        public decimal  TotalKg       { get; set; }       // Her zaman KG
        public decimal? OriginalTotal { get; set; }       // MT/ADET gibi orijinal birimde toplam
        public decimal? Factor        { get; set; }       // Donusum faktoru (orj -> KG)
        public int      LotCount      { get; set; }
        public int      SerialCount   { get; set; }
    }

    public class AdminStockItem
    {
        public int      ProductId     { get; set; }
        public string   ExternalCode  { get; set; } = "";
        public string   ProductCode   { get; set; } = "";
        public string   ProductName   { get; set; } = "";
        public string   GroupName     { get; set; } = "";
        public string   Unit          { get; set; } = "KG";
        public decimal  InQty         { get; set; }   // Giriş — orijinal birim
        public decimal  OutQty        { get; set; }   // Çıkış — orijinal birim
        public decimal  NetQty        { get; set; }   // Net — orijinal birim
        public decimal? InKg          { get; set; }   // Giriş KG karşılığı (dönüşüm varsa)
        public decimal? OutKg         { get; set; }
        public decimal? NetKg         { get; set; }
        public int      MovementCount { get; set; }
        public DateTime LastDate      { get; set; }
    }

    public class StockSummaryRow
    {
        public int ProductId  { get; set; }
        public Product Product { get; set; } = null!;
        public decimal GirisAdet  { get; set; }
        public decimal CikisAdet  { get; set; }
        public decimal TransferAdet { get; set; }
        public decimal NetAdet => GirisAdet - CikisAdet;
        public DateTime? LastMovementDate { get; set; }
    }

    public class FireMovementRow
    {
        public int      ProductId   { get; set; }
        public string   ProductName { get; set; } = "";
        public string   ProductCode { get; set; } = "";
        public string?  ExtCode     { get; set; }
        public decimal  FireKg      { get; set; }
        public DateTime Date        { get; set; }
        public string   WorkOrderNo { get; set; } = "";
        public string   SerialNo    { get; set; } = "";
    }
}
