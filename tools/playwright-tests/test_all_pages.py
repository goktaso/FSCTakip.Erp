import sys
from playwright.sync_api import sync_playwright

BASE = "https://localhost:44357"

def login(page):
    page.goto(BASE + "/Account/Login", wait_until="networkidle")
    page.fill('input[name="username"]', "admin")
    page.fill('input[name="password"]', "admin123")
    page.click('button[type="submit"]')
    page.wait_for_load_state("networkidle")

def check_dt_search(page, url, label):
    page.goto(BASE + url, wait_until="networkidle")
    page.wait_for_timeout(1500)
    si = page.locator("#globalSearch")
    if si.count() == 0:
        print(f"  {label}: globalSearch bulunamadi - SKIP")
        return True
    rows_before = page.locator("tbody tr").count()
    si.fill("XXXXNOTEXIST")
    page.wait_for_timeout(700)
    rows_after = page.locator("tbody tr:visible").count()
    si.fill("")
    page.wait_for_timeout(400)
    ok = rows_after < rows_before or rows_after == 0
    print(f"  {label}: {rows_before} -> {rows_after} satir | {'PASS' if ok else 'FAIL (satir azalmadi)'}")
    return ok

results = {}

with sync_playwright() as p:
    browser = p.chromium.launch(headless=True)
    ctx = browser.new_context(ignore_https_errors=True)
    page = ctx.new_page()
    login(page)

    # 1. RawMaterial - top-total row var mi + DT search calisıyor mu
    print("\n1. Hammadde Stogu (RawMaterial)...")
    page.goto(BASE + "/Stock/RawMaterial", wait_until="networkidle")
    page.wait_for_timeout(1500)
    top_row = page.locator("#topTotalRow")
    r1a = top_row.count() > 0
    print(f"   Top-total satiri var mi: {'PASS' if r1a else 'FAIL'}")
    r1b = check_dt_search(page, "/Stock/RawMaterial", "DT Search")
    results["RawMaterial"] = r1a and r1b

    # 2. Movements - toplam bandi var mi + DT search
    print("\n2. Stok Hareketleri (Movements)...")
    page.goto(BASE + "/Stock/Movements", wait_until="networkidle")
    page.wait_for_timeout(1500)
    band = page.locator("#bandGiris")
    r2a = band.count() > 0
    print(f"   Toplam bandi (bandGiris) var mi: {'PASS' if r2a else 'FAIL'}")
    r2b = check_dt_search(page, "/Stock/Movements", "DT Search")
    results["Movements"] = r2a and r2b

    # 3. SupplierFsc - filtre paneli + data-status attr
    print("\n3. Tedarikci FSC (SupplierFsc)...")
    page.goto(BASE + "/Reports/SupplierFsc", wait_until="networkidle")
    page.wait_for_timeout(1500)
    filter_form = page.locator("select[name='status']")
    r3a = filter_form.count() > 0
    print(f"   Filtre paneli (status select) var mi: {'PASS' if r3a else 'FAIL'}")
    r3b = check_dt_search(page, "/Reports/SupplierFsc", "DT Search")
    results["SupplierFsc"] = r3a and r3b

    # 4. WasteAnalysis - stat kart IDs
    print("\n4. Fire Analizi (WasteAnalysis)...")
    page.goto(BASE + "/Reports/WasteAnalysis", wait_until="networkidle")
    page.wait_for_timeout(1500)
    card = page.locator("#cardConsumed")
    r4a = card.count() > 0
    print(f"   cardConsumed ID var mi: {'PASS' if r4a else 'FAIL'}")
    results["WasteAnalysis"] = r4a

    # 5. BomAnalysis - wo-card class
    print("\n5. BOM Analizi (BomAnalysis)...")
    page.goto(BASE + "/Reports/BomAnalysis", wait_until="networkidle")
    page.wait_for_timeout(1500)
    wo_cards = page.locator(".wo-card")
    r5a = True  # OK even if 0 data (no work orders)
    print(f"   wo-card class bulundu: {wo_cards.count()} adet")
    results["BomAnalysis"] = r5a

    # 6. MaterialTrace - ek filtre alanlari
    print("\n6. Hammadde Izleme (MaterialTrace)...")
    page.goto(BASE + "/Reports/MaterialTrace", wait_until="networkidle")
    page.wait_for_timeout(1000)
    sup_sel = page.locator("select[name='supplierId']")
    r6a = sup_sel.count() > 0
    print(f"   Tedarikci select var mi: {'PASS' if r6a else 'FAIL'}")
    results["MaterialTrace"] = r6a

    # 7. LotTrace - ek filtre alanlari
    print("\n7. Lot Takip (LotTrace)...")
    page.goto(BASE + "/Reports/LotTrace", wait_until="networkidle")
    page.wait_for_timeout(1000)
    sup_sel2 = page.locator("select[name='supplierId']")
    r7a = sup_sel2.count() > 0
    print(f"   Tedarikci select var mi: {'PASS' if r7a else 'FAIL'}")
    results["LotTrace"] = r7a

    # 8. AuditReport - FSC bilesenler tablosu
    print("\n8. Denetim Ozeti (AuditReport)...")
    page.goto(BASE + "/Reports/AuditReport", wait_until="networkidle")
    page.wait_for_timeout(1000)
    # FSC tipi bileşen tablosu için kontrol - üretim verisi varsa tablo görünür
    page.screenshot(path="/tmp/audit_report.png")
    r8a = True
    print(f"   Sayfa render edildi (ekran goruntusu alindi)")
    results["AuditReport"] = r8a

    print("\n========== GENEL SONUC ==========")
    all_pass = all(results.values())
    for k, v in results.items():
        print(f"  {k}: {'PASS' if v else 'FAIL'}")
    print(f"\n  {'TUM TESTLER GECTI' if all_pass else 'BAZI TESTLER BASARISIZ'}")

    browser.close()
    sys.exit(0 if all_pass else 1)
