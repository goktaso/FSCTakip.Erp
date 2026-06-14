# FSC Zincir Denetimi (CoC) — Uçtan Uca Kontrol Listesi

> Ham kağıt alımı → Yarı mamül dönüşümü → Mamul üretimi → (Mamul stok) → Satış sevkiyatı.
> Her adımda: ekran · ne kontrol edilir · **bizim gerçek verimizle beklenen değer**.
> Doğrulama tarihi: 2026-06-14 · Demo zinciri: 10267/10274/10052 ham → 23357/20017/21036 YM → 30443 mamul.

---

## 1. Hammadde Girişi (alım belgeleriyle)
- **Ekran:** Satın Alma / Hammadde Girişi (`/Purchase/Index`) → satıra tıkla → **Detay**
- **Kontrol:** Parti, tedarikçi, FSC tipi, bobin/kg, **İrsaliye + Fatura PDF** açılıyor mu.
- **Beklenen (gerçek):**
  - HAM-DEMO-001 · 10267 NATRON · FSC RECYCLED 100% · 2.000 kg · IRS-2025-3001 / FAT-2025-4001
  - HAM-DEMO-002 · 10274 · 250 kg · HAM-DEMO-003 · 10052 · 100 kg
- **FSC denetim noktası:** Tedarikçinin FSC sertifikası geçerli mi → **FSC Raporlar → Tedarikçi FSC**.

## 2. Hammadde Stoğu (giriş sonrası)
- **Ekran:** Stok → **Hammadde Stoğu** (`/Stock/RawMaterial`)
- **Kontrol:** Dış Kod · Giriş · Tüketim · Fire · Kalan · FSC tipi.
- **Beklenen (gerçek):**
  | Bobin | Dış Kod | Giriş | Tüketim | Fire | Kalan |
  |---|---|---:|---:|---:|---:|
  | HAM-DEMO-001 | 10267 | 2.000 | 1.500 | 50 | 500 |
  | HAM-DEMO-002 | 10274 | 250 | 185 | 5 | 65 |
  | HAM-DEMO-003 | 10052 | 100 | 52 | 2 | 48 |
- **Kural:** Giriş = Tüketim + Kalan; Tüketim brüttür (fire dahil), Kalan fire'ı düşmüştür.

## 3. Yarı Mamül Dönüşümü
- **Ekran:** FSC İşlemleri → **Yarı Mamül Dönüşüm** (`/Conversion`) → Son Dönüşümler
- **Kontrol:** Üretilen YM · FSC tipi **kaynaktan devralındı** mı · kalan.
- **Beklenen (gerçek):**
  - YM26-001 · 23357 BB Edeka · 1.450 kg · FSC RECYCLED 100%
  - YM26-002 · 20017 Sap · 180 kg · YM26-003 · 21036 Etiket · 50 kg (hepsi RECYCLED 100%)
- **CoC noktası:** Ham kağıdın FSC tipi YM'ye birebir taşındı (zincir kopmadı).

## 4. Mamul Üretimi (iş emri)
- **Ekran:** Üretim → **İş Emirleri** (`/Production/Index`) → IE2026-002 → **Detay**
- **Kontrol:** Plan/Gerçek adet · BOM 3 bileşen tüketimi · sapma.
- **Beklenen (gerçek):** 30443 · Plan 115.600 · **Gerçek 27.600** · Tamamlandı.
  BOM tüketim: 23357 = 1.363,22 · 20017 = 165,60 · 21036 = 46,92 kg.

## 5. Stok Hareketleri (giriş + çıkış)
- **Ekran:** Stok → **Stok Hareketleri** (`/Stock/Movements`)
- **Beklenen (gerçek):**
  - 4 × Satın Alma (giriş) = **+2.600 kg**
  - 3 × Üretim girişi/YM (giriş) = **+1.680 kg**
  - 7 × Üretim Tüketimi (çıkış) = **−3.492,74 kg**
  - Toplam Giriş 4.280 · Toplam Çıkış 3.492,74.

## 6. Stok Durumu (net)
- **Ekran:** Stok → **Stok Durumu** (`/Stock/Index`)
- **Beklenen (gerçek):** 10267 Net **500** · 23357 Net **86,78** (= bobin kalanı) · 20017 Net 14,40 · 21036 Net 3,08.
- **⚠️ EKSİK:** 30443 mamulü burada **görünmez** — üretim mamulü stoğa YAZMIYOR (0 hareket). Aşağıdaki bulguya bak.

## 7. Fire (denetim için kritik)
- **Ekran:** Üretim → **İmha Kayıtları** (`/Production/WasteManagement`) = **tüm fire 65 kg** (57 dönüşüm + 8 üretim).
- **Ekran:** Üretim → **Fire Raporu** (`/Production/WasteReport`) = sadece **üretim fire analizi** 8 kg (makine/ürün/trend).

## 8. Mamul → Satış Çıkışı  *(YAPILACAK adım)*
- **Ekran:** Satış / Sevkiyat (`/Sales/Index`) → sipariş oluştur → kalem ekle (30443) → **Sevk Et**.
- **Beklenen:** Sevkte SalesDispatch (çıkış) hareketi oluşur; durum **Teslim Edildi**; irsaliye/fatura yüklenebilir.
- **⚠️ BULGU:** Mamul üretimde stoğa girmediği için (Adım 6), satış çıkışı **negatif net** yaratır. FSC dengesi için mamulün üretimde stoğa girmesi gerekir.
- **FSC noktası:** Müşterinin FSC lisansı geçerli mi → şu an tüm müşteriler "FSC Pasif/yok"; gerçek FSC satışında lisans doğrulanmalı.

## 9. FSC İzlenebilirlik Raporları
- **Ekran:** FSC Raporlar → **Chain of Custody · Lot Takip · Hammadde İzleme · Tedarikçi/Müşteri FSC · Denetim Özeti**
- **Kontrol:** Ham → YM → mamul zinciri, FSC tipleri, mass balance bütünlüğü.

---

## 🔴 Tespit edilen tek model eksiği
**Mamul üretimi stok hareketi (ProductionEntry) oluşturmuyor.** Bu yüzden:
- Stok Durumu'nda mamul görünmez (Adım 6).
- Satış çıkışı yapılırsa net stok negatife düşer (Adım 8).
- FSC mass-balance (üretilen mamul = sevk edilen mamul) tam kapanmaz.

**Düzeltme önerisi:** İş emri "Tamamla"da mamul için bir ProductionEntry (adet) oluştur → mamul stoğa girer, satışta çıkar, denge kapanır.
