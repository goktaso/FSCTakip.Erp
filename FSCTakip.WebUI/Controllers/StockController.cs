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

        // GET /Stock/Index — ürün bazlı net stok özeti
        public async Task<IActionResult> Index(int? productGroupId, int? productId, string? stockCode, string? erpCode)
        {
            // Ürün filtrelerini DB'de uygula — tüm hareketleri belleğe çekme
            var productQuery = _context.Products.AsQueryable();
            if (productGroupId.HasValue)
                productQuery = productQuery.Where(p => p.ProductGroupId == productGroupId.Value);
            if (productId.HasValue)
                productQuery = productQuery.Where(p => p.Id == productId.Value);
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
                    CikisAdet   = g.Where(m => m.Type == MovementType.SalesDispatch).Sum(m => m.Quantity),
                    TransferAdet = g.Where(m => m.Type == MovementType.WarehouseTransfer).Sum(m => m.Quantity),
                    LastMovementDate = g.Max(m => m.DocumentDate)
                })
                .ToList();

            ViewBag.ProductGroups = await _context.ProductGroups.OrderBy(g => g.GroupName).ToListAsync();
            ViewBag.Products      = await _context.Products.Where(p => p.IsActive).OrderBy(p => p.ProductName).ToListAsync();
            ViewBag.Warehouses    = await _context.Warehouses.Where(w => w.IsActive).OrderBy(w => w.Name).ToListAsync();
            ViewBag.ProductGroupId = productGroupId;
            ViewBag.ProductId      = productId;
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
            int? productId, MovementType? type,
            DateTime? startDate, DateTime? endDate,
            string? stockCode)
        {
            var query = _context.StockMovements
                .Include(m => m.Product)
                .Include(m => m.Customer)
                .Include(m => m.WorkOrder)
                .AsQueryable();

            if (productId.HasValue) query = query.Where(m => m.ProductId == productId.Value);
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

            ViewBag.Products   = await _context.Products.Where(p => p.IsActive).OrderBy(p => p.ProductName).ToListAsync();
            ViewBag.StartDate  = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate    = endDate?.ToString("yyyy-MM-dd");
            ViewBag.StockCode  = stockCode;

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
                    var cikis = g.Where(m => m.Type == MovementType.SalesDispatch).Sum(m => m.Quantity);
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
            int? fscTypeId, int? supplierId, int? productId,
            bool? showEmpty = false)
        {
            var query = _context.FscSerials
                .Include(s => s.Lot).ThenInclude(l => l.Supplier)
                .Include(s => s.Lot).ThenInclude(l => l.FscType)
                .Include(s => s.Lot).ThenInclude(l => l.Product)
                .AsQueryable();

            if (showEmpty != true)
                query = query.Where(s => s.CurrentWeight > 0);

            if (fscTypeId.HasValue)
                query = query.Where(s => s.Lot.FscTypeId == fscTypeId.Value);
            if (supplierId.HasValue)
                query = query.Where(s => s.Lot.SupplierId == supplierId.Value);
            if (productId.HasValue)
                query = query.Where(s => s.Lot.ProductId == productId.Value);

            var serials = await query
                .OrderBy(s => s.Lot.FscType.Name)
                .ThenBy(s => s.Lot.Supplier.Name)
                .ThenBy(s => s.Lot.PartiNo)
                .ToListAsync();

            // Özet kartlar
            ViewBag.TotalKg      = serials.Sum(s => s.CurrentWeight);
            ViewBag.TotalBobins  = serials.Count;
            ViewBag.TotalLots    = serials.Select(s => s.LotId).Distinct().Count();
            ViewBag.ByFscType    = serials
                .GroupBy(s => s.Lot?.FscType?.Name ?? "—")
                .Select(g => new FscTypeStockSummary { FscType = g.Key, TotalKg = g.Sum(s => s.CurrentWeight), Count = g.Count() })
                .OrderByDescending(x => x.TotalKg)
                .ToList();

            ViewBag.FscTypes  = await _context.FscTypes.Where(f => f.IsActive).ToListAsync();
            ViewBag.Suppliers = await _context.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
            ViewBag.Products  = await _context.Products.Where(p => p.IsActive).OrderBy(p => p.ProductName).ToListAsync();
            ViewBag.FscTypeId  = fscTypeId;
            ViewBag.SupplierId = supplierId;
            ViewBag.ProductId  = productId;
            ViewBag.ShowEmpty  = showEmpty ?? false;

            return View(serials);
        }

        // GET /Stock/ExportRawMaterial
        public async Task<IActionResult> ExportRawMaterial(
            int? fscTypeId, int? supplierId, int? productId, bool? showEmpty = false)
        {
            var query = _context.FscSerials
                .Include(s => s.Lot).ThenInclude(l => l.Supplier)
                .Include(s => s.Lot).ThenInclude(l => l.FscType)
                .Include(s => s.Lot).ThenInclude(l => l.Product)
                .AsQueryable();

            if (showEmpty != true)
                query = query.Where(s => s.CurrentWeight > 0);
            if (fscTypeId.HasValue)
                query = query.Where(s => s.Lot.FscTypeId == fscTypeId.Value);
            if (supplierId.HasValue)
                query = query.Where(s => s.Lot.SupplierId == supplierId.Value);
            if (productId.HasValue)
                query = query.Where(s => s.Lot.ProductId == productId.Value);

            var rows = await query
                .OrderBy(s => s.Lot.FscType.Name)
                .ThenBy(s => s.Lot.Supplier.Name)
                .ThenBy(s => s.Lot.PartiNo)
                .Select(s => new
                {
                    SeriNo         = s.SerialNo,
                    PartiNo        = s.Lot.PartiNo,
                    Urun           = s.Lot.Product != null ? s.Lot.Product.ProductName : "",
                    Tedarikci      = s.Lot.Supplier != null ? s.Lot.Supplier.Name : "",
                    FscTipi        = s.Lot.FscType != null ? s.Lot.FscType.Name : "",
                    GirisKg        = s.InitialWeight,
                    KalanKg        = s.CurrentWeight,
                    TuketimKg      = s.InitialWeight - s.CurrentWeight,
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
                    Tip         = m.Type == MovementType.ProductionEntry  ? "Üretim Girişi"
                                : m.Type == MovementType.PurchaseEntry    ? "Satın Alma Girişi"
                                : m.Type == MovementType.SalesDispatch    ? "Satış Çıkışı"
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
