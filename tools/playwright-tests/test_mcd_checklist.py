import sys
from playwright.sync_api import sync_playwright

BASE = "https://localhost:44357"

def login(page):
    page.goto(BASE + "/Account/Login", wait_until="networkidle")
    page.fill('input[name="username"]', "admin")
    page.fill('input[name="password"]', "admin123")
    page.click('button[type="submit"]')
    page.wait_for_load_state("networkidle")

results = {}

def chk(name, ok, detail=""):
    results[name] = ok
    print(f"  [{'PASS' if ok else 'FAIL'}] {name}" + (f" — {detail}" if detail else ""))

with sync_playwright() as p:
    browser = p.chromium.launch(headless=True)
    ctx = browser.new_context(ignore_https_errors=True)
    page = ctx.new_page()
    login(page)

    # 1. Conversion — iç stok kodu + kaynak search + hedef ext kod
    print("\n1. Yarı Mamül Dönüşüm — Stok Kodu (NOT: .cs degisikligi build gerektirir)")
    page.goto(BASE + "/Conversion/Index", wait_until="networkidle")
    page.wait_for_timeout(1500)
    page_ok = page.locator("#sourceSerialId").count() > 0 or page.locator("#sourceSearch").count() > 0
    if not page_ok:
        # Sayfa 500 / compile error — build bekleniyor
        chk("Conv.sourceSearch", False, "BUILD GEREKLI — Ctrl+Shift+B + IIS restart")
        chk("Conv.icKodInSource", False, "BUILD GEREKLI")
        chk("Conv.targetRenders", False, "BUILD GEREKLI")
    else:
        chk("Conv.sourceSearch", page.locator("#sourceSearch").count() > 0, "kaynak arama kutusu")
        opts = page.locator("#sourceSerialId option").all_text_contents()
        has_ic_kod = any("[" in o for o in opts if o.strip())
        chk("Conv.icKodInSource", has_ic_kod, f"ilk option: {opts[1][:60] if len(opts)>1 else 'yok'}")
        target_opts = page.locator("#targetProductId option").all_text_contents()
        chk("Conv.targetRenders", len(target_opts) > 0, f"{len(target_opts)} hedef ürün")

    # 2. Movements — MCD dropdown
    print("\n2. Stok Hareketleri — MCD Ürün")
    page.goto(BASE + "/Stock/Movements", wait_until="networkidle")
    page.wait_for_timeout(1500)
    chk("Mov.pageLoads", page.locator("table.data-table").count() > 0, "tablo var")
    mcd = page.locator("#mcd-products")
    chk("Mov.mcdExists", mcd.count() > 0, "MCD div var")
    # MCD aç
    mcd_btn = page.locator("#mcd-products .mcd-btn")
    if mcd_btn.count() > 0:
        mcd_btn.click()
        page.wait_for_timeout(400)
        panel = page.locator("#mcd-products-panel")
        chk("Mov.mcdPanelOpens", panel.is_visible(), "panel açılıyor")
        # Arama kutusu var mi
        chk("Mov.mcdSearch", panel.locator(".mcd-search").count() > 0, "arama kutusu")
        # Tümünü seç checkbox
        chk("Mov.mcdSelectAll", panel.locator(".mcd-all-cb").count() > 0, "tümünü seç")
        # Item checkbox'ları
        items = panel.locator(".mcd-cb").count()
        chk("Mov.mcdItems", items > 0, f"{items} ürün var")
        # ProductCode/ExternalCode badge'leri
        sub_badges = panel.locator(".mcd-sub").count()
        chk("Mov.mcdSubBadge", sub_badges > 0, f"{sub_badges} stok kodu badge")
        # Kapat
        page.keyboard.press("Escape")
        page.wait_for_timeout(200)
    else:
        chk("Mov.mcdPanelOpens", False, "btn bulunamadi")
        chk("Mov.mcdSearch", False)
        chk("Mov.mcdSelectAll", False)
        chk("Mov.mcdItems", False)
        chk("Mov.mcdSubBadge", False)
    # Supplier filter
    chk("Mov.supplierFilter", page.locator("select[name='supplierId']").count() > 0)
    # Toplam bant
    chk("Mov.totalBand", page.locator("#bandGiris").count() > 0)

    # 3. Chain of Custody — MCD ürün
    print("\n3. Chain of Custody — MCD Ürün")
    page.goto(BASE + "/Reports/ChainOfCustody", wait_until="networkidle")
    page.wait_for_timeout(1200)
    chk("CoC.pageLoads", page.locator("table.data-table").count() > 0)
    mcd_coc = page.locator("#mcd-coc-prod")
    chk("CoC.mcdExists", mcd_coc.count() > 0, "MCD div var")
    if mcd_coc.count() > 0:
        # Filtre paneli gizli olabilir — JS ile göster
        page.evaluate("document.getElementById('filterPanel').style.display = ''")
        page.wait_for_timeout(300)
        mcd_coc.locator(".mcd-btn").click()
        page.wait_for_timeout(400)
        coc_panel = page.locator("#mcd-coc-prod-panel")
        chk("CoC.mcdPanelOpens", coc_panel.is_visible())
        coc_items = coc_panel.locator(".mcd-cb").count()
        chk("CoC.mcdItems", coc_items > 0, f"{coc_items} ürün")
        page.keyboard.press("Escape")
        page.wait_for_timeout(200)
    else:
        chk("CoC.mcdPanelOpens", False)
        chk("CoC.mcdItems", False)
    chk("CoC.statCards", page.locator("#cardTotal").count() > 0)

    # 4. Lot Takip — MCD ürün multi-select
    print("\n4. Lot Takip — MCD Ürün")
    page.goto(BASE + "/Reports/LotTrace", wait_until="networkidle")
    page.wait_for_timeout(1200)
    mcd_lot = page.locator("#mcd-lot-prod")
    chk("Lot.mcdExists", mcd_lot.count() > 0, "MCD div var")
    if mcd_lot.count() > 0:
        mcd_lot.locator(".mcd-btn").click()
        page.wait_for_timeout(400)
        lot_panel = page.locator("#mcd-lot-prod-panel")
        chk("Lot.mcdPanelOpens", lot_panel.is_visible())
        lot_items = lot_panel.locator(".mcd-cb").count()
        chk("Lot.mcdItems", lot_items > 0, f"{lot_items} ürün")
        page.keyboard.press("Escape")
        page.wait_for_timeout(200)
    else:
        chk("Lot.mcdPanelOpens", False)
        chk("Lot.mcdItems", False)
    # Text filter alanları
    chk("Lot.productCode", page.locator("input[name='productCode']").count() > 0)

    print("\n" + "="*55)
    passed = sum(1 for v in results.values() if v)
    total  = len(results)
    print(f"TOPLAM: {passed}/{total} PASS")
    all_ok = all(results.values())
    print("SONUC: " + ("TUM TESTLER GECTI" if all_ok else "BAZI TESTLER BASARISIZ"))
    for k, v in results.items():
        if not v:
            print(f"  FAIL: {k}")

    browser.close()
    sys.exit(0 if all_ok else 1)
