# -*- coding: utf-8 -*-
"""ARD markali test irsaliye + fatura PDF uretici (lot bazli). Edge headless ile."""
import os, subprocess, html

ROOT = r"C:\Users\User\Desktop\FSC_ERP_Blackboxai\FSCTakip.WebUI\wwwroot\uploads\test_belgeler"
HTMLDIR = os.path.join(ROOT, "_html")
os.makedirs(HTMLDIR, exist_ok=True)
EDGE = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"

SUPPLIER = "KMK PAPER KAHRAMANMARAŞ KAĞIT SAN. ve TİC. A.Ş."
SUP_ADDR = "Organize Sanayi Bölgesi 4. Cad. No:12 · Kahramanmaraş"
SUP_VKN  = "5710012345"
BUYER    = "ARD SİSTEM VE DANIŞMANLIK"
BUY_ADDR = "FSC Takip ERP · Kraft Kağıt İzlenebilirlik · Türkiye"
PRICE    = 32.00  # TL/kg

# parti, tarih(gg.aa.yyyy), urunKod, urunAd, kg
LOTS = [
 ("BB30006-001","14.10.2024","23005","BB Belbake SR GB Flour With Logo",250),
 ("L30007-001","27.09.2024","23006","BB Belbake Plain GB Flour With Logo",150),
 ("L30163-001","15.05.2024","23156","BB Aldi Essentials Plain 1,5 kg",450),
 ("L30164-001","13.05.2024","23157","BB Aldi Essentials Self Raising",350),
 ("L30187-001","23.07.2024","10148","VBMF080 1080 N",500),
 ("L30223-001","12.08.2024","23182","BB Hobbycraft White Paper Carrier",350),
 ("L30443-001","18.11.2024","23357","BB Edeka WLL Flat Handle Bag 320",1450),
 ("L30444-001","13.12.2024","23358","BB Edeka Brown 80gsm Recycled",600),
 ("L30463-001","11.02.2025","23377","BB Edeka WLL2s/6c Flat Handle",1450),
 ("L30465-001","02.05.2025","10048","Alier RBMF080 790 N",350),
 ("L30469-001","17.04.2025","23380","BB Taste the Joy Flat Handle",250),
 ("L30186-001","05.08.2024","23167","BB Wenzel's Large Flat Handle",1350),
 ("L30208-001","25.03.2024","24038","YM VB070 1080",850),
 ("L30322-001","26.08.2024","23260","BB P&G Whistl Bag White 80gsm",700),
 ("L30439-001","16.09.2024","23353","BB Wasabi Printed Paper Bag",400),
 ("L30471-001","30.04.2025","23382","BB Fish and Chips Twisted Handle",350),
]

CSS = """
<style>
@page { size: A4; margin: 16mm; }
* { font-family:'Segoe UI',Arial,sans-serif; box-sizing:border-box; }
body { color:#1a2035; font-size:13px; }
.top { display:flex; justify-content:space-between; align-items:flex-start;
  border-bottom:3px solid #1976d2; padding-bottom:14px; margin-bottom:18px; }
.brand { font-size:20px; font-weight:700; color:#0d47a1; }
.brand small { display:block; font-size:11px; font-weight:400; color:#5f6b7a; margin-top:2px; }
.doc { text-align:right; }
.doc .t { font-size:22px; font-weight:700; letter-spacing:.04em; color:#1976d2; }
.doc .n { font-size:13px; color:#444; margin-top:4px; }
.meta { display:flex; gap:16px; margin-bottom:18px; }
.box { flex:1; border:1px solid #e2e6ee; border-radius:8px; padding:11px 13px; }
.box h4 { margin:0 0 6px; font-size:11px; text-transform:uppercase; letter-spacing:.05em; color:#1976d2; }
.box .nm { font-weight:600; }
.box .sm { font-size:11.5px; color:#5f6b7a; margin-top:2px; }
table { width:100%; border-collapse:collapse; margin-top:6px; }
th { background:#1976d2; color:#fff; font-size:11px; text-transform:uppercase;
  letter-spacing:.03em; padding:8px 10px; text-align:left; }
td { padding:9px 10px; border-bottom:1px solid #eef1f6; }
.r { text-align:right; }
.tot { margin-top:14px; margin-left:auto; width:48%; }
.tot td { border:none; padding:4px 10px; }
.tot .g { font-weight:700; font-size:15px; color:#0d47a1; border-top:2px solid #1976d2; }
.fsc { margin-top:16px; background:#eef6ee; border:1px solid #cfe6cf; border-radius:8px;
  padding:9px 12px; font-size:11.5px; color:#1e5631; }
.foot { margin-top:26px; display:flex; justify-content:space-between; font-size:11px; color:#8a94a3; }
.sign { margin-top:34px; display:flex; justify-content:space-between; }
.sign div { width:42%; border-top:1px solid #aab3c0; padding-top:5px; font-size:11px; color:#5f6b7a; text-align:center; }
</style>
"""

def head(doc_title, doc_no):
    return f"""<div class="top">
      <div><div class="brand">{BUYER}<small>{BUY_ADDR}</small></div></div>
      <div class="doc"><div class="t">{doc_title}</div><div class="n">No: {doc_no}</div></div>
    </div>"""

def meta(tarih):
    return f"""<div class="meta">
      <div class="box"><h4>Tedarikçi (Satıcı)</h4><div class="nm">{SUPPLIER}</div>
        <div class="sm">{SUP_ADDR}</div><div class="sm">VKN: {SUP_VKN}</div></div>
      <div class="box"><h4>Alıcı</h4><div class="nm">{BUYER}</div>
        <div class="sm">{BUY_ADDR}</div><div class="sm">Tarih: {tarih}</div></div>
    </div>"""

def render_irsaliye(parti, tarih, kod, ad, kg, irs_no):
    ad = html.escape(ad)
    return f"""<!doctype html><html><head><meta charset="utf-8">{CSS}</head><body>
    {head("İRSALİYE", irs_no)}
    {meta(tarih)}
    <table><tr><th>Stok Kodu</th><th>Hammadde / Bobin</th><th>Parti No</th><th class="r">Miktar</th><th>Birim</th></tr>
    <tr><td>{kod}</td><td>{ad}</td><td>{parti}</td><td class="r">{kg:,.2f}</td><td>kg</td></tr></table>
    <div class="fsc">♻ FSC® Sertifikalı ürün — Chain of Custody kapsamında sevk edilmiştir. Parti: <b>{parti}</b></div>
    <div class="sign"><div>Teslim Eden</div><div>Teslim Alan</div></div>
    <div class="foot"><span>FSC Takip ERP · Test Belgesi</span><span>Sevk Tarihi: {tarih}</span></div>
    </body></html>"""

def render_fatura(parti, tarih, kod, ad, kg, fat_no):
    ad = html.escape(ad)
    ara = kg * PRICE
    kdv = ara * 0.20
    gen = ara + kdv
    return f"""<!doctype html><html><head><meta charset="utf-8">{CSS}</head><body>
    {head("FATURA", fat_no)}
    {meta(tarih)}
    <table><tr><th>Stok Kodu</th><th>Açıklama</th><th class="r">Miktar</th><th class="r">Birim Fiyat</th><th class="r">Tutar</th></tr>
    <tr><td>{kod}</td><td>{ad} (Parti: {parti})</td><td class="r">{kg:,.2f} kg</td>
        <td class="r">{PRICE:,.2f} ₺</td><td class="r">{ara:,.2f} ₺</td></tr></table>
    <table class="tot">
      <tr><td>Ara Toplam</td><td class="r">{ara:,.2f} ₺</td></tr>
      <tr><td>KDV %20</td><td class="r">{kdv:,.2f} ₺</td></tr>
      <tr class="g"><td>Genel Toplam</td><td class="r">{gen:,.2f} ₺</td></tr>
    </table>
    <div class="fsc">♻ FSC® Chain of Custody · Parti: <b>{parti}</b> · Hammadde girişi faturasıdır.</div>
    <div class="foot"><span>FSC Takip ERP · Test Belgesi</span><span>Fatura Tarihi: {tarih}</span></div>
    </body></html>"""

def to_pdf(html_str, out_pdf):
    hp = os.path.join(HTMLDIR, os.path.basename(out_pdf).replace(".pdf",".html"))
    with open(hp, "w", encoding="utf-8") as f:
        f.write(html_str)
    subprocess.run([EDGE, "--headless=new", "--disable-gpu", "--no-pdf-header-footer",
                    f"--print-to-pdf={out_pdf}", "file:///" + hp.replace("\\","/")],
                   check=True, timeout=60,
                   stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

irs_no = 1001
fat_no = 2001
done = 0
for parti, tarih, kod, ad, kg in LOTS:
    to_pdf(render_irsaliye(parti, tarih, kod, ad, kg, f"IRS-2025-{irs_no}"),
           os.path.join(ROOT, f"IRS_{parti}.pdf"))
    to_pdf(render_fatura(parti, tarih, kod, ad, kg, f"FAT-2025-{fat_no}"),
           os.path.join(ROOT, f"FAT_{parti}.pdf"))
    irs_no += 1; fat_no += 1; done += 2
    print(f"OK {parti}")

print(f"\nTOPLAM {done} PDF -> {ROOT}")
