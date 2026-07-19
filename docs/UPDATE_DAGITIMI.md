# Müşteri Güncelleme Dağıtımı

> **Versiyon:** 1.0 · 2026-07-19

## Özet

Her sürümde **tek bir paket** üretilir (`FscErpUpdate-<versiyon>.zip`) ve **tek bir script**
(`update-engine.ps1`) ile uygulanır — müşterinin internete çıkışı olsun ya da olmasın, aynı
paket/script kullanılır. Fark sadece paketin müşteriye **nasıl ulaştığıdır**:

| Müşteri durumu | Paket nasıl ulaşır |
|---|---|
| İnternet erişimi var | Uygulama içi *Ayarlar → Güncellemeler* sayfasından tek tıkla indirilir |
| İnternet erişimi yok, uzak masaüstü (AnyDesk/RDP) var | ARD paketi kendi makinesinden indirir, uzak bağlantıyla müşteri sunucusuna kopyalar |
| Hiçbiri yok | Sahada USB/elden taşınır |

**Neden bu kadar basit:** `Program.cs`'te `Database:AutoMigrate=true` (müşteri kurulumlarının
varsayılanı) ile uygulama açılışta **kendi kendini migrate eder**. Yani dosyaları değiştirip
siteyi yeniden başlatmak yeterli — elle SQL script koşturmaya gerek yok (yalnız DBA'lı,
`AutoMigrate=false` olan istisnai kurulumlarda `apply-vm-update.ps1` + manuel SQL script yolu
kullanılır).

## Akış — ARD tarafı (her sürümde)

```powershell
cd installer
.\build-installer.ps1 -Version 1.1.0                # kurulum EXE + güncelleme zip üretir
.\build-installer.ps1 -Version 1.1.0 -PublishRelease # + GitHub Release'e yükler (internetli müşteriler için)
```

Çıktı: `installer\dist\FscErpUpdate-1.1.0.zip` — içinde `app\` (yayın çıktısı),
`version.txt`, `update-engine.ps1`.

## Akış — Müşteri tarafı (internetli, uygulama-içi)

1. *Ayarlar → Güncellemeler* sayfasını aç.
2. **Güncelleme Kontrol Et** → yeni sürüm varsa gösterilir.
3. **İndir** → paket `C:\FscErpUpdates\v<versiyon>\` altına indirilip AÇILIR (uygulanmaz).
4. Yönetici PowerShell aç, o klasöre git, `update-engine.ps1`'i çalıştır.

## Akış — Müşteri tarafı (internetsiz / AnyDesk)

1. ARD, `FscErpUpdate-<versiyon>.zip`'i AnyDesk/RDP ile müşteri sunucusuna kopyalar.
2. Zip bir klasöre açılır.
3. Yönetici PowerShell'de o klasöre gidip `update-engine.ps1` çalıştırılır.

`update-engine.ps1` sırasıyla: DB'yi yedekler → uygulama klasörünü yedekler → IIS'i durdurur
→ yeni dosyaları kopyalar (`appsettings.json` ve `license.lic` **dokunulmadan korunur**) →
IIS'i başlatır → siteyi doğrular (ilk istek migration+seed çalıştırır, buna göre bekler).

## GitHub Release altyapısı (internetli müşteriler için)

- **Repo:** `goktaso/FSCTakip-Releases` — **private**, SADECE derlenmiş `.zip` paketlerini
  barındırır. Kaynak kod hiçbir zaman bu repoya girmez.
- **Neden private:** Paket, derlenmiş DLL'lerin tamamını içerir — decompile edilebilir
  (ILSpy/dotPeek). Private tutmak, müşteri olmayan birinin (rakip, script kiddie) bu paketi
  bulup indirmesini engeller. Zaten her müşterinin kurulu kopyasında bu DLL'ler var — private
  repo sadece "internetten rastgele bulunabilir olma" riskini kapatır, müşteriye zaten
  verdiğiniz bilgiyi gizlemez.
- **Erişim:** Müşteri sunucusundaki `appsettings.json`'a, SADECE bu repoya salt-okunur
  (Contents: Read-only) erişimi olan bir **fine-grained GitHub Personal Access Token**
  girilir. Bu token:
  - Kaynak koda/git'e ASLA girmez (yalnız müşteriye özel appsettings.json'da durur — connection
    string gibi).
  - Sızarsa yalnız "update paketini indirebilir" riski taşır — repo yazma, diğer repolara
    erişim, hesap ele geçirme riski YOKTUR (fine-grained scope tek repo + tek izinle sınırlı).

### Fine-grained token nasıl üretilir (ARD tarafı, her müşteri için ayrı token önerilir)

1. https://github.com/settings/personal-access-tokens/new adresine git.
2. **Token name:** `FscErp-Update-<müşteri adı>` (hangi müşteride kullanıldığı belli olsun).
3. **Expiration:** 1 yıl (süre dolunca yenile — sızıntı riskini azaltır).
4. **Repository access:** *Only select repositories* → `FSCTakip-Releases` seç.
5. **Permissions → Repository permissions → Contents:** *Read-only*.
6. Diğer tüm izinler *No access* kalsın.
7. **Generate token** → oluşan `github_pat_...` değerini kopyala.
8. Müşteri sunucusundaki `appsettings.json` → `UpdateCheck.Token` alanına yapıştır,
   `UpdateCheck.Enabled` değerini `true` yap.

### Yayınlama (ARD tarafı)

`build-installer.ps1 -PublishRelease` verildiğinde, GitHub CLI (`gh`, zaten `goktaso`
hesabıyla giriş yapılı) ile `goktaso/FSCTakip-Releases` reposuna otomatik bir Release
(`v<versiyon>` etiketiyle) açılıp zip paketi yüklenir.

## Uygulama içi mekanizma (kod tarafı)

- `FSCTakip.WebUI/Services/UpdateCheckService.cs` — GitHub API'den `releases/latest` çeker,
  sürüm karşılaştırır, private repo asset'ini `Accept: application/octet-stream` ile indirir.
- `FSCTakip.WebUI/Controllers/UpdateController.cs` — yalnız admin (`/Update/Index`).
- **Bilinçli sınır:** Bu servis migration'ı OTOMATİK UYGULAMAZ, sadece indirip açar. Şema
  değişikliğinin sessizce, onaysız uygulanması riskli görüldüğü için (CLAUDE.md güvenlik
  kuralı) gerçek "uygulama" adımı hep bir insanın `update-engine.ps1`'i elle çalıştırmasıyla
  olur.
