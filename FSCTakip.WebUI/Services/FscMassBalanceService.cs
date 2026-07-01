using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Services
{
    /// <summary>
    /// FSC Kütle Dengesi — Ham+YM+Burgu Sap scope için tek, merkezi hesap.
    /// Tüm sayfa ve kartlar bu servisi kullanır; filtre mantığı TEK yerde.
    ///
    /// Kurallar:
    ///   Giriş  = satın alınan lotlar (DispatchNo/InvoiceNo/IsOpeningStock)
    ///             SourceSerialId IS NULL + !PartiNo.StartsWith("YM")
    ///             → Sum(FscSerial.InitialWeight)
    ///   Kalan  = scope'taki tüm seriler (dönüşüm YM dahil) CurrentWeight > 0
    ///             → Sum(FscSerial.CurrentWeight)
    ///   Tüketim = Giriş - Kalan (türetilmiş; dönüşüm YM Kalan'da göründüğü için
    ///              tüketim = sadece üretim tüketimi + dönüşüm firesi)
    /// </summary>
    public class FscMassBalance
    {
        public decimal FscliGiris    { get; init; }
        public decimal FscsizGiris   { get; init; }
        public decimal FscliKalan    { get; init; }
        public decimal FscsizKalan   { get; init; }

        public decimal FscliTuketim  => FscliGiris  - FscliKalan;
        public decimal FscsizTuketim => FscsizGiris - FscsizKalan;
        public decimal ToplamGiris   => FscliGiris  + FscsizGiris;
        public decimal ToplamKalan   => FscliKalan  + FscsizKalan;
        public decimal ToplamTuketim => ToplamGiris - ToplamKalan;

        /// <summary>ViewData'ya 6 değeri yazar — partial _FscStokOzeti.cshtml bunu okur.</summary>
        public void ApplyToViewData(Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary vd)
        {
            vd["FscliGiris"]    = FscliGiris;
            vd["FscliTuketim"]  = FscliTuketim;
            vd["FscliKalan"]    = FscliKalan;
            vd["FscsizGiris"]   = FscsizGiris;
            vd["FscsizTuketim"] = FscsizTuketim;
            vd["FscsizKalan"]   = FscsizKalan;
        }
    }

    public static class FscMassBalanceService
    {
        // Ham+YM+BS kapsam — SQL'e gönderilen sabit liste
        private static readonly string[] ScopeGroups =
            { "HAMMADDE", "YARI MAMUL", "YARI MAMÜL", "BURGU SAP" };

        /// <summary>
        /// "Gerçek satın alma lotu" filtresi — tüm sayfalarda ortak kural.
        /// AnaOzet ve FscMassBalance aynı logu sayar; tutarsızlık olmaz.
        /// </summary>
        public static IQueryable<FscLot> ApplyHamLotGirisFilter(
            IQueryable<FscLot> query,
            AppDbContext context) =>
            query.Where(l =>
                l.SourceSerialId == null
                && !l.PartiNo.StartsWith("YM")
                && (l.DispatchNo != null
                    || l.InvoiceNo != null
                    || context.FscSerials.Any(s => s.LotId == l.Id && s.IsOpeningStock)));

        /// <summary>
        /// Tek sorgu çifti (Giriş + Kalan) — tüm FSC Kütle Dengesi değerlerini hesaplar.
        /// Her controller bu metodu çağırır; ayrı sorgu yazmaz.
        /// </summary>
        public static async Task<FscMassBalance> ComputeAsync(AppDbContext context)
        {
            // --- Giriş: yalnızca gerçek satın alma lotları ---
            var girisRows = await context.FscSerials
                .Include(s => s.Lot).ThenInclude(l => l.FscType)
                .Include(s => s.Lot).ThenInclude(l => l.Product).ThenInclude(p => p!.ProductGroup)
                .Where(s =>
                    s.Lot.SourceSerialId == null                          // dönüşüm output değil
                    && !s.Lot.PartiNo.StartsWith("YM")                    // YM prefix'li lot değil
                    && (s.Lot.DispatchNo != null                          // irsaliye VEYA
                        || s.Lot.InvoiceNo != null                        // fatura VEYA
                        || s.Lot.Serials.Any(x => x.IsOpeningStock))      // devir (açılış stoku)
                    && s.Lot.Product != null
                    && s.Lot.Product.ProductGroup != null
                    && ScopeGroups.Contains(s.Lot.Product.ProductGroup.GroupName.ToUpper()))
                .GroupBy(s => s.Lot.FscType!.Name.ToLower().Contains("siz") ? "FSCSIZ" : "FSCLI")
                .Select(g => new { Tip = g.Key, Val = g.Sum(s => s.InitialWeight) })
                .ToListAsync();

            // --- Kalan: scope'taki tüm seriler (satın alma + dönüşüm YM) ---
            var kalanRows = await context.FscSerials
                .Include(s => s.Lot).ThenInclude(l => l.FscType)
                .Include(s => s.Lot).ThenInclude(l => l.Product).ThenInclude(p => p!.ProductGroup)
                .Where(s =>
                    s.CurrentWeight > 0
                    && s.Lot.Product != null
                    && s.Lot.Product.ProductGroup != null
                    && ScopeGroups.Contains(s.Lot.Product.ProductGroup.GroupName.ToUpper()))
                .GroupBy(s => s.Lot.FscType!.Name.ToLower().Contains("siz") ? "FSCSIZ" : "FSCLI")
                .Select(g => new { Tip = g.Key, Val = g.Sum(s => s.CurrentWeight) })
                .ToListAsync();

            return new FscMassBalance
            {
                FscliGiris  = girisRows.FirstOrDefault(x => x.Tip == "FSCLI")?.Val  ?? 0m,
                FscsizGiris = girisRows.FirstOrDefault(x => x.Tip == "FSCSIZ")?.Val ?? 0m,
                FscliKalan  = kalanRows.FirstOrDefault(x => x.Tip == "FSCLI")?.Val  ?? 0m,
                FscsizKalan = kalanRows.FirstOrDefault(x => x.Tip == "FSCSIZ")?.Val ?? 0m,
            };
        }
    }
}
