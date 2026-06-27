# Claude Code Prompt Seti — FSCTakip.Erp

Bu dosya projeyi Claude Code'a anlatmak ve yeni görevler vermek için hazır prompt şablonları içerir.

---

## MASTER PROMPT — Projeyi Tanıtma

Yeni bir konuşma başlatırken önce CLAUDE.md'yi okutun, sonra bu promptu kullanın:

```
Bu proje FSCTakip.Erp — kraft kağıt ve ambalaj sektöründe FSC (Forest Stewardship Council) 
sertifikasyonu takibi için ASP.NET Core 8 MVC uygulaması. 

Mimari: 4 katmanlı (Core, DataAccess, Business, WebUI)
Stack: SQL Server, Entity Framework Core, Bootstrap 5, ClosedXML, jQuery
Dil: C#, Razor Views, Türkçe UI

Mevcut CLAUDE.md dosyasını oku ve projeyi tam anlayarak devam et.
```

---

## PROMPT 1 — Hammadde Girişi Modülü

```
FSCTakip.Erp projesinde Hammadde Girişi (Purchase) modülünü geliştir.

YAPILACAKLAR:
1. docs/03_YENI_ENTITY_KODLARI.md dosyasındaki FscLot ve FscSerial güncellemelerini entity dosyalarına ekle
2. docs/04_CONTROLLER_SABLONLARI.md'deki PurchaseController'ı oluştur (FSCTakip.WebUI/Controllers/PurchaseController.cs)
3. Şu View'ları oluştur:
   - Views/Purchase/Index.cshtml — Lot listesi (tarih filtreli, tedarikçi filtreli)
   - Views/Purchase/Create.cshtml — Yeni lot oluşturma formu
   - Views/Purchase/Detail.cshtml — Lot detayı (seri listesi + belge linkleri)

VIEW STANDARTLARI (mevcut projeyle uyumlu):
- Bootstrap 5 table + modal dialog pattern kullan
- Sidebar sol menü yeşil (#1e3d14) — _Layout.cshtml ile uyumlu
- AJAX form submit, JSON response, toast/alert bildirim
- Tarih formatı: dd.MM.yyyy (Türkçe)
- Ağırlık birimi: kg, 2 ondalık basamak
- FSC sertifikası geçersiz tedarikçilerde sarı uyarı badge göster

BELGE YÜKLEME:
- İrsaliye ve fatura PDF yükleme alanı (input type=file, accept=".pdf")
- Yüklenen dosya wwwroot/uploads/purchases/yyyy/MM/ klasörüne kaydedilecek
- Kaydedilen yol veritabanında FscLot.InvoicePdfPath ve DispatchPdfPath alanlarına yazılacak
- Belge linki: <a href="/Purchase/ViewDocument?path=..." target="_blank"> ile PDF yeni sekmede açılacak

EF CORE MİGRASYON:
- Yeni alanlar için migration oluştur: AddPurchaseLotFields
- Migration komutu: cd FSCTakip.DataAccess && dotnet ef migrations add AddPurchaseLotFields --startup-project ../FSCTakip.WebUI
```

---

## PROMPT 2 — Üretim Modülü

```
FSCTakip.Erp projesinde Üretim modülünü geliştir.

YAPILACAKLAR:
1. docs/03_YENI_ENTITY_KODLARI.md'deki WorkOrder güncellemelerini uygula
2. docs/04_CONTROLLER_SABLONLARI.md'deki ProductionController'ı oluştur
3. Şu View'ları oluştur:
   - Views/Production/WorkOrders.cshtml — İş emri listesi (durum filtreli)
   - Views/Production/Detail.cshtml — İş emri detayı + üretim giriş formu
   - Views/Production/WasteReport.cshtml — Fire raporu

WORKORDER DURUM MAKİNESİ:
Bekliyor → DevamEdiyor → Tamamlandi / Iptal
Status değişiminde ilgili tarih alanları güncellenir (StartDate, EndDate)

ÜRETİM DETAYI FORMU (Detail sayfasındaki modal):
- Seri No seçimi: dropdown (sadece CurrentWeight > 0 olanlar)
- Seri seçildiğinde kalan ağırlık gösterilir (AJAX ile /Production/GetSerialInfo/{id})
- Tüketim alanı (ConsumptionArea): dropdown (TorbaGovde, Sap, Etiket, Yapiskan)
- Tüketilen ağırlık (kg): number input, max = kalan ağırlık
- Fire ağırlığı (kg): number input
- Üretilen adet: number input
- Dönüşüm oranı: otomatik hesaplanır ((Tüketilen - Fire) / Tüketilen * 100)

FİRE HESAPLAMA KURALI:
Fire oranı > %15 ise kırmızı uyarı göster
Fire oranı %10-15 arası sarı uyarı

İŞ EMRİ TAMAMLAMA:
- "Tamamla" butonu tıklandığında depo seçim modal açılır
- Onaylanınca StockMovement (Type=ProductionEntry) oluşturulur
- İş emri IsCompleted=true, Status=Tamamlandi, EndDate=şimdiki zaman
```

---

## PROMPT 3 — Satış Modülü

```
FSCTakip.Erp projesinde Satış modülünü geliştir.

YAPILACAKLAR:
1. docs/03_YENI_ENTITY_KODLARI.md'deki SalesOrder ve SalesOrderLine entity'lerini oluştur:
   - FSCTakip.Core/Entities/SalesOrder.cs
2. AppDbContext'e SalesOrders ve SalesOrderLines DbSet ekle
3. Migration oluştur: AddSalesOrder
4. docs/04_CONTROLLER_SABLONLARI.md'deki SalesController'ı oluştur
5. Şu View'ları oluştur:
   - Views/Sales/Index.cshtml — Sipariş listesi
   - Views/Sales/Create.cshtml — Yeni sipariş (satır ekleme destekli)
   - Views/Sales/Detail.cshtml — Sipariş detayı + belge yükleme

SATIR EKLEME UI:
- "Ürün Ekle" butonu ile dinamik satır ekleme (JavaScript ile)
- Her satırda: Ürün seçimi (dropdown), Miktar, Birim, Birim Fiyat, İş Emri (opsiyonel)
- Toplam tutar otomatik hesaplanır

SEVK İŞLEMİ:
- "Sevk Et" butonu bir modal açar
- Modal'da: İrsaliye No, araç plakası, PDF yükleme alanları
- Onaylanınca her satır için StockMovement (Type=SalesDispatch) oluşturulur
- FSC gerekli mi? checkbox — müşteri FSC kodu kontrolü

BELGE YÖNETİMİ:
- İrsaliye ve fatura PDF yükleme
- wwwroot/uploads/sales/yyyy/MM/ klasörüne kaydet
- Belgeler Detail sayfasında link olarak listelenir (yeni sekmede açılır)
```

---

## PROMPT 4 — Stok Ekranları

```
FSCTakip.Erp projesinde Stok modülünü geliştir. StockController mevcut ama stub halde.

YAPILACAKLAR:
1. StockController.cs'yi genişlet
2. Şu View'ları oluştur:
   - Views/Stock/Index.cshtml — Ürün bazlı stok durumu
   - Views/Stock/Movements.cshtml — Stok hareketleri listesi
   - Views/Stock/RawMaterial.cshtml — Hammadde (bobin) durumu
   - Views/Stock/Transfer.cshtml — Depo transferi

STOK HESAPLAMA SQL SORGUSU (EF LINQ):
```csharp
var stocks = _context.StockMovements
    .Include(sm => sm.Product)
    .GroupBy(sm => new { sm.ProductId, sm.Product.ProductCode, sm.Product.ProductName })
    .Select(g => new StockViewModel
    {
        ProductCode = g.Key.ProductCode,
        ProductName = g.Key.ProductName,
        CurrentStock = g.Sum(sm => 
            sm.Type == MovementType.ProductionEntry || sm.Type == MovementType.PurchaseEntry
                ? sm.Quantity
                : -sm.Quantity)
    })
    .Where(s => s.CurrentStock > 0)
    .ToList();
```

HAMMADDE EKRANI:
- FscSerial listesi (CurrentWeight > 0 olanlar)
- Sütunlar: Lot No, Seri No, Tedarikçi, FSC Tipi, Başlangıç Ağırlık, Kalan Ağırlık, Tüketim %
- Renk kodu: Kalan > %50 yeşil, %20-50 sarı, < %20 kırmızı

DEPO TRANSFERİ:
- Ürün seçimi, kaynak depo, hedef depo, miktar, açıklama
- StockMovement (Type=WarehouseTransfer) oluşturur
```

---

## PROMPT 5 — FSC Raporları

```
FSCTakip.Erp projesinde FSC denetim raporlarını geliştir.

YAPILACAKLAR:
1. docs/04_CONTROLLER_SABLONLARI.md'deki ReportsController'ı oluştur
2. Şu View'ları oluştur:
   - Views/Reports/Index.cshtml — Rapor ana menüsü (kartlar halinde)
   - Views/Reports/ChainOfCustody.cshtml — FSC CoC raporu
   - Views/Reports/WasteSummary.cshtml — Fire özet raporu
   - Views/Reports/SupplierFsc.cshtml — Tedarikçi FSC durumu

CHAIN OF CUSTODY RAPORU:
- Hiyerarşik gösterim: Lot → Seri → Üretim Detayları
- Sütunlar: Lot No, Tedarikçi, FSC Tipi, Seri No, Başlangıç Ağırlık, İş Emri, Üretim Tarihi, Tüketilen, Fire, Üretilen Adet
- Tarih aralığı filtresi + FSC tipi filtresi
- Excel export butonu (/Reports/ExportChainOfCustody)

TEDARIKÇI FSC DURUMU:
- Trafik ışığı renklendirme:
  - Yeşil: Geçerli (> 90 gün kalan)
  - Sarı: Yakında doluyor (≤ 90 gün)
  - Kırmızı: Süresi dolmuş
- Toplamda kaç tedarikçi, kaçı geçerli, kaçı yakında doluyor: özet kartlar
- Her satırda: Tedarikçi Adı, FSC Kodu, Son Geçerlilik, Kalan Gün, Durum badge

EXCEL EXPORT:
- BaseController.ExportToExcel<T>() generic metodu kullan
- ClosedXML formatı mevcut projeyle aynı (header koyu yeşil, beyaz metin)
```

---

## PROMPT 6 — ETL / ERP Entegrasyon Modülü

```
FSCTakip.Erp projesinde ETL/ERP entegrasyon modülünü geliştir.

ÖNCELİKLE OKU: docs/02_ETL_ERP_MIMARISI.md

YAPILACAKLAR:
1. docs/03_YENI_ENTITY_KODLARI.md'deki ETL entity'lerini oluştur:
   - FSCTakip.Core/Entities/Etl/ klasörü oluştur
   - ErpConnection.cs, ErpSyncLog.cs, ErpFieldMapping.cs
   - ErpStagingPurchase.cs, ErpStagingSale.cs
2. AppDbContext'e ETL DbSet'leri ekle
3. Migration oluştur: AddEtlTables
4. EtlController.cs oluştur
5. View'lar:
   - Views/Etl/Index.cshtml — ETL paneli (bağlantılar + son sync)
   - Views/Etl/Connections.cshtml — Bağlantı yönetimi
   - Views/Etl/SyncLogs.cshtml — Log listesi
   - Views/Etl/Staging.cshtml — Staging kayıt onayı

ETL SERVIS:
- FSCTakip.Business/Services/ErpEtlService.cs oluştur
- IErpEtlService interface (docs/02_ETL_ERP_MIMARISI.md'de tanımlı)
- Logo Tiger SQL sorguları docs/02_ETL_ERP_MIMARISI.md'de mevcut

BAĞLANTI KAYDETME:
- Şifre alanı: ProtectedData.Protect() ile şifrele (Windows DPAPI)
- Test bağlantısı: "SELECT 1" ile basit kontrol
- Bağlantı tipi değiştiğinde ilgili form alanları görünür/gizlenir (JavaScript)

GÜVENLİK:
- SQL şifreler asla plaintext saklanmaz
- Staging kayıtları onay gerektirmeden otomatik aktarılmaz
```

---

## PROMPT 7 — Dashboard Geliştirme

```
FSCTakip.Erp projesinde Home/Index.cshtml dashboard'unu geliştir.

DASHBOARD KARTLARI (Bootstrap card grid):
Satır 1:
- Bu ay hammadde girişi (kg) — FscSerials toplam InitialWeight (bu ay)
- Açık iş emirleri — WorkOrders Status=Bekliyor|DevamEdiyor count
- Bu ay satış (adet) — SalesOrders bu ay count
- Toplam aktif bobin — FscSerials CurrentWeight > 0 count

Satır 2:
- FSC sertifikası dolmak üzere tedarikçiler (sarı kart, varsa)
- FSC sertifikası geçersiz tedarikçiler (kırmızı kart, varsa)
- Bu ayki fire oranı (%) — ProductionDetails bu ay avg
- Son 5 lot girişi listesi

RENK ŞEMASI:
- İstatistik kartlar: yeşil/mavi/turuncu/kırmızı Bootstrap renkleri
- FSC uyarı kartları: warning sarı, danger kırmızı
- Tablo stilleri: mevcut projeyle aynı

VERİ ÇEKME:
- HomeController.Index() metodunu genişlet
- ViewModel oluştur: DashboardViewModel.cs (ViewModels/ klasöründe)
- Null/sıfır değer kontrolü (uygulama yeni başlatıldığında boş veri olabilir)
```

---

## HIZLI GÖREV PROMPTLARI

### Excel export ekle (herhangi bir controller için)
```
[ControllerAdı] controller'ına Excel export ekle. 
BaseController.ExportToExcel<T>() metodunu kullan.
Endpoint: GET /[Controller]/Export
Dosya adı: [DataType]_{tarih:ddMMyyyy}.xlsx
Kolonlar: [ilgili entity alanları]
```

### Yeni filtre ekle (mevcut bir sayfaya)
```
[Sayfa/View yolu] sayfasına [alan adı] filtresi ekle.
- Form: GET method, mevcut diğer filtrelerle birlikte
- Controller: Query'ye .Where() koşulu ekle
- URL parametresi olarak sayfada kalsın (GET ile)
```

### Modal form iyileştirme
```
[View yolu] sayfasındaki modal form'a [alan adı] alanını ekle.
Entity: [entity adı]
Validation: [gerekli mi, format kısıtı var mı]
Mevcut projedeki diğer modal formlarla aynı Bootstrap 5 pattern kullan.
```
