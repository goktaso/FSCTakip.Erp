# TODO - Production Detail BOM Panel İyileştirmeleri

## BOM Bileşen Analizi Panel İyileştirmeleri
- [x] BOM panelinde "Kalan İhtiyaç" gösterimi eklenecek (Planned - Actual)
- [x] Tüketim modalında seçilen bileşen için ihtiyaç bilgisi gösterilecek
- [x] Progress bar rengi güncellenecek (kırmızı > %110, yeşil %90-110, sarı < %90)
- [x] Her tüketim kaydından sonra hızlı yenileme (800ms delay + page reload)

## Düzeltmeler
- [x] BOM bileşen "Toplam" gösterimi - wrToplamTuketim (bileşen bazlı) kullanılıyor

## Tamamlananlar
- [x] PDF Upload Modernization (Purchase/Index.cshtml)
- [x] Product Search in New Lot Modal (Purchase/Index.cshtml)
- [x] BOM Panel İyileştirmeleri (Production/Detail.cshtml)

---

# Fonksiyonel Denetim (2026-06-13) — Buton/Form/Dropdown/Textbox↔DB

> Yöntem: view buton/form/AJAX → controller action; form alanı → model property; textbox → DB kolon tipi (decimal scale, string length, int, date); HTTP verb uyumu.

## Modül Durumu (✅ = denetlendi, temiz / form↔controller↔DB eşleşiyor)
| Modül | Durum |
|-------|-------|
| Home / Layout topbar | ✅ düzeltildi (F1) |
| Customers | ✅ temiz (not: IsFscActive form alanı yok — N1) |
| Suppliers | ✅ temiz |
| Products / Recipe | ✅ temiz |
| Product (BagType/Group) | ✅ temiz |
| Paper (Types/Colors/Fsc/Width/Weight) | ✅ temiz |
| Machine / Warehouse | ✅ temiz |
| Purchase (Lot/Serial) | ✅ temiz |
| Production (WO/Detail/Waste) | ✅ temiz |
| Sales (Order/Line/Dispatch) | ✅ temiz |
| Stock (Transfer/Movements/RawMaterial) | ✅ temiz |
| Planning / UnitConversion | ✅ temiz |
| Reports (8 menü raporu) | ✅ temiz (Traceability sipariş-bazlı drill-down) |
| Etl | ✅ temiz |
| AuditPeriod / AuditLog / Users / Groups / Account | ✅ temiz |

## Bulgular
### ✅ F1 — Topbar bildirim çanı + global arama kırık (TÜM SAYFALAR)
`_Layout.cshtml` `/Home/GetNotifications` ve `/Home/GlobalSearch` çağırıyordu ama HomeController'da bu action'lar yoktu → çan ve üst arama hiç çalışmıyordu.
**Düzeltme:** İki action eklendi (FSC expiry + düşük stok bildirimi; müşteri/tedarikçi/ürün/lot araması). Canlı doğrulandı: GetNotifications gerçek JSON dönüyor.

### ✅ F2 — NOT NULL string kolonlara null INSERT/UPDATE hatası (SİSTEMİK, tüm formlar)
Customers tablosunda 9 string kolon (Name, TaxNumber, TaxOffice, Address, City, Email, Phone, FscLicenseCode, CustomerCode) **NOT NULL**. ASP.NET Core boş form alanını varsayılan olarak `null`'a bind ediyor → bu alanlardan biri boş bırakılınca `SqlException 515: Cannot insert NULL` → müşteri kaydı/düzenlemesi **başarısız** (kullanıcıya "An error occurred while saving the entity changes"). Aynı risk NOT NULL string kolonu olan tüm entity'lerde mevcuttu.
**Kök neden:** Entity'ler non-nullable string (intent: boş = `""`) ama DB'ye null gidiyor.
**Düzeltme:** `AppDbContext.SaveChangesAsync` içindeki string döngüsüne `null && !prop.IsNullable → ""` coalesce eklendi → tek noktada tüm entity'ler için 515 hatası önlenir. Canlı doğrulandı: boş FscLicenseCode+Phone ile müşteri kaydı artık başarılı.

### ✅ F4 — Değişmemiş geçersiz e-posta/telefon düzenlemeyi engelliyordu (Customers + Suppliers)
DB'de geçersiz formatlı e-posta (örn. `-`) veya kısa telefonu olan eski kayıtlar, başka bir alan (örn. FSC checkbox) güncellenmek istendiğinde `Save`'deki regex/uzunluk doğrulamasına takılıp kaydedilemiyordu.
**Düzeltme:** Customers + Suppliers `Save` → düzenlemede mevcut e-posta/telefon DB'den alınıyor; **yalnızca değer değiştiyse** format/uzunluk doğrulaması uygulanıyor. Canlı doğrulandı: değişmemiş geçersiz e-postalı kayıt artık güncellenebiliyor; yeni girilen geçersiz e-posta hâlâ reddediliyor (doğrulama bütünlüğü korundu).

### ✅ N1→F3 — Customers düzenleme formuna `IsFscActive` checkbox eklendi
Formda alan yoktu; bind edilmediğinden düzenlemede entity default'u (true) kalıyordu. FSC bölümüne hidden(false)+checkbox(true) kalıbıyla checkbox eklendi; openAdd varsayılanı true, editCustomer JS'i `res.data.isFscActive` ile dolduruyor. Canlı round-trip doğrulandı (False↔True kaydoluyor).

## Sonuç
- Derleme: **0 hata** (yalnızca nullable uyarıları).
- Tüm sidebar menü linkleri (37 link) geçerli action'a çözülüyor.
- Tüm view buton/form/AJAX çağrıları controller action'larıyla eşleşiyor (Home hariç — düzeltildi).
- Form alan adları ↔ entity property'leri ↔ DB tipleri uyumlu; checkbox'lar `.checked` ile bool'a doğru bind ediliyor; dropdown'lar controller'da dolduruluyor.
- **Canlı duman testi (IIS Express yerine derlenmiş DLL/Kestrel):** ADMİN oturumuyla 33 sayfa + 2 düzeltilen endpoint test edildi → hepsi HTTP 200, 500 hatası yok.
