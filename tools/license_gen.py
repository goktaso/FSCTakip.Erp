# -*- coding: utf-8 -*-
"""
FSC Takip ERP — Lisans Üretici (YALNIZ ARD içi kullanım)
========================================================
Özel anahtar bu repoda DEĞİLDİR ve asla commit edilmez.

Kullanım:
  python tools/license_gen.py --private-key "C:/Users/User/Desktop/ARD_Lisans/ard_private.pem" ^
      --licensed-to "PACKY PACKAGING AMBALAJ SAN. TIC. LTD. STI." ^
      --machine a1b2c3d4e5f60718 ^
      --valid-until 2027-07-04 ^
      --out license.lic

  --machine verilmezse  : makineden bağımsız lisans (yalnız ARD içi geliştirme için!)
  --valid-until verilmezse: süresiz lisans

Müşterinin makine kodu: kurulum sonrası http://<sunucu>/License/Status sayfasında görünür.
"""
import argparse, base64, json, sys, uuid
from datetime import datetime, timezone

from cryptography.hazmat.primitives import hashes, serialization
from cryptography.hazmat.primitives.asymmetric import padding


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--private-key", required=True, help="ard_private.pem yolu (repo dışı)")
    ap.add_argument("--licensed-to", required=True, help="Lisans sahibi firma ünvanı")
    ap.add_argument("--machine", default=None, help="Sunucu kimlik kodu (License/Status'tan; boş = makineden bağımsız)")
    ap.add_argument("--valid-until", default=None, help="YYYY-MM-DD (boş = süresiz)")
    ap.add_argument("--out", default="license.lic")
    args = ap.parse_args()

    with open(args.private_key, "rb") as f:
        key = serialization.load_pem_private_key(f.read(), password=None)

    payload = {
        "licenseId":  str(uuid.uuid4())[:8].upper(),
        "licensedTo": args.licensed_to,
        "machineKey": args.machine,
        "validUntil": args.valid_until,
        "issuedAt":   datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
    }
    payload_bytes = json.dumps(payload, ensure_ascii=False, separators=(",", ":")).encode("utf-8")
    signature = key.sign(payload_bytes, padding.PKCS1v15(), hashes.SHA256())

    content = base64.b64encode(payload_bytes).decode() + "." + base64.b64encode(signature).decode()
    with open(args.out, "w", encoding="ascii") as f:
        f.write(content)

    print(f"Lisans üretildi: {args.out}")
    print(f"  Lisans No : {payload['licenseId']}")
    print(f"  Sahibi    : {payload['licensedTo']}")
    print(f"  Makine    : {payload['machineKey'] or '(bağımsız — yalnız ARD içi!)'}")
    print(f"  Geçerlilik: {payload['validUntil'] or 'süresiz'}")


if __name__ == "__main__":
    sys.exit(main())
