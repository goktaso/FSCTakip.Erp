# -*- coding: utf-8 -*-
"""Ham kagit lotlari icin ARD markali irsaliye + fatura (Edge headless)."""
import os, subprocess, html

ROOT = r"C:\Users\User\Desktop\FSC_ERP_Blackboxai\FSCTakip.WebUI\wwwroot\uploads\test_belgeler"
HTMLDIR = os.path.join(ROOT, "_html")
os.makedirs(HTMLDIR, exist_ok=True)
EDGE = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"

BUYER   = "ARD SİSTEM VE DANIŞMANLIK"
BUY_ADDR= "FSC Takip ERP · Kraft Kağıt İzlenebilirlik · Türkiye"
PRICE   = 28.50  # TL/kg ham kağıt

# parti, tarih, urunKod, urunAd, kg, tedarikci, ted_adres, vkn, irsNo, fatNo
LOTS = [
 ("HAM-DEMO-001","14.06.2026","10267","NATRON NH ECO RBMF090 980 FSC_RECYCLED 100%",2000,
   "NATRON-HAYAT D.O.O. MAGLAJ","Industrijska zona bb · Maglaj, Bosna","BA-4400112233","IRS-2025-3001","FAT-2025-4001"),
 ("HAM-DEMO-002","14.06.2026","10274","NATRON NH ECO RBMF090 800 FSC_RECYCLED 100%",250,
   "NATRON-HAYAT D.O.O. MAGLAJ","Industrijska zona bb · Maglaj, Bosna","BA-4400112233","IRS-2025-3002","FAT-2025-4002"),
 ("HAM-DEMO-003","14.06.2026","10052","KMK RBMF080 780 FSC_RECYCLED",100,
   "KMK PAPER KAHRAMANMARAŞ KAĞIT SAN. ve TİC. A.Ş.","OSB 4. Cad. No:12 · Kahramanmaraş","5710012345","IRS-2025-3003","FAT-2025-4003"),
]

CSS = """<style>
@page { size: A4; margin: 16mm; }
* { font-family:'Segoe UI',Arial,sans-serif; box-sizing:border-box; }
body { color:#1a2035; font-size:13px; }
.top { display:flex; justify-content:space-between; align-items:flex-start; border-bottom:3px solid #1976d2; padding-bottom:14px; margin-bottom:18px; }
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
th { background:#1976d2; color:#fff; font-size:11px; text-transform:uppercase; letter-spacing:.03em; padding:8px 10px; text-align:left; }
td { padding:9px 10px; border-bottom:1px solid #eef1f6; }
.r { text-align:right; }
.tot { margin-top:14px; margin-left:auto; width:48%; }
.tot td { border:none; padding:4px 10px; }
.tot .g { font-weight:700; font-size:15px; color:#0d47a1; border-top:2px solid #1976d2; }
.fsc { margin-top:16px; background:#eef6ee; border:1px solid #cfe6cf; border-radius:8px; padding:9px 12px; font-size:11.5px; color:#1e5631; }
.foot { margin-top:26px; display:flex; justify-content:space-between; font-size:11px; color:#8a94a3; }
.sign { margin-top:34px; display:flex; justify-content:space-between; }
.sign div { width:42%; border-top:1px solid #aab3c0; padding-top:5px; font-size:11px; color:#5f6b7a; text-align:center; }
</style>"""

def head(title, no):
    return f'<div class="top"><div><div class="brand">{BUYER}<small>{BUY_ADDR}</small></div></div><div class="doc"><div class="t">{title}</div><div class="n">No: {no}</div></div></div>'

def meta(tarih, ted, adr, vkn):
    return (f'<div class="meta"><div class="box"><h4>Tedarikçi (Satıcı)</h4><div class="nm">{html.escape(ted)}</div>'
            f'<div class="sm">{html.escape(adr)}</div><div class="sm">VKN: {vkn}</div></div>'
            f'<div class="box"><h4>Alıcı</h4><div class="nm">{BUYER}</div><div class="sm">{BUY_ADDR}</div>'
            f'<div class="sm">Tarih: {tarih}</div></div></div>')

def irsaliye(p):
    parti,tarih,kod,ad,kg,ted,adr,vkn,irs,fat = p
    return (f'<!doctype html><html><head><meta charset="utf-8">{CSS}</head><body>{head("İRSALİYE",irs)}{meta(tarih,ted,adr,vkn)}'
            f'<table><tr><th>Stok Kodu</th><th>Ham Kağıt / Bobin</th><th>Parti No</th><th class="r">Miktar</th><th>Birim</th></tr>'
            f'<tr><td>{kod}</td><td>{html.escape(ad)}</td><td>{parti}</td><td class="r">{kg:,.2f}</td><td>kg</td></tr></table>'
            f'<div class="fsc">♻ FSC® Recycled 100% · Chain of Custody kapsamında sevk. Parti: <b>{parti}</b></div>'
            f'<div class="sign"><div>Teslim Eden</div><div>Teslim Alan</div></div>'
            f'<div class="foot"><span>FSC Takip ERP · Test Belgesi</span><span>Sevk: {tarih}</span></div></body></html>')

def fatura(p):
    parti,tarih,kod,ad,kg,ted,adr,vkn,irs,fat = p
    ara=kg*PRICE; kdv=ara*0.20; gen=ara+kdv
    return (f'<!doctype html><html><head><meta charset="utf-8">{CSS}</head><body>{head("FATURA",fat)}{meta(tarih,ted,adr,vkn)}'
            f'<table><tr><th>Stok Kodu</th><th>Açıklama</th><th class="r">Miktar</th><th class="r">Birim Fiyat</th><th class="r">Tutar</th></tr>'
            f'<tr><td>{kod}</td><td>{html.escape(ad)} (Parti: {parti})</td><td class="r">{kg:,.2f} kg</td>'
            f'<td class="r">{PRICE:,.2f} ₺</td><td class="r">{ara:,.2f} ₺</td></tr></table>'
            f'<table class="tot"><tr><td>Ara Toplam</td><td class="r">{ara:,.2f} ₺</td></tr>'
            f'<tr><td>KDV %20</td><td class="r">{kdv:,.2f} ₺</td></tr>'
            f'<tr class="g"><td>Genel Toplam</td><td class="r">{gen:,.2f} ₺</td></tr></table>'
            f'<div class="fsc">♻ FSC® Recycled 100% · Parti: <b>{parti}</b> · Ham kağıt alış faturası.</div>'
            f'<div class="foot"><span>FSC Takip ERP · Test Belgesi</span><span>Fatura: {tarih}</span></div></body></html>')

def to_pdf(s, out):
    hp = os.path.join(HTMLDIR, os.path.basename(out).replace(".pdf",".html"))
    with open(hp,"w",encoding="utf-8") as f: f.write(s)
    subprocess.run([EDGE,"--headless=new","--disable-gpu","--no-pdf-header-footer",
                    f"--print-to-pdf={out}","file:///"+hp.replace("\\","/")],
                   check=True, timeout=60, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

for p in LOTS:
    parti=p[0]
    to_pdf(irsaliye(p), os.path.join(ROOT,f"IRS_{parti}.pdf"))
    to_pdf(fatura(p),   os.path.join(ROOT,f"FAT_{parti}.pdf"))
    print(f"OK {parti}")
print("BITTI")
