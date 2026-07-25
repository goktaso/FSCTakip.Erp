---
name: fsc-erp-patterns
description: FSCTakip.Erp kod kalıpları kataloğu — MCD çoklu seçim filtresi, filtre/stat kartı senkronizasyonu, shared partial ViewData aktarımı, decimal kültür güvenliği, sticky kolon, Razor tuzakları, varsayılan filtre toggle'ı, mesaj kutusu standardı, StockMovement senkronizasyonu, EF navigation property ve LINQ GroupBy kuralları. Bu projede Razor/.cshtml, filtre paneli, tablo, modal, dropdown veya stok hareketi kodu yazarken ya da gözden geçirirken kullan.
---

# FSCTakip.Erp — Kod Kalıpları Kataloğu

Bu katalog `CLAUDE.md`'den ayrıldı: içeriğin çoğu 10 turda 1 gerekiyor ama her turda bağlama giriyordu. Tüketimin %97'si cache read olduğu için sabit ön ekte duran her KB, her turda yeniden faturalanır.

`CLAUDE.md` yalnız **her turda gereken** kuralları tutar (mimari, entity haritası, çapraz-etki tablosu, tamamlama kontrol listesi). Ayrıntı buradadır.

Aynı kalıplar Obsidian vault'ta da not olarak duruyor: `D:\ARD-Vault\30-Patterns\`.

---

## 1. Filtre/Arama — Zorunlu Tam Ekran Güncelleme

**Kural:** Sayfada filtre veya arama varsa ekrandaki TÜM öğeler filtreye göre güncellenir — stat kartları, `<tfoot>` toplamları, grup/ara toplamlar, badge sayaçları.

**Sunucu tarafı Razor `<tfoot>` toplamları DataTables ile UYUMSUZ** — filtreden bağımsız tüm satırları toplar. Hiçbir zaman sadece server-side toplam bırakma.

```javascript
// 1. Her <tr>'ye data attribute (InvariantCulture!)
<tr data-giris="@val.ToString(CultureInfo.InvariantCulture)"
    data-kalan="@val2.ToString(CultureInfo.InvariantCulture)">

// 2. Stat kart / tfoot elementlerine ID
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

---

## 2. Decimal → JS/HTML Attribute Güvenliği

JS'e veya `data-*` attribute'üne basılan her decimal **InvariantCulture** (nokta) kullanmalı.

```cshtml
@* DOĞRU *@
data-val="@item.Weight.ToString(System.Globalization.CultureInfo.InvariantCulture)"
var x = @item.Weight.ToString(System.Globalization.CultureInfo.InvariantCulture);

@* YANLIŞ — JS SyntaxError (tr-TR virgüllü yazar) *@
data-val="@item.Weight"
var x = @item.Weight;

@* YANLIŞ — N0/N2 binlik ayraç ekler, JS argümanında virgül kaydırır *@
onclick="fn(@item.Weight.ToString("N2"))"
```

---

## 3. MCD (Multi-Choice Dropdown) — Çoklu Seçim Filtresi

Tekli dropdown yerine Excel Advanced Filter tarzı çoklu seçim paneli.

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

JavaScript view'de **tek seferde** tanımlanır, `window._mcdReady` guard ile:

```javascript
if (!window._mcdReady) {
    window._mcdReady = true;
    window.mcdOpen = function(id) {
        var panel = document.getElementById(id + '-panel');
        var isOpen = panel && panel.style.display !== 'none';
        document.querySelectorAll('.mcd-panel').forEach(p => p.style.display = 'none');
        if (!isOpen && panel) { panel.style.display = 'flex'; mcdUpdate(id); }
    };
    window.mcdSearch = function(el) {
        var term = el.value.toLowerCase();
        el.closest('.mcd-panel').querySelectorAll('.mcd-row:not(.mcd-header)').forEach(row => {
            row.style.display = row.textContent.toLowerCase().includes(term) ? '' : 'none';
        });
    };
    window.mcdToggleAll = function(cb, id) {
        document.getElementById(id + '-panel').querySelectorAll('.mcd-row:not(.mcd-header)').forEach(row => {
            if (row.style.display !== 'none') { var c = row.querySelector('.mcd-cb'); if (c) c.checked = cb.checked; }
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
        if (!e.target.closest('.mcd')) document.querySelectorAll('.mcd-panel').forEach(p => p.style.display = 'none');
    });
}
```

- **Controller parametresi:** `int? itemId` → `int[]? itemIds` → `.Where(x => itemIds.Contains(x.ItemId))`
- **ViewBag:** `ViewBag.ItemIds = itemIds ?? Array.Empty<int>();` (checkbox state için)
- **Excel export:** URL'ye `itemIds=1&itemIds=2` (ASP.NET Core array binding)

### MCD layout kuralı — `display:block` + flex-column wrapper

Global CSS'te `.mcd { position: relative; display: block; }` — `inline-block` DEĞİL. Flex container'da `inline-block` hizalama kontrol edilemiyor; etiket ile input aynı satıra düşüyor.

```html
<div class="d-flex align-items-end gap-2 flex-wrap">
    <div style="display:flex; flex-direction:column; min-width:160px;">
        <label class="form-label small fw-semibold mb-1">Tedarikçi</label>
        <div class="mcd" id="mcd-id" data-placeholder="...">...</div>
    </div>
</div>
```

---

## 4. Shared Partial'lerde ViewData Aktarımı

Shared partial'lere (`Views/Shared/_X.cshtml`) ViewBag geçmek **işe yaramaz** — partial farklı `ViewContext`'te render edilir.

```razor
@await Html.PartialAsync("_PartialName", null,
    new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(ViewData))
```

- **Shared partial'ler:** ViewData kullan (`ViewData["FscliGiris"] = ...`)
- **Page partial'ler** (aynı klasörde): ViewBag veya Model
- **Reusable component'ler:** ViewData daha güvenli (context-independent)

Uygulandığı: `_FscStokOzeti.cshtml` (Purchase, Stock/Summary, Stock/RawMaterial, Stock/AdminStock).

---

## 5. Mesaj/Onay Kutusu Standardı

Native `confirm()` / `alert()` KULLANMA. `_Layout.cshtml`'deki ARD temalı sistemler:

- `await appConfirm('mesaj', { danger, title })` → `Promise<bool>`
- `showToast('mesaj', 'success|error|warning|info')` → sağ-alt toast
- `await appAlert('mesaj')` → OK-only uyarı

---

## 6. StockMovement Senkronizasyonu

FscSerial ağırlığı değiştiğinde ilgili StockMovement da güncellenir (1 lot = 1 SM kaydı):

```csharp
// SaveSerial() sonunda: lot toplamını hesapla, SM'yi bul ve güncelle
// Anahtar: sm.DocumentNo == lot.DispatchNo ?? lot.PartiNo
// Toplam KG: FscSerials.Sum(s => s.InitialWeight)
// Orijinal birim: FscSerials.Sum(s => s.OriginalQuantity ?? s.InitialWeight)
```

SM yoksa oluştur, varsa güncelle. Referans: `PurchaseController.SaveSerial()`.

**`StockMovement.ProductId` asla NULL olmaz:**

```csharp
public int ProductId { get; set; }          // NOT NULL
public virtual Product Product { get; set; }

query = query.Where(sm => sm.ProductId == productId);   // ✓
// query.Where(sm => sm.ProductId?.HasValue == true)    // YANLIŞ
```

---

## 7. Varsayılan Filtre + "Tüm Kayıtları Göster" Toggle

Kullanıcı filtre seçmediğinde sayfa **sensible varsayılan** grupla sınırlanır, ama tam listeye erişim mümkün kalır.

```csharp
public async Task<IActionResult> Index(
    int[]? supplierIds, int[]? fscTypeIds,
    string? stockCode, string? stockName,
    int[]? productIds, bool showAll = false)
{
    bool hasUserFilter = showAll
        || (supplierIds?.Length > 0) || (fscTypeIds?.Length > 0)
        || !string.IsNullOrWhiteSpace(stockCode)
        || !string.IsNullOrWhiteSpace(stockName)
        || (productIds?.Length > 0);

    if (!hasUserFilter)
    {
        // Varsayılan: Hammadde + Yarı Mamül + Burgu Sap
        var defaultGroupIds = new[] { 1, 3, 4 };
        query = query.Where(l => l.Product != null
            && l.Product.ProductGroupId.HasValue
            && defaultGroupIds.Contains(l.Product.ProductGroupId.Value));
    }

    ViewBag.IsDefaultFilter = !hasUserFilter;
    ViewBag.ShowAll         = showAll;
}
```

View'de bilgi bandı + toggle: varsayılandayken `?showAll=true` linki, `showAll` aktifken "Varsayılana Dön" linki.

**URL deseni:** `?` (varsayılan) · `?showAll=true` (tümü) · `?productIds=5&productIds=6` (seçili).

**⚠️ Grup adları dinamik:** yeni kodlarda hardcoded GroupId yerine `ProductGroup.GroupName.ToUpper()` ile lookup yap.

Uygulandığı: `Purchase/Index`, `Stock/RawMaterial`.

---

## 8. Sticky Kolon — wrapper `position:relative`

```html
<div class="table-wrapper" style="position: relative; overflow-x: auto;">
    <table>
        <td class="sticky-col"
            style="position: sticky; right: 0; z-index: 10; background: white; box-shadow: -2px 0 4px rgba(0,0,0,.1);">
            <button>Düzenle</button><button>Sil</button>
        </td>
    </table>
</div>
```

Wrapper `position:relative` olmadan sticky konum bağlamını kaybeder. Arka plan rengi koyu olmalı ki scroll'da arkasındaki metin gizlensin.

---

## 9. Razor Tuzakları

**HTML yorumunda `@` direktifi:** Razor parser `<!-- -->` içini de parse eder.

```razor
@* YANLIŞ *@   <!-- @section Scripts bloğu tanımlanır -->
@* DOĞRU  *@   <!-- Section Scripts bolumu tanimlanir -->
@* veya   *@   @* @section Scripts açıklaması *@
```

**`@foreach` bloğunun ilk satırında `@{ }` kullanılamaz:**

```razor
@* YANLIŞ *@
@foreach (var s in suppliers) {
    @{ bool sel = condition; }
    <input checked="@(sel ? "checked" : null)">
}

@* DOĞRU *@
@foreach (var s in suppliers) {
    bool sel = condition;
    <input checked="@(sel ? "checked" : null)">
}
```

---

## 10. JavaScript Event Sıralaması — `onclick` vs `onmousedown`

Dropdown açma/kapama ile seçim çakışıyorsa: grup başlığına `onmousedown="event.preventDefault()"`, seçim işlemi `onclick`'e.

```razor
<div onmousedown="event.preventDefault()" onclick="toggleProdGroup('@id')">Grup Başlığı</div>
<tr onclick="selectSerial(...); hideSerialDropdown();">Seri Satırı</tr>
```

`preventDefault()` mousedown'ın panel toggle'ını engeller; click sırasında seçim tamamlanır, sonra panel kapanır. Panel kapanma gecikmesi `setTimeout` ile (400 ms).

---

## 11. EF Core Navigation Property

FK alanı tanımlandığında navigation property de eklenir ve Include zinciriyle eager-load edilir.

```csharp
public int? SourceSerialId { get; set; }
public virtual FscSerial? SourceSerial { get; set; }   // ← zorunlu

modelBuilder.Entity<FscLot>()
    .HasOne(l => l.SourceSerial).WithMany()
    .HasForeignKey(l => l.SourceSerialId);

var lots = _context.FscLots
    .Include(l => l.SourceSerial).ThenInclude(s => s!.Lot)
    .Include(l => l.Product)
    .ToListAsync();

var kaynak = lot.SourceSerial;   // Find() ile anonim FK okuma yerine
```

Property olmadan `Include` çalışmaz — soft-error (null reference) riski.

---

## 12. LINQ GroupBy Key'ini Minimal Tut

Key 10+ alanla tanımlanırsa sorgu şişer, bellek tüketimi artar. Ek veriler `lg.First()` ile alınır.

```csharp
// YANLIŞ — key çok alan
var byLot = serials.GroupBy(s => new {
    PartiNo = s.Lot?.PartiNo, Supplier = s.Lot?.Supplier?.Name,
    FscType = s.Lot?.FscType.Name, SourceInfo = s.Lot?.SourceSerial?.Lot?.PartiNo,
}).ToList();

// DOĞRU — key minimal
var byLot = serials
    .Include(s => s.Lot!.SourceSerial!.Lot)
    .GroupBy(s => new { s.Lot!.PartiNo, Supplier = s.Lot.Supplier!.Name })
    .Select(lg => new {
        FirstLot     = lg.First().Lot,
        SourceSerial = lg.First().Lot?.SourceSerial,
        Items        = lg.ToList()
    }).ToList();
```

---

## 13. Kullanım Kılavuzu Güncelleme

Her yeni sayfa/modül bitiminde iki dosya senkronize edilir:

**`docs/KULLANIM_KILAVUZU.md`** — yeni bölüm: `## N. Sayfa Adı {#anchor-id}`, sayfa URL'i + menü yolu, ASCII topbar diyagramı, özet kartlar, işlem adımları (yeni/düzenle/sil/filtre/Excel), uyarı kutuları (`> **⚠️**` / `> **ℹ️**`), alan tabloları. İçindekiler + Modül Durumu tablosu + versiyon numarası güncellenir.

**`FSCTakip.WebUI/Views/Guide/Index.cshtml`** — TOC'a yeni `<a class="toc-item">` (`toc-badge done`), `map` nesnesine heading → anchor eşleşmesi, status bar sayısı.

Standart topbar diyagramı:
```
[≡] [+ Yeni Kayıt Ekle]   Sayfa Başlığı   [Filtrele] [Excel] [👤]
```
Birincil aksiyon (mavi gradient) sol üstte hamburgerin yanında; Filtrele + Excel sağda.
