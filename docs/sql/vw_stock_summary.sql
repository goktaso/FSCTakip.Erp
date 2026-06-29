-- ============================================================
-- FSC Takip ERP — Stok Özet View
-- Aşama 2: Lot > 500 VEYA sorgu > 500ms olduğunda uygula
-- Bu view EF Core'da keyless entity olarak map edilir.
-- ============================================================

USE FscErpDb;
GO

-- ── Ham Lot Özeti ────────────────────────────────────────────────
CREATE OR ALTER VIEW vw_HamLotOzet AS
SELECT
    l.Id                                          AS LotId,
    l.PartiNo,
    l.ArrivalDate,
    l.SupplierId,
    l.FscTypeId,
    l.ProductId,
    -- Giriş: lot'a ait tüm serilerin başlangıç ağırlığı
    ISNULL(SUM(s.InitialWeight), 0)               AS GirisKg,
    -- Tüketim: üretimde direkt tüketilen (YM dönüşüm dahil değil)
    ISNULL(SUM(pd.ConsumedWeight), 0)             AS TuketimKg,
    -- Fire
    ISNULL(SUM(pd.WasteWeight), 0)                AS FireKg,
    -- YM'e dönüşen: bu lot'un serilerini kaynak alan YM serilerinin toplamı
    ISNULL(SUM(yms.InitialWeight), 0)             AS YmKg,
    -- Kalan = Giriş - YM - Tüketim - Fire
    ISNULL(SUM(s.InitialWeight), 0)
        - ISNULL(SUM(pd.ConsumedWeight), 0)
        - ISNULL(SUM(pd.WasteWeight), 0)
        - ISNULL(SUM(yms.InitialWeight), 0)       AS KalanKg,
    COUNT(DISTINCT s.Id)                          AS SerialCount
FROM FscLots l
LEFT JOIN FscSerials s   ON s.LotId = l.Id
LEFT JOIN ProductionDetails pd ON pd.FscSerialId = s.Id
-- YM dönüşüm: bu serinin SourceSerialId olduğu YM lotlar
LEFT JOIN FscLots ymLot  ON ymLot.SourceSerialId = s.Id
LEFT JOIN FscSerials yms ON yms.LotId = ymLot.Id
WHERE l.SourceSerialId IS NULL          -- sadece ham malzeme lotları
GROUP BY l.Id, l.PartiNo, l.ArrivalDate, l.SupplierId, l.FscTypeId, l.ProductId;
GO

-- ── YM Lot Özeti ─────────────────────────────────────────────────
CREATE OR ALTER VIEW vw_YmLotOzet AS
SELECT
    l.Id                                          AS LotId,
    l.PartiNo,
    l.ArrivalDate,
    l.SourceSerialId,
    l.ProductId,
    l.FscTypeId,
    l.ConversionFireKg,
    -- YM girişi
    ISNULL(SUM(s.InitialWeight), 0)               AS GirisKg,
    -- YM tüketim (üretimde kullanılan)
    ISNULL(SUM(pd.ConsumedWeight), 0)             AS TuketimKg,
    -- YM fire
    ISNULL(SUM(pd.WasteWeight), 0)                AS FireKg,
    -- YM kalan
    ISNULL(SUM(s.InitialWeight), 0)
        - ISNULL(SUM(pd.ConsumedWeight), 0)
        - ISNULL(SUM(pd.WasteWeight), 0)          AS KalanKg
FROM FscLots l
LEFT JOIN FscSerials s   ON s.LotId = l.Id
LEFT JOIN ProductionDetails pd ON pd.FscSerialId = s.Id
WHERE l.SourceSerialId IS NOT NULL      -- sadece YM dönüşüm lotları
GROUP BY l.Id, l.PartiNo, l.ArrivalDate, l.SourceSerialId, l.ProductId, l.FscTypeId, l.ConversionFireKg;
GO

PRINT 'View''ler oluşturuldu: vw_HamLotOzet, vw_YmLotOzet';
GO

-- ── EF Core Keyless Entity (DataAccess katmanında) ───────────────
-- AppDbContext.cs içine eklenecek:
--
--   public DbSet<HamLotOzetDto> HamLotOzetler => Set<HamLotOzetDto>();
--   public DbSet<YmLotOzetDto>  YmLotOzeti    => Set<YmLotOzetDto>();
--
--   modelBuilder.Entity<HamLotOzetDto>().HasNoKey().ToView("vw_HamLotOzet");
--   modelBuilder.Entity<YmLotOzetDto>().HasNoKey().ToView("vw_YmLotOzet");
--
-- StockController.AnaOzet():
--   var rows = await _context.HamLotOzetler
--                  .Where(r => r.ArrivalDate >= startDate)
--                  .OrderByDescending(r => r.ArrivalDate)
--                  .ToListAsync();
