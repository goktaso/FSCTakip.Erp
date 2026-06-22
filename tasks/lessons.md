# Alınan Dersler — FSC Takip ERP

## Razor → JavaScript'e decimal basarken kültür tuzağı (2026-06-14)

**Belirti:** Bir sayfadaki TÜM butonlar çalışmıyor (JS fonksiyonları tanımsız).

**Kök neden:** `.cshtml` script bloğunda `var x = @Model.DecimalAlan;` yazıldığında, decimal tr-TR kültürüyle **virgüllü** basılır (`500000,00`). Tarayıcıda `var x = 500000,00;` → **SyntaxError** → `<script>` bloğunun tamamı çöker → o bloktaki hiçbir fonksiyon tanımlanmaz → tüm `onclick` butonları ölür.

**Neden geç fark edildi:** İlgili veri (WorkOrder) silinmişti; sayfa gerçek decimal değerle ilk kez render edilince patladı.

**Çözüm:** JS'e veya HTML `data-*` attribute'üne basılan her decimal'i kültürden bağımsız (nokta) yaz:
```cshtml
var x = @Model.PlannedQuantity.ToString(System.Globalization.CultureInfo.InvariantCulture);
data-plan="@wr.PlannedQuantity.ToString(System.Globalization.CultureInfo.InvariantCulture)"
```
`@Model.Id` / int alanlar güvenli (ondalık yok). `parseFloat("6000,00")` virgülde durup `6000` döndürür — çökmez ama sessizce ondalığı kaybeder, yine de invariant bas.

**Kontrol kalıbı:** Yeni bir view'da JS'e sayı basıyorsan `grep "= @Model\.\w+;"` ve `data-...="@...Quantity"` ara; decimal ise InvariantCulture uygula.

**Ek nüans — `ToString("N2")` JS'i bozar:** `"N2"`/`"N0"` formatı InvariantCulture'da bile **binlik ayraç** ekler (1450 → `"1,450.00"`). Bu bir JS fonksiyon çağrısına argüman olarak basılırsa (`onclick="selectSerial(id,'x',@v.ToString("N2",inv),...)"`) virgül argümanları kaydırır (kalan=1, sonraki=450). JS'e sayı basarken **gruplama YOK** format kullan: `ToString("0.####", InvariantCulture)`. (Görünür metinde "N2" sorun değil; sadece JS/attribute bağlamında.)

**Düzeltilen dosya:** `Views/Production/Detail.cshtml` (satır 947 + BOM `<option>` data-* attribute'leri).

## Belge depolama: yapılandırılabilir kök + ayrı klasör + satış yükleme (2026-06-14)

**Amaç:** Müşteri kurulumunda fatura/irsaliye konumu esnek olsun, klasörler ayrı olsun, satışta da belge yüklenebilsin.

**Yapı:**
- `appsettings.json → FileStorage:{ Root, Folders:{Invoice,Dispatch,Other} }`. `Root` mutlak yol (örn. `D:\ErpBelgeler`) veya wwwroot'a göre göreli (varsayılan `uploads`) olabilir.
- `Services/FileStorageService.cs` (`IFileStorageService`) — validasyon (mime+magic bytes+20MB) + GUID adlandırma + `SaveAsync(file, "Invoice"|"Dispatch")` → DB'ye **köke göreli anahtar** döner (`invoices/2026/{guid}.pdf`). `TryResolve` legacy `/uploads/...` ve mutlak kökü çözer + path-traversal guard.
- `Controllers/DocumentController.cs` `[AllowAnonymous]` → `/Document/Serve?key=...` ile servis (iframe'de auth yönlendirmesi olmasın; dosyalar GUID adlı). `SessionAuthFilter` artık `[AllowAnonymous]` endpoint'leri muaf tutuyor (`GetEndpoint().Metadata.GetMetadata<IAllowAnonymous>()`).
- Purchase + Sales: `_storage.SaveAsync` ile fatura→`invoices/`, irsaliye→`dispatches/`. View linkleri `showPdf('/Document/Serve?key=@Model.XxxPdfPath', ...)`.
- Sales: `SalesOrder` zaten Invoice/DispatchPdfPath alanlarına sahipti (migration GEREKMEDİ) — `SalesController.UploadDocument` + `Sales/Detail` yükleme UI eklendi.

**Tuzak:** Form'dan `key` query'sine path geçerken slash'lar sorun değil; ama mutlak kök kullanılırsa eski `/uploads/...` kayıtları wwwroot guard'ı sayesinde hâlâ çözülür (iki kök birden allow-list'te).

**Temizlik notu:** `PurchaseController.SaveFile` ve `ViewDocument` artık ölü kod (Document/Serve'e geçildi) — sonra kaldırılabilir.

## Tüketim stok hareketi (çıkış) eksikti — Stok Durumu/Hareketleri'nde çıkış 0 (2026-06-14)

**Belirti:** Üretim/dönüşüm yapılmasına rağmen Stok Durumu'nda çıkış 0, Stok Hareketleri'nde sadece girişler.

**Kök neden:** Tüketim `FscSerial.CurrentWeight`'ten düşülüyordu ama **hiç `StockMovement` (çıkış) yazılmıyordu**. Stok ekranları StockMovement tabanlı → çıkış görünmüyordu.

**Çözüm:**
- `MovementType.ProductionConsumption = 5` (çıkış) eklendi.
- `ConversionController.Convert` → kaynak ham/YM tüketimi için çıkış hareketi.
- `ProductionController.SaveDetail` → tüketim için çıkış hareketi (detay ile **ErpReferenceId** üzerinden eşlenir; edit'te güncellenir). `DeleteDetail` → ilgili hareketi siler.
- `StockController.Index/ExportStock` → CikisAdet'e ProductionConsumption dahil (Net = Giriş − Çıkış → bobin kalanıyla tutar).
- `Stock/Movements` görünümü → tip 5 rozet/etiket/işaret + filtre + özet. `Stock/RawMaterial` → Dış Kod + Tüketim(kg) + Fire(kg) kolonları (Fire = ProductionDetail.WasteWeight/bobin; dönüşüm firesi Fire Raporu'nda).
- Geçmiş veri SQL ile backfill (dönüşüm: HAM-DEMO Initial−Current; üretim: ProductionDetails).

**Not:** Mamul (3xxxx) üretimi hâlâ stok hareketi oluşturmuyor (sadece WorkOrder.ActualQuantity); mamul stoğa satış sevkiyatında çıkış olarak girer — tasarım böyle.

## Tek tip mesaj kutusu standardı (2026-06-14) — §11'e eklenecek

Tüm mesaj/onay kutuları `_Layout.cshtml`'de tanımlı tek bir ARD temalı sistemden çıkar (beyaz kart, ikon halkası, marka mavisi `#1976d2` / tehlike kırmızısı `#dc2626`, 16px köşe, yumuşak animasyon, Enter=onay/Esc=iptal):

- **`appConfirm(message, opts)`** → `Promise<bool>`. Native `confirm()` yerine. `opts: { danger, title, icon, confirmText, cancelText }`. Mesajda "sil/kaldır/geri alınamaz/dönüştür" geçerse otomatik kırmızı.
  Kullanım: `if (!await appConfirm('...')) return;` (kapsayan fonksiyon `async` olmalı).
- **`showToast(msg, type)`** → sağ-alt toast (`success|error|warning|info`).
- **`window.alert`** override → otomatik `showToast(msg,'error')`. Çağrı yeri değişmez.
- **`Swal.fire`** override (shim) → SweetAlert çağrılarını aynı temaya taşır; konumsal + `{showCancelButton, timer, showConfirmButton, html, icon}` + `.then(r=>r.isConfirmed)` desteklenir. CDN (`sweetalert2@11`) hâlâ yüklü ama `fire` override'lı.
- **`appAlert(message, opts)`** → OK-only temalı uyarı kısayolu.

**Kural:** Yeni kodda native `confirm/alert` veya ham SweetAlert kullanma; bu API'leri kullan. `_Layout` script bloğu SweetAlert CDN'inden (satır ~1743) **sonra** gelmeli ki `Swal.fire` override'ı geçerli olsun.

**BEKLEYEN:** Bu standardı `~/.claude/CLAUDE.md` §11'e ekleyip `goktaso/claude-config` repoya push et (kullanıcı onayıyla).

## Decimal form binding tr-TR'de bozuluyor — "6000.00" → 60000000 (2026-06-14)

**Belirti:** Forma gönderilen ondalık değer sunucuda ~10000× büyüyor (BOM planı 6.000 yerine 60.000.000 kg).

**Kök neden:** `Program.cs`'te hiç kültür ayarı yok → ASP.NET Core form decimal binding'i **OS kültürünü (tr-TR)** kullanıyor. tr-TR'de `.` binlik ayracı olduğu için JS'in gönderdiği nokta-ondalık `6000.0000` → `60000000` olarak parse ediliyor. JS `toFixed()`/`Number.toString()` hep nokta üretir → her decimal form alanı risk altında (tüketim kg, planlanan miktar, fiyat...).

**Çözüm (kök, global):** `Binders/InvariantDecimalModelBinder.cs` — decimal/decimal? için custom binder; önce InvariantCulture (nokta), olmazsa tr-TR (virgül) dener. `Program.cs` → `options.ModelBinderProviders.Insert(0, new InvariantDecimalModelBinderProvider())`. Ekran gösterimi (ToString N0/N2) tr-TR kalır, sadece **parse** invariant-öncelikli olur. Hem JS nokta hem kullanıcı virgül girişini tolere eder.

**Not:** Bu repo'da `AddRazorRuntimeCompilation` AKTİF (Program.cs) → `.cshtml` değişiklikleri rebuild istemez, sadece F5. Ama `.cs` (controller/binder/Program) değişiklikleri rebuild + restart ister.

## ViewBag'e anonim tip geçince RuntimeBinderException (2026-06-14)

**Belirti:** `'object' does not contain a definition for 'Id'` — view render 500.

**Kök neden:** Controller'da `ViewBag.X = ....Select(s => new { s.Id, ... })` (anonim tip) → view runtime-compiled ayrı assembly'de derlendiği için anonim tipin `internal` üyelerine `dynamic` ile erişemiyor.

**Çözüm:** ViewBag/dynamic'e geçen projeksiyonlarda **public view-model sınıfı** kullan (`public class ConvSourceVM {...}`), `@using`'le tipe cast et (`ViewBag.X as List<ConvSourceVM>`). EF `.Select(s => new ConvSourceVM {...})` ile projeksiyon yapılabilir.

## DataTables v2 arama kutusu ID değişti (2026-06-20)

**Belirti:** `.dataTables_filter input` selector ile topbar arama kutusu bulunamıyor.

**Kök neden:** DataTables v2'de arama input ID'si `#dt-search-0` — eski v1 class selector `.dataTables_filter input` artık çalışmıyor.

**Çözüm:** Topbar global arama kutusunu DataTables API üzerinden bağla:
```javascript
$(document).on('init.dt', 'table.data-table', function () {
    var dt = $(this).DataTable();
    document.getElementById('globalSearch').addEventListener('input', function() {
        dt.search(this.value).draw();
    }, true);
});
```
`rows({ search: 'applied' })` tüm sayfalar dahil filtrelenmiş satırları verir.

## draw.dt + stat kartı senkronizasyon pattern'i (2026-06-20)

Filtre/arama sonrası stat kartlarının güncellenmesi için:
1. Her `<tr>`'ye `data-giris`, `data-kalan` vb. InvariantCulture decimal attribute ekle
2. `$(document).on('draw.dt', ...)` ile `dt.rows({ search: 'applied' }).nodes()` topla
3. Server-side Razor `<tfoot>` toplamları DataTables filtresiyle UYUMSUZ — asla sadece server-side bırakma

## Razor option tag helper — inline ternary yasak (2026-06-20)

**Hata:** `The tag helper 'option' must not have C# in the element's attribute declaration area`
```razor
// YANLIŞ
<option value="@s.Id" @(cond ? "selected" : "")>

// DOĞRU
@{ var isSel = condition; }
<option value="@s.Id" selected="@(isSel ? "selected" : null)">
```

## Grup subtotal duplikasyon — tek mekanizma kuralı (2026-06-20)

Razor'da grup satırı ve ara toplam ikisi için de aynı anda `newGroup` bayrağı kullanılırsa duplikat oluşabilir. Çözüm: grup-header için `newGroup`, subtotal için `lastInGrp` — iki ayrı bayrak, hiç çakışmasın.

## ExternalCode OR filtresi (2026-06-20)

Purchase ve stok filtrelerinde kullanıcı hem iç stok kodu (ProductCode) hem dış kod (ExternalCode) ile arama yapabilmeli:
```csharp
query = query.Where(l => l.Product != null &&
    (l.Product.ProductCode.Contains(sc) ||
     (l.Product.ExternalCode != null && l.Product.ExternalCode.Contains(sc))));
```
**Uygulandığı yerler:** `PurchaseController.Index` (Stok Kodu filtresi), `StockController.Summary` (arama), `StockController.RawMaterial` (arama).
**Neden:** Bazı ürünlerin dış kodu tercih edilir (örn. tedarikçi kodları); her iki alan da taranmalı.

## Proaktif test disiplini — build sonrası Playwright zorunlu (2026-06-20)

**Sorun:** Kod değişikliği yapıldı ama IIS Express yeni build'i yüklemedi; Playwright eski davranışı test etti → "çalışıyor" yanılsaması.

**Kural:** Her değişiklikten sonra:
1. Kullanıcıdan `Ctrl+Shift+B` build istenecek (`.cs` değiştiyse IIS Express restart da)
2. Playwright testi çalıştırılacak
3. PASS olmadan "tamamlandı" denmeyecek

**Not:** DataTables pagination varsa Playwright testi "Tümü" ile çalıştır; subtotal/istatistik 2. sayfada olabilir.

## Ürün birimi KG/MT/ADET ile otomatik dönüşüm (2026-06-20)

**Amaç:** Bazı ürünler MT (metrik ton), ADET, vb. ile giriliyor; stok takibi için KG'ye çevrilmesi gerekiyor.

**Yapı:**
- `FscSerial`: `OriginalQuantity` (orijinal birim cinsinden), `OriginalUnit` (MT/ADET) eklendi. `InitialWeight` hep KG.
- `StockMovement`: `Quantity` (orijinal birim), `Unit` (MT/ADET), `QuantityKg` (KG karşılığı) — birim KG ise `QuantityKg` NULL olabilir.
- `UnitConversions` tablosu: `(SourceUnit, ProductId, ProductGroupId, ConversionFactor)` — örn. (MT, null, 1, 1000) = 1 MT = 1000 KG.
- `UnitConversionController.FindFactor()`: Ürün bazlı factor aranır; yoksa grup bazlı; yoksa default. MT→KG = 1000, ADET için yapılandırılan factor.
- **Purchase modal:** Ürün seçilince birim badge gösterilir. Kullanıcı MT giri,
ş yaparsa `500 MT → 500.000 KG` otomatik dönüştürülür (server tarafında).
- **Stok Durumu:** `StockMovement.QuantityKg` varsa onu, yoksa `Quantity` kullan (legacy uyumluluğu). Net = `(giriş − çıkış) KG`.
- **İlgili alanlar:** Purchase.Index filtresi (ProductCode + ExternalCode), Stock.Index (input birim gösterimi), RawMaterial (giriş/çıkış vb. hep KG gösterilir).

**Not:** Eski kayıtlarda `StockMovement.Quantity` MT olabilir ama `Unit="KG"` yazılı (bug). Migration ile `QuantityKg` backfill'i yapıldı. Yeni kod hep `QuantityKg`'ye yazıyor.

## Yarı Mamül Dönüşüm özelliği eklendi (2026-06-14)

3 katmanlı yapı: **1xxxx ham kağıt → 23xxx BB yarı mamül (baskılı) → 3xxxx mamul**. Netsis'te 2xxxx ürünlerin reçetesi 1xxxx ham + 4xxxx boyaya bağlı (oran 1:1 kg). Mevcut üretim akışı çıktı stoğu üretmiyordu; bu yüzden yeni izole ekran eklendi:
- `ConversionController` (Index + Convert) + `Views/Conversion/Index.cshtml` + sidebar "Yarı Mamül Dönüşüm".
- Kaynak ham bobin tüketilir → hedef yarı mamül için **yeni FscLot+FscSerial+ProductionEntry** oluşur, **FSC tipi kaynaktan devralınır** (CoC). Şema değişmedi. Boya yok (manuel, kullanıcı kararı).
- Doğru zincir artık: **Hammadde Girişi (1xxxx) → Yarı Mamül Dönüşüm (→23xxx) → İş Emri/Tüketim (23xxx tüket)**. Yani satın alma/irsaliye/fatura **1xxxx ham kağıt** için olmalı (BB için değil).

## MCD (Multi-Choice Dropdown) — coklu ürün/stok filtresi (2026-06-21)

**Amaç:** Tek tek ürün seçimi yerine bir seferde birden fazla ürün/hammadde seçebilme.

**Bileşen:**
```html
<div class="mcd" id="mcd-[page-id]" data-placeholder="— Ürün/Stok —">
    <button type="button" class="mcd-btn" onclick="mcdOpen('mcd-[id]')">
        <span class="mcd-lbl">[selected count]</span>
        <i class="fas fa-angle-down mcd-arrow"></i>
    </button>
    <div class="mcd-panel" id="mcd-[id]-panel">
        <div class="mcd-search-row">
            <input type="text" class="mcd-search" placeholder="Kod/ad ara..." oninput="mcdSearch(this)">
        </div>
        <label class="mcd-row mcd-header">
            <input type="checkbox" class="mcd-all-cb" onchange="mcdToggleAll(this,'mcd-[id]')">
            <span>Tümünü Seç/Temizle</span>
        </label>
        <div class="mcd-items">
            @foreach (var item in items) {
                <label class="mcd-row">
                    <input type="checkbox" name="itemIds" value="@item.Id" class="mcd-cb"
                           onchange="mcdUpdate('mcd-[id]')">
                    <span>@item.Code</span>
                    <span class="mcd-sub">@item.Name</span>
                    <span class="mcd-ext" title="Dış kod">@item.ExtCode</span>
                </label>
            }
        </div>
    </div>
</div>
```

**İnline JavaScript (view'de tanımlı, _mcdReady guard ile):**
- `window.mcdOpen(id)` → panel aç/kapat, öteki panelleri kapat
- `window.mcdSearch(inputEl)` → arama kutusuna göre satırları filtrele
- `window.mcdToggleAll(checkboxEl, id)` → "Tümünü seç" butonu
- `window.mcdUpdate(id)` → seçili sayısını label'e yaz, "Tümü seç" checkbox durumunu güncelle
- Dışarı tıklanınca tüm paneller kapanır (`mousedown` event)

**CSS:** `.mcd` flex container, `.mcd-panel` hidden default, `.mcd-row` label → input + metin + badge'ler, `.mcd-search` filtre input, `.mcd-sub` / `.mcd-ext` ürün kodu/dış kod badge'leri.

**Etkilenen sayfalar (2026-06-21 commit fc7c101):**
- `Stock/RawMaterial` — `productId` → `productIds[]` (MCD id=mcd-raw-prod)
- `Stock/Index` (Stok Durumu) — MCD id=mcd-idx-prod
- `Production/Index` (İş Emirleri) — MCD id=mcd-prod-idx
- `Production/WasteReport` (Fire Raporu) — MCD id=mcd-waste-prod
- `Reports/FscConsumption` — MCD id=mcd-fsc-prod
- `Reports/MaterialTrace` — mamul modu radio-button MCD

**Controller değişiklikleri:** `productId?: int` → `productIds?: int[]` parametreleri, query `.Where(w => productIds.Contains(w.ProductId))` ile `IN` sözdizimi. Excel export href'leri loop'la güncellendi:
```csharp
// Örnek: ReportsController.ChainOfCustody
if (productIds != null && productIds.Length > 0)
    query = query.Where(l => productIds.Contains(l.Product.Id));
```

**ViewBag değişiklikleri:** `ViewBag.ProductIds = productIds ?? Array.Empty<int>();` ile view'de checkbox durumları tutuluyor (`checked="@(prodIds.Contains(p.Id))"`).

## MCD entegrasyon uyarıları (2026-06-21)

1. **Birden çok MCD varsa ID'leri benzersiz yapma:** İki `mcdOpen` fonksiyonu yoksa ilki hiç açılmaz. Çözüm: her sayfada `window._mcdReady = true` guard ile coklu tanımlama engelleniyor; view'de ilk MCD'nin hemen sonuna `<script>window._mcdReady=true;</script>` ekle.

2. **Excel export href'i doğru loop'yla:** MCD seçimleri varsa link şu şekilde:
```html
<a href="@Url.Action("ExportChainOfCustody", 
    new RouteValueDictionary(
        new { supplierId, fscTypeId, stockCode, stockName, 
              productIds = string.Join(",", prodIds) }))">
```
Ya da form action'ı: `<form method="get" action="@Url.Action("ExportPage")">` + hidden input'lar + `onclick="form.submit()"`.

3. **Checkbox state tutulması:** View'de `prodIds` = `ViewBag.ProductIds as int[]`, checkbox template:
```razor
@{ var prodIds = ViewBag.ProductIds as int[] ?? Array.Empty<int>(); }
<input type="checkbox" name="productIds" value="@p.Id" class="mcd-cb"
       checked="@(prodIds.Contains(p.Id))" onchange="mcdUpdate('mcd-xyz')">
```

## MCD + draw.dt kombinasyon (2026-06-21)

Filtre ekranlarında MCD seçildikten sonra tablo yenilenmez (JS yenileme gerekli). Çözüm:
```html
<!-- Tablo yüklenmişse form gönderimi otomatik (normal), yoksa button: -->
<form method="get" id="filterForm">
    <!-- MCD + diğer input'lar -->
    <button type="submit" class="btn btn-primary">Filtrele</button>
</form>
```
Veya DataTables draw → stat kartlarını yenile (draw.dt + recalc pattern, bkz. lessons.md daha yukarıda).

Ama **form.submit()** yaparken url parametreleri doğru olmalı. Checkbox array'i `productIds=1&productIds=2` veya `productIds[]=1&productIds[]=2` ama ASP.NET Core `int[]? productIds` ile otomatik bind ediyor; hiçbir şey yapma — form normal şekilde submit et.

## StockMovement.ProductId nullable → non-nullable (2026-06-21)

**Değişiklik:** `public int? ProductId` → `public int ProductId` (non-nullable)

**Neden:** Tüm hareketi kayıt, ProductId şart. Entity'de nullable ise `== null` ve `.HasValue` yapılan tüm yerlerde karmaşa artıyor.

**Etkilenen kod:**
- `PurchaseController.SaveLot`: `sm.ProductId = model.ProductId.Value;` → `sm.ProductId = model.ProductId.Value;` (zaten guarded)
- `Stock/Index` / `Stock/RawMaterial`: `sm.ProductId?.ToString()` → `sm.ProductId.ToString()`
- Query'ler: `WHERE ProductId IS NOT NULL` döngüleri kaldırıldı

**Migration:** `StockMovement` tablosunda `ProductId INT NOT NULL` constraint eklendi (mevcut NULL kayıtlar yoksa, varsa backfill lazım).

## Stock Summary + Admin Stock sayfaları (2026-06-21)

Iki yeni view eklendi:

**Stock/Summary.cshtml:**
- Grup bazlı stok özeti (her ürün grubu + toplamlar)
- Checkbox filtresi (grup seçimi)
- Dinamik kartlar (toplam KG, giriş, çıkış, kalan)
- Detay butonuyla `Stock/RawMaterial`'a drill-down

**Stock/AdminStock.cshtml:**
- Tam stok admin görünümü (hesaplama adımları)
- Orijinal birimler (`InitialWeight` vs `OriginalQuantity`, dönüşüm faktörü)
- Sarf/döngü analizi
- Hata ayıklama için teknik alanlar (CreatedBy, UpdatedDate vb.)

**Menüde:** "Stok" → "Stok Özeti" (Summary), "Yönetici Stok" (AdminStock)

## Razor yorum satırında @section direktifi hata vermesi (2026-06-21)

**Belirti:** _Layout.cshtml compile hatası — "Unexpected character in tag helper element".

**Kök neden:** Razor parser HTML yorum satırında (`<!-- ... @section ... -->`) bulunan `@section` ifadesini Razor direktifi olarak yorumlamaya çalışıyor ve hata veriyor. Açıklama: HTML yorum `<!-- ... -->` Razor'un gözüne saydam değil; içindeki `@` karakterleri directive olarak parse edilir.

**Çözüm:** HTML yorumda `@` karakteri bulunacaksa kaldır veya Razor `@*...*@` yorum bloğu kullan:
```razor
// YANLIŞ
<!-- @section Scripts bloğu hata veriyordu -->

// DOĞRU
<!-- Section Scripts blogu -->
// veya
@* @section Scripts bloğu açıklaması *@
```

## MCD (Multi-Choice Dropdown) filtre layout — display:block + flex-column wrapper (2026-06-21)

**Belirti:** MCD butonları yanyana hizalanıyor ama içindeki input alanı alta inmiyor (label ve input aynı satırda).

**Kök neden:** MCD'nin CSS `display:inline-block` ayarından dolayı flex container içinde yatay yaygınlık buluyor. Etiket-input dikey hizalama için wrapper üzerine flex kuralı uygulanmıyor.

**Çözüm (iki adım):**
1. **Global CSS (_Layout):** `.mcd { display: inline-block; }` → `.mcd { display: block; }` 
2. **View'deki MCD grubu:** Her MCD etiketi ve MCD konteynerini kapsayan `<div>` üzerine flex-direction:column ve min-width ekle:
   ```html
   <div style="display:flex;flex-direction:column;min-width:160px;">
       <label class="form-label small fw-semibold mb-1">Tedarikçi</label>
       <div class="mcd" id="mcd-id" ...><!-- MCD buton ve panel --></div>
   </div>
   ```

**Nerede uygulandı:** Purchase/Index (Tedarikçi/Ürün/FSC Tipi MCD'leri), Production/WasteReport (Makine/Ürün MCD'leri).

**Filtre paneli hizalama (Purchase/Index):** `row g-2 align-items-end` → `d-flex align-items-end gap-2 flex-wrap` + her grup kendi min-width taşıyor.

## Razor option tag helper — seçili attribute'u dinamik yapabilme (2026-06-21)

**Durum güncelle:** lessons.md'deki eski uyarı hâlâ geçerli ama çözüm biraz farklı:
```razor
// DOĞRU (eski)
@{ var isSel = condition; }
<option value="@s.Id" selected="@(isSel ? "selected" : null)">

// DOĞRU (alternatif — teşekkürler)
<option value="@s.Id" selected="@condition">
// Razor null değer HTML'de attribute çıkarmaz; bool true → selected="selected"
```

## 5 sayfa MCD uyumluluğu sağlandı (2026-06-21)

**Yapılan:**
1. **Purchase/Index** — productIds[] eklenip filtre çalışıyor
2. **Stock/Summary** — productIds[] + ViewBag.AllProducts
3. **Production/Index** — woProductId select gizlendi (compat); MCD eklendi
4. **Conversion/Index** — sourceSerialId MCD + targetProductId MCD
5. **Production/WasteReport** — Makine/Ürün MCD + layout düzeltme

**Pattern:** Her sayfada MCD güncelleme şu adımları takip eder:
- HTML: `<div class="mcd" id="mcd-[page-id]">` yapısı
- Script: `window._mcdReady` guard ile coklu tanımlama önlenir
- Controller: `int? id` → `int[]? ids` parametresi; `WHERE IN (ids)` query filtresi
- ViewBag: seçili ID'leri tutup view'de checkbox durumunu korur

**Excel export:** Form action'ı veya href URL'de `productIds=1&productIds=2` şeklinde çoklu value geçişi — ASP.NET Core otomatik array binding yapar.

## Razor @foreach { } içinde ilk satırda @{ } kullanilamaz (2026-06-21)

**Belirti:** Razor hatası — `"Unexpected end of file after parsing a block"` veya compile hatasında `@{ }` açıklama bloğu `@foreach` altında.

**Kök neden:** `@foreach (var item in list) { @{ var x = ...; } ... }` → Razor parser blok başında başka bir statement beklediğinde, `@{ }` bloğu (code block) ilk satır olarak geçersizdir. Statement'ler ile karıştırılıyor.

**Çözüm:** Foreach bloğunun ilk satırında değişken bildirimi yapılacaksa `@{ }` kaldır, doğrudan kod yaz:
```razor
// YANLIŞ
@foreach (var s in suppliers) {
    @{ bool sel = condition; }
    <label>
        <input type="checkbox" checked="@(sel ? "checked" : null)">
    </label>
}

// DOĞRU
@foreach (var s in suppliers) {
    bool sel = condition;  // @{ } kaldırıldı
    <label>
        <input type="checkbox" checked="@(sel ? "checked" : null)">
    </label>
}
```

**Not:** `@foreach` içinde direktif olmayan kod blokları `{ statement }` şeklinde yazılır; `@{ }` Razor'un kod-HTML karma bölümleri için tasarlanmıştır.

**Uygulandığı:** `FSCTakip.WebUI/Views/Products/Index.cshtml` satır 66 ve 97 (tedarikçi ve ürün grubu MCD checkbox'ları için `bool sel` değişkeni).

## 6 sayfa MCD + layout düzeltmeleri tamamlandı (2026-06-21)

**Sayfalar:** Production/Index, Production/WasteReport, Conversion/Index ve 3 report sayfası (ChainOfCustody, BomAnalysis, MaterialTrace).

**Layout düzeltmeleri:**
- MCD `.mcd { display: block; }` (inline-block değil)
- MCD grubu wrapper `<div style="display:flex;flex-direction:column;min-width:...px;">` ile etiket-input dikey hizalama
- Filtre paneli `d-flex align-items-end gap-2 flex-wrap` (Bootstrap grid değil, elle hizalama)

**Referans:** CLAUDE.md §"MCD Filtresi — CSS display:block + flex-column Wrapper Layout Kuralı" bölümüne bakınız.

## Products/Index tedarikçi ve ürün grubu MCD filtreleri (2026-06-21)

**Yapılan:** Products/Index sayfası tedarikçi (supplierIds) ve ürün grubu (productGroupIds) MCD filtrelerine geçirildi.

**Veri akışı:**
- Controller: `ProductsController.Index` → `int[]? supplierIds`, `int[]? productGroupIds` parametreleri
- Query filtresi: `query.Where(p => supplierIds.Contains(p.SupplierId))` → `IN` sözdizimi
- ViewBag: `SupplierIds`, `ProductGroupIds` array'leri checkout state tutmak için
- View: MCD panelinde `checked="@(supplierIds.Contains(sid))"` ile durumlar korunur

**Checkbox state tutulması (MCD + form):** ViewBag'deki array'i `checked` attribute'üne bind etmek için:
```razor
@{ var supplierIds = ViewBag.SupplierIds as int[] ?? Array.Empty<int>(); }
<input type="checkbox" name="supplierIds" value="@s.Value" 
       checked="@(supplierIds.Contains(int.Parse(s.Value)) ? "checked" : null)">
```

**Not:** `int.TryParse` güvenli olmak için kullanıldı; item Value her zaman int gibi görünmez (dropdown'da string olabilir).

## Bağımlı field otomatik doldurma — çoklu filtre dialog pattern (2026-06-21)

**Amaç:** Conversion gibi çoklu filtre dialog'larda, bir field (örn. stok kodu) seçilince bağımlı field'lar otomatik set edilebilmeli.

**Örnek — Conversion/Index stok kodu → stok adı:**
```javascript
if (field === 'kod') {
    var ads = _unique(_srcFiltered(), 'ad');  // Kod filtresi sonrası unik stok adları
    if (ads.length === 1) {  // Tek bir seçenek varsa
        _srcSel.ad = ads[0];  // Stok adını otomatik seç
        _srcUpdate('ad');  // UI'ı güncelle
    }
}
```

**Deseni:** Her `_srcSel[field]` set'inde, bağımlı field'ların seçenek sayısını kontrol et. Tek seçenek varsa (`length === 1`) otomatik seç; yoksa kullanıcı seçsin. Bu UX fluidliğini artırır — "3 filtreyi seçersen 4. otomatik gelir".

**Uygulandığı:** `FSCTakip.WebUI/Views/Conversion/Index.cshtml` satır 326-332 (stok kodu seçilince stok adı).

## Varsayılan filtre + "Tüm Kayıtları Göster" toggle (2026-06-21)

**Amaç:** Kullanıcı filtre seçmediğinde sensible varsayılan yapı göster (örn. Purchase'da Hammadde+YM+BS), ama "Tüm Kayıtları Göster" seçeneğiyle full liste erişimini sağla.

**Yapı:**
- Controller: `bool showAll = false` parametresi + `hasUserFilter` bayrağı (filtre seçildi mi = supplierIds/productIds/stockCode/etc. boş değil)
- `hasUserFilter == false && showAll == false` → varsayılan GroupIds (1,3,4 = Hammadde, YM, BS) filtresiyle query çalışır
- `showAll == true` → filtreler yok, tüm kayıtlar
- ViewBag: `IsDefaultFilter` ve `ShowAll` boolean'ları view'de render kontrolü için

**View HTML:** İki info bander:
1. Varsayılan filtre aktif: "Varsayılan görünüm: Hammadde · Yarı Mamül · Burgu Sap grupları. [Tüm Kayıtları Göster]"
2. ShowAll aktif: "Tüm kayıtlar gösteriliyor. [Varsayılana Dön]"

**URL deseni:**
- `?` veya parametresiz → varsayılan filtre + bilgi bandı
- `?showAll=true` → tüm kayıtlar + ikinci bilgi bandı
- `?productIds=5&productIds=6` veya diğer filtreler → varsayılan geçersiz, seçilen filtreler uygulanır

**Kullanılan yerler (2026-06-21):** `Purchase/Index` (Hammadde+YM+BS).

**Not:** Bu pattern diğer sayfalar (Stock/Index, Production/Index) için de uygulanabilir; her sayfa kendi varsayılan GroupIds'ini tanımlayabilir.

**CSS:** Bilgi bandı inline style (rgba mavi/beyaz arka plan, 12.5px font), hover efekti veya geçiş animasyonu YOK (statik banner). Linkler `btn btn-sm` sınıfı taşır, hover renk değişimi minimal.

## ProductRecipe BilesenYeri alanı — mamulde bileşen konumu (2026-06-21)

**Amaç:** Reçete'de her bileşenin mamülün hangi bölümünde kullanıldığını (Gövde, Sap, Dip Kapak, Etiket, Diğer) belirtebilme.

**Yapı:**
- `ProductRecipe.BilesenYeri`: nullable string — tüketim girerken ve BOM analizinde bileşen ayrımı için
- Migration: `20260621182117_AddBilesenYeriToProductRecipe` — ProductRecipes tablosuna `BilesenYeri` sütunu (nvarchar(max), nullable)

**Uygulandığı yerler:**
- `Products/Recipe.cshtml`: Reçete modal'ında Bileşen Yeri dropdown (veya text input)
- Production/Detail.cshtml: Tüketim girerken bileşen yeri gösterilip seçimi mümkün olacak
- BOM Raporu: Bileşen yeri bazında gruplandırılıp alt toplam gösterebilecek

**Not:** Alan eklenmiş ama Conversion akışında doğrudan kullanılmıyor — henüz UI'ı entegre edilmemiş.

## Products/Recipe.cshtml — Coklu bileşen seçimi + dinamik miktar girişi (2026-06-21)

**Amaç:** Reçete düzenlemesinde bir seferde birden fazla bileşen seçip, her biri için ayrı miktar/birim/yeri satırını dinamik açabilme.

**Yapı:**
- **"Bileşen Ekle" butonu:** `addModal` açıyor (coklu seçim modu)
- **addModal:** arama kutusu + checkbox ürün listesi, tümünü seç/temizle, panel açık/kapalı durumu
- **Seçim sonrası:** Her seçili ürün için dinamik satır ekleniyor (Ürün Kodu, Ad, Birim, Miktar, Bileşen Yeri)
- **"Tümünü Kaydet" butonu:** Tüm seçimleri sırayla kaydediyor; hata varsa toast bildirim
- **Düzenleme (id>0):** openModal sadece tekli mode'de, gizli select dropdown ile (sayfayı tekli UI olarak gösterir)
- **editProductName input:** Seçili ürün adını otomatik gösterip, ürün değiştirimi engeller (düzenleme sırasında karışıklık önleme)

**Teknik detaylar:**
- `editModal` ID'li item seçilince `editProductId`/`editProductName`/`editStdQuantity`/`editUnit`/`editBilesenYeri` alacakları doldurulur
- Yeni kayıt: `<div id="editRow-[seq]">` dinamik olarak ekle/kaldırabilir (sequence tekil indexleme)
- Form submit: POST → `ProductsController.SaveRecipe` (batch işlem, loop → SaveChangesAsync)

**Uyarıları:**
- Coklu kayıt sırasında birisi başarısız olursa (validasyon hatası), sıradaki kayıtlar yine işlenebilir (transaction'a alınmadı — rollback yok)
- Aynı ürün tekrar seçilirse duplicate satırlar açılabiliyor; UI'da bu kontrol edilmemiş

**Uygulandığı:** `FSCTakip.WebUI/Views/Products/Recipe.cshtml` (commit 31dc698).

## Production/Detail tüketim modal — BilesenYeri integrasyon (2026-06-21)

**Yapı:**
- Tüketim girerken, Bileşen Yeri dropdown'u görüntüleniyor (veya text input)
- ProductRecipe'de BilesenYeri varsa seçili olarak gelir; kullanıcı değiştirebilir

**Henüz yapılmayan:** Production/Detail.cshtml'de modal UI eksik — henüz eklenmiş değil; ProductionDetail entity'sinde BilesenYeri alanı bulunmuyor. Sadece ProductRecipe'de tanımlandı.

## Conversion modal'ında stok kodu seçilince stok adı otomatik doldurma (2026-06-21)

**Amaç:** Filtreleme dialog'larında (örn. Conversion/Index sourceSerial seçimi) bağımlı alan otomatik set edilebilmeli.

**Yöntem:**
```javascript
// Seçilen bileşenin filtrelenmiş listesi (_srcFiltered()' ten unik değerleri al
var ads = _unique(_srcFiltered(), 'ad');
if (ads.length === 1) {  // Tek seçenek varsa
    _srcSel.ad = ads[0];  // Otomatik seç
    _srcUpdate('ad');      // UI yenile
}
```

**Zaman:** Kullanıcı bir field (örn. kod) seçtikten hemen sonra; sistem diğer field'ları kontrol ediyor. Seçenek sayı 1 ise otomatik set et; >1 ise kullanıcı seçsin.

**Uygulandığı:** `Conversion/Index.cshtml` satır 326-332 (stok kodu seçilince stok adı).

**UX faydası:** "Kod seçersen ad otomatik gelir; 3 filtreyi ayarlarsam 4. otomatik oluşur" gibi progressive disclosure sağlıyor — form doldurmayı akıcılaştırır.

## Çoklu ürün modal pattern — "addModal" → "formModal" ayrımı (2026-06-21)

**Amaç:** Reçete düzenlemesinde iki işlem modu:
1. **Çoklu Bileşen Ekleme** (`addModal`) — bir seferde birden fazla ürün seçip her biri için miktar girişi
2. **Tekli Düzenleme** (`formModal`) — mevcut reçete satırını düzenleme

**Yapı:**
- **addModal** (`id="addModal"`, modal-lg): Arama kutusu → checkbox ürün listesi → seçilen her ürün için dinamik satır (miktar/birim/yeri)
- **formModal** (`id="formModal"`, modal-dialog-centered): Tekli düzenleme — ürün readonly gösterilir, sadece miktar/birim/yeri editlenebilir
- Buton çağrıları: 
  - "Bileşen Ekle" butonu → `openAddModal()` (yeni çoklu ekle)
  - Tablo satır düzenleme → `openModal(id)` (id>0 ise tekli edit; id==0 ise `openAddModal()` redirect)

**JavaScript mekanizması:**
- `openAddModal()`: Tüm checkbox'ları sıfırla, arama kutusu temizle, seçim satırları sil, paneli göster/gizle
- `filterAddList()`: Arama kutusuna yazdıkça ürün listesini `data-filter` attribute'ına göre filtrele (kod+dış kod+ad)
- `onAddCbChange(cb)`: Checkbox seçilince dinamik satır div'ini (`addRow_[pid]`) oluştur, seçilmezse sil
- `saveAllLines()`: Seçili ürünleri loop'la kaydedi; her birine POST (`/Products/SaveRecipeLine`) → batch insert → toast + reload

**HTML özellikler:**
- Checkbox row: `.add-prod-row` flex label, `data-filter` ve `data-name/code/ext` attribute'leri
- Seçim paneli: `addSelections` id'li div, başlangıçta hidden (seçim yapılınca gösterilir)
- Dinamik satır: `addRow_[pid]` id'li, 3 input (qty/unit/yer), buton bar'ında hızlı seçim (Gövde/Sap/Dip Kapak/Etiket/Diğer)

**Farklılık eski tasarımdan:**
- Eski: `<select>` dropdown tek ürün seçimi + form submit
- Yeni: Checkbox liste + çoklu seçim + her biri için inline miktar girişi + async batch save

**Tuzak:** `saveAllLines()` loop'ta hata varsa o ürünü skip ediyor (transaction yok). Kısmi başarı →  "X bileşen eklendi" toast + hata listesi. Kullanıcı hataları düzeltip tekrar eklemelidir.

**Uygulandığı:** `FSCTakip.WebUI/Views/Products/Recipe.cshtml` (commit c44af09).

## Arama kutusu selector değişimi — data-filter vs data-search (2026-06-21)

**Sorun:** Recipe sayfasında arama kutusu (`childProductSearch`) çalışmıyor → "Bileşen Ekle"deki tüm ürünler görünüyor, filtresi yok.

**Kök neden:** Eski modal'da `<select id="childProductId">` alanının `data-search` attribute'üne JavaScript filter uygulanıyordu (`childProductSearch.oninput` → option'ları gizle). Yeni addModal'da `<label class="add-prod-row" data-filter="...">` yapısı kullanıldı; ama arama box'ın ID'si hâlâ `addSearch` (değiştirildi). JS fonksiyonları güncellendi.

**Çözüm:** 
- Arama box: `<input id="addSearch" ... oninput="filterAddList()">`
- Filter fonksiyonu: Ürün row'larının `data-filter` attribute'ini sorgula (kod+dış kod+ad kombinasyonu, lowercase)
- Teknisyen kolaylığı: `data-filter` vs `data-search` iki farklı pattern'dir; aynı sayfada tuple attribute adı consistent olmalı

**Not:** Diğer sayfalar (Purchase, Stock) `data-filter` kullanıyor. Recipe sayfası artık aynı pattern'i takip ediyor.

## Tekli modal'da ürün read-only gösterimi (2026-06-21)

**Yapı:** Mevcut reçete satırı düzenlemesinde (formModal):
- `editProductName` input: readonly, background `#f8fafc`, metin koyu — sadece gösterim amaçlı
- `childProductId` input: hidden, gerçek ürün ID'si tutuyor
- Düzenleme açıldığında `GetRecipeLine` API'si ürün ID döndürür → `.add-prod-row .add-cb` listesinde aranıp name bulunur → `editProductName`'e yazılır

**Amaç:** Kullanıcı mevcut ürünü yanlışlıkla değiştiremez (ürün değişirse reçete mantığı kırılabilir). Sadece miktar/birim/yeri editlenebilir.

**Teknisyen notu:** EditProductName'i text readonly yerine label olarak göstermek daha zarif (input değil static metin). Ama input alanı doldurma işlemi çok hızlı; label ile inline text de yapılabilir.

**Uygulandığı:** `FSCTakip.WebUI/Views/Products/Recipe.cshtml` düzenleme flow'u.

## Production/Detail — Bileşen seçimi dropdown yerine hızlı tiklama butonları (2026-06-21)

**Amaç:** Tüketim girerken bileşen seçimi dropdown'dan kurtulup, sık kullanılan bileşenleri hızlı butonlarla seçebilme.

**Yapı:**
- Reçete modal'da bileşen listesi (ProductRecipe) checkbox + buton bar olarak gösterilir
- **Hızlı seçim butonları:** "Gövde", "Sap", "Dip Kapak", "Etiket", "Diğer" — bileşen yeri (BilesenYeri) filterler
- Kullanıcı buton tıklar → seçili bileşenleri filtrele → liste güncellenir
- Seçili bileşen `editSelected`/`editBilesenYeri` input'larına otomatik yüklenir

**Teknik detaylar:**
- Bileşen satırları `.recipe-row` label'ler; checkbox + metin + buton bar
- Buton click: `onQuickSelect(yeri)` → bileşenleri filtrele, seçili olanları göster
- Bootstrap grid grid'de buton layout düzeltme: `gap-1` ve `flex-wrap` ile responsive tasarım

**Tuzak:** Çok sayıda bileşen varsa buton bar'ının yüksekliği tablo hizalanmasını bozabilir (sticky positioning). Min-height veya grid row-span ile dengelenmesi gerekebilir.

**Uygulandığı:** `FSCTakip.WebUI/Views/Production/Detail.cshtml` (commit b616e23).

## Production/Detail — Sticky işlem butonu + min-width sutun genislikleri (2026-06-21)

**Sorun:** Tüketim tablosunda işlem butonları (Düzenle/Sil) satır scroll'lanırken kayboluyor (sticky position broken).

**Kök neden:** Tablo `<table>` element'inde `position: relative` yok; sticky column'lar parent bağlamını kaybediyor. İlaveten, `.col-auto` ve `flex-grow` sutunlar masaya sığmadığında min-width tanısı yok.

**Çözüm:**
- `<table>` wrapper `position: relative` ve `overflow-x: auto` (horizontal scroll)
- Işlem butonlarını `<th>` ve `<td>` içine koy: `position: sticky; right: 0; z-index: 10;`
- Tüm sutunlara min-width ver (örn. 100px); flex-grow ile büyümesine izin ver
- Tüketim, Fire, Üretim sayısal sutunları `text-end` align
- Last column (işlem) `width: 120px; min-width: 120px;` ile sabit genişlik

**CSS pattern:**
```css
.sticky-col {
    position: sticky;
    right: 0;
    z-index: 10;
    background: white;  /* scroll'da arka plan tutulmalı */
}
td.sticky-col {
    box-shadow: -2px 0 4px rgba(0,0,0,.1);  /* kenar gölgesi */
}
```

**Uygulandığı:** `FSCTakip.WebUI/Views/Production/Detail.cshtml` (commit c319c16).

## Production/Detail — CoC (Chain of Custody) modal otomatik açılış (2026-06-21)

**Amaç:** Bileşen düzenlemesinde, kullanıcının CoC kontrol etmesi için modal otomatik açılsın.

**Yapı:**
- Edit modunda (`openModal(id)` → id > 0) form doldurulduktan sonra `showCoC` flag'i true olur
- Modal render'ında `@if (ViewBag.ShowCoC) ... ready` durumu check edilir
- Bootstrap modal'ın `.show` sınıfı + `backdrop: 'static'` ile modal açılır ve dışarı tıklama kapanmaz
- Kullanıcı CoC'yi kontrol edip "Tamam" veya "İptal" tıklar

**İş akışı:**
1. Bileşen edit → form doldurulur → `openModal(id)` çağrılır
2. Controller `Edit` action'ında `ViewBag.ShowCoC = true` set edilir
3. View render → CoC modal otomatik açılır
4. Kullanıcı kontrol edip modal kapat → tüketim form ready

**Tuzak:** Modal content (CoC tablosu) ağırsam bootstrap modal animation delay'ı fark olabilir (biraz yavaş görünür). Performans için CoC verilerini pre-load etmek önerilir.

**Uygulandığı:** `FSCTakip.WebUI/Views/Production/Detail.cshtml` (commit c838990).

## Production/Detail — Tüketim tablosuna Bilesen + Kullanim yeri sutunu (2026-06-21)

**Amaç:** Tüketim listesinde hangi bileşenin kullanıldığını ve hangi bölümde (Gövde/Sap/vb.) açıkça gösterme.

**Yapı:**
- Tüketim tablosu `<tr>`'lerine yeni sutunlar: `Bileşen (ürün adı)` + `Kullanım Yeri`
- Bileşen adı: `ProductionDetail → ProductRecipe → Product.ProductName` (navigation via FK)
- Kullanım yeri: `ProductionDetail.BilesenYeri` (nullable string) — ProductRecipe'den inherit edilir
- Bootstrap sutun genişliği: `col-md-3` (bileşen), `col-md-2` (yer) — responsive

**Veri akışı:**
- SaveDetail'de ProductRecipeId alınır → reçete kaydına erişilir
- BilesenYeri, ProductRecipe'den çekilir ve ProductionDetail'e yazılır
- Edit/Delete operasyonlarında sutun güncellenir

**CSS:** 
- Bileşen adı `font-weight: 500` (vurgu)
- Kullanım yeri `text-muted` (gri, ikincil)
- Responsive breakpoint'lerde satır kaydırması düşük ekranlar için

**Uygulandığı:** `FSCTakip.WebUI/Views/Production/Detail.cshtml` (commit 81a77d3).

## FscLot.SourceSerial navigation property — anonim FK'den typed property'e (2026-06-22)

**Sorun:** Dönüşüm izlenebilirliğinde (YM → kaynak ham bobin) `FscLot.SourceSerialId` FK var ama navigation property yok → Controller'da her seferde `_context.FscSerials.Find(lot.SourceSerialId.Value)` çağrı gerekiyordu.

**Çözüm:**
- `FscLot.cs`'ye navigation property eklendi: `public virtual FscSerial? SourceSerial { get; set; }`
- `AppDbContext.cs` fluent config: `.HasOne<FscSerial>().WithMany()` → `.HasOne(l => l.SourceSerial).WithMany()` (anonim yerine typed)
- Include zinciri artık çalışıyor: `.Include(l => l.SourceSerial).ThenInclude(src => src!.Lot)...`

**Kod örneği:**
```csharp
// Eski (FK kaynaşma)
var srcBobin = lot.SourceSerialId.HasValue
    ? _context.FscSerials.Find(lot.SourceSerialId.Value)
    : null;

// Yeni (navigation)
var srcBobin = lot.SourceSerial;  // veya Include ile pre-loaded
```

**Not:** Dönüşüm Düzenle/Silme (`Conversion/UpdateConversion`, `DeleteConversion`) action'larında Include zincir eklendi.

**Uygulandığı:** `FscLot.cs`, `AppDbContext.cs`, `ConversionController.cs`, `ProductionController.cs` (commit dfc96ab, 539ed15).

## Conversion/Index — Son Dönüşümler filtresi + Düzenle/Sil butonları (2026-06-22)

**Amaç:** Dönüşüm geçmişi sayfasında (Last Conversions tablosu) kaynak ürün/hedef YM/FSC tipi/tarih filtreleri + satır başına Edit/Delete butonları.

**Filtre komponentleri (MCD + tarih):**
- **Kaynak Ürün MCD:** Kaynak ham/YM ürün adına/koduna göre filtre
- **Hedef YM MCD:** Hedef yarı mamül ürün adına göre filtre
- **FSC Tipi MCD:** FSC-100 / FSC-MIX vb. kategorisine göre filtre
- **Tarih Input:** Belirli tarihte yapılan dönüşümleri göster

**Filtreleme JS mekanizması:**
```javascript
function applyRecentFilter() {
    var selKaynak = getChecked('rf-kaynak-cb');  // seçili kaynak ürünler
    var selHedef  = getChecked('rf-hedef-cb');
    var selFsc    = getChecked('rf-fsc-cb');
    var tarih     = document.getElementById('rfTarih').value;
    
    // Her `<tr class="recent-row">` veri attribute'leri kontrol et (data-kaynak, data-hedef, data-fsc, data-tarih)
    // Filtreler boşsa veya eşleşirse satırı göster
}
```

**Satır işlem butonları:**
- **Düzenle (✏️):** Modal açar → tarih + fire(kg) düzenle → POST `/Conversion/UpdateConversion`
- **Sil (🗑️):** Onay iste → POST `/Conversion/DeleteConversion` → kaynak bobin miktarı geri yüklenir

**Controller yeni action'ları:**
- `GetConversion(serialId)` — mevcut veri JSON dönüş (edit modal doldurma için)
- `UpdateConversion(serialId, tarih, fire)` — tarih/fire update
- `DeleteConversion(serialId)` — lot+seri sil, StockMovement çıkış kaydını sil, kaynak bobini restore

**HTML `<tr>` data attribute'leri:**
```razor
<tr class="recent-row"
    data-filter="@filterText"  // Tüm alanlar → arama
    data-kaynak="@(...KaynakKod KaynakAd)"  // Kaynak ürün filtresi
    data-hedef="@r.Hedef.ToLower()"
    data-fsc="@r.FscType.ToLower()"
    data-tarih="@r.Tarih:yyyy-MM-dd">
```

**Tuzaklar:**
- MCD checkbox'larının `value` attribute'ü lowercase olmalı (filtre metni de lowercase)
- `data-tarih` format `yyyy-MM-dd` (form input value eşleşmesi için)
- Edit modal açılırken Fire decimal'i InvariantCulture ile geçilmeli

**Uygulandığı:** `ConversionController.cs` (Recent list expand, GetConversion, UpdateConversion, DeleteConversion), `Conversion/Index.cshtml` (filtre panel + butonlar, modal) (commit c09abba, c221a87).

## Production/Detail — GroupBy key ve SourceSerial izlenebilirliği (2026-06-22)

**Sorun:** Tüketim/Bobin seçim panelinde, YM kaynaklı ürünlerin kaynak ham bilgisi gösterilmiyordu. Ayrıca, `filterSerials()` JS'de panel ID'si güvenli alınamıyordu.

**Çözüm 1 — GroupBy Key Optimizasyonu:**
```csharp
// Eski: GroupBy keyi çok alan taşıyordu (bloat)
var byLot = pg.GroupBy(s => new {
    PartiNo  = s.Lot?.PartiNo,
    Supplier = s.Lot?.Supplier?.Name
    // ... + başka alanlar
});

// Yeni: Minimal key; SourceSerial'i ayrı oku
var byLot = pg.GroupBy(s => new {
    PartiNo     = s.Lot?.PartiNo,
    Supplier    = s.Lot?.Supplier?.Name,
    LotDate     = s.Lot?.ArrivalDate.ToString("dd.MM.yyyy"),
    IsYariMamul = s.Lot?.Supplier == null && s.Lot?.SourceSerialId != null
});

// Razor'da: lg.First().Lot?.SourceSerial ile kaynak bilgisini al
var srcSerial = lg.First().Lot?.SourceSerial;
var kaynakPartiNo = srcSerial?.Lot?.PartiNo;
```

**Çözüm 2 — Seçim Panel ID'si Güvenli Alma:**
```razor
<!-- HTML'de data attribute ekle -->
<div class="serial-product-header"
     data-panelid="@prodGroupId"
     onclick="toggleProdGroup('@prodGroupId')">

<!-- JS'de: onclick'ten regex parse etme yerine data attribute oku -->
const panelId = hdr.dataset.panelid || hdr.getAttribute('onclick')?.match(/'([^']+)'/)?.[1];
```

**Çözüm 3 — onmousedown + preventDefault():**
```razor
<!-- Olay sırasını kontrol et: grup başlığı tıklaması dropdown kapatmasın -->
<div onclick="toggleProdGroup('@prodGroupId')"
     onmousedown="event.preventDefault()">
```
Bu, `selectSerial()` öncesinde `hideSerialDropdown()` call'ının grup header'ı toggle etmesini engeller.

**Kaynak Hammadde Görünümü (yeni UI):**
```html
<!-- YM dönüşüm kaynaklı kayıtlar için -->
<div style="background:#fef3c7;color:#92400e;padding:3px 8px;border-radius:4px;">
    ♻️ Dönüşüm
</div>

<!-- Kaynak ham bobin izlenebilirliği banner -->
<div style="background:#eff6ff;border-left:3px solid #3b82f6;">
    🔗 HAM. KAYNAK: <strong>@kaynakPartiNo</strong> @kaynakSeriNo @kaynakExtKod · @kaynakTedarikci
</div>
```

**Uygulandığı:** `ProductionController.cs` (Include zincir SourceSerial + nested navigations), `Production/Detail.cshtml` (GroupBy key minimal, srcSerial öncü okuma, UI banner, JS data-panelid) (commit 38eeee7, 6964415, 4c1872b).

## JavaScript event yönetimi — onclick vs onmousedown preventDefault (2026-06-22)

**Durum:** Dropdown panel'i kapalı tutmak için `onmousedown="event.preventDefault()"` kullanıldı (group başlığında).

**Mekanizma:**
- `onmousedown` → `selectSerial()` → `hideSerialDropdown()` sırasında
- `onmousedown` return false yapması grup toggle'ı engeller
- Ama `mouseup` / `click` yine de tetiklenebiliyor (dropdown kapalı kalsa da)

**Tuzak:** Bazen Seri satırı (`<tr onclick="selectSerial(...)"`) tıklandığında grup başlığı toggle ediliyordu. Nedeni: event propagation + timing.

**Çözüm:** Her iki handler'da:
```razor
<div onmousedown="event.preventDefault()" onclick="toggleProdGroup('@prodGroupId')">
     <!-- Grup başlığı -->
</div>

<tr onclick="selectSerial(...)">  <!-- Seri satırı — onClick doğrudan çağrılır -->
```

**hideSerialDropdown() optimizasyonu:**
```javascript
function hideSerialDropdown() {
    setTimeout(() => {
        const dropdown = document.getElementById('serialDropdown');
        if (!dropdown) return;
        // Aktif eleman dropdown içindeyse kapat
        if (dropdown.contains(document.activeElement)) return;
        dropdown.style.display = 'none';
    }, 400);  // 200ms → 400ms (grup genişletme + seri tıklama için yeterli)
}
```

**Not:** Pure CSS `:hover` veya `pointer-events: none` alternative'leri varsa daha temiz; ama bu event order mekanizması dokümante etmek faydalı (future refactor için).

**Uygulandığı:** `Production/Detail.cshtml` (commit 6964415, 4c1872b).

## Production/Detail — selectSerial input value gösterimi (2026-06-22)

**Iyileştirme:** Bobin seçildiğinde arama kutusu (`serialSearch`) hâlâ boş kalmıyordu (placeholder'da sadece ✓ işareti + seri no). Artık seçili seri no açıkça input.value'de görünüyor.

**Eski davranış:**
```javascript
input.value = '';
input.placeholder = serialNo + ' ✓';  // Placeholder'da gizli
input.style.background = '#dcfce7';   // Sadece renk ile anlaşılıyor
```

**Yeni davranış:**
```javascript
input.value = serialNo;  // Seçilen seri no açıkça input alanında
input.placeholder = serialNo;
input.style.background = '#dcfce7';  // Yeşil arka plan
input.style.fontWeight = '600';      // Vurgulu yazı tipi
```

**UX avantajı:** Kullanıcı seçili bobin numarasını input'ta açıkça görür; placeholder'a bakmaya gerek kalmaz. Daha net ve profesyonel görünüm.

**Uygulandığı:** `FSCTakip.WebUI/Views/Production/Detail.cshtml` satır 1040-1047 (commit afde463).
