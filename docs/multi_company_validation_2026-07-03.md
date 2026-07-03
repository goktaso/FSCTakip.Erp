# FSC Takip ERP — Çoklu Şirket Doğrulama ve Satışa Hazırlık Raporu

**Tarih:** 03.07.2026
**Amaç:** Uygulamanın ACORE'a özgü gizli varsayımlar olmadan, farklı şirket verileriyle (farklı ürün kodları, makineler, tedarikçiler, BOM yapıları, FSC iddia karışımları) ve farklı onboarding yollarıyla (otomatik kurulum, ETL/Excel aktarımı) doğru çalıştığını kanıtlamak.
**Yöntem:** Aynı kod tabanı (tek satır değişmeden), 4 ayrı sıfırdan-kurulmuş veritabanına karşı, gerçek deployment modunda (Kestrel, `dotnet publish` çıktısı) çalıştırıldı; her şirkette 7 kontrollük regresyon paketi + 19 birim test.

---

## 1. Sonuç Özeti

| Şirket | Onboarding Yolu | Veri Profili | Regresyon |
|---|---|---|---|
| TestCo_Seed | Otomatik kurulum (DbSeeder) | ACORE'dan farklı ürün/makine/tedarikçi adları | **7/7 PASS** |
| TestCo_Etl | **Gerçek ETL/Excel yolu** — "yabancı ERP export'u" formatında 2 dosya | 3 yabancı tedarikçi + 2 lot/4 bobin ETL ile içeri alındı (3+2 insert, 0 hata) | **7/7 PASS** |
| TestCo_Edge | SQL ile bilinçli uç senaryolar | Farklı grup kod aralığı (5000-5999), süresi dolmuş sertifika, sertifikasız tedarikçiden FSC iddialı lot, 2 seviyeli dönüşüm zinciri, çok-günlü iş emri, kısmi sevk | **7/7 PASS** |
| TestCo_Scale | Hacim (302 lot / 607 bobin / 153 iş emri / 94 satış / 547 stok hareketi) | Performans ölçümlü | **7/7 PASS** |

**Birim testler:** 19/19 PASS (ActualQuantity formülü, kütle dengesi, stok yeterlilik kuralı, sertifikasız-tedarikçi tespiti).
**ACORE canlı verisi:** Program boyunca hiç dokunulmadı, kapanışta doğrulandı (16 iş emri / 8 satış / 3.533.750 adet üretim — değişmemiş).

### Regresyon Paketi Kontrolleri (her şirkette aynı)
P1 Login · P2 24 sayfalık tam tarama (HTTP 200 + exception yok) · P3 Denetim Özeti kütle dengesi · P4 Fazla sevkiyat reddi (negatif bakiye engeli, canlı) · P5 Tam İzlenebilirlik zinciri · P6 Uyarı Paneli + kritik özet endpoint'i · P7 İş Emri Formu / Sevk / Fatura print sayfaları.

Paket kalıcı olarak repoda: `tools/regression_suite.py` — yeni müşteri kurulumunda aynı komutla tekrar koşturulabilir.

---

## 2. Program Sırasında Bulunan ve Düzeltilen Gerçek Hatalar

Bu programın asıl değeri: **sıfır kurulum daha önce hiç denenmemişti** ve ilk denemede art arda gerçek satış engelleyiciler çıktı. Tamamı düzeltildi ve doğrulandı:

| # | Bulgu | Etki | Düzeltme |
|---|---|---|---|
| B1 | `AddExternalCodes` migration'ının Designer.cs'i eksik — zincir sıfır DB'de kırılıyor | Yeni müşteri kurulumu **imkânsızdı** | İdempotent yama migration (önceki oturumda, `927082c`) |
| B2 | `RenamePartiNoAddSerialLotNo` + `FscLotSupplierIdNullable` migration'ları da kayıtsız (Designer yok) — sıfır DB'de `PartiNo` kolonu yok, `SupplierId` NOT NULL | Uygulama sıfır DB'de **açılışta çöküyordu** | `20260526000003_FixUnregisteredManualMigrations` — idempotent, ACORE'da no-op |
| B3 | `StockMovements.QuantityKg` kolonu hiçbir migration'da yok (SSMS ile elle eklenmiş) | Seeder sıfır DB'de çöküyordu | Aynı yama migration'a eklendi; **tam şema diff'i** ile başka boşluk kalmadığı doğrulandı |
| B4 | Hiçbir mekanizma ilk admin kullanıcısını oluşturmuyordu | Sıfır kurulumda **login imkânsızdı** | `DbSeeder` artık AppUsers boşsa `admin/admin123` oluşturuyor (guard'dan bağımsız) |

## 3. Bilinen Küçük Sapmalar (düzeltme gerektirmedi, kayıt için)

- **Kolon genişlikleri:** 25 nvarchar kolonu sıfır kurulumda ACORE'dakinden daha geniş (max vs 100) — veri kaybı riski yok, davranış farkı yok.
- **`Units` tablosu:** yalnız ACORE'da var, kod hiç kullanmıyor, 0 satır — ölü tablo, temizlenebilir.
- **Referans FSC tipleri:** sıfır kurulum 2 tip seed'liyor (FSC-100, FSC-MIX); ACORE'da elle 5'e çıkarılmış. ETL'nin fuzzy eşleşmesi bunu tolere ediyor (doğrulandı) — ama yeni müşteriye "FSC'siz" tipi dahil tam liste öneriliyor (Tanımlamalar > FSC Tipleri'nden 1 dk'lık iş, veya ileride HasData genişletilir).

## 4. Performans (TestCo_Scale, 302 lot)

En yavaş sayfalar: Stok Hareketleri 1,5 sn · Hammadde Girişi 1,4 sn · geri kalan tümü < 1,3 sn. Eşik (5 sn) çok rahat sağlanıyor. Mevcut performans yol haritası (500+ lot için index/SQL View aşamaları — `docs/PERFORMANCE_ROADMAP.md`) geçerliliğini koruyor.

---

## 5. Hazırlık Değerlendirmesi (dürüst çerçeve)

### Kanıtlanan
- Sıfır kurulum → migration → seed → login → tam iş akışı → FSC denetim raporları: **uçtan uca, 4 farklı veri profili ile çalışıyor.**
- ETL/Excel onboarding yolu gerçek dosyalarla çalışıyor; yabancı formattaki tedarikçi/lot verisi doğru tablolara iniyor.
- Kritik iş kuralları (negatif bakiye engeli, sertifikasız tedarikçi tespiti, kütle dengesi, çok-günlü üretim hesabı) hem birim testte hem 4 canlı ortamda doğrulandı.
- ACORE'a özgü tespit edilen **hiçbir gizli varsayım kalmadı** (bulunanların tümü düzeltildi).

### Test EDİLMEYEN / Açık Riskler (satış öncesi bilinçli karar konuları)
1. **SQL Server collation bağımlılığı:** kullanıcı adları tr-TR uppercase'e çevriliyor (`admin`→`ADMİN`); Turkish collation'da login çalışıyor (doğrulandı), ama **Latin1/farklı collation'lu bir sunucuda login kırılabilir** — kurulum dokümanına "Turkish_CI_AS collation" şartı yazılmalı veya karşılaştırma kültür-bağımsız hâle getirilmeli.
2. **Eşzamanlı çok kullanıcı yükü** test edilmedi (tek kullanıcılı akışlar test edildi).
3. **Gerçek Logo/Canias canlı DB bağlantısı** test edilmedi — Excel/ETL yolu test edildi (tasarım gereği birincil yol); Netsis doğrudan-senkron modülü bu programda kapsam dışıydı.
4. **CAR/denetim geçmişi modülü** hâlâ yok (03.07 denetim raporundaki Gözlem #2) — sertifikasyon kuruluşları sorabilir.
5. Tek makine/tek OS (Windows + SQL Server 2019+) üzerinde test edildi.

### Sonuç
Sistem, **"A firması kendi verisiyle, B firması ETL ile, C firması uç senaryolarla"** kurgusunun tamamını sistemsel hata olmadan geçti. "Hiçbir firmada asla sorun çıkmaz" garantisi hiçbir yazılım için verilemez; verilebilecek en güçlü ifade şudur: **test edilen tüm kurulum yolları ve veri profilleri için sistem doğru ve eksiksiz çalışıyor; bilinen açık riskler yukarıda 5 maddede sınırlandırılmış ve yönetilebilir durumda.** Satış öncesi asgari tavsiye: madde 1 (collation) kurulum şartnamesine yazılsın, madde 4 (CAR modülü) ilk sürüm yol haritasına alınsın.

---

*Kanıt dosyaları: `tools/regression_suite.py`, şirket başına JSON sonuçları (oturum scratchpad), `FSCTakip.Tests` (19 test), migration yamaları (`20260524120001`, `20260526000003`).*
