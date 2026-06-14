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

## Yarı Mamül Dönüşüm özelliği eklendi (2026-06-14)

3 katmanlı yapı: **1xxxx ham kağıt → 23xxx BB yarı mamül (baskılı) → 3xxxx mamul**. Netsis'te 2xxxx ürünlerin reçetesi 1xxxx ham + 4xxxx boyaya bağlı (oran 1:1 kg). Mevcut üretim akışı çıktı stoğu üretmiyordu; bu yüzden yeni izole ekran eklendi:
- `ConversionController` (Index + Convert) + `Views/Conversion/Index.cshtml` + sidebar "Yarı Mamül Dönüşüm".
- Kaynak ham bobin tüketilir → hedef yarı mamül için **yeni FscLot+FscSerial+ProductionEntry** oluşur, **FSC tipi kaynaktan devralınır** (CoC). Şema değişmedi. Boya yok (manuel, kullanıcı kararı).
- Doğru zincir artık: **Hammadde Girişi (1xxxx) → Yarı Mamül Dönüşüm (→23xxx) → İş Emri/Tüketim (23xxx tüket)**. Yani satın alma/irsaliye/fatura **1xxxx ham kağıt** için olmalı (BB için değil).
