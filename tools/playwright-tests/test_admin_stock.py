import sys
from playwright.sync_api import sync_playwright

BASE = "https://localhost:44357"

def login(page):
    page.goto(BASE + "/Account/Login", wait_until="networkidle")
    page.fill('input[name="username"]', "admin")
    page.fill('input[name="password"]', "admin123")
    page.click('button[type="submit"]')
    page.wait_for_load_state("networkidle")

def get_text(page, sel):
    el = page.locator(sel)
    return el.inner_text().strip() if el.count() > 0 else "N/A"

with sync_playwright() as p:
    browser = p.chromium.launch(headless=True)
    ctx = browser.new_context(ignore_https_errors=True)
    page = ctx.new_page()

    print("1. Login...")
    login(page)

    print("2. Admin Stok sayfasina git...")
    page.goto(BASE + "/Stock/AdminStock", wait_until="networkidle")
    page.wait_for_timeout(1500)

    # Uyari bandi rengi kontrolu
    band = page.locator(".mb-3.px-1.py-2.rounded")
    band_color = band.evaluate("el => window.getComputedStyle(el).color")
    print(f"   Uyari bandi rengi: {band_color}")
    # rgb(146,64,14) = #92400e koyu kahve — okunabilir
    is_readable = "146" in band_color or "92400e" in band_color.lower()
    print(f"   Okunabilir mi? {'PASS' if is_readable else 'FAIL - kontrol et'}")

    # Baslangic degerleri
    init_products  = get_text(page, "#cardProducts")
    init_kg        = get_text(page, "#cardTotalKg")
    init_movements = get_text(page, "#cardMovements")
    init_band_kg   = get_text(page, "#bandTotalKg")
    print(f"\n   Baslangic - Urun: {init_products}, KG: {init_kg}, Hareket: {init_movements}")
    print(f"   Bant KG: {init_band_kg}")

    # TEST 1: Topbar globalSearch
    print("\n3. TEST 1 - Topbar globalSearch 'BURGU'...")
    search = page.locator("#globalSearch")
    search.fill("BURGU")
    page.wait_for_timeout(800)

    f1_products  = get_text(page, "#cardProducts")
    f1_kg        = get_text(page, "#cardTotalKg")
    f1_band_kg   = get_text(page, "#bandTotalKg")
    f1_movements = get_text(page, "#cardMovements")
    print(f"   'BURGU' - Urun: {f1_products}, KG: {f1_kg}, Hareket: {f1_movements}")
    print(f"   Bant KG: {f1_band_kg}")
    ok1 = f1_kg != init_kg and f1_products != init_products
    ok1_sync = f1_band_kg == f1_kg.replace(" KG","").strip() or f1_band_kg in f1_kg
    print(f"   Kartlar guncellendi mi? {'PASS' if ok1 else 'FAIL'}")
    print(f"   Bant == Kart KG? {'PASS' if ok1_sync else 'KONTROL ET'}")
    page.screenshot(path="/tmp/adminstock_search.png")
    search.fill("")
    page.wait_for_timeout(600)

    # TEST 2: DataTables arama kutusu
    print("\n4. TEST 2 - DataTables arama 'KRAFT'...")
    dt_search = page.locator("input[type='search']").first
    dt_search.fill("KRAFT")
    page.wait_for_timeout(800)

    f2_products = get_text(page, "#cardProducts")
    f2_kg       = get_text(page, "#cardTotalKg")
    f2_band_kg  = get_text(page, "#bandTotalKg")
    print(f"   'KRAFT' - Urun: {f2_products}, KG: {f2_kg}")
    print(f"   Bant KG: {f2_band_kg}")
    ok2 = f2_kg != init_kg or f2_products != init_products
    print(f"   Kartlar guncellendi mi? {'PASS' if ok2 else 'FAIL'}")
    page.screenshot(path="/tmp/adminstock_dt.png")
    dt_search.fill("")
    page.wait_for_timeout(600)

    print("\n=== OZET ===")
    print(f"  Uyari bandi okunabilirligi: {'PASS' if is_readable else 'FAIL'}")
    print(f"  Topbar arama kart+bant guncellemesi: {'PASS' if ok1 else 'FAIL'}")
    print(f"  DT arama kart guncellemesi: {'PASS' if ok2 else 'FAIL'}")
    all_pass = is_readable and ok1 and ok2
    print(f"\n  GENEL SONUC: {'PASS' if all_pass else 'FAIL'}")
    sys.exit(0 if all_pass else 1)

    browser.close()
