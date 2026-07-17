# Kurulum Paketi — FscErpSetup.exe

Müşteri sunucusunda çift tıkla çalışan, tam çevrimdışı kurulum paketi.
`docs/ThirdPartyKurulum.md` runbook'unun **FAZ 1–4**'ünü otomatikleştirir (~1 gün saha işi → ~15 dakika).

Kalan fazlar (FAZ 0 keşif, FAZ 5–6 ana veri, FAZ 7 ETL, FAZ 8–10 doğrulama/eğitim/kabul)
danışmanlık işidir ve pakete dahil değildir.

---

## Dosyalar

| Dosya | Nerede çalışır | Ne yapar |
|---|---|---|
| `build-installer.ps1` | **ARD makinesinde** (internet var) | publish + ön koşulları indir/doğrula + EXE derle |
| `FscErpSetup.iss` | derleme sırasında | Inno Setup sihirbazı (ince sarmalayıcı) |
| `install-engine.ps1` | **müşteri sunucusunda** | asıl kurulum: IIS, SQL, DB, site, firewall, doğrulama |

Tasarım ilkesi: **karmaşıklık ARD tarafında, sahada sıfır sürpriz.**
Ön koşullar burada indirilip doğrulanır, sahaya tek EXE gider — müşteri sunucusunun
internete çıkması gerekmez (ERP sunucusunda zaten istenmeyen bir şeydir).

---

## Paket üretme

```powershell
cd installer
.\build-installer.ps1 -Version 1.0.0
```

Çıktı: `installer\dist\FscErpSetup-1.0.0.exe` (~600 MB) + `.sha256.txt` teslim kaydı.

### Gereksinimler
- .NET 8 SDK
- [Inno Setup 6](https://jrsoftware.org/isdl.php) — varsayılan konuma kurmak yeterli

### İlk derlemede: ön koşul hash'lerini sabitleme

`$Prereqs` listesindeki `Sha256` değerleri başlangıçta **boştur**. İlk çalıştırmada script
dosyaları indirir, hash'lerini ekrana basar ve **bilerek durur**. Yapılacaklar:

1. İndirilen dosyaların imzasını doğrula:
   ```powershell
   Get-AuthenticodeSignature .\build\prereq\SQLEXPR_x64_ENU.exe | Format-List Status, SignerCertificate
   ```
   `Status` = `Valid` ve imzalayan Microsoft olmalı.
2. Ekrandaki `Sha256` değerlerini `build-installer.ps1` içindeki `$Prereqs` listesine yapıştır.
3. Script'i tekrar çalıştır.

Bu kapı bilinçlidir: müşteri sunucusuna kuracağın yükleyicinin ne olduğunu bir kere
insan gözüyle onaylaman gerekir. **Tahmini/uydurma hash yazma** — doğrulama varmış
görüntüsü verip hiçbir şey doğrulamaz.

Microsoft yeni yama sürümü yayınlayıp içerik değişirse script yine durur. Yeni dosyayı
aynı şekilde doğrulayıp hash'i güncelle.

---

## ⚠️ Dağıtımdan önce: SQL Express lisans teyidi

SQL Server Express'i kendi kurulum paketine **gömmeden önce**, kullandığın sürümün
yeniden dağıtım (redistribution) şartlarını teyit et. Microsoft genelde ISV'lerin
Express'i uygulamalarıyla dağıtmasına izin verir (`/IACCEPTSQLSERVERLICENSETERMS`
bunun içindir), ancak şartlar sürümden sürüme değişebilir ve bu bir hukuki konudur —
script'in çalışıyor olması dağıtım hakkın olduğu anlamına gelmez.

**Sorun çıkarsa geri dönüş yolu hazır:** ön koşulları EXE'ye gömmek yerine yanında ayrı
bir klasörde ship et. Motor bunu zaten destekliyor:

```powershell
.\install-engine.ps1 -SourcePath .\app -PrereqPath D:\prereq -Port 80
```

Bu durumda müşteriye USB/klasör olarak `FscErpSetup.exe` + `prereq\` gider.

---

## Sahada kurulum

Sunucuya EXE'yi kopyala, çift tıkla. Sihirbaz sorar:

1. **SQL:** mevcut örnek (registry'den otomatik bulunur) veya SQL Express kur
2. **Port:** 80 doluysa otomatik 8080 önerilir

Kurulum ~10–15 dakika. Bitişte ekranda ve `<VeriKlasörü>\kurulum-raporu.txt` içinde:
erişim adresi, `admin`/`admin123`, **sunucu kimlik kodu**, deneme bitiş tarihi.

### Kurulumdan sonra
1. Admin parolasını **hemen** değiştir.
2. Kimlik kodunu al → `python tools/license_gen.py --private-key <özel-anahtar> --licensed-to "<Firma>" --machine <kod>` → `license.lic`'i müşteriye gönder → `/License/Status`'tan yüklesin.
3. Ağdaki **başka bir bilgisayardan** adresi aç — sunucunun kendisinden yapılan test güvenlik duvarı eksiğini yakalamaz.

---

## Lisans akışı

Kurulum lisans beklemez: sistem **30 gün** tam özellikli çalışır (`LicenseState.Trial`),
her sayfada kalan gün bandı görünür. Süre dolunca erişim kapanır, **veriler korunur** —
lisans dosyası yüklenince kaldığı yerden devam eder.

Deneme başlangıcı iki yerden okunur ve **erken olanı** esas alınır:
- `C:\ProgramData\ArdFscErp\.init` (kurulumda yazılır)
- veritabanının `create_date`'i

İkisi birden yok edilmeden deneme sıfırlanamaz; veritabanını silmek müşterinin tüm ERP
verisini kaybetmesi demektir. Mutlak koruma değil — bilinçli olarak kabul edilmiş sınır.

---

## Güncelleme (FAZ 3.6)

Yeni sürümün EXE'sini çalıştırmak yeterli. `appsettings.json` ve `license.lic`
**korunur** (robocopy `/XF`), uygulama dosyaları yenilenir, şema ilk açılışta
`Database.Migrate()` ile güncellenir.

## Kaldırma

Denetim Masası → Programlar. Veritabanı ve belge arşivinin silinip silinmeyeceği sorulur;
**varsayılan: korunur.** (FSC denetimi 5 yıl geriye belge isteyebilir.)

---

## Bakım notları

- **PowerShell 5.1 + Türkçe = UTF-8 BOM zorunlu.** BOM'suz kaydedilen script'te PS 5.1
  dosyayı ANSI sanar, Türkçe karakterler bozulur ve script parse bile edilemez.
  Bu dosyaları düzenledikten sonra BOM'un durduğunu doğrula.
- **`$args` kullanma** — PowerShell'de ayrılmış otomatik değişkendir.
- `install-engine.ps1` içindeki `Get-MachineKey`, C# `LicenseService.GetMachineKey()`
  ile **aynı** kodu üretmek zorundadır (SHA256(MachineGuid) ilk 16 hex, küçük harf).
  Biri değişirse diğeri de değişmeli — yoksa rapordaki kod yanlış lisans üretilmesine
  yol açar.
- Adım sırası keyfi değildir: **IIS → Hosting Bundle** (ters sıra HTTP 500.30),
  **AppPool → SQL login** (login AppPool hesabı var olmadan açılamaz),
  **CREATE DATABASE → Migrate** (collation `Turkish_CI_AS` sonradan değiştirilemez).

---

## Doğrulama — temiz VM zorunlu

Kirli makinede yeşil sonuç kanıt değildir. Her senaryo öncesi snapshot'a dönülür:

| # | Senaryo | Başarı ölçütü |
|---|---|---|
| 1 | Temiz VM, hiç ön koşul yok | ~15 dk'da **başka makineden** erişim, admin girişi, Dashboard |
| 2 | Collation | `SELECT DATABASEPROPERTYEX('FscErpDb','Collation')` = `Turkish_CI_AS` |
| 3 | Şema | `SELECT COUNT(*) FROM __EFMigrationsHistory` = 34 |
| 4 | Deneme | Band görünüyor, gün doğru; `license.lic` yüklenince "Lisans Geçerli" |
| 5 | Regresyon | `python tools/regression_suite.py --base-url http://<vm-ip>:<port>` → 7/7 |
| 6 | Mevcut SQL | Önce elle SQL kur → EXE tespit edip kullanıyor mu (FAZ 2B) |
| 7 | Güncelleme | EXE'yi ikinci kez çalıştır → `appsettings.json` + `license.lic` değişmedi |
| 8 | Kaldırma | DB ve uploads duruyor |
