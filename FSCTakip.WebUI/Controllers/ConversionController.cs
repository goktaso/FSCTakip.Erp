using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Controllers
{
    /// <summary>
    /// Yarı Mamül Dönüşüm — ham kağıt (1xxxx) / YM (24xxx) bobinini tüketip
    /// yarı mamül (23xxx BB) bobinine dönüştürür. Çıktı tekrar stok olur ve
    /// 3xxxx mamul üretiminde tüketilebilir. FSC tipi kaynaktan devralınır (CoC).
    /// </summary>
    public class ConversionController : BaseController
    {
        public ConversionController(AppDbContext context) : base(context) { }

        // GET /Conversion
        public async Task<IActionResult> Index()
        {
            // Stokta kalanı olan kaynak bobinler (ham/YM)
            ViewBag.SourceSerials = await _context.FscSerials
                .Include(s => s.Lot).ThenInclude(l => l.Product)
                .Include(s => s.Lot).ThenInclude(l => l.FscType)
                .Where(s => s.CurrentWeight > 0)
                .OrderByDescending(s => s.Id)
                .Select(s => new ConvSourceVM {
                    Id            = s.Id,
                    SerialNo      = s.SerialNo,
                    CurrentWeight = s.CurrentWeight,
                    FscType       = s.Lot.FscType.Name,
                    UrunKod       = s.Lot.Product != null ? s.Lot.Product.ExternalCode : null,
                    UrunAd        = s.Lot.Product != null ? s.Lot.Product.ProductName : "—"
                })
                .ToListAsync();

            // Hedef yarı mamüller (2 ile başlayan kodlar)
            ViewBag.TargetProducts = await _context.Products
                .Where(p => p.ExternalCode != null && p.ExternalCode.StartsWith("2"))
                .OrderBy(p => p.ProductName)
                .Select(p => new ConvTargetVM { Id = p.Id, Kod = p.ExternalCode!, Ad = p.ProductName })
                .ToListAsync();

            // Son dönüşümler (kaynak bobin/ürün bilgisiyle)
            var recentYm = await _context.FscSerials
                .Include(s => s.Lot).ThenInclude(l => l.Product)
                .Include(s => s.Lot).ThenInclude(l => l.FscType)
                .Where(s => s.Lot.PartiNo.StartsWith("YM"))
                .OrderByDescending(s => s.Id)
                .Take(20)
                .Select(s => new {
                    s.Lot.ArrivalDate, s.Lot.PartiNo,
                    Hedef   = s.Lot.Product != null ? s.Lot.Product.ProductName : "—",
                    FscType = s.Lot.FscType.Name,
                    s.InitialWeight, s.CurrentWeight,
                    s.Lot.SourceSerialId, s.Lot.ConversionFireKg
                })
                .ToListAsync();

            var srcIds = recentYm.Where(x => x.SourceSerialId != null).Select(x => x.SourceSerialId!.Value).Distinct().ToList();
            var srcMap = await _context.FscSerials
                .Where(s => srcIds.Contains(s.Id))
                .Include(s => s.Lot).ThenInclude(l => l.Product)
                .Select(s => new {
                    s.Id, s.SerialNo,
                    Kod = s.Lot.Product != null ? s.Lot.Product.ExternalCode : null,
                    Ad  = s.Lot.Product != null ? s.Lot.Product.ProductName : null
                })
                .ToDictionaryAsync(x => x.Id, x => x);

            ViewBag.Recent = recentYm.Select(x => new ConvRecentVM {
                Tarih        = x.ArrivalDate,
                Parti        = x.PartiNo,
                Hedef        = x.Hedef,
                FscType      = x.FscType,
                Kg           = x.InitialWeight,
                Kalan        = x.CurrentWeight,
                Fire         = x.ConversionFireKg ?? 0m,
                KaynakSerial = x.SourceSerialId != null && srcMap.ContainsKey(x.SourceSerialId.Value) ? srcMap[x.SourceSerialId.Value].SerialNo : "—",
                KaynakKod    = x.SourceSerialId != null && srcMap.ContainsKey(x.SourceSerialId.Value) ? srcMap[x.SourceSerialId.Value].Kod : null,
                KaynakAd     = x.SourceSerialId != null && srcMap.ContainsKey(x.SourceSerialId.Value) ? srcMap[x.SourceSerialId.Value].Ad : "—"
            }).ToList();

            return View();
        }

        // POST /Conversion/Convert
        [HttpPost]
        public async Task<IActionResult> Convert(int sourceSerialId, int targetProductId,
                                                  decimal producedKg, decimal consumedKg,
                                                  DateTime? date, string? notes)
        {
            try
            {
                if (producedKg <= 0)
                    return Json(new { success = false, message = "Üretilen miktar sıfırdan büyük olmalıdır." });
                if (consumedKg <= 0)
                    return Json(new { success = false, message = "Tüketilen ham miktar sıfırdan büyük olmalıdır." });
                if (producedKg > consumedKg)
                    return Json(new { success = false, message = "Üretilen miktar, tüketilen ham miktarı aşamaz." });

                var source = await _context.FscSerials
                    .Include(s => s.Lot).ThenInclude(l => l.Product)
                    .FirstOrDefaultAsync(s => s.Id == sourceSerialId);
                if (source == null)
                    return Json(new { success = false, message = "Kaynak bobin bulunamadı." });
                if (consumedKg > source.CurrentWeight)
                    return Json(new { success = false, message = $"Tüketim ({consumedKg:N2} kg), kaynak bobinin kalanını ({source.CurrentWeight:N2} kg) aşıyor." });

                var target = await _context.Products.FindAsync(targetProductId);
                if (target == null)
                    return Json(new { success = false, message = "Hedef yarı mamül bulunamadı." });

                var when = (date ?? DateTime.Now).Date;
                var user = User.Identity?.Name ?? "System";

                // Parti no: YM{yy}-{sıra}
                var ymCount = await _context.FscLots.CountAsync(l => l.PartiNo.StartsWith("YM"));
                var partiNo = $"YM{when:yy}-{ymCount + 1:D3}";

                // 1) Kaynak bobinden düş
                source.CurrentWeight -= consumedKg;

                // 2) Hedef için yeni lot (FSC tipi kaynaktan devralınır — CoC)
                var srcName = source.Lot.Product?.ProductName ?? source.Lot.PartiNo;
                var lot = new FscLot
                {
                    PartiNo          = partiNo,
                    FscTypeId        = source.Lot.FscTypeId,
                    SupplierId       = null,
                    ProductId        = targetProductId,
                    ArrivalDate      = when,
                    Currency         = "TRY",
                    SourceSerialId   = source.Id,
                    ConversionFireKg = consumedKg - producedKg,
                    Notes       = $"Yarı mamül dönüşüm — kaynak: {srcName} / {source.SerialNo} ({consumedKg:N2} kg → {producedKg:N2} kg)"
                                  + (string.IsNullOrWhiteSpace(notes) ? "" : $" · {notes.Trim()}"),
                    CreatedBy   = user,
                    CreatedDate = DateTime.Now
                };
                _context.FscLots.Add(lot);
                await _context.SaveChangesAsync();

                // 3) Hedef bobin
                var serial = new FscSerial
                {
                    LotId         = lot.Id,
                    SerialNo      = $"{partiNo}-B01",
                    InitialWeight = producedKg,
                    CurrentWeight = producedKg,
                    Notes         = $"Dönüşüm çıktısı (fire {(consumedKg - producedKg):N2} kg)",
                    CreatedBy     = user,
                    CreatedDate   = DateTime.Now
                };
                _context.FscSerials.Add(serial);

                // 4) Stok hareketi — yarı mamül girişi
                _context.StockMovements.Add(new StockMovement
                {
                    Type         = MovementType.ProductionEntry,
                    ProductId    = targetProductId,
                    Quantity     = producedKg,
                    Unit         = "kg",
                    DocumentNo   = partiNo,
                    DocumentDate = when,
                    Description  = $"Yarı mamül dönüşüm: {srcName} → {target.ProductName}",
                    CreatedBy    = user,
                    CreatedDate  = DateTime.Now
                });

                // 4b) Stok hareketi — kaynak ham/YM tüketimi (çıkış)
                if (source.Lot.ProductId.HasValue)
                {
                    _context.StockMovements.Add(new StockMovement
                    {
                        Type         = MovementType.ProductionConsumption,
                        ProductId    = source.Lot.ProductId.Value,
                        Quantity     = consumedKg,
                        Unit         = "kg",
                        DocumentNo   = partiNo,
                        DocumentDate = when,
                        Description  = $"Dönüşüm tüketimi: {srcName} → {target.ProductName}",
                        CreatedBy    = user,
                        CreatedDate  = DateTime.Now
                    });
                }

                // 5) Fire → Fire Raporu (WasteManagement) — tüketilen ile üretilen farkı
                var fire = consumedKg - producedKg;
                if (fire > 0)
                {
                    var atkCount = await _context.WasteManagements.CountAsync(w => w.CreatedDate.Year == DateTime.Now.Year);
                    _context.WasteManagements.Add(new WasteManagement
                    {
                        WasteCode      = $"ATK{DateTime.Now.Year}-{atkCount + 1:D3}",
                        Category       = WasteCategory.KesimArtigi,
                        Description    = $"Yarı mamül dönüşüm firesi: {srcName} → {target.ProductName}",
                        Quantity       = fire,
                        Unit           = "kg",
                        DisposalDate   = when,
                        DisposalMethod = "Geri Dönüşüm",
                        Notes          = $"Dönüşüm parti {partiNo}",
                        CreatedBy      = user,
                        CreatedDate    = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Dönüşüm tamam: {producedKg:N2} kg {target.ProductName} üretildi (Parti {partiNo}).",
                    partiNo,
                    kalanKaynak = source.CurrentWeight
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    // ── View modelleri (dynamic ViewBag yerine public tipler) ──────────────
    public class ConvSourceVM
    {
        public int Id { get; set; }
        public string SerialNo { get; set; } = "";
        public decimal CurrentWeight { get; set; }
        public string FscType { get; set; } = "";
        public string? UrunKod { get; set; }
        public string UrunAd { get; set; } = "";
    }

    public class ConvTargetVM
    {
        public int Id { get; set; }
        public string Kod { get; set; } = "";
        public string Ad { get; set; } = "";
    }

    public class ConvRecentVM
    {
        public DateTime Tarih { get; set; }
        public string Parti { get; set; } = "";
        public string Hedef { get; set; } = "";
        public string FscType { get; set; } = "";
        public decimal Kg { get; set; }
        public decimal Kalan { get; set; }
        public decimal Fire { get; set; }
        public string KaynakSerial { get; set; } = "—";
        public string? KaynakKod { get; set; }
        public string? KaynakAd { get; set; }
    }
}
