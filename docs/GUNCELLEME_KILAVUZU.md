# FSC Takip ERP — Güncelleme El Kitabı

**Doküman No:** ARD-KUR-002 · **Sürüm:** 1.0 · **Tarih:** 17.07.2026
**Hazırlayan:** ARD Sistem ve Danışmanlık

> Bu kitap, müşteride **çalışan** bir FSC Takip ERP kurulumuna yeni sürüm (hata düzeltmesi,
> güvenlik yaması, yeni özellik) uygularken **mevcut veriyi ve yapıyı bozmadan** nasıl
> ilerleneceğini anlatır. Kurulumun kendisi için ayrı doküman: `ThirdPartyKurulum.md`.

---

## 1. Temel Mantık — Neden Mevcut Yapı Bozulmaz?

**Benzetme:** Uygulama bir binadır. **Kod** binanın mobilyası (değiştirilebilir), **veri**
binadaki insanlardır (yerinde kalır). Güncelleme mobilyayı yeniler, insanları tahliye etmez.

Üç parça fiziksel olarak **ayrı yerlerde** durur ve güncellemede farklı davranır:

| Parça | Nerede | Güncellemede |
|---|---|---|
| **Kod** (DLL, .cshtml sayfalar, js/css) | `C:\inetpub\FscErp\app` | **Yenisiyle değiştirilir** |
| **Veritabanı** (tüm kayıtlar) | SQL Server `FscErpDb` | **Dokunulmaz** (yalnız şema eklenir, bkz. §3) |
| **Belgeler** (irsaliye/fatura PDF) | `D:\FscErpData\uploads` | **Dokunulmaz** (uygulama klasörünün DIŞINDA) |
| **Ayarlar** | `app\appsettings.json` | **Korunur** (üzerine yazılmaz) |
| **Lisans** | `app\license.lic` | **Korunur** (üzerine yazılmaz) |

Kod ile veri **ayrı yerlerde** durduğu için kodu değiştirmek veriye dokunmaz. İşin sırrı budur.

---

## 2. Basit Örnek — Bir Güncelleme Adım Adım

**Senaryo:** Bugün yaptığımız güvenlik düzeltmelerini (güçlü parola, XSS kapatma vb.)
müşteriye göndereceğiz.

### ARD tarafında (bizim ofis)
1. Değişiklikler koda işlendi, test edildi.
2. `installer\build-installer.ps1` çalıştırılır → yeni `FscErpSetup-1.x.exe` üretilir.

### Müşteri sunucusunda
1. **Önce yedek al** (ZORUNLU — bkz. §4).
2. Yeni `FscErpSetup.exe`'yi çift tıkla, yönetici olarak çalıştır.
3. Kurulum motoru **idempotent**'tir — sistemin zaten kurulu olduğunu görür ve
   **güncelleme** moduna geçer:
   - IIS uygulama havuzunu durdurur
   - `app` klasöründeki DLL/sayfaları yenisiyle değiştirir
   - **`appsettings.json` ve `license.lic`'e dokunmaz** (robocopy `/XF` ile hariç tutulur)
   - Havuzu tekrar başlatır
4. Uygulama ilk açılışta **bekleyen veritabanı değişikliklerini otomatik uygular** (bkz. §3).
5. Bir sayfa gezilip hatasız açıldığı doğrulanır.

**Sonuç:** Yeni kod devrede, tüm müşteri verisi ve belgeleri yerinde, lisans geçerli.

---

## 3. Veritabanı Şema Değişikliği — "Yapılan İşler Defteri"

En çok kafa karıştıran konu budur: *"Yeni sürüm veritabanına yeni kolon ekliyorsa, mevcut
kayıtlar bozulmaz mı?"* Hayır — ve nedeni şu mekanizmadır.

### Migration nedir?
Her veritabanı değişikliği (yeni kolon, yeni tablo, yeni index) numaralı bir **migration**
dosyasıdır. Örnekler (bu projeden gerçek):
- `20260704093714_AddCompanySettings` — şirket bilgileri tablosu eklendi
- `20260717183923_AddMustChangePassword` — kullanıcıya "şifre değiştir" kolonu eklendi
- `20260717xxxxxx_AddRowVersionAndDecimalPrecision` — eşzamanlılık damgası + ondalık hassasiyet

### "Defter" nasıl çalışır?
Veritabanında gizli bir tablo vardır: **`__EFMigrationsHistory`**. Bu, "hangi migration'lar
uygulandı" defteridir. Uygulama her açılışta şunu yapar (`Program.cs` içinde):

```
1. Deftere bak: hangi migration'lar zaten uygulanmış?
2. Kodda hangi migration'lar var?
3. FARKI hesapla: yalnız EKSİK olanları, SIRAYLA uygula.
4. Uygulananları deftere yaz.
```

**Somut örnek:**
- Müşteri veritabanında 1–34 arası migration uygulanmış (deftere yazılı).
- Yeni sürüm 35. migration'ı (`AddMustChangePassword`) içeriyor.
- Açılışta uygulama: "34'e kadar var, 35 eksik" → **yalnız 35'i çalıştırır**:
  `ALTER TABLE AppUsers ADD MustChangePassword bit NOT NULL DEFAULT 0`
- Bu komut **yeni bir kolon EKLER**, mevcut satırlara varsayılan (`0`) verir. Hiçbir
  kaydı silmez, değiştirmez, bozmaz.

### Neden güvenli?
Migration'lar tasarım gereği **eklemelidir (additive)**:
- Yeni kolon ekleme → varsayılan değerle, mevcut veri etkilenmez ✓
- Yeni tablo/index ekleme → mevcut veriye dokunmaz ✓
- Kolon genişletme (ör. `decimal(18,2)` → `decimal(18,4)`) → veri korunur, sadece daha
  fazla ondalık yer açılır ✓

**Riskli olabilecek tek durum:** kolon SİLME veya DARALTMA (ör. metin alanını kısaltma).
Bunlar veri kaybına yol açabilir. Bizim migration'larımız bunu yapmaz; ama her güncelleme
öncesi **yedek almak** (§4) bu riski de sıfırlar.

### Otomatik mi, elle mi?
- **Varsayılan (küçük müşteri):** Otomatik. `appsettings.json`'da `"Database:AutoMigrate": true`.
  Uygulama açılışta bekleyen migration'ları kendi uygular. Ek işlem gerekmez.
- **DBA'lı kurumsal müşteri:** `"Database:AutoMigrate": false` yapılır. ARD, migration'ları
  SQL script (`migration.sql`) olarak üretir, müşteri DBA'sı bakım penceresinde elle çalıştırır.

---

## 4. Güncelleme Öncesi — Zorunlu Yedek

Güncelleme %99 sorunsuzdur, ama şema değişen her güncellemede **önce yedek** kuralı
tartışmasızdır. İki şey yedeklenir:

1. **Veritabanı** (SSMS → FscErpDb → sağ tık → Tasks → Back Up):
   ```sql
   BACKUP DATABASE FscErpDb TO DISK = 'D:\FscErpData\backup\FscErpDb_guncelleme_oncesi.bak' WITH INIT;
   ```
2. **Belge klasörü** (`D:\FscErpData\uploads`) — dosya kopyası yeterli (nadiren değişir ama garanti).

`appsettings.json` ve `license.lic` güncellemede zaten korunur, ama bir kopyalarını almak
zarar vermez.

**Neden:** Bir migration beklenmedik biçimde patlarsa (ör. eski veride tutarsızlık), yedekten
5 dakikada geri dönülür — "yedek var" değil, "yedek DÖNÜYOR" güvencesi.

---

## 5. Güncelleme Adımları — Kontrol Listesi

```
[ ] 1. Müşteriye kısa bakım penceresi bildir (uygulama ~5 dk kapanacak).
[ ] 2. Veritabanı yedeği al (§4).
[ ] 3. uploads klasörü kopyası al (§4).
[ ] 4. Yeni FscErpSetup.exe'yi sunucuya kopyala.
[ ] 5. Yönetici olarak çalıştır → güncelleme modu otomatik.
[ ] 6. Kurulum bitince siteyi aç, bir-iki sayfa gez (Dashboard, bir liste sayfası).
[ ] 7. Yeni özelliği/düzeltmeyi doğrula.
[ ] 8. Sorun yoksa müşteriye "güncelleme tamam" bildir.
```

### Sorun çıkarsa (nadir)
- Site açılmıyor / HTTP 500.30 → `app\logs\stdout_*.log` en yeni dosyaya bak
  (bkz. ThirdPartyKurulum EK A).
- Migration patladı → yedekten geri dön (§4), logu ARD'ye ilet, düzeltilmiş paket beklenir.
- **Geri dönüş her zaman mümkün:** DB yedeği + eski `app` klasörü kopyası = tam eski hale dönüş.

---

## 6. Sürüm Numaralama ve İzlenebilirlik

- Her paketin bir sürüm etiketi (`FscErpSetup-1.2.exe`) ve SHA256 özeti vardır
  (`build-installer.ps1` üretir, `dist\*.sha256.txt`).
- Müşteride hangi sürümün kurulu olduğu sol menü altındaki **"FSC ERP v…"** satırında görünür.
- Hangi migration'ların uygulandığı: `SELECT * FROM __EFMigrationsHistory` ile görülür.
- ARD teslim kaydı: her müşteri için "hangi tarihte hangi sürüm + SHA256" not edilir.

---

## 7. Sık Sorulanlar

**S: Güncelleme müşteri verisini siler mi?**
C: Hayır. Kod ile veri ayrı yerlerde. Güncelleme yalnız kodu değiştirir; şema değişikliği
yalnız EKLER (kolon/tablo), mevcut satırlara dokunmaz.

**S: Müşterinin girdiği firma logosu / ayarları kaybolur mu?**
C: Hayır. Logo veritabanında + veri klasöründe, ayarlar `appsettings.json`'da — üçü de
güncellemede korunur.

**S: Lisans yeniden mi gerekir?**
C: Hayır. `license.lic` güncellemede hiç ezilmez. Aynı sunucu, aynı lisans.

**S: Ya internet yoksa?**
C: Güncelleme paketi de kurulum gibi tam çevrimdışıdır (ön koşullar zaten kuruludur,
sadece uygulama dosyaları değişir). İnternet gerekmez.

**S: Kaç sürüm atlayabilirim? (ör. v1.0'dan v1.5'e)**
C: Sınırsız. Migration defteri "eksik olanları sırayla" uygular — aradaki tüm migration'lar
tek açılışta sırayla çalışır. Ara sürümleri tek tek kurmaya gerek yok.

---

**Özet:** Güncelleme = "yeni EXE'yi çalıştır". Kod değişir, veri/belge/lisans/ayar korunur,
şema yalnız eklenerek büyür. Önce yedek al, sonra çalıştır, bir sayfa gez — bitti.
