# FSCTakip.Erp — Geliştirme Planı

## Mevcut Durum Özeti

**Tamamlanan:** Tüm master data (tanımlamalar) — Müşteri, Tedarikçi, Ürün, Makine, Kağıt parametreleri, Depo  
**Eksik:** Tüm işlem modülleri — Giriş, Üretim, Çıkış, Stok, Fire, Raporlama, Belge Yönetimi, ERP Entegrasyonu

---

## FAZ 1 — İşlem Modülleri (Öncelikli)

### 1.1 Hammadde Girişi (PurchaseController)
**Kapsam:** Tedarikçiden hammadde kabulü, FSC lot ve seri kaydı, belge yükleme

**Yeni/Güncellenecek Entity'ler:**
- `FscLot` → Mevcut, UI yazılacak
- `FscSerial` → Mevcut, UI yazılacak
- `StockMovement` (MovementType.PurchaseEntry) → Mevcut, UI yazılacak

**Yeni Alanlar gerekiyor (`FscLot` entity güncellemesi):**
```csharp
public decimal TotalWeight { get; set; }          // Toplam kabul ağırlığı (kg)
public int SerialCount { get; set; }               // Bobin adedi
public DateTime ArrivalDate { get; set; }          // Geliş tarihi
public string? TruckPlate { get; set; }            // Araç plakası
public decimal? InvoiceAmount { get; set; }        // Fatura tutarı
public string? Currency { get; set; } = "TRY";    // Para birimi
```

**Sayfalar:**
- `/Purchase/Index` — Hammadde giriş listesi (lot bazlı, tarih filtreli)
- `/Purchase/Create` — Yeni lot + seri girişi (wizard: 2 adım)
- `/Purchase/Detail/{lotId}` — Lot detayı (seriler + belgeler)
- `/Purchase/SerialEdit/{serialId}` — Seri düzenleme (ağırlık güncelleme)

**Özellikler:**
- Lot oluşturulduğunda otomatik `L{yıl}-{seq:D3}` numarası
- Lot altına N adet seri ekleme (toplu veya tek tek)
- Her seri için `InitialWeight` ve `CurrentWeight`
- İrsaliye ve fatura PDF yükleme (dosyaya kayıt + DB'ye yol)
- `StockMovement.PurchaseEntry` otomatik oluşturma
- Tedarikçi FSC durumu kontrolü (geçerlilik uyarısı)

---

### 1.2 Üretim Modülü (ProductionController)
**Kapsam:** İş emri, seri bazlı hammadde tüketimi, fire hesaplama, üretim kaydı

**Entity'ler:** `WorkOrder`, `ProductionDetail` — Mevcut, UI yazılacak

**Yeni Alanlar (`WorkOrder` güncellemesi):**
```csharp
public DateTime PlannedDate { get; set; }
public DateTime? StartDate { get; set; }
public DateTime? EndDate { get; set; }
public string Status { get; set; } = "Bekliyor"; // Bekliyor/Devam Ediyor/Tamamlandı/İptal
public string? Notes { get; set; }
public int? CustomerId { get; set; }              // Sipariş müşterisi (opsiyonel)
```

**Sayfalar:**
- `/Production/WorkOrders` — İş emri listesi (durum filtreli)
- `/Production/CreateWorkOrder` — Yeni iş emri oluşturma
- `/Production/WorkOrderDetail/{id}` — İş emri detayı + üretim giriş formu
- `/Production/AddProductionDetail` [POST] — Seri tüketim kaydı
- `/Production/WasteReport` — Fire raporu

**Üretim Hesaplama Mantığı:**
```
Tüketim Alanları (ConsumptionArea enum):
  TorbaGovde  = 1  // Ana torba gövdesi
  Sap         = 2  // Sap kısmı
  Etiket      = 3  // Etiket
  Yapiskan    = 4  // Yapışkan

Fire Hesabı:
  FireOranı = (ConsumedWeight - (ProducedQuantity * ÜrünGramajı / 1000)) / ConsumedWeight * 100
  
Bobin Tüketimi:
  FscSerial.CurrentWeight -= ConsumedWeight
  (CurrentWeight < 0 ise hata ver)
```

**Özellikler:**
- Seri seçiminde sadece ilgili FSC tipine uygun seriler listelenir
- Üretim sonrası `FscSerial.CurrentWeight` güncellenir
- Fire > belirlenen eşik değeri ise uyarı
- İş emri tamamlandığında `StockMovement.ProductionEntry` oluşturulur
- FSC Chain of Custody için: hangi lot/seriden ne üretildiği kaydı

---

### 1.3 Satış Modülü (SalesController)
**Kapsam:** Satış irsaliyesi, fatura, çıkış hareketi, FSC belgeli satış takibi

**Entity'ler:** `StockMovement` (MovementType.SalesDispatch) + yeni `SalesOrder`

**Yeni Entity (`SalesOrder`):**
```csharp
public class SalesOrder : BaseEntity
{
    public string SalesOrderNo { get; set; }      // SAT-2026-001
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ShipmentDate { get; set; }
    public string Status { get; set; } = "Bekliyor";
    public string? DispatchNo { get; set; }       // Sevk irsaliye no
    public string? InvoiceNo { get; set; }        // Fatura no
    public string? DispatchPdfPath { get; set; }
    public string? InvoicePdfPath { get; set; }
    public string? TruckPlate { get; set; }
    public string? Notes { get; set; }
    public bool IsFscRequired { get; set; }       // FSC belgeli satış mı?
    
    public Customer Customer { get; set; }
    public ICollection<SalesOrderLine> Lines { get; set; }
}

public class SalesOrderLine : BaseEntity
{
    public int SalesOrderId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; }
    public decimal? UnitPrice { get; set; }
    public int? WorkOrderId { get; set; }         // Hangi iş emrinden üretildi (izlenebilirlik)
    
    public SalesOrder SalesOrder { get; set; }
    public Product Product { get; set; }
}
```

**Sayfalar:**
- `/Sales/Index` — Satış siparişi listesi
- `/Sales/Create` — Yeni satış siparişi
- `/Sales/Detail/{id}` — Satış detayı + irsaliye/fatura yükleme
- `/Sales/Dispatch/{id}` — Sevk işlemi (stok düşme)
- `/Sales/FscSalesReport` — FSC belgeli satış raporu (denetim için)

---

### 1.4 Stok Modülü (StockController — Genişletme)

**Sayfalar:**
- `/Stock/Index` — Ürün bazlı mevcut stok durumu
- `/Stock/Movements` — Stok hareketleri (tarih + tip filtreli)
- `/Stock/RawMaterial` — Hammadde durumu (seri bazlı bobin listesi)
- `/Stock/Transfer` — Depo transferi
- `/Stock/Valuation` — Stok değerleme (opsiyonel)

**Stok Hesaplama:**
```sql
-- Ürün bazlı stok:
SELECT p.ProductCode, p.ProductName,
    SUM(CASE WHEN sm.Type IN (1,4) THEN sm.Quantity ELSE -sm.Quantity END) as Stock
FROM StockMovements sm
JOIN Products p ON sm.ProductId = p.Id
GROUP BY p.ProductCode, p.ProductName
```

---

## FAZ 2 — Belge Yönetimi

### 2.1 Document Upload Sistemi

**Yapı:**
```
wwwroot/uploads/
├── purchases/{yıl}/{ay}/     → Alım irsaliyeleri + faturaları
├── sales/{yıl}/{ay}/         → Satış irsaliyeleri + faturaları
├── other/                    → Diğer belgeler
```

**DocumentController:**
```csharp
[HttpPost] Upload(IFormFile file, string docType, int refId)
[HttpGet]  View(string path)     // PDF inline görüntüleme
[HttpGet]  Download(string path) // Dosya indirme
[HttpPost] Delete(int docId)
```

**Yeni Entity (`Document`):**
```csharp
public class Document : BaseEntity
{
    public string FileName { get; set; }
    public string FilePath { get; set; }        // wwwroot'a göre göreli yol
    public string DocumentType { get; set; }   // Invoice, Dispatch, Other
    public string ReferenceType { get; set; }  // FscLot, SalesOrder, vb.
    public int ReferenceId { get; set; }
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; }    // application/pdf
}
```

---

## FAZ 3 — FSC Raporlama

### 3.1 Zorunlu FSC Raporları

**Chain of Custody (CoC) Raporu:**
- Hammadde kaynağı → Lot → Seri → İş Emri → Ürün → Satış
- FSC denetçisine sunulacak format
- Tarih aralıklı filtreleme
- Excel/PDF export

**Lot İzleme Raporu:**
- Belirli bir lot'tan ne üretildi, kime satıldı
- `FscSerial → ProductionDetail → WorkOrder → SalesOrderLine → SalesOrder → Customer`

**Tedarikçi FSC Sertifika Raporu:**
- Geçerlilik tarihleri yaklaşan/geçen tedarikçiler
- Sertifika durumu dashboard

**Yıllık Özet Raporu (FSC Denetim):**
- Dönem bazlı giriş/çıkış/üretim özeti
- FSC ve Non-FSC ayrımı
- Fire oranları

### 3.2 ReportsController
```
GET /Reports/Index          → Rapor ana menüsü
GET /Reports/ChainOfCustody → CoC raporu
GET /Reports/LotTrace/{id}  → Lot izleme
GET /Reports/SupplierFsc    → Tedarikçi FSC durumu
GET /Reports/AnnualSummary  → Yıllık özet
GET /Reports/WasteSummary   → Fire özet raporu
```

---

## FAZ 4 — ETL / ERP Entegrasyonu

Detay için: `docs/02_ETL_ERP_MIMARISI.md`

---

## FAZ 5 — Çoklu Firma & SaaS Hazırlığı (Opsiyonel)

### 5.1 Multi-Tenant Yapı
- Her firma kendi veritabanı şemasına sahip (schema-based tenancy)
- Ya da `TenantId` kolonuyla tek DB (row-level tenancy)
- `TenantId` BaseEntity'ye eklenir

### 5.2 Kullanıcı Yönetimi
- ASP.NET Core Identity entegrasyonu
- Rol bazlı yetkilendirme: Admin, Depo, Satış, Üretim, Denetçi
- Program.cs'de Identity altyapısı mevcut ama kullanıcı tabloları yok

### 5.3 API Katmanı
- Minimal API veya Web API controller'lar
- ERP entegrasyonu için REST endpoint'ler
- API key authentication

---

## Sprint Tahmini

| Sprint | Kapsam | Süre |
|--------|--------|------|
| 1 | Hammadde Girişi (Purchase) + Belge Upload | 2 hafta |
| 2 | Üretim (WorkOrder + ProductionDetail + Fire) | 2 hafta |
| 3 | Satış (SalesOrder + SalesOrderLine) | 1.5 hafta |
| 4 | Stok ekranları + Dashboard | 1 hafta |
| 5 | FSC Raporları | 1.5 hafta |
| 6 | ETL/ERP Entegrasyonu — Temel | 2 hafta |
| 7 | Kullanıcı Yönetimi + Yetkilendirme | 1 hafta |
| **Toplam** | | **~11 hafta** |
