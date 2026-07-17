# FSC Takip ERP — Üçüncü Taraf (Müşteri Sahası) Kurulum Runbook'u

**Doküman No:** ARD-KUR-001 · **Sürüm:** 2.0 · **Tarih:** 16.07.2026
**Hazırlayan:** ARD Sistem ve Danışmanlık
**Örnek Müşteri Profili:** XYZ Ambalaj (mevcut ERP: Canias — veri aktarımı Excel/ETL yoluyla)

> Bu doküman, FSC Takip ERP'nin müşteri intranetine sıfırdan kurulumunu, ilk veri yüklemesini, doğrulamasını ve teslimini adım adım tanımlar. Her ana bölümün sonunda ✅ kontrol maddesi vardır — tamamı işaretlenmeden bir sonraki faza geçilmez.

---

## ⚡ v2.0 — FAZ 1–4 artık otomatik: `FscErpSetup.exe`

**Teknik kurulum (ön koşullar, SQL, IIS, veritabanı şeması) tek bir kurulum paketiyle yapılır.**
Sunucuya EXE'yi kopyala, çift tıkla, SQL örneği ve portu seç — ~15 dakikada çalışan sistem.

| Faz | v1.0 (elle) | v2.0 |
|---|---|---|
| FAZ 1 ön koşullar (IIS, .NET Hosting Bundle) | elle | **EXE** |
| FAZ 2 SQL Server (kurulum veya mevcut örnek) | elle | **EXE** |
| FAZ 3 uygulama dağıtımı, IIS, firewall, appsettings | elle | **EXE** |
| FAZ 3.5 lisans | kurulum beklerdi | **30 gün deneme otomatik başlar** |
| FAZ 3.6 güncelleme | elle kopyalama | **EXE'yi tekrar çalıştır** |
| FAZ 4 şema + ilk açılış | SSMS'te migration.sql | **EXE (uygulama kendi migrate eder)** |
| FAZ 0, 5–10 | danışmanlık | **değişmedi — elle** |

Paket üretimi ve saha kullanımı: **`installer/README.md`**.

Aşağıdaki FAZ 1–4 bölümleri **başvuru** olarak korunuyor: müşteri DBA'sı sürecin
her adımını görmek istediğinde, EXE bir adımda takıldığında tanı için, ve elle
kurulum gereken kurumsal senaryolarda geçerlidir. Paketin yaptığı iş birebir budur.

---

## FAZ 0 — Satış Sonrası Keşif (kurulumdan ÖNCE, uzaktan yapılabilir)

Müşteriden yazılı olarak toplanacaklar:

| # | Bilgi | Neden |
|---|---|---|
| 0.1 | Sunucu var mı / temin edilecek mi, mevcutsa özellikleri | Faz 1 kararı |
| 0.1b | **Mevcut bir SQL Server var mı** (başka bir uygulama/ERP için)? Varsa sürümü/instance adı | Faz 2A/2B kararı — varsa sıfırdan kurulum gerekmez |
| 0.2 | Ağ yapısı: sunucuya hangi istemciler, hangi VLAN'dan erişecek; DNS var mı | Erişim adresi (`http://fsc.xyz.local` gibi) |
| 0.3 | Canias'tan Excel export alabilen bir yetkili/IT sorumlusu kim | Faz 7 veri aktarımı |
| 0.4 | FSC sertifika bilgileri: CoC kodu, lisans kodu, denetim dönemi tarihleri, sertifikasyon kuruluşu | Faz 6 ana veri |
| 0.5 | Ürün grupları, makine listesi, depo listesi (kaba taslak) | Faz 6 hazırlığı |
| 0.6 | Tedarikçi listesi + her birinin FSC sertifika kodu ve geçerlilik tarihi | Faz 7; **eksik FSC kodu = denetimde Major bulgu riski**, baştan toplansın |
| 0.7 | Uzaktan destek yöntemi tercihi (VPN / AnyDesk / TeamViewer) ve müşteri IT politikası | Bakım sözleşmesi eki |
| 0.8 | Yedekleme altyapısı: mevcut bir yedek sunucusu/NAS var mı | Faz 9 |

✅ Keşif formu dolduruldu ve iki tarafça e-postayla teyit edildi.

---

## FAZ 1 — Donanım ve Altyapı Gereksinimleri

### Sunucu (minimum → önerilen)

| Bileşen | Minimum | Önerilen |
|---|---|---|
| İşletim sistemi | Windows Server 2019 | Windows Server 2022 |
| CPU | 4 çekirdek | 8 çekirdek |
| RAM | 8 GB | 16 GB |
| Disk | 256 GB SSD | 512 GB NVMe SSD (OS/uygulama ve veri için ayrı bölüm) |
| Ağ | 1 Gbps intranet | 1 Gbps + sabit IP |
| Güç | — | UPS (SQL Server ani kapanmada veri bütünlüğü riski taşır) |

- Tek sunucu yeterlidir (uygulama + SQL aynı makinede). 10+ eşzamanlı kullanıcı veya 2.000+ lot öngörülüyorsa SQL ayrı sunucuya alınabilir (ilk kurulumda gerekmez).
- Sanal makine (Hyper-V/VMware/Proxmox) tamamen uygundur — anlık görüntü (snapshot) imkânı kurulum güvenliği sağlar.

### İstemciler
- Herhangi bir modern tarayıcı (Chrome/Edge önerilir). İstemciye **hiçbir kurulum yapılmaz.**
- Yazdırma: irsaliye/fatura/iş emri formları tarayıcı yazdırmasıyla çıkar — istemcide tanımlı yazıcı yeterli.

### Yazılım ön koşulları (sunucuya, ARD kurar)
1. **.NET 8 Hosting Bundle** (ASP.NET Core Runtime — IIS entegrasyonlu)
2. **SQL Server 2019+** — başlangıç için **Express** sürümü yeterlidir (ücretsiz, 10 GB veri limiti; mevcut test verilerine göre 10 GB ≈ yıllarca kayıt). Müşteri Standard lisansına sahipse o kullanılır.
3. **SQL Server Management Studio (SSMS)** — bakım/yedek kontrolü için.
4. IIS (Windows özelliği) **veya** Kestrel + Windows Service (aşağıda ikisi de tanımlı; IIS önerilir).

✅ Sunucu teslim alındı, RDP erişimi doğrulandı, Windows güncellemeleri tamamlandı, saat dilimi `Turkey Standard Time`.

---

## FAZ 2 — SQL Server Kurulumu

**Önce karar ver:** Müşteride zaten çalışan bir SQL Server var mı (başka bir uygulama/ERP için)? Çoğu kurumsal müşteride vardır — sıfırdan kurulum şart değil.

- **Yok / bilmiyorum →** FAZ 2A'yı uygula (sıfırdan kurulum).
- **Var (Express/Standard/Enterprise, herhangi bir sürüm) →** FAZ 2B'yi uygula (yalnız yeni veritabanı ekle). Sunucunun **genel/varsayılan collation'ı önemli değil** — bizim veritabanımız kendi collation'ını `CREATE DATABASE` sırasında ayrıca alır, sunucudaki diğer veritabanlarını etkilemez.

### FAZ 2A — Sıfırdan SQL Server Kurulumu

1. SQL Server kurulum medyasını başlat → **New SQL Server stand-alone installation**.
2. Feature seçimi: yalnız **Database Engine Services** (SSRS/SSAS gerekmez).
3. Instance: `MSSQLSERVER` (default) veya `FSCERP` adlı named instance.
4. **⚠️ KRİTİK — Collation:** Server Configuration → Collation sekmesinde **`Turkish_CI_AS`** seçilmelidir. *(Sistem kullanıcı adlarını Türkçe kurallarla büyük harfe çevirerek saklar; farklı collation'da oturum açma eşleşmesi bozulabilir. Bu adım kurulumdan sonra değiştirilemez — atlanırsa SQL yeniden kurulur.)* Bu adım yalnız FAZ 2A'da (yeni kurulum) geçerlidir — mevcut sunucuya ekleniyorsa FAZ 2B'ye bak, sunucu collation'ını değiştirmene gerek yok.
5. Authentication: **Windows Authentication** (uygulama aynı sunucudaysa yeterli ve en güvenlisi). Uygulama ayrı sunucuda olacaksa Mixed Mode + güçlü `sa`-dışı SQL login.
6. Kurulum sonrası:
   - SQL Server Configuration Manager → TCP/IP **Enabled** (yalnız uygulama ayrı sunucudaysa gerekli).
   - Windows Güvenlik Duvarı: 1433 portu **yalnız intranet** (hiçbir koşulda internete açılmaz).
   - `ALTER SERVER CONFIGURATION` gerekmez; varsayılanlar yeterli.
7. Devamı için FAZ 2B madde 2-3'ü uygula (veritabanı oluşturma + servis hesabı yetkisi — ortak adımlar).

✅ `SELECT SERVERPROPERTY('Collation')` = `Turkish_CI_AS` (sunucu varsayılanı) · boş `FscErpDb` mevcut · servis hesabı bağlanabiliyor.

### FAZ 2B — Mevcut SQL Server'a Ekleme (paylaşılan/kurumsal sunucu)

1. **Bilgi topla:** Instance adı (`SUNUCU\INSTANCE` veya default), port (1433 mü değişik mi), kimlik doğrulama modu (Windows/Mixed), müşteri DBA'sı var mı — varsa DB oluşturma ve login yetkisini ondan iste (sistemik olarak `sysadmin` ile bağlanmamız gerekmeyebilir, sadece `dbcreator` + hedef DB'de `db_owner` yeterli).
2. Veritabanını oluştur — **sunucu collation'ından bağımsız**, kendi collation'ımızı açıkça belirtiyoruz:
   ```sql
   CREATE DATABASE FscErpDb COLLATE Turkish_CI_AS;
   ```
   *(Sunucunun kendisi `SQL_Latin1_General_CP1_CI_AS` veya başka bir collation'da olsa dahi bu sorunsuz çalışır — diğer müşteri veritabanlarını etkilemez.)*
3. Uygulama servis hesabına yetki (IIS AppPool kullanılıyorsa):
   ```sql
   CREATE LOGIN [IIS APPPOOL\FscErpAppPool] FROM WINDOWS;
   USE FscErpDb;
   CREATE USER [IIS APPPOOL\FscErpAppPool] FOR LOGIN [IIS APPPOOL\FscErpAppPool];
   ALTER ROLE db_owner ADD MEMBER [IIS APPPOOL\FscErpAppPool];
   ```
4. **appsettings.json bağlantı dizesi** instance adına göre ayarlanır — named instance ise `Server=SUNUCUADI\INSTANCE`, default instance ise `Server=SUNUCUADI` (uygulama SQL ile aynı makinede değilse `localhost` yerine gerçek sunucu adı/IP'si yazılır).
5. **Paylaşılan sunucu notu:** Diğer uygulamaların performansını etkilememek için resource governor/iş yükü konusunda DBA ile konuş; SQL Server Agent job'ları (yedekleme planı, Faz 9) mevcut bakım pencerelerine çakışmayacak şekilde planlanır.

✅ `FscErpDb` mevcut sunucuda oluşturuldu · `SELECT DATABASEPROPERTYEX('FscErpDb','Collation')` = `Turkish_CI_AS` (sunucu geneli farklı olsa da DB özelinde doğru) · servis hesabı bağlanabiliyor · diğer müşteri veritabanları etkilenmedi.

---

## FAZ 3 — Uygulama Dağıtımı

### 3.1 Dağıtım paketi (ARD tarafından önceden hazırlanır)
`dotnet publish -c Release` çıktısı + şu ekler:
- `appsettings.example.json` (şablon — gerçek bağlantı bilgisi paketle **gönderilmez**)
- `migration.sql` — **idempotent tam şema scripti** (`dotnet ef migrations script --idempotent` ile üretilir; müşteri sunucusunda .NET SDK/EF tooling gerektirmez)
- `tools/regression_suite.py` + gereksinimleri (Faz 8 doğrulaması; alternatif olarak ARD dizüstüsünden koşulur)
- Sürüm etiketi (git tag) ve SHA256 özet listesi

### 3.2 IIS ile kurulum (önerilen)
1. **Önce IIS'i etkinleştir** (Windows Özellikleri → World Wide Web Hizmetleri + Web Yönetim Araçları), **sonra** .NET Hosting Bundle'ı kur. Sıra ters olduysa (Hosting Bundle IIS'ten önce kurulduysa) Hosting Bundle yükleyicisini **Onar (Repair)** ile tekrar çalıştır — aksi halde ASP.NET Core IIS modülü kayıt olmaz, ilk açılışta HTTP 500.30 verir.
2. Uygulama dosyalarını `C:\inetpub\FscErp\app` altına kopyala.
3. IIS Manager → Application Pool: `FscErpAppPool`, .NET CLR = **No Managed Code**, Start Mode = AlwaysRunning.
4. Site: `FSC-ERP`, fiziksel yol `C:\inetpub\FscErp\app`, binding `http :80` (host: `fsc.xyz.local` — müşteri DNS'ine A kaydı ekletilir). **Port 80 doluysa** (başka bir web sunucusu/panel çakışıyorsa) alternatif port seç (ör. 8080) — ama bu durumda kullanıcılara adres çubuğuna portu da yazmaları gerektiği açıkça bildirilmeli (`http://fsc.xyz.local:8080`), aksi halde kullanıcı yalnız `fsc.xyz.local` yazar ve tarayıcı varsayılan olarak HTTPS/443 dener, `ERR_SSL_PROTOCOL_ERROR` alır.
5. **HTTPS (intranet):** müşteri iç CA'sı varsa oradan sertifika; yoksa self-signed + istemcilere GPO ile güven dağıtımı, veya intranette HTTP kabulü müşteri IT politikasına yazılır.
6. **Windows Firewall — ZORUNLU:** IIS site'ın IP'ye bağlı olması (Tümü Atanmamış) tek başına yetmez; Windows Firewall'da site portu için gelen (inbound) kural olmadıkça sunucunun KENDİSİ dışında hiçbir istemci bağlanamaz (sunucudan/RDP'den test "çalışıyor" görünse bile ağdaki diğer PC'lerden erişilemez). Yönetici PowerShell:
   ```powershell
   New-NetFirewallRule -DisplayName "FSC ERP HTTP" -Direction Inbound -Protocol TCP -LocalPort 80 -Action Allow
   ```
   (Port 8080 kullanıldıysa `-LocalPort 8080`.) Kural eklendikten sonra **başka bir bilgisayardan** (sunucunun kendisinden değil) `http://<sunucu-ip>:<port>` ile test et — asıl doğrulama budur.
7. `appsettings.json` oluştur (example'dan kopya) — yalnız şu alanlar düzenlenir:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=FscErpDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   },
   "FileStorage": { "Root": "D:\\FscErpData\\uploads" }
   ```
   **SQL Server named instance kullanıldıysa** (ör. Express kurulumunda varsayılan `SQLEXPRESS`), `Server=localhost` YETMEZ — `Server=localhost\\SQLEXPRESS` yazılmalı (JSON'da tek ters slash geçersizdir, **çift** `\\\\SQLEXPRESS` gerekir).
8. `D:\FscErpData\uploads` klasörünü oluştur, AppPool hesabına **Modify** yetkisi ver. *(Belge arşivi — irsaliye/fatura PDF'leri — uygulama klasörünün DIŞINDA tutulur ki sürüm güncellemesi dosyaları ezmesin.)*
9. **SQL login — ZORUNLU (migration'dan sonra, ilk açılıştan önce):** Yeni bir IIS App Pool'un SQL Server'da varsayılan olarak hiçbir erişimi yoktur; aksi halde "Cannot open database" (SQL Error 4060) alınır. SSMS'te çalıştır:
   ```sql
   CREATE LOGIN [IIS APPPOOL\FscErpAppPool] FROM WINDOWS;
   USE FscErpDb;
   CREATE USER [IIS APPPOOL\FscErpAppPool] FOR LOGIN [IIS APPPOOL\FscErpAppPool];
   ALTER ROLE db_owner ADD MEMBER [IIS APPPOOL\FscErpAppPool];
   ```
   (App Pool adı farklıysa `FscErpAppPool` yerine onu yaz.)

### 3.3 Alternatif: Kestrel + Windows Service (IIS istenmiyorsa)
```
sc create FscErp binPath= "C:\Program Files\dotnet\dotnet.exe C:\FscErp\app\FSCTakip.WebUI.dll" start= auto
```
+ `ASPNETCORE_URLS=http://0.0.0.0:5000` ortam değişkeni (servis düzeyinde). Güvenlik duvarında 5000 yalnız intranet.

✅ Site ayakta, `http://fsc.xyz.local` sunucudan ve bir istemciden açılıyor (henüz login beklenmez — DB boş).

---

## FAZ 3.5 — Lisans Etkinleştirme

> **v2.0:** Kurulum artık lisans beklemez. Sistem **30 gün** tam özellikli çalışır
> (her sayfada kalan gün bandı görünür), bu sürede kalıcı lisans uzaktan hallolur —
> saha ekibi kurulumu bitirip çıkabilir. Süre dolduğunda erişim kapanır, **veriler
> korunur**; lisans yüklenince kaldığı yerden devam eder.
>
> Kurulum raporu (`<VeriKlasörü>\kurulum-raporu.txt`) ve sihirbazın son sayfası
> sunucu kimlik kodunu zaten yazar — aşağıdaki 1-2. adımlara gerek kalmaz, doğrudan
> 3. adımdan devam edilir.

Uygulama, RSA-imzalı ve **sunucuya özel** bir lisans dosyası (`license.lic`) olmadan deneme süresi dışında çalışmaz — deneme bitmiş ve lisans yoksa tüm sayfalar Lisans Durumu ekranına yönlenir.

1. Uygulamayı aç → `http://fsc.xyz.local/License/Status` (deneme sürümünde bandın üzerindeki "Lisans etkinleştir" linkinden).
2. Ekrandaki **Sunucu Kimlik Kodu**'nu (16 haneli) not al ve ARD'ye ilet.
3. ARD, bu koda kesilmiş `license.lic` dosyasını üretip gönderir *(ARD içi: `python tools/license_gen.py --private-key <özel-anahtar> --licensed-to "<Firma Ünvanı>" --machine <kod> [--valid-until YYYY-AA-GG]`)*.
4. Status sayfasındaki **Lisans Dosyasını Yükle** alanından dosyayı yükle — sistem imzayı doğrular, geçersiz/başka sunucuya ait dosyayı reddeder.
5. Sayfa "Lisans Geçerli" durumuna döner; lisans sahibi ve geçerlilik tarihi görünür.

> **Notlar:** Süreli lisanslarda bitişe 30 gün kala sistem girişte otomatik uyarı gösterir. Sunucu değişiminde (donanım yenileme, VM taşıma) kimlik kodu değişir — ARD'den yeni lisans istenir (bakım sözleşmesi kapsamında ücretsiz).

✅ `/License/Status` "Lisans Geçerli" gösteriyor · lisans dosyasının bir kopyası müşteri IT arşivine kaydedildi.

---

## FAZ 3.6 — Güncelleme Dağıtımı (Kurulum Sonrası Patch/Sürüm Yükseltme)

Müşteriye kurulum bittikten sonra yeni bir sürüm (bugfix/özellik) çıktığında uygulanacak prosedür:

1. ARD tarafında: `dotnet publish -c Release -o publish_update` ile yeni sürüm derlenir.
2. Paket **appsettings.json ve license.lic HARİÇ** her şeyi içerir (bu iki dosya müşteriye özel, publish çıktısında olmamalı/ezilmemeli). Zip'lenip müşteriye iletilir.
3. Müşteri sunucusunda: IIS Manager → Uygulama Havuzu → **Durdur**.
4. Zip'i geçici bir klasöre aç, içeriğini uygulama klasörünün üzerine kopyala — **appsettings.json çakışmasında "Atla/Bu dosyayı kopyalama" seç**, geri kalan her dosyada "Üzerine yaz".
5. IIS Manager → Uygulama Havuzu → **Başlat**.
6. Siteyi aç, temel bir sayfa gezinip hatasız açıldığını doğrula (500.30 çıkarsa `app\logs\stdout_*.log` dosyasına bak — bkz. EK A).

> `license.lic` publish çıktısında hiçbir zaman yer almaz (paket script'i bunu garanti eder) — güncelleme müşterinin lisansını etkilemez.

✅ Yeni sürüm ayakta · appsettings.json/license.lic değişmedi · temel gezinme testi geçti.

---

## FAZ 4 — Veritabanı Şeması ve İlk Açılış

> **v2.0:** Uygulama ilk açılışta bekleyen migration'ları **kendisi uygular**
> (`Program.cs` → `Database.Migrate()`, `Database:AutoMigrate` bayrağıyla açık).
> SSMS'te `migration.sql` çalıştırmaya gerek yoktur.
>
> **DBA'sı olan kurumsal müşteride** şema değişikliğini kendisi yönetmek isteyen bir
> DBA varsa: `appsettings.json` → `"Database": { "AutoMigrate": false }` yap ve aşağıdaki
> 1. maddeyi elle uygula. `migration.sql` yolu bu senaryo için korunmuştur.

1. *(yalnız `AutoMigrate: false` ise)* SSMS'te `migration.sql`'i `FscErpDb` üzerinde çalıştır (idempotent — hata alırsa tamamı durur, ARD'ye bildirilir).
2. Uygulamayı aç. İlk açılışta sistem **otomatik** olarak:
   - Bekleyen migration'ları uygular (şemayı kurar/günceller),
   - Başlangıç referans verilerini yükler (depolar, torba tipleri, ürün grupları, makineler, kağıt tanımları, örnek kayıtlar),
   - **`admin` / `admin123`** tam yetkili kullanıcısını oluşturur.
3. `http://fsc.xyz.local` → `admin`/`admin123` ile giriş **doğrulanır**.
4. **HEMEN:** sağ üst menü → Şifre Değiştir → güçlü parola. Yeni parola müşteri IT sorumlusuna kapalı zarf/parola kasası ile teslim edilir — e-postayla gönderilmez.

✅ Login başarılı · admin parolası değiştirildi · Dashboard açılıyor.

---

## FAZ 5 — Örnek Verinin Temizlenmesi ve Kullanıcılar

1. Otomatik yüklenen **örnek işlem kayıtları** (örnek lot/iş emri/satış) müşteri verisi girilmeden önce silinir (Sistem Yönetimi → ilgili modüllerden; ARD kurulum scriptiyle de yapabilir). Referans tanımlar (depo/makine/grup) müşteriye göre düzenlenerek **korunur**.
2. Kullanıcılar (Sistem Yönetimi → Kullanıcılar): her personele **kendi hesabı** açılır — ortak hesap kullanılmaz (FSC denetiminde "kim girdi/kim düzeltti" izi için).
3. Yetki grupları: en az iki profil önerilir — `Veri Girişi` (üretim/satış personeli) ve `Yönetici` (düzeltme + dönem kilidi yetkisi).

✅ Örnek işlem verisi temiz · en az 2 gerçek kullanıcı tanımlı · admin dışındaki hesaplarla giriş test edildi.

---

## FAZ 6 — Ana Veri (Master Data) Kurulumu — müşteriyle birlikte

Sıra önemlidir (bağımlılıklar):

0. **Şirket Bilgileri (beyaz etiket)** — Tanımlamalar → Şirket Bilgileri (`/Company/Settings`, yalnız admin): firmanın tam ünvanı, adresi, vergi bilgileri ve **kendi FSC CoC + Lisans kodları** girilir. Bu bilgiler sevk irsaliyesi, satış faturası ve iş emri formunun başlığında **belge sahibi ünvanı** olarak basılır — girilmezse belgeler varsayılan ARD ünvanıyla çıkar. *(Denetçi, belgedeki ünvan ve FSC kodlarının denetlenen firmaya ait olmasını bekler — bu adım atlanamaz.)*
1. **FSC Tipleri** (Tanımlamalar → FSC Tipleri): tam liste girilir — `FSC 100%`, `FSC MIX Credit`, `FSC MIX %70`, `FSC Recycled 100%`, **`FSC'siz (FSC-NONE)`**. *(FSC'siz tipi zorunludur: sertifikasız hammadde girişlerinin iddiasız işaretlenmesi denetim gereğidir.)*
2. **Ürün Grupları**: müşterinin kod aralıklarına göre (Canias stok kod yapısıyla uyumlu seçilirse ETL eşleşmesi kolaylaşır).
3. **Depolar, Makineler, Torba Tipleri, Kağıt tanımları** (tip/renk/gramaj/en).
4. **Tedarikçiler**: ad, vergi no, **FSC sertifika kodu + geçerlilik tarihi** (Faz 0.6 listesinden). FSC kodu olmayan tedarikçi `FSC belgesi yok` olarak işaretlenir — sistem bu tedarikçiden FSC iddialı giriş yapılırsa **otomatik kritik uyarı** üretir.
5. **Müşteriler**: ad, vergi no, varsa FSC lisans kodu.
6. **Ürün kartları**: mamul + hammadde. Canias stok kodu her karta **Dış Kod (ExternalCode)** alanına girilir — ETL eşleştirmenin anahtarıdır.
7. **Denetim Dönemi** (FSC İzlenebilirlik → Denetim Dönemleri): müşterinin gerçek FSC denetim dönemi tarihleriyle açılır.

✅ Tüm tanım listeleri müşteri yetkilisiyle birlikte kontrol edildi ve yazılı onaylandı.

---

## FAZ 7 — Canias'tan Veri Aktarımı (ETL)

Mimari ilke: **sistem Canias veritabanına doğrudan bağlanmaz.** Müşteri IT'si Canias'tan Excel export alır → ARD şablonuna eşlenir → ERP Entegrasyon → Excel Aktarımı ekranından yüklenir. (Şablonlar sistemden indirilir: ERP Entegrasyon → Şablon İndir — dropdown doğrulamalı, örnek satırlı.)

### 7.1 Eşleme tablosu (müşteri IT'siyle doldurulur)

| FSC ERP Alanı | Şablon Sütunu | Canias Karşılığı (müşteri IT doldurur) |
|---|---|---|
| Tedarikçi adı | `TedarikciAdi` | … |
| Vergi no | `VergiNo` | … |
| FSC kodu | `FscKodu` | *(genelde Canias'ta yoktur — Faz 0.6 listesinden)* |
| Müşteri adı | `MusteriAdi` | … |
| Stok kodu | `StokKodu` | Canias malzeme no |
| Parti/lot no | `PartiNo` | Canias parti/charge no |
| Bobin/seri no | `SeriNo` | … |
| Miktar (kg) | `Miktar_kg` | … |
| İrsaliye no | `IrsaliyeNo` | … |
| Fatura no | `FisNo` | … |
| Tarih | `Tarih` (`gg.aa.yyyy`) | … |

### 7.2 Yükleme sırası (bağımlılık zinciri)
1. Tedarikçiler → 2. Müşteriler → 3. Ürünler → 4. Hammadde lotları/bobinler → 5. (İsteğe bağlı geçmiş) üretim ve satış kayıtları.

Her dosyada **Önizleme** adımı kullanılır; hata satırları (eşleşmeyen stok kodu/tedarikçi) düzeltilip yeniden yüklenir. Sistem mükerrer kaydı günceller, yenisini ekler — aynı dosyanın iki kez yüklenmesi veri bozmaz.

### 7.3 Açılış stoku kararı (müşteriyle netleştirilir)
- **Seçenek A (önerilen):** yalnız *mevcut stoktaki* lotlar açılış bakiyesi olarak yüklenir; izlenebilirlik kurulum tarihinden ileri doğru işler. Basit ve temiz.
- **Seçenek B:** içinde bulunulan denetim döneminin tüm geçmişi yüklenir (üretim+satış dahil) — ilk denetim raporu tam dönem kapsar; veri hazırlama eforu belirgin şekilde yüksektir.

✅ ETL yüklemeleri 0 hata ile tamamlandı · lot sayısı/kg toplamları Canias raporuyla mutabakatlandı (yazılı tutanak).

---

## FAZ 8 — Doğrulama (kabul öncesi, ARD yapar)

1. **Regresyon paketi:** `python tools/regression_suite.py --base-url http://fsc.xyz.local --company XYZ` → **7/7 PASS** zorunlu (login, 24 sayfa taraması, kütle dengesi, negatif bakiye engeli, izlenebilirlik, kritik uyarılar, yazdırma).
2. **Uçtan uca canlı akış (müşteri personeliyle):** 1 hammadde girişi → 1 iş emri + tüketim → üretim tamamlama → 1 satış + sevkiyat → Tam İzlenebilirlik ekranında zincirin tek ekranda görünmesi.
3. **Denetim provası:** Denetim Özeti raporu açılır → kütle dengesi "Dengeli" doğrulanır; Uyarı Paneli'ndeki kritikler (varsa) müşteriyle tek tek kapatma planına bağlanır.
4. **Fazla sevkiyat reddi** müşteri gözü önünde canlı gösterilir (sistemin negatif stoka izin vermediğinin kanıtı — güven adımı).

✅ Regresyon 7/7 · uçtan uca akış personel tarafından yapıldı · doğrulama tutanağı imzalandı.

---

## FAZ 9 — Yedekleme ve İşletim

1. **SQL yedek planı (SSMS → Maintenance Plan veya Agent Job; Express'te Windows Görev Zamanlayıcı + script):**
   - Günlük FULL backup (gece) → `D:\FscErpData\backup\`
   - Recovery model FULL ise 4 saatte bir log backup (Express'te SIMPLE + günlük FULL kabul edilebilir başlangıçtır)
   - 30 gün yerel saklama + **sunucu dışına** (NAS/başka makine) günlük kopya
2. **Uploads klasörü** (`D:\FscErpData\uploads` — irsaliye/fatura PDF arşivi) aynı plana dahil edilir. *(FSC denetimi 5 yıl geriye belge isteyebilir — bu klasör veritabanı kadar kritiktir.)*
3. **Geri dönüş tatbikatı:** kurulum haftasında bir yedek, boş bir DB'ye fiilen geri yüklenip login test edilir — "yedek var" değil "yedek dönüyor" kanıtlanır.
4. **Sürüm güncelleme prosedürü (bakım sözleşmesi kapsamı):** DB yedeği al → `app` klasörünü yenisiyle değiştir (uploads dışarıda olduğundan etkilenmez) → varsa yeni `migration.sql` çalıştır → regresyon paketini koştur → tutanak.
5. İzleme: Windows Event Log + uygulama loglarının konumu müşteri IT'sine gösterilir.

✅ Yedek görevleri kurulu ve ilk yedek alındı · geri dönüş tatbikatı yapıldı · güncelleme prosedürü IT sorumlusuna teslim edildi.

---

## FAZ 10 — Eğitim ve Teslim

1. **Kullanıcı eğitimi** (½ gün, KULLANIM_KILAVUZU.md üzerinden): hammadde girişi, iş emri/tüketim, satış/sevk, raporlar. Katılımcı listesi + eğitim tutanağı imzalatılır.
2. **Yönetici eğitimi** (½ gün): kullanıcı/yetki yönetimi, dönem kilidi, düzeltme (neden zorunlu) akışı, Uyarı Paneli, denetim raporları, ETL.
3. **Denetim hazırlık dosyası teslim edilir:** FSC CoC İç Denetim Protokolü (denetçiye gösterim sırası — 11 adım) + Denetim Özeti örnek çıktısı.
4. **Kabul tutanağı** imzalanır: kurulum kapsamı, doğrulama sonuçları (7/7), teslim edilen belgeler, admin parolasının teslimi, garanti/bakım başlangıç tarihi.
5. Destek kanalı tanımlanır: telefon/e-posta, yanıt süreleri (bakım sözleşmesindeki SLA), uzaktan erişim yöntemi.

✅ Eğitim tutanakları + kabul tutanağı imzalı · destek kanalı aktif · **kurulum tamamlandı.**

---

## EK A — Hızlı Sorun Giderme

| Belirti | İlk kontrol |
|---|---|
| Site açılmıyor (502/503) | App Pool durumu; Hosting Bundle kurulu mu; Event Log → uygulama hatası |
| **HTTP 500.30 "app failed to start"** | `web.config`'te `stdoutLogEnabled="true"` yap + `app\logs\` klasörü oluştur, siteyi tekrar tetikle, en yeni `stdout_*.log` dosyasına bak. Sık nedenler: (a) SQL login yok → "Cannot open database" (bkz. Faz 3.2 madde 9), (b) Hosting Bundle IIS'ten önce kurulmuş → Hosting Bundle'ı **Onar** ile tekrar çalıştır |
| Sunucudan açılıyor ama başka bilgisayardan açılmıyor | Windows Firewall'da site portu için inbound kural var mı (bkz. Faz 3.2 madde 6) — bu senaryoda "localhost'ta çalışıyor" testi YANILTICIDIR |
| Kullanıcı `ERR_SSL_PROTOCOL_ERROR` alıyor | Adres çubuğuna port yazmamış / `http://` şemasını atlamış — tarayıcı varsayılan 443/HTTPS deniyor. Tam adresi (`http://sunucu:port`) kullanım kılavuzuna ekle |
| Login "kullanıcı bulunamadı" | DB collation `Turkish_CI_AS` mi (`SELECT SERVERPROPERTY('Collation')`) |
| Şifre değiştirme "mevcut şifre hatalı" (login doğru şifreyle çalışıyor olsa bile) | `AppUser.PasswordHash` bozulmuş olabilir (eski sürüm bug'ı, `SaveChangesAsync` uppercase). Düzeltme: `UPDATE AppUsers SET PasswordHash = LOWER(PasswordHash);` (güvenli, veri kaybı yok) — 2026-07-05 sonrası derlemelerde bug giderildi |
| ETL "Ürün bulunamadı" | Ürün kartında Dış Kod (Canias stok kodu) girilmiş mi |
| PDF belge açılmıyor | `FileStorage:Root` yolu ve AppPool yazma yetkisi |
| Sayfalar yavaşladı (2.000+ lot) | ARD'ye bildir — performans yol haritası (index/SQL View aşaması) devreye alınır |

## EK B — Kurulum Zaman Planı (tipik)

| Gün | İş |
|---|---|
| 1 (uzaktan) | Faz 0 keşif + paket hazırlığı (`build-installer.ps1`) |
| 2 (sahada) | **Faz 1–4: `FscErpSetup.exe` — ~15 dk.** Kalan gün Faz 5–6'ya kayar |
| 2–3 (sahada) | Faz 5–6: kullanıcılar + ana veri (müşteriyle) |
| 3–4 (sahada/uzaktan) | Faz 7: ETL aktarımı + mutabakat |
| 4–5 (sahada) | Faz 8–10: doğrulama, yedek, eğitim, kabul |

*v2.0 ile teknik kurulum bir günden ~15 dakikaya indi; darboğaz artık ana veri ve ETL'dir
(Faz 6–7). Toplam ~3-4 gün. Geçmiş dönem verisi (Seçenek B) seçilirse +2-4 gün.*

**Faz 0'da bunu sor:** müşteride mevcut bir SQL Server var mı? Varsa örnek adını önceden
al — kurulum sihirbazı örnekleri otomatik listeler ama doğru olanı seçmek için bilgi gerekir.

## EK C — Kurulum Paketi Takılırsa

EXE hata verirse ekranda gösterilen çıktı dosyasını aç; motor hangi adımda durduğunu ve
ne yapılacağını yazar. Ayrıntılı kayıt: `<VeriKlasörü>\logs\setup-<tarih>.log`.

| Belirti | Sebep / çözüm |
|---|---|
| "SQL örneğine bağlanılamadı" | Örnek adı yanlış veya Windows hesabınızın SQL yetkisi yok. Doğru ad için: `Get-Service MSSQL*` |
| "Uygulama havuzuna SQL yetkisi verilemedi" | Kurumsal SQL'de sysadmin değilsiniz. Hata mesajı DBA'ya verilecek SQL'i birebir yazar; çalıştırılınca kurulumu tekrar başlatın (FAZ 2B) |
| "ASP.NET Core IIS modülü bulunamadı" | Hosting Bundle yeniden başlatma istedi. Sunucuyu yeniden başlatıp EXE'yi tekrar çalıştırın |
| "Site yanıt vermedi" | En sık: SQL login veya 500.30. Motor `stdout_*.log` yolunu yazar — EK A'ya bakın |
| SQL Express kurulumu takıldı | Sunucuda bozuk/yarım bir SQL kurulumu olabilir. `%ProgramFiles%\Microsoft SQL Server\<sürüm>\Setup Bootstrap\Log\Summary.txt` |

Kurulum idempotenttir: sorunu giderip EXE'yi **tekrar çalıştırmak güvenlidir**, tamamlanmış
adımlar atlanır, `appsettings.json` ve `license.lic` korunur.
