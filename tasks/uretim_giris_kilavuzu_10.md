# Üretim Giriş Kılavuzu — 10 Mamul (Netsis gerçek verisi)

> Her mamul için **aynı 5 adımı** tekrarla. Değerler Netsis ACORE23'ten birebir alınmıştır.
> Makine her yerde **F6** (istersen değiştir). Müşteri hepsinde **ACORE DIŞ TİC.**

## Tekrarlanan 5 Adım

1. **Hammadde Girişi** (`Satın Alma / Hammadde Girişi → + Yeni Hammadde Girişi`)
   - Parti No (aşağıdaki), Tedarikçi = herhangi aktif (örn. KMK PAPER), FSC Tipi = `FSC MIX_CREDIT`
     *(ürün adında RECYCLED varsa `FSC RECYCLED 100%`)*, Geliş Tarihi = üretim tarihi
   - Hammadde Ürünü = **ana bileşen kodu**, Bobin Sayısı = 1, Birim Ağırlık = aşağıdaki **bobin kg**
2. **İş Emri** (`Üretim → + Yeni Üretim`)
   - Ürün = mamul, Makine = F6, Plan Tarihi = üretim tarihi, **Plan Adet = Sipariş Mik.**, **ERP İş Emri No = Fiş**
3. **Reçeteden BOM Yükle** (İş emri **Detay** → `Reçeteden Yükle` → `BOM'a Ekle`)
   - Tüm bileşenler planlananıyla otomatik gelir (Planlanan = std × Plan Adet)
4. **Tüketim Gir** (`+ Tüketim Gir`)
   - Bobin = az önce açtığın bobin · Üretim Tarihi · **FSC CoC İzleme aç → Reçete Bileşeni = ana bileşen**
   - **Üretilen Miktar = Üretim Mik.** → Tüketilen (kg) otomatik dolar · Fire = 0 · **Kaydet**
5. **Üretimi Tamamla** (`Tamamla` butonu) → Gerçekleşen adet işlenir.

> **Not:** Her mamulün ana (gövde) bileşeni stoğa alınır ve tüketilir. Sap/etiket/burgu sap bileşenleri **opsiyonel** — istersen aynı yöntemle ek bobin + ek tüketim kaydı gir (alt kısımdaki "diğer bileşenler" değerleriyle).

---

## 1) 30007 — BELBAKE PLAIN GB FLOUR
- İş Emri: Plan Adet **250.000** · Plan/Üretim Tarihi **27.09.2024** · ERP No **230000200005194**
- Hammadde (ana): **23006** BB Belbake Plain · Parti `L30007-001` · Bobin **150 kg**
- Tüketim: Üretilen **9.000** adet → ana tüketim **≈107,1 kg**

## 2) 30163 — ALDİ ESSENTIALS PLAIN 1,5 KG
- İş Emri: Plan Adet **300.000** · Tarih **15.05.2024** · ERP No **230000000004118**
- Hammadde (ana): **23156** BB Aldi Essentials Plain · Parti `L30163-001` · Bobin **450 kg**
- Tüketim: Üretilen **33.000** adet → ana tüketim **≈391,05 kg**

## 3) 30164 — ALDİ ESSENTIALS SELF RAISING
- İş Emri: Plan Adet **300.000** · Tarih **13.05.2024** · ERP No **230000000004078**
- Hammadde (ana): **23157** BB Aldi Essentials Self Raising · Parti `L30164-001` · Bobin **350 kg**
- Tüketim: Üretilen **25.000** adet → ana tüketim **≈296,25 kg**

## 4) 30187 — TAKEAWAY KRAFT WIDE BOTTOM BROWN
- İş Emri: Plan Adet **250.000** · Tarih **23.07.2024** · ERP No **230000000004611**
- Hammadde (ana): **10148** VBMF080 1080 N · Parti `L30187-001` · Bobin **500 kg**
- Tüketim: Üretilen **10.500** adet → ana tüketim **≈457,8 kg**
- Diğer bileşenler (ops.): 20002 Sap ≈42 kg · 21004 Etiket ≈20,16 kg

## 5) 30223 — HOBBYCRAFT WHITE PAPER CARRIER
- İş Emri: Plan Adet **1.283.000** · Tarih **12.08.2024** · ERP No **230000000004781**
- Hammadde (ana): **23182** BB Hobbycraft White · Parti `L30223-001` · Bobin **350 kg**
- Tüketim: Üretilen **8.500** adet → ana tüketim **≈306 kg**
- Diğer (ops.): 21015 Etiket ≈22,95 kg · 40112 Burgu Sap ≈6.800 **metre** (birim MT — istersen atla)

## 6) 30443 — EDEKA WLL FLAT HANDLE BAG
- İş Emri: Plan Adet **115.600** · Tarih **18.11.2024** · ERP No **230000200005582**
- Hammadde (ana): **23357** BB Edeka WLL Flat Handle · Parti `L30443-001` · Bobin **1.450 kg**
- Tüketim: Üretilen **27.600** adet → ana tüketim **≈1.363,22 kg**
- Diğer (ops.): 20017 Sap ≈165,6 kg · 21036 Etiket ≈46,92 kg

## 7) 30444 — EDEKA BROWN 80GSM RECYCLED
- İş Emri: Plan Adet **350.000** · Tarih **13.12.2024** · ERP No **230000200005776**
- Hammadde (ana): **23358** BB Edeka Brown 80gsm · Parti `L30444-001` · Bobin **600 kg** · FSC RECYCLED 100%
- Tüketim: Üretilen **15.250** adet → ana tüketim **≈536,8 kg**
- Diğer (ops.): 20001 Sap ≈68,6 kg · 21032 Etiket ≈27,45 kg

## 8) 30463 — EDEKA WLL2S/6C FLAT HANDLE BAG
- İş Emri: Plan Adet **280.000** · Tarih **11.02.2025** · ERP No **230000200005979**
- Hammadde (ana): **23377** BB Edeka WLL2s/6c · Parti `L30463-001` · Bobin **1.450 kg**
- Tüketim: Üretilen **27.600** adet → ana tüketim **≈1.363,22 kg**
- Diğer (ops.): 20017 Sap ≈165,6 kg · 21036 Etiket ≈46,92 kg

## 9) 30465 — FLAT HANDLE UNPRINTED BROWN BAG
- İş Emri: Plan Adet **525.000** · Tarih **02.05.2025** · ERP No **230000200006255**
- Hammadde (ana): **10048** Alier RBMF080 790 N · Parti `L30465-001` · Bobin **350 kg** · FSC RECYCLED 100%
- Tüketim: Üretilen **11.750** adet → ana tüketim **≈293,75 kg**
- Diğer (ops.): 20002 Sap ≈47 kg · 21006 Etiket ≈23,5 kg

## 10) 30469 — TASTE THE JOY FLAT HANDLE BROWN
- İş Emri: Plan Adet **1.032.750** · Tarih **17.04.2025** · ERP No **230000200006212**
- Hammadde (ana): **23380** BB Taste the Joy · Parti `L30469-001` · Bobin **250 kg**
- Tüketim: Üretilen **8.250** adet → ana tüketim **≈206,25 kg**
- Diğer (ops.): 20008 Sap ≈33 kg · 21006 Etiket ≈16,5 kg

---

## 11) 30186 — WENZEL'S LARGE FLAT HANDLE BAG
- İş Emri: Plan Adet **150.000** · Tarih **05.08.2024** · ERP No **230000000004712**
- Hammadde (ana): **23167** BB Wenzel's Large Flat Handle · Parti `L30186-001` · Bobin **1.350 kg**
- Tüketim: Üretilen **30.400** adet → ana tüketim **≈1.269,5 kg**
- Diğer (ops.): 20005 Sap ≈121,6 kg · 21024 Etiket ≈57,2 kg

## 12) 30208 — QP-INTERTAN FLAT HANDLE BROWN
- İş Emri: Plan Adet **375.000** · Tarih **25.03.2024** · ERP No **230000000003724**
- Hammadde (ana): **24038** YM VB070 1080 · Parti `L30208-001` · Bobin **850 kg**
- Tüketim: Üretilen **23.250** adet → ana tüketim **≈808,54 kg**
- Diğer (ops.): 20002 Sap ≈105,79 kg · 21004 Etiket ≈34,88 kg

## 13) 30322 — P&G WHISTL BAG WHITE 80GSM
- İş Emri: Plan Adet **1.200.000** · Tarih **26.08.2024** · ERP No **230000000004898**
- Hammadde (ana): **23260** BB P&G Whistl White · Parti `L30322-001` · Bobin **700 kg**
- Tüketim: Üretilen **37.000** adet → ana tüketim **≈625,3 kg**

## 14) 30439 — WASABI PRINTED PAPER BAG
- İş Emri: Plan Adet **1.125.000** · Tarih **16.09.2024** · ERP No **230000000005086**
- Hammadde (ana): **23353** BB Wasabi Printed · Parti `L30439-001` · Bobin **400 kg**
- Tüketim: Üretilen **16.500** adet → ana tüketim **≈365,9 kg**
- Diğer (ops.): 20002 Sap ≈66 kg · 21006 Etiket ≈24,75 kg

## 15) 30471 — FISH AND CHIPS TWISTED HANDLE
- İş Emri: Plan Adet **672.200** · Tarih **30.04.2025** · ERP No **230000200006247**
- Hammadde (ana): **23382** BB Fish and Chips Twisted · Parti `L30471-001` · Bobin **350 kg**
- Tüketim: Üretilen **7.000** adet → ana tüketim **≈296,8 kg**
- Diğer (ops.): 21015 Etiket ≈21 kg · 40119 Burgu Sap ≈5.600 **metre** (birim MT — istersen atla)

---

### İpuçları
- Tüketilen (kg) **otomatik** dolar (Üretilen × Planlanan ÷ Plan Adet). Dolmazsa: önce **Reçete Bileşeni**'ni seç, sonra Üretilen'i yaz; ya da elle yukarıdaki kg'yi gir.
- Ondalık girişlerde nokta da virgül de çalışır (binder düzeltildi).
- Bobin kg'leri tüketimi rahat karşılayacak şekilde yukarı yuvarlandı; istersen değiştir.
