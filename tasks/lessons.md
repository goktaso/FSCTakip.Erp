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
