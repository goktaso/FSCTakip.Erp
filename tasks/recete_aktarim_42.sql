/* Netsis ACORE23 -> FscErpDb recete aktarimi (Kapsam A: mevcut 42 mamul)
   - 54 eksik bilesen urun karti eklenir (URN-NNN devam)
   - mevcut 3 recete silinir
   - 106 recete satiri kurulur
   Beklenen sayilar tutmazsa ROLLBACK. */
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @now datetime2 = SYSUTCDATETIME();
DECLARE @base int = (SELECT MAX(TRY_CONVERT(int, SUBSTRING(ProductCode,5,10)))
                     FROM Products WHERE ProductCode LIKE 'URN-%');

/* 1) Eksik bilesen urun kartlari (1xxxx + 2xxxx + 4xxxx-BurguSap), sadece 42 mamulun ihtiyaci */
;WITH need AS (
  SELECT DISTINCT
         r.HAM_KODU AS kod,
         s.STOK_ADI AS ad,
         UPPER(LTRIM(RTRIM(ISNULL(NULLIF(s.OLCU_BR1,''),'KG')))) AS birim
  FROM ACORE23.dbo.TBLSTOKURM r
  JOIN ACORE23.dbo.TBLSTSABIT s ON s.STOK_KODU = r.HAM_KODU
  WHERE r.MAMUL_KODU LIKE '3%'
    AND EXISTS (SELECT 1 FROM Products p
                WHERE p.ExternalCode COLLATE DATABASE_DEFAULT = r.MAMUL_KODU COLLATE DATABASE_DEFAULT)
    AND ( r.HAM_KODU LIKE '1%' OR r.HAM_KODU LIKE '2%'
          OR (r.HAM_KODU LIKE '4%' AND s.STOK_ADI LIKE '%Burgu Sap%') )
    AND NOT EXISTS (SELECT 1 FROM Products p2
                    WHERE p2.ExternalCode COLLATE DATABASE_DEFAULT = r.HAM_KODU COLLATE DATABASE_DEFAULT)
)
INSERT INTO Products
  (PaperColorId, CreatedBy, CreatedDate, FscTypeId, IsActive, PaperTypeId,
   ProductCode, ProductGroupId, ProductName, Unit, PaperWidthId, PaperWeightId, SupplierId, ExternalCode)
SELECT NULL, 'NETSIS-ETL', @now, NULL, 1, NULL,
       'URN-' + RIGHT('000' + CAST(@base + ROW_NUMBER() OVER (ORDER BY kod) AS varchar(10)), 3),
       NULL, ad, birim, NULL, NULL, NULL, kod
FROM need;
DECLARE @added int = @@ROWCOUNT;

/* 2) Mevcut receteleri sil */
DELETE FROM ProductRecipes;

/* 3) Receteleri kur (parent ve child artik iki tarafta da mevcut) */
INSERT INTO ProductRecipes
  (ParentProductId, ChildProductId, StandardQuantity, Unit, IsActive, CreatedBy, CreatedDate)
SELECT pm.Id, pc.Id, r.MIKTAR,
       UPPER(LTRIM(RTRIM(ISNULL(NULLIF(s.OLCU_BR1,''),'KG')))),
       1, 'NETSIS-ETL', @now
FROM ACORE23.dbo.TBLSTOKURM r
JOIN ACORE23.dbo.TBLSTSABIT s ON s.STOK_KODU = r.HAM_KODU
JOIN Products pm ON pm.ExternalCode COLLATE DATABASE_DEFAULT = r.MAMUL_KODU COLLATE DATABASE_DEFAULT
JOIN Products pc ON pc.ExternalCode COLLATE DATABASE_DEFAULT = r.HAM_KODU COLLATE DATABASE_DEFAULT
WHERE r.MAMUL_KODU LIKE '3%'
  AND ( r.HAM_KODU LIKE '1%' OR r.HAM_KODU LIKE '2%'
        OR (r.HAM_KODU LIKE '4%' AND s.STOK_ADI LIKE '%Burgu Sap%') );
DECLARE @recipes int = @@ROWCOUNT;

SELECT 'EklenenUrun' AS Metrik, @added AS Deger
UNION ALL SELECT 'KurulanRecete', @recipes
UNION ALL SELECT 'ToplamUrun', (SELECT COUNT(*) FROM Products)
UNION ALL SELECT 'ToplamRecete', (SELECT COUNT(*) FROM ProductRecipes);

IF (@added = 54 AND @recipes = 106)
BEGIN
    COMMIT;
    PRINT '>>> COMMIT: aktarim basarili (54 urun + 106 recete).';
END
ELSE
BEGIN
    ROLLBACK;
    PRINT '>>> ROLLBACK: beklenmeyen sayilar, degisiklik yapilmadi.';
END
