# Alınan Dersler — FSC Takip ERP

## İndeks

64 kayıt. Oturum başı hook'u **yalnız bu indeksi** enjekte eder — ham dosya
86 KB ve her prompt'ta taşınması ~22K token/tur eder. İlgili başlığı `Read` +
`offset/limit` ile oku; dosyayı baştan sona okuma.

| # | Konu | Satır |
|---|------|-------|
| 1 | Beyaz etiket + çevrimdışı lisans kilidi (2026-07-04) | 3 |
| 2 | Sıfır kurulum hiç test edilmemişti — migration/seed zinciri 4 ayrı yerden kırıktı (2026-07-03) | 10 |
| 3 | WorkOrder.ActualQuantity gün-bazlı toplama hatası (2026-07-02) | 23 |
| 4 | ProductionDetail admin düzeltme + denetim izi (2026-06-30) | 35 |
| 5 | İş Emri Formu — Yazdır/PDF (çoklu id print action) (2026-07-01) | 49 |
| 6 | Satış stok yeterlilik kuralı + tarihsel içe aktarım + Fatura print view (2026-07-01) | 62 |
| 7 | Razor → JavaScript'e decimal basarken kültür tuzağı (2026-06-14) | 76 |
| 8 | Belge depolama: yapılandırılabilir kök + ayrı klasör + satış yükleme (2026-06-14) | 97 |
| 9 | Tüketim stok hareketi (çıkış) eksikti — Stok Durumu/Hareketleri'nde çıkış 0 (2026-06-14) | 112 |
| 10 | Fire (WasteWeight) stok bakiyesinden bağımsız kalem — tüketim değil (2026-06-29) | 128 |
| 11 | Tek tip mesaj kutusu standardı (2026-06-14) — §11'e eklenecek | 189 |
| 12 | Decimal form binding tr-TR'de bozuluyor — "6000.00" → 60000000 (2026-06-14) | 204 |
| 13 | ViewBag'e anonim tip geçince RuntimeBinderException (2026-06-14) | 214 |
| 14 | DataTables v2 arama kutusu ID değişti (2026-06-20) | 222 |
| 15 | draw.dt + stat kartı senkronizasyon pattern'i (2026-06-20) | 239 |
| 16 | Razor option tag helper — inline ternary yasak (2026-06-20) | 246 |
| 17 | Grup subtotal duplikasyon — tek mekanizma kuralı (2026-06-20) | 258 |
| 18 | ExternalCode OR filtresi (2026-06-20) | 262 |
| 19 | Proaktif test disiplini — build sonrası Playwright zorunlu (2026-06-20) | 273 |
| 20 | Ürün birimi KG/MT/ADET ile otomatik dönüşüm (2026-06-20) | 284 |
| 21 | Yarı Mamül Dönüşüm özelliği eklendi (2026-06-14) | 300 |
| 22 | MCD (Multi-Choice Dropdown) — coklu ürün/stok filtresi (2026-06-21) | 307 |
| 23 | MCD entegrasyon uyarıları (2026-06-21) | 367 |
| 24 | MCD + draw.dt kombinasyon (2026-06-21) | 387 |
| 25 | StockMovement.ProductId nullable → non-nullable (2026-06-21) | 401 |
| 26 | Stock Summary + Admin Stock sayfaları (2026-06-21) | 414 |
| 27 | Razor yorum satırında @section direktifi hata vermesi (2026-06-21) | 432 |
| 28 | MCD (Multi-Choice Dropdown) filtre layout — display:block + flex-column wrapper (2026-06-21) | 449 |
| 29 | Razor option tag helper — seçili attribute'u dinamik yapabilme (2026-06-21) | 469 |
| 30 | 5 sayfa MCD uyumluluğu sağlandı (2026-06-21) | 482 |
| 31 | Razor @foreach { } içinde ilk satırda @{ } kullanilamaz (2026-06-21) | 499 |
| 32 | 6 sayfa MCD + layout düzeltmeleri tamamlandı (2026-06-21) | 528 |
| 33 | Products/Index tedarikçi ve ürün grubu MCD filtreleri (2026-06-21) | 539 |
| 34 | Bağımlı field otomatik doldurma — çoklu filtre dialog pattern (2026-06-21) | 558 |
| 35 | Varsayılan filtre + "Tüm Kayıtları Göster" toggle (2026-06-21, genişletildi 2026-06-22) | 577 |
| 36 | ProductRecipe BilesenYeri alanı — mamulde bileşen konumu (2026-06-21) | 604 |
| 37 | Products/Recipe.cshtml — Coklu bileşen seçimi + dinamik miktar girişi (2026-06-21) | 619 |
| 38 | Production/Detail tüketim modal — BilesenYeri integrasyon (2026-06-21) | 642 |
| 39 | Conversion modal'ında stok kodu seçilince stok adı otomatik doldurma (2026-06-21) | 650 |
| 40 | Çoklu ürün modal pattern — "addModal" → "formModal" ayrımı (2026-06-21) | 670 |
| 41 | Arama kutusu selector değişimi — data-filter vs data-search (2026-06-21) | 702 |
| 42 | Tekli modal'da ürün read-only gösterimi (2026-06-21) | 715 |
| 43 | Production/Detail — Bileşen seçimi dropdown yerine hızlı tiklama butonları (2026-06-21) | 728 |
| 44 | Production/Detail — Sticky işlem butonu + min-width sutun genislikleri (2026-06-21) | 747 |
| 45 | Production/Detail — CoC (Chain of Custody) modal otomatik açılış (2026-06-21) | 775 |
| 46 | Production/Detail — Tüketim tablosuna Bilesen + Kullanim yeri sutunu (2026-06-21) | 795 |
| 47 | FscLot.SourceSerial navigation property — anonim FK'den typed property'e (2026-06-22) | 817 |
| 48 | Conversion/Index — Son Dönüşümler filtresi + Düzenle/Sil butonları (2026-06-22) | 841 |
| 49 | Production/Detail — GroupBy key ve SourceSerial izlenebilirliği (2026-06-22) | 890 |
| 50 | JavaScript event yönetimi — onclick vs onmousedown preventDefault (2026-06-22) | 950 |
| 51 | Production/Detail — selectSerial input value gösterimi (2026-06-22) | 987 |
| 52 | Toplam Fiziksel Stok kartı — Purchase + Stock/RawMaterial (2026-06-22) | 1010 |
| 53 | FSC Kütle Dengesi Partial (_FscStokOzeti.cshtml) — 4 sayfada ortak kart (2026-06-26) | 1041 |
| 54 | Stock/RawMaterial action'a varsayılan grup filtresi + ShowAll (2026-06-26) | 1113 |
| 55 | ViewData vs ViewBag farkı — Partial'lere ViewData aktarımı (2026-06-26) | 1129 |
| 56 | FSC'siz Türkçe ToUpper() Hata Tespiti ve Düzeltme (2026-06-26) — KRİTİK | 1145 |
| 57 | FSC Kütle Dengesi Formülü — Giriş/Tüketim/Kalan Kategorisizasyonu (2026-06-26) — SEMANTİK | 1177 |
| 58 | ToplamFizikselStok ViewBag Fallback Hata (2026-06-26) | 1201 |
| 59 | FSC Bakiye Kartları Partial — 3 büyük statü kartı (_FscStokOzeti.cshtml) (2026-06-26) | 1228 |
| 60 | StockMovement.Type vs MovementType — Naming (2026-06-26) | 1269 |
| 61 | FSC Kütle Dengesi — Merkezi Servis (FscMassBalanceService) (2026-06-27) | 1284 |
| 62 | Performans Mimarisi — Ne Zaman Ne Yapılır (2026-06-30) | 1307 |
| 63 | Kritik Gizli Bug — SaveChangesAsync Tüm String'leri Körü Körüne Büyütüyordu (2026-07-05) | 1327 |
| 64 | Kurulum Provası — Ek Dersler (2026-07-05, oğlun PC'si dress rehearsal) | 1339 |
| 65 | Tek tık kurulum paketi — FscErpSetup.exe (2026-07-16) | 1426 |

> Yeni ders eklerken bu tabloya da satır ekle (team-protocol → Öğrenimler kapısı).

## Beyaz etiket + çevrimdışı lisans kilidi (2026-07-04)

- **Beyaz etiket:** `CompanySetting` (tek satır tablo) → 3 print view'de belge ünvanı + firmanın kendi FSC CoC/Lisans kodları. Ürün markası (login/sidebar ARD) bilinçli sabit; yalnız **dışa dönük belgeler** müşteri ünvanı taşır. Seed varsayılanı eski hardcoded değer — ACORE görünümü değişmedi (canlı doğrulandı).
- **Lisans:** RSA-2048 imzalı `license.lic` (base64(payload).base64(imza)), makine bağlama = SHA256(Windows MachineGuid)[:16]. Özel anahtar repo DIŞI (`Desktop/ARD_Lisans/` — kasaya taşınmalı!), genel anahtar `LicenseService`'e gömülü. Global `LicenseFilter` SessionAuthFilter'dan ÖNCE kayıtlı; `LicenseController` [AllowAnonymous] — müşteri lisanssızken parmak izini görmeli. Upload kaydetmeden ÖNCE doğrular (bozuk dosya geçerli lisansı ezemez) + `[IgnoreAntiforgeryToken]` (Layout'suz sayfada token yok; güvenlik RSA imzada).
- **Tuzak — ContentRootPath:** lisans dosyası ContentRoot'ta aranır; VS/IIS Express'te bu **proje kökü değil `FSCTakip.WebUI/` klasörüdür** — dev lisansı iki yere de kopyalandı (`license.lic` + `FSCTakip.WebUI/license.lic`, ikisi de gitignored `*.lic`). Publish'te sorun yok (kök = publish klasörü).
- Üretici: `tools/license_gen.py` (repoda, özel anahtar YOLU parametre). Test: 4 birim test — gerçek dev lisansıyla imza matematiği; bozuk imza/yanlış makine reddi canlıda da doğrulandı.

## Sıfır kurulum hiç test edilmemişti — migration/seed zinciri 4 ayrı yerden kırıktı (2026-07-03)

**Bağlam:** Çoklu şirket doğrulama programı (satış öncesi) ilk kez tamamen boş bir DB'ye kurulum denedi. ACORE'un canlı DB'si aylar içinde adım adım evrildiği için şu sınıf hatalar hiç görünmemişti:

1. **Elle yazılmış migration'lar (.Designer.cs olmadan) EF tarafından GÖRÜLMEZ** — `[Migration]` attribute'ü Designer dosyasındadır; onsuz sınıf migration listesine girmez, sıfır DB'de sessizce atlanır. 3 migration böyleydi (AddExternalCodes, RenamePartiNoAddSerialLotNo, FscLotSupplierIdNullable). Belirti: sonraki migration "column does not exist" ile patlar.
2. **SSMS ile elle eklenen kolon** (StockMovements.QuantityKg) hiçbir migration'da yoktu — canlıda var, sıfır kurulumda yok. **Tespit yöntemi: INFORMATION_SCHEMA üzerinden canlı-vs-sıfır tam şema diff'i** — tek tek hata kovalamaktan çok daha hızlı, kalan TÜM boşlukları tek seferde gösterdi.
3. **İlk admin kullanıcısını hiçbir şey oluşturmuyordu** — login imkânsızdı. DbSeeder'a, demo-veri guard'ından BAĞIMSIZ bir AppUsers-boşsa-oluştur bloğu eklendi.
4. **Yama deseni:** eski migration'a dokunma; scaffold'la boş migration üret → dosya adı+attribute'ü doğru kronolojik ID'ye taşı → içine IF NOT EXISTS'li idempotent SQL yaz (canlıda no-op, sıfırda tamamlayıcı). `20260524120001` ve `20260526000003` bu desenle yazıldı.

**Tuzaklar:** (a) C# verbatim string (@"...") içinde SQL yorumu yazarken çift tırnak string'i kapatır — migration derlenmez; (b) `dotnet ef`/`dotnet build` IIS Express Debug kilidine takılıyorsa `--configuration Release` kullan, kullanıcının VS'ini durdurmasına gerek kalmaz; (c) test koşumları için `dotnet publish -o <scratchpad>` + `dotnet FSCTakip.WebUI.dll` (Kestrel) + `ConnectionStrings__DefaultConnection` env var — hem VS'le çakışmaz hem gerçek deployment modunu test eder.

**Kalıcı çıktı:** `tools/regression_suite.py` (7 kontrol, parametrik base-url) — her yeni müşteri kurulumunda koşturulmalı. Uppercase mekanizması kullanıcı adını `ADMİN` yapar; login Turkish collation'da çalışır ama farklı collation'da kırılabilir — kurulum şartına yazıldı.

## WorkOrder.ActualQuantity gün-bazlı toplama hatası (2026-07-02)

**Belirti:** Üretim iş emri, farklı günlerde (fiş 1 günde de 1 haftada da kapansa) parça parça tamamlandığında `WorkOrder.ActualQuantity` ve ilgili `StockMovement` (Tip=ProductionEntry) gerçek üretimin katları olarak şişiyordu (5 iş emrinde tespit edildi: bazıları 2 katı).

**Kök neden:** `ProductionController.CompleteWorkOrder` (~satır 141-150) ve `RecalcAllActualQty` (~satır 211-214) formülü: `GroupBy(ProductionDate.Date).Sum(g => g.Max(ProducedQuantity))` — yani her GÜNÜN max'ını alıp GÜNLERİ TOPLUYORDU. Oysa `ProductionDetail.ProducedQuantity` alanı günlük değil, **iş emrinin o ana kadarki TOPLAM kümülatif üretimini** taşır (bkz. `Views/Production/Detail.cshtml` içindeki zaten doğru olan `toplamUretim` hesaplaması — orada günlere bölmeden tek `Max()` alınıyor). İki kod parçası birbirine ters varsayımla yazılmış — biri "günlük delta" sanıyordu, diğeri "kümülatif toplam" biliyordu.

**Çözüm:** Her iki yerde de formül tek `prodDetails.Max(d => d.ProducedQuantity)` (günlere bölmeden, tüm satırlar arası tek max) olarak düzeltildi. `RecalcAllActualQty` ayrıca artık ilgili `StockMovement` (ProductionEntry) kaydını da aynı doğru değere senkronize ediyor — önceden sadece `ActualQuantity`'yi düzeltiyordu, StockMovement'ı hiç dokunmuyordu, bu da veri kaymasının kalıcı kalmasına yol açıyordu.

**⚠️ Tuzak — düzeltme, gerçek bir farklı veri sorununu ortaya çıkardı:** IE2026-004 (WorkOrderId=12)'de formül düzeltmesi `ActualQuantity`'yi 274.000'den 27.400'e düşürdü (ProductionDetail satırlarındaki gerçek max buydu) — ama bu iş emrinden gerçekte (kullanıcının verdiği `FSC_Fatura.xlsx` kaynaklı gerçek satış kayıtlarına göre) **274.000 adet satılmıştı**. Formül matematiksel olarak doğru çalıştı ama `ProductionDetail` tablosundaki kaynak veri bu bir iş emri için muhtemelen elle girişte 10 kat eksik girilmişti (274.000 yerine 27.400). Düzeltme sonrası bu iş emri **-246.600 negatif bakiyeye düştü** — hemen fark edilip `ActualQuantity`+`StockMovement` gerçek satışla eşleşecek şekilde (274.000) SQL ile geri alındı; `ProductionDetail` satırlarındaki asıl 27.400 değerine dokunulmadı (ayrı, daha dikkatli bir düzeltme gerektirir).

**Ders:** Bir hesaplama formülünü "doğru" formüle çevirmek, altındaki KAYNAK VERİ zaten yanlışsa yeni bir tutarsızlık (hatta negatif bakiye) açığa çıkarabilir. Kod düzeltmesinden sonra MUTLAKA gerçek dünya kanıtıyla (burada: gerçek satış/fatura kayıtları) çapraz kontrol et, sadece "formül artık iç tutarlı" demek yetmez.

## ProductionDetail admin düzeltme + denetim izi (2026-06-30)

**Amaç:** Tüketim kaydı (ProductionDetail) düzeltme/silme işlemini sadece admin yapabilsin, FSC CoC denetimi için neden zorunlu, eksik stokta düzeltmeye izin verme.

**Yapı:**
- Yeni mevcut auth sistemi kullanıldı: `BaseController.IsAdminUser` (session bazlı), yeni rol sistemi kurulmadı.
- `ProductionDetailAudit` tablosu (yeni entity + migration `AddProductionDetailAudit`) — kim/ne zaman/neden/eski-yeni değerler.
- `SaveDetail`: `model.Id > 0` (düzenleme) dalında admin + `correctionReason` zorunlu. Stok yetersizse (`diff > serial.CurrentWeight`) hata mesajı kullanıcıyı YM Dönüşüm sayfasına yönlendirir — yeni validasyon mekanizması kurulmadı, var olan `diff` kontrolü zaten yeterliydi (over-engineering'den kaçınıldı).
- `DeleteDetail`: aynı admin + reason zorunluluğu, silmeden önce audit kaydı yazılır (silme = stok her zaman geri eklenir, yetersizlik riski yok — ek kontrol gereksiz).
- UI: `Views/Production/Detail.cshtml` — düzenleme modalında "Düzeltme Nedeni" alanı sadece mevcut kayıt düzenlenirken görünür/zorunlu; silme `Swal.fire({input:'text', inputValidator})` ile neden zorunlu kılındı (native `prompt()`/`confirm()` kullanılmadı, proje SweetAlert2 zaten bu dosyada kurulu).
- Edit/Sil butonları `ViewBag.IsAdmin` ile gated; admin değilse kilit ikonu gösterilir.

**Build kilidi tuzağı:** `dotnet build` IIS Express + VS debugger DLL'i kilitlerken `MSB3027/MSB3021` hatası verir (gerçek derleme hatası değil). Çözüm: kullanıcıdan debug session'ı durdurmasını iste, sonra tekrar build et.

## İş Emri Formu — Yazdır/PDF (çoklu id print action) (2026-07-01)

**Amaç:** Excel'de elle doldurulan "Torba İş Emri Formu" yerine sistemden otomatik, hammadde tüketimini gösteren yazdırılabilir form; Detail sayfasından tekil, Index'ten checkbox ile toplu yazdırma.

**Yapı:**
- Yeni PDF kütüphanesi eklenmedi — mevcut desen (`Views/Sales/Print.cshtml`: `Layout = null` + `@media print` + `window.print()`) aynen tekrarlandı. "PDF formatında" isteği tarayıcının "Yazdır → PDF olarak kaydet" seçeneğiyle karşılanıyor.
- Çoklu iş emri desteği tek action'da: `ProductionController.PrintForm(int[] ids)` — query string `?ids=1&ids=2` (ASP.NET Core native array binding, ekstra parsing gerekmedi). `@model List<WorkOrder>`, view içinde `foreach` ile her iş emri kendi `.print-page` div'inde, CSS `page-break-after: always` (`:last-child` hariç) — tek id de çoklu id de aynı action/view'den geçer, dallanma yok.
- Hammadde kaynağı: `ProductionDetail.FscSerial.Lot` zinciri (Supplier, FscType, Product) — reçete/planlanan bileşenler DEĞİL, sadece gerçekleşen tüketim (kullanıcı kararı: form "sonuç" belgesi, planlama değil).
- Index'te toplu seçim: checkbox sütunu + `#chkAll` + JS `woUpdateSelection()` seçili sayıyı sayar, `#btnPrintSelected` disabled/enabled toggle eder — MCD gibi karmaşık komponent gerekmedi, düz checkbox yeterliydi (çoklu kombinasyon filtresi değil, sadece "hangi satırlar seçili" listesi).
- `window.open()` ile yeni sekmede açılıyor (form ayrı belge, mevcut sayfa navigasyonunu bozmamalı).

**Playwright doğrulama notu:** IIS Express arka planda kapalıyken Playwright `ERR_CONNECTION_REFUSED` verir — sunucunun gerçekten ayakta olduğunu (`F5`/Debug başlatılmış) teyit etmeden test scripti çalıştırma. Login session Playwright'ın kendi browser context'inde ayrı; kullanıcının kendi tarayıcısında giriş yapmış olması yardımcı olmaz, script içinde `/Account/Login` POST ile ayrıca giriş yapılmalı.

## Satış stok yeterlilik kuralı + tarihsel içe aktarım + Fatura print view (2026-07-01)

**Amaç:** Excel'den (FSC_Fatura.xlsx) 16 satır tarihsel sevkiyat verisini SalesOrder/SalesOrderLine olarak sisteme işle, negatif stok bakiyesi asla oluşmasın kuralını mimariye ekle, sevk irsaliyesi + satış faturası için şablon PDF üret.

**Bulgular ve kararlar:**
- `SalesController.Dispatch()` içinde stok yeterlilik kontrolü hiç yoktu — StockMovement kontrolsüz oluşuyordu. Kontrol Dispatch anına eklendi (SaveLine anına değil): fiziksel stok düşüşü ancak StockMovement oluştuğunda gerçekleşir; taslak siparişte fazla miktar girmek henüz negatif bakiye yaratmaz. Kural: `WorkOrder.ActualQuantity - Sum(StockMovements Type=SalesDispatch WorkOrderId=X) >= istenen miktar`, aksi halde tüm dispatch reddedilir (kısmi kayıt oluşmaz — kontrol döngüden önce, tüm gruplar için).
- `Sales/Index.cshtml`'de PDF görüntüleme butonları gerçek bir bug içeriyordu: `data-pdf-path="/@s.DispatchPdfPath"` — `/Document/Serve?key=...` yerine ham path kullanıyordu, dosya asla açılmıyordu (`Sales/Detail.cshtml` doğru pattern'i zaten kullanıyordu). Bug bu işin kapsamında düzeltildi çünkü yeni eklenecek PDF'ler görüntülenemeyecekti.
- PDF üretimi: yeni kütüphane (QuestPDF vb.) eklenmedi. Var olan ARD-markalı print view deseni (`Sales/Print.cshtml`) + Playwright'ın headless Chromium `page.pdf()` yeteneği kullanıldı — `webapp-testing` altyapısı zaten Playwright kullanıyor, ek bağımlılık gerekmedi. `Sales/PrintInvoice.cshtml` (yeni) aynı deseni "Satış Faturası" için tekrarlıyor.
- **Kritik:** Kullanıcı netleştirdi — gerçek ERP faturaları/irsaliyeleri ileride mevcut `UploadDocument` akışıyla manuel yüklenecek; bu PDF'ler yalnızca alan boş kalmasın diye üretilen **şablon/placeholder**. Bu yüzden PDF üretimi kalıcı bir buton/özellik olarak UI'ya bağlanmadı, sadece bir kereye mahsus import scripti içinde kullanıldı.
- `SalesOrder.InvoiceDate` alanı yoktu, migration ile eklendi (`AddSalesOrderInvoiceDate`). `SaveOrder`/`GetOrder` ve `Sales/Index.cshtml` sipariş modalı güncellendi.
- Tarihsel veri import'u EF/controller katmanından değil **doğrudan T-SQL** ile yapıldı (`Invoke-Sqlcmd`) — bire bir tek seferlik, kod tabanına kalıcı script eklemeye gerek yoktu. İçe aktarım öncesi her WorkOrder için toplam miktar `ActualQuantity`'yi aşmıyor mu diye elle doğrulandı (DB'den okunan gerçek veriyle karşılaştırıldı, tam eşit çıktı — sınırda test senaryosu).
- **Global CSRF tuzağı:** Proje `AutoValidateAntiforgeryTokenAttribute` global filter kullanıyor (`Program.cs`) — Playwright'tan doğrudan `context.request.post()` ile form-encoded istek atmak 400 döner. Çözüm: herhangi bir sayfadan `input[name="__RequestVerificationToken"]` değerini oku, `RequestVerificationToken` header'ı olarak ekle (`_Layout.cshtml`'deki global fetch/jQuery monkey-patch'in yaptığı işi elle taklit et).
- **Test sırasında bulunan pre-existing veri durumu:** Customer Id=5 (ACORE DIŞ TİCARET) `IsFscActive=False` — Dispatch() FSC kontrolü bu müşteri için her zaman engelliyor (benim yeni stok kontrolümden bağımsız, önceden var olan davranış). Stok guard'ı izole test etmek için geçici test müşterisi oluşturulup silindi. Bu, kullanıcıya ayrıca bildirilecek gerçek veri sorunu — kod değişikliği kapsamı dışında bırakıldı.

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

## Fire (WasteWeight) stok bakiyesinden bağımsız kalem — tüketim değil (2026-06-29)

**Kural (kesinleşti):** Fire, tüketimin içinde DEĞİLDİR. İkisi bağımsız kayıp kalemdir.

```
Kalan bakiye = InitialWeight − ConsumedWeight − WasteWeight
StockMovement.Quantity = ConsumedWeight + WasteWeight
```

**Uygulanan değişiklikler:**

`ProductionController.SaveDetail`:
- Yeni kayıt: `serial.CurrentWeight -= consumed + fire`
- Düzenleme: `diff = (yeni consumed+fire) − (eski consumed+fire)`
- Silme: `CurrentWeight += consumed + fire` (tam iade)
- `WasteWeight > ConsumedWeight` validasyonu kaldırıldı (fire bağımsız)
- `StockMovement.Quantity = consumed + fire`

**Stok sayfalarında hesaplama — DB'ye güvenmeme kuralı:**
Eski kayıtlarda `StockMovements.Quantity` sadece `ConsumedWeight` içerir (fire eksik) ve `FscSerial.CurrentWeight` fire düşülmeden kaydedilmiş olabilir. Bu yüzden **hiçbir stok sayfası `StockMovements.ProductionConsumption` veya `FscSerial.CurrentWeight`'i doğrudan toplamaz** — bunun yerine `ProductionDetails`'tan yeniden hesaplar:

```csharp
// Her stok sayfasında uretim tuketim toplami:
var prodCons = await _context.ProductionDetails
    .Where(...)
    .GroupBy(d => d.FscSerial.Lot.ProductId)
    .Select(g => new { ProductId = g.Key, TotalKg = g.Sum(d => d.ConsumedWeight + d.WasteWeight) })
    .ToDictionaryAsync(...);

// Kalan (bobin/seri bazinda):
kalan = serial.InitialWeight
      - serialConsumed[serial.Id]   // ProductionDetails.ConsumedWeight toplami
      - serialFire[serial.Id]       // ProductionDetails.WasteWeight toplami
      - (conversionFireKg ?? 0m);   // Donusum firesi (YM icin)

// Net stok (urun bazinda):
net = inbound - salesOut - prodCons[productId];  // consumed+fire birlikte
```

**Uygulanan sayfalar:**
- `StockController.RawMaterial` → `ViewBag.SerialConsumed` + `ViewBag.SerialFire` + `ViewBag.TotalKg` yeniden hesaplandı
- `StockController.Summary` → `productionConsumptionByProduct` (PD'dan), `outboundKg = salesOut + prodConsumKg`
- `StockController.AdminStock` → aynı mantık
- `StockController.Index` → `indexProdConsByProduct`, `CikisAdet = sales + prodCons`
- `StockController.Movements` → `ViewBag.TotalProdConsumKg`, band/footer toplamları düzeltildi
- `ConversionController.Index` → YM KALAN = `InitialWeight - pdConsumed - pdFire - convFire`
- `RawMaterial.cshtml` → `tuketim`/`fire`/`kalan` değişkenleri, tfoot, data-kalan, data-status

**Veri düzeltme endpoint'leri (bir kez çalıştır):**
- `POST /Production/RecalcCurrentWeightFire` → `FscSerial.CurrentWeight = InitialWeight - consumed - fire`
- `POST /Production/RecalcStockMovementsFire` → `StockMovement.Quantity = consumed + fire`

**Kural: Yeni stok sayfası/özellik yazarken ASLA şunu yapma:**
```csharp
// YANLIŞ — eski kayitlarda fire eksik
.Sum(m => m.Type == MovementType.ProductionConsumption ? m.Quantity : 0)
// YANLIŞ — CurrentWeight fire duşülmemiş olabilir  
kalan = serial.CurrentWeight
```
Her zaman `ProductionDetails.Sum(consumed + fire)` kullan.

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

## Varsayılan filtre + "Tüm Kayıtları Göster" toggle (2026-06-21, genişletildi 2026-06-22)

**Amaç:** Kullanıcı filtre seçmediğinde sensible varsayılan yapı göster (örn. Hammadde+YM+BS), ama "Tüm Kayıtları Göster" seçeneğiyle full liste erişimini sağla.

**Yapı:**
- Controller: `bool showAll = false` parametresi + `hasUserFilter` bayrağı (filtre seçildi mi = supplierIds/productIds/stockCode/etc. boş değil)
- `hasUserFilter == false && showAll == false` → varsayılan GroupIds (ada göre: "HAMMADDE", "YARI MAMUL", "BURGU SAP") filtresiyle query çalışır
- `showAll == true` → filtreler yok, tüm kayıtlar
- ViewBag: `IsDefaultFilter` ve `ShowAll` boolean'ları view'de render kontrolü için

**View HTML:** İki info bander:
1. Varsayılan filtre aktif: "Varsayılan görünüm: Hammadde · Yarı Mamül · Burgu Sap grupları. [Tüm Kayıtları Göster]"
2. ShowAll aktif: "Tüm kayıtlar gösteriliyor. [Varsayılana Dön]"

**URL deseni:**
- `?` veya parametresiz → varsayılan filtre + bilgi bandı
- `?showAll=true` → tüm kayıtlar + ikinci bilgi bandı
- `?productIds=5&productIds=6` veya diğer filtreler → varsayılan geçersiz, seçilen filtreler uygulanır

**Kullanılan yerler:**
- `Purchase/Index` — varsayılan: HAMMADDE+YARI MAMUL+BURGU SAP (dinamik GroupName sorgusu)
- `Stock/RawMaterial` — varsayılan: HAMMADDE+YARI MAMUL+BURGU SAP (dinamik GroupName sorgusu)

**Not:** Grup adları **grup ID'sine göre değil**, `ProductGroup.GroupName.ToUpper()` sorgusuyla dinamik alınıyor. Böylece grup ID'leri değişse bile pattern çalışmaya devam ediyor.

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

## Toplam Fiziksel Stok kartı — Purchase + Stock/RawMaterial (2026-06-22)

**Amaç:** Purchase ve Stock/RawMaterial sayfalarında, "Kalan (kg)" kartı artık yalnızca satın alınan hamların kalanını değil, **dönüşümle oluşturulan YM lotlarını da dahil** eder (üretim tüketimine kadarki tüm Ham+YM+BS fiziksel stoğu).

**Yapı:**
- **Purchase/Index:**
  - Controller: `ViewBag.ToplamFizikselStok` — `FscSerials` tablosundan HAMMADDE+YARI MAMUL+BURGU SAP GroupName'li ürünlerin `CurrentWeight` toplamı
  - Query: `GroupName.ToUpper()` içinde "HAMMADDE", "YARI MAMUL", "BURGU SAP" var mı kontrolü
  - View: Kart etiketi `isDefaultFilter` true'ysa "Toplam Fiziksel Stok", false'ysa "Kalan (kg)"
  - Fallback: `toplamFizikselStok ?? kalanKg` — ViewBag'den gelmezse model toplamını kullan

- **Stock/RawMaterial:**
  - Controller: Zaten varsayılan filtresiyle HAMMADDE+YARI MAMUL+BURGU SAP gösteriliyor; bu sayfayı açmak = fiziksel stok görüntülemek

**Veri akışı:**
```csharp
// PurchaseController.Index
var defaultGrpNames = new[] { "HAMMADDE", "YARI MAMUL", "YARI MAMÜL", "BURGU SAP" };
ViewBag.ToplamFizikselStok = await _context.FscSerials
    .Include(s => s.Lot).ThenInclude(l => l.Product).ThenInclude(p => p!.ProductGroup)
    .Where(s => s.CurrentWeight > 0
        && s.Lot.Product != null
        && s.Lot.Product.ProductGroup != null
        && defaultGrpNames.Contains(s.Lot.Product.ProductGroup.GroupName.ToUpper()))
    .SumAsync(s => s.CurrentWeight);
```

**Not:** Grup adlarında typo (YARI MAMÜL vs YARI MAMUL) toleransı için her iki form dahil edildi (`ToUpper()` ile).

**Uygulandığı:** `PurchaseController.Index` (commit d94db7c), `StockController.RawMaterial` (commit c0c79bd).

## FSC Kütle Dengesi Partial (_FscStokOzeti.cshtml) — 4 sayfada ortak kart (2026-06-26)

**Amaç:** Purchase, Stock/Summary, Stock/RawMaterial, Stock/AdminStock sayfalarında ortak FSC stok kırılım kartını bir partial olarak merkezi yönetme.

**Yapı:**
- **Partial:** `Views/Shared/_FscStokOzeti.cshtml` — 6 ViewData değeri alır:
  - `FscliGiris`, `FscsizGiris` (giriş KG, FSC'li/siz ayrımı)
  - `FscliTuketim`, `FscsizTuketim` (tüketim KG)
  - `FscliKalan`, `FscsizKalan` (kalan stok KG)

**Rendering logic:**
```csharp
// Partial başında ViewData'dan çek, default 0
var fscliGiris = (decimal)(ViewData["FscliGiris"] ?? 0m);
var fscsizGiris = (decimal)(ViewData["FscsizGiris"] ?? 0m);
// ...

// Hesapla
var toplamGiris = fscliGiris + fscsizGiris;
var toplamTuketim = fscliTuketim + fscsizTuketim;
var toplamKalan = fscliKalan + fscsizKalan;
```

**UI tasarımı:**
- Kart: koyu gradient zemin (#0f172a → #1a2744), Bootstrap rounded-3
- Başlık: "FSC Kütle Dengesi" + "Ham · YM · Burgu Sap" badge'i
- 3 kolon × 3 satır (FSC'li, FSC'siz, Toplam) × 4 kolon (Kategori, Giriş, Tüketim, Kalan)
- Renk kodlaması:
  - FSC'li giriş: beyaz yazı, koyu arka plan
  - FSC'li tüketim: kırmızı (#fca5a5)
  - FSC'li kalan: yeşil (#86efac)
  - FSC'siz benzer, ama altın/sarı ton (#fbbf24)

**Çağrı pattern (4 sayfada ortak):**
```razor
@await Html.PartialAsync("_FscStokOzeti", null, 
    new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(ViewData))
```
ViewData'yı partial'e doğrudan aktar; null model (sadece ViewData kullanılır).

**Controller tarafında ViewData doldurma (örn. StockController.Summary):**
```csharp
public async Task<IActionResult> Summary(int[]? productGroupIds, bool showAll = false)
{
    // ... query ve filtreleme ...
    
    // FSC kırılım hesaplaması
    var fscliGirisKg = serials
        .Where(s => s.Lot.FscType != null && s.Lot.FscType.Name.Contains("FSC"))
        .SelectMany(s => _context.StockMovements
            .Where(sm => sm.DocumentNo == s.Lot.PartiNo && sm.MovementType == MovementType.PurchaseEntry))
        .Sum(sm => sm.QuantityKg ?? sm.Quantity);
    var fscsizGirisKg = serials
        .Where(s => s.Lot.FscType == null || !s.Lot.FscType.Name.Contains("FSC"))
        .SelectMany(s => _context.StockMovements
            .Where(sm => sm.DocumentNo == s.Lot.PartiNo && sm.MovementType == MovementType.PurchaseEntry))
        .Sum(sm => sm.QuantityKg ?? sm.Quantity);
    
    ViewData["FscliGiris"] = fscliGirisKg;
    ViewData["FscsizGiris"] = fscsizGirisKg;
    // ... benzer şekilde Tüketim ve Kalan ...
    
    return View(summary);
}
```

**Tuzak — Null kontrolü:**
- StockMovement.QuantityKg nullable olabilir (eski kayıtlar). Her zaman `sm.QuantityKg ?? sm.Quantity` fallback'i kullan.
- FscType null olabilir. Null'u "FSC'siz" olarak kategorize et.

**Uygulandığı:** `PurchaseController.Index`, `StockController.Summary`, `StockController.RawMaterial`, `StockController.AdminStock` (commit d32e8dc).

## Stock/RawMaterial action'a varsayılan grup filtresi + ShowAll (2026-06-26)

**Güncelleme:** RawMaterial action'ında varsayılan filtre eklendi (Purchase/Index'le tutarlı).

**Yapı:**
- Parametreler: `showAll = false` eklendi
- `hasUserFilter` bayrağı: filtre/showEmpty/showAll boş değil mi kontrol et
- Varsayılan filtre: "HAMMADDE", "YARI MAMUL", "BURGU SAP" grup adlarına göre (dynamik, ID değil)
- ViewBag.IsDefaultFilter: bilgi bandısı render control'ü için

**Kontrol istemeyen sayfalar (admin/debug):**
- `Stock/AdminStock` — varsayılan filtre YOK; her zaman tüm kayıtlar
- Bu sayfalar zaten partial kart ile FSC kırılımı gösteriyor

**Uygulandığı:** `StockController.RawMaterial` (commit c0c79bd).

## ViewData vs ViewBag farkı — Partial'lere ViewData aktarımı (2026-06-26)

**Durum:** Partial'lere ViewBag değerleri geçiş işe yaramıyor çünkü partial farklı ViewContext'te render edilir.

**Çözüm:** ViewData dictionary'yi `new ViewDataDictionary(ViewData)` ile explicit çoğaltıp partial'e ver:
```razor
@await Html.PartialAsync("_PartialName", null, 
    new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(ViewData))
```

Partial içinde ViewData["key"] erişimi çalışır; `ViewBag.key` ve `Model` partial için bağlam dışı kalabilir.

**Best practice:** Shared partial'lerde **ViewData** kullan; page partial'lerde **Model** veya **ViewBag** tercih edilebilir (ama küçük component'ler için ViewData daha güvenli).

**Uygulandığı:** `_FscStokOzeti.cshtml` (commit d32e8dc).

## FSC'siz Türkçe ToUpper() Hata Tespiti ve Düzeltme (2026-06-26) — KRİTİK

**Sorun:** "FSC'siz" stringinin FSC kategorisini tespitinde `ToUpper().Contains("SIZ")` pattern'i yanlış sonuç veriyordu.

**Kök neden:** Türkçe (tr-TR) kültüründe:
- `"fsc'siz".ToUpper()` → `"FSC'SİZ"` (doğru `i` yerine Latin `İ` karakteri kullanılır)
- `.Contains("SIZ")` → false (aranılan string `SIZ` ama bulunacak `SİZ` olduğu için)

SQL Server LOWER() fonksiyonunun aksine, C# `ToUpper()` kültüre bağlı dönüşüm yapar.

**Etkilenen sayfalar:** 
- `PurchaseController.Index` — kalan kg (kalanKg) hesaplaması
- `StockController.Summary` — tüm hesaplamalar
- `StockController.AdminStock` — tüm hesaplamalar
- `StockController.RawMaterial` — filtre ve hesaplamalar
- Toplamda 8 lokasyon

**Çözüm:** Türkçe kültürde güvenli olan `ToLower().Contains("siz")` pattern'ine değiştir:
```csharp
// YANLIŞ (Türkçe'de i → İ dönüşümü nedeniyle SIZ bulunamıyor)
if (isFsc == null && fscType?.ToUpper().Contains("SIZ") == true)
    kategorisi = "FSC'siz";

// DOĞRU (Türkçe'de s → s, i → i, z → z; lowercase'de karşılaştırma tutarlı)
if (isFsc == null && fscType?.ToLower().Contains("siz") == true)
    kategorisi = "FSC'siz";
```

**NOT:** Her iki pattern'de yeterince test edilmemiş olasılığı var. Bu çözüm (lowercase) pragmatik ve işlev yapıyor; ama SQL Server vs C# string işlemesi arasındaki kültür farkı dikkat gerektirir.

**Uygulandığı:** `PurchaseController.Index`, `StockController.cs` (4 action) (commit 992a37c).

## FSC Kütle Dengesi Formülü — Giriş/Tüketim/Kalan Kategorisizasyonu (2026-06-26) — SEMANTİK

**Eski (yanlış) mantık:**
- **Giriş:** Tüm scope'daki `FscSerials` InitialWeight toplamı → **YANLIŞ** çünkü dönüşümle oluşturulan YM lotları da sayılıyordu (double-count)
- **Tüketim:** `InitialWeight - CurrentWeight` → **YANLIŞ** çünkü hammadde→YM dönüşümü tüketim (üretim loss) gibi sayılıyordu (aslında stok dönüşümü)

**Yeni (doğru) mantık:**
- **Giriş:** `Lot.SourceSerialId IS NULL` olan serilerin `InitialWeight` toplamı (= sadece tedarikçiden satın alınan HAM + BURGU SAP)
  - Dönüşümle oluşturulan YM lotları (`SourceSerialId != NULL`) Giriş'e dahil edilmiyor
- **Kalan:** Tüm scope'daki (HAM + YM + BS) `CurrentWeight > 0` toplamı (üretim öncesi depolanan tüm stok)
- **Tüketim:** Türetilmiş = `Giriş - Kalan` (hammadde→YM dönüşümü YM olarak Kalan'da kaldığı için Tüketim'e girmiyor)

**Sonuç:** Hammadde girişi ve dönüşüm yoluyla oluşan YM'nin toplamı fiziksel stokta Kalan olarak kalır. Tüketim sadece gerçek üretim/satış çıkışıdır.

**StockMovement.MovementType kategorileri:**
- `PurchaseEntry (1)` = Tedarikçiden hammadde girişi → Giriş
- `ProductionEntry (1)` = Dönüşümden YM girişi → Giriş (alternatif sayılan)
- `ProductionConsumption (5)` = Üretimde tüketim (bobin ağırlık azalışı) → Tüketim
- `SalesDispatch (3)` = Müşteriye çıkış → Tüketim

**NOT:** "Tüketim" tanımı geniş: hem üretim sürtme/fire hem de satış sevkiyatı. Net stok = Giriş − Çıkış.

**Uygulandığı:** `PurchaseController.Index`, `StockController.Summary/AdminStock/RawMaterial` (commit ba9c725).

## ToplamFizikselStok ViewBag Fallback Hata (2026-06-26)

**Sorun:** Purchase/Index sayfasında "Toplam Fiziksel Stok" kartı yanlış değer gösteriyordu.

**Sebep:** ViewBag.ToplamFizikselStok set edilmişti ama view'de fallback'e düşüyordu:
```razor
<!-- YANLIŞ: fallback toplamı sadece Model (default filtre) lotlarını sayıyor -->
@{
    var toplamFizikselStok = ViewBag.ToplamFizikselStok ?? kalanKg;
}
```
ViewBag null gelince `kalanKg` (Model'den hesaplanan, varsayılan grup filtresi) kullanılıyordu. Ama ViewBag'den gelen controller value 218,872 kg iken, fallback 200,858 kg (eksik) gösteriyordu.

**Çözüm:** ViewBag yerine ViewData kullanıp ve partial'e doğru aktarım:
```razor
@{
    // Partial'den gelen ViewData toplam değerini kullan
    var fscliKalan = (decimal)(ViewData["FscliKalan"] ?? 0m);
    var fscsizKalan = (decimal)(ViewData["FscsizKalan"] ?? 0m);
    var toplamFizikselStok = fscliKalan + fscsizKalan;
}
```

**İlgili:** _FscStokOzeti.cshtml partial — FSC'li/siz stok kartlarını doldurur ve ViewData["FscliKalan"], ViewData["FscsizKalan"] set eder.

**Uygulandığı:** `PurchaseController.Index` (ViewData set), `Purchase/Index.cshtml` (fallback logic) (commit aa31c24).

## FSC Bakiye Kartları Partial — 3 büyük statü kartı (_FscStokOzeti.cshtml) (2026-06-26)

**Amaç:** Purchase, Stock/Summary, Stock/RawMaterial, Stock/AdminStock sayfalarında ortak **FSC'li Bakiye / FSC'siz Bakiye / Toplam Bakiye** kartlarını merkezi yönetme.

**Bileşenler:**
1. **FSC'li Bakiye kartı** (yeşil arka plan, #10b981)
   - Başlık: "FSC'li Bakiye"
   - Büyük sayı: `FscliKalan` kg
   - Alt metin: "FSC sertifikalı stok"

2. **FSC'siz Bakiye kartı** (sarı/altın arka plan, #f59e0b)
   - Başlık: "FSC'siz Bakiye"
   - Büyük sayı: `FscsizKalan` kg
   - Alt metin: "Sertifikat olmayan stok"

3. **Toplam Bakiye kartı** (mavi arka plan, #3b82f6)
   - Başlık: "Toplam Bakiye"
   - Büyük sayı: `FscliKalan + FscsizKalan` kg
   - Alt metin: "Tüm fiziksel stok"

**Tasarım:**
- Kart: Bootstrap rounded-3, gölge (box-shadow), padding 20px
- Sayı: 36px font-weight-bold, beyaz yazı
- Label: 14px semibold, rgba(255,255,255,.85)
- İkon: 24px ikon (fa-check / fa-times / fa-boxes)

**ViewData parametreleri (controller'dan set edilecek):**
- `ViewData["FscliGiris"]`, `ViewData["FscsizGiris"]` — giriş KG (şu an kullanılmıyor kartlarda, ileride FSC kırılım raporu için)
- `ViewData["FscliKalan"]`, `ViewData["FscsizKalan"]` — **ana değerler** (kartlarda gösterilen)
- `ViewData["FscliTuketim"]`, `ViewData["FscsizTuketim"]` (şu an kullanılmıyor)

**Çağrı (4 sayfada ortak):**
```razor
@await Html.PartialAsync("_FscStokOzeti", null, 
    new ViewDataDictionary(ViewData))
```

**NOT:** Partial içinde Razor hesaplama yapılıyor; ViewData'dan gelen raw sayılar kullanılır.

**Uygulandığı:** `Views/Shared/_FscStokOzeti.cshtml` (commit 992a37c, d32e8dc).

## StockMovement.Type vs MovementType — Naming (2026-06-26)

**Dikkat:** Bazı yerlerde `StockMovement.Type` (property), diğerlerinde `MovementType.ProductionEntry` (enum) kullanılıyor.

**Doğru isim:** `StockMovement.MovementType` (property name — nullable değil değilse NOT NULL entity'de)

**Yanlış kullanım (legacy):** `sm.Type` → `sm.MovementType` olarak düzeltildi (commit ba9c725).

**Enum tanımı (StockMovement.cs):**
```csharp
public int MovementType { get; set; }  // 1,2,3,4,5 değerleri
```

**Uygulandığı:** `StockController.cs` (commit ba9c725).

## FSC Kütle Dengesi — Merkezi Servis (FscMassBalanceService) (2026-06-27)

**Sorun:** Her controller kendi girisOzet/kalanOzet sorgusunu yazıyordu. Filtre kaymaları (eksik `!PartiNo.StartsWith("YM")`, eksik `DispatchNo/InvoiceNo` kontrolü) farklı sayfalarda farklı rakam vermesine yol açıyordu. Günlerce aynı tutarsızlığı düzeltme döngüsüne girdi.

**Kök neden:** Duplike sorgu → her birini ayrı güncelleme gereksinimi → bir yerde düzeltilince diğerinde kalan bug.

**Çözüm:** `FscMassBalanceService.cs` (Services klasörü) — TEK merkezi hesap:
```csharp
// Tüm controller'larda sadece 1 satır:
(await FscMassBalanceService.ComputeAsync(_context)).ApplyToViewData(ViewData);
```

**Kural — FSC Kütle Dengesi tanımları (bir daha değişmez):**
- **Giriş** = `Lot.SourceSerialId == null` + `!PartiNo.StartsWith("YM")` + `(DispatchNo != null || InvoiceNo != null || IsOpeningStock)` → `Sum(InitialWeight)`
- **Kalan** = Scope'taki tüm seriler (satın alma + dönüşüm YM) `CurrentWeight > 0` → `Sum(CurrentWeight)`
- **Tüketim** = Giriş − Kalan (türetilmiş, computed property; dönüşüm YM Kalan'da olduğundan tüketim = üretim tüketimi + dönüşüm firesi)
- **Kapsam** = HAMMADDE + YARI MAMUL + YARI MAMÜL + BURGU SAP (GroupName.ToUpper() ile match)
- **FSC'siz tespiti** = `FscType.Name.ToLower().Contains("siz")` (ToUpper + Turkish İ bug'ından kaçınmak için)

**Eklenecek yeni sayfa için:** Sadece `FscMassBalanceService.ComputeAsync` çağır. Sorgu yazmak yasak.

**Uygulandığı:** commit `a88c6cf` (refactor).

## Performans Mimarisi — Ne Zaman Ne Yapılır (2026-06-30)

**Bağlam:** AnaOzet, RawMaterial, Detail sayfaları EF Core Include zinciriyle runtime hesaplıyor.
Veri şu an küçük (< 3.000 satır) — doğru. Yanlış mimari erkenden kurulursa bakım yükü artar.

**Eşik tabanlı karar ağacı:**

| Eşik | Belirti | Aksiyon |
|------|---------|---------|
| Hemen (ŞIMDI) | — | 4 kritik index ekle (bkz. docs/PERFORMANCE_ROADMAP.md) |
| Lot > 500 VEYA sorgu > 500ms | AnaOzet / RawMaterial yavaşlıyor | SQL View `vw_StockSummary` devreye al |
| Lot > 2.000 VEYA sorgu > 2s | Dashboard her açılışta gecikmeli | SQL View'i Indexed View'e çevir |
| Lot > 10.000 VEYA çok kullanıcı | Concurrent request spike | Nightly job → `StockSummaryCache` staging tablo |

**Hazır SQL:** `docs/sql/performance_indexes.sql` + `docs/sql/vw_stock_summary.sql`
Sıra geldiğinde bu dosyaları SSMS'te çalıştır, EF Core mapping ekle.

**Altın kural:** Önce ölç, sonra optimize et.
`SET STATISTICS TIME ON` ile sorgu süresini gör. 500ms geçmedikçe dokunma.

## Kritik Gizli Bug — SaveChangesAsync Tüm String'leri Körü Körüne Büyütüyordu (2026-07-05)

**Sorun:** `AppDbContext.SaveChangesAsync()` override'ı, kaydedilen HER entity'nin HER string alanını istisnasız `ToUpper(trCulture)` ile büyütüyordu. `AppUser.PasswordHash` da dahildi.

**Neden fark edilmedi:** DB collation `Turkish_CI_AS` (case-insensitive) olduğu için SQL tarafındaki karşılaştırmalar (`u.PasswordHash == hash` login sorgusu) çalışmaya devam ediyordu — login hiç bozulmadı. Ama `ChangePassword` action'ındaki C# tarafı karşılaştırma (`user.PasswordHash != HashPassword(currentPassword)`) case-sensitive olduğundan sessizce başarısız oluyordu. Sadece "gerçek kullanıcı gerçek sırayla" (giriş yap → sonra şifre değiştir) test edilince ortaya çıktı — dress rehearsal / gerçek kurulum testi olmadan asla yakalanamazdı.

**Çözüm:** `_skipUppercaseProps` HashSet eklendi (`PasswordHash`, `Email`, `InvoicePdfPath`, `DispatchPdfPath`, `LogoPath`, `FilePath`). Mevcut bozulmuş kayıtlar geri döndürülebilir: `UPDATE AppUsers SET PasswordHash = LOWER(PasswordHash);` (hex hash için güvenli, veri kaybı yok).

**Kural:** Yeni bir entity'ye hash/token/path/base64/teknik-string alanı eklerken `_skipUppercaseProps`'a ekle. "Görünüşte çalışıyor" (login) ≠ "gerçekten doğru" — case-insensitive collation bug'ları maskeleyebilir, C# tarafı karşılaştırmalar açığa çıkarır.

**Ders:** Otomatik kod incelemesi (agent review) genelde "bu PR'da ne değişti" bakar; global interceptor'lar gibi yıllar önce yazılmış, o an dokunulmayan kodları taramaz. Bu tür sessiz bug'lar ancak gerçek uçtan uca kullanım senaryosuyla (dress rehearsal) yakalanır.

## Kurulum Provası — Ek Dersler (2026-07-05, oğlun PC'si dress rehearsal)

- **Windows Firewall:** IIS site "Tümü Atanmamış" IP'ye bağlı olsa bile, Windows Firewall'da o port için inbound kural yoksa ağdaki başka makineler erişemez (localhost/RDP-aynı-makine testi bunu YAKALAMAZ). Kural: `New-NetFirewallRule -DisplayName "..." -Direction Inbound -Protocol TCP -LocalPort <port> -Action Allow`. Runbook'a kalıcı madde olarak eklendi.
- **Tarayıcı adres çubuğu:** Kullanıcılar bare IP yazınca (`192.168.0.55`) tarayıcı otomatik `https://` (443) dener, `ERR_SSL_PROTOCOL_ERROR` verir. Port + şema açıkça yazılmalı: `http://<ip>:<port>`. Kullanım kılavuzuna not düşülmeli.
- **IIS named instance (SQLEXPRESS):** appsettings.json'da `Server=localhost` YETMEZ, `Server=localhost\SQLEXPRESS` gerekir (named instance). appsettings.json'da JSON escape: tek backslash geçersiz, `\\SQLEXPRESS` (çift) yazılmalı.
- **IIS APPPOOL SQL login:** Yeni bir IIS sitesi ilk kurulduğunda `IIS APPPOOL\<SiteAdi>` için SQL Server'da login+user+db_owner YOKTUR — SQL Error 4060 ("Cannot open database") verir. Runbook'a migration sonrası zorunlu adım: `CREATE LOGIN [IIS APPPOOL\<Site>] FROM WINDOWS; USE <Db>; CREATE USER [IIS APPPOOL\<Site>] FOR LOGIN [IIS APPPOOL\<Site>]; ALTER ROLE db_owner ADD MEMBER [IIS APPPOOL\<Site>];`
- **Hosting Bundle + IIS sırası:** IIS sonradan etkinleştirilirse, .NET Hosting Bundle kurulumunun **Onar (Repair)** ile tekrar çalıştırılması gerekir (aksi halde ASP.NET Core IIS modülü kayıt olmaz, HTTP 500.30 verir).
- **stdout log:** 500.30 hatasında `web.config`'te `stdoutLogEnabled="true"` yapıp `logs\` klasörü oluşturmadan gerçek hata görünmez.
- **Güncelleme dağıtımı (canlıya patch):** `dotnet publish` çıktısından appsettings.json/license.lic HARİÇ her şey kopyalanır (üzerine yaz), sonra app pool restart. RDP üzerinden büyük dosya (zip) taşımak için: normal RDP panosu (Ctrl+C/Ctrl+V) dosya kopyalamayı destekler — küçük metin dosyaları için de aynı pano yeterli, ekstra araç gerekmez.

---

## Tek tık kurulum paketi — FscErpSetup.exe (2026-07-16)

Runbook FAZ 1–4 (~1 gün saha işi) tek EXE'ye indirildi. `installer/` altında üç parça:
`build-installer.ps1` (ARD makinesinde paket üretir), `FscErpSetup.iss` (Inno sihirbazı),
`install-engine.ps1` (müşteri sunucusunda asıl iş). Tasarım ilkesi: **karmaşıklık ARD
tarafında, sahada sıfır sürpriz** — ön koşullar derleme sırasında indirilip doğrulanır,
müşteri sunucusunun internete çıkması gerekmez.

- **PowerShell 5.1 + Türkçe = UTF-8 BOM ZORUNLU.** BOM'suz kaydedilen `.ps1`'i PS 5.1
  ANSI sanar; `Sık` → `SÄ±k` olur ve script **parse bile edilemez** ("Missing closing '}'"
  gibi alakasız hatalar verir, gerçek sebep encoding'dir). Write tool BOM yazmaz — script
  yazdıktan sonra BOM'u elle ekle:
  ```powershell
  $t = [IO.File]::ReadAllText($p, (New-Object Text.UTF8Encoding($false)))
  [IO.File]::WriteAllText($p, $t, (New-Object Text.UTF8Encoding($true)))
  ```
  Doğrulama: `[Parser]::ParseFile($p, [ref]$null, [ref]$errors)` — hata listesi boş olmalı.
- **`$args` PowerShell'de ayrılmış otomatik değişken** — fonksiyon içinde `$args = @(...)`
  yazınca parser "Missing argument in parameter list" der. `$setupArgs` gibi adlandır.
- **Gizli sızıntı: `appsettings.json` gitignored AMA publish'e girer.** Dosya geliştirme
  makinesinde diskte durur, `dotnet publish` onu çıktıya kopyalar — ARD'nin kendi bağlantı
  dizesi (`Server=ARDA\ARDA`) müşteri paketine sızar. gitignore bunu engellemez; publish
  çıktısını ayrıca denetle. `build-installer.ps1` → `Test-NoSecretLeak` bunu yakalayıp
  siler, `*.lic` veya `*.pem` bulursa derlemeyi durdurur.
- **Uydurma SHA256 yazma.** Ön koşul hash'i bilinmiyorsa boş bırak ve script'i durdur;
  tahmini hash doğrulama varmış görüntüsü verip hiçbir şey doğrulamaz. Desen: `Sha256 = ''`
  → indir, hesapla, ekrana bas, DUR, insan onayıyla sabitle (trust-on-first-use).
- **Adım sırası keyfi değil, hepsi gerçek hatadan geliyor:**
  IIS → Hosting Bundle (ters sıra HTTP 500.30) ·
  AppPool → SQL login (login, AppPool hesabı var olmadan açılamaz) ·
  CREATE DATABASE (COLLATE Turkish_CI_AS) → Migrate (collation sonradan değiştirilemez;
  `Migrate()` DB'yi kendi yaratırsa sunucu varsayılan collation'ını alır).
- **`Get-MachineKey` iki yerde yaşıyor** — PS motoru + C# `LicenseService`. Aynı kodu
  üretmek ZORUNDA (SHA256(MachineGuid) ilk 16 hex, küçük harf), yoksa kurulum raporundaki
  kimlik kodu yanlış lisans üretilmesine yol açar. Repodaki dev lisansının makinesiyle
  (`b6c87ee3c2563e19`) karşılaştırarak doğrulandı.
- **Robocopy `/MIR` + korunacak dosya:** güncellemede `appsettings.json`/`license.lic`
  ezilmemeli → `/XF` ile hariç tut. `/MIR` hedefte olup kaynakta olmayanı siler; `logs/`
  klasörünü `/XD` ile koru. Çıkış kodu 0-7 başarı, **8+ hata** (0 değil!).
- **`ConvertTo-Json` ters slash'ı kendi kaçırır** — `localhost\FSCERP` otomatik olarak
  `localhost\FSCERP` yazılır. appsettings.json'ı elle string olarak kurmak yerine
  PSCustomObject + ConvertTo-Json kullan; named instance escape hatası tarihe karışır.
  Ama **UTF-8 olarak yaz**: PS 5.1 varsayılanı UTF-16'dır, ASP.NET Core okuyamaz →
  `[IO.File]::WriteAllText($p, $json, (New-Object Text.UTF8Encoding($false)))`.
- **Production'da `UseHttpsRedirection` intranet kurulumunda mayın.** IIS'te https binding
  yokken sessizce no-op'a düşüyor (bu yüzden bugüne dek fark edilmedi), ama müşteri kötü
  bir sertifikayla binding eklerse runbook'taki `ERR_SSL_PROTOCOL_ERROR` geri gelir.
  `Security:HttpsRedirection` bayrağına alındı; kurulum `false` yazıyor.
- **30 gün deneme — sıfırlanmaya karşı iki kaynak:** `C:\ProgramData\ArdFscErp\.init` +
  veritabanının `create_date`'i, **erken olanı** geçerli. Yeni tablo/migration gerekmedi.
  DB'yi drop etmek denemeyi sıfırlar ama müşterinin tüm ERP verisini yok eder — pratik
  caydırıcılık bu. Mutlak koruma değil, bilinçli sınır.
  Lisans dosyası VARSA (bozuk/süresi dolmuş olsa bile) denemeye geri düşülmez — yoksa
  süresi dolmuş lisansı silmek 30 gün daha kazandırırdı.
- **Enum'a değer eklemenin bedeli:** `LicenseState.Trial` → `LicenseFilter` (`!= Valid`
  yerine `IsUsable`), `Status.cshtml` switch, `_Layout` bandı, `ReportsController`'daki
  30-gün-kala uyarısı (Trial'da ValidUntil hep dolu → her gün çift uyarı verirdi, `Valid`
  durumuna kısıtlandı). Çapraz-etki tablosu bunu yakaladı.

### KRİTİK — Müşteri belgeleri pakete sızıyordu (2026-07-17)

İlk ISCC derlemesinin çıktısında görüldü: `wwwroot/uploads/` altındaki **197 dosya /
55 MB** kurulum EXE'sine giriyordu. Bunlar test verisi değildi — gerçek bir kraft torba
üreticisinin FSC denetim arşiviydi: müşteri listesi (Harrods, Domino's, Hunkemöller),
tedarikçi listesi, FSC Agreement'ları, organizasyon şeması, düzeltici faaliyetleri.
Paket her müşteriye aynı gittiği için, bir firmanın gizli arşivi **rakiplerine** gidecekti.

**Kök neden:** `Microsoft.NET.Sdk.Web` varsayılan olarak `wwwroot/**` altındaki her şeyi
yayına dahil eder. **`.gitignore`'un publish'e hiçbir etkisi yoktur.** Klasör gitignored
olduğu için repoda görünmüyordu (`git ls-files` → 0), bu yüzden fark edilmedi — ama
diskte duruyordu ve `dotnet publish` onu kopyaladı.

**Bu aynı sınıf hatanın ÜÇÜNCÜ tekrarı:**
1. `appsettings.json` — gitignored, diskte var, publish kopyaladı (ARD bağlantı dizesi)
2. `license.lic` — gitignored, publish kopyalayabilir
3. `wwwroot/uploads/` — gitignored, publish kopyaladı (müşteri belgeleri)

**Kural: "gitignored" ≠ "pakete girmez".** İki ayrı mekanizma. Repoda görünmeyen bir şey
pakette rahatça bulunabilir — ve gözden kaçması tam da bu yüzden kolaydır.

**Çözüm iki katmanlı:**
- Kök: `.csproj` → `<Content Remove="wwwroot\uploads\**" />` +
  `<None Include="..." CopyToPublishDirectory="Never" />`
- Savunma: `build-installer.ps1 / Test-NoSecretLeak` publish çıktısında hâlâ varsa
  siler ve uyarır (csproj değişirse veya publish önbelleği bayatlarsa yakalar).

**Kanıt:** 830 dosya/135.7 MB → 634 dosya/82 MB, PDF sayısı 197 → **0**.

**Ders:** Paketi ilk kez üretirken **içindekileri listele**. ISCC'nin "Compressing: ..."
çıktısı bunu bedavaya verdi. Yeni bir dağıtım artefaktı üretirken "ne giriyor?" sorusu
tahminle değil, dosya listesiyle cevaplanır.

### PowerShell fonksiyonundan native exe çağrısı dönüş değerini kirletir (2026-07-17)

`build-installer.ps1 / Invoke-Compile` son satırda `$exe` (EXE yolu) dönüyordu. Ama
ISCC çağrısı `& $iscc ...` **redirect edilmeden** yazıldığı için ISCC'nin tüm stdout'u
(aralarındaki BOŞ satırlar dahil) fonksiyonun **çıktı akışına** karıştı. Sonuç:
`$exe = Invoke-Compile` tek path yerine `[ISCC çıktı satırları..., path]` DİZİSİ oldu;
sonraki `Get-FileHash $exe` dizideki ilk boş string'e takıldı →
"Cannot bind argument to parameter 'Path' because it is an empty string."

Kafa karıştıran taraf: hata `Get-FileHash` satırında (350) patlıyordu ama kök neden
ISCC çağrısındaydı (329). Elle çalıştırınca ISCC kusursuz derliyordu — çünkü elle
çağrıda dönüş değeri yakalanmıyordu. Teşhis: `$_.ScriptStackTrace` gerçek satırı verdi
(InvocationInfo.Line yanıltıcıydı, top-level çağrıyı gösteriyordu).

**Kural:** PowerShell fonksiyonu bir değer döndürecekse, içindeki native exe / cmdlet
çıktısını **akıştan ayır**: `& $exe args | Out-Host` (veya `| Out-Null`, `2>&1 | ...`).
Fonksiyonun output stream'ine yalnız `return` edilen değer düşmeli. Aksi halde çağıran
`$x = Fn` beklenmedik dizi yakalar. Bu, C#'taki tek return'e alışkınlığın PowerShell'de
bıraktığı klasik tuzak — PS'de "yazılan her şey" dönüş değeridir.

### Güvenlik denetimi (3 paralel Opus uzmanı) + düzeltmeler (2026-07-17)

Müşteriye satış öncesi security-reviewer + database-reviewer + razor-blazor-reviewer paralel
salt-okunur denetim. FAZ 2 yeni kodu (marka/login/belge/hata sayfaları) TEMİZ çıktı; bulgular
önceden var olan ERP sorunlarıydı. Düzeltilenler:

- **Parola hash SHA256+sabit salt → PBKDF2** (AccountController). Kullanıcıya özel rastgele tuz,
  120k iterasyon, format `pbkdf2$iter$salt$hash`. Geriye uyum: `VerifyPassword` eski SHA256'yı da
  tanır, girişte PBKDF2'ye yükseltir (`IsLegacyHash`). **Kritik: PBKDF2 tuzu kullanıcıya özel
  olduğu için Login'deki `WHERE PasswordHash == hash` SQL karşılaştırması ÇALIŞMAZ** — kullanıcıyı
  username ile çekip `VerifyPassword` ile KODDA doğrula.
- **ETL düz metin DB parolası + yetki boşluğu** — en düşük yetkili kullanıcı `GetConnection` ile
  Netsis üretim parolasını okuyordu. Çözüm: `OnActionExecuting` override ile tüm EtlController
  admin-only + `GetConnection`'da parola maskeleme + kaydetmede maske→eski parola koru.
- **`BaseController.GeneralToggleStatus` yetkisiz + serbest tableName** — reflection ile herhangi
  Core entity pasifleştirilebiliyordu (orphan endpoint, UI kullanmıyor ama routable). Admin guard
  + entity whitelist.
- **XSS: `_Layout` kritik popup `innerHTML`** — tedarikçi/müşteri adından stored XSS. DOM node +
  `textContent`'e çevrildi. **`SaveChangesAsync` uppercase davranışı XSS savunması DEĞİL** —
  yapısal HTML enjeksiyonunu durdurmaz.
- **XSS: `Html.Raw` + inline `onclick` + elle tırnak kaçırma** (Sales/Index, MaterialTrace,
  BomAnalysis). `.Replace("'","\'")` C#'ta NO-OP (çift ters-slash gerekli). Kök çözüm: veriyi
  `data-*` attribute'e bas (Razor auto-encode), JS'te `this.dataset.x` oku — inline string
  interpolasyonu yapma.
- **DocumentController `[AllowAnonymous]`** — finansal PDF'ler anonim. Kaldırıldı (GUID gizliliği
  tek savunma yetmez).
- **Çok adımlı yazmalar transaction'sız** — SaveSerial/SaveConsumption/Convert'te 2-3 ayrı
  `SaveChangesAsync`; biri patlarsa defterler tutarsız. `BeginTransactionAsync` + `CommitAsync`
  ile atomik yapıldı. Ham `ex.Message` → `Serilog.Log.Error` + genel kullanıcı mesajı.
- Session cookie: `SameSite=Lax` + `SecurePolicy=SameAsRequest`.

**Bilinçli ERTELENEN (tek-firma/düşük-eşzamanlılık için kabul; sonraki sürüm):**
- FscSerial `rowversion` concurrency token — aynı bobini iki kullanıcı AYNI ANDA tüketirse
  kayıp-güncelleme. Tek firmada düşük olasılık.
- decimal ölçek (18,2)↔(18,4) hizalama (ağırlık kolonları).
- Login hız sınırı / hesap kilidi + daha güçlü parola politikası (min 8 yapıldı, karmaşıklık yok).
- Session-ID yenileme (fixation).
- Dağınık `ex.Message` (liste/ETL satır hataları) — ETL artık admin-only, düşük risk.

### Temiz VM testi bug'ı: D: = DVD sürücüsü tuzağı (2026-07-18)

İlk temiz-VM kurulum testinde EXE ilk adımda patladı:
`New-Item : Access to the path 'FscErpData' is denied` → `D:\FscErpData`.

**Kök neden:** `install-engine.ps1 / Resolve-DataPath` "D: sürücüsü varsa veri klasörünü
oraya koy" diyordu ve kontrolü `Get-PSDrive -Name D -PSProvider FileSystem` ile yapıyordu.
Ama ISO'dan kurulan VM'de D: = **DVD sürücüsü** (takılı Windows ISO'su). `Get-PSDrive`
DVD'yi de FileSystem sürücüsü sayar → D: seçildi → salt-okunur optik sürücüye yazma =
Access Denied. Geliştirici makinesinde D: gerçek sabit diskti, bu yüzden hiç görülmedi.

**Çözüm:** Yalnız DriveType=3 (yerel SABİT disk) kabul et:
`Get-CimInstance Win32_LogicalDisk -Filter "DeviceID='D:' AND DriveType=3"`.
DriveType: 2=removable, 3=fixed, 4=network, 5=DVD/CD.

**Ders:** Sürücü/ortam varsayımları geliştirici makinesine göre yazılır, temiz hedefte
kırılır. "D: var mı" değil "D: yazılabilir SABİT disk mi" sorulmalı. Ve bu tam olarak
temiz-VM testinin yakalaması gereken sınıf — dev makinede asla görünmez.

### Installer'da 4 katmanlı gizli bug zinciri — SQL hiç bağlanamıyordu (2026-07-18)

Temiz VM testinde installer ADIM 3'te ("SQL Server Express kuruluyor" / "Mevcut SQL
örneği kullanılıyor") sürekli "bağlantı kurulamadı" hatasıyla düşüyordu. Servisler
(`MSSQL$FSCERP`, `SQLBrowser`) `Running`, registry doğru, SQL setup log'unda hata yok —
görünürde HER ŞEY sağlıklıydı. Gerçek neden 4 katmana gömülüydü, sırayla açığa çıktı:

**Katman 1 — Kök neden: `$Db` parametresi `-Debug`'ın gizli takma adıyla çakışıyordu.**
`Invoke-Sql` fonksiyonunda `[string] $Db = 'master'` parametresi vardı.
`[Parameter(Mandatory)]` içeren HER fonksiyon örtük olarak "advanced function" sayılır
ve PowerShell'in ortak parametrelerini (`-Verbose`, `-Debug`, `-ErrorAction`...) otomatik
kazanır. `-Debug`'ın **belgelenmemiş gizli takma adı `-db`**'dir — case-insensitive
eşleşme yüzünden kendi `$Db` parametremiz bu takma adla çakışıyor ve fonksiyon **HER
ÇAĞRIDA** (açıkça `-Db` verilmese bile, sırf parametre bildirimi var diye) şu hatayı
fırlatıyordu: *"The parameter 'Db' cannot be specified because it conflicts with the
parameter alias of the same name for parameter 'Debug'."* Yani `Install-SqlExpress`,
`New-FscDatabase`, `Grant-AppPoolSqlAccess` içindeki HER `Invoke-Sql` çağrısı baştan
beri bozuktu. **Çözüm:** parametre adı `$DbName` yapıldı (`-Debug` ile hiçbir ortak
harf öbeği paylaşmıyor).

**Katman 2 — `Test-SqlConnection` gerçek hatayı yutuyordu.** `try { ... } catch {
return $false }` — Katman 1'in ürettiği gerçek .NET exception mesajı hiçbir yere
yazılmıyor, sadece `$false`'a indirgeniyordu. Bu yüzden saatlerce "SQL bağlanmıyor"
diye servis durumu, registry, setup log'u kontrol edildi ama asıl mesaj hiç
görünmedi. **Çözüm:** `catch` bloğunda `$script:LastSqlError = $_.Exception.Message`
saklanıp retry döngülerinin `throw` mesajına eklendi. **Ders: bir fonksiyon `catch`
ile hatayı `$true`/`$false`'a indirgiyorsa, orijinal exception mesajını bir yerde
(script-scope değişken, log) MUTLAKA sakla — aksi halde teşhis kör uçuşa döner.**

**Katman 3 — Sıralama hatası: `Set-IisSite`, `Set-AppFiles`'tan ÖNCE çalışıyordu.**
Katman 1-2 düzeltilip SQL adımı geçilince ortaya çıktı (önceki denemeler hiç bu
noktaya ulaşmamıştı). `New-Website -PhysicalPath $InstallPath` var olmayan bir
klasörü işaret ediyordu çünkü `$InstallPath`'i ilk oluşturan `Set-AppFiles` ana
akışta `Set-IisSite`'tan SONRA çağrılıyordu. **Çözüm:** `Set-AppFiles` +
`Set-AppSettings` çifti `Set-IisSite`'tan önceye alındı. **Ders: bir script'in adım
sırası ancak o adıma gerçekten ULAŞILDIĞINDA test edilmiş sayılır — erken bir adımda
sürekli patlayan bir script'in sonraki adımları "test edilmemiş" kabul edilmeli.**

**Katman 4 — `Invoke-WebRequest -MaximumRedirection 0` Windows PowerShell 5.1 hatası.**
Katman 3 düzeltilip site kurulunca ortaya çıktı. Site canlıydı, ama doğrulama isteği
"Operation is not valid due to the current state of the object" fırlatıyordu — sitenin
durumuyla ilgisi yoktu, `-MaximumRedirection 0` parametresinin kendisi .NET Framework
tabanlı `Invoke-WebRequest`'te (Windows PowerShell 5.1, PowerShell Core değil) bilinen
bir hataydı. **Çözüm:** `-MaximumRedirection 0` kaldırıldı, yönlendirmeler serbestçe
takip edilsin diye bırakıldı; catch bloğundaki durum kodu kontrolü `2xx-4xx` aralığına
genişletildi (3xx artık zaten otomatik takip edildiği için nadiren görülür ama
defansif olarak kalsın).

**Genel ders — katmanlı bug'lar sırayla açığa çıkar, hepsini önceden tahmin edemezsin.**
Her katman bir öncekini düzeltmeden GÖRÜNMÜYORDU (script hep aynı erken noktada
patlıyordu). "Bu düzeltme işe yaramadı, demek ki teşhis yanlıştı" sonucuna varmak
yanlıştı — her düzeltme gerçekten bir katmanı çözüyordu, sadece ARKASINDA bir katman
daha vardı. Doğru tepki: her düzeltmeden sonra yeniden dene, YENİ ve FARKLI bir hata
mesajı çıkıyorsa (aynısı değil) ilerleme var demektir, vazgeçme. Ayrıca: **VM testi
olmadan bu 4 bug'ın hiçbiri bulunamazdı** — dev makinesinde SQL zaten kurulu/sağlıklı
olduğu için `Install-SqlExpress`/`Resolve-SqlInstance` yolları hiç gerçek anlamda
egzersiz edilmemişti.

### 5. bug — Antiforgery çerezi ayrı konfigüre edilmemişti, IP+HTTP'de 400 (2026-07-18)

Kurulum tamamlanıp uygulama açıldıktan sonra Şifre Değiştir formu `HTTP ERROR 400`
verdi. Sebep: `AddSession`'da Session çerezi `SameSite=Lax` + `SecurePolicy=
SameAsRequest` ile intranet (HTTP + IP adresi, HTTPS/DNS değil) için açıkça
ayarlanmıştı, ama `AutoValidateAntiforgeryTokenAttribute` global filtresinin
kullandığı Antiforgery çerezi **hiç konfigüre edilmemişti** — kendi ayrı
`CookieBuilder`'ı var, Session'ınkini miras almaz. Framework varsayılanı bu
senaryoda token round-trip'ini kırdı → her POST formunda (sadece Şifre Değiştir
değil, antiforgery token içeren HER form) 400 riski vardı.

**Çözüm:** `builder.Services.AddAntiforgery(...)` ile Session ile birebir aynı
politika açıkça verildi (Program.cs).

**Ders:** Bir cookie-tabanlı güvenlik mekanizmasını (Session) intranet/HTTP
senaryosu için ayarlarken, aynı uygulamadaki DİĞER cookie-tabanlı mekanizmaları
(Antiforgery, kimlik doğrulama çerezi vb.) da tara — her biri kendi ayrı
`CookieBuilder`'ına sahiptir, biri ayarlanınca diğerleri otomatik uyum sağlamaz.

### 6. bug — Form submit pattern tutarsızlığı: Customers JSON sayfası + çift kayıt (2026-07-19)

Müşteri kaydet sonrası ekranda ham `{"success":true,...}` JSON'u açılıyordu ve aynı
müşteri 2 kere kaydedilmişti. Kök neden: `Customers/Index.cshtml`'deki form
`type="submit"` düz HTML POST yapıyordu, `CustomersController.Save` ise HER ZAMAN
`Json(...)` döndürüyordu (redirect yok) — tarayıcı POST endpoint'ine native
navigate edip JSON'u sayfa olarak render etti. Çift kayıt da aynı kökten: native
form submit'te buton disable/debounce yok, çift tıkta çift POST gider.

Aynı sayfa ailesinde (`Suppliers/Index.cshtml`) doğru pattern zaten vardı: buton
`type="button"`, `saveSupplier()` JS fonksiyonu `fetch()` ile AJAX POST atıyor,
istek sırasında butonu disable ediyor, `showToast` + `location.reload()` ile
kapatıyor. **Ders:** Bir modül-ailesinde (CRUD sayfaları) bir sayfa doğru pattern
kullanıyorsa, YENİ/eksik sayfayı o pattern'e göre denetle — controller `Json(...)`
döndürüyorsa form'un `type="submit"` OLMAMASI ZORUNLU, aksi halde JSON sayfası +
debounce'suz çift-submit riski birlikte gelir.

### 7. bug — "FSC'siz tedarikçi" ile "FSC süresi dolmuş tedarikçi" karıştırılması (2026-07-19)

Hammadde girişinde (Purchase/SaveLot) her FSC'siz (yani hiç sertifika iddiası
olmayan, `IsFscActive=false`) tedarikçi seçildiğinde "FSC sertifikası geçersiz!"
uyarısı + onay penceresi çıkıyordu — sanki gerçekten bir sorun varmış gibi. Kök
neden: client-side JS `dataset.fsc === 'true'` kontrolü `IsFscActive`'i doğrudan
kullanıyordu; ama `IsFscActive=false` çoğu tedarikçi için NORMAL durumdur (FSC
iddiası yoktur), "geçersiz/süresi dolmuş" ile aynı şey değildir. Backend
(`PurchaseController.SaveLot`) zaten doğru mantığı kullanıyordu:
`IsFscActive && FscExpiryDate.HasValue && FscExpiryDate < Today` (yani ÖNCEDEN
sertifikalıydı ama süresi geçti) — ama frontend bunu kopyalamamıştı.

**Ders:** "X aktif değil" ile "X aktifti ama süresi doldu" iki ayrı durumdur;
UI'da uyarı/blok tetikleyen boolean'ı türetirken backend'deki GERÇEK iş kuralı
mantığını (sadece flag'i değil, flag+tarih kombinasyonunu) birebir kopyala. Aksi
halde en sık karşılaşılan (ve tamamen normal) durum yanlışlıkla "hata" gibi
gösterilir, kullanıcı sistemin kendisini engellediğini düşünür.

### 8. bug — Migration zincirinde EF'in bilmediği elle eklenmiş index (2026-07-19)

Yerel geliştirme DB'sinde (`ARDA\ARDA`) bekleyen bir migration
(`AddRowVersionAndDecimalPrecision`, `ConversionFireKg` kolonunu
`decimal(18,2)`→`decimal(18,4)` genişletiyordu) "index X kolona bağımlı" hatasıyla
patladı: `IX_FscLots_SourceSerialId_Filtered`. Bu index migration geçmişinde
(`grep`) HİÇ tanımlı değildi — EF model snapshot'ı da bilmiyordu. Demek ki bir
noktada DB'ye elle/SSMS'ten eklenmiş, versiyon kontrolüne hiç girmemiş.

**Çözüm:** Migration'ın başına `IF EXISTS (...) DROP INDEX ...` guard'lı bir
`Sql()` adımı eklendi — index yoksa no-op, varsa (herhangi bir ortamda, bu DB'ye
özel olması gerekmez) güvenle kaldırılır.

**Ders:** `dotnet ef database update` bir ortamda beklenmedik obje-bağımlılığı
hatasıyla patlarsa önce "bu objeyi migration geçmişi biliyor mu?" diye sor
(`grep` ile index/constraint adını migration dosyalarında ara). Bilmiyorsa elle
eklenmiş bir kalıntıdır — migration'ı o objeyi varsaymayacak/temizleyecek şekilde
sağlamlaştırmak, DB'yi elle düzeltmekten daha güvenlidir (kod versiyon kontrolünde
kalır, başka ortamda aynı kalıntı varsa orada da otomatik çözülür).

### 9. Müşteri güncelleme dağıtımı — AutoMigrate keşfi script'i çok basitleştirdi (2026-07-19)

10-15 müşteride her sürümde "yeni dosya + elle SQL script" yöntemi zahmetli
görünüyordu. Çözüme başlamadan önce `Program.cs`'i okuyunca fark edildi:
`Database:AutoMigrate=true` (müşteri kurulumlarının VARSAYILANI) ile uygulama
zaten AÇILIŞTA `context.Database.Migrate()` çağırıyor. Yani derlenmiş DLL'ler
yeni migration sınıflarını içeriyorsa, tek yapılması gereken dosyaları
değiştirip siteyi yeniden başlatmak — migration'ı uygulama kendisi, DB'ye elle
SQL script yapıştırmaya HİÇ gerek yok (yalnız `AutoMigrate=false` olan DBA'lı
kurumsal kurulumlarda manuel script yolu gerekir, azınlık durum).

**Ders:** Bir "nasıl dağıtırız/otomatikleştiririz" sorusuna cevap ararken önce
uygulamanın KENDİ mevcut davranışını oku — genelde "zor" görünen problem
zaten çözülmüş oluyor, sadece fark edilmemiş. `installer/update-engine.ps1` bu
yüzden SQL script çalıştırmaz; sadece DB+app klasörü yedekler, dosyaları
`robocopy` ile (asla `/MIR` değil — müşteriye özel `appsettings.json`/
`license.lic` silinmesin) üzerine kopyalar, IIS'i yeniden başlatır.

### 10. `Get-Content -Raw` + yeniden encode = kendi ürettiğin mojibake (2026-07-19)

`update-engine.ps1`'e BOM eklerken `Get-Content -Raw` ile okuyup
`[IO.File]::WriteAllText(...UTF8Encoding($true))` ile geri yazdım — dosyada
BOM yoktu, PowerShell 5.1 BOM'suz dosyayı ANSI/sistem code page sanıp
"─" (kutu-çizim tire, U+2500) gibi çok baytlı UTF-8 karakterlerini yanlış
çözdü; sonucu UTF-8 olarak yazınca mojibake KALICI hale geldi (aynı sınıf
hata `SalesController.cs`'de bu oturumun başında bulunup düzeltilmişti —
üstüne bir de kendim üretmiş oldum).

**Ders:** Türkçe/özel karakter içeren bir `.ps1`/metin dosyasına BOM eklerken
veya encoding değiştirirken ASLA `Get-Content`'in ambient/varsayılan encoding
tahminine güvenme. Okurken de yazarken de encoding'i AÇIKÇA belirt:
`[System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)` →
`[System.IO.File]::WriteAllText($path, $text, (New-Object
System.Text.UTF8Encoding($true)))`. Değişiklik sonrası mutlaka
`-match '[ÃÅÄ]|â€'` gibi bir mojibake taraması + parser doğrulaması yap.

### 11. `update-engine.ps1` VM'de ilk çalıştırma — 3 katmanlı gerçek hata (2026-07-19)

`update-engine.ps1`'i test VM'inde ilk kez çalıştırınca sırayla 3 farklı, birbirinden
bağımsız hata çıktı (installer'daki 4-katmanlı bug zincirine benzer desen —
her katman bir öncekini çözmeden görünmüyordu):

**Katman 1 — `BACKUP DATABASE` yanlış klasöre yazmaya çalıştı.**
`Operating system error 5 (Access is denied)` — hedef `C:\Users\Administrator\Desktop\...`
idi. Kök neden: `BACKUP DATABASE` dosyayı **PowerShell'i çalıştıran kullanıcı değil,
SQL Server SERVİS HESABI** (ör. `NT SERVICE\MSSQL$FSCERP`) yazar; bu hesabın
Administrator'ın Desktop'ına yazma izni yoktur. **Çözüm:** hedef klasörü elle
seçmek yerine `SELECT SERVERPROPERTY('InstanceDefaultBackupPath')` ile SQL'in
KENDİ varsayılan yedek klasörünü sorgulayıp oraya yaz — o klasör zaten servis
hesabınca yazılabilir.

**Katman 2 — `WITH COMPRESSION` SQL Express'te desteklenmiyor.**
Katman 1 düzeltilince yeni hata: `BACKUP DATABASE WITH COMPRESSION is not
supported on Express Edition`. Backup compression Standard/Enterprise'a özel bir
özellik. **Çözüm:** `WITH COMPRESSION` kaldırıldı, sade `WITH INIT` kullanıldı.

**Katman 3 — `Stop-Website`/`Stop-WebAppPool` COM sınıfı kayıt hatası.**
DB+app yedek adımları geçince: `Retrieving the COM class factory for component
with CLSID 688EEEE5-... failed ... Class not registered`. WebAdministration
PowerShell modülünün Stop/Start-WebItem cmdlet'leri bazı Windows Server
kurulumlarında (muhtemelen IIS 6 Metabase uyumluluk bileşeni eksik/kayıtsız)
bu COM hatasını veriyor — modül `Import-Module` ile sorunsuz yükleniyor ama
gerçek çağrı patlıyor. **Çözüm:** Stop/Start-Website, Stop/Start-WebAppPool,
Get-WebBinding çağrılarının HEPSİ `appcmd.exe` (`$env:windir\System32\inetsrv\appcmd.exe`)
ile değiştirildi — native exe, COM/modül bağımlılığı yok, daha sağlam.

**Genel ders:** Bir script'i ilk kez GERÇEK bir müşteri/VM ortamında çalıştırmak,
geliştirme makinesinde asla görülmeyecek 3 farklı altyapı varsayımını (klasör
izni, SQL Edition özelliği, IIS PowerShell provider sağlığı) tek seferde ortaya
çıkardı. Yerel makinede "mantıken doğru" görünen bir script, hedef ortamın
gerçek kısıtlarına (servis hesabı izinleri, Express sürüm limitleri, IIS modül
sağlığı) göre en az bir kez GERÇEK ortamda prova edilmeden güvenilir sayılmamalı.
