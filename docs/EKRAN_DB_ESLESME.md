# Ekran → Veritabanı Eşleme — FSCTakip.Erp

> **Güncellenme:** 2026-06-22  
> **Version:** 1.0  
> Her sayfanın hangi tabloları ve kolonları kullandığını gösteren referans belgesi.

---

## İçindekiler

1. [Operasyonel Modüller](#operasyonel-modüller)
2. [Ayar & Yönetim Modülleri](#ayar--yönetim-modülleri)
3. [Raporlama & Analiz](#raporlama--analiz)

---

## Operasyonel Modüller

### Hammadde Girişi (/Purchase/Index)
**URL:** `/Purchase/Index`  
**Menü:** Operasyonlar → Hammadde Girişi  
**Açıklama:** Tedarikçilerden gelen hammadde lotlarını ve bobinlerini kaydetme. Varsayılan görünüm: Hammadde + Yarı Mamül + Burgu Sap gruplarından FSC ürünleri.

**Tablolar:**
- FscLots (ana liste)
- FscSerials (bobin detayları)
- Suppliers (tedarikçi seçimi)
- FscTypes (sertifika tipi)
- Products (ürün bilgisi)
- ProductGroups (varsayılan filtre)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Parti No | FscLots | PartiNo | Tedarikçi parti numarası |
| Tedarikçi | Suppliers | Name | Tedarikçi adı |
| FSC Tipi | FscTypes | Name | Sertifika tipi |
| Ürün Kodu | Products | ProductCode \| ExternalCode | İç veya harici ürün kodu |
| Ürün Adı | Products | ProductName | Ürün tanımı |
| Varış Tarihi | FscLots | ArrivalDate | Giriş tarihi |
| Fatura No | FscLots | InvoiceNo | Fatura numarası |
| İrsaliye No | FscLots | DispatchNo | İrsaliye numarası |
| Notlar | FscLots | Notes | İlave notlar |

**Filtre Seçenekleri:**
- Tedarikçi (MCD) → `supplierIds`
- FSC Tipi (MCD) → `fscTypeIds`
- Ürün (MCD) → `productIds`
- Ürün Grubu (MCD) → `productGroupIds`
- Stok Kodu (text) → `stockCode`
- Stok Adı (text) → `stockName`
- Tüm Kayıtları Göster → `showAll`

**Alt Sayfalar:**
- Lot Detayı → `/Purchase/Detail/{lotId}` (bobin listesi, ağırlık güncelleme)

---

### Hammadde Girişi - Detay (/Purchase/Detail)
**URL:** `/Purchase/Detail/{lotId}`  
**Açıklama:** Tek bir lot ve içindeki bobinler. Ağırlık düzenleme, StockMovement senkronizasyonu.

**Tablolar:**
- FscLots (ana kayıt)
- FscSerials (bobin listesi)
- StockMovement (senkronizasyon)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Lot Bilgileri | FscLots | PartiNo, InvoiceNo, DispatchNo | Lot header |
| Bobin No | FscSerials | SerialNo | Bobin numarası |
| Başlangıç KG | FscSerials | InitialWeight | Giriş ağırlığı |
| Güncel KG | FscSerials | CurrentWeight | Şu anki stok |
| Parti No (Tedarikçi) | FscSerials | LotNo | Bobin parti kodu |
| Notlar | FscSerials | Notes | Bobin notları |

**İşlemler:**
- Bobin ekle
- Bobin ağırlığını düzenle → StockMovement QuantityKg güncelle
- Bobin sil
- Fatura/İrsaliye PDF yükleme

---

### İş Emirleri (/Production/Index)
**URL:** `/Production/Index`  
**Menü:** Operasyonlar → İş Emirleri  
**Açıklama:** Ürün üretimi planlaması ve izleme. İş emri oluşturma, durumu güncelleme, üretim detayı giriş.

**Tablolar:**
- WorkOrders (ana liste)
- Products (üretilecek ürün)
- Machines (makine seçimi)
- ProductionDetails (üretim hareketi)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| İş Emri No | WorkOrders | WorkOrderNo | Sistem kodu |
| Ürün | Products | ProductName | Üretilecek ürün |
| Makine | Machines | Name | Makine |
| Planlanan Tarih | WorkOrders | PlannedDate | Üretim tarihi |
| Planlanan Miktar | WorkOrders | PlannedQuantity | Planlanan üretim |
| Gerçekleştirilen | WorkOrders | ActualQuantity | Gerçek üretim |
| Durum | WorkOrders | Status | 1=Taslak, 2=Üretimde, 3=Tamamlandı, 4=İptal |
| Notlar | WorkOrders | Notes | İlave notlar |

**Filtre Seçenekleri:**
- Başlangıç Tarihi → `startDate`
- Bitiş Tarihi → `endDate`
- Ürün (MCD) → `productIds`
- Durum → `status`

**Alt Sayfalar:**
- İş Emri Detayı (modal) → Reçete satırları ve üretim detayları
- Üretim Detayı Tablosu → `/Production/Details/{workOrderId}`

---

### Üretim Detayları (/Production/Details/{workOrderId})
**URL:** `/Production/Details/{workOrderId}`  
**Açıklama:** İş emrine ait reçete satırları ve her bileşen için üretim kayıtları.

**Tablolar:**
- WorkOrders (başlık)
- WorkOrderRecipes (reçete satırları)
- ProductionDetails (üretim hareketi)
- FscSerials (tüketilen bobinler)
- Products (bileşen ürünleri)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Bileşen Ürünü | Products | ProductName | Reçete bileşeni |
| Seçili Bobin | FscSerials | SerialNo | Tüketilen bobin |
| Planlanan KG | WorkOrderRecipes | PlannedQuantity | Reçeteden miktar |
| Harcanan KG | WorkOrderRecipes | ActualConsumedQuantity | Gerçek tüketim |
| Fire KG | WorkOrderRecipes | WasteQuantity | Verilen fire |
| Üretim Miktarı | WorkOrderRecipes | ProducedQuantity | Ürün adet |

**İşlemler:**
- Reçete satırı ekle (ürün + bobin seçimi)
- Tüketim ağırlığı güncelle
- Fire ağırlığı düzenle
- Üretim Detayı kaydet
- İş Emrini tamamla (durum → Tamamlandı)

---

### Yarı Mamül Dönüşüm (/Conversion/Index)
**URL:** `/Conversion/Index`  
**Menü:** Operasyonlar → Dönüşüm  
**Açıklama:** Hammadde/YM bobinini tüketip yeni YM bobini üretme. FSC tipi kaynak lottan devralınır.

**Tablolar:**
- FscSerials (kaynak bobinler)
- FscLots (kaynak lotlar, çıkış lotları)
- Products (kaynak ve hedef ürünler)
- FscTypes (sertifika tipi)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Kaynak Bobin No | FscSerials | SerialNo | Tüketilecek bobin |
| Güncel KG | FscSerials | CurrentWeight | Mevcut stok |
| FSC Tipi | FscTypes | Name | Devralınan tip |
| Hedef Ürün | Products | ProductName | Çıkış ürünü |
| Ürün Kodu | Products | ExternalCode | ERP kodu |
| Dönüşüm Fire | (hesaplanan) | ConversionFireKg | Üretim kaybı |
| Yeni Lot No | FscLots | PartiNo | YM-{yıl}-{seq} |

**Son Dönüşümler (Tablo):**
- Tarih, Parti, Hedef Ürün, Başlangıç KG, Güncel KG, FSC Tipi

**İşlemler:**
- Kaynak bobin seç
- Hedef ürün seç
- Dönüşüm KG'si gir (otomatik)
- Fire KG'si gir
- Kaydet (yeni FscLot oluştur, kaynak bobin weight güncelle)

---

### Stok Özeti (/Stock/Summary)
**URL:** `/Stock/Summary`  
**Menü:** Stok → Stok Özeti  
**Açıklama:** Ürün bazında net stok özeti (giriş - çıkış = net). Varsayılan: Hammadde + Yarı Mamül + Burgu Sap grupları.

**Tablolar:**
- StockMovements (hareketi kaynağı)
- Products (ürün bilgisi)
- ProductGroups (grup adları)
- FscLots (lot sayısı)
- FscSerials (bobin sayısı)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Ürün Kodu | Products | ProductCode \| ExternalCode | İç veya harici kod |
| Ürün Adı | Products | ProductName | Ürün tanımı |
| Grup | ProductGroups | GroupName | Ürün grubu |
| Birim | Products | Unit | Varsayılan birim |
| Giriş Miktarı | StockMovements | QuantityKg | Toplam giriş |
| Çıkış Miktarı | StockMovements | QuantityKg | Toplam çıkış |
| Net Stok KG | (hesaplanan) | Giriş - Çıkış | Mevcut bakiye |
| Lot Sayısı | FscLots | COUNT(*) | Açık lot sayısı |
| Bobin Sayısı | FscSerials | COUNT(*) | Toplam bobin |

**Filtre Seçenekleri:**
- Ürün Grupları (MCD) → `groupIds`
- Ürün Arama (text) → `search`
- Tüm Kayıtları Göster → değiştirir default grupları

**Excel Export:** Tüm görünen satırlar

---

### Hammadde Stoğu (/Stock/RawMaterial)
**URL:** `/Stock/RawMaterial`  
**Menü:** Stok → Hammadde Stoğu  
**Açıklama:** Hammadde grubu ürünlerinin detaylı stok listesi (lot + bobin seviyesinde).

**Tablolar:**
- Products (hammadde ürünleri)
- FscLots (malzeme lotları)
- FscSerials (stokta kalan bobinler)
- FscTypes (sertifika tipi)
- Suppliers (tedarikçi)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Ürün Kodu | Products | ProductCode | Ürün kodu |
| Ürün Adı | Products | ProductName | Ürün tanımı |
| Parti No | FscLots | PartiNo | Lot numarası |
| Bobin No | FscSerials | SerialNo | Bobin/seri numarası |
| Başlangıç KG | FscSerials | InitialWeight | Giriş miktarı |
| Güncel KG | FscSerials | CurrentWeight | Mevcut stok |
| FSC Tipi | FscTypes | Name | Sertifika tipi |
| Tedarikçi | Suppliers | Name | Kaynak tedarikçi |
| Varış Tarihi | FscLots | ArrivalDate | Giriş tarihi |

**Filtre Seçenekleri:**
- Ürün (MCD)
- Tedarikçi (MCD)
- FSC Tipi (MCD)
- Arama (kod/ad)

---

### Stok Hareketleri (/Stock/Movements)
**URL:** `/Stock/Movements`  
**Menü:** Stok → Hareketler  
**Açıklama:** Tüm stok hareketi kayıtları (giriş, çıkış, transfer).

**Tablolar:**
- StockMovements (ana tablo)
- Products (hareket ürünü)
- Warehouses (depo)
- Customers (satış müşterisi)
- WorkOrders (üretim referansı)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Hareket Tipi | StockMovements | Type | 1=ÜretimG, 2=Transfer, 3=SatışÇ, 4=SatınAlmaG, 5=ÜretimT |
| Evrak No | StockMovements | DocumentNo | Fatura/İrsaliye no |
| Tarih | StockMovements | DocumentDate | Hareket tarihi |
| Ürün Kodu | Products | ProductCode | Ürün kodu |
| Miktar | StockMovements | Quantity | Orijinal birim miktarı |
| Birim | StockMovements | Unit | MT, ADET, KG vb. |
| KG Eşdeğeri | StockMovements | QuantityKg | Dönüştürülmüş KG |
| Çıkış Deposu | Warehouses | Name | Nereden çıktı |
| Giriş Deposu | Warehouses | Name | Nereye girdi |
| Müşteri | Customers | Name | Satış müşterisi |

**Filtre Seçenekleri:**
- Hareket Tipi (MCD)
- Ürün (MCD)
- Tarih Aralığı
- Depo

---

### Tüm Stok Admin (/Stock/AdminStock)
**URL:** `/Stock/AdminStock`  
**Menü:** Stok → Admin Stok  
**Açıklama:** İdari işlemler (stok düzeltme, koreksiyon hareketleri).

**Tablolar:**
- StockMovements (koreksiyon hareketleri)
- Products
- Warehouses

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| (Tanımlı değil) | — | — | Sayfa henüz geliştirilmemiş (stub) |

---

## Ayar & Yönetim Modülleri

### Ürünler (/Products/Index)
**URL:** `/Products/Index`  
**Menü:** Ayarlar → Ürünler  
**Açıklama:** Ürün kartı yönetimi (CRUD + kod otomatik üretimi).

**Tablolar:**
- Products (ana tablo)
- ProductGroups (grup seçimi)
- FscTypes (sertifika tipi)
- PaperTypes (kağıt tipi)
- PaperColors (renk)
- PaperWeights (gramaj)
- PaperWidths (bobin eni)
- Suppliers (varsayılan tedarikçi)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Ürün Kodu | Products | ProductCode | Otomatik: {GrupKodu}{Seq:D3} |
| Ürün Adı | Products | ProductName | Zorunlu |
| Harici Kod | Products | ExternalCode | ERP stok kodu |
| Grup | ProductGroups | GroupName | Seçme |
| Birim | Products | Unit | KG, ADET, MT vb. |
| FSC Tipi | FscTypes | Name | Seçme |
| Kağıt Tipi | PaperTypes | Name | Seçme |
| Renk | PaperColors | Name | Seçme |
| Gramaj | PaperWeights | Value | Seçme |
| Bobin Eni | PaperWidths | Code | Seçme |
| Tedarikçi | Suppliers | Name | Seçme |
| Aktif | Products | IsActive | Toggle |

**İşlemler:**
- Ürün ekle → Kod otomatik üretilir
- Ürün düzenle
- Ürün sil
- Excel export (tüm sütunlar)

---

### Ürün Reçetesi (/Products/Recipe)
**URL:** `/Products/Recipe`  
**Menü:** Ayarlar → Ürün Reçetesi  
**Açıklama:** Ana ürün + bileşen ürünler tanımlaması (BOM).

**Tablolar:**
- Products (ana ve bileşen)
- ProductRecipes (ilişki)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Ana Ürün | Products | ProductName | Üretilecek mamul |
| Bileşen Ürünü | Products | ProductName | İçerisinde kullanılan |
| Miktar | ProductRecipes | StandardQuantity | Birim cinsinden |
| Birim | ProductRecipes | Unit | KG, ADET, MT |
| Bileşen Yeri | ProductRecipes | BilesenYeri | Gövde, Sap, Etiket vb. |
| Aktif | ProductRecipes | IsActive | Toggle |

**İşlemler:**
- Reçete satırı ekle
- Reçete satırı düzenle
- Reçete satırı sil

---

### Tedarikçiler (/Suppliers/Index)
**URL:** `/Suppliers/Index`  
**Menü:** Ayarlar → Tedarikçiler  
**Açıklama:** Tedarikçi kartı yönetimi (kod otomatik: TED-XXX).

**Tablolar:**
- Suppliers (ana tablo)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Tedarikçi Kodu | Suppliers | SupplierCode | Otomatik: TED-{Seq:D3} |
| Harici Kod | Suppliers | ExternalCode | ERP kodu |
| Adı | Suppliers | Name | Şirket adı |
| FSC Kodu | Suppliers | FscCode | Sertifika numarası |
| FSC Bitiş | Suppliers | FscExpiryDate | Sertifika son tarih |
| Yetkili Kişi | Suppliers | ContactPerson | İletişim |
| Telefon | Suppliers | Phone | Telefon |
| E-Posta | Suppliers | Email | E-posta |
| Adres | Suppliers | Address | Açık adres |
| Şehir | Suppliers | City | Şehir |
| Vergi No | Suppliers | TaxNumber | Vergi numarası |
| Vergi Dairesi | Suppliers | TaxOffice | Vergi dairesi |
| FSC Aktif | Suppliers | IsFscActive | FSC durumu |
| Aktif | Suppliers | IsActive | Genel durumu |

**İşlemler:**
- Tedarikçi ekle
- Tedarikçi düzenle
- Tedarikçi sil
- Durum toggle (Aktif/Pasif)

---

### Müşteriler (/Customers/Index)
**URL:** `/Customers/Index`  
**Menü:** Ayarlar → Müşteriler  
**Açıklama:** Müşteri kartı yönetimi (kod otomatik: MHS-XXX).

**Tablolar:**
- Customers (ana tablo)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Müşteri Kodu | Customers | CustomerCode | Otomatik: MHS-{Seq:D3} |
| Harici Kod | Customers | ExternalCode | ERP kodu |
| Adı | Customers | Name | Şirket adı |
| Vergi No | Customers | TaxNumber | Vergi numarası |
| Vergi Dairesi | Customers | TaxOffice | Vergi dairesi |
| Adres | Customers | Address | Açık adres |
| Şehir | Customers | City | Şehir |
| E-Posta | Customers | Email | E-posta |
| Telefon | Customers | Phone | Telefon |
| FSC Lisans Kodu | Customers | FscLicenseCode | Müşteri FSC lisansı |
| FSC Aktif | Customers | IsFscActive | FSC durumu |
| FSC Bitiş | Customers | FscExpiryDate | Sertifika son tarih |
| Aktif | Customers | IsActive | Genel durumu |

**İşlemler:**
- Müşteri ekle
- Müşteri düzenle
- Müşteri sil
- Durum toggle

---

### Makineler (/Machine/Index)
**URL:** `/Machine/Index`  
**Menü:** Ayarlar → Makineler  
**Açıklama:** Makine tanımları ve yönetimi.

**Tablolar:**
- Machines (ana tablo)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Makine Adı | Machines | Name | Makine adı |
| Makine Kodu | Machines | Code | Sistem kodu |
| Makine Tipi | Machines | Type | Tipi |
| Aktif | Machines | IsActive | Durumu |

**İşlemler:**
- Makine ekle
- Makine düzenle
- Makine sil

---

### Kağıt Özellikleri (/Paper/*)
**URL:** `/Paper/...`  
**Menü:** Ayarlar → Kağıt Özellikleri  
**Açıklama:** Kağıt tipi, renk, gramaj, bobin eni, FSC tipi tanımları.

**Alt Sayfalar:**

#### Kağıt Tipleri
- Tablo: PaperTypes
- İşlemler: Ekle, Düzenle, Sil

#### Kağıt Renkleri
- Tablo: PaperColors
- İşlemler: Ekle, Düzenle, Sil

#### Gramaj Değerleri
- Tablo: PaperWeights
- İşlemler: Ekle, Düzenle, Sil

#### Bobin Eni Değerleri
- Tablo: PaperWidths
- İşlemler: Ekle, Düzenle, Sil

#### FSC Tipi Tanımları
- Tablo: FscTypes
- İşlemler: Ekle, Düzenle, Sil

---

### Depo Yönetimi (/Warehouse/Index)
**URL:** `/Warehouse/Index`  
**Menü:** Ayarlar → Depolar  
**Açıklama:** Depo tanımları.

**Tablolar:**
- Warehouses (ana tablo)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Depo Adı | Warehouses | Name | Depo adı |
| Depo Kodu | Warehouses | Code | Depo kodu |
| Aktif | Warehouses | IsActive | Durumu |

---

### Birim Dönüşümleri (/UnitConversion/Index)
**URL:** `/UnitConversion/Index`  
**Menü:** Ayarlar → Birim Dönüşümleri  
**Açıklama:** MT → KG, ADET → KG vb. dönüşüm katsayıları.

**Tablolar:**
- UnitConversions (ana tablo)
- ProductGroups (optional scope)
- Products (optional scope)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Kaynak Birim | UnitConversions | FromUnit | MT, ADET, M2 vb. |
| Hedef Birim | UnitConversions | ToUnit | Daima KG |
| Çarpan | UnitConversions | Factor | 1 {FromUnit} = {Factor} KG |
| Açıklama | UnitConversions | Description | Not |
| Kapsam | UnitConversions | ProductGroupId / ProductId | Null = tüm, ProductGroupId = grup, ProductId = ürün |
| Aktif | UnitConversions | IsActive | Durumu |

**İşlemler:**
- Dönüşüm ekle
- Dönüşüm düzenle
- Dönüşüm sil

---

### Torba Tipleri (/Product/BagTypes)
**URL:** `/Product/BagTypes`  
**Menü:** Ayarlar → Torba Tipleri  
**Açıklama:** Torba tipi tanımları (Kare Dip, V Kesim, Dip Takviyeli vb.).

**Tablolar:**
- BagTypes (ana tablo)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Kod | BagTypes | Code | BT-KD, BT-VK vb. |
| Adı | BagTypes | Name | Torba tipi adı |
| Açıklama | BagTypes | Description | Teknik notlar |
| Aktif | BagTypes | IsActive | Durumu |

---

### Ürün Grupları (/Product/Groups)
**URL:** `/Product/Groups`  
**Menü:** Ayarlar → Ürün Grupları  
**Açıklama:** Ürün grup tanımları (Hammadde, Yarı Mamül, Mamul vb.).

**Tablolar:**
- ProductGroups (ana tablo)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Grup Adı | ProductGroups | GroupName | Grup adı |
| Grup Kodu | ProductGroups | GroupCode | Sayısal kod |
| Kod Aralığı | ProductGroups | RangeStart, RangeEnd | Ürün kod aralığı |
| Aktif | ProductGroups | IsActive | Durumu |

---

## Raporlama & Analiz

### FSC Chain of Custody (/Reports/ChainOfCustody)
**URL:** `/Reports/ChainOfCustody`  
**Menü:** Raporlar → FSC CoC  
**Açıklama:** Satış siparişini tedarikçiye kadar izleme (FSC sertifikası zinciri).

**Tablolar:**
- SalesOrderLines (satış satırları)
- SalesOrders (başlık + müşteri)
- WorkOrders (üretim emri)
- ProductionDetails (üretim hareketi)
- FscSerials (tüketilen bobinler)
- FscLots (kaynak lotlar)
- Suppliers (tedarikçiler)
- FscTypes (sertifika tipi)
- Products (ürünler)
- Customers (müşteriler)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Sevk Tarihi | SalesOrders | DispatchDate | Müşteriye sevkiyat tarihi |
| Sipariş No | SalesOrders | SalesOrderNo | Satış siparişi numarası |
| Müşteri | Customers | Name | Satış müşterisi |
| Müşteri FSC | Customers | FscLicenseCode | Müşteri FSC lisans kodu |
| Ürün | Products | ProductName | Satılan ürün |
| Miktar | SalesOrderLines | Quantity | Satış miktarı |
| İş Emri | WorkOrders | WorkOrderNo | Üretim emri numarası |
| Partiler | FscLots | PartiNo | Kaynak parti numaraları (virgülle ayrılmış) |
| Bobinler | FscSerials | SerialNo | Kaynak bobin numaraları |
| Tedarikçiler | Suppliers | Name | Kaynak tedarikçiler |
| Tedarikçi FSC | Suppliers | FscCode | Tedarikçi sertifika numaraları |
| FSC Tipleri | FscTypes | Name | Sertifika tipleri |
| Zincir Durumu | (hesaplanan) | ChainComplete | Tam = tüm tedarikçilerin aktif FSC'si var, Eksik = eksik |

**Filtre Seçenekleri:**
- Başlangıç Tarihi → `startDate`
- Bitiş Tarihi → `endDate`
- Müşteri → `customerId`
- Ürün (MCD) → `productIds`

**Stat Kartları:**
- Toplam Satış Satırı
- Tam Zincirli
- Eksik Zincirli

**Excel Export:** Tüm satirlar + koç özeti

---

### Denetim Özeti (/Reports/AuditReport)
**URL:** `/Reports/AuditReport`  
**Menü:** Raporlar → Denetim Özeti  
**Açıklama:** Denetim dönemine göz fırsat özeti (tamamlanmış işler + CoC raporu).

**Tablolar:**
- AuditPeriods (denetim dönemleri)
- WorkOrders (tamamlanan işler)
- SalesOrders (teslim edilen siparişler)
- SalesOrderLines (detaylar)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Denetim Dönemi | AuditPeriods | Year | Denetim yılı |
| Başlangıç Tarihi | AuditPeriods | StartDate | Dönem başlangıcı |
| Bitiş Tarihi | AuditPeriods | EndDate | Dönem bitişi |
| Tamamlanan İşler | WorkOrders | COUNT(*) | Üretimi tamamlanan emirler |
| Teslim Edilen Siparişler | SalesOrders | COUNT(*) | Sevkiyatı yapılan siparişler |
| Toplam Üretim | WorkOrders | SUM(ActualQuantity) | Toplam ürün miktarı |
| Toplam Satış | SalesOrderLines | SUM(Quantity) | Toplam satış miktarı |

**Filtre Seçenekleri:**
- Denetim Dönemi → `periodId`

---

### Fire Raporu (/Production/WasteReport)
**URL:** `/Production/WasteReport`  
**Menü:** Raporlar → Fire Raporu  
**Açıklama:** Üretim kaybı ve fire özeti (kategori bazında).

**Tablolar:**
- WasteManagements (ana tablo)
- WorkOrders (üretim emri)
- ProductionDetails (üretim hareketi)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Tarih | WasteManagements | DisposalDate | Fire tarihi |
| Atık Kodu | WasteManagements | WasteCode | Atık numarası |
| İş Emri | WorkOrders | WorkOrderNo | İlişkili üretim emri |
| Kategori | WasteManagements | Category | 1=Kesim, 2=Baskı, 3=Islanma, 4=Nakliye, 5=Makine, 99=Diğer |
| Açıklama | WasteManagements | Description | Detay |
| Miktar | WasteManagements | Quantity | Fire miktarı |
| Birim | WasteManagements | Unit | Birim |
| İmha Yöntemi | WasteManagements | DisposalMethod | İmha, Geri Dönüşüm, Satış |
| İmha Yapan | WasteManagements | DisposedBy | Kişi |

**Filtre Seçenekleri:**
- Tarih Aralığı
- Kategori
- İş Emri

---

### Hammadde İzleme (/Reports/MaterialTrace)
**URL:** `/Reports/MaterialTrace`  
**Menü:** Raporlar → Hammadde İzleme  
**Açıklama:** Seçilen hammadde bobininin, üretimde hangi ürünlere dönüştüğünü izleme.

**Tablolar:**
- FscSerials (seçili bobin)
- FscLots (kaynak lot)
- ProductionDetails (tüketim)
- WorkOrders (üretim emri)
- Products (nihai ürün)
- SalesOrderLines (satış)
- Customers (müşteri)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Bobin No | FscSerials | SerialNo | Başlangıç bobini |
| Başlangıç KG | FscSerials | InitialWeight | Giriş miktarı |
| Güncel KG | FscSerials | CurrentWeight | Mevcut stok |
| Üretim Emri | WorkOrders | WorkOrderNo | Tüketildiği iş emri |
| Üretim Ürünü | Products | ProductName | Üretilen ürün |
| Tüketim KG | ProductionDetails | ConsumedWeight | Kullanılan miktar |
| Fire KG | ProductionDetails | WasteWeight | Verilen fire |
| Satış Siparişi | SalesOrders | SalesOrderNo | Müşteriye satılan sipariş |
| Müşteri | Customers | Name | Son müşteri |

**İşlemler:**
- Bobin seç
- Takip yap (cascade görüntüleme)

---

### Lot Takip (/Reports/LotTrace)
**URL:** `/Reports/LotTrace`  
**Menü:** Raporlar → Lot Takip  
**Açıklama:** Seçilen hammadde lotunun üretimde nasıl kullanıldığını ve müşteriye ne kadar ulaştığını izleme.

**Tablolar:**
- FscLots (seçili lot)
- FscSerials (lot içi bobinler)
- Suppliers (tedarikçi)
- ProductionDetails (üretim)
- WorkOrders (iş emirleri)
- SalesOrderLines (satış)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Parti No | FscLots | PartiNo | Lot numarası |
| Tedarikçi | Suppliers | Name | Kaynak tedarikçi |
| Varış Tarihi | FscLots | ArrivalDate | Giriş tarihi |
| Toplam KG | FscSerials | SUM(InitialWeight) | Lot içi toplam ağırlık |
| Bobinler | FscSerials | COUNT(*) | Toplam bobin sayısı |
| Tüketim Ayrıntıları | ProductionDetails | WorkOrderId, ConsumedWeight | Hangi iş emrinde ne kadar tüketildi |
| Satış Bilgisi | SalesOrderLines | SalesOrderNo, Quantity | Müşteriye satılan miktar ve sipariş |

**İşlemler:**
- Lot seç
- Takip yap (hierarchical görüntüleme)

---

### BOM Bileşen Analizi (/Reports/BomAnalysis)
**URL:** `/Reports/BomAnalysis`  
**Menü:** Raporlar → BOM Analizi  
**Açıklama:** Seçili mamul ürünün reçetesi ve geçmiş üretim sapmalarını analiz.

**Tablolar:**
- Products (ana ürün)
- ProductRecipes (reçete satırları)
- WorkOrders (üretim emirleri)
- WorkOrderRecipes (iş emri reçete satırları)
- ProductionDetails (gerçek tüketim)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Mamul Ürün | Products | ProductName | Seçili ana ürün |
| Bileşen | Products | ProductName | Reçete bileşeni |
| Standart Miktar | ProductRecipes | StandardQuantity | Reçete miktarı |
| Birim | ProductRecipes | Unit | Birim |
| Bileşen Yeri | ProductRecipes | BilesenYeri | Gövde, Sap, Etiket vb. |
| Geçmiş Tüketim Ort. | ProductionDetails | AVG(ConsumedWeight) | Son üretimlerdeki ortalama |
| Sapma % | (hesaplanan) | (Gerçek - Standart) / Standart × 100 | Sapma yüzdesi |

---

### Kullanıcı Yönetimi (/Users/Index)
**URL:** `/Users/Index`  
**Menü:** Yönetim → Kullanıcılar  
**Açıklama:** Sistem kullanıcılarının yönetimi (CRUD).

**Tablolar:**
- AppUsers (ana tablo)
- UserGroups (grup üyeliği)
- PermissionGroups (yetki grupları)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Kullanıcı Adı | AppUsers | Username | Login adı |
| Ad Soyad | AppUsers | FullName | Tam ad |
| E-Posta | AppUsers | Email | E-posta |
| Gruplar | UserGroups / PermissionGroups | Name | Üye olduğu yetki grupları |
| Admin | AppUsers | IsAdmin | Admin flagu |
| Aktif | AppUsers | IsActive | Durumu |
| Son Giriş | AppUsers | LastLoginDate | Son oturum açış |

**İşlemler:**
- Kullanıcı ekle
- Kullanıcı düzenle (parola sıfırla, grup değişikliği)
- Kullanıcı sil
- Admin toggle

---

### Yetki Grupları (/Groups/Index)
**URL:** `/Groups/Index`  
**Menü:** Yönetim → Yetki Grupları  
**Açıklama:** Yetki grup tanımı ve modül izinleri.

**Tablolar:**
- PermissionGroups (ana tablo)
- GroupPermissions (detay)
- PermissionModules (modül tanımları)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Grup Adı | PermissionGroups | Name | Grup adı |
| Açıklama | PermissionGroups | Description | Not |
| Modül | PermissionModules | DisplayName | Modül adı |
| Oku | GroupPermissions | CanRead | Okuma izni |
| Yaz | GroupPermissions | CanWrite | Yazma izni |
| Sil | GroupPermissions | CanDelete | Silme izni |

**İşlemler:**
- Grup ekle
- Modül izinleri değiştir
- Grup sil

---

### Denetim Günlüğü (/AuditLog/Index)
**URL:** `/AuditLog/Index`  
**Menü:** Yönetim → Denetim Günlüğü  
**Açıklama:** Tüm sistem değişiklikleri (kim, ne zaman, ne değişti).

**Tablolar:**
- AuditLogs (ana tablo)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Tarih/Saat | AuditLogs | ChangedAt | Değişiklik tarihi |
| Kişi | AuditLogs | ChangedBy | Kimin tarafından |
| İşlem | AuditLogs | Action | INSERT, UPDATE, DELETE |
| Tablo | AuditLogs | TableName | Hangi tablo |
| Kayıt ID | AuditLogs | RecordId | Kayıt numarası |
| Eski Değerler | AuditLogs | OldValues | JSON formatında |
| Yeni Değerler | AuditLogs | NewValues | JSON formatında |
| IP Adresi | AuditLogs | IpAddress | İstemci IP'si |

**Filtre Seçenekleri:**
- Tablo
- İşlem türü
- Tarih aralığı
- Kullanıcı

---

### Denetim Dönemleri (/AuditPeriod/Index)
**URL:** `/AuditPeriod/Index`  
**Menü:** Yönetim → Denetim Dönemleri  
**Açıklama:** FSC CoC denetim dönemlerinin tanımı ve kilitleme yönetimi.

**Tablolar:**
- AuditPeriods (ana tablo)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Denetim Yılı | AuditPeriods | Year | Denetim yılı |
| Başlangıç | AuditPeriods | StartDate | Dönem başlangıcı |
| Bitiş | AuditPeriods | EndDate | Dönem bitişi |
| Kilitli | AuditPeriods | IsLocked | Kilit durumu |
| Kilitlenme Tarihi | AuditPeriods | LockedAt | Kilit zamanı |
| Kilitleyenin Adı | AuditPeriods | LockedBy | Kilit yapan |
| Açıklama | AuditPeriods | Description | Not |
| Aktif | AuditPeriods | IsActive | Durumu |

**İşlemler:**
- Denetim dönemi ekle
- Dönemi kilitle (denetim tamamlanma işareti)
- Dönemi aç (kilit kaldır)
- Dönemi sil

---

### FSC Belge Arşivi (/DocumentArchive/Index)
**URL:** `/DocumentArchive/Index`  
**Menü:** Raporlar → Belge Arşivi  
**Açıklama:** FSC sertifikaları, anlaşmalar, eğitimler, formlar vb. dokümantasyon arşivi.

**Tablolar:**
- FscDocuments (ana tablo)

| Ekran Alanı | Tablo | Kolon | Açıklama |
|---|---|---|---|
| Başlık | FscDocuments | Title | Belge başlığı |
| Kategori | FscDocuments | Category | 1=CoC, 2=Tedarikçi, 3=Anlaşma, 4=El Kitabı, 5=Org.Şema, 6=Talimat, 7=Eğitim, 8=Artwork, 9=Atama, 10=Düzeltici, 11=İş Akışı, 12=Standart, 13=Form, 99=Diğer |
| Yıl | FscDocuments | Year | Belge yılı |
| Dosya Adı | FscDocuments | FileName | Yüklenen dosya |
| Dosya Boyutu | FscDocuments | FileSize | Byte cinsinden |
| Uzantı | FscDocuments | FileExtension | pdf, docx, xlsx vb. |
| Etiketler | FscDocuments | Tags | Arama etiketleri |
| Notlar | FscDocuments | Notes | Açıklama |

**İşlemler:**
- Belge yükle
- Belge indir
- Belge sil
- Belge bilgisini düzenle

---

## Notlar

### Filtre Paneli Kuralı (MCD)
Multi-Choice Dropdown (MCD) kullanılan sayfalar:
- Hammadde Girişi: Tedarikçi, FSC Tipi, Ürün, Ürün Grubu
- Stok Özeti: Ürün Grupları
- Hammadde Stoğu: Ürün, Tedarikçi, FSC Tipi
- Stok Hareketleri: Hareket Tipi, Ürün
- Ürün Reçetesi: (dropdown seçme)

### StockMovement Senkronizasyonu
FscSerial ağırlığı değiştiğinde ilgili StockMovement kaydının QuantityKg alanı güncellenmesi gerekir:
- StockMovement.DocumentNo = FscLot.DispatchNo || FscLot.PartiNo
- StockMovement.QuantityKg = FscSerials toplamı

### Sayfa Durumları
- ✅ Tamamlandı: Hammadde Girişi, İş Emirleri, Üretim Detayları, Dönüşüm, Stok Özeti, Hammadde Stoğu, Stok Hareketleri, Ürünler, Ürün Reçetesi, Tedarikçiler, Müşteriler, Makineler, Kağıt Özellikleri, Depo Yönetimi, Birim Dönüşümleri, FSC CoC Raporu, Denetim Özeti, Yönetim modülleri
- 🔄 Geliştirilmekte: Yarı Mamül Dönüşüm (üretim-stok entegrasyonu detaylandırılacak)
- ⏳ Planlı: Satış Modülü (SalesOrders), Fire Raporu detayları, Materyal İzleme, Lot İzleme, BOM Analizi

---

**Oluşturma tarihi:** 2026-06-22  
**Son güncelleme:** 2026-06-22  
**Versiyon:** 1.0
