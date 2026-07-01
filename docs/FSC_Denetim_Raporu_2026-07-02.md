# FSC Takip ERP — Kütle Dengesi ve Sayfa Denetim Raporu

**Tarih:** 02.07.2026
**Hazırlayan:** Claude Code (otomatik veri analizi — SQL sorguları + canlı sayfa kontrolü)
**Kapsam:** Hammadde girişinden satışa kadar tüm zincirin sayfa sayfa doğrulanması, kütle dengesi hesabı, bulunan tutarsızlıklar ve önerilen düzeltmeler.

---

## 1. Yönetici Özeti

Sistemdeki tüm modüller (Hammadde Girişi, Dönüşüm, Üretim, Satış, Stok, Fire/İmha) canlı olarak kontrol edildi. **10 sayfa** doğrudan tarayıcı üzerinden çekildi, **hiçbiri boş/hatalı dönmedi** (tümü HTTP 200, veri satırları mevcut). Kütle dengesi SQL sorgularıyla ayrıştırıldı.

**Bulunan en kritik sorun:** İş Emri bazında üretim girişi (StockMovement) ile İş Emri gerçek üretim miktarı (WorkOrder.ActualQuantity) arasında **5 iş emrinde** sayısal tutarsızlık var (bkz. Bölüm 4). Bu, denetimde "üretilen ile stoğa girene" arasında rakam farkı olarak karşınıza çıkabilir — denetim öncesi düzeltilmesi önerilir.

Diğer tüm modüller (hammadde girişi, satış, fire, imha) rakamsal olarak tutarlı.

---

## 2. Denetçiye Gösterim Sırası

FSC Chain of Custody (CoC) denetiminde malzeme akışını **fiziksel akışla aynı sırada** göstermek en ikna edici yöntemdir:

| # | Aşama | Sayfa (Route) | Ne Gösterir |
|---|-------|----------------|-------------|
| 1 | **Tedarikçi FSC Durumu** | `/Reports/SupplierFsc` | Hammaddeyi veren tedarikçilerin FSC sertifika geçerliliği |
| 2 | **Hammadde Girişi** | `/Purchase/Index` → `/Purchase/Detail/{id}` | Lot/bobin bazlı hammadde girişi, irsaliye/fatura PDF'leri, FSC tipi |
| 3 | **Hammadde Stoğu (Bobin Bazlı)** | `/Stock/RawMaterial` | Bobin bazında kalan miktar, hangi lottan geldiği |
| 4 | **Dönüşüm (YM)** | `/Conversion/Index` | Hammaddeden yarı mamüle dönüşüm, kaynak-hedef bobin ilişkisi |
| 5 | **İş Emirleri (Üretim)** | `/Production/Index` → `/Production/Detail/{id}` | Plan/gerçek üretim, hangi bobinlerden ne kadar tüketildiği |
| 6 | **Üretim Fişi** | `/Production/Detail/{id}` → Üretim Fişi Yazdır | İş emri bazlı yazdırılabilir form (yeni eklendi) |
| 7 | **Fire Raporu** | `/Production/WasteReport` | Üretim firesi, fire oranı |
| 8 | **İmha Kayıtları** | `/Production/WasteManagement` | Elden çıkarılan/imha edilen malzeme |
| 9 | **Satış Siparişleri** | `/Sales/Index` → `/Sales/Detail/{id}` | Sevkiyat, hangi iş emrinden satıldığı |
| 10 | **Sevk İrsaliyesi / Fatura** | `/Sales/Print/{id}`, `/Sales/PrintInvoice/{id}` | Basılı sevkiyat/fatura belgesi (FSC beyanı dahil) |
| 11 | **Tam İzlenebilirlik** | `/Reports/Traceability/{siparisId}` | **Tek ekranda satıştan hammadde lotuna kadar tüm zincir** — denetçiye en etkili sayfa budur |
| 12 | **FSC Chain of Custody Raporu** | `/Reports/ChainOfCustody` | Toplu CoC özeti |
| 13 | **Lot Takip Raporu** | `/Reports/LotTrace` | Lot bazlı arama/izleme |
| 14 | **Denetim Özet Raporu** | `/Reports/AuditReport` | Sistemin kendi ürettiği özet denetim raporu |
| 15 | **Stok Durumu (kalan bakiye)** | `/Stock/Index`, `/Stock/AnaOzet`, `/Stock/AdminStock` | Dönem sonu / yıl sonu kalan bakiyeler |

**Pratik öneri:** Denetçiyle önce **11. Tam İzlenebilirlik** sayfasını açıp tek bir satışı uçtan uca gösterin (en somut ispat), sonra yukarıdaki sırayla detaya inin.

---

## 3. Kütle Dengesi — Doğrulanmış Rakamlar (SQL'den)

### 3.1 Hammadde / Yarı Mamül / Burgu Sap — Giriş vs Kalan

| Grup | Lot Sayısı | Bobin Sayısı | Toplam Giriş (kg) | Toplam Kalan (kg) |
|---|---|---|---|---|
| HAMMADDE | 32 | 61 | 232.699,08 | 70.183,08 |
| YARI MAMUL | 15 | 15 | 23.680,00 | 1.751,00 |
| BURGU SAP | 4 | 5 | 3.907,45 | 1.563,45 |

### 3.2 Üretimde Tüketim (ProductionDetail bazlı)

| Kaynak Grup | Tüketilen (kg) | Fire (kg) |
|---|---|---|
| HAMMADDE | 144.943,00 | 6.415,00 |
| YARI MAMUL | 20.813,00 | 1.116,00 |
| BURGU SAP | 2.344,00 | 0,00 |

### 3.3 Stok Hareketi Tipine Göre Toplamlar

| Tip | Hareket Sayısı | Toplam Miktar |
|---|---|---|
| 1 — Üretimden Girişi (Mamul) | 31 | 3.971.630,00 adet |
| 3 — Satış Sevkiyatı | 16 | 1.927.250,00 adet |
| 4 — Tedarikçiden Hammadde Girişi | 36 | 903.912,30 kg |
| 5 — Üretim Tüketimi | 80 | 183.698,00 kg |

### 3.4 Üretim ↔ Satış Dengesi

- Tamamlanmış iş emri sayısı: **16**, toplam gerçek üretim: **3.533.750 adet**
- Satış (tüm sevkiyatlar): **8 sipariş, 16 kalem, 1.927.250 adet** — tek müşteri: ACORE DIŞ TİCARET LTD.ŞTİ.
- İçe aktarılan 10 iş emri için (IE2026-001…010) sevkiyat = üretilen miktar birebir eşit, **kalan = 0**, hiçbir iş emrinde negatif bakiye yok. ✅
- Kalan 6 tamamlanmış iş emri (IE2026-011…016, toplam 1.606.500 adet) henüz satılmamış — bu normal, stokta bekliyor demektir.

### 3.5 Fire / İmha

- Fire Raporu sayfası: toplam fire **7.531,0 kg**, fire oranı **%4,54**, 57 kayıt.
- İmha Kayıtları sayfası ile aynı rakam (7.531,0 kg / 57 kayıt) — **tutarlı**. ✅

---

## 4. ⚠️ Bulunan Tutarsızlık — Üretim Girişi (StockMovement) vs İş Emri Gerçek Üretimi

Her tamamlanmış iş emrinin **bir kez** "Üretimden Depoya Giriş" (StockMovement Tip=1) hareketi oluşturması ve bu hareketin miktarının `WorkOrder.ActualQuantity` ile **birebir aynı** olması beklenir. Kontrol sonucu:

| İş Emri | Gerçek Üretim (Adet) | Stok Hareketi Toplamı (Adet) | Fark | Durum |
|---|---:|---:|---:|---|
| IE2026-001 | 281.600 | 563.200 | +281.600 (tam 2 kat) | ❌ Hatalı |
| IE2026-002 | 286.200 | 286.200 | 0 | ✅ |
| IE2026-003 | 90.600 | 181.200 | +90.600 (tam 2 kat) | ❌ Hatalı |
| IE2026-004 | 274.000 | 27.400 | −246.600 (10'da 1'i) | ❌ Hatalı |
| IE2026-005 | 284.400 | 568.800 | +284.400 (tam 2 kat) | ❌ Hatalı |
| IE2026-006 | 440.200 | 440.200 | 0 | ✅ |
| IE2026-007 | 50.500 | 50.500 | 0 | ✅ |
| IE2026-008 | 42.000 | 46.200 | +4.200 (%10 fazla) | ❌ Hatalı |
| IE2026-009 | 129.750 | 129.750 | 0 | ✅ |
| IE2026-010 | 48.000 | 48.000 | 0 | ✅ |
| IE2026-011…016 | 1.606.500 (toplam) | 1.606.500 (toplam) | 0 | ✅ |

**Yorum:**
- IE2026-001, 003, 005 → StockMovement tam **2 katı** görünüyor. Muhtemelen aynı üretim için hareket **iki kez** kaydedilmiş (örn. bir düzeltme sırasında eski kayıt silinmeden yenisi eklenmiş).
- IE2026-004 → StockMovement, gerçek üretimin **10'da 1'i**. Muhtemelen elle girişte ondalık/birim hatası (27.400 yerine 274.000 girilmesi gerekiyordu).
- IE2026-008 → %10 fazla, küçük bir elle-düzeltme kalıntısı olabilir.
- Ayrıca WorkOrderId'si **boş (NULL)** olan 15 adet "Üretimden Giriş" hareketi var, toplamı **23.680 adet** — bu rakam YARI MAMÜL toplam girişine (23.680,00 kg) birebir eşit. Bu muhtemelen Dönüşüm (Conversion) sürecinin ürettiği kayıtlar (iş emrine değil, dönüşüm işlemine bağlı) — **hata değil**, yapısal bir fark, ama raporlarda "İş emrine bağlı olmayan üretim girişi" olarak ayrıca etiketlenmesi faydalı olur.

**Etki:** Bu 5 iş emrindeki StockMovement tutarsızlığı, `/Stock/Index` ve genel mamul stok raporlarında **gerçekte var olmayan fazladan stok** (veya IE2026-004'te eksik stok) gösterebilir. Denetçi bu sayfaları incelerken toplamı `WorkOrder.ActualQuantity` ile karşılaştırırsa fark sorgulanabilir.

**Önerilen düzeltme adımları:**
1. IE2026-001 / 003 / 005 için StockMovement tablosunda o iş emrine ait Tip=1 kayıtları tekil olarak incelenmeli — muhtemel mükerrer kayıt silinmeli (tek kayıt kalmalı, miktar = ActualQuantity).
2. IE2026-004 için StockMovement miktarı 27.400 → 274.000 olarak düzeltilmeli (ya da kaynağı incelenip doğru değer teyit edilmeli).
3. IE2026-008 için 4.200 adetlik farkın kaynağı (muhtemel manuel düzeltme/fire) araştırılmalı.
4. Düzeltme sonrası `/Stock/Index` ve `/Stock/AnaOzet` sayfalarındaki mamul toplamlarının yeniden kontrol edilmesi önerilir.

> Bu satırlar mevcut sistemde önceden vardı — bu oturumdaki değişikliklerden (satış içe aktarımı, stok yeterlilik kuralı vb.) **kaynaklanmıyor**. İçe aktardığım 8 satış siparişi ve 16 satış hareketi ayrıca doğrulandı, hiçbirinde tutarsızlık yok (Bölüm 3.4).

---

## 5. Küçük Ölçekli Gözlemler (Öncelik: Düşük — İncelemeye Değer)

- **Üretim Tüketimi (StockMovement Tip=5) toplamı 183.698 kg**, ancak ProductionDetail bazlı hesaplanan toplam tüketim+fire **175.631 kg** (144.943+6.415+20.813+1.116+2.344). Aradaki **8.067 kg fark**ın kaynağı bu oturumda tespit edilemedi — muhtemelen elle girilmiş ek stok hareketleri veya birim dönüşüm farkı. Mali/denetim öncesi ayrı incelenmesi önerilir.
- **Dönüşüm Fire Alanı (`FscLot.ConversionFireKg`)** — sistemdeki 15 dönüşüm kaydının **tamamında 0,00** kayıtlı. Gerçekten fire oluşmadıysa sorun yok; ama bu alanın hiç kullanılmamış olma ihtimali de var (veri girişi eksikliği). Dönüşüm ekranında bu alanın doldurulup doldurulmadığı kontrol edilmeli.
- **Müşteri ACORE DIŞ TİCARET LTD.ŞTİ.** — bu oturumda `IsFscActive` **False → True** olarak güncellendi (kullanıcı onayıyla). `FscExpiryDate` alanı hâlâ boş — sertifika süre takibi istenirse bu tarih girilmeli.

---

## 6. Bu Oturumda Yapılan Değişiklikler (Bilgi Amaçlı)

Bu rapor, aynı oturumda yapılan şu geliştirmelerin ardından hazırlandı — denetim öncesi bunların da bilinmesi faydalı:

1. `SalesOrder.InvoiceDate` alanı eklendi (migration).
2. **Stok yeterlilik kuralı** eklendi: bir iş emrinden üretilenden fazla sevkiyat yapılması artık sistemsel olarak engelleniyor (negatif bakiye imkânsız).
3. Satış Detayı sayfasında iş emri seçiminde "Kalan: N adet" göstergesi eklendi.
4. Fatura yazdırma sayfası (`/Sales/PrintInvoice/{id}`) eklendi.
5. Satış listesindeki İrsaliye/Fatura PDF görüntüleme linklerinde bir hata düzeltildi (önceden hiç açılmıyordu).
6. `FSC_Fatura.xlsx`'teki 16 satırlık tarihsel sevkiyat verisi 8 satış siparişi olarak sisteme işlendi, ilgili 16 şablon PDF (irsaliye+fatura) üretilip ilişkilendirildi.
7. İş Emri Formu yazdırma özelliği (hammadde tüketim detaylı) eklendi.

---

## 7. Sonuç ve Öncelik Sırası

| Öncelik | Yapılacak İş |
|---|---|
| 🔴 Yüksek | Bölüm 4'teki 5 iş emrindeki StockMovement/ActualQuantity tutarsızlığını düzelt (denetim öncesi mutlaka) |
| 🟡 Orta | Bölüm 5'teki 8.067 kg'lık tüketim farkının kaynağını araştır |
| 🟡 Orta | Dönüşüm fire alanının gerçekten kullanılıp kullanılmadığını teyit et |
| 🟢 Düşük | ACORE DIŞ TİCARET için FSC sertifika bitiş tarihi (FscExpiryDate) gir |

Sistemde **hiçbir sayfa boş/veri göstermiyor değil** — tüm modüller aktif ve dolu. Asıl risk, Bölüm 4'teki rakamsal tutarsızlıklar; bunlar düzeltilmeden denetime girilirse mamul stok toplamlarında açıklanması gereken bir fark ortaya çıkar.

---

*Bu rapor otomatik SQL sorguları ve canlı sayfa taramasıyla üretilmiştir. Rakamlar `FscErpDb` veritabanından 02.07.2026 tarihinde çekilmiştir.*
