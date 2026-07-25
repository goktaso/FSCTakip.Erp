

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
ProductionConsumption = 5 // Üretim tüketimi (çıkış)
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

// ⚠️ ToUpper().Contains() Türkçe kültürde YANLIŞ! (2026-06-26 BUG)
// Sorun: "fsc'siz".ToUpper() → "FSC'SİZ" (i→İ; S I Z aranırken S İ Z bulunur = false)
// YANLIŞ:  if (fscType?.ToUpper().Contains("SIZ")) ...  // Türkçe'de i→İ
// DOĞRU:   if (fscType?.ToLower().Contains("siz")) ...  // Türkçe'de s→s, i→i, z→z (safe)
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

### ⚠️ Filtre/Arama — Zorunlu Tam Ekran Güncelleme Kuralı

Filtre/arama varsa **tüm** stat kartları, `<tfoot>` toplamları ve badge sayaçları da güncellenir. Server-side Razor toplamları DataTables ile uyumsuzdur. `draw.dt` recalc pattern'i: skill `fsc-erp-patterns` §1.

### ⚠️ Mesaj/Onay Kutusu Standardı

Native `confirm()`/`alert()` yasak — `appConfirm()` / `showToast()` / `appAlert()` kullan. Ayrıntı: skill `fsc-erp-patterns` §5.

### ⚠️ StockMovement Senkronizasyon Kuralı

FscSerial ağırlığı değişince ilgili StockMovement güncellenir (1 lot = 1 SM). Ayrıntı: skill `fsc-erp-patterns` §6.

### ⚠️ Decimal → JS/HTML Attribute Güvenliği

JS'e/`data-*`'a basılan her decimal **InvariantCulture** olmalı; `N0`/`N2` kullanma. Ayrıntı: skill `fsc-erp-patterns` §2.

### ⚠️ MCD (Multi-Choice Dropdown) Komponenti — Coklu Seçim Filtresi

Çoklu seçim filtresi için MCD komponenti (HTML + `window._mcdReady` guard'lı JS + `int[]` controller binding). Ayrıntı: skill `fsc-erp-patterns` §3.

### ⚠️ Shared Partial'lerde ViewData Aktarımı — `new ViewDataDictionary(ViewData)`

Shared partial'e ViewBag geçmez; `new ViewDataDictionary(ViewData)` ile aktar. Ayrıntı: skill `fsc-erp-patterns` §4.

### ⚠️ Varsayılan Filtre Deseni — "Tüm Kayıtları Göster" Toggle

Filtre seçilmediğinde varsayılan grup uygulanır, `?showAll=true` ile tam liste açılır. Ayrıntı: skill `fsc-erp-patterns` §7.

### ⚠️ StockMovement.ProductId Non-Nullable Kural

`StockMovement.ProductId` **asla NULL olmamalıdır** (her hareketi bir ürüne atanmalıdır):
```csharp
public int ProductId { get; set; }  // NOT NULL
public virtual Product Product { get; set; }

// Query'lerde `?. HasValue` diye bakmaya gerek yok; doğrudan ProductId kullan
query = query.Where(sm => sm.ProductId == productId);  // ✓
// query = query.Where(sm => sm.ProductId?.HasValue == true) YANLIŞ
```

**Neden:** StockMovement daima bir ürün ile ilişkili olmalıdır. Veri tabanında NOT NULL constraint sağlanmalıdır. Eski kayıtlarda NULL varsa migration ile backfill edilir.

### ⚠️ HTML Yorum Satırında Razor Direktifi Hata Vermesi

Razor tuzakları (HTML yorumunda `@`, `@foreach` ilk satırında `@{ }`): skill `fsc-erp-patterns` §9.

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

Üretimde bu klasör uygulama dizininin **dışındadır** — `FileStorage:Root` ile yönetilir
(kurulum `D:\FscErpData\uploads`'a yönlendirir), böylece sürüm güncellemesi belgeleri ezmez.

### ⚠️ "gitignored" ≠ "pakete girmez" — Dağıtım Sızıntısı Kuralı

`.gitignore` yalnız git'i bağlar. **`dotnet publish` onu hiç okumaz** ve `Microsoft.NET.Sdk.Web`
varsayılan olarak `wwwroot/**` altındaki HER ŞEYİ yayına koyar. Gitignored bir klasör repoda
görünmediği için gözden kaçar — ama diskte durur ve pakete girer.

2026-07-17'de `wwwroot/uploads/` altındaki 197 dosya (55 MB gerçek müşteri FSC arşivi:
müşteri listesi, tedarikçi listesi, sözleşmeler) kurulum EXE'sine girmişti. Aynı sınıf
hata `appsettings.json` ve `license.lic`'te de yaşandı.

**Kural:** müşteriye giden bir artefakt üretirken *içindekileri listele*, tahmin etme.
Hariç tutma `.csproj`'da yapılır:
```xml
<ItemGroup>
  <Content Remove="wwwroot\uploads\**" />
  <None Include="wwwroot\uploads\**" CopyToPublishDirectory="Never" />
</ItemGroup>
```
İkinci savunma hattı: `installer/build-installer.ps1` → `Test-NoSecretLeak`.

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

## ⚠️ Özellik Tamamlama Kontrol Listesi (Her Yeni Sayfa/Özellik İçin ZORUNLU)

Her yeni sayfa veya özellik tamamlandığında aşağıdaki adımlar **otomatik** yapılır — kullanıcı sormadan:

1. **Filtre/arama varsa:** filtreye tıkla/yaz → tüm stat kartları, tfoot toplamları, badge sayaçları güncellendi mi? (JS `draw.dt` pattern uygulandı mı?)
2. **Modal/form varsa:** kaydet → başarı mesajı, iptal → state sıfırlandı mı?
3. **Boş durum:** veri yokken sayfa kırılıyor mu?
4. **Build + webapp-testing ile tarayıcı doğrulaması** (ZORUNLU):
   - Kullanıcıdan `Ctrl+Shift+B` (build) basmasını iste; build hatası yoksa devam et
   - Playwright ile Golden Path + kenar durum testi çalıştır
   - Test PASS olmadan "tamamlandı" deme — FAIL/PASS raporunu kullanıcıya bildir
5. **KULLANIM_KILAVUZU.md güncelle** (bkz. "Kullanım Kılavuzu Güncelleme Kuralı" bölümü)
6. **tasks/lessons.md güncelle** — Bu özellikte karşılaşılan teknik tuzakları, DataTables davranışlarını, Razor kısıtlamalarını ekle

> **`webapp-testing` skill'i her özelliğin sonunda ZORUNLUDUR.** Sadece "kod doğru görünüyor" yetmez — çalıştırarak kanıtla.

## ⚠️ Çapraz-Ekip Etki Kuralı (Cross-Impact Cascade)

Bir alan değiştiğinde, **aşağıdaki ilgili alanlar otomatik kontrol edilmeli**; gerekirse güncellenmeli:

| Değişen Alan | Etkilenen Alanlar |
|---|---|
| FscSerial ağırlığı (InitialWeight / CurrentWeight) | StockMovement (PurchaseEntry SM'i güncelle), Stok kartları, RawMaterial sayfası |
| StockMovement yeni tip eklendi | StockController.Index/ExportStock filtreleri, Movements sayfası badge'leri, Net hesaplaması |
| Product / ExternalCode | Purchase filtresi (ExternalCode OR ProductCode), StockSummary |
| FscLot / DispatchNo | StockMovement.DocumentNo eşleşmesi |
| Filtre paneli değişti (yeni alan eklendi/kaldırıldı) | Tüm stat kartları, tfoot toplamları, draw.dt recalc — MUTLAKA güncelle |
| Yeni Controller / Action eklendi | _Layout.cshtml sidebar menü, KULLANIM_KILAVUZU.md |
| Entity eklendi/değişti | Migration, AppDbContext, ilgili servis, ilgili controller/view |
| Enum değeri eklendi (MovementType vb.) | Tüm switch/if blokları, badge renkleri, filtre dropdown'ları |

**Kural:** Bir dosyada değişiklik yaparken yukarıdaki tabloyu zihinsel olarak tara. Etkilenen alan varsa aynı PR'da güncelle veya kullanıcıyı uyar.

**Ekip yöneticisi (Ali) yükümlülüğü:** Uygulama ekibi bir alan teslim ettiğinde, etki tablosuna göre diğer ekip üyelerini (Ayşe/Ahmet/Nuri/Kadir) bilgilendir. "Bu stok hareketi tipi eklemesi RawMaterial sayfasını etkiliyor — Ayşe kontrol etsin" gibi açık bildirimler yapılmalıdır.

## Migration Komutları

```bash

# WebUI projesini startup, DataAccess projesini migration hedefi olarak kullan
cd FSCTakip.DataAccess
dotnet ef migrations add MigrationName --startup-project ../FSCTakip.WebUI
dotnet ef database update --startup-project ../FSCTakip.WebUI
```

## Kullanım Kılavuzu Güncelleme Kuralı

**Her yeni sayfa veya modül tamamlandığında** `docs/KULLANIM_KILAVUZU.md` güncellenir ve `FSCTakip.WebUI/Views/Guide/Index.cshtml` içindeki TOC + heading haritası senkronize edilir.

Bölüm şablonu, topbar ASCII diyagramı ve adım adım kontrol listesi: skill `fsc-erp-patterns` §13.

### Güncelleme Adımları (her modül bitiminde otomatik yap)

Kılavuz güncelleme adımları, topbar ASCII şablonu ve dosya yapısı: skill `fsc-erp-patterns` §13.

---

## Referans kalıplar → skill `fsc-erp-patterns`

Aşağıdaki kurallar talep üzerine yüklenen `fsc-erp-patterns` skill'inde:
EF Core navigation property (§11) · LINQ GroupBy key minimal (§12) · sticky kolon layout (§8) · JS event sıralaması `onclick`/`onmousedown` (§10) · kullanım kılavuzu güncelleme adımları (§13).

Razor/.cshtml, filtre paneli, tablo, modal, dropdown veya stok hareketi kodu yazarken bu skill'i yükle.
Aynı kalıplar Obsidian vault'ta da not olarak duruyor: `D:\ARD-Vault\30-Patterns\`.

## Önemli Notlar

- `AppDbContext.SaveChangesAsync()` içindeki string uppercase kodu tüm entity stringleri otomatik büyük harfe çevirir (tr-TR). Email alanlarında bu sorun çıkarır, o alanları SaveChanges içinde exclude etmek gerekir.
- `PaperWeight` entity'si `BaseEntity`'den türemiyor — manuel audit alanları var, dikkat et.
- `Machine` entity'si de `BaseEntity`'den türemiyor — ayrı audit alanları.
- FSC sertifikası takibi için `Supplier.FscExpiryDate` ve `Customer.FscExpiryDate` dashboard'da uyarı gösterilmeli.
- `ProductRecipe` çoka-çok ilişkisi `DeleteBehavior.Restrict` ile konfigüre edilmiş.
- **ProductionConsumption enum değeri** MovementType'a eklendi (5 = Üretim tüketimi). Tüm switch/if blokları, badge'ler, filtre dropdown'ları kontrol et.
- **Toplam Fiziksel Stok tutarlılığı** (2026-06-22): Purchase ve Stock/RawMaterial sayfalarında "Kalan (kg)" kartı tüm Ham+YM+BS serilerin CurrentWeight toplamını göstermeli. ViewBag'den gelen değer varsa kullan; yoksa modelden hesapla. Grup adları dinamik (HAMMADDE/YARI MAMUL/BURGU SAP uppercase match) — hardcoded GroupId'ler kullanma.
