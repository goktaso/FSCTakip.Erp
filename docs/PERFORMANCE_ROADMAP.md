# FSC Takip ERP — Performans Yol Haritası

> Şu an veri küçük → runtime C# hesaplama yeterli.
> Aşağıdaki eşikler aşıldığında sıradaki adıma geç.

---

## Aşama 0 — Hemen Uygula (Bugün)

**Ne:** 6 kritik index  
**Dosya:** `docs/sql/performance_indexes.sql`  
**Nasıl:** SSMS → FscErpDb → New Query → dosyayı yapıştır → Execute  
**Etki:** Mevcut sorguların 3–10x hızlanması, yük altında zaman aşımı riski ortadan kalkar  
**Gereksinim:** Sıfır kod değişikliği

---

## Aşama 1 — SQL View (Eşik: Lot > 500 VEYA sorgu > 500ms)

**Ne:** `vw_HamLotOzet` + `vw_YmLotOzet` view'leri  
**Dosya:** `docs/sql/vw_stock_summary.sql`  
**Nasıl:**
1. SSMS'te view'leri oluştur
2. `FSCTakip.Core/Dtos/` altına `HamLotOzetDto.cs` + `YmLotOzetDto.cs` ekle
3. `AppDbContext`'e keyless entity map ekle
4. `StockController.AnaOzet()` → Include zinciri yerine view sorgula

**Etki:** Controller kodu %70 küçülür, sorgu süresi sabit kalır (SQL optimizer devralır)  
**Gereksinim:** EF Core migration gerekmez (view'ler migration dışında)

**Ölçüm komutu (ne zaman geçileceğini anlamak için):**
```sql
SET STATISTICS TIME ON;
-- Ana Özet sorgusunu burada çalıştır
SET STATISTICS TIME OFF;
-- CPU time > 200ms VEYA elapsed time > 500ms → Aşama 1'e geç
```

---

## Aşama 2 — Indexed View (Eşik: Lot > 2.000 VEYA sorgu > 2s)

**Ne:** `vw_HamLotOzet` üzerine `WITH SCHEMABINDING` + clustered index  
**Nasıl:**
```sql
-- View'i schema-bound yeniden oluştur
CREATE OR ALTER VIEW vw_HamLotOzet WITH SCHEMABINDING AS ...

-- Materialized index
CREATE UNIQUE CLUSTERED INDEX IX_vw_HamLotOzet_LotId
    ON vw_HamLotOzet (LotId);
```
**Etki:** View artık disk'te fiziksel veri tutar — SELECT anında sonuç  
**Kısıt:** `WITH SCHEMABINDING` → tablo/kolon adı değiştiremezsin, dikkatli ol

---

## Aşama 3 — Staging Tablo + Nightly Job (Eşik: Lot > 10.000 VEYA çok kullanıcı)

**Ne:** `StockSummaryCache` tablosu, her gece 02:00'de dolan SQL Agent Job  
**Nasıl:**
```sql
CREATE TABLE StockSummaryCache (
    LotId       INT PRIMARY KEY,
    PartiNo     NVARCHAR(50),
    GirisKg     DECIMAL(18,4),
    YmKg        DECIMAL(18,4),
    TuketimKg   DECIMAL(18,4),
    FireKg      DECIMAL(18,4),
    KalanKg     DECIMAL(18,4),
    UpdatedAt   DATETIME2 DEFAULT GETDATE()
);

-- SQL Agent Job: her gece 02:00
-- TRUNCATE TABLE StockSummaryCache;
-- INSERT INTO StockSummaryCache SELECT * FROM vw_HamLotOzet;
```
**ASP.NET tarafı:**
- `IHostedService` veya Hangfire job ile tetikle
- Cache geçerliliği: sayfa başında `UpdatedAt > DATEADD(hour,-8,GETDATE())` kontrol et
- Bayat ise arka planda yenile, eski veri göster

**Uyarı:** Staging = stale data riski. Manuel lot girişinden sonra "Yenile" butonu ekle.

---

## Ölçüm Kontrol Listesi (Her Sprint Sonunda)

```sql
-- En yavaş 5 sorgu
SELECT TOP 5
    total_elapsed_time / execution_count AS avg_ms,
    SUBSTRING(text, 1, 100) AS sorgu
FROM sys.dm_exec_query_stats
CROSS APPLY sys.dm_exec_sql_text(sql_handle)
WHERE text LIKE '%FscLots%' OR text LIKE '%ProductionDetails%'
ORDER BY avg_ms DESC;
```

Sonuç > 500ms → Bir sonraki aşamaya geç.

---

## Özet

```
Şimdi    → Aşama 0 (index) — 10 dakika, sıfır risk
Lot 500+ → Aşama 1 (SQL View) — yarım gün, düşük risk
Lot 2K+  → Aşama 2 (Indexed View) — 1 gün, orta risk
Lot 10K+ → Aşama 3 (Staging) — 2-3 gün, yönetim gerekir
```

Şu an Aşama 0'dasınız. Bir sonraki adım için gerçek ölçüm verisi beklenir.
