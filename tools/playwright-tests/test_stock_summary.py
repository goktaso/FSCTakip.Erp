from playwright.sync_api import sync_playwright
import time

BASE = "https://localhost:44357"

def login(page):
    page.goto(BASE + "/Account/Login", wait_until="networkidle")
    page.fill('input[name="username"]', "admin")
    page.fill('input[name="password"]', "admin123")
    page.click('button[type="submit"]')
    page.wait_for_load_state("networkidle")

def get_card_text(page, card_id):
    return page.locator(f"#{card_id}").inner_text().strip()

with sync_playwright() as p:
    browser = p.chromium.launch(headless=True)
    ctx = browser.new_context(ignore_https_errors=True)
    page = ctx.new_page()

    print("1. Login...")
    login(page)

    print("2. Stok Özeti sayfasına git...")
    page.goto(BASE + "/Stock/Summary", wait_until="networkidle")
    page.wait_for_timeout(1500)

    # Başlangıç değerleri
    initial_products = get_card_text(page, "cardProducts")
    initial_kg       = get_card_text(page, "cardTotalKg")
    initial_lots     = get_card_text(page, "cardLots")
    initial_serials  = get_card_text(page, "cardSerials")
    initial_header   = get_card_text(page, "headerTotalKg")

    print(f"   Başlangıç — Ürün: {initial_products}, KG: {initial_kg}, Lot: {initial_lots}, Bobin: {initial_serials}")
    print(f"   Başlık Toplam KG: {initial_header}")

    # --- TEST 1: Topbar globalSearch ile arama ---
    print("\n3. TEST 1 — Topbar globalSearch ile arama...")
    search = page.locator("#globalSearch")
    search.fill("BURGU")
    page.wait_for_timeout(800)

    filtered_products = get_card_text(page, "cardProducts")
    filtered_kg       = get_card_text(page, "cardTotalKg")
    filtered_header   = get_card_text(page, "headerTotalKg")

    print(f"   'BURGU' sonrası — Ürün: {filtered_products}, KG: {filtered_kg}")
    print(f"   Başlık Toplam KG: {filtered_header}")

    ok1 = filtered_products != initial_products or filtered_kg != initial_kg
    ok1_header = filtered_header == filtered_kg.replace(" KG", "").strip() or filtered_header.replace(" KG","") == filtered_kg.replace(" KG","")
    print(f"   Kartlar güncellendi mi? {'PASS' if ok1 else 'FAIL'}")
    print(f"   Başlık == Kart KG? {'PASS' if filtered_header.replace(' ','') in filtered_kg.replace(' ','') or filtered_kg.replace(' ','') in filtered_header.replace(' ','') else 'KONTROL ET'}")

    page.screenshot(path="/tmp/summary_search.png", full_page=True)

    # Aramayı temizle
    search.fill("")
    page.wait_for_timeout(600)

    # --- TEST 2: DataTables kendi arama kutusu ---
    print("\n4. TEST 2 — DataTables ara kutusu...")
    dt_search = page.locator("input[type='search']").first
    dt_search.fill("KRAFT")
    page.wait_for_timeout(800)

    dt_products = get_card_text(page, "cardProducts")
    dt_kg       = get_card_text(page, "cardTotalKg")
    dt_header   = get_card_text(page, "headerTotalKg")
    print(f"   'KRAFT' sonrası — Ürün: {dt_products}, KG: {dt_kg}")
    print(f"   Başlık Toplam KG: {dt_header}")
    ok2 = dt_products != initial_products or dt_kg != initial_kg
    print(f"   Kartlar güncellendi mi? {'PASS' if ok2 else 'FAIL'}")

    page.screenshot(path="/tmp/summary_dt_search.png", full_page=True)
    dt_search.fill("")
    page.wait_for_timeout(600)

    # --- TEST 3: Ürün Grubu filtresi (form submit) ---
    print("\n5. TEST 3 - Urun Grubu checkbox filtresi...")
    # Önce tüm kutucukları kaldır, sadece birini bırak
    checkboxes = page.locator('#groupChecks input[type="checkbox"]').all()
    print(f"   Toplam grup: {len(checkboxes)}")
    if len(checkboxes) > 1:
        # Tüm grupları kapat, ilkini aç
        for cb in checkboxes:
            if cb.is_checked():
                page.locator(f"label.grp-pill:has(input[value='{cb.get_attribute('value')}'])").click()
                page.wait_for_timeout(100)
        first_cb = checkboxes[0]
        if not first_cb.is_checked():
            page.locator(f"label.grp-pill:has(input[value='{first_cb.get_attribute('value')}'])").click()
            page.wait_for_timeout(100)
        page.locator("button[type='submit']").click()
        page.wait_for_load_state("networkidle")
        page.wait_for_timeout(800)

        grp_products = get_card_text(page, "cardProducts")
        grp_kg       = get_card_text(page, "cardTotalKg")
        grp_header   = get_card_text(page, "headerTotalKg")
        print(f"   Tek grup sonrası — Ürün: {grp_products}, KG: {grp_kg}")
        print(f"   Başlık Toplam KG: {grp_header}")
        ok3 = grp_kg == grp_header.replace(" KG","").strip() or grp_products != initial_products
        print(f"   Grup filtresi çalışıyor mu? {'PASS' if ok3 else 'KONTROL ET'}")
        page.screenshot(path="/tmp/summary_group.png", full_page=True)

    print("\n=== ÖZET ===")
    print(f"  Topbar arama kart güncellemesi: {'PASS' if ok1 else 'FAIL'}")
    print(f"  DT arama kart güncellemesi:     {'PASS' if ok2 else 'FAIL'}")
    print(f"  Ekran görüntüleri: /tmp/summary_search.png, /tmp/summary_dt_search.png")

    browser.close()
