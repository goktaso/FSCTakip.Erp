using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Controllers
{
    /// <summary>
    /// Birim dönüşüm parametrelerini yönetir.
    /// Örn: 1 MT (metre) BURGU SAP = 0.0045 KG
    /// </summary>
    public class UnitConversionController : BaseController
    {
        public UnitConversionController(AppDbContext context) : base(context) { }

        // GET /UnitConversion
        public async Task<IActionResult> Index()
        {
            var deny = RequireRead("SETTINGS");
            if (deny != null) return deny;

            ViewData["Title"] = "Birim Dönüşüm Parametreleri";

            var list = await _context.UnitConversions
                .Include(u => u.ProductGroup)
                .Include(u => u.Product)
                .OrderBy(u => u.FromUnit)
                .ThenBy(u => u.Description)
                .ToListAsync();

            ViewBag.ProductGroups = await _context.ProductGroups.OrderBy(g => g.GroupName).ToListAsync();
            ViewBag.Products = await _context.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.ProductName)
                .Select(p => new { p.Id, p.ProductName, p.ProductCode, p.Unit })
                .ToListAsync();

            // Özet: kaç seri dönüştürülmemiş durumda
            var pendingCount = await _context.FscSerials
                .Include(s => s.Lot).ThenInclude(l => l.Product)
                .CountAsync(s => s.OriginalUnit == null
                    && s.Lot.Product != null
                    && s.Lot.Product.Unit != "KG"
                    && s.Lot.Product.Unit != "KG ");

            ViewBag.PendingCount = pendingCount;

            return View(list);
        }

        // POST /UnitConversion/Save
        [HttpPost]
        public async Task<IActionResult> Save(UnitConversion model)
        {
            var deny = RequireWrite("SETTINGS");
            if (deny != null) return deny;

            if (string.IsNullOrWhiteSpace(model.FromUnit))
                return Json(new { success = false, message = "Kaynak birim zorunludur." });
            if (model.Factor <= 0)
                return Json(new { success = false, message = "Çarpan sıfırdan büyük olmalıdır." });
            if (string.IsNullOrWhiteSpace(model.ToUnit))
                model.ToUnit = "KG";

            model.FromUnit = model.FromUnit.Trim().ToUpperInvariant();
            model.ToUnit   = model.ToUnit.Trim().ToUpperInvariant();

            try
            {
                if (model.Id == 0)
                {
                    model.IsActive    = true;
                    model.CreatedDate = DateTime.Now;
                    model.CreatedBy   = "SYSTEM";
                    _context.UnitConversions.Add(model);
                }
                else
                {
                    var existing = await _context.UnitConversions.FindAsync(model.Id);
                    if (existing == null) return Json(new { success = false, message = "Kayıt bulunamadı." });
                    existing.FromUnit      = model.FromUnit;
                    existing.ToUnit        = model.ToUnit;
                    existing.Factor        = model.Factor;
                    existing.Description   = model.Description;
                    existing.ProductGroupId = model.ProductGroupId;
                    existing.ProductId     = model.ProductId;
                    existing.UpdatedDate   = DateTime.Now;
                    existing.UpdatedBy     = "SYSTEM";
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Dönüşüm parametresi kaydedildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var deny = RequireDelete("SETTINGS");
            if (deny != null) return deny;

            var item = await _context.UnitConversions.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Kayıt bulunamadı." });
            _context.UnitConversions.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var deny = RequireWrite("SETTINGS");
            if (deny != null) return deny;

            var item = await _context.UnitConversions.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Kayıt bulunamadı." });
            item.IsActive    = !item.IsActive;
            item.UpdatedDate = DateTime.Now;
            item.UpdatedBy   = "SYSTEM";
            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = item.IsActive });
        }

        // ── Ön izleme: dönüştürülecek seriler ─────────────────────────────────
        // GET /UnitConversion/Preview
        public async Task<IActionResult> Preview()
        {
            var deny = RequireRead("SETTINGS");
            if (deny != null) return deny;

            var conversions = await _context.UnitConversions.Where(c => c.IsActive).ToListAsync();

            var serials = await _context.FscSerials
                .Include(s => s.Lot).ThenInclude(l => l.Product).ThenInclude(p => p!.ProductGroup)
                .Include(s => s.Lot).ThenInclude(l => l.Supplier)
                .Include(s => s.ProductionDetails)
                .Where(s => s.OriginalUnit == null) // henüz dönüştürülmemiş
                .ToListAsync();

            var rows = serials
                .Select(s =>
                {
                    var unit = s.Lot.Product?.Unit?.Trim().ToUpperInvariant() ?? "";
                    var factor = FindFactor(conversions, unit, s.Lot.Product?.Id, s.Lot.Product?.ProductGroupId);
                    return new SerialConversionPreview
                    {
                        SerialId        = s.Id,
                        SerialNo        = s.SerialNo,
                        LotPartiNo      = s.Lot.PartiNo,
                        ProductName     = s.Lot.Product?.ProductName ?? "—",
                        SupplierName    = s.Lot.Supplier?.Name ?? "—",
                        OriginalQty     = s.InitialWeight,
                        OriginalUnit    = string.IsNullOrEmpty(unit) ? "—" : unit,
                        Factor          = factor,
                        ConvertedKg     = factor.HasValue ? s.InitialWeight * factor.Value : null,
                        CurrentOriginal = s.CurrentWeight,
                        CurrentKg       = factor.HasValue ? s.CurrentWeight * factor.Value : null,
                        HasFactor       = factor.HasValue
                    };
                })
                .Where(r => !string.IsNullOrEmpty(r.OriginalUnit) && r.OriginalUnit != "KG" && r.OriginalUnit != "—")
                .OrderBy(r => r.ProductName)
                .ToList();

            ViewData["Title"] = "Dönüşüm Önizlemesi";
            return View(rows);
        }

        // ── Retro-aktif uygulama ───────────────────────────────────────────────
        // POST /UnitConversion/ApplyAll
        [HttpPost]
        public async Task<IActionResult> ApplyAll()
        {
            if (!IsAdminUser)
                return Json(new { success = false, message = "Bu işlem yalnızca admin tarafından yapılabilir." });

            var conversions = await _context.UnitConversions.Where(c => c.IsActive).ToListAsync();
            if (!conversions.Any())
                return Json(new { success = false, message = "Aktif dönüşüm parametresi bulunamadı." });

            var serials = await _context.FscSerials
                .Include(s => s.Lot).ThenInclude(l => l.Product)
                .Include(s => s.ProductionDetails)
                .Where(s => s.OriginalUnit == null) // sadece henüz dönüştürülmemişler
                .ToListAsync();

            int converted = 0, skipped = 0;

            foreach (var serial in serials)
            {
                var unit = serial.Lot.Product?.Unit?.Trim().ToUpperInvariant() ?? "";
                if (string.IsNullOrEmpty(unit) || unit == "KG") { skipped++; continue; }

                var factor = FindFactor(conversions, unit, serial.Lot.Product?.Id, serial.Lot.Product?.ProductGroupId);
                if (!factor.HasValue) { skipped++; continue; }

                // Orijinal değerleri sakla
                serial.OriginalQuantity = serial.InitialWeight;
                serial.OriginalUnit     = unit;

                // KG'ye çevir
                serial.InitialWeight = serial.OriginalQuantity.Value * factor.Value;
                serial.CurrentWeight = serial.CurrentWeight * factor.Value; // CurrentWeight = InitialWeight - consumed, oran korunur

                serial.UpdatedDate = DateTime.Now;
                serial.UpdatedBy   = "UNIT_CONV";
                converted++;
            }

            if (converted > 0)
            {
                await _context.SaveChangesAsync();

                // ProductionDetail.ConsumedWeight ve WasteWeight'i de dönüştür
                // (aynı ürün + aynı dönem)
                var serialIds = serials
                    .Where(s => s.OriginalUnit != null)
                    .Select(s => s.Id)
                    .ToList();

                if (serialIds.Any())
                {
                    var prodDetails = await _context.ProductionDetails
                        .Include(d => d.FscSerial).ThenInclude(s => s.Lot).ThenInclude(l => l.Product)
                        .Where(d => serialIds.Contains(d.FscSerialId) && !d.UnitConverted)
                        .ToListAsync();

                    foreach (var pd in prodDetails)
                    {
                        var unit = pd.FscSerial?.Lot?.Product?.Unit?.Trim().ToUpperInvariant() ?? "";
                        var factor = FindFactor(conversions, unit, pd.FscSerial?.Lot?.Product?.Id, pd.FscSerial?.Lot?.Product?.ProductGroupId);
                        if (!factor.HasValue) continue;

                        pd.ConsumedWeight   = pd.ConsumedWeight * factor.Value;
                        pd.WasteWeight      = pd.WasteWeight    * factor.Value;
                        pd.UnitConverted    = true;
                        pd.UpdatedDate      = DateTime.Now;
                        pd.UpdatedBy        = "UNIT_CONV";
                    }
                    await _context.SaveChangesAsync();
                }
            }

            return Json(new {
                success = true,
                converted,
                skipped,
                message = $"{converted} seri dönüştürüldü, {skipped} seri atlandı (KG veya parametre yok)."
            });
        }

        // ── Yardımcı: en uygun dönüşüm katsayısını bul ───────────────────────
        public static decimal? FindFactor(
            IEnumerable<UnitConversion> conversions,
            string fromUnit,
            int? productId,
            int? productGroupId)
        {
            if (string.IsNullOrEmpty(fromUnit)) return null;

            var candidates = conversions
                .Where(c => c.IsActive
                    && c.FromUnit.Equals(fromUnit, StringComparison.OrdinalIgnoreCase)
                    && c.ToUnit.Equals("KG", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Öncelik: ürün bazlı > grup bazlı > genel (ProductId=null AND ProductGroupId=null)
            return candidates
                .OrderByDescending(c => c.ProductId.HasValue    ? 2 :
                                        c.ProductGroupId.HasValue ? 1 : 0)
                .FirstOrDefault(c =>
                    (!c.ProductId.HasValue     || c.ProductId     == productId) &&
                    (!c.ProductGroupId.HasValue || c.ProductGroupId == productGroupId))
                ?.Factor;
        }
    }

    // ─── View Model ────────────────────────────────────────────────────────────
    public class SerialConversionPreview
    {
        public int     SerialId        { get; set; }
        public string  SerialNo        { get; set; } = "";
        public string  LotPartiNo      { get; set; } = "";
        public string  ProductName     { get; set; } = "";
        public string  SupplierName    { get; set; } = "";
        public decimal OriginalQty     { get; set; }
        public string  OriginalUnit    { get; set; } = "";
        public decimal? Factor         { get; set; }
        public decimal? ConvertedKg    { get; set; }
        public decimal  CurrentOriginal { get; set; }
        public decimal? CurrentKg      { get; set; }
        public bool    HasFactor       { get; set; }
    }
}
