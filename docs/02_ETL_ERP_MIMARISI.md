# FSCTakip.Erp — ETL / ERP Entegrasyon Mimarisi

## Genel Yaklaşım

FSC uygulaması firmanın mevcut ERP'siyle **çift yönlü** çalışabilecek şekilde tasarlanmalıdır:

```
[Firma ERP'si]
    ↓ ETL Pull (Otomatik veya Manuel)
[Staging Tabloları]
    ↓ Doğrulama + Eşleştirme
[FSC Operasyonel Tablolar]
    ↓ FSC İşlemleri
[FSC Raporlama + Denetim]
```

## Desteklenecek ERP Sistemleri

| ERP | Bağlantı Yöntemi | Yaygınlık |
|-----|-----------------|-----------|
| Logo Tiger / Go / Netsis | SQL Server direkt bağlantı | Çok yaygın |
| SAP B1 | Service Layer API veya SQL | Orta |
| Microsoft Dynamics 365 BC | OData API | Az |
| Mikro ERP | SQL Server direkt | Yaygın |
| Özel ERP | SQL / REST API / Excel import | Değişken |

---

## Yeni Veritabanı Tabloları (ETL için)

### 1. `ErpConnections` — ERP Bağlantı Tanımları
```csharp
public class ErpConnection : BaseEntity
{
    public string Name { get; set; }              // "Logo Tiger - Ana Firma"
    public string ErpType { get; set; }           // Logo, SAP, Dynamics, Custom, Excel
    public string ConnectionMethod { get; set; }  // SqlServer, RestApi, ExcelImport
    
    // SQL Server bağlantısı için:
    public string? SqlServer { get; set; }
    public string? SqlDatabase { get; set; }
    public string? SqlUsername { get; set; }
    public string? SqlPasswordEncrypted { get; set; }  // AES şifreli
    
    // REST API için:
    public string? ApiBaseUrl { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiSecretEncrypted { get; set; }
    
    public bool IsActive { get; set; }
    public DateTime? LastSyncDate { get; set; }
    public string? LastSyncStatus { get; set; }   // Success, Failed, Partial
}
```

### 2. `ErpFieldMappings` — Alan Eşleştirme
```csharp
public class ErpFieldMapping : BaseEntity
{
    public int ErpConnectionId { get; set; }
    public string MappingType { get; set; }       // Product, Supplier, Customer, Purchase, Sales
    public string ErpFieldName { get; set; }      // ERP'deki alan adı
    public string FscFieldName { get; set; }      // FSC sistemindeki alan adı
    public string? TransformRule { get; set; }    // Dönüşüm kuralı (JSON)
    public string? DefaultValue { get; set; }     // Varsayılan değer
    public bool IsRequired { get; set; }
    
    public ErpConnection ErpConnection { get; set; }
}
```

### 3. `ErpSyncLogs` — Senkronizasyon Kayıtları
```csharp
public class ErpSyncLog : BaseEntity
{
    public int ErpConnectionId { get; set; }
    public string SyncType { get; set; }          // Full, Incremental
    public string EntityType { get; set; }        // Products, Purchases, Sales, vb.
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; }            // Running, Success, Failed
    public int RecordsRead { get; set; }
    public int RecordsInserted { get; set; }
    public int RecordsUpdated { get; set; }
    public int RecordsFailed { get; set; }
    public string? ErrorDetails { get; set; }     // Hata detayı (JSON)
    public string? TriggeredBy { get; set; }      // Kullanıcı veya "Otomatik"
    
    public ErpConnection ErpConnection { get; set; }
}
```

### 4. `ErpStagingPurchases` — Alım Staging
```csharp
public class ErpStagingPurchase
{
    public int Id { get; set; }
    public int SyncLogId { get; set; }
    
    // ERP'den gelen ham veriler
    public string ErpDocumentNo { get; set; }     // ERP'deki irsaliye/fatura no
    public string ErpDocumentType { get; set; }   // Receipt, Invoice
    public DateTime DocumentDate { get; set; }
    public string ErpSupplierCode { get; set; }   // ERP tedarikçi kodu
    public string? ErpSupplierName { get; set; }
    public string ErpProductCode { get; set; }    // ERP ürün kodu
    public string? ErpProductName { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; }
    public decimal? UnitWeight { get; set; }      // Birim ağırlık (bobin için)
    public decimal? TotalWeight { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Currency { get; set; }
    public string? LotNumber { get; set; }        // ERP'deki lot/parti no
    public string? SerialNumber { get; set; }     // ERP'deki seri no
    public string? WarehouseCode { get; set; }
    
    // Eşleştirme sonuçları
    public int? MappedSupplierId { get; set; }    // FSC Supplier.Id
    public int? MappedProductId { get; set; }     // FSC Product.Id
    public int? MappedWarehouseId { get; set; }
    
    // İşlem durumu
    public string Status { get; set; } = "Bekliyor"; // Bekliyor, Eşleşti, Aktarıldı, Hatalı
    public string? ValidationErrors { get; set; }    // JSON hata listesi
    public int? FscLotId { get; set; }               // Oluşturulan FSC lot ID
    public DateTime ImportedAt { get; set; } = DateTime.Now;
}
```

### 5. `ErpStagingSales` — Satış Staging
```csharp
public class ErpStagingSale
{
    public int Id { get; set; }
    public int SyncLogId { get; set; }
    
    public string ErpDocumentNo { get; set; }
    public string ErpDocumentType { get; set; }   // SalesDispatch, Invoice
    public DateTime DocumentDate { get; set; }
    public string ErpCustomerCode { get; set; }
    public string? ErpCustomerName { get; set; }
    public string ErpProductCode { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Currency { get; set; }
    public string? WarehouseCode { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? TruckPlate { get; set; }
    
    public int? MappedCustomerId { get; set; }
    public int? MappedProductId { get; set; }
    
    public string Status { get; set; } = "Bekliyor";
    public string? ValidationErrors { get; set; }
    public int? FscSalesOrderId { get; set; }
    public DateTime ImportedAt { get; set; } = DateTime.Now;
}
```

### 6. `ErpStagingProducts` — Ürün Staging
```csharp
public class ErpStagingProduct
{
    public int Id { get; set; }
    public int SyncLogId { get; set; }
    
    public string ErpProductCode { get; set; }
    public string ErpProductName { get; set; }
    public string? ErpProductGroup { get; set; }
    public string? ErpUnit { get; set; }
    public decimal? Grammage { get; set; }
    public decimal? Width { get; set; }
    public string? Color { get; set; }
    public bool IsActive { get; set; }
    
    public int? MappedProductId { get; set; }
    public string Status { get; set; } = "Bekliyor";
    public string? ValidationErrors { get; set; }
    public DateTime ImportedAt { get; set; } = DateTime.Now;
}
```

---

## ETL Servis Katmanı

### IErpEtlService Interface
```csharp
public interface IErpEtlService
{
    Task<ErpSyncLog> PullProductsAsync(int connectionId);
    Task<ErpSyncLog> PullPurchasesAsync(int connectionId, DateTime? fromDate = null);
    Task<ErpSyncLog> PullSalesAsync(int connectionId, DateTime? fromDate = null);
    Task<int> ProcessStagingPurchasesAsync(int syncLogId);
    Task<int> ProcessStagingSalesAsync(int syncLogId);
    Task<List<string>> ValidateStagingRecord(ErpStagingPurchase record);
}
```

### Logo Tiger SQL Sorguları (Örnek)

**Alım irsaliyeleri (Logo Tiger / Go):**
```sql
-- Logo Tiger STOK tabloları
SELECT 
    STFICHE.FICHENO AS ErpDocumentNo,
    STFICHE.DATE_ AS DocumentDate,
    CLCARD.CODE AS ErpSupplierCode,
    CLCARD.DEFINITION_ AS ErpSupplierName,
    ITEMS.CODE AS ErpProductCode,
    ITEMS.NAME AS ErpProductName,
    STLINE.AMOUNT AS Quantity,
    STLINE.UNITPRICE AS UnitPrice,
    STLINE.TOTAL AS TotalAmount,
    STLINE.UINFO1 AS UnitWeight,
    UNITSETF.CODE AS Unit,
    STFICHE.WAREHOUSEREF AS WarehouseCode
FROM LG_{FIRMA_NO}_01_STFICHE STFICHE
    INNER JOIN LG_{FIRMA_NO}_01_STLINE STLINE ON STFICHE.LOGICALREF = STLINE.STFICHEREF
    INNER JOIN LG_{FIRMA_NO}_ITEMS ITEMS ON STLINE.STOCKREF = ITEMS.LOGICALREF
    LEFT JOIN LG_{FIRMA_NO}_CLCARD CLCARD ON STFICHE.CLIENTREF = CLCARD.LOGICALREF
    LEFT JOIN LG_{FIRMA_NO}_UNITSETF UNITSETF ON STLINE.USREF = UNITSETF.LOGICALREF
WHERE STFICHE.TRCODE = 1  -- Alım irsaliyesi
  AND STFICHE.DATE_ >= @FromDate
ORDER BY STFICHE.DATE_ DESC
```

**Satış irsaliyeleri:**
```sql
SELECT 
    STFICHE.FICHENO AS ErpDocumentNo,
    STFICHE.DATE_ AS DocumentDate,
    CLCARD.CODE AS ErpCustomerCode,
    CLCARD.DEFINITION_ AS ErpCustomerName,
    ITEMS.CODE AS ErpProductCode,
    STLINE.AMOUNT AS Quantity,
    STLINE.UNITPRICE AS UnitPrice,
    STFICHE.SPECODE AS TruckPlate
FROM LG_{FIRMA_NO}_01_STFICHE STFICHE
    INNER JOIN LG_{FIRMA_NO}_01_STLINE STLINE ON STFICHE.LOGICALREF = STLINE.STFICHEREF
    INNER JOIN LG_{FIRMA_NO}_ITEMS ITEMS ON STLINE.STOCKREF = ITEMS.LOGICALREF
    LEFT JOIN LG_{FIRMA_NO}_CLCARD CLCARD ON STFICHE.CLIENTREF = CLCARD.LOGICALREF
WHERE STFICHE.TRCODE = 8  -- Satış irsaliyesi
  AND STFICHE.DATE_ >= @FromDate
```

**Netsis (örnek):**
```sql
SELECT 
    STHAR.FISNO AS ErpDocumentNo,
    STHAR.TARIH AS DocumentDate,
    STHAR.FIRMAREF AS ErpSupplierCode,
    STHAR.STOKKODU AS ErpProductCode,
    STHAR.MIKTAR AS Quantity,
    STHAR.FIYAT AS UnitPrice
FROM STHAR
WHERE STHAR.HTUR = 'A'  -- Alım
  AND STHAR.TARIH >= @FromDate
```

---

## ETL Controller (EtlController)

### Sayfalar
```
GET  /Etl/Index              → ETL ana paneli (bağlantılar + son senkronizasyonlar)
GET  /Etl/Connections        → ERP bağlantı listesi
POST /Etl/SaveConnection     → Bağlantı kaydet/güncelle
GET  /Etl/TestConnection/{id} → Bağlantı testi
POST /Etl/RunSync/{id}       → Manuel senkronizasyon başlat
GET  /Etl/SyncLogs           → Senkronizasyon geçmişi
GET  /Etl/SyncLogDetail/{id} → Log detayı
GET  /Etl/Staging            → Staging tablosu görüntüleme
POST /Etl/ProcessStaging/{id} → Staging'i onaylayıp aktarma
GET  /Etl/Mappings/{id}      → Alan eşleştirme yönetimi
```

---

## Excel Import (ERP'siz Firmalar İçin)

ERP'si olmayan veya bağlantı kurulamayan firmalar için Excel şablonu:

### Alım Şablonu (purchases_template.xlsx)
| Sütun | Zorunlu | Açıklama |
|-------|---------|----------|
| IrsaliyeNo | Evet | ERP irsaliye numarası |
| FaturaNo | Hayır | ERP fatura numarası |
| Tarih | Evet | dd.MM.yyyy formatı |
| TedarikciKodu | Evet | FSC sistemindeki tedarikçi kodu |
| UrunKodu | Evet | FSC sistemindeki ürün kodu |
| LotNo | Evet | Lot numarası |
| SeriNo | Evet | Seri/bobin numarası |
| BaslangicAgirlik | Evet | kg cinsinden |
| BirimFiyat | Hayır | |
| ParaBirimi | Hayır | TRY varsayılan |

### Satış Şablonu (sales_template.xlsx)
| Sütun | Zorunlu | Açıklama |
|-------|---------|----------|
| IrsaliyeNo | Evet | |
| FaturaNo | Hayır | |
| Tarih | Evet | |
| MusteriKodu | Evet | |
| UrunKodu | Evet | |
| Miktar | Evet | |
| Birim | Evet | ADET/KG vb. |
| AracPlaka | Hayır | |

---

## Otomatik Senkronizasyon (Background Service)

```csharp
// Program.cs'e eklenecek
builder.Services.AddHostedService<ErpSyncBackgroundService>();

// ErpSyncBackgroundService.cs
public class ErpSyncBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Her gece 02:00'de çalış
            var now = DateTime.Now;
            var nextRun = now.Date.AddDays(1).AddHours(2);
            await Task.Delay(nextRun - now, stoppingToken);
            
            var connections = await _repo.GetActiveConnectionsAsync();
            foreach (var conn in connections)
            {
                await _etlService.PullPurchasesAsync(conn.Id, DateTime.Today.AddDays(-1));
                await _etlService.PullSalesAsync(conn.Id, DateTime.Today.AddDays(-1));
            }
        }
    }
}
```

---

## Güvenlik Notları

1. **SQL şifreler** `ErpConnection.SqlPasswordEncrypted` alanında AES-256 ile şifreli saklanmalı
2. **ERP SQL bağlantısı** `READ ONLY` yetkili ayrı bir kullanıcıyla yapılmalı
3. **Staging kayıtları** işlendikten sonra arşivlenebilir (silinmez)
4. **API key'ler** `appsettings.json` değil, Azure Key Vault / ortam değişkenleri ile saklanmalı
5. **Bağlantı testi** `TestConnection` metodu sadece `SELECT 1` sorgusu çalıştırır

---

## Veri Kalitesi Kuralları

### Doğrulama (Validation)
- Tedarikçi kodu eşleşmiyorsa: staging `Hatalı`, kullanıcıya uyarı
- Ürün kodu eşleşmiyorsa: yeni ürün önerisi veya red
- FSC sertifika tarihi geçmişse: uyarı (bloklama değil)
- Lot no zaten mevcutsa: duplicate kontrolü

### Eşleştirme Stratejisi
1. Birebir kod eşleştirme (ERP kodu = FSC kodu)
2. Manuel eşleştirme tablosu (ErpFieldMappings)
3. Fuzzy match önerisi (Levenshtein distance ≤ 2)
4. Yeni kayıt oluşturma (kullanıcı onayı ile)
