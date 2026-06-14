# Oturum Devir Notu (Handoff) — FSC Takip ERP

> Yeni oturumda bu dosyayı okut, kaldığın yerden devam et. Tarih: 2026-06-13

## Proje & Ortam
- **Uygulama:** FSCTakip.Erp — ASP.NET Core 8 MVC, SQL Server, EF Core, Bootstrap 5.
- **Kök:** `C:\Users\User\Desktop\FSC_ERP_Blackboxai`
- **DB:** `Server=ARDA\ARDA; Database=FscErpDb` (ev makinesi). Erişilebilir.
- **Çalıştırma kuralı:** `dotnet run` YASAK → derlenmiş DLL'i Kestrel ile çalıştır:
  `ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://localhost:5261 dotnet bin\Debug\net8.0\FSCTakip.WebUI.dll`
  (kullanıcının VS/IIS Express akışına dokunma; ayrı portta çalıştır, işin bitince durdur).
- **Giriş:** kullanıcı `ADMİN` / şifre `admin123` (dev DB; SHA-256 + sabit tuz, SQL CI collation).
- **Repo:** `github.com/goktaso/FSCTakip.Erp` (PUBLIC), branch `claude/affectionate-hawking-cb4488`. Global config repo: `goktaso/claude-config`.

## Bu Oturumda Yapılanlar (push edildi — commit 698644f)
- **F1:** HomeController'a `GetNotifications` + `GlobalSearch` eklendi (topbar çanı + global arama tüm sayfalarda kırıktı). Canlı doğrulandı.
- **F2:** `AppDbContext.SaveChangesAsync` → NOT NULL string kolona null gelirse `""` coalesce (`!prop.IsNullable`). SQL 515 hatasını kökten önler. Canlı doğrulandı.
- **F3:** Customers düzenleme formuna `IsFscActive` checkbox (hidden+checkbox bool kalıbı).
- **F4:** Customers + Suppliers `Save` → değişmemiş e-posta/telefon yeniden doğrulanmıyor (eski/geçersiz kayıtlar düzenlenebiliyor); yeni geçersiz değer hâlâ reddediliyor. Canlı doğrulandı.
- **Login:** şifre göster/gizle (👁) butonu + ARD markası (logo `wwwroot/images/ard_logo.png`, "ARD SİSTEM VE DANIŞMANLIK", alt başlık "FSC Takip ERP · Kraft Kağıt İzlenebilirlik Sistemi").
- **Kılavuz:** `docs/KULLANIM_KILAVUZU.md` v3.1 (şifre butonu, topbar arama/bildirim, FSC Aktif). Guide sayfası md'yi runtime okur, rebuild gerekmez.
- **DB temizliği:** işlem verisi silindi (FscLot/FscSerial/StockMovement/WorkOrder/ProductionDetail/Sales/Waste/WorkOrderRecipe/EtlJob → 0), identity reset. Tanımlar+Müşteri(17)/Tedarikçi(10)/Ürün(108)/Reçete(3) korundu. **Yedek:** `...MSSQL17.ARDA\MSSQL\Backup\FscErpDb_PreCleanup_20260613_210643.bak`.
- **Güvenlik hijyeni:** `appsettings.json` + `appsettings.Development.json` + `wwwroot/uploads/` + `.claude/settings.local.json` git takibinden çıkarıldı + `.gitignore`'a eklendi; `appsettings.example.json` şablonu eklendi.
- **Global tasarım standardı:** `~/.claude/CLAUDE.md` §11 (ARD palet/tipografi/komponent) → claude-config repo'ya push (bdc3ae2).
- **PDF:** `docs/Uretim_Tuketim_Is_Mantigi.pdf` — üretim/tüketim iş mantığı açıklaması (Edge headless ile üretildi; kaynak `docs/_uretim_tuketim_mantigi.html`).

## AÇIK İŞ — Öncelikli (kullanıcının asıl istediği)
**Üretim "beklenen hammadde tüketimi" otomatik hesabı.** Şu an `WorkOrderRecipe.PlannedQuantity` elle giriliyor; sistem `StandardQuantity × üretilen adet` hesabını yapmıyor → operatöre güvenmek/dışarıda hesaplamak gerekiyor.
- **Çözüm (PDF'teki E):**
  1. `Production/Detail.cshtml` → `loadFromRecipe()`/BOM ekleme: `PlannedQuantity = ProductRecipe.StandardQuantity × WorkOrder.PlannedQuantity` otomatik doldur.
  2. Tüketim modalı: "Üretilen adet" girilince `Beklenen = StandardQuantity × adet` + gerçek tüketimle **sapma %** (yeşil/sarı/kırmızı) canlı göster. İlgili fonksiyon: `updateRecipeNeedInfo()` (Detail.cshtml ~980), sabit `WORKORDER_PLANNED_QTY` (~947).
- **BEKLEYEN KARAR:** `ProductRecipe.StandardQuantity` tabanı nedir? "1 adet başına kg" mı, "1000 adet başına" mı? Hesap buna göre kurulacak — başlamadan kullanıcıya sor/teyit al.
- **Ön koşul:** Sadece 3 reçete tanımlı (108 ürün). Reçetesi olmayan üründe hesap çalışmaz; "Ürünler → Reçete"den tanım girilmeli.

## Diğer Bekleyen Kararlar
- **SQL şifre rotasyonu (güvenlik):** `data123` public repo history'de → yanmış. Kullanıcı isterse: `ALTER LOGIN [data] WITH PASSWORD='...'` + yerel `appsettings.json` (iki connection string) güncelle + app restart. Kullanıcı şifreyi belirleyecek.
- **AuditLogs/AuditPeriods:** temizlikte korundu; kullanıcı isterse temizlenebilir.

## Kod Notları / Tuzaklar
- `AppDbContext.SaveChangesAsync` tüm string'leri tr-TR UPPERCASE yapar (PasswordHash/email dikkat; F2 coalesce burada).
- `PaperWeight` ve `Machine` BaseEntity'den türemez (manuel audit alanları).
- Form alan adları ↔ entity property / controller param birebir eşleşmeli (FormData append pattern).
- Önceki tam denetim sonucu: tüm modüller (23 controller, 37 menü linki) statik+canlı temiz; bilinen başka kırık yok.
