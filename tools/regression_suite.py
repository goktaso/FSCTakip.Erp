# -*- coding: utf-8 -*-
"""
FSC Takip ERP — Çoklu Şirket Regresyon Paketi
=============================================
Aynı kod tabanını farklı veritabanlarına (farklı şirket verisi) karşı doğrular.
Kullanım:
    python tools/regression_suite.py --base-url http://localhost:5210 --company TestCo_Seed [--out results.json]

Kontroller:
  P1  Login (admin/admin123)
  P2  Ana sayfa taraması — tüm modül sayfaları HTTP 200 + exception yok
  P3  Denetim Özeti (AuditReport) render + "Dengeli" rozeti (veri varsa)
  P4  Stok yeterlilik: üretilenden fazla sevkiyat REDDEDİLMELİ (kalıcı mimari kural)
  P5  Tam İzlenebilirlik: sipariş varsa sayfa render + zincir rozeti mevcut
  P6  Uyarı Paneli render + kritik özet endpoint'i (CriticalSummary) JSON döner
  P7  Print sayfaları: İş Emri Formu, Sevk, Fatura render
Çıktı: konsol PASS/FAIL matrisi + JSON dosyası (--out)
"""
import argparse, json, sys, time
from playwright.sync_api import sync_playwright

PAGES = [
    "/",  # dashboard
    "/Purchase/Index",
    "/Conversion/Index",
    "/Production/Index",
    "/Production/WasteReport",
    "/Production/WasteManagement",
    "/Sales/Index",
    "/Stock/Index",
    "/Stock/AnaOzet",
    "/Stock/RawMaterial",
    "/Stock/AdminStock",
    "/Stock/Movements",
    "/Reports/ChainOfCustody",
    "/Reports/LotTrace",
    "/Reports/SupplierFsc",
    "/Reports/AuditReport",
    "/Reports/Warnings",
    "/AuditPeriod",
    "/Customers/Index",
    "/Suppliers/Index",
    "/Products/Index",
    "/Machine/Machines",
    "/Etl/Index",
    "/Users",
]

ERROR_MARKERS = ["An unhandled exception", "DbUpdateException", "NullReferenceException",
                 "InvalidOperationException", "SqlException", "Stack Trace"]


def run(base_url: str, company: str):
    results = {"company": company, "base_url": base_url, "checks": {}, "timings_ms": {}}

    def rec(key, ok, detail=""):
        results["checks"][key] = {"pass": bool(ok), "detail": str(detail)[:300]}
        print(f"  [{'PASS' if ok else 'FAIL'}] {key}  {detail if not ok else ''}")

    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        ctx = browser.new_context(ignore_https_errors=True, viewport={"width": 1440, "height": 900})
        page = ctx.new_page()

        # ── P1 Login ──────────────────────────────────────────────────────
        page.goto(f"{base_url}/Account/Login", timeout=20000)
        page.wait_for_load_state("networkidle")
        page.fill('input[name="username"]', "admin")
        page.fill('input[name="password"]', "admin123")
        page.click('button[type="submit"], input[type="submit"]')
        page.wait_for_load_state("networkidle")
        rec("P1_login", "Login" not in page.url, f"url={page.url}")
        if "Login" in page.url:
            browser.close()
            return results

        token = None
        # CSRF token (P4 için)
        try:
            page.goto(f"{base_url}/Sales/Index")
            page.wait_for_load_state("networkidle")
            token = page.eval_on_selector('input[name="__RequestVerificationToken"]', "el => el.value")
        except Exception:
            pass

        # ── P2 Sayfa taraması ─────────────────────────────────────────────
        bad = []
        for url in PAGES:
            t0 = time.time()
            try:
                resp = page.goto(f"{base_url}{url}", timeout=25000)
                page.wait_for_load_state("networkidle", timeout=25000)
                ms = int((time.time() - t0) * 1000)
                results["timings_ms"][url] = ms
                status = resp.status if resp else 0
                body = page.content()
                err = next((m for m in ERROR_MARKERS if m in body), None)
                if status != 200 or err:
                    bad.append(f"{url} (HTTP {status}{', ' + err if err else ''})")
            except Exception as e:
                bad.append(f"{url} (EXC {str(e)[:60]})")
        rec("P2_page_sweep", not bad, "; ".join(bad))

        # ── P3 Denetim Özeti ─────────────────────────────────────────────
        page.goto(f"{base_url}/Reports/AuditReport")
        page.wait_for_load_state("networkidle")
        body = page.content()
        has_balance_badge = ("Dengeli" in body) or ("Denge Tamam" in body) or ("Kütle Dengesi" in body)
        rec("P3_audit_report", has_balance_badge, "kütle dengesi rozeti bulunamadı")

        # ── P4 Stok yeterlilik reddi ─────────────────────────────────────
        # Tamamlanmış bir iş emri bul, kalanından fazlasını sevk etmeyi dene.
        p4_ok, p4_detail = False, ""
        try:
            hdrs = {"RequestVerificationToken": token} if token else {}
            # müşteri listesi (ilk aktif müşteri)
            cust = ctx.request.get(f"{base_url}/Customers/Index")
            # sipariş oluştur — ilk müşteri Id'sini sayfadan çekmek yerine 1..20 dene
            order_id = None
            for cid in range(1, 21):
                r = ctx.request.post(f"{base_url}/Sales/SaveOrder", form={
                    "salesOrderId": "0", "customerId": str(cid),
                    "orderDate": "2026-07-03", "currency": "TRY", "status": "1",
                    "notes": "REGRESSION-P4"}, headers=hdrs)
                if r.ok:
                    try:
                        j = r.json()
                        if j.get("success"):
                            order_id = j["id"]; break
                    except Exception:
                        continue
            if order_id:
                # tamamlanmış WO bul (GetWorkOrder 1..50 tara)
                wo_id, prod_id = None, None
                for wid in range(1, 51):
                    r = ctx.request.get(f"{base_url}/Production/GetWorkOrder/{wid}")
                    if r.ok:
                        try:
                            j = r.json()
                            if j.get("success") and j["data"]["status"] == 3:
                                wo_id, prod_id = wid, j["data"]["productId"]; break
                        except Exception:
                            continue
                if wo_id:
                    ctx.request.post(f"{base_url}/Sales/SaveLine", form={
                        "lineId": "0", "salesOrderId": str(order_id),
                        "productId": str(prod_id), "workOrderId": str(wo_id),
                        "quantity": "99999999", "unitPrice": "0", "unit": "Adet"}, headers=hdrs)
                    r = ctx.request.post(f"{base_url}/Sales/Dispatch/{order_id}", form={
                        "dispatchDate": "2026-07-03", "dispatchNo": "REG-P4"}, headers=hdrs)
                    j = r.json()
                    reject = (not j.get("success")) and ("engellendi" in j.get("message", "").lower()
                              or "yetersiz" in j.get("message", "").lower()
                              or "fsc" in j.get("message", "").lower())
                    p4_ok, p4_detail = reject, j.get("message", "")
                else:
                    p4_ok, p4_detail = True, "SKIP: tamamlanmış iş emri yok (veri seti içermiyor)"
            else:
                p4_ok, p4_detail = False, "sipariş oluşturulamadı"
        except Exception as e:
            p4_detail = str(e)[:200]
        rec("P4_stock_guard", p4_ok, p4_detail)

        # ── P5 Tam İzlenebilirlik ─────────────────────────────────────────
        p5_ok, p5_detail = True, "SKIP: sipariş yok"
        for sid in range(1, 21):
            resp = page.goto(f"{base_url}/Reports/Traceability/{sid}", timeout=20000)
            if resp and resp.status == 200:
                page.wait_for_load_state("networkidle")
                body = page.content()
                p5_ok = ("FSC Zinciri" in body) or ("İzlenebilirlik" in body)
                p5_detail = f"sipariş {sid}: rozet {'var' if p5_ok else 'YOK'}"
                break
        rec("P5_traceability", p5_ok, p5_detail)

        # ── P6 Uyarı Paneli + kritik özet ────────────────────────────────
        r = ctx.request.get(f"{base_url}/Reports/CriticalSummary")
        p6a = r.ok
        try:
            j = r.json(); p6b = "count" in j
        except Exception:
            p6b = False
        rec("P6_warnings_critical", p6a and p6b, f"HTTP {r.status}")

        # ── P7 Print sayfaları ───────────────────────────────────────────
        p7_bad = []
        for wid in range(1, 51):
            resp = page.goto(f"{base_url}/Production/PrintForm?ids={wid}", timeout=20000)
            if resp and resp.status == 200:
                if "İş Emri Formu" not in page.content():
                    p7_bad.append("PrintForm içerik eksik")
                break
        for sid in range(1, 21):
            resp = page.goto(f"{base_url}/Sales/Print/{sid}", timeout=20000)
            if resp and resp.status == 200:
                break
        for sid in range(1, 21):
            resp = page.goto(f"{base_url}/Sales/PrintInvoice/{sid}", timeout=20000)
            if resp and resp.status == 200:
                if "Satış Faturası" not in page.content():
                    p7_bad.append("PrintInvoice içerik eksik")
                break
        rec("P7_print_pages", not p7_bad, "; ".join(p7_bad))

        browser.close()

    total = len(results["checks"])
    passed = sum(1 for c in results["checks"].values() if c["pass"])
    results["summary"] = f"{passed}/{total} PASS"
    print(f"\n== {company}: {results['summary']} ==")
    return results


if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument("--base-url", required=True)
    ap.add_argument("--company", required=True)
    ap.add_argument("--out")
    args = ap.parse_args()
    res = run(args.base_url, args.company)
    if args.out:
        with open(args.out, "w", encoding="utf-8") as f:
            json.dump(res, f, ensure_ascii=False, indent=2)
    sys.exit(0 if all(c["pass"] for c in res["checks"].values()) else 1)
