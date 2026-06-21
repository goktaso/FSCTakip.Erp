# Veritabanı Şeması — FSCTakip.Erp

> **Güncellenme:** 2026-06-22  
> **Version:** 1.0  
> Bu dokümantasyon tüm tablolar, kolonlar, veri tipleri ve ilişkileri içerir.

---

## İçindekiler

1. [Temel Varlıklar (Settings)](#temel-varlıklar-settings)
2. [Operasyonel Varlıklar (Transactions)](#operasyonel-varlıklar-transactions)
3. [Sistem & Yönetim](#sistem--yönetim)
4. [Tüm Tablo Referansı (Alfabetik)](#tüm-tablo-referansı-alfabetik)

---

## Temel Varlıklar (Settings)

### Tablo: FscTypes
Sertifika tipi tanımları (FSC-100, Mix, Recycled vb.).

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK, otomatik artış |
| Code | nvarchar(100) | No | Kod (örn: FSC-100, MIX-01) |
| Name | nvarchar(200) | No | Görünen isim (örn: FSC %100) |
| Description | nvarchar(max) | No | Denetim notları |
| IsActive | bit | No | Aktiflik durumu (varsayılan: true) |
| CreatedBy | nvarchar(max) | No | Oluşturan kullanıcı |
| CreatedDate | datetime2(7) | No | Oluşturulma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen kullanıcı |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

**İlişkiler:**
- Product.FscTypeId → FscTypes.Id
- Supplier × FscTypes (tedarikçilerin sertifika tipi)

---

### Tablo: PaperTypes
Kağıt tipi tanımları (Kraft, Beyaz vb.).

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| Name | nvarchar(max) | No | Kağıt tipi adı |
| ShortCode | nvarchar(max) | No | Kısaltma |
| IsActive | bit | No | Aktiflik |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

**İlişkiler:**
- Product.PaperTypeId → PaperTypes.Id

---

### Tablo: PaperColors
Kağıt rengi tanımları (White, Brown, Natural vb.).

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| Name | nvarchar(50) | No | Renk adı |
| IsActive | bit | No | Aktiflik |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

**İlişkiler:**
- Product.PaperColorId → PaperColors.Id

---

### Tablo: PaperWeights
Gramaj tanımları (70, 80, 90 vb.).

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| Value | decimal(10,2) | No | Gramaj değeri (örn: 70.5) |
| Unit | nvarchar(max) | No | Birim (varsayılan: gr) |
| IsActive | bit | No | Aktiflik |

**İlişkiler:**
- Product.PaperWeightId → PaperWeights.Id

---

### Tablo: PaperWidths
Bobin en tanımları (600mm, 1050mm vb.).

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| Code | nvarchar(max) | No | Kod (örn: EN-600) |
| Value | decimal(10,2) | No | En değeri |
| Unit | nvarchar(max) | No | Birim (varsayılan: mm) |
| IsActive | bit | No | Aktiflik |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

**İlişkiler:**
- Product.PaperWidthId → PaperWidths.Id

---

### Tablo: ProductGroups
Ürün grubu tanımları (Hammadde, Yarı Mamül, Burgu Sap vb.).

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| GroupCode | int | No | Grup kodu |
| GroupName | nvarchar(max) | No | Grup adı |
| RangeStart | int | No | Ürün kodu aralığı başlangıcı |
| RangeEnd | int | No | Ürün kodu aralığı bitişi |
| IsActive | bit | No | Aktiflik |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

**İlişkiler:**
- Product.ProductGroupId → ProductGroups.Id
- UnitConversion.ProductGroupId → ProductGroups.Id

---

### Tablo: Machines
Makine tanımları.

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| Name | nvarchar(max) | No | Makine adı |
| Code | nvarchar(max) | No | Makine kodu |
| Type | nvarchar(max) | No | Makine tipi |
| IsActive | bit | No | Aktiflik |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

**İlişkiler:**
- WorkOrder.MachineId → Machines.Id
- ProductionDetail.MachineId → Machines.Id

---

### Tablo: BagTypes
Torba tipi tanımları (Kare Dip, V Kesim, Dip Takviyeli vb.).

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| Code | nvarchar(max) | No | Kod (örn: BT-KD) |
| Name | nvarchar(max) | No | Torba tipi adı |
| Description | nvarchar(max) | No | Teknik notlar |
| IsActive | bit | No | Aktiflik |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

---

### Tablo: ProductGrammages
Ürün gramajı referansı (70 gr/m², 80 gr/m² vb.).

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| Value | int | No | Gramaj değeri |
| Description | nvarchar(max) | No | Tanım |
| IsActive | bit | No | Aktiflik |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

---

### Tablo: Warehouses
Depo tanımları.

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| Name | nvarchar(max) | No | Depo adı |
| Code | nvarchar(max) | No | Depo kodu |
| IsActive | bit | No | Aktiflik |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

**İlişkiler:**
- StockMovement.FromWarehouseId → Warehouses.Id
- StockMovement.ToWarehouseId → Warehouses.Id

---

### Tablo: UnitConversions
Birim dönüşüm parametreleri (MT → KG, ADET → KG vb.).

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| FromUnit | nvarchar(max) | No | Kaynak birim (MT, ADET, M2) |
| ToUnit | nvarchar(max) | No | Hedef birim (her zaman KG) |
| Factor | decimal(18,6) | No | Çarpan (KG = Miktar × Factor) |
| Description | nvarchar(max) | No | Açıklama |
| ProductGroupId | int | Yes | Kapsam: Ürün grubu (null = tüm gruplar) |
| ProductId | int | Yes | Kapsam: Ürün (null = tüm ürünler) |
| IsActive | bit | No | Aktiflik |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

**İlişkiler:**
- ProductGroupId → ProductGroups.Id (nullable)
- ProductId → Products.Id (nullable)

---

## Operasyonel Varlıklar (Transactions)

### Tablo: Suppliers
Tedarikçi tanımları (hammadde ve yarı mamül kaynakları).

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| SupplierCode | nvarchar(max) | No | Sistem kodu (örn: TED-001) |
| ExternalCode | nvarchar(max) | Yes | ERP kodu (ETL eşleştirme) |
| Name | nvarchar(max) | No | Tedarikçi adı |
| FscCode | nvarchar(max) | Yes | FSC sertifika numarası |
| FscExpiryDate | datetime2(7) | Yes | Sertifika bitiş tarihi |
| ContactPerson | nvarchar(max) | Yes | Yetkili kişi |
| Phone | nvarchar(max) | Yes | Telefon |
| Email | nvarchar(max) | Yes | E-posta |
| Address | nvarchar(max) | Yes | Adres |
| City | nvarchar(max) | Yes | Şehir |
| TaxNumber | nvarchar(max) | Yes | Vergi numarası |
| TaxOffice | nvarchar(max) | Yes | Vergi dairesi |
| IsFscActive | bit | No | FSC aktiflik durumu |
| IsActive | bit | No | Genel aktiflik |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

**İlişkiler:**
- FscLot.SupplierId → Suppliers.Id
- Product.SupplierId → Suppliers.Id

---

### Tablo: Customers
Müşteri tanımları (satış hedefleri ve sevkiyat izleme).

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| CustomerCode | nvarchar(max) | No | Sistem kodu (örn: MHS-001) |
| ExternalCode | nvarchar(max) | Yes | ERP kodu (ETL eşleştirme) |
| Name | nvarchar(max) | No | Müşteri adı |
| TaxNumber | nvarchar(max) | No | Vergi numarası |
| TaxOffice | nvarchar(max) | No | Vergi dairesi |
| Address | nvarchar(max) | No | Adres |
| City | nvarchar(max) | No | Şehir |
| Email | nvarchar(max) | No | E-posta |
| Phone | nvarchar(max) | No | Telefon |
| FscLicenseCode | nvarchar(max) | No | Müşteri FSC lisans kodu |
| IsFscActive | bit | No | FSC aktiflik durumu |
| FscExpiryDate | datetime2(7) | Yes | Sertifika bitiş tarihi |
| IsActive | bit | No | Genel aktiflik |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

**İlişkiler:**
- SalesOrder.CustomerId → Customers.Id
- StockMovement.CustomerId → Customers.Id (satış çıkışı için)

---

### Tablo: Products
Ürün kartları (hammadde, yarı mamül, mamul).

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| ProductGroupId | int | Yes | Grup (hammadde/YM/mamul) |
| ProductCode | nvarchar(max) | No | Sistem kodu (otomatik üretilir) |
| ExternalCode | nvarchar(max) | Yes | ERP kodu (ETL eşleştirme) |
| ProductName | nvarchar(max) | No | Ürün adı |
| Unit | nvarchar(max) | No | Birim (varsayılan: ADET) |
| FscTypeId | int | Yes | FSC tipi |
| PaperTypeId | int | Yes | Kağıt tipi |
| PaperColorId | int | Yes | Kağıt rengi |
| PaperWeightId | int | Yes | Gramaj |
| PaperWidthId | int | Yes | Bobin eni |
| SupplierId | int | Yes | Varsayılan tedarikçi |
| IsActive | bit | No | Aktiflik |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

**İlişkiler:**
- ProductGroupId → ProductGroups.Id
- FscTypeId → FscTypes.Id
- PaperTypeId → PaperTypes.Id
- PaperColorId → PaperColors.Id
- PaperWeightId → PaperWeights.Id
- PaperWidthId → PaperWidths.Id
- SupplierId → Suppliers.Id
- ProductRecipe.ParentProductId / ChildProductId → Products.Id
- FscLot.ProductId → Products.Id
- StockMovement.ProductId → Products.Id
- WorkOrder.ProductId → Products.Id

---

### Tablo: ProductRecipes
Ürün reçetesi/BOM (parent ürün + child ürünler ve oranları).

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| ParentProductId | int | No | Ana ürün (mamul) |
| ChildProductId | int | No | Bileşen ürün (hammadde/YM) |
| StandardQuantity | decimal(18,6) | No | Standart miktar |
| Unit | nvarchar(max) | No | Birim |
| IsActive | bit | No | Aktiflik |
| BilesenYeri | nvarchar(max) | Yes | Bileşen yeri (Gövde, Sap, Etiket vb.) |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

**İlişkiler:**
- ParentProductId → Products.Id (mamul)
- ChildProductId → Products.Id (hammadde/YM)

---

### Tablo: FscLots
FSC lot tanımları (Tedarikçi partileri ve dönüşüm çıktıları).

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| PartiNo | nvarchar(max) | No | Tedarikçi parti numarası (örn: 24H0604) |
| FscTypeId | int | No | FSC tipi |
| SupplierId | int | Yes | Tedarikçi |
| ProductId | int | Yes | Ürün |
| InvoiceNo | nvarchar(max) | Yes | Fatura numarası |
| DispatchNo | nvarchar(max) | Yes | İrsaliye numarası |
| ArrivalDate | datetime2(7) | No | Varış tarihi |
| TruckPlate | nvarchar(max) | Yes | Araç plakası |
| InvoiceAmount | decimal(18,2) | Yes | Fatura tutarı |
| Currency | nvarchar(max) | Yes | Para birimi (varsayılan: TRY) |
| Notes | nvarchar(max) | Yes | Notlar |
| InvoicePdfPath | nvarchar(max) | Yes | Fatura PDF yolu |
| DispatchPdfPath | nvarchar(max) | Yes | İrsaliye PDF yolu |
| SourceSerialId | int | Yes | Kaynak bobin (dönüşüm için) |
| ConversionFireKg | decimal(18,2) | Yes | Dönüşüm firesi (KG) |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

**İlişkiler:**
- FscTypeId → FscTypes.Id
- SupplierId → Suppliers.Id (nullable)
- ProductId → Products.Id (nullable)
- SourceSerialId → FscSerials.Id (nullable, dönüşüm için)

---

### Tablo: FscSerials
Lot içindeki bobinler/seriler (her lot bir veya daha çok seride bölünür).

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| LotId | int | No | İlişkili lot |
| SerialNo | nvarchar(max) | No | Bobin/seri numarası |
| LotNo | nvarchar(max) | Yes | Bobin parti numarası (tedarikçi) |
| InitialWeight | decimal(18,2) | No | Başlangıç ağırlığı (KG) |
| CurrentWeight | decimal(18,2) | No | Güncel ağırlık (KG) |
| OriginalQuantity | decimal(18,6) | Yes | Orijinal miktar (dönüşüm öncesi) |
| OriginalUnit | nvarchar(max) | Yes | Orijinal birim (MT, ADET vb.) |
| IsOpeningStock | bit | No | Açılış stoku mu? |
| Notes | nvarchar(max) | Yes | Notlar |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

**İlişkiler:**
- LotId → FscLots.Id
- FscLot.SourceSerialId → FscSerials.Id (dönüşüm izlenebilirliği)
- ProductionDetail.FscSerialId → FscSerials.Id
- WorkOrderRecipe.FscSerialId → FscSerials.Id

---

### Tablo: WorkOrders
İş emirleri (üretim planlaması ve izleme).

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| WorkOrderNo | nvarchar(max) | No | İş emri numarası (IE2026-001) |
| ExternalOrderNo | nvarchar(max) | Yes | ERP iş emri numarası |
| ProductId | int | No | Üretilecek ürün |
| MachineId | int | No | Makine |
| PlannedDate | datetime2(7) | No | Planlanan tarih |
| CompletedDate | datetime2(7) | Yes | Tamamlanma tarihi |
| PlannedQuantity | decimal(18,6) | No | Planlanan miktar |
| ActualQuantity | decimal(18,6) | No | Gerçekleştirilen miktar |
| Status | int | No | Durum (1=Taslak, 2=Üretimde, 3=Tamamlandı, 4=İptal) |
| Notes | nvarchar(max) | Yes | Notlar |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

**İlişkiler:**
- ProductId → Products.Id
- MachineId → Machines.Id
- ProductionDetail.WorkOrderId → WorkOrders.Id
- WorkOrderRecipe.WorkOrderId → WorkOrders.Id
- SalesOrderLine.WorkOrderId → WorkOrders.Id
- StockMovement.WorkOrderId → WorkOrders.Id

---

### Tablo: WorkOrderRecipes
İş emri reçete satırları (hangi bileşenler tüketileceği ve kaynaklar).

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| WorkOrderId | int | No | İş emri |
| ProductId | int | No | Reçete bileşeni (ürün) |
| FscSerialId | int | Yes | Seçilen bobin/seri |
| PlannedQuantity | decimal(18,6) | No | Planlanan miktar (KG) |
| ActualConsumedQuantity | decimal(18,6) | No | Gerçekleştirilen tüketim (KG) |
| WasteQuantity | decimal(18,6) | No | Fire (KG) |
| ProducedQuantity | decimal(18,6) | No | Üretim miktarı (adet) |
| Description | nvarchar(max) | Yes | Notlar |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

**İlişkiler:**
- WorkOrderId → WorkOrders.Id
- ProductId → Products.Id
- FscSerialId → FscSerials.Id (nullable)
- ProductionDetail.WorkOrderRecipeId → WorkOrderRecipes.Id

---

### Tablo: ProductionDetails
Seri bazlı üretim detayları (hangi bobin tüketildi, ne kadar üretildi).

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| WorkOrderId | int | No | İş emri |
| FscSerialId | int | No | Tüketilen bobin |
| MachineId | int | No | Makine |
| WorkOrderRecipeId | int | Yes | İş emri reçete satırı |
| ProductionDate | datetime2(7) | No | Üretim tarihi |
| ConsumedWeight | decimal(18,2) | No | Tüketilen ağırlık (KG) |
| WasteWeight | decimal(18,2) | No | Fire (KG) |
| ProducedQuantity | decimal(18,6) | No | Üretim miktarı |
| Notes | nvarchar(max) | Yes | Notlar |
| UnitConverted | bit | No | Birim dönüşümü uygulandı mı? |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

**İlişkiler:**
- WorkOrderId → WorkOrders.Id
- FscSerialId → FscSerials.Id
- MachineId → Machines.Id
- WorkOrderRecipeId → WorkOrderRecipes.Id (nullable)

---

### Tablo: StockMovements
Stok hareketi kayıtları (giriş, çıkış, transfer).

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| Type | int | No | Hareket tipi (1=ÜretimGiriş, 2=Transfer, 3=SatışÇıkış, 4=SatınAlmaGiriş, 5=ÜretimTüketim) |
| ErpReferenceId | int | Yes | ERP hareket numarası |
| DocumentNo | nvarchar(max) | No | Evrak numarası |
| DocumentDate | datetime2(7) | No | Evrak tarihi |
| ProductId | int | No | Ürün |
| Quantity | decimal(18,6) | No | Orijinal birim cinsinden miktar |
| Unit | nvarchar(max) | No | Orijinal birim (MT, KG, ADET vb.) |
| QuantityKg | decimal(18,6) | Yes | KG cinsinden miktar (dönüşüm varsa) |
| FromWarehouseId | int | Yes | Çıkış deposu |
| ToWarehouseId | int | Yes | Giriş deposu |
| CustomerId | int | Yes | Müşteri (satış çıkışı için) |
| PlateNumber | nvarchar(max) | Yes | Araç plakası |
| DeliveryAddress | nvarchar(max) | Yes | Teslim adresi |
| WorkOrderId | int | Yes | İş emri |
| Description | nvarchar(max) | Yes | Açıklama |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

**İlişkiler:**
- ProductId → Products.Id (NOT NULL)
- FromWarehouseId → Warehouses.Id (nullable)
- ToWarehouseId → Warehouses.Id (nullable)
- CustomerId → Customers.Id (nullable)
- WorkOrderId → WorkOrders.Id (nullable)

---

### Tablo: WasteManagements
Fire/atık yönetimi kayıtları.

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| WasteCode | nvarchar(max) | No | Atık kodu |
| WorkOrderId | int | Yes | İş emri |
| Category | int | No | Kategori (1=Kesim, 2=Baskı, 3=Islanma, 4=Nakliye, 5=Makine, 99=Diğer) |
| Description | nvarchar(max) | No | Açıklama |
| Quantity | decimal(18,2) | No | Miktar |
| Unit | nvarchar(max) | No | Birim (varsayılan: kg) |
| DisposalDate | datetime2(7) | No | İmha tarihi |
| DisposalMethod | nvarchar(max) | Yes | İmha yöntemi (İmha, Geri Dönüşüm, Satış) |
| DisposedBy | nvarchar(max) | Yes | İmha yapan |
| Notes | nvarchar(max) | Yes | Notlar |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

**İlişkiler:**
- WorkOrderId → WorkOrders.Id (nullable)

---

### Tablo: SalesOrders
Satış siparişleri (müşteriye sevkiyat).

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| SalesOrderNo | nvarchar(max) | No | Sipariş numarası |
| ExternalOrderNo | nvarchar(max) | Yes | ERP sipariş numarası |
| CustomerId | int | No | Müşteri |
| OrderDate | datetime2(7) | No | Sipariş tarihi |
| DispatchDate | datetime2(7) | Yes | Sevkiyat tarihi |
| DispatchNo | nvarchar(max) | Yes | İrsaliye numarası |
| InvoiceNo | nvarchar(max) | Yes | Fatura numarası |
| InvoiceAmount | decimal(18,2) | Yes | Fatura tutarı |
| Currency | nvarchar(max) | No | Para birimi (varsayılan: TRY) |
| PlateNumber | nvarchar(max) | Yes | Araç plakası |
| DeliveryAddress | nvarchar(max) | Yes | Teslim adresi |
| Status | int | No | Durum (1=Taslak, 2=TeslimEdildi, 3=İptal) |
| DispatchPdfPath | nvarchar(max) | Yes | İrsaliye PDF yolu |
| InvoicePdfPath | nvarchar(max) | Yes | Fatura PDF yolu |
| Notes | nvarchar(max) | Yes | Notlar |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

**İlişkiler:**
- CustomerId → Customers.Id
- SalesOrderLine.SalesOrderId → SalesOrders.Id

---

### Tablo: SalesOrderLines
Satış sipariş satırları (her sipariş bir veya daha çok satır içerir).

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| SalesOrderId | int | No | Satış sipariş |
| ProductId | int | No | Ürün |
| WorkOrderId | int | Yes | İş emri (CoC zinciri için) |
| Quantity | decimal(18,6) | No | Miktar |
| UnitPrice | decimal(18,2) | No | Birim fiyatı |
| Unit | nvarchar(max) | No | Birim (varsayılan: Adet) |
| Notes | nvarchar(max) | Yes | Notlar |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

**İlişkiler:**
- SalesOrderId → SalesOrders.Id
- ProductId → Products.Id
- WorkOrderId → WorkOrders.Id (nullable)

---

## Sistem & Yönetim

### Tablo: AppUsers
Sistem kullanıcıları.

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| Username | nvarchar(max) | No | Kullanıcı adı |
| PasswordHash | nvarchar(max) | No | BCrypt hash |
| FullName | nvarchar(max) | No | Tam ad |
| Email | nvarchar(max) | Yes | E-posta |
| IsAdmin | bit | No | Admin mi? |
| IsActive | bit | No | Aktiflik |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| LastLoginDate | datetime2(7) | Yes | Son giriş tarihi |

**İlişkiler:**
- UserGroup.UserId → AppUsers.Id
- UserPermissionOverride.UserId → AppUsers.Id

---

### Tablo: PermissionGroups
Yetki grupları (Operatör, Muhasebe, Yönetici vb.).

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| Name | nvarchar(max) | No | Grup adı |
| Description | nvarchar(max) | Yes | Açıklama |
| IsActive | bit | No | Aktiflik |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| CreatedBy | nvarchar(max) | No | Oluşturan |

**İlişkiler:**
- UserGroup.GroupId → PermissionGroups.Id
- GroupPermission.GroupId → PermissionGroups.Id

---

### Tablo: PermissionModules
Yetkilendirilebilir modüller (PURCHASE, PRODUCTION vb.).

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| Code | nvarchar(max) | No | Kod (PURCHASE, PRODUCTION) |
| DisplayName | nvarchar(max) | No | Görünen adı |
| Description | nvarchar(max) | Yes | Açıklama |
| IconClass | nvarchar(max) | No | FontAwesome sınıfı |
| SortOrder | int | No | Sıralama |

**İlişkiler:**
- GroupPermission.ModuleId → PermissionModules.Id
- UserPermissionOverride.ModuleId → PermissionModules.Id

---

### Tablo: UserGroups
Kullanıcı-Grup çoka-çok tablosu.

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| UserId | int | No | Kullanıcı (PK1) |
| GroupId | int | No | Grup (PK2) |

**İlişkiler:**
- UserId → AppUsers.Id
- GroupId → PermissionGroups.Id

---

### Tablo: GroupPermissions
Grup-Modül yetki satırları.

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| GroupId | int | No | Grup (PK1) |
| ModuleId | int | No | Modül (PK2) |
| CanRead | bit | No | Okuma |
| CanWrite | bit | No | Yazma |
| CanDelete | bit | No | Silme |

**İlişkiler:**
- GroupId → PermissionGroups.Id
- ModuleId → PermissionModules.Id

---

### Tablo: UserPermissionOverrides
Kullanıcı-özgü yetki istisnası.

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| UserId | int | No | Kullanıcı (PK1) |
| ModuleId | int | No | Modül (PK2) |
| CanRead | bit | Yes | null = devral, true = zorla aç, false = zorla kapat |
| CanWrite | bit | Yes | null = devral, true = zorla aç, false = zorla kapat |
| CanDelete | bit | Yes | null = devral, true = zorla aç, false = zorla kapat |

**İlişkiler:**
- UserId → AppUsers.Id
- ModuleId → PermissionModules.Id

---

### Tablo: AuditLogs
Tüm değişikliklerin denetim kaydı.

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | bigint | No | PK |
| TableName | nvarchar(max) | No | Tablo adı |
| RecordId | int | Yes | Kayıt ID |
| Action | nvarchar(max) | No | İşlem (INSERT, UPDATE, DELETE) |
| OldValues | nvarchar(max) | Yes | Eski değerler (JSON) |
| NewValues | nvarchar(max) | Yes | Yeni değerler (JSON) |
| ChangedBy | nvarchar(max) | Yes | Değiştiren |
| ChangedAt | datetime2(7) | No | Değişiklik tarihi |
| IpAddress | nvarchar(max) | Yes | IP adresi |

---

### Tablo: AuditPeriods
FSC CoC denetim dönemleri (kilitlenebilir).

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| Year | int | No | Denetim yılı |
| StartDate | datetime2(7) | No | Dönem başlangıcı |
| EndDate | datetime2(7) | No | Dönem bitişi |
| Description | nvarchar(max) | Yes | Notlar |
| IsActive | bit | No | Aktiflik |
| IsLocked | bit | No | Kilitli mi? |
| LockedAt | datetime2(7) | Yes | Kilitlenme tarihi |
| LockedBy | nvarchar(max) | Yes | Kilitleyenin adı |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| CreatedBy | nvarchar(max) | No | Oluşturan |

---

### Tablo: FscDocuments
FSC dokümantasyon arşivi.

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| Title | nvarchar(max) | No | Başlık |
| Category | int | No | Kategori (1=CoC, 2=Tedarikçi, 3=Anlaşma vb.) |
| Year | int | No | Yıl |
| FileName | nvarchar(max) | No | Dosya adı |
| FilePath | nvarchar(max) | No | Dosya yolu |
| FileSize | bigint | No | Dosya boyutu (byte) |
| FileExtension | nvarchar(max) | No | Uzantı (pdf, docx vb.) |
| Notes | nvarchar(max) | Yes | Notlar |
| Tags | nvarchar(max) | Yes | Etiketler (virgülle ayrılmış) |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

---

### Tablo: EtlConnections
ETL bağlantı tanımları (Excel, Logo, Netsis vb.).

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| Name | nvarchar(max) | No | Bağlantı adı |
| Type | nvarchar(max) | No | Tipi (Excel, Logo, Mikro, Netsis, Api, SqlServer) |
| Description | nvarchar(max) | Yes | Açıklama |
| Settings | nvarchar(max) | Yes | JSON ayarları |
| IsActive | bit | No | Aktiflik |
| LastSyncAt | datetime2(7) | Yes | Son senkronizasyon tarihi |
| LastSyncStatus | nvarchar(max) | Yes | Son durum (Success, Failed, Partial) |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

**İlişkiler:**
- EtlJob.EtlConnectionId → EtlConnections.Id

---

### Tablo: EtlJobs
ETL işi kayıtları.

| Kolon | Tip | Nullable | Açıklama |
|-------|-----|----------|----------|
| Id | int | No | PK |
| EtlConnectionId | int | Yes | Bağlantı |
| JobType | nvarchar(max) | No | İş tipi (ProductImport, SupplierImport vb.) |
| Source | nvarchar(max) | No | Kaynak (Excel, Logo, Netsis) |
| Status | nvarchar(max) | No | Durum (Running, Completed, Failed, Partial) |
| StartedAt | datetime2(7) | No | Başlangıç zamanı |
| CompletedAt | datetime2(7) | Yes | Tamamlanma zamanı |
| TotalRecords | int | No | Toplam kayıt |
| InsertedCount | int | No | Eklenen |
| UpdatedCount | int | No | Güncellenen |
| SkippedCount | int | No | Atlanan |
| ErrorCount | int | No | Hata |
| SourceFile | nvarchar(max) | Yes | Kaynak dosya |
| Notes | nvarchar(max) | Yes | Notlar |
| ErrorDetails | nvarchar(max) | Yes | Hata detayları |
| CreatedBy | nvarchar(max) | No | Oluşturan |
| CreatedDate | datetime2(7) | No | Oluşturma tarihi |
| UpdatedBy | nvarchar(max) | Yes | Güncelleyen |
| UpdatedDate | datetime2(7) | Yes | Güncelleme tarihi |

**İlişkiler:**
- EtlConnectionId → EtlConnections.Id

---

## Tüm Tablo Referansı (Alfabetik)

| Tablo | Tür | Satırlar | Açıklama |
|-------|-----|---------|----------|
| AppUsers | Sistem | ~50 | Kullanıcı hesapları |
| AuditLogs | Sistem | ~10K | Değişiklik denetim kaydı |
| AuditPeriods | Sistem | ~10 | Denetim dönemleri |
| BagTypes | Ayar | ~10 | Torba tipi tanımları |
| Customers | İş | ~500 | Müşteri kartları |
| EtlConnections | Sistem | ~20 | ETL bağlantıları |
| EtlJobs | Sistem | ~200 | ETL işi geçmişi |
| FscDocuments | Sistem | ~200 | Dokümantasyon arşivi |
| FscLots | İş | ~5K | Hammadde lotları |
| FscSerials | İş | ~50K | Bobin serileri |
| FscTypes | Ayar | ~10 | FSC sertifika tipleri |
| GroupPermissions | Sistem | ~100 | Grup yetkileri |
| PaperColors | Ayar | ~20 | Kağıt renkleri |
| PaperTypes | Ayar | ~10 | Kağıt türleri |
| PaperWeights | Ayar | ~30 | Gramaj değerleri |
| PaperWidths | Ayar | ~20 | Bobin en değerleri |
| PermissionGroups | Sistem | ~10 | Yetki grupları |
| PermissionModules | Sistem | ~30 | Modüller |
| ProductGrammages | Ayar | ~30 | Ürün gramajları |
| ProductGroups | Ayar | ~20 | Ürün grupları |
| ProductRecipes | İş | ~500 | Reçete/BOM |
| Products | İş | ~1K | Ürün katalog |
| ProductionDetails | İş | ~50K | Üretim detayları |
| SalesOrderLines | İş | ~2K | Satış satırları |
| SalesOrders | İş | ~500 | Satış siparişleri |
| StockMovements | İş | ~50K | Stok hareketleri |
| Suppliers | İş | ~200 | Tedarikçi kartları |
| UnitConversions | Ayar | ~50 | Birim dönüşümleri |
| UserGroups | Sistem | ~100 | Kullanıcı-Grup çoka-çok |
| UserPermissionOverrides | Sistem | ~50 | Kullanıcı yetki istisnaları |
| Warehouses | Ayar | ~10 | Depo tanımları |
| WasteManagements | İş | ~1K | Atık yönetimi |
| WorkOrderRecipes | İş | ~5K | İş emri reçete satırları |
| WorkOrders | İş | ~2K | İş emirleri |

---

## Not: Audit Alanları

Çoğu tablo BaseEntity'den türediği için şu audit alanlarını içerir:
- `CreatedBy` (nvarchar, NOT NULL)
- `CreatedDate` (datetime2, NOT NULL, varsayılan: NOW)
- `UpdatedBy` (nvarchar, nullable)
- `UpdatedDate` (datetime2, nullable)

Bazı tablolar (Machine, PaperWeight, AuditLog vb.) manuel olarak tanımlanmış audit alanlarına sahiptir.

---

**Oluşturma tarihi:** 2026-06-22  
**Son güncelleme:** 2026-06-22  
**Versiyon:** 1.0
