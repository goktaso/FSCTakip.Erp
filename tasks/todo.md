# FSC Takip ERP — Görev Listesi

## Aktif Görevler

_Buraya aktif sprint görevleri eklenir._

## Bekleyen Görevler

### Faz 1 — İşlem Modülleri
- [ ] StockController: Stok durumu sayfası (depo bazlı görünüm)
- [ ] StockController: Depo transfer işlemi
- [ ] Satış modülü: SalesOrder → SalesDetail zincirleme tüketim

### Faz 2 — Belge Yönetimi
- [ ] Satın alma belgelerinde PDF önizleme iyileştirme
- [ ] Toplu belge arşiv export (zip)

### Faz 3 — Raporlama
- [ ] FSC CoC (Chain of Custody) resmi rapor formatı (PDF)
- [ ] Denetim özet raporu export (Excel)
- [ ] Lot takip — bobin izleme raporu

### Faz 4 — ETL/ERP Entegrasyonu
- [ ] Netsis senkronizasyonu (hammadde)
- [ ] Netsis senkronizasyonu (satış irsaliyeleri)
- [ ] ETL hata loglama ve yeniden deneme mekanizması

## Tamamlanan Görevler

- [x] PurchaseController: Hammadde girişi + lot/seri kayıt
- [x] ProductionController: İş emri + üretim detayı + fire
- [x] SalesController: Satış irsaliyesi/fatura çıkışı (temel)
- [x] FSC CoC tamamlama koruması (FSC bileşen tüketim zorunluluğu)
- [x] İş emri: Bobin dropdown zenginleştirme (ürün kodu, dış kod, lot, seri)
- [x] İş emri: Tüketimden üretim miktarı otomatik hesaplama
- [x] Belge arşiv modülü (FscDocument entity + DocumentArchiveController)
- [x] DocumentArchive CRUD + PDF yükleme + önizleme
- [x] İş emri kullanım kılavuzu PDF (docs/IS_Emri_Kullanim_Kilavuzu.pdf)

---

## CHECKPOINT

_Son durum:_ claude-config repo analizi tamamlandı; governance ayarları ve todo.md ekleniyor.

_Sıradaki:_ tasks/todo.md ve .claude/settings.json oluşturma ✓
