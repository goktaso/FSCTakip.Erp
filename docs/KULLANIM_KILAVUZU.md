# FSC Takip ERP — Kullanım Kılavuzu

> **Versiyon:** 1.2 · **Güncelleme:** Mayıs 2026  
> Bu kılavuz, FSC Takip ERP sistemini ilk kez kullanan firma personeli ve yöneticiler için hazırlanmıştır.

---

## İçindekiler

1. [Sisteme Giriş](#giris)
2. [Ana Ekran (Dashboard)](#dashboard)
3. [Sol Menü Yapısı](#menu)
4. [Tedarikçi Yönetimi](#tedarikciler)
5. [Müşteri Yönetimi](#musteriler)
6. [Ürün Yönetimi](#urunler)
7. [Sistem Tanımlamaları](#tanimlamalar)
8. [Hammadde Girişi — FSC Lot](#hammadde)
9. [Hammadde Detayı — Bobin / Seri Takibi](#detail)
10. [Sık Sorulan Sorular](#sss)
11. [Üretim — İş Emirleri](#uretim)
12. [Üretim Detayı — Hammadde Tüketimi](#uretim-detail)
13. [Fire Raporu](#fire)

---

## 1. Sisteme Giriş {#giris}

**Sayfa:** `/Account/Login`

### Adımlar

1. Tarayıcınızda sistem adresini açın (ör. `http://sunucu:5000`)
2. **Kullanıcı Kodu** alanına kullanıcı adınızı girin
3. **Şifre** alanına şifrenizi girin
4. **Şirket Veritabanı** açılır listesinden firmanızı seçin
5. **GİRİŞ YAP** butonuna tıklayın

```
┌─────────────────────────────────────┐
│           FSC TAKİP ERP             │
│      Kraft Kağıt İzlenebilirlik     │
│                                     │
│  Kullanıcı Kodu                     │
│  ┌─[👤]──────────────────────────┐  │
│  │  admin                        │  │
│  └───────────────────────────────┘  │
│                                     │
│  Şifre                              │
│  ┌─[🔒]──────────────────────────┐  │
│  │  ••••••••                     │  │
│  └───────────────────────────────┘  │
│                                     │
│  ┌─────────────────────────────┐    │
│  │  ▶  GİRİŞ YAP              │    │
│  └─────────────────────────────┘    │
└─────────────────────────────────────┘
```

> **İpucu:** Yanlış şifre girişi yaptığınızda "Kullanıcı adı veya şifre hatalı" uyarısı görürsünüz. Büyük/küçük harf duyarlıdır.

### Çıkış Yapma
Sağ üst köşedeki kırmızı **⏻ güç** butonuna tıklayarak oturumu sonlandırabilirsiniz.

---

## 2. Ana Ekran (Dashboard) {#dashboard}

**Sayfa:** `/Home/Index`

Giriş yapıldıktan sonra açılan ana ekranda:

- **Özet istatistikler:** Toplam lot, hammadde stoku, aktif tedarikçi sayısı
- **FSC uyarıları:** Sertifikası yaklaşan tedarikçiler için otomatik uyarı bandı
- **Hızlı erişim linkleri:** En sık kullanılan sayfalar

---

## 3. Sol Menü Yapısı {#menu}

Ekranın sol tarafındaki lacivert menü sistemi şu bölümlerden oluşur:

```
┌─────────────────────┐
│  🌿 FSC Takip ERP   │
│  Kraft Kağıt Takip  │
├─────────────────────┤
│  GENEL              │
│  📊 Dashboard       │
├─────────────────────┤
│  FSC İŞLEMLERİ      │
│  🚛 Hammadde Girişi │
│  🏭 Üretim        > │
│  📦 Satış/Sevkiyat  │
│  🏪 Stok Durumu     │
├─────────────────────┤
│  TİCARİ             │
│  👔 Müşteriler      │
│  🚚 Tedarikçiler    │
│  📦 Ürünler         │
├─────────────────────┤
│  FSC RAPORLAR       │
│  📈 Raporlar      > │
├─────────────────────┤
│  SİSTEM             │
│  ⚙️ Tanımlamalar  > │
│  🔄 ERP Entegrasyon │
│  📖 Kullanım Kıl.   │
└─────────────────────┘
```

**Menüyü daraltma/genişletme:** Sol üst köşedeki ≡ (hamburger) ikonuna tıklayarak menüyü simge boyutuna küçültebilir, tekrar tıklayarak genişletebilirsiniz. Bu tercih tarayıcı hafızasına kaydedilir.

**Alt menüler:** `>` işaretli menü öğelerine tıklandığında alt menü açılır. Açık alt menüyü kapatmak için tekrar tıklayın.

---

## 4. Tedarikçi Yönetimi {#tedarikciler}

**Sayfa:** `/Suppliers/Index`  
**Menü Yolu:** Ticari → Tedarikçiler

Hammadde aldığınız tedarikçi firmalar bu bölümde tanımlanır.

### Ekran Düzeni

```
[≡] [+ Yeni Tedarikçi]   Tedarikçiler   [Filtrele] [Excel] [👤]
┌──────────────────────────────────────────────────────────┐
│ Filtre Paneli (Tedarikçi Adı, FSC Durumu, Ülke)         │
│                                                         │
│ ┌─────┬──────────┬──────────┬────────┬────────┬──────┐ │
│ │ KOD │   ADI    │ FSC KODU │ DURUM  │ SON TRH│ İŞLEM│ │
│ ├─────┼──────────┼──────────┼────────┼────────┼──────┤ │
│ │TED-1│ BOSNA    │ NC-COC-  │ ✅Aktif│31.12.26│ ✏️🔄│ │
│ │TED-2│ SEGEZHA  │ SCS-COC- │ ✅Aktif│30.06.26│ ✏️🔄│ │
│ │TED-3│ OYKA     │RA-COC-   │ ⚠️Pasif│ —     │ ✏️🔄│ │
│ └─────┴──────────┴──────────┴────────┴────────┴──────┘ │
└─────────────────────────────────────────────────────────┘
```

### Yeni Tedarikçi Ekleme

1. Sol üstte menünün hemen yanındaki **+ Yeni Tedarikçi** butonuna tıklayın
2. Açılan formda doldurun:
   - **Tedarikçi Adı** *(zorunlu)* — firma tam adı
   - **Ülke** — menşei ülke
   - **FSC Lisans Kodu** — sertifika numarası (ör. `NC-COC-123456`)
   - **FSC Tipi** — FSC 100%, FSC Mix, FSC Recycled
   - **FSC Geçerlilik Tarihi** — sertifikanın bitiş tarihi
   - **E-posta / Telefon** — iletişim bilgileri
3. **Kaydet** butonuna tıklayın
4. Sistem otomatik olarak `TED-001`, `TED-002` şeklinde sıralı kod atar

> **⚠️ Önemli:** FSC Geçerlilik Tarihi 90 günden az kaldığında sistem otomatik uyarı verir. Bu tedarikçiden hammadde girişi yapıldığında sarı uyarı gösterilir.

### Tedarikçi Düzenleme
Listede tedarikçinin yanındaki **✏️ kalem** ikonuna tıklayarak düzenleyebilirsiniz.

### Aktif/Pasif Yapma
**🔄** ikonuna tıkladığınızda durum değişir:
- **Aktif (yeşil):** Hammadde girişinde seçilebilir
- **Pasif (sarı):** Listede görünür ama yeni girişlerde seçilemez

---

## 5. Müşteri Yönetimi {#musteriler}

**Sayfa:** `/Customers/Index`  
**Menü Yolu:** Ticari → Müşteriler

Ürün sattığınız müşteri firmalar bu bölümde tanımlanır.

### Yeni Müşteri Ekleme

1. **+ Yeni Müşteri** butonuna tıklayın
2. Zorunlu alanlar:
   - **Müşteri Adı**
   - **Müşteri Tipi:** FSC Sertifikalı / Standart
3. Opsiyonel alanlar:
   - **FSC Lisans No** — müşterinin kendi FSC sertifika numarası
   - **FSC Geçerlilik Tarihi** — chain of custody takibi için
   - **Vergi No / Adres** — fatura bilgileri
4. Sistem `MHS-001`, `MHS-002`... şeklinde otomatik kod atar

> **ℹ️ Not:** FSC sertifikalı müşteriye satış yapıldığında FSC CoC zinciri otomatik takip edilir. Müşterinin sertifikası geçersizse satış sırasında uyarı alırsınız.

---

## 6. Ürün Yönetimi {#urunler}

**Sayfa:** `/Products/Index`  
**Menü Yolu:** Ticari → Ürünler

Üretip sattığınız torba/ambalaj ürünleri bu bölümde tanımlanır.

### Ürün Kartı Alanları

| Alan | Açıklama | Örnek |
|------|----------|-------|
| Ürün Kodu | Otomatik üretilir | `PRD-001` |
| Ürün Adı | Ürünün tam adı | `KRAFT ÇUVALI 50KG` |
| Ürün Grubu | Hangi gruba ait | `Kraft Torba` |
| FSC Tipi | Bu ürün için geçerli FSC tipi | `FSC-MIX` |
| Kağıt Tipi | Kullanılan kağıt | `KRAFT BROWN` |
| Gramaj | g/m² | `80 gr` |
| En | Bobin eni (mm) | `1080 mm` |
| Torba Tipi | Üretim tipi | `V Kesim` |
| Aktif | Üretimde kullanılıyor mu | Evet/Hayır |

### Excel Dışa Aktarma
Sağ üst **📥 Excel** butonuyla tüm ürün listesini Excel dosyası olarak indirebilirsiniz.

---

## 7. Sistem Tanımlamaları {#tanimlamalar}

**Menü Yolu:** Sistem → Tanımlamalar

Alt bölümler:

| Alt Menü | Sayfa | Açıklama |
|----------|-------|----------|
| Torba Tipleri | `/Product/BagTypes` | Kare Dip, V Kesim, Körüklü... |
| Ürün Grupları | `/Product/Groups` | Ürün kod aralıkları |
| Kağıt Tipleri | `/Paper/Types` | Kraft, Beyaz Kraft... |
| Kağıt Renkleri | `/Paper/Colors` | Brown, White... |
| Gramajlar | `/Paper/Weights` | 70, 80, 90 g/m²... |
| Kağıt Enleri | `/Paper/Widths` | 1040mm, 1080mm... |
| FSC Tipleri | `/Paper/FscTypes` | FSC-100%, FSC-MIX... |
| Makineler | `/Machine/Machines` | Makine adı + kodu |

> **ℹ️ Not:** Tanımlama sayfaları aynı yapıdadır: listele, ekle, düzenle, aktif/pasif. Hammadde ve üretim kayıtlarında kullanılmadan önce tanımlamalar tamamlanmalıdır.

### Önce Yapılması Gerekenler (İlk Kurulum Sırası)

```
1. FSC Tipleri tanımla (FSC-100%, FSC-MIX vb.)
2. Kağıt Tipleri ekle (Kraft Brown, White Kraft...)
3. Gramajlar ekle (70g, 80g, 90g...)
4. Kağıt Enleri ekle (1040mm, 1080mm...)
5. Ürün Grupları tanımla
6. Torba Tipleri ekle
7. Makineler tanımla
8. Tedarikçiler ekle (FSC sertifikalarıyla birlikte)
9. Müşteriler ekle
10. Ürünler tanımla (reçeteyle birlikte)
```

---

## 8. Hammadde Girişi — FSC Lot {#hammadde}

**Sayfa:** `/Purchase/Index`  
**Menü Yolu:** FSC İşlemleri → Hammadde Girişi

FSC belgelenmiş hammadde (kraft kağıt bobini) girişleri bu modülde yönetilir. Her sevkiyat bir **LOT** olarak kaydedilir.

### Kavramlar

```
LOT (Sevkiyat)
├── Tedarikçi + FSC Sertifikası
├── İrsaliye / Fatura (PDF)
└── BOBİNLER (Seriler)
    ├── Bobin-1: 1.250 kg  → Kalan: 890 kg
    ├── Bobin-2: 1.180 kg  → Kalan: 1.180 kg (kullanılmadı)
    └── Bobin-3: 1.300 kg  → Kalan: 0 kg (tamamen tüketildi)
```

- **LOT:** Tedarikçiden gelen bir kamyon/sevkiyat
- **Bobin/Seri:** Lot içindeki her bir kağıt bobini
- **Kalan Kg:** Üretimde henüz kullanılmayan ağırlık

### Ekran Üstü — Topbar

```
[≡] [+ Yeni Lot Ekle]   Hammadde Girişleri   [Filtrele] [Excel] [👤]
```

Mavi gradient **+ Yeni Lot Ekle** butonu sol üstte, menü hamburgerin hemen yanındadır.  
Sağda **Filtrele** ve **Excel** butonları yer alır.

### Özet Kartları

Topbar altında 4 özet kart bulunur:

```
┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐
│    47    │  │ 382.450  │  │   215    │  │ 128.900  │
│ Toplam   │  │  Giriş   │  │  Toplam  │  │  Kalan   │
│   Lot    │  │   (kg)   │  │  Bobin   │  │  Hmm(kg) │
└──────────┘  └──────────┘  └──────────┘  └──────────┘
```

### Yeni Lot Kaydı

1. Sol üstteki mavi **+ Yeni Lot Ekle** butonuna tıklayın
2. Formu doldurun:

```
┌─ Hammadde Girişi ────────────────────────────────────┐
│                                                        │
│  Lot No           Geliş Tarihi      Araç Plakası      │
│  [L2026-001 ]     [17.05.2026   ]   [34 ABC 123  ]   │
│  (boş=otomatik)                                       │
│                                                        │
│  Tedarikçi *                FSC Tipi *                │
│  [BOSNA HERSEK LTD   ▼]    [FSC-100%         ▼]      │
│                                                        │
│  ⚠️ Tedarikçi FSC uyarısı (varsa burada görünür)     │
│                                                        │
│  Hammadde Ürünü                                        │
│  [10001 — NATRON VBMF090 1080 FSC 100%   ▼]          │
│                                                        │
│  ────────────────────────────────────────────────     │
│  İrsaliye No    Fatura No    Tutar     Para Birimi    │
│  [IRS-2026-45 ] [FTR-1234 ]  [45.000 ] [TRY ▼]      │
│                                                        │
│  İrsaliye PDF           Fatura PDF                    │
│  [📎 Dosya Seç]         [📎 Dosya Seç]               │
│                                                        │
│  Notlar                                               │
│  [                                               ]    │
│                                                        │
│                    [Vazgeç] [💾 Kaydet]               │
└────────────────────────────────────────────────────────┘
```

3. **Kaydet** butonuna tıklayın
4. Sistem otomatik lot numarası atar: `L2026-001`, `L2026-002`...

> **⚠️ FSC Uyarısı:** Seçilen tedarikçinin FSC sertifikası geçersiz veya süresi dolmuşsa form üzerinde **sarı uyarı** belirir. Kayıt yapılabilir ancak uyarı bildirilir.

### Lot Listesi

Her lot için listede şunlar görünür:

| Kolon | Açıklama |
|-------|----------|
| **Lot No** | Tıklanabilir link → Lot Detay sayfası |
| **Tedarikçi** | FSC sorunu varsa yanında ⚠️ rozeti |
| **FSC Tipi** | Yeşil rozet ile |
| **Geliş Tarihi** | `17.05.2026` formatında |
| **İrsaliye No** | Varsa gösterilir |
| **Bobin** | Toplam bobin sayısı (mavi rozet) |
| **Giriş (kg)** | Toplam giriş ağırlığı |
| **Kalan (kg)** | Progress bar + renk kodlu değer |
| **Belgeler** | PDF görüntüleme butonları |
| **İşlem** | 👁️ Detay · ✏️ Düzenle |

#### Renk Kodu — Kalan Kg
- 🟢 **Yeşil:** %50 üzeri kalan var
- 🟡 **Sarı:** %20–50 arası kalan
- 🔴 **Kırmızı:** %20 altında kalan (kritik)

### Filtreleme

**Filtrele** butonuna tıkladığınızda filtre paneli açılır:

- **Başlangıç / Bitiş Tarihi:** Belirli tarih aralığındaki lotları listeler
- **Tedarikçi:** Sadece seçilen tedarikçinin lotlarını gösterir
- **FSC Tipi:** FSC-100%, FSC-MIX gibi tipe göre filtreler

### Excel Dışa Aktarma

**📥 Excel** butonu ile tüm lot listesi (filtre uygulanmış haliyle) Excel dosyası olarak indirilir.

İçerik: Lot No, Tedarikçi, FSC Tipi, Geliş Tarihi, İrsaliye/Fatura No, Bobin Adedi, Toplam Kg, Kalan Kg

---

## 9. Hammadde Detayı — Bobin / Seri Takibi {#detail}

**Sayfa:** `/Purchase/Detail/{id}`  
**Erişim:** Lot listesinde Lot No linkine veya 👁️ butonuna tıklayın

Bu sayfa, bir lota ait tüm bobinleri ve tüketim durumlarını gösterir.

### Sayfa Düzeni

```
┌──────────────────┬───────────────────────────────────────┐
│   LOT BİLGİSİ   │         BOBİN LİSTESİ                 │
│                  │                                         │
│  L2026-047       │  Seri No    Giriş  Kalan  Tük.%  İşlem│
│  BOSNA HERSEK    │  L2026-047-B01 1250  890  28.8%  ✏️🗑️│
│  FSC-100%        │  L2026-047-B02 1180 1180   0.0%  ✏️🗑️│
│  17.05.2026      │  L2026-047-B03 1300    0  100.%  ✏️   │
│                  │                                         │
│  ÖZET            │  [+ Bobin Ekle]                        │
│  Giriş: 3.730kg  │                                         │
│  Kalan: 2.070kg  │                                         │
│  Tük.: %44.5     │                                         │
│  [====▓▓▓▓▓   ] │                                         │
│                  │                                         │
│  BELGELER        │                                         │
│  📄 İrsaliye     │                                         │
│  📄 Fatura       │                                         │
└──────────────────┴───────────────────────────────────────┘
```

### Bobin Ekleme

1. **+ Bobin Ekle** butonuna tıklayın
2. Formu doldurun:
   - **Seri No:** Boş bırakılırsa otomatik atanır (`L2026-047-B01`)
   - **Giriş Ağırlığı (kg):** Kantarda tartılan ağırlık *(zorunlu)*
   - **Açılış Stoğu mu?** Sistemden önce var olan bobin ise işaretleyin
   - **Notlar:** Opsiyonel

> **ℹ️ Not:** Bobin eklendikten sonra "Kalan Ağırlık" otomatik olarak "Giriş Ağırlığı" ile eşit başlar. Üretimde kullandıkça otomatik düşer.

### Bobin Silme

Bobinin yanındaki 🗑️ ikonuna tıklayın.

> **⛔ Dikkat:** Üretimde kullanılmış bobinler **silinemez**. Tüketim kaydı olan bir bobini silmeye çalışırsanız sistem hata mesajı verir.

### Belge Yükleme / Görüntüleme

Sol panelde belge bölümünde:
- Belge yüklenmemişse **"PDF Yükle"** butonu görünür
- Belge yüklenmişse **"Görüntüle"** butonu aktif olur, tıklandığında PDF yeni sekmede açılır

---

## 10. Sık Sorulan Sorular {#sss}

**S: Yanlış lot numarası girdim, düzeltebilir miyim?**  
A: Evet. Lot listesinde ✏️ düzenle butonuyla lot bilgilerini güncelleyebilirsiniz.

**S: Bir bobini yanlışlıkla sildim ne yapabilirim?**  
A: Silinen bobin geri getirilemiyor, tekrar eklenebilir. Üretim kaydı olan bobinler zaten silinemez.

**S: FSC sertifikası uyarısı aldım ama tedarikçi sertifika numarasını verdi, ne yapmalıyım?**  
A: Tedarikçiler menüsünden ilgili tedarikçiyi düzenleyip "FSC Geçerlilik Tarihi" ve "FSC Lisans Kodu" alanlarını güncelleyin. Ardından "FSC Aktif" kutucuğunu işaretleyin.

**S: Excel'e aktardığımda Türkçe karakterler bozuk çıkıyor?**  
A: Excel dosyası açılırken "Dosya Kökeni" ayarını **Windows-1254 (Türkçe)** veya **UTF-8** olarak seçin.

**S: Birden fazla şirketin verisi nasıl ayrılır?**  
A: Giriş sayfasında **Şirket Veritabanı** alanından farklı firma veritabanı seçilerek çalışılır.

---

---

## 11. Üretim — İş Emirleri {#uretim}

**Sayfa:** `/Production/Index`  
**Menü Yolu:** FSC İşlemleri → Üretim → İş Emirleri

Üretim planı bu modülde yönetilir. Her üretim çalışması bir **İş Emri** olarak oluşturulur.

### Kavramlar

```
İŞ EMRİ (Work Order)
├── Ürün: KRAFT ÇUVALI 50KG
├── Makine: MAKİNE-1
├── Plan: 10.000 adet
└── TÜKETİM KAYITLARI
    ├── Bobin L2026-001-B01 → 1.250 kg tüketildi → 4.200 adet üretildi
    ├── Bobin L2026-001-B02 → 1.180 kg tüketildi → 3.900 adet üretildi
    └── ...
```

### İş Emri Durumları

| Durum | Renk | Açıklama |
|-------|------|----------|
| **Taslak** | Gri | Oluşturuldu, henüz üretime başlanmadı |
| **Üretimde** | Sarı | İlk tüketim kaydı girildiğinde otomatik geçer |
| **Tamamlandı** | Yeşil | Manuel olarak tamamlandı işaretlenince |
| **İptal** | Kırmızı | İptal edilmiş iş emri |

### Ekran Üstü — Topbar

```
[≡] [+ Yeni İş Emri]   İş Emirleri   [Filtrele] [Excel] [👤]
```

Mavi gradient **+ Yeni İş Emri** butonu sol üstte, menü hamburgerin hemen yanındadır.

### Özet Kartları

```
┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐
│    23    │  │    5     │  │    17    │  │  48.200  │
│  Toplam  │  │ Üretimde │  │Tamamlandı│  │  Üretim  │
│ İş Emri │  │          │  │          │  │  (adet)  │
└──────────┘  └──────────┘  └──────────┘  └──────────┘
```

### Yeni İş Emri Oluşturma

1. Sol üstteki mavi **+ Yeni İş Emri** butonuna tıklayın
2. Formu doldurun:

```
┌─ Yeni İş Emri ───────────────────────────────────────┐
│                                                         │
│  İş Emri No           Plan Tarihi        Durum        │
│  [IE2026-001     ]   [17.05.2026   ]   [Taslak ▼]    │
│  (boş=otomatik)                                        │
│                                                         │
│  Ürün *                                                 │
│  [PRD-001 — KRAFT ÇUVALI 50KG                   ▼]    │
│                                                         │
│  Makine *                  Planlanan Miktar *          │
│  [MAKİNE-1          ▼]    [10000              adet]   │
│                                                         │
│  Notlar                                                 │
│  [                                               ]     │
│                                                         │
│                    [Vazgeç] [💾 Kaydet]                │
└─────────────────────────────────────────────────────────┘
```

3. Sistem otomatik olarak `IE2026-001`, `IE2026-002`... numarası atar

### İş Emrini Tamamlama

İş emri tamamlandığında listede ✅ butonuna tıklayın veya detay sayfasında **Tamamla** butonunu kullanın.

> **ℹ️ Not:** Tamamlanmış iş emirlerine yeni tüketim kaydı eklenemez.

---

## 12. Üretim Detayı — Hammadde Tüketimi {#uretim-detail}

**Sayfa:** `/Production/Detail/{id}`  
**Erişim:** İş emirleri listesinde İş Emri No linkine tıklayın

### Sayfa Düzeni

```
┌──────────────────┬────────────────────────────────────────┐
│  İŞ EMRİ BİLGİSİ│       TÜKETİM KAYITLARI               │
│                  │                                          │
│  IE2026-047      │  Tarih    Bobin       Tük.  Fire  Ür.  │
│  KRAFT ÇUVAL     │  17.05.26 L047-B01  1250   25   4200  │
│  MAKİNE-1        │  17.05.26 L047-B02  1180   18   3900  │
│  Taslak → Üretimde│                                        │
│                  │  [+ Tüketim Gir]                        │
│  ÖZET            │                                          │
│  Üretilen: 8.100 │                                          │
│  Tüketim: 2.430kg│                                          │
│  Fire: 43 kg     │                                          │
│  Fire %: 1.8%    │                                          │
│                  │                                          │
│  [====▓▓▓▓▓▓   ]│                                          │
│  %81 tamamlandı  │                                          │
└──────────────────┴────────────────────────────────────────┘
```

### Tüketim Kaydı Girme

1. **+ Tüketim Gir** butonuna tıklayın
2. Açılan formda:

```
┌─ Tüketim Kaydı ──────────────────────────────────────┐
│                                                         │
│  Bobin (Hammadde Serisi) *                             │
│  [L2026-047-B01 — Kalan: 1.250,00 kg            ▼]   │
│  ℹ️ L2026-047 · BOSNA HERSEK · Kalan: 1.250,00 kg    │
│                                                         │
│  Makine *                                               │
│  [MAKİNE-1                                      ▼]    │
│                                                         │
│  Üretim Tarihi *    Tüketilen (kg) *   Fire (kg)       │
│  [17.05.2026   ]   [1250.00     kg]   [25.00   kg]    │
│                                                         │
│  Üretilen (adet) *  Notlar                             │
│  [4200      adet]   [                          ]       │
│                                                         │
│                    [Vazgeç] [💾 Kaydet]                │
└─────────────────────────────────────────────────────────┘
```

3. **Bobin seçilince** altında lot/tedarikçi/kalan bilgisi otomatik gösterilir
4. **Kaydet** butonuna tıklayın

> **⚠️ Stok Kontrolü:** Girilen tüketim miktarı bobinin kalan ağırlığını aşarsa sistem hata verir ve kaydetmez.
>
> **ℹ️ Otomatik Durum:** İlk tüketim kaydı girildiğinde iş emri "Taslak" → "Üretimde" durumuna otomatik geçer.

### Tüketim Kaydını Silme

Kayıt satırındaki 🗑️ butonuna tıklayın.

> **ℹ️ Not:** Kayıt silindiğinde, tüketilen ağırlık bobine geri iade edilir (stok geri artar).

---

## 13. Fire Raporu {#fire}

**Sayfa:** `/Production/WasteReport`  
**Menü Yolu:** FSC İşlemleri → Üretim → Fire Raporu

Belirli tarih aralığında gerçekleşen hammadde fire/atıklarını gösterir.

### Fire Oranı Renk Kodu
- 🟢 **Yeşil:** %3 altı — Normal
- 🟡 **Sarı:** %3–6 arası — Takip et
- 🔴 **Kırmızı:** %6 üzeri — Dikkat!

### Filtreleme
Başlangıç / Bitiş tarihi seçip **Sorgula** butonuyla istediğiniz dönemi filtreleyin.

---

---

## 📋 Modül Durumu

| Modül | Durum | Kılavuz Bölümü |
|-------|-------|----------------|
| Giriş / Çıkış | ✅ Tamamlandı | Bölüm 1 |
| Tedarikçiler | ✅ Tamamlandı | Bölüm 4 |
| Müşteriler | ✅ Tamamlandı | Bölüm 5 |
| Ürünler | ✅ Tamamlandı | Bölüm 6 |
| Tanımlamalar | ✅ Tamamlandı | Bölüm 7 |
| Hammadde Girişi | ✅ Tamamlandı | Bölüm 8–9 |
| Üretim / İş Emirleri | ✅ Tamamlandı | Bölüm 11–13 |
| Satış / Sevkiyat | ⏳ Planlandı | — |
| Stok Durumu | ⏳ Planlandı | — |
| FSC Raporlar | ⏳ Planlandı | — |
| ERP Entegrasyon | ⏳ Planlandı | — |

---

*Bu kılavuz her yeni modül tamamlandıkça güncellenmektedir.*
