# FSC Takip ERP — Kullanım Kılavuzu

> **Versiyon:** 3.1 · **Güncelleme:** Haziran 2026  
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
14. [Satış Siparişleri](#satis)
15. [Satış Detayı — Kalem ve Sevkiyat](#satis-detail)
16. [Stok Durumu](#stok)
17. [Stok Hareketleri](#stok-hareketleri)
18. [FSC Chain of Custody Raporu](#coc)
19. [Lot Takip Raporu](#lot-takip)
20. [Tedarikçi FSC Sertifika Durumu](#tedarikci-fsc)
21. [Depo Tanımlamaları](#depolar)
22. [Ürün Reçetesi — BOM](#recete)
23. [İmha Kayıtları](#imha)
24. [ERP Entegrasyonu — ETL](#etl)
25. [Netsis Senkronizasyonu](#netsis-sync)
26. [Üretim Fişi — Lot Evrak Görüntüleyici](#lot-evrak)
27. [Denetim Özet Raporu](#denetim-ozet)
28. [Tam İzlenebilirlik — Satış → Üretim → Lot](#tam-izlenebilirlik)
29. [BOM Bileşen Analizi](#bom-analizi)
30. [Netsis ETL Excel Dosyaları](#netsis-etl-excel)
31. [Hammadde Stoğu — Bobin Bazlı](#hammadde-stogu)
32. [ETL Otomatik Algıla Import](#etl-oto-import)
33. [Kullanıcı ve Yetki Yönetimi](#kullanici-yonetimi)
34. [Müşteri FSC Lisans Durumu](#musteri-fsc)
35. [Üretim Planı Takvimi](#uretim-plani)
36. [Fire / Atık Analizi](#fire-analizi)
37. [Dönem Kilidi](#donem-kilidi)

---

## 1. Sisteme Giriş {#giris}

**Sayfa:** `/Account/Login`

### Adımlar

1. Tarayıcınızda sistem adresini açın (ör. `http://sunucu:5000`)
2. **Kullanıcı Kodu** alanına kullanıcı adınızı girin
3. **Şifre** alanına şifrenizi girin — alanın sağındaki **👁 göz** simgesine tıklayarak girdiğiniz şifreyi görünür yapıp doğrulayabilir, tekrar tıklayarak gizleyebilirsiniz
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
│  ┌─[🔒]──────────────────────┬[👁]┐ │
│  │  ••••••••                 │    │ │
│  └───────────────────────────┴────┘ │
│                                     │
│  ┌─────────────────────────────┐    │
│  │  ▶  GİRİŞ YAP              │    │
│  └─────────────────────────────┘    │
└─────────────────────────────────────┘
```

> **İpucu:** Yanlış şifre girişi yaptığınızda "Kullanıcı adı veya şifre hatalı" uyarısı görürsünüz. Büyük/küçük harf duyarlıdır.
>
> **👁 Şifreyi göster:** Şifre alanının sağındaki göz simgesi, yazdığınız şifreyi açık metin olarak gösterip gizlemenizi sağlar — özellikle uzun/karmaşık şifrelerde hatalı giriş riskini azaltır.

### Çıkış Yapma
Sağ üst köşedeki kırmızı **⏻ güç** butonuna tıklayarak oturumu sonlandırabilirsiniz.

---

## 2. Ana Ekran (Dashboard) {#dashboard}

**Sayfa:** `/Home/Index`

Giriş yapıldıktan sonra açılan ana ekranda:

- **Özet istatistikler:** Toplam lot, hammadde stoku, aktif tedarikçi sayısı
- **FSC uyarıları:** Sertifikası yaklaşan tedarikçiler için otomatik uyarı bandı
- **Hızlı erişim linkleri:** En sık kullanılan sayfalar

### Üst Çubuk (Topbar) — Tüm Sayfalarda

Üst çubuktaki şu iki araç **her sayfada** kullanılabilir:

- **🔍 Global Arama:** Üstteki arama kutusuna en az 2 karakter yazdığınızda; müşteri, tedarikçi, ürün ve lot kayıtları arasında anlık arama yapılır. Sonuca tıklayınca ilgili sayfaya gidersiniz.
- **🔔 Bildirim Çanı:** Sağ üstteki zil simgesine tıklayınca açılan listede; **FSC sertifikası süresi dolan/yaklaşan** tedarikçi-müşteriler ve **düşük stok** (500 kg altı bobin) uyarıları görünür. Acil (süresi geçmiş) uyarılar kırmızı, yaklaşanlar turuncu noktayla işaretlenir; acil uyarı sayısı zilin üzerinde rozet olarak gösterilir.

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
   - **FSC Aktif** — müşterinin FSC sertifikasının geçerli olup olmadığını belirten kutucuk. FSC bölümündedir; yeni müşteride varsayılan **işaretlidir**. Sertifikası olmayan/geçersiz müşteriler için işareti kaldırın.
   - **Vergi No / Adres** — fatura bilgileri
4. Sistem `MHS-001`, `MHS-002`... şeklinde otomatik kod atar

> **ℹ️ Not:** FSC sertifikalı müşteriye satış yapıldığında FSC CoC zinciri otomatik takip edilir. Müşterinin sertifikası geçersizse (veya **FSC Aktif** kutucuğu işaretli değilse) satış sırasında uyarı alırsınız.

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

- **Stok Kodu:** Ürünün iç stok koduyla arama yapılabilir (örn. "10001")
- **Stok Adı (Dış Kod):** Ürünün dış koduyla veya adıyla arama yapılabilir — hem `ProductCode` hem `ExternalCode` alanları taranır
- **Tedarikçi:** Sadece seçilen tedarikçinin lotlarını gösterir
- **FSC Tipi:** FSC-100%, FSC-MIX gibi tipe göre filtreler

> **ℹ️ Not:** Filtreleme sonrası üstte görünen özet kartları (Toplam Lot, Giriş (kg), Toplam Bobin, Kalan Hmm(kg)) otomatik olarak filtrelenmiş veriye göre güncellenir.

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

## 14. Satış Siparişleri {#satis}

**Menü Yolu:** Ticari → Satış

**Sayfa:** `/Sales/Index`

### Ekran Düzeni

```
[≡] [+ Yeni Sipariş]   Satış Siparişleri   [Excel] [👤]
┌─────────────────────────────────────────────────────┐
│  📦 Toplam   ⏳ Bekleyen   ✅ Teslim   💰 Toplam Tutar │
├─────────────────────────────────────────────────────┤
│  Müşteri ▼   Başlangıç   Bitiş   Durum ▼  [Ara]    │
├─────────────────────────────────────────────────────┤
│ # │ Sipariş No │ Müşteri │ Tarih │ Tutar │ Durum    │
└─────────────────────────────────────────────────────┘
```

### Yeni Sipariş Oluşturma

1. Sol üstteki mavi **+ Yeni Sipariş** butonuna tıklayın.
2. Açılan formda şu alanları doldurun:
   - **Müşteri** *(zorunlu)* — açılır listeden seçin; FSC lisanslı müşterilerde yeşil rozet görünür
   - **Sipariş Tarihi** *(zorunlu)*
   - **Döviz** — TRY, USD, EUR (varsayılan: TRY)
   - **Durum** — Taslak (varsayılan), Teslim Edildi, İptal
   - Fatura No, İrsaliye No, Plaka, Teslimat Adresi, Notlar *(isteğe bağlı)*
3. **Kaydet** butonuyla sipariş oluşturulur; sistem otomatik olarak **SIP2026-001** formatında numara üretir.

### Sipariş Durumları

| Durum | Renk | Açıklama |
|-------|------|----------|
| Taslak | 🔵 Mavi | Hazırlanıyor, henüz sevk edilmedi |
| Teslim Edildi | 🟢 Yeşil | Sevk yapıldı, stok hareketi oluşturuldu |
| İptal | 🔴 Kırmızı | İptal edildi |

> **Not:** Teslim edilmiş sipariş düzenlenemez ve silinemez.

### Filtreleme ve Arama

- **Müşteri**, **Tarih aralığı** ve **Durum** filtrelerini birlikte kullanabilirsiniz.
- **Excel** butonu ile mevcut listeyi `.xlsx` formatında dışa aktarın.

### Sipariş Silme

- Yalnızca **Taslak** durumundaki siparişler silinebilir.
- Silme işlemi önce onay ister, ardından sipariş ve tüm kalemleri kaldırılır.

---

## 15. Satış Detayı — Kalem ve Sevkiyat {#satis-detail}

**Sayfa:** `/Sales/Detail/{id}`

Sipariş listesinden sipariş numarasına tıklayarak detay sayfasına ulaşın.

### Sayfa Bölümleri

**Sol Panel — Sipariş Bilgileri**
- Müşteri, tarih, irsaliye/fatura bilgileri
- Kalem sayısı, toplam adet ve tutar özeti
- **FSC CoC Durumu** göstergesi — siparişte en az bir iş emrine bağlı kalem varsa zincir sağlam kabul edilir

**Sağ Panel — Sipariş Kalemleri**
- Her kalemin ürün adı, miktar, birim fiyat ve tutarı
- FSC iş emri bağlantısı olan kalemlerde 🏆 rozeti görünür
- Kalem toplamı, satır sayısı ve genel tutar tablo altında gösterilir

### Kalem Ekleme / Düzenleme

1. **+ Kalem Ekle** butonuna tıklayın.
2. Formda şu alanları doldurun:
   - **Ürün** *(zorunlu)* — aktif ürünler listelenir
   - **Miktar** ve **Birim Fiyat** *(zorunlu)*
   - **Birim** — Adet (varsayılan), Kg, Ton, Rulo vb.
   - **İş Emri (FSC CoC)** *(isteğe bağlı)* — tamamlanmış iş emirleri listelenir; seçildiğinde satışın hangi üretimden geldiği izlenebilir olur
3. Kaydet.

> **FSC Zinciri (Chain of Custody):** Bir satış kalemi iş emrine bağlandığında FSC sertifikası gereken müşteri teslimatlarında izlenebilirlik sağlanmış olur. Denetimde bu zinciri kullanın.

### Sevkiyat (Sevk Et)

Sipariş hazır olduğunda **Sevk Et** (yeşil buton) ile teslimatı kaydedin:

1. **Sevk Et** butonuna tıklayın.
2. Açılan formda **Sevk Tarihi**, **İrsaliye No** ve **Plaka** bilgilerini girin.
3. **Sevkiyatı Onayla** butonuna tıklayın.

**Sevk işlemi şunu yapar:**
- Sipariş durumu → **Teslim Edildi**
- Her kalem için otomatik **Stok Hareketi** (Tür: Satış Sevkiyatı) oluşturulur
- Tarih, belge no, müşteri ve plaka stok hareketine işlenir

> **Dikkat:** Sevk edilmiş siparişe kalem eklenemez, silinemez ve sipariş silinemez.

---

## 16. Stok Durumu {#stok}

**Menü Yolu:** Operasyonel → Stok Durumu

**Sayfa:** `/Stock/Index`

### Ekran Düzeni

```
[≡] [⇄ Depo Transferi]   Stok Durumu   [Excel] [👤]
┌──────────────────────────────────────────────────────────────┐
│  📦 Ürün Çeşidi   ✅ Stokta Var   ⚠️ Stok Yok   ↓ Giriş   ↑ Çıkış │
├──────────────────────────────────────────────────────────────┤
│  Ürün Grubu ▼   Ürün ▼   [Filtrele]   [Temizle]   [Excel]   │
├──────────────────────────────────────────────────────────────┤
│ Kod │ Ürün Adı │ Grup │ Giriş │ Çıkış │ Net Stok │ Birim │ Son H. │
└──────────────────────────────────────────────────────────────┘
```

### Net Stok Hesabı

Her ürün için stok **Giriş − Çıkış** formülüyle hesaplanır:

| Hareket Tipi | Etkisi |
|-------------|--------|
| Üretim Girişi | + Giriş |
| Satın Alma Girişi | + Giriş |
| Satış Çıkışı | − Çıkış |
| Depo Transferi | Nötr (net etkilemez) |

### Durum Renkleri

- 🟢 **Var** — Net stok 0'dan büyük
- 🔴 **Yok** — Net stok 0 veya negatif (üretim yapılmış ancak henüz stok hareketi girilmemiş olabilir)

### Depo Transferi

Sol üstteki **⇄ Depo Transferi** butonu ile depo içi/depo arası ürün hareketini kaydedin:

1. Ürün seçin
2. Çıkış ve giriş deposunu belirtin
3. Miktar ve tarihi doldurun
4. Kaydet — hareket `TRF2026-001` formatında otomatik numaralanır

> Depo transferleri net stoğu değiştirmez, yalnızca depo bazlı izlenebilirlik için kaydedilir.

---

## 17. Stok Hareketleri {#stok-hareketleri}

**Menü Yolu:** Operasyonel → Stok Hareketleri

**Sayfa:** `/Stock/Movements`

### Ekran Düzeni

```
[≡]   Stok Hareketleri   [Excel] [👤]
┌──────────────────────────────────────────────────────────────┐
│  Ürün ▼   Hareket Tipi ▼   Başlangıç   Bitiş   [Filtrele]   │
├──────────────────────────────────────────────────────────────┤
│ Tarih │ Belge No │ Tip │ Ürün │ Miktar │ Birim │ Müşteri │ Plaka │
└──────────────────────────────────────────────────────────────┘
```

### Hareket Tipleri ve Renk Kodları

| Renk | Tip | Kaynak |
|------|-----|--------|
| 🔵 Mavi | Satın Alma Girişi | Hammadde modülü |
| 🟦 Koyu Mavi | Üretim Girişi | Üretim modülü |
| 🔴 Kırmızı | Satış Çıkışı | Satış → Sevk Et |
| 🟡 Sarı | Depo Transferi | Stok → Depo Transferi |

### Miktar Gösterimi

- **+ değerler** yeşil: stoka giren miktar
- **− değerler** kırmızı: stoktan çıkan miktar
- Satır altında toplam giriş ve çıkış özeti gösterilir

### Filtreleme

Ürün, hareket tipi ve tarih aralığını kombine ederek belirli bir ürünün tüm geçmişini veya belirli bir tarih dilimindeki satış çıkışlarını görüntüleyebilirsiniz.

> **FSC İzlenebilirlik:** Satış çıkışı olan hareketlerin "İş Emri" sütununda bağlı iş emri kodu görünür. Bu link FSC CoC zincirini kanıtlar.

---

## 18. FSC Chain of Custody Raporu {#coc}

**Menü Yolu:** FSC Raporlar → Chain of Custody

**Sayfa:** `/Reports/ChainOfCustody`

Bu rapor, **FSC denetiminin en kritik belgesidir.** Her satış sevkiyatının tedarikçiye kadar izlenebilirliğini tek ekranda gösterir.

### Zincir Mantığı

```
Tedarikçi (FSC Kodu) → FscLot → FscSerial → Üretim (İş Emri) → Satış → Müşteri
```

| Durum | Anlam |
|-------|-------|
| ✅ **Tam** | Satış kalemi bir iş emrine, o iş emri en az bir FSC lot/serisine bağlı |
| ❌ **Eksik** | İş emri bağlantısı yok veya lot/seri kaydı eksik |

### Denetim Kullanımı

1. Müşteri filtresiyle ilgili müşterinin tüm alımlarını listeleyin
2. Tüm satırların **Tam** statüsünde olduğunu doğrulayın
3. **Excel** butonu ile dışa aktarıp denetçiye sunun

> **Önemli:** Denetçi her satış kalemi için tedarikçi FSC sertifika kopyası isteyebilir. Tedarikçi FSC kodunu bu rapordan alarak Tedarikçi modülündeki belgelerle eşleştirin.

---

## 19. Lot Takip Raporu {#lot-takip}

**Menü Yolu:** FSC Raporlar → Lot Takip

**Sayfa:** `/Reports/LotTrace`

Bir hammadde lotunun giriş → seri → üretim → satış yolculuğunu adım adım gösterir.

### Kullanım

1. Üstteki açılır listeden takip edilecek lotu seçin
2. **Takip Et** butonuna tıklayın

### Sayfa Bölümleri

- **Lot Bilgileri Kartı** — Tedarikçi, geliş tarihi, FSC sertifika kodu, irsaliye/fatura referansı
- **FSC Zinciri Özeti** — 4 adımlı görsel zincir (Tedarikçi → Lot → Üretim → Satış)
- **Seri Listesi** — Her bobinin başlangıç/kalan ağırlığı, tüketim yüzdesi, kullanıldığı iş emirleri
- **İlgili Satışlar** — Bu lottan üretilen ürünlerin gittiği müşteriler ve siparişler

> **İpucu:** Şikayet veya geri çağırma durumunda etkilenen müşterileri hızla tespit etmek için bu raporu kullanın.

---

## 20. Tedarikçi FSC Sertifika Durumu {#tedarikci-fsc}

**Menü Yolu:** FSC Raporlar → Tedarikçi FSC

**Sayfa:** `/Reports/SupplierFsc`

| Renk | Anlam |
|------|-------|
| 🟢 Geçerli | Sertifika aktif, 30 günden fazla var |
| 🟡 Yakında Bitiyor | 30 gün veya daha az kalmış |
| 🔴 Geçersiz | Süresi geçmiş veya pasif olarak işaretlenmiş |

> **Öneri:** Her ay bu sayfayı kontrol ederek sarı veya kırmızı tedarikçilerle sertifika yenileme sürecini başlatın. FSC denetiminde tedarikçinin sertifika geçerlilik tarihi kontrol edilir; süresi geçmiş tedarikçiden alınan hammadde FSC zincirini kırar.

---

## 21. Depo Tanımlamaları {#depolar}

**Menü Yolu:** Tanımlamalar → Depolar

**Sayfa:** `/Warehouse/Index`

```
[≡] [+ Yeni Depo Ekle]   Depo Tanımlamaları   [Filtrele] [Excel] [👤]
```

Hammadde ve mamul depolarını tanımlamak için kullanılır. Stok transferlerinde kaynak/hedef depo olarak seçilir.

| Alan | Açıklama | Örnek |
|------|----------|-------|
| Depo Kodu | Kısa kod | DEP-01 |
| Depo Adı | Açıklayıcı isim | Hammadde Deposu |
| Durum | Aktif / Pasif | Aktif |

> **Dikkat:** Stok hareketinde kullanılmış bir depo silinemez. Önce pasife alın.

---

## 22. Ürün Reçetesi — BOM {#recete}

**Menü Yolu:** Ürünler → Ürün Listesi → Reçete butonu (yeşil liste ikonu)

**Sayfa:** `/Products/Recipe/{id}`

```
[≡] [+ Bileşen Ekle]   Reçete — {Ürün Adı}   [← Ürünler] [👤]
```

Her mamul ürünün standart hammadde/bileşen tüketimini (BOM — Bill of Materials) tanımlamak için kullanılır. İş emri üretim sayfasında **Reçeteden Yükle** butonu ile otomatik uygulanır.

### Ürün Bilgi Kartı

Sayfanın üstünde ürünün kodu, adı, grubu ve birim bilgisi gösterilir. Sağ üstte toplam bileşen sayısı ve kaç adedinin aktif olduğu görünür.

### Bileşen Ekleme

**Çoklu Bileşen Seçimi Modu (Yeni):**

1. **"Bileşen Ekle" butonuna tıklayın** — Modal açılır
2. **Arama kutusu** — Stok kodu / dış kod / ürün adı ile ara
3. **Checkbox listesi** — Bir seferde birden fazla ürün seçin (alt+tıkla = toplu seçim)
4. **"Tümünü Seç / Temizle"** — Listedeki tüm ürünleri hızlı seçim
5. **Seçili her ürün için:** Standart Miktar, Birim, Kullanım Yeri (Gövde/Sap/Dip Kapak/Etiket/Diğer) girin
6. **"Tümünü Kaydet" butonuna tıklayın** — Tüm seçimleri batch insert (hata varsa listelenip tekrar deneyin)

| Alan | Açıklama | Örnek |
|------|----------|-------|
| Stok Kodu | Ürünün İç kodu (Kodlanmış etiket) | KRAFT-80-BOB |
| Dış Kod | Ürünün tedarikçi/sistem kodu | EXT-12345 |
| Ürün Adı | Ürünün tam adı | Kraft Kağıt 80gr Bobin |
| Standart Miktar | 1 birim mamul için gereken miktar | 0.082 |
| Birim | kg, Adet, m², m, lt | kg |
| Kullanım Yeri | Mamülün hangi bölümünde kullanıldığı (opsiyonel) | Gövde |

**Tek Bileşen Düzenleme:**

Mevcut reçete satırına tıklandığında tekli edit modu açılır:
- Ürün read-only gösterilir (değiştiremez)
- Sadece Standart Miktar, Birim, Kullanım Yeri düzenlenebilir

> **Not:** Aynı bileşen iki kez eklenemez — sistem uyarı verir. Batch işlem sırasında hata varsa sadece başarısız kayıt tekrar denenir.

### İşlemler

| Buton | Açıklama |
|-------|----------|
| Düzenle (kalem) | Miktarı, birimi veya yeri günceller |
| Pause/Play | Bileşeni pasife/aktife alır; pasif bileşenler otomatik yüklemeye dahil edilmez |
| Çöp kutusu | Bileşeni reçeteden kalıcı kaldırır |

### Üretimde Kullanımı

Üretim Detayı sayfasında tüketim girerken **Reçeteden Yükle** butonuna tıklandığında bu sayfadaki aktif bileşenler standart miktarlarıyla otomatik önerilir.

---

## 23. İmha Kayıtları {#imha}

**Menü Yolu:** Üretim → İmha Kayıtları

**Sayfa:** `/Production/WasteManagement`

```
[≡] [+ Yeni İmha Kaydı]   İmha Kayıtları   [← Fire Raporu] [👤]
```

Üretim sürecinde oluşan ve artık kullanılamaz hale gelen malzemelerin takibini sağlar. Her kayıt bir atık koduna (ATK-YYYY-NNN) bağlanır.

### İmha Kategorileri

| Kategori | Açıklama |
|----------|----------|
| Kesim Artığı | Kesim makinesinden çıkan kenar ve kırpıntılar |
| Baskı Artığı | Hatalı baskı veya boya denemeleri |
| Islanma/Hasarı | Su veya nem nedeniyle bozulan malzeme |
| Nakliye Hasarı | Taşıma sırasında hasar gören ürün |
| Makine Hatası | Makine arızası/kalibrasyonu sırasında çıkan fire |
| Diğer | Yukarıdakilerin dışındaki imha kalemleri |

### İmha Kaydı Alanları

| Alan | Açıklama | Zorunlu |
|------|----------|---------|
| İmha Kodu | Otomatik üretilir (ATK2026-001) | — |
| İş Emri | Hangi üretimle ilişkili olduğu | Hayır |
| Kategori | Atık türü | Evet |
| Açıklama | Serbest metin | Evet |
| Miktar | Sayısal değer | Evet |
| Birim | kg, Adet, m², m, lt | Evet |
| İmha Tarihi | Gerçekleşme tarihi | Evet |
| İmha Yöntemi | Geri Dönüşüm, Yakma, Çöp vb. | Hayır |
| İmha Eden | Sorumlu kişi | Hayır |
| Notlar | Ek açıklama | Hayır |

> **İpucu:** Fire Raporu sayfasında görüntülenen fire miktarları üretim tüketiminden otomatik hesaplanır; İmha Kayıtları ise fiziksel imha belgesi niteliğindedir. Her ikisini birlikte kullanarak FSC denetiminde tam iz bırakabilirsiniz.

---

## 📋 Modül Durumu

| Modül | Durum | Kılavuz Bölümü |
|-------|-------|----------------|
| Giriş / Çıkış | ✅ Tamamlandı | Bölüm 1 |
| Tedarikçiler | ✅ Tamamlandı | Bölüm 4 |
| Müşteriler | ✅ Tamamlandı | Bölüm 5 |
| Ürünler | ✅ Tamamlandı | Bölüm 6 |
| Tanımlamalar | ✅ Tamamlandı | Bölüm 7 |
| Depolar | ✅ Tamamlandı | Bölüm 21 |
| Ürün Reçetesi (BOM) | ✅ Tamamlandı | Bölüm 22 |
| İmha Kayıtları | ✅ Tamamlandı | Bölüm 23 |
| Hammadde Girişi | ✅ Tamamlandı | Bölüm 8–9 |
| Üretim / İş Emirleri | ✅ Tamamlandı | Bölüm 11–13 |
| Satış / Sevkiyat | ✅ Tamamlandı | Bölüm 14–15 |
| Stok Durumu | ✅ Tamamlandı | Bölüm 16–17 |
| FSC Raporlar | ✅ Tamamlandı | Bölüm 18–20 |
| Denetim Özet Raporu | ✅ Tamamlandı | Bölüm 27 |
| Tam İzlenebilirlik | ✅ Tamamlandı | Bölüm 28 |
| ERP Entegrasyon (ETL) | ✅ Tamamlandı | Bölüm 24 |

---

## 24. ERP Entegrasyonu — ETL {#etl}

**Menü Yolu:** Sol Menü → ERP Entegrasyon

**Sayfa:** `/Etl/Index`

```
[≡] [Excel Aktarımı]   ERP Entegrasyonu   [Netsis Sync] [Bağlantılar] [Geçmiş] [👤]
```

ERP sistemlerinden veri aktarımını (ETL — Extract, Transform, Load) yönetir. İki yöntem desteklenmektedir: **Excel tabanlı manuel aktarım** ve **Netsis doğrudan senkronizasyonu**.

### Alt Sayfalar

| Sayfa | URL | Açıklama |
|-------|-----|----------|
| ETL Paneli | `/Etl/Index` | Dashboard — istatistikler ve son aktarımlar |
| Excel Aktarımı | `/Etl/Import` | Dosya yükle → önizle → aktar |
| Netsis Sync | `/Etl/NetsisSync` | Netsis veritabanından doğrudan senkronizasyon |
| Bağlantı Yönetimi | `/Etl/Connections` | ERP bağlantı profillerinin CRUD yönetimi |
| Aktarım Geçmişi | `/Etl/History` | Tüm aktarım logları ve sonuçları |

### Excel Aktarım Türleri

Aktarımlar iki grupta kategorize edilmiştir:

#### FSC İşlem Kayıtları (Operasyonel)

| Tür | Anahtar Alan | Kolonlar |
|-----|-------------|---------|
| 🟢 Hammadde Lot Girişi | LotNo | LotNo, TedarikciKodu, UrunKodu, FscTipi, SeriNo, Miktar, AlisIrsaliyeNo, AlisFaturaNo, GirisTarihi, Plaka, Notlar |
| 🔵 Üretim Kaydı | UretimNo | UretimNo, Tarih, Makine, MamulKodu, UretimMiktari, LotNo, HammaddeKodu, KullanilanMiktar, Fire, Notlar |
| 🟠 Satış / Sevkiyat | SatisNo | SatisNo, Tarih, MusteriKodu, UrunKodu, Miktar, BirimFiyat, IrsaliyeNo, FaturaNo, Notlar |

#### Tanım Tabloları (Master Data)

| Tür | Kolonlar |
|-----|---------|
| Ürün Aktarımı | UrunKodu, UrunAdi, Birim, GrupAdi, IsActive |
| Tedarikçi Aktarımı | TedarikciKodu, TedarikciAdi, FscKodu, ContactPerson, Telefon, Email |
| Müşteri Aktarımı | MusteriKodu, MusteriAdi, VergiNo, VergiDairesi, Sehir, Telefon, Email |

### Şablon Kuralları

> **⚠️ Önemli:** Şablonlar DB'den gerçek geçerli değerleri içerecek şekilde üretilir. Boş sistemde şablonu indirdiyseniz önce Tedarikçi/Müşteri/Ürün/Makine tanımlamalarını yapın.

İndirilen Excel şablonunda 3 sayfa bulunur:

| Sayfa | İçerik |
|-------|--------|
| Veri Sayfası | Doldurulacak tablo (zorunlu `*` başlıklar koyu yeşil) |
| OKUYUN | Sütun açıklamaları, tarih/ondalık kuralları, geçerli değer listesi |
| _Referans | Gizli — dropdown kaynak listesi (silinmemeli) |

**Önemli format kuralları:**

| Alan | Kural | Örnek |
|------|-------|-------|
| Tarihler | gg.AA.yyyy (4 haneli yıl) | `15.05.2025` |
| Ondalıklı sayılar | Nokta veya virgül | `1757.50` veya `1757,50` |
| Kodlar | Sistemde kayıtlı olmalı | `TED-001`, `MHS-001` |
| FSC Tipi | Dropdown listesinden seç | `FSC 100%`, `FSC_MIX` |

### LotImport — Gruplama Mantığı

Aynı LotNo birden fazla satırda tekrarlanabilir — her satır aynı lot'a ait **ayrı bir bobin/seri** anlamına gelir:

```
LotNo     | TedarikciKodu | FscTipi | SeriNo      | Miktar
24H0537   | TED-001       | FSC_MIX | 24H0537-01  | 1757.50   ← Lot oluşturulur
24H0537   | TED-001       | FSC_MIX | 24H0537-02  | 1820.00   ← Aynı lot'a 2. bobin
24H0538   | TED-001       | FSC_MIX | 24H0538-01  | 1650.00   ← Yeni lot
```

Aynı mantık UretimImport (UretimNo) ve SatisImport (SatisNo) için de geçerlidir.

### Aktarım Adımları

1. `/Etl/Import` sayfasını açın
2. **Aktarım Türü** seçin (açılır listede iki grup var)
3. **Şablon İndir** ile boş Excel şablonunu indirin (ilk kullanımda)
4. Şablonu doldurun — OKUYUN sayfasındaki kurallara uyun
5. Doldurulmuş dosyayı seçin, **Önizle** ile ilk 10 satırı kontrol edin
6. **Aktarımı Başlat** — sonuç anında ekranda gösterilir
7. Aktarım Geçmişi'nde (`/Etl/History`) detaylı log görüntülenir

### Bağlantı Profili Tipleri

| Tip | Açıklama |
|-----|---------|
| Excel | Manuel dosya yükleme (aktif) |
| Netsis | Netsis ERP doğrudan SQL bağlantısı (aktif) |
| Logo | Logo ERP (gelecek faz) |
| Mikro | Mikro ERP (gelecek faz) |
| Api | REST API bağlantısı (gelecek faz) |

---

## 25. Netsis Senkronizasyonu {#netsis-sync}

**Menü Yolu:** Sol Menü → ERP Entegrasyon → Netsis Sync

**Sayfa:** `/Etl/NetsisSync`

```
[≡]   Netsis Senkronizasyonu   [ETL Paneli] [Geçmiş] [👤]
```

Netsis ERP veritabanına doğrudan SQL bağlantısı kurarak ürün, tedarikçi ve müşteri verilerini otomatik olarak FSC Takip sistemine çeker.

### Senkronizasyon Türleri

| Buton | Netsis Tablo | FSC Hedef | Filtre |
|-------|-------------|-----------|--------|
| Ürünleri Senkronize Et | `TBLSTSABIT` | Ürünler | Grup 10/20/30/40/50 |
| Tedarikçileri Senkronize Et | `TBLCASABIT` | Tedarikçiler | TİP = S |
| Müşterileri Senkronize Et | `TBLCASABIT` | Müşteriler | TİP = A |
| **Tümünü Senkronize Et** | Hepsi | Hepsi | Ürün → Tedarikçi → Müşteri sırasıyla |

### Bağlantı Kurulumu

1. Sol panelde **Bağlantı Profili** açılır listesi görünür (Netsis tipi bağlantı tanımlıysa)
2. Profil seçilmezse `appsettings.json`'daki `NetsisConnection` bağlantı dizesi kullanılır
3. Varsayılan bağlantı: `Server=ARDA\ARDA; Database=ACOREFSC25; User=data`

### Senkronizasyon Adımları

1. `/Etl/NetsisSync` sayfasını açın
2. Bağlantı profili seçin (veya varsayılan kullanın)
3. Senkronize etmek istediğiniz türün butonuna tıklayın
4. Onay sorusuna **Evet** yanıtı verin
5. Sağ panelde yükleniyor göstergesi görünür
6. Tamamlandığında: Eklendi / Güncellendi / Atlandı / Hata sayıları gösterilir

> **ℹ️ Upsert Mantığı:** Senkronizasyon her çalıştığında mevcut kayıtları günceller, yeni olanları ekler. Yinelenen çalıştırma zararsızdır.

> **⚠️ Hata Durumu:** Netsis bağlantısı kurulamazsa sağ panelde hata mesajı görünür. Bağlantı profilinin doğru yapılandırıldığını kontrol edin.

---

## 26. Üretim Fişi — Lot Evrak Görüntüleyici {#lot-evrak}

**Sayfa:** `/Production/Detail/{id}` → Tüketim Tablosu → LotNo tıklama

Bu özellik, bir üretim fişindeki hammadde tüketim satırlarından doğrudan o hammaddenin **giriş evraklarına** (fatura ve irsaliye PDF) erişim sağlar. Hammaddeden üretime, üretimden satışa kadar tam izlenebilirlik zincirinin kritik halkasıdır.

### Nasıl Kullanılır

1. `/Production/Detail/{id}` sayfasını açın
2. Sağ panelde **Tüketim Kayıtları** tablosunu bulun
3. **Lot / Tedarikçi** sütununda LotNo'nun yanındaki 📂 simgesiyle birlikte mavi linkli lot numarasına tıklayın
4. Modal penceresi açılır ve şunları gösterir:

### Modal İçeriği

**Lot Bilgileri (üst bölüm):**

| Alan | Açıklama |
|------|----------|
| Lot No | Tedarikçi tarafından verilen lot/parti numarası |
| Tedarikçi | Tedarikçi adı ve kodu |
| FSC Tipi | FSC sertifika tipi (yeşil badge ile) |
| Hammadde | Ürün adı ve kodu |
| Giriş Tarihi | Hammaddenin sisteme giriş tarihi |
| Plaka | Varsa araç plakası |

**Evrak Panelleri (alt bölüm):**

| Panel | İçerik |
|-------|--------|
| Alış Faturası | Fatura No + fatura tutarı + **"Fatura PDF'ini Aç"** butonu |
| Alış İrsaliyesi | İrsaliye No + **"İrsaliye PDF'ini Aç"** butonu |

> **ℹ️ PDF Yüklenmemişse:** "PDF henüz yüklenmemiş" bilgisi gösterilir. PDF'leri yüklemek için `/Purchase/Detail/{lotId}` sayfasından Lot Detayı'nı açın.

**Alt Butonlar:**
- **Lot Detayına Git** → Hammadde giriş sayfasını açar (`/Purchase/Detail/{lotId}`)
- **Kapat** → Modalı kapatır

### FSC CoC Zinciri

Bu özellik sayesinde herhangi bir üretim fişinde:
- Hangi hammaddeden üretildi?
- O hammadde hangi tedarikçiden geldi?
- FSC sertifika tipi neydi?
- Giriş faturası ve irsaliyesi hangileri?

sorularının cevapları tek tıkla erişilebilir durumdadır.

> **⚠️ İzlenebilirlik Notu:** FSC CoC denetimlerinde denetçiler bu belgeleri talep eder. PDF'lerin sisteme yüklü olması denetim sürecini önemli ölçüde hızlandırır.

---

### Modül Durumu

| Modül | Durum | Versiyon |
|-------|-------|---------|
| Tedarikçiler | ✅ Tamamlandı | 1.0 |
| Müşteriler | ✅ Tamamlandı | 1.0 |
| Ürünler + Reçete | ✅ Tamamlandı | 1.1 |
| Sistem Tanımlamaları | ✅ Tamamlandı | 1.1 |
| Hammadde Girişi (Purchase) | ✅ Tamamlandı | 1.2 |
| Üretim / İş Emirleri | ✅ Tamamlandı | 1.3 |
| Fire Raporu | ✅ Tamamlandı | 1.4 |
| Satış Siparişleri | ✅ Tamamlandı | 1.5 |
| Stok Durumu + Hareketleri | ✅ Tamamlandı | 1.6 |
| FSC CoC + Lot Takip Raporu | ✅ Tamamlandı | 1.6 |
| İmha Kayıtları | ✅ Tamamlandı | 1.7 |
| ERP Entegrasyon — Bağlantılar | ✅ Tamamlandı | 1.7 |
| Excel Aktarımı (Tanım Tabloları) | ✅ Tamamlandı | 1.8 |
| Excel Aktarımı (Lot/Üretim/Satış) | ✅ Tamamlandı | 1.9 |
| Netsis Senkronizasyonu | ✅ Tamamlandı | 1.9 |
| Lot Evrak Görüntüleyici | ✅ Tamamlandı | 1.9 |
| Gelişmiş Excel Şablonları | ✅ Tamamlandı | 1.9 |
| Denetim Özet Raporu | ✅ Tamamlandı | 2.0 |
| Tam İzlenebilirlik (Satış→Lot) | ✅ Tamamlandı | 2.0 |
| BOM Bileşen Analizi | ✅ Tamamlandı | 2.1 |
| Netsis ETL Excel Dosyaları | ✅ Tamamlandı | 2.2 |
| Hammadde Stoğu — Bobin Bazlı | ✅ Tamamlandı | 2.3 |
| ETL Otomatik Algıla Import | ✅ Tamamlandı | 2.4 |
| Satış & Fatura ETL Dosyaları | ✅ Tamamlandı | 2.4 |

---

## 27. Denetim Özet Raporu {#denetim-ozet}

**Menü Yolu:** Sol Menü → FSC Raporlar → Denetim Özeti  
**Sayfa:** `/Reports/AuditReport`

```
[≡]   Denetim Özet Raporu   [Excel İndir] [Yazdır] [👤]
```

FSC CoC denetimlerinde sorulacak tüm sorulara tek sayfada cevap veren kapsamlı denge ve izlenebilirlik raporu.

### İçerik

| Bölüm | Açıklama |
|-------|----------|
| **Dönem Filtresi** | Başlangıç–bitiş tarihi ile filtreleme |
| **5 Özet Kart** | Giriş kg · Tüketim kg · Fire kg · Satış adet · Denge durumu |
| **FSC Denge Özeti** | FSC tip bazında giriş / tüketim / fire / stok denge kontrolü |
| **Hammadde Girişleri** | Lot ve bobin sayısı, tedarikçi, fatura/irsaliye bilgileri |
| **Üretim Tüketimi** | İş emri bazında tüketim, fire, üretim miktarı |
| **Satış Sevkiyatları** | Müşteri, ürün, iş emri bağlantısı, CoC durumu |

### FSC Denge Formülü

```
Giriş(kg) ≥ Tüketim(kg) + Fire(kg)    → ✓ Dengeli
Giriş(kg) < Tüketim(kg) + Fire(kg)    → ⚠ Kontrol Et
```

### Excel İndirme

Rapor 4 ayrı sayfadan oluşan Excel dosyası olarak indirilir:
1. Hammadde Girişleri
2. Üretim Tüketimi
3. Satış Sevkiyatları
4. FSC Denge Özeti

> **ℹ️ Denetim İpucu:** Bu raporu FSC denetim döneminin başlangıç ve bitiş tarihleriyle filtreleyin. "Denge Özeti" tablosundaki tüm satırlar ✓ Dengeli göstermelidir. Eksik satırlar varsa ilgili iş emirleri veya lot kayıtlarını kontrol edin.

---

## 28. Tam İzlenebilirlik — Satış → Üretim → Lot {#tam-izlenebilirlik}

**Menü Yolu:** Sol Menü → Satış → Sipariş Detayı → Tam İzlenebilirlik butonu  
**Sayfa:** `/Reports/Traceability/{siparisId}`

```
[≡] [← Sipariş Detayı] [Yazdır]   Tam İzlenebilirlik — SIS-001   [👤]
```

Seçilen satış siparişindeki her ürün kaleminin hammadde lot'larına kadar tam tedarik zincirini görsel olarak sunar.

### Zincir Yapısı

```
SATIŞ SİPARİŞİ (Müşteri, Tarih, Durum, FSC Lisans)
  └── ÜRÜN KALEMİ (Ürün, Miktar, Birim)
        └── İŞ EMRİ (Makine, Plan Tarihi, Durum)
              └── LOT (Lot No, FSC Tipi, Giriş Tarihi)
                    ├── Tedarikçi Bilgisi + FSC Sertifika Durumu
                    ├── Fatura / İrsaliye PDF Linkleri
                    └── BOBİN TABLOSU (Seri No, Tüketim kg, Fire kg, Üretim Adet)
```

### Renk Kodları

| Renk | Anlam |
|------|-------|
| 🟢 Yeşil sol çizgi (Lot) | Tedarikçi FSC sertifikası geçerli |
| 🔴 Kırmızı sol çizgi (Lot) | FSC sertifikası geçersiz veya süresi dolmuş |
| ✓ FSC Tam badge | Tüm lot grupları geçerli FSC sertifikasına sahip |
| ⚠ Eksik badge | En az bir lot'ta sorun var |

### Nasıl Kullanılır

1. Sol Menü → **Satışlar** → ilgili siparişe tıklayın
2. Sipariş Detayı sayfasında üst sağdaki **"Tam İzlenebilirlik"** butonuna tıklayın
3. Açılan sayfada satıştan hammaddeye kadar tüm zinciri inceleyin
4. Kart başlıklarına tıklayarak ilgili bölümü daraltabilir / genişletebilirsiniz
5. **"Üretim Fişi"** butonu ile ilgili iş emri detayına geçebilirsiniz
6. **"Lot Detayına Git"** ile hammadde giriş sayfasına ulaşabilirsiniz

> **⚠️ FSC CoC Uyarısı:** Denetçiler, her sevk edilen üründe bu zincirin eksiksiz olduğunu ve tüm tedarikçi FSC sertifikalarının sevkiyat tarihi itibarıyla geçerli olduğunu kontrol eder. "FSC Zinciri Eksik" uyarısı varsa ilgili satış kaleminin iş emri bağlantısını tamamlayın.

---

## 29. BOM Bileşen Analizi {#bom-analizi}

**Menü Yolu:** Sol Menü → BOM Analizi  
**Sayfa:** `/Reports/BomAnalysis`

```
[≡] [Excel]   BOM Bileşen Analizi   [Denetim Özeti] [👤]
     [Başlangıç Tarihi] [Bitiş Tarihi] [İş Emri (Opsiyonel)] [Filtrele]
```

Seçilen dönemdeki tüm iş emirleri için **reçete bileşeni (BOM) bazlı hammadde tüketim ve fire analizini** gösterir. FSC CoC denetiminde "hangi bileşen için ne kadar hangi tip hammadde kullanıldı, ne kadar fire çıktı" sorusunu yanıtlar.

### Ekran Yapısı

Her iş emri için ayrı bir kart görünür:

**Kart başlığı (koyu yeşil):** İş emri no (linke tıklanabilir), mamul adı, FSC tipi, durum, tarih, makine, toplam üretilen adet.

**Özet çizgisi:** Plan, üretilen, toplam tüketim, toplam fire, fire %.

**BOM tablosu — sütunlar:**

| Sütun | Açıklama |
|-------|----------|
| Bileşen / Hammadde | Reçete bileşeninin adı ve kodu |
| FSC Tipi | Bileşen ürün kartındaki FSC tipi + gerçek giriş malzemesinin FSC tipi |
| Planlanan | WorkOrderRecipe'deki planlanan miktar (kg) |
| Tüketilen | Gerçek tüketim (kg) + plana göre yüzde |
| Fire | Fire miktarı (kg) |
| Fire % | Fire yüzdesi (yeşil <3%, sarı 3-6%, kırmızı >6%) |
| Üretilen | Bu bileşenden üretilen adet |
| Mass-Balance | ✓ Dengeli / ⚠ Plan Aşıldı / devam ediyor |
| Kaynaklar | Kullanılan lot no ve tedarikçi |

### Renk Kodları

| Renk | Anlam |
|------|-------|
| Normal arka plan | Bileşen planı dahilinde |
| Kırmızı arka plan | Plan %110'dan fazla aşıldı |
| Sarı arka plan (⚡ satır) | Reçete bileşenine bağlanmamış serbest tüketim |
| Yeşil toplam satırı | İş emri toplamı |

### Serbest Tüketim Uyarısı

Tüketim girilirken "Reçete Bileşeni" seçilmemiş kayıtlar sarı "Serbest Tüketim" satırı olarak görünür. FSC CoC mass-balance analizi için **tüm tüketim kayıtlarının bir bileşene bağlı olması** gerekir.

### Nasıl Kullanılır

1. Sol Menü → **BOM Analizi** tıklayın
2. Dönem filtresi girin (varsayılan: bu yılın başı — bugün)
3. Belirli bir iş emrini incelemek için İş Emri açılır listesini kullanın
4. İş emri başlığındaki linke tıklayarak Üretim Detayı'na geçebilirsiniz
5. **Excel** ile tam raporu indirin — iş emri × bileşen bazında tüm satırlar dışa aktarılır
6. Serbest tüketim varsa iş emri sayfasına gidip o tüketim kaydını düzenleyerek bileşene bağlayın

### Excel İndirme

Sütunlar: İş Emri No · Mamul · Durum · Plan Tarihi · Bileşen · Bileşen FSC Tipi · Planlanan (kg) · Gerçek Tüketim (kg) · Fire (kg) · Üretilen (adet) · Mass-Balance · Kullanılan Lotlar · Tedarikçiler

> **ℹ️ BOM Tanımlama İpucu:** BOM bileşenleri iki yoldan eklenebilir:  
> 1. **Ürün Reçetesi** sayfasından ürüne kalıcı standart reçete tanımlayın  
> 2. **Üretim Detayı** sayfasındaki "Bileşen Ekle" butonu ile iş emri özelinde ekleyin  
> Her iki yöntem de bu raporda görünür. BOM tanımlı olmayan iş emirleri sarı uyarı ile gösterilir.

> **⚠️ FSC CoC Uyarısı:** Denetimde her mamul bileşeni için ayrı hammadde takibi istenebilir. Kraft torba üretiminde gövde, sap ve etiket bileşenlerinin her birinin ayrı lot ve FSC tipiyle eşleştirilmesi mass-balance hesabını kesinleştirir.

---

## 30. Netsis ETL Excel Dosyaları {#netsis-etl-excel}

**Sayfa:** `/Etl/NetsisSync`  
**Menü:** Sol Menü → ERP Entegrasyon → Netsis Senkronizasyonu

ACOREFSC25 (Netsis FSC izleme veritabanı) ve ACORE23 (Netsis ana veritabanı) kaynaklarından çıkarılmış, FSC Takip ERP'ye toplu aktarım için hazır Excel dosyalarıdır.

```
[≡] [Bağlantılar]   Netsis Senkronizasyonu   [📥 ETL Excel ▼]   [👤]
```

### Mevcut Dosyalar

| Dosya | Kaynak | Kayıt Sayısı | Kapsam |
|-------|--------|--------------|--------|
| **ETL_Tedarikciler.xlsx** | ACOREFSC25 · TBLCASABIT (S tipi) | 16 tedarikçi | Hammadde tedarikçileri |
| **ETL_Musteriler.xlsx** | ACORE23 · TBLCASABIT (A tipi) | 316 müşteri | Aktif müşteri kartları |
| **ETL_HammaddeGirisleri.xlsx** | ACOREFSC25 · TBLSTHAR (G girişleri) | 160 satır / 151 lot | 2022–2025 alış hareketleri |
| **ETL_SatisGirisleri.xlsx** | ACOREFSC25 · TBLSTHAR (C çıkışları) | 734 çıkış hareketi | Grup 10/20/30/40 satış hareketleri |
| **ETL_FaturaListesi.xlsx** | ACOREFSC25 · TBLFATUIRS | 206 fatura/irsaliye | Tüm satış faturaları ve irsaliyeleri |

### İndirme

Netsis Senkronizasyonu sayfasının altındaki **"Netsis ETL Excel Dosyaları"** kartından her dosya tek tıkla indirilir. Dosya adı indirildiği tarihe göre otomatik güncellenir (`ETL_Tedarikciler_19052026.xlsx` gibi).

### Renk Kodları

| Renk | Anlam |
|------|-------|
| **Beyaz / normal** | Otomatik çıkarılan, doğrulanmış veriler |
| **Sarı arka plan** | Manuel doldurulması gereken alanlar (FSC Kodu, Sertifika Bitiş Tarihi) |
| **Yeşil başlık satırı** | Sütun isimleri (içe aktarma sütun eşleşmesi bu isimlere göre yapılır) |

### Tedarikçi Dosyası (ETL_Tedarikciler.xlsx)

Sütunlar:

| Sütun | Dolu mu? | Açıklama |
|-------|----------|----------|
| TedarikciAdi | ✓ | TBLCASABIT.CARI_ISIM (temizlenmiş) |
| Telefon | ✓ | CARI_TEL / GSM1 |
| Email | ✓ | EMAIL |
| Adres | ✓ | CARI_ADRES |
| Sehir | ✓ | CARI_IL |
| VergiDairesi | ✓ | VERGI_DAIRESI |
| VergiNo | ✓ | VERGI_NUMARASI |
| FscKodu | ⚠ Sarı | Manuel doldurulacak |
| FscBitisTarihi | ⚠ Sarı | Manuel doldurulacak |

### Müşteri Dosyası (ETL_Musteriler.xlsx)

Sütunlar:

| Sütun | Dolu mu? | Açıklama |
|-------|----------|----------|
| MusteriAdi | ✓ | TBLCASABIT.CARI_ISIM (temizlenmiş) |
| Telefon | ✓ | CARI_TEL / GSM1 |
| Email | ✓ | EMAIL |
| Adres | ✓ | CARI_ADRES |
| Sehir | ✓ | CARI_IL |
| VergiDairesi | ✓ | VERGI_DAIRESI |
| VergiNo | ✓ | VERGI_NUMARASI |
| FscLisansKodu | ⚠ Sarı | Manuel doldurulacak |
| FscBitisTarihi | ⚠ Sarı | Manuel doldurulacak |

### Hammadde Girişleri Dosyası (ETL_HammaddeGirisleri.xlsx)

Her satır bir FscSerial kaydına, her LotNo grubu bir FscLot kaydına karşılık gelir.

Sütunlar:

| Sütun | Dolu mu? | Açıklama |
|-------|----------|----------|
| LotNo | ✓ | L{YYYY}-{NNN} formatında otomatik üretildi |
| SeriNo | ✓ | S{YYYY}-{NNN}-{NN} formatında |
| StokKodu | ✓ | TBLSTSABIT.STOK_KODU |
| StokAdi | ✓ | TBLSTSABIT.STOK_ADI (temizlenmiş) |
| FscTipi | ✓ | FSC-100 / FSC-MIX / FSC-MIX-70 vb. (stok adından çıkarıldı) |
| Tedarikci | ✓ | Cari kodu veya stok adından tahmin edildi |
| Miktar_kg | ✓ | STHAR_GCMIK |
| Tarih | ✓ | STHAR tarih alanı |
| FisNo | ✓ | TBLSTHAR.FISNO (fiş/irsaliye numarası) |
| IrsaliyeNo | ✓ | IRSALIYE_NO (varsa) |
| DepoKodu | ✓ | DEPO_KODU |

### Satış Girişleri Dosyası (ETL_SatisGirisleri.xlsx)

TBLSTHAR tablosundan GCKOD='C' (çıkış) kayıtları. Grup dağılımı: Grup 30 (282 kayıt), Grup 10 (248), Grup 20 (183), Grup 40 (21).

| Sütun | Dolu mu? | Açıklama |
|-------|----------|----------|
| StokKodu | ✓ | TBLSTSABIT.STOK_KODU |
| StokAdi | ✓ | Stok adı (temizlenmiş) |
| GrupKodu | ✓ | Ürün grubu (10/20/30/40) |
| CariKod | ✓ | Müşteri cari kodu |
| CariIsim | ✓ | Müşteri adı |
| Tarih | ✓ | İşlem tarihi |
| FisNo | ✓ | Fiş/irsaliye numarası |
| IrsaliyeNo | ✓ | İrsaliye no (varsa) |
| Miktar | ✓ | Çıkış miktarı |
| Htur | ✓ | Hareket türü (B=Fatura, H=Diğer) |
| DepoKodu | ✓ | Kaynak depo |

> **ℹ️ Bilgi:** Bu dosya referans / analiz amaçlıdır. ETL Otomatik Algıla import ile satış hareketi girişi henüz desteklenmemektedir — satış fişleri SalesController üzerinden yapılacaktır.

### Fatura Listesi Dosyası (ETL_FaturaListesi.xlsx)

TBLFATUIRS tablosundan tüm fatura ve irsaliye belgeleri.

| Sütun | Dolu mu? | Açıklama |
|-------|----------|----------|
| BelgeTuru | ✓ | F=Fatura, İ=İrsaliye (FTIRSIP) |
| FisNo | ✓ | Fiş numarası (FATIRS_NO) |
| CariKod | ✓ | Müşteri cari kodu |
| CariIsim | ✓ | Müşteri adı |
| Tarih | ✓ | Belge tarihi |
| BrutTutar | ✓ | Brüt tutar (TL) |
| KdvTutar | ✓ | KDV tutarı (TL) |
| ToplamTutar | ✓ | Net toplam (Brüt + KDV) |
| ParaBirimi | ✓ | TL / USD / EUR |

> **ℹ️ Bilgi:** Fatura listesi referans / mutabakat amaçlıdır. Fatura PDF yükleme işlemi PurchaseController ve SalesController üzerinden yapılacaktır.

### Aktarım Sırası

Dosyaları şu sırayla kullanın:

1. **Tedarikçiler** — Önce tedarikçileri sisteme ekleyin (sarı alanları doldurun)
2. **Müşteriler** — Ardından müşteri kartlarını aktarın
3. **Hammadde Girişleri** — En son lot/seri kayıtlarını girin (tedarikçi eşleşmesi için 1. adım tamamlanmış olmalı)
4. **Satış Girişleri** — İsteğe bağlı; satış geçmişini analiz etmek için gözden geçirin
5. **Fatura Listesi** — Mutabakat ve belge takibi için referans olarak kullanın

> **⚠️ Sarı Alan Uyarısı:** FSC Kodu ve Sertifika Bitiş Tarihi alanları Netsis veritabanında bulunmadığından boş gelir. Bu alanlar tedarikçi/müşteri ile birebir görüşülerek veya FSC CoC lisans sorgulama portalından doğrulanarak doldurulmalıdır.

> **ℹ️ Açılış Bakiyesi:** HTUR='A' (açılış devir) kayıtlarında STHAR_CARIKOD genellikle boş gelir. Bu durumda tedarikçi adı stok adındaki anahtar kelimelerden tahmin edilmiştir (KMK, MONDI, BILLERUD vb.). Aktarım öncesinde tedarikçi sütununu doğrulayın.

> **ℹ️ Lot Gruplama Mantığı:** Her FISNO + STOK_KODU kombinasyonu tek bir FscLot olarak tanımlanmıştır. FISNO boş olan açılış devir kayıtları `ACILIS-{tarih}-{stokKodu}` anahtarıyla gruplandırılmıştır.

---

## 31. Hammadde Stoğu — Bobin Bazlı {#hammadde-stogu}

**Sayfa:** `/Stock/RawMaterial`  
**Menü:** Sol Menü → FSC İşlemleri → Hammadde Stoğu

FSC CoC (Chain of Custody) mass-balance hesabı için en kritik ekran. Her FscSerial (bobin/rulo) kaydının anlık kalan ağırlığını, FSC tipini ve hangi lot'a ait olduğunu gösterir.

```
[≡] [← Hammadde Girişi]   Hammadde Stoğu   [Filtrele] [Excel] [👤]
```

### Özet Kartlar

| Kart | Gösterge |
|------|----------|
| Toplam Kalan Ağırlık | Tüm aktif bobinlerin CurrentWeight toplamı (kg) |
| Bobin / Seri | Aktif seri kayıt sayısı |
| Lot Sayısı | Kaç farklı lot'tan geldiği |
| FSC-100 Stoku | Yalnızca FSC-100 sertifikalı bobin ağırlığı |

### FSC Tipi Dağılım Bandı

Özet kartların altında her FSC tipinin kalan kg ve yüzdesi renkli kutularda gösterilir:
- **Yeşil kutu** — FSC-100 (en katı sertifika)
- **Mavi kutu** — FSC-MIX sınıfları
- **Sarı kutu** — FSC-RECYCLED

### Tablo Sütunları

| Sütun | Açıklama |
|-------|----------|
| Seri No | FscSerial.SerialNo — lot detayına link |
| Lot No | Bağlı FscLot.LotNo — lot detayına link |
| Stok Adı | Hammadde ürün adı |
| Tedarikçi | Tedarikçi firma adı |
| FSC Tipi | FSC sertifika sınıfı (renkli badge) |
| Giriş (kg) | InitialWeight — sisteme ilk girildiğindeki ağırlık |
| Kalan (kg) | CurrentWeight — anlık kalan ağırlık |
| Tüketim % | İlerleme çubuğu: kırmızı >%90, sarı >%50, yeşil diğer |
| Durum | Mevcut / Az Kaldı (%10 altı) / Tükendi |

### Filtreler

| Alan | Açıklama |
|------|----------|
| FSC Tipi | Belirli sertifika sınıfını göster |
| Tedarikçi | Tedarikçiye göre filtrele |
| Stok / Ürün | Belirli hammadde türüne göre filtrele |
| Tükenenler dahil | İşaretlenirse CurrentWeight=0 bobinler de gösterilir |

### Kullanım

1. Sol Menü → **Hammadde Stoğu** tıklayın
2. Varsayılan görünümde yalnızca aktif (CurrentWeight > 0) bobinler listelenir
3. **Filtrele** ile FSC tipine veya tedarikçiye göre daraltın — denetim anında hangi FSC sınıfından kaç kg kaldığını gösterir
4. **Excel** ile tüm aktif stoğu dışa aktarın
5. Seri No veya Lot No linkine tıklayarak Lot Detayı sayfasına geçin

### Excel İndirme

Sütunlar: Seri No · Lot No · Ürün · Tedarikçi · FSC Tipi · Giriş kg · Kalan kg · Tüketim kg · % Kalan · Açılış Devir · Lot Tarihi · Fatura No · İrsaliye No · Notlar

> **ℹ️ FSC CoC Mass-Balance:** Denetimde "hangi FSC tipinden kaç kg hammadde girdiniz, kaç kg kullandınız, kaç kg hâlâ var?" sorusu sorulur. Bu ekran anlık kalan stoku FSC tipi bazında gösterir. CoC raporu ile birlikte kullanıldığında dönem içi giriş-çıkış-kalan dengesi tam olarak izlenebilir.

> **⚠️ CurrentWeight Güncelleme:** Kalan ağırlık (CurrentWeight) üretimde hammadde tüketim kaydedilirken otomatik düşürülür. Tüketim kaydı girilmemiş iş emirleri bu değeri güncellemez — tüm tüketimlerin sisteme girilmesi FSC doğruluğu için zorunludur.

---

## 32. ETL Otomatik Algıla Import {#etl-oto-import}

**Sayfa:** `/Etl/Import`  
**Menü:** Sol Menü → ERP Entegrasyon → Excel Aktarımı

Netsis ETL Excel dosyalarını (ETL_Tedarikciler / ETL_Musteriler / ETL_HammaddeGirisleri) **şablon indirmeye gerek kalmadan** doğrudan içe aktarır. Başlık satırı okunarak dosya türü otomatik belirlenir.

### Kullanım Adımları

1. Sol Menü → **ERP Entegrasyon** → **Excel Aktarımı**
2. **Aktarım Türü** açılır listesinden **"⭐ ETL Otomatik Algıla"** seçin
3. Netsis → Senkronizasyon sayfasından indirdiğiniz ETL Excel dosyasını seçin
4. **Önizle** butonuyla ilk 10 satırı kontrol edin
5. **Aktarmayı Başlat** — sistem dosya tipini otomatik algılar ve içe aktarır

### Otomatik Algılama Mantığı

| Başlık Satırında Bulunan | Algılanan Tip |
|--------------------------|---------------|
| `TedarikciAdi` sütunu | Tedarikçi ETL (ETL_Tedarikciler.xlsx) |
| `MusteriAdi` sütunu | Müşteri ETL (ETL_Musteriler.xlsx) |
| `LotNo` + `SeriNo` sütunları | Hammadde ETL (ETL_HammaddeGirisleri.xlsx) |

### Sütun Eşleştirme (ETL Formatı)

**Tedarikçi dosyası:** TedarikciAdi · Telefon · Email · Adres · Sehir · VergiDairesi · VergiNo · FscKodu · FscBitisTarihi

**Müşteri dosyası:** MusteriAdi · Telefon · Email · Adres · Sehir · VergiDairesi · VergiNo · FscLisansKodu · FscBitisTarihi

**Hammadde dosyası:** LotNo · SeriNo · StokKodu · StokAdi · FscTipi · Tedarikci · Miktar_kg · Tarih · FisNo · IrsaliyeNo · DepoKodu

### Sonuç Raporu

Aktarım bittikten sonra gösterilen özet:

| Gösterge | Açıklama |
|----------|----------|
| Eklendi | Yeni kayıt sayısı |
| Güncellendi | Mevcut eşleşip güncellenen |
| Atlandı | Boş satır veya eşleşme bulunamayan |
| Hata | Açıklama listesinde gösterilir |

### Çakışma Kuralları

- **Tedarikçi:** Aynı `Name` veya `TaxNumber` varsa günceller, yoksa `TED-NNN` kodu ile ekler
- **Müşteri:** Aynı `Name` veya `TaxNumber` varsa günceller, yoksa `MHS-NNN` kodu ile ekler
- **Hammadde:** Aynı `LotNo` varsa lot güncellenmez, ancak aynı `LotNo+SeriNo` yoksa yeni seri eklenir

### Aktarım Sonrası

- Tedarikçi/Müşteri için FSC Kodu ve Bitiş Tarihi sarı alanlar Tedarikçiler/Müşteriler listesinden düzenlenmelidir
- Hammadde için ürün `StokKodu` FSC ERP'deki `ProductCode` ile eşleşmelidir — ürün bulunamazsa satır hata listesine düşer

> **ℹ️ Sıralama:** Hammadde aktarımı öncesinde tedarikçi ve ürün kartları sistemde mevcut olmalıdır. Önce Tedarikçi → sonra Ürünler → son olarak Hammadde dosyasını aktarın.

---

---

## 33. Kullanıcı ve Yetki Yönetimi {#kullanici-yonetimi}

**Sayfalar:** `/Users/Index` · `/Users/Detail/{id}` · `/Groups/Index` · `/Groups/Detail/{id}` · `/AuditLog/Index`  
**Menü:** Sol Menü → Sistem Yönetimi (yalnızca admin kullanıcılara görünür)

```
[≡] [+ Yeni Kullanıcı]   Kullanıcı Yönetimi   [👤 Admin]
```

### Kullanıcı Yönetimi

| Alan | Açıklama |
|------|----------|
| Kullanıcı Adı | Giriş için kullanılır (benzersiz) |
| Ad Soyad | Görüntülenen isim |
| Gruplar | Kullanıcının dahil olduğu yetki grupları |
| Admin | ✓ işaretlenirse tüm yetkilere sahip olur |

**Şifre sıfırlama:** Düzenle → Yeni Şifre alanını doldurup Kaydet.

### Yetki Grupları

Gruplar modül bazlı **Okuma / Yazma / Silme** yetkisi verir:

| Yetki | Açıklama |
|-------|----------|
| Okuma | Sayfayı görüntüleyebilir |
| Yazma | Kayıt ekleyip düzenleyebilir |
| Silme | Kayıt silebilir |

Bir kullanıcı birden fazla gruba dahil olabilir; yetkiler **OR** mantığıyla birleşir.

### Kullanıcı Bazlı Override

`/Users/Detail/{id}` sayfasında her modül için bireysel yetki geçersiz kılma:

| Değer | Anlamı |
|-------|--------|
| `—` | Grup izinlerini miras al (varsayılan) |
| `✓` | Grup izninden bağımsız olarak her zaman VER |
| `✗` | Grup izninden bağımsız olarak her zaman ENGELLE |

### Denetim Kaydı (AuditLog)

`/AuditLog/Index` sayfası tüm INSERT / UPDATE / DELETE işlemlerini kaydeder.  
Filtreler: Tablo · İşlem Tipi · Kullanıcı · Tarih Aralığı  
Detay sayfasında eski ve yeni değerler yan yana görüntülenir.

> **⚠️ Güvenlik:** Admin şifresini yalnızca ilk kurulum sırasında değiştirin. Şifreler SHA-256 + salt ile şifrelenir.

---

## 34. Müşteri FSC Lisans Durumu {#musteri-fsc}

**Sayfa:** `/Reports/CustomerFsc`  
**Menü:** Sol Menü → Raporlar → Müşteri FSC

```
[≡]   Müşteri FSC Lisans Durumu   [👤]
```

Tedarikçi FSC sayfasının müşteri karşılığı. Aktif müşterilerin FSC lisans bitiş tarihlerini ve geçerlilik durumlarını gösterir.

| Kart | İçerik |
|------|--------|
| Geçerli Lisans | 30 günden fazla süresi kalan |
| 30 Gün İçinde Bitiyor | Kritik uyarı |
| Süresi Geçmiş / Pasif | Satışta CoC riski |

> **⚠️ FSC CoC:** Lisansı sona ermiş müşteriye FSC etiketli ürün satmak CoC zincirini kırar. Denetimde bu tablonun tüm satırları **Geçerli** olmalıdır.

---

## 35. Üretim Planı Takvimi {#uretim-plani}

**Sayfa:** `/Planning`  
**Menü:** Sol Menü → FSC Süreçleri → Üretim Planı

```
[≡] [← Önceki] [Mayıs 2026] [Sonraki →] [Bugün] [İş Emirleri]   Üretim Planı   [👤]
```

FullCalendar takvim görünümü ile iş emirlerini planlı tarihlerine göre görselleştirir.

### Renk Kodu

| Renk | Anlamı |
|------|--------|
| Mavi | Üretimde |
| Yeşil | Tamamlandı |
| Kırmızı | Gecikmiş (planlı tarih geçmiş, tamamlanmamış) |
| Turuncu | Taslak — yaklaşan |
| Gri | İptal |

### Sürükle-Bırak Tarih Güncelleme

Takvimde iş emri kutucuğunu başka bir güne sürükleyin → otomatik olarak planlı tarih güncellenir (Yazma yetkisi gerekir).

### İş Emri Detayı

Takvim üzerindeki iş emrine tıklayın → modal popup'ta ürün, makine, planlı tarih, durum, adet bilgileri görüntülenir. **İş Emrine Git** butonu ile üretim detay sayfasına geçilir.

### Makine Yükü

Sağ panelde bu ay için her makinenin kaç iş emrinin bekliyor/üretimde/tamamlandığını gösterir.

> **ℹ️ Gecikmiş Uyarı:** Sayfa başında kırmızı banner ile gecikmiş iş emirleri listelenir.

---

## 36. Fire / Atık Analizi {#fire-analizi}

**Sayfa:** `/Reports/WasteAnalysis`  
**Menü:** Sol Menü → Raporlar → Fire Analizi

```
[≡] [Tarih] [—] [Tarih] [Makine ▼] [Filtrele]   Fire / Atık Analizi   [👤]
```

### Özet Kartları

| Kart | İçerik |
|------|--------|
| Toplam Tüketim | Seçilen dönemde kullanılan hammadde (kg) |
| Üretim Kaynaklı Fire | Üretim detaylarından hesaplanan fire |
| Genel Fire Oranı | Fire / Tüketim × 100 |
| Atık Yönetim Kaydı | İmha kayıtlarında girilen toplam (kg) |

### Makine Bazında Fire Oranı

Her makine için tüketim, fire ve fire oranı tablo halinde gösterilir.  
Oran renk kodlaması: **%3 altı** yeşil · **%3–5** turuncu · **%5 üstü** kırmızı.

### Aylık Trend

Ay bazında tüketim ve fire bar görsel ile izlenir. Zaman içindeki fire oranı değişimi görülür.

### Kategori Bazında Atık

`/Production/WasteManagement` sayfasında girilen atık kategorilerinin dönem özeti.

> **⚠️ FSC CoC:** FSC denetiminde fire oranlarının belgelenmesi gerekir. Bu rapor ile beklenen fire oranı (%3–5) dışına çıkan makineler tespit edilebilir.

---

## 37. Dönem Kilidi {#donem-kilidi}

**Sayfa:** `/AuditPeriod`  
**Menü:** Sol Menü → Raporlar → Denetim Dönemleri

```
[≡] [+ Yeni Dönem Ekle] [Denetim Raporu]   Denetim Dönemleri   [👤 Admin]
```

### Dönem Kilitleme

Admin kullanıcı, denetim döneminin bitişinden sonra dönemi **kilitleyebilir**:

1. İlgili dönem satırında **Kilitle** (kırmızı) butonuna tıklayın
2. Onay dialogunda **Evet, Kilitle** seçin
3. Dönem artık kilitli — **KİLİT** sütununda kırmızı rozet görünür

### Kilitli Dönemin Etkisi

Kilitli döneme ait tarih aralığında **oluşturulan tüm işlemler** (hammadde girişi, üretim, satış, stok hareketi) **admin olmayan kullanıcılar tarafından değiştirilemez veya silinemez**.

Admin kullanıcı yetkisine sahip kişiler kilitli dönem işlemlerini düzenleyebilir.

### Dönem Kilidini Kaldırma

**Aç** (sarı) butonuna tıklayarak kilit kaldırılabilir.

### Dashboard Uyarısı

Kilitli dönemler varsa Dashboard ana sayfasında kırmızı banner ile gösterilir:  
`🔒 Kilitli Dönemler: 2024 (17.12.23–22.11.24, kilitli: admin) ...`

> **⚠️ Denetim Hazırlığı:** FSC CoC denetimi öncesinde ilgili dönemi kilitleyin. Bu sayede denetim dönemindeki kayıtlar değiştirilmeden korunur ve denetçiye sunulan veriler tutarlı kalır.

---

*Bu kılavuz her yeni modül tamamlandıkça güncellenmektedir.*
