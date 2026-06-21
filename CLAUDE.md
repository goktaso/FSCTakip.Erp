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

### ⚠️ Filtre/Arama — Zorunlu Tam Ekran Güncelleme Kuralı

**Her sayfada filtre veya arama kutusu varsa**, ekrandaki TÜM öğeler filtreye göre güncellenmelidir:
- Stat kartları (Toplam Lot, Toplam KG, vb.)
- Tablo footer toplamları (`<tfoot>`)
- Grup/ara toplamlar
- Badge sayaçları

**Uygulama pattern'i (DataTables kullanan sayfalar):**
```javascript
// 1. Her <tr>'ye data attribute ekle (InvariantCulture!)
<tr data-giris="@val.ToString(CultureInfo.InvariantCulture)"
    data-kalan="@val2.ToString(CultureInfo.InvariantCulture)">

// 2. Stat kart/tfoot elementlerine ID ekle
<div class="stat-value" id="cardGiris">...</div>
<td id="ftGiris">...</td>

// 3. draw.dt + input event'e bağla
function recalcCards() {
    var rows = table.querySelectorAll('tbody tr');
    var giris = 0;
    rows.forEach(tr => { if (tr.style.display !== 'none') giris += parseFloat(tr.dataset.giris) || 0; });
    document.getElementById('cardGiris').textContent = Math.round(giris).toLocaleString('tr-TR');
}
$(table).on('draw.dt', recalcCards);
searchInput.addEventListener('input', () => setTimeout(recalcCards, 60));
recalcCards(); // ilk yüklemede de çalıştır
```

**Sunucu tarafı Razor `<tfoot>` toplamları DataTables ile UYUMSUZ** — filtreden bağımsız tüm satırları toplar. Çözüm: `data-val` attribute + JS recalc (yukarıdaki pattern). Hiçbir zaman sadece server-side toplam bırakma.

### ⚠️ Mesaj/Onay Kutusu Standardı

Native `confirm()` / `alert()` KULLANMA. Bunun yerine `_Layout.cshtml`'de tanımlı ARD temalı sistemleri kullan:
- `await appConfirm('mesaj', { danger, title })` → Promise\<bool\>
- `showToast('mesaj', 'success|error|warning|info')` → sağ-alt toast
- `await appAlert('mesaj')` → OK-only uyarı

### ⚠️ StockMovement Senkronizasyon Kuralı

FscSerial ağırlığı değiştiğinde ilgili StockMovement da güncellenmelidir (1 lot = 1 SM kaydı):
```csharp
// SaveSerial() sonunda: lot toplamını hesapla, SM'yi bul ve güncelle
// Anahtar: sm.DocumentNo == lot.DispatchNo ?? lot.PartiNo
// Toplam KG: FscSerials.Sum(s => s.InitialWeight)
// Orijinal birim: FscSerials.Sum(s => s.OriginalQuantity ?? s.InitialWeight)
```
SM yoksa oluştur, varsa güncelle. Bakınız: `PurchaseController.SaveSerial()`.

### ⚠️ Decimal → JS/HTML Attribute Güvenliği

JS'e veya `data-*` attribute'üne basılan her decimal **InvariantCulture** (nokta) kullanmalı:
```cshtml
// DOĞRU
data-val="@item.Weight.ToString(System.Globalization.CultureInfo.InvariantCulture)"
var x = @item.Weight.ToString(System.Globalization.CultureInfo.InvariantCulture);

// YANLIŞ — JS SyntaxError'a yol açar (tr-TR virgüllü yazar)
data-val="@item.Weight"
var x = @item.Weight;

// YANLIŞ — JS argümanında virgül kaydırır
onclick="fn(@item.Weight.ToString("N2"))"  // N0/N2 binlik ayraç ekler!
```

### ⚠️ MCD (Multi-Choice Dropdown) Komponenti — Coklu Seçim Filtresi

Tekli dropdown yerine Excel Advanced Filter tarzı coklu seçim paneli:

**Yapı:**
```html
<div class="mcd" id="mcd-[pageId]" data-placeholder="— Seçiniz —">
    <button type="button" class="mcd-btn" onclick="mcdOpen('mcd-[pageId]')">
        <span class="mcd-lbl">[seçili sayısı]</span>
        <i class="fas fa-angle-down mcd-arrow"></i>
    </button>
    <div class="mcd-panel" id="mcd-[pageId]-panel" style="display:none;">
        <div class="mcd-search-row">
            <input type="text" class="mcd-search" placeholder="Kod/ad ara..." oninput="mcdSearch(this)">
        </div>
        <label class="mcd-row mcd-header">
            <input type="checkbox" class="mcd-all-cb" onchange="mcdToggleAll(this,'mcd-[pageId]')">
            <span>Tümünü Seç / Temizle</span>
        </label>
        <div class="mcd-items">
            @foreach (var item in items) {
                <label class="mcd-row">
                    <input type="checkbox" name="itemIds" value="@item.Id" class="mcd-cb" onchange="mcdUpdate('mcd-[pageId]')">
                    <span class="mcd-text">@item.Code</span>
                    <span class="mcd-sub">@item.Name</span>
                    <span class="mcd-ext" title="Dış kod">@item.ExternalCode</span>
                </label>
            }
        </div>
    </div>
</div>
```

**JavaScript (view'de tek seferde tanımlanır, `window._mcdReady` guard ile):**
```javascript
if (!window._mcdReady) {
    window._mcdReady = true;
    window.mcdOpen = function(id) {
        var panel = document.getElementById(id + '-panel');
        var isOpen = panel && panel.style.display !== 'none';
        document.querySelectorAll('.mcd-panel').forEach(p => p.style.display = 'none');
        if (!isOpen && panel) {
            panel.style.display = 'flex';
            mcdUpdate(id);
        }
    };
    window.mcdSearch = function(el) {
        var term = el.value.toLowerCase();
        el.closest('.mcd-panel').querySelectorAll('.mcd-row:not(.mcd-header)').forEach(row => {
            row.style.display = row.textContent.toLowerCase().includes(term) ? '' : 'none';
        });
    };
    window.mcdToggleAll = function(cb, id) {
        document.getElementById(id + '-panel').querySelectorAll('.mcd-row:not(.mcd-header)').forEach(row => {
            if (row.style.display !== 'none') {
                var c = row.querySelector('.mcd-cb');
                if (c) c.checked = cb.checked;
            }
        });
        mcdUpdate(id);
    };
    window.mcdUpdate = function(id) {
        var panel = document.getElementById(id + '-panel');
        var container = document.getElementById(id);
        if (!panel || !container) return;
        var count = panel.querySelectorAll('.mcd-cb:checked').length;
        var label = container.querySelector('.mcd-lbl');
        if (label) label.textContent = count === 0 ? (container.dataset.placeholder || '— Seçiniz —') : count + ' seçildi';
        var allCb = panel.querySelector('.mcd-all-cb');
        var visibleCbs = panel.querySelectorAll('.mcd-row:not(.mcd-header):not([style*="none"]) .mcd-cb').length;
        if (allCb && visibleCbs > 0) allCb.checked = (count >= visibleCbs);
    };
    document.addEventListener('mousedown', e => {
        if (!e.target.closest('.mcd')) {
            document.querySelectorAll('.mcd-panel').forEach(p => p.style.display = 'none');
        }
    });
}
```

**Controller parametresi:** `int? itemId` → `int[]? itemIds` → query `.Where(x => itemIds.Contains(x.ItemId))`

**ViewBag tutulması:** `ViewBag.ItemIds = itemIds ?? Array.Empty<int>();` view'de checkbox state'i için.

**Excel export:** Parametre URL'ye `itemIds=1&itemIds=2` şeklinde gönderilir (ASP.NET Core array binding).

**Uyarı:** Her view'de MCD kullanılırsa, `window._mcdReady` guard ile coklu tanımlama önlenmelidir.

### ⚠️ Varsayılan Filtre Deseni — "Tüm Kayıtları Göster" Toggle

Kullanıcı filtre seçmediğinde, sayfalar **sensible varsayılan** grup/kategori ile içerik sınırlamalı; ama "Tüm Kayıtları Göster" seçeneğiyle tam liste erişimi mümkün olmalı:

```csharp
// Controller
public async Task<IActionResult> Index(
    int[]? supplierIds, int[]? fscTypeIds,
    string? stockCode, string? stockName,
    int[]? productIds, bool showAll = false)
{
    // Kullanıcı herhangi bir filtre uyguladı mı?
    bool hasUserFilter = showAll
        || (supplierIds?.Length > 0)
        || (fscTypeIds?.Length > 0)
        || !string.IsNullOrWhiteSpace(stockCode)
        || !string.IsNullOrWhiteSpace(stockName)
        || (productIds?.Length > 0);

    if (!hasUserFilter)
    {
        // Varsayılan: Hammadde + Yarı Mamül + Burgu Sap (GroupIds: 1, 3, 4)
        var defaultGroupIds = new[] { 1, 3, 4 };
        query = query.Where(l => l.Product != null
            && l.Product.ProductGroupId.HasValue
            && defaultGroupIds.Contains(l.Product.ProductGroupId.Value));
    }
    
    ViewBag.IsDefaultFilter = !hasUserFilter;
    ViewBag.ShowAll         = showAll;
    // ... rest of filters
}
```

**View HTML:**
```html
<!-- Varsayılan filtre aktif →  bilgi bandı + toggle -->
@if (isDefaultFilter)
{
    <div class="d-flex align-items-center gap-2 mb-2 px-1 py-2 rounded"
         style="background:rgba(25,118,210,.08);border:1px solid rgba(25,118,210,.2);">
        <i class="fas fa-filter text-primary"></i>
        <span>Varsayılan görünüm: <strong>Hammadde · Yarı Mamül · Burgu Sap</strong></span>
        <a href="/Purchase/Index?showAll=true" class="ms-auto btn btn-sm">
            <i class="fas fa-eye me-1"></i>Tüm Kayıtları Göster
        </a>
    </div>
}
else if ((bool)(ViewBag.ShowAll ?? false))
{
    <div class="d-flex align-items-center gap-2 mb-2 ...">
        <i class="fas fa-list text-muted"></i>
        <span>Tüm kayıtlar gösteriliyor.</span>
        <a href="/Purchase/Index" class="ms-auto btn btn-sm">
            <i class="fas fa-undo me-1"></i>Varsayılana Dön
        </a>
    </div>
}
```

**URL deseni:** `?` (varsayılan) vs `?showAll=true` (tümü) vs `?productIds=5&productIds=6` (seçili filtre).

**Uygulanan yerler:** Purchase/Index (varsayılan: Hammadde+YM+BS).

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

**Sorun:** HTML yorum satırında `@section` yazması Razor compile hatasına yol açar ("Unexpected character in tag helper").

**Kök neden:** Razor parser HTML yorum `<!-- ... -->` bloğunun içini parse eder; `@` karakterleri directive olarak yorumlanır.

**Çözüm:** Açıklama satırında `@` karakteri bulunacaksa Razor yorum `@* ... *@` kullan veya `@` kaldır:
```razor
// YANLIŞ
<!-- @section Scripts bloğu tanımlanır -->

// DOĞRU
<!-- Section Scripts bolumu tanimlanir -->
// veya
@* @section Scripts bloğu açıklaması *@
```

### ⚠️ Razor @foreach Bloğunda İlk Satırda @{ } Kullanilamaz

**Sorun:** `@foreach (var item in list) { @{ var x = ...; } ... }` → Razor compile hatası ("Unexpected end of file" vb.).

**Kök neden:** `@foreach` blok içinin ilk statement'i `@{ }` (code block) olamaz. Razor parser blok başında HTML veya doğrudan C# statement beklediğinde, `@{ }` sözdizimi geçersizdir.

**Çözüm:** Foreach içinde değişken bildirimi yapılacaksa `@{ }` kaldır, doğrudan statement yazın:
```razor
// YANLIŞ
@foreach (var s in suppliers) {
    @{ bool sel = condition; }
    <input checked="@(sel ? "checked" : null)">
}

// DOĞRU
@foreach (var s in suppliers) {
    bool sel = condition;  // @{ } kaldırıldı
    <input checked="@(sel ? "checked" : null)">
}
```

### ⚠️ MCD Filtresi — CSS display:block + flex-column Wrapper Layout Kuralı

Tekli dropdown yerine multi-select MCD komponenti içeren sayfalarda filtre paneli layout'unda iki düzeltme gerekli:

**1. Global CSS** (_Layout.cshtml / mcd stil bloğu):
```css
.mcd {
    position: relative;
    display: block;  /* ← inline-block değil; flex container'da dikey hizalama sağlasın */
}
```

**2. View'de MCD gruplarının wrapper'ı** (her MCD etiketi + MCD konteynerini kapsayan div):
```html
<div style="display:flex; flex-direction:column; min-width:160px;">
    <label class="form-label small fw-semibold mb-1">Tedarikçi</label>
    <div class="mcd" id="mcd-id" data-placeholder="...">
        <button type="button" class="mcd-btn" onclick="mcdOpen('mcd-id')">
            ...
        </button>
        <div class="mcd-panel" id="mcd-id-panel">
            ...
        </div>
    </div>
</div>
```

**Filtre paneli hizalama:** Birden çok MCD grubu yan yana basıldığında `row g-2 align-items-end` yerine:
```html
<div class="d-flex align-items-end gap-2 flex-wrap">
    <!-- her grup kendi min-width taşır -->
    <div style="min-width:130px;">...</div>
    <div style="display:flex;flex-direction:column;min-width:160px;">
        <label>...</label>
        <div class="mcd">...</div>
    </div>
</div>
```

**Neden:** `display:inline-block` flex container'da hizalama kontrol edilemiyor; dikey etiket-input çiftinde input alanı etiketin yanına gelir (aynı satır). `display:block` ile wrapper flex-direction:column ayarıyla etiket tamamen üstte, input tamamen alta iner.

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
