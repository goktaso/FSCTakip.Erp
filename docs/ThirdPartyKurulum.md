# FSC Takip ERP — Üçüncü Taraf (Müşteri Sahası) Kurulum Runbook'u

**Doküman No:** ARD-KUR-001 · **Sürüm:** 1.0 · **Tarih:** 04.07.2026
**Hazırlayan:** ARD Sistem ve Danışmanlık
**Örnek Müşteri Profili:** Packy Packaging (mevcut ERP: Canias — veri aktarımı Excel/ETL yoluyla)

> Bu doküman, FSC Takip ERP'nin müşteri intranetine sıfırdan kurulumunu, ilk veri yüklemesini, doğrulamasını ve teslimini adım adım tanımlar. Her ana bölümün sonunda ✅ kontrol maddesi vardır — tamamı işaretlenmeden bir sonraki faza geçilmez.

---

## FAZ 0 — Satış Sonrası Keşif (kurulumdan ÖNCE, uzaktan yapılabilir)

Müşteriden yazılı olarak toplanacaklar:

| # | Bilgi | Neden |
|---|---|---|
| 0.1 | Sunucu var mı / temin edilecek mi, mevcutsa özellikleri | Faz 1 kararı |
| 0.2 | Ağ yapısı: sunucuya hangi istemciler, hangi VLAN'dan erişecek; DNS var mı | Erişim adresi (`http://fsc.packy.local` gibi) |
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

1. SQL Server kurulum medyasını başlat → **New SQL Server stand-alone installation**.
2. Feature seçimi: yalnız **Database Engine Services** (SSRS/SSAS gerekmez).
3. Instance: `MSSQLSERVER` (default) veya `FSCERP` adlı named instance.
4. **⚠️ KRİTİK — Collation:** Server Configuration → Collation sekmesinde **`Turkish_CI_AS`** seçilmelidir. *(Sistem kullanıcı adlarını Türkçe kurallarla büyük harfe çevirerek saklar; farklı collation'da oturum açma eşleşmesi bozulabilir. Bu adım kurulumdan sonra değiştirilemez — atlanırsa SQL yeniden kurulur.)*
5. Authentication: **Windows Authentication** (uygulama aynı sunucudaysa yeterli ve en güvenlisi). Uygulama ayrı sunucuda olacaksa Mixed Mode + güçlü `sa`-dışı SQL login.
6. Kurulum sonrası:
   - SQL Server Configuration Manager → TCP/IP **Enabled** (yalnız uygulama ayrı sunucudaysa gerekli).
   - Windows Güvenlik Duvarı: 1433 portu **yalnız intranet** (hiçbir koşulda internete açılmaz).
   - `ALTER SERVER CONFIGURATION` gerekmez; varsayılanlar yeterli.
7. Veritabanını oluştur (SSMS'te):
   ```sql
   CREATE DATABASE FscErpDb COLLATE Turkish_CI_AS;
   ```
8. Uygulama servis hesabına yetki (IIS AppPool kullanılıyorsa):
   ```sql
   CREATE LOGIN [IIS APPPOOL\FscErpAppPool] FROM WINDOWS;
   USE FscErpDb;
   CREATE USER [IIS APPPOOL\FscErpAppPool] FOR LOGIN [IIS APPPOOL\FscErpAppPool];
   ALTER ROLE db_owner ADD MEMBER [IIS APPPOOL\FscErpAppPool];
   ```

✅ `SELECT SERVERPROPERTY('Collation')` = `Turkish_CI_AS` · boş `FscErpDb` mevcut · servis hesabı bağlanabiliyor.

---

## FAZ 3 — Uygulama Dağıtımı

### 3.1 Dağıtım paketi (ARD tarafından önceden hazırlanır)
`dotnet publish -c Release` çıktısı + şu ekler:
- `appsettings.example.json` (şablon — gerçek bağlantı bilgisi paketle **gönderilmez**)
- `migration.sql` — **idempotent tam şema scripti** (`dotnet ef migrations script --idempotent` ile üretilir; müşteri sunucusunda .NET SDK/EF tooling gerektirmez)
- `tools/regression_suite.py` + gereksinimleri (Faz 8 doğrulaması; alternatif olarak ARD dizüstüsünden koşulur)
- Sürüm etiketi (git tag) ve SHA256 özet listesi

### 3.2 IIS ile kurulum (önerilen)
1. Windows Features → IIS + **ASP.NET Core Hosting Bundle** kur, `iisreset`.
2. Uygulama dosyalarını `C:\inetpub\FscErp\app` altına kopyala.
3. IIS Manager → Application Pool: `FscErpAppPool`, .NET CLR = **No Managed Code**, Start Mode = AlwaysRunning.
4. Site: `FSC-ERP`, fiziksel yol `C:\inetpub\FscErp\app`, binding `http :80` (host: `fsc.packy.local` — müşteri DNS'ine A kaydı ekletilir).
5. **HTTPS (intranet):** müşteri iç CA'sı varsa oradan sertifika; yoksa self-signed + istemcilere GPO ile güven dağıtımı, veya intranette HTTP kabulü müşteri IT politikasına yazılır.
6. `appsettings.json` oluştur (example'dan kopya) — yalnız şu alanlar düzenlenir:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=FscErpDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   },
   "FileStorage": { "Root": "D:\\FscErpData\\uploads" }
   ```
7. `D:\FscErpData\uploads` klasörünü oluştur, AppPool hesabına **Modify** yetkisi ver. *(Belge arşivi — irsaliye/fatura PDF'leri — uygulama klasörünün DIŞINDA tutulur ki sürüm güncellemesi dosyaları ezmesin.)*

### 3.3 Alternatif: Kestrel + Windows Service (IIS istenmiyorsa)
```
sc create FscErp binPath= "C:\Program Files\dotnet\dotnet.exe C:\FscErp\app\FSCTakip.WebUI.dll" start= auto
```
+ `ASPNETCORE_URLS=http://0.0.0.0:5000` ortam değişkeni (servis düzeyinde). Güvenlik duvarında 5000 yalnız intranet.

✅ Site ayakta, `http://fsc.packy.local` sunucudan ve bir istemciden açılıyor (henüz login beklenmez — DB boş).

---

## FAZ 4 — Veritabanı Şeması ve İlk Açılış

1. SSMS'te `migration.sql`'i `FscErpDb` üzerinde çalıştır (idempotent — hata alırsa tamamı durur, ARD'ye bildirilir).
2. Uygulamayı yeniden başlat (App Pool recycle). İlk açılışta sistem **otomatik** olarak:
   - Başlangıç referans verilerini yükler (depolar, torba tipleri, ürün grupları, makineler, kağıt tanımları, örnek kayıtlar),
   - **`admin` / `admin123`** tam yetkili kullanıcısını oluşturur.
3. `http://fsc.packy.local` → `admin`/`admin123` ile giriş **doğrulanır**.
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

1. **Regresyon paketi:** `python tools/regression_suite.py --base-url http://fsc.packy.local --company Packy` → **7/7 PASS** zorunlu (login, 24 sayfa taraması, kütle dengesi, negatif bakiye engeli, izlenebilirlik, kritik uyarılar, yazdırma).
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
| Login "kullanıcı bulunamadı" | DB collation `Turkish_CI_AS` mi (`SELECT SERVERPROPERTY('Collation')`) |
| ETL "Ürün bulunamadı" | Ürün kartında Dış Kod (Canias stok kodu) girilmiş mi |
| PDF belge açılmıyor | `FileStorage:Root` yolu ve AppPool yazma yetkisi |
| Sayfalar yavaşladı (2.000+ lot) | ARD'ye bildir — performans yol haritası (index/SQL View aşaması) devreye alınır |

## EK B — Kurulum Zaman Planı (tipik)

| Gün | İş |
|---|---|
| 1 (uzaktan) | Faz 0 keşif + paket hazırlığı |
| 2 (sahada) | Faz 1–4: altyapı, SQL, uygulama, ilk açılış |
| 3 (sahada) | Faz 5–6: kullanıcılar + ana veri (müşteriyle) |
| 4 (sahada/uzaktan) | Faz 7: ETL aktarımı + mutabakat |
| 5 (sahada) | Faz 8–10: doğrulama, yedek, eğitim, kabul |

*Toplam: ~1 adam-hafta (3 gün saha + 2 gün uzaktan). Geçmiş dönem verisi (Seçenek B) seçilirse +2-4 gün.*
