using ClosedXML.Excel;
using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
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

                    // KG bazli net hesap: giris - cikis (QuantityKg varsa onu kullan, yoksa Quantity)
                    var inboundKg  = g.Where(m => m.Type == MovementType.PurchaseEntry || m.Type == MovementType.ProductionEntry)
                                      .Sum(m => m.QuantityKg ?? m.Quantity);
                    var outboundKg = g.Where(m => m.Type == MovementType.SalesDispatch || m.Type == MovementType.ProductionConsumption)
                                      .Sum(m => m.QuantityKg ?? m.Quantity);
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

            var rows = movements
                .GroupBy(m => m.ProductId)
                .Select(g =>
                {
                    var prod = g.First().Product!;
                    var unit = (prod.Unit ?? "KG").Trim().ToUpperInvariant();
                    bool hasConv = unit != "KG";

                    var inOrig  = g.Where(m => m.Type == MovementType.PurchaseEntry || m.Type == MovementType.ProductionEntry).Sum(m => m.Quantity);
                    var outOrig = g.Where(m => m.Type == MovementType.SalesDispatch || m.Type == MovementType.ProductionConsumption).Sum(m => m.Quantity);
                    var inKg    = g.Where(m => m.Type == MovementType.PurchaseEntry || m.Type == MovementType.ProductionEntry).Sum(m => m.QuantityKg ?? m.Quantity);
                    var outKg   = g.Where(m => m.Type == MovementType.SalesDispatch || m.Type == MovementType.ProductionConsumption).Sum(m => m.QuantityKg ?? m.Quantity);

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

            var grouped = movements
                .GroupBy(m => m.ProductId)
                .Select(g => new StockSummaryRow
                {
                    ProductId   = g.Key,
                    Product     = g.First().Product!,
                    GirisAdet   = g.Where(m => m.Type == MovementType.ProductionEntry || m.Type == MovementType.PurchaseEntry).Sum(m => m.Quantity),
                    CikisAdet   = g.Where(m => m.Type == MovementType.SalesDispatch || m.Type == MovementType.ProductionConsumption).Sum(m => m.Quantity),
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
                query = query.Where(m => m.Product != null && m.Product.FscTypeId.HasValue && fscTypeIds.Contains(m.Product.FscTypeId.Value));

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
            bool? showEmpty = false)
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
            if (supplierIds != null && supplierIds.Length > 0)
                query = query.Where(s => s.Lot.SupplierId.HasValue && supplierIds.Contains(s.Lot.SupplierId.Value));
            if (productIds != null && productIds.Length > 0)
                query = query.Where(s => s.Lot.ProductId.HasValue && productIds.Contains(s.Lot.ProductId.Value));
            if (productGroupIds != null && productGroupIds.Length > 0)
                query = query.Where(s => s.Lot.Product != null && s.Lot.Product.ProductGroupId.HasValue
                    && productGroupIds.Contains(s.Lot.Product.ProductGroupId.Value));

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

            // Özet kartlar
            ViewBag.TotalKg      = serials.Sum(s => s.CurrentWeight);
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
}
