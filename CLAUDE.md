# FSCTakip.Erp — Claude Code Kılavuzu

## Proje Özeti

**FSCTakip.Erp**, kağıt ve ambalaj sektöründe FSC (Forest Stewardship Council) sertifikasyonu zorunluluğu olan firmalar için geliştirilmiş **web tabanlı ERP uygulamasıdır**. Kraft kağıt kullanıcılarına hammadde girişinden üretime, satışa ve FSC denetimine kadar tüm süreç izlenebilirliğini sağlar.

## Mimari

```
FSCTakip.Erp (Solution)
├── FSCTakip.Core          → Entity/Domain katmanı (17 entity + enum)
├── FSCTakip.DataAccess    → EF Core, AppDbContext, Migrations
├── FSCTakip.Business      → Servis katmanı (genişletilecek)
└── FSCTakip.WebUI         → ASP.NET Core 8 MVC, Controllers, Views
```

**Stack:** ASP.NET Core 8.0 MVC · SQL Server · Entity Framework Core · Bootstrap 5 · ClosedXML · jQuery · FontAwesome

**Veritabanı adı:** `FscErpDb`  
**Connection string:** `appsettings.json` → `ConnectionStrings:DefaultConnection`

## Mevcut Entity'ler (FSCTakip.Core/Entities/)

### Temel Varlıklar
| Entity | Tablo | Açıklama |
|--------|-------|----------|
| BaseEntity | — | Id, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate (abstract) |
| FscType | FscTypes | FSC-100, FSC-MIX sertifika tipleri |
| PaperType | PaperTypes | Kraft, Beyaz vb. kağıt türleri |
| PaperColor | PaperColors | Renk tanımları |
| PaperWeight | PaperWeights | Gramaj değerleri |
| PaperWidth | PaperWidths | Bobin en kodu ve değeri (mm) |
| Machine | Machines | Makine tanımları |
| BagType | BagTypes | Torba tipi (Kare Dip, V Kesim vb.) |
| ProductGroup | ProductGroups | Ürün grubu + otomatik kod aralığı |
| Warehouse | Warehouses | Depo tanımları |
| ProductGrammage | ProductGrammages | Gramaj referansı |

### Operasyonel Varlıklar
| Entity | Tablo | Açıklama |
|--------|-------|----------|
| Supplier | Suppliers | Tedarikçi + FSC kodu/geçerlilik |
| Customer | Customers | Müşteri + FSC lisans takibi |
| Product | Products | Ürün kartı (kod otomatik üretilir) |
| ProductRecipe | ProductRecipes | Reçete/BOM (çoktan-çoğa) |
| FscLot | FscLots | FSC lot — irsaliye/fatura PDF yolları dahil |
| FscSerial | FscSerials | Lot içindeki bireysel bobin/seri |
| WorkOrder | WorkOrders | İş emri |
| ProductionDetail | ProductionDetails | Seri bazlı tüketim + fire + üretim |
| StockMovement | StockMovements | Stok hareketi (4 tip: enum MovementType) |
| WasteManagement | WasteManagements | Fire/atık yönetimi |

### MovementType Enum
```csharp
ProductionEntry  = 1  // Üretimden depoya giriş
WarehouseTransfer = 2 // Depo transferi
SalesDispatch    = 3  // Müşteriye satış çıkışı
PurchaseEntry    = 4  // Tedarikçiden hammadde girişi
```

## Mevcut Controller'lar (FSCTakip.WebUI/Controllers/)

| Controller | Durum | Notlar |
|------------|-------|--------|
| BaseController | Tamamlandı | GeneralToggleStatus, ExportToExcel, ExportToCsv |
| HomeController | Tamamlandı | Dashboard |
| CustomersController | Tamamlandı | CRUD + auto-code MHS-XXX |
| SuppliersController | Tamamlandı | CRUD + auto-code TED-XXX |
| ProductsController | Tamamlandı | CRUD + Excel export |
| ProductController | Tamamlandı | BagTypes, Groups |
| PaperController | Tamamlandı | Types, Colors, FscTypes, Widths, Weights |
| MachineController | Tamamlandı | CRUD |
| SettingsController | Tamamlandı | Navigation hub |
| StockController | Stub | Geliştirilecek |
| PlanningController | Stub | Geliştirilecek |
| ShippingController | Stub | Geliştirilecek |

## Kod Konvansiyonları

### Otomatik Kod Üretimi
```csharp
// Pattern: PREFIX-NNN (3 basamaklı, sıfır doldurmalı)
"MHS-{count+1:D3}"  // Müşteriler
"TED-{count+1:D3}"  // Tedarikçiler
"L{yıl}-{count:D3}" // FscLot: L2026-001
"S{yıl}-{lotNo}-{seq:D2}" // FscSerial: S2026-001-01
```

### Türkçe Karakter İşleme
```csharp
// Büyük harf: SaveChangesAsync() içinde otomatik (tr-TR)
// Email normalizasyonu:
email.Replace("İ", "i").Replace("I", "ı").ToLowerInvariant()
// Telefon temizleme:
new string(phone.Where(char.IsDigit).ToArray())
```

### Controller Pattern
```csharp
// Her controller BaseController'dan türer
// JSON response: new { success = true/false, message = "..." }
// TempData["Error"] flash mesajlar için
// Dropdown'lar: PopulateDropdowns(ViewData) ile doldurulur
```

### Excel Export
```csharp
// ClosedXML kullanılır
// Header: Bold, koyu arka plan, beyaz metin
// Dosya adı: {DataType}_{DateTime:ddMMyyyy}.xlsx
// BaseController.ExportToExcel<T>() generic metod
```

### UI Konvansiyonları
- Bootstrap 5 modal dialog ile form (Add/Edit aynı modal)
- AJAX ile ToggleStatus (sayfayı yenilemeden durum değişimi)
- Status badge: badge-success (Aktif), badge-warning (Pasif)
- Sidebar sol menü — FSC yeşil (#1e3d14 arka plan)
- FontAwesome ikonlar
- Responsive tablo + filtre paneli
- Dashboard & Analytics: Kütle dengesi (Mass Balance) takibi için üstte 3'lü metrik kartları ve mini grafikler kullanılacak.
- Lot/Seri İşlemleri: Satır içi (inline) hesaplamalar ve dinamik badge'ler (FSC Mix/100%) ile kullanıcı anlık olarak kütle girdisini görecek.
- Estetik: Endüstriyel ERP ciddiyetinde, temiz padding'li, oval köşeli (rounded-3) modern kart tasarımları tercih edilecek.

## Dosya Yükleme Konvansiyonu

PDF belgeler (irsaliye, fatura) şu dizine kaydedilmeli:
```
wwwroot/uploads/
├── invoices/      → Faturalar (purchase + sales)
├── dispatches/    → İrsaliyeler
├── other/         → Diğer belgeler
```

Veritabanında sadece göreli yol saklanır: `uploads/invoices/2026/...pdf`  
FscLot entity'sinde `InvoicePdfPath` ve `DispatchPdfPath` alanları mevcuttur.

## Geliştirme Öncelik Sırası

1. **Faz 1 — İşlem Modülleri** (Kritik)
   - PurchaseController: Hammadde girişi + lot/seri kayıt
   - ProductionController: İş emri + üretim detayı + fire
   - SalesController: Satış irsaliyesi/fatura çıkışı
   - StockController: Stok durumu + depo transfer

2. **Faz 2 — Belge Yönetimi**
   - Dosya yükleme (PDF irsaliye/fatura)
   - Belge listesi + link ile görüntüleme

3. **Faz 3 — Raporlama**
   - FSC CoC (Chain of Custody) raporu
   - Lot takip raporu
   - Denetim özet raporu

4. **Faz 4 — ETL/ERP Entegrasyonu**
   - Staging tabloları
   - ERP bağlantı yönetimi
   - Otomatik senkronizasyon

## Migration Komutları

```bash
# WebUI projesini startup, DataAccess projesini migration hedefi olarak kullan
cd FSCTakip.DataAccess
dotnet ef migrations add MigrationName --startup-project ../FSCTakip.WebUI
dotnet ef database update --startup-project ../FSCTakip.WebUI
```

## Kullanım Kılavuzu Güncelleme Kuralı

**Her yeni sayfa veya modül tamamlandığında** `docs/KULLANIM_KILAVUZU.md` dosyası mutlaka güncellenmeli ve `FSCTakip.WebUI/Views/Guide/Index.cshtml` sayfasındaki TOC + heading haritası senkronize edilmelidir.

### Güncelleme Adımları (her modül bitiminde otomatik yap)

1. **`docs/KULLANIM_KILAVUZU.md`** — yeni bölüm ekle:
   - Bölüm başlığı: `## N. Sayfa Adı {#anchor-id}`
   - Sayfa URL'i ve menü yolu
   - ASCII topbar diyagramı (buton konumları dahil)
   - Özet kartlar (varsa)
   - İşlem adımları (yeni kayıt, düzenleme, silme, filtre, Excel)
   - Uyarı/ipucu kutuları (`> **⚠️**` veya `> **ℹ️**`)
   - Tablolar (alan açıklamaları gerekiyorsa)
   - İçindekiler tablosuna yeni satır ekle
   - Modül Durumu tablosunu güncelle
   - Versiyon numarasını artır (1.2 → 1.3 vb.)

2. **`FSCTakip.WebUI/Views/Guide/Index.cshtml`** — şunları güncelle:
   - TOC listesine yeni `<a class="toc-item">` satırı ekle (`toc-badge done` ile)
   - `map` nesnesine heading → anchor ID eşleşmesini ekle
   - Status bar sayısını güncelle

### Topbar ASCII Şablonu

Her sayfa için standart topbar diyagramı:
```
[≡] [+ Yeni Kayıt Ekle]   Sayfa Başlığı   [Filtrele] [Excel] [👤]
```
- Birincil aksiyon butonu (mavi gradient) sol üstte, hamburgerin hemen yanında
- Filtrele + Excel sağda

### Kılavuz Dosya Yapısı

```
docs/KULLANIM_KILAVUZU.md    ← Markdown içerik (v1.2+)
FSCTakip.WebUI/
  Views/Guide/
    Index.cshtml              ← TOC + heading haritası
  wwwroot/docs/               ← İndirme için kopyalanmış .md (opsiyonel)
```

---

## Önemli Notlar

- `AppDbContext.SaveChangesAsync()` içindeki string uppercase kodu tüm entity stringleri otomatik büyük harfe çevirir (tr-TR). Email alanlarında bu sorun çıkarır, o alanları SaveChanges içinde exclude etmek gerekir.
- `PaperWeight` entity'si `BaseEntity`'den türemiyor — manuel audit alanları var, dikkat et.
- `Machine` entity'si de `BaseEntity`'den türemiyor — ayrı audit alanları.
- FSC sertifikası takibi için `Supplier.FscExpiryDate` ve `Customer.FscExpiryDate` dashboard'da uyarı gösterilmeli.
- `ProductRecipe` çoka-çok ilişkisi `DeleteBehavior.Restrict` ile konfigüre edilmiş.
