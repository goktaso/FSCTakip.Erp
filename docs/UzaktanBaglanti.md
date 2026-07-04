# FSC Takip ERP — Uzaktan Bağlantı ve Destek Rehberi

**Doküman No:** ARD-KUR-002 · **Sürüm:** 1.0 · **Tarih:** 04.07.2026
**Amaç:** Kurulum ve bakım desteğinde kullanılacak uzak bağlantı yöntemleri — senaryoya göre doğru araç seçimi ve saha tecrübesiyle doğrulanmış kurulum adımları.

---

## Senaryo → Araç Seçimi

| Senaryo | Araç | Neden |
|---|---|---|
| Aynı yerel ağ (kurulum günü, sahada) | **Windows RDP** (Uzak Masaüstü) | Yerleşik, ücretsiz, süresiz, LAN'da en akıcı. Şart: hedef makine Windows **Pro** |
| İnternet üzerinden anlık destek | **RustDesk** | Açık kaynak, tamamen ücretsiz, ticari kullanımda kısıt yok (AnyDesk/TeamViewer ücretsiz sürümleri ticari algıda oturum keser). Tek exe, kurulum bile gerektirmez |
| Bakım sözleşmeli müşteri (sunucuya düzenli erişim) | **Tailscale + RDP** | 5 dk'lık modern VPN; müşteri sunucusu kalıcı ve şifreli olarak özel ağınızda görünür. Port açtırma/sabit IP gerekmez. Ücretsiz katman (3 kullanıcı/100 cihaz) yeterli |
| Hiçbir şey kurdurulamayan durum | **Hızlı Yardım** (`Win+Ctrl+Q`) | Windows'ta hazır, Microsoft imzalı — kurumsal IT itiraz etmez. Gözetimsiz erişim yok, müşteri her oturumu onaylar |

> Kurumsal müşteri kendi standardını (VPN/Citrix vb.) dayatıyorsa onlarınki kullanılır — runbook Faz 0.7'de yazılı olarak sorulur.

---

## RDP Kurulumu (hedef makine: Windows 11 Pro)

### Hedef (bağlanılacak) makinede
1. `Ayarlar → Sistem → Uzak Masaüstü` → **Etkinleştir**
2. PC adını not al (aynı ekranda yazar) — veya `cmd` → `hostname`
3. IP öğren: `cmd` → `ipconfig | findstr IPv4`
4. **Hesap şartları:**
   - Hesabın **parolası olmalı** — parolasız/salt-PIN hesabı RDP reddeder (PIN yetmez, gerçek parola gerekir; Microsoft hesabıysa hotmail/outlook parolası)
   - Hesap **Yönetici** olmalı ya da "Uzak Masaüstü Kullanıcıları" grubuna eklenmeli
5. **Önerilen desen — ayrı yerel kurulum hesabı** (kişisel Microsoft hesabıyla sunucu yönetme):
   - `Ayarlar → Hesaplar → Diğer kullanıcılar → Hesap ekle → "Bu kişinin oturum açma bilgilerine sahip değilim" → "Microsoft hesabı olmayan bir kullanıcı ekle"`
   - Ad: `kurulum`, güçlü parola belirle
   - Hesaba tıkla → **Hesap türünü değiştir → Yönetici** ← *atlanırsa aşağıdaki 3. hata alınır*

### Bağlanan makinede
`Win+R` → `mstsc` → hedef PC adı veya IP → kullanıcı adı `HEDEFPC\kurulum` formatında + parola.

### Saha tecrübesiyle doğrulanmış hatalar ve çözümleri

| Hata mesajı | Gerçek sebep | Çözüm |
|---|---|---|
| "Zaten sürmekte olan bir konsol oturumunuz olduğundan… bağlanamadı" | mstsc'ye **kendi makinenin** adını/IP'sini yazdın | Hedef makinenin adını/IP'sini gir (hedefte `hostname` / `ipconfig` çalıştırıp doğrula) |
| Parola soruyor ama hesapta "parola yok" | Hesap PIN'le açılıyor; RDP PIN kabul etmez | Microsoft hesabı parolasını kullan, ya da parolalı yerel `kurulum` hesabı aç |
| "Kullanıcı hesabı uzaktan oturum açmaya yetkili olmadığından bağlantı reddedildi" | Hesap standart kullanıcı — RDP yetkisi yok | Hesabı Yönetici yap (`Ayarlar → Hesaplar → Diğer kullanıcılar → Hesap türünü değiştir`) veya yönetici CMD: `net localgroup "Remote Desktop Users" kurulum /add` |
| Ağda ad çözülmüyor (`OGLUM-PC` bulunamadı) | mDNS/NetBIOS engeli | Ad yerine IPv4 adresini kullan; kalıcı çözüm: modemde DHCP rezervasyonu |

---

## RustDesk (internet üzerinden destek)

1. Müşteri `rustdesk.exe`'yi çalıştırır (kurulum paketi `araclar\` klasöründe; yoksa https://rustdesk.com/download) — kurulum gerektirmez, direkt açılır.
2. Müşteri ekrandaki **ID + tek seferlik şifreyi** telefonla okur.
3. Siz kendi RustDesk'inizde ID'yi girip bağlanırsınız.
4. Kalıcı erişim istenirse (müşteri onayıyla): müşteri tarafında "Kalıcı şifre" belirlenir + servis olarak kurulur — bakım sözleşmesindeki uzaktan erişim maddesine yazılır.

## Tailscale (bakım sözleşmeli sunucu erişimi)

1. Hem sizin makinede hem müşteri sunucusunda https://tailscale.com → indir, aynı hesapla (veya davetle) oturum aç.
2. Müşteri sunucusu artık `100.x.y.z` özel adresiyle her yerden erişilebilir (trafik uçtan uca şifreli, WireGuard).
3. `mstsc` → `100.x.y.z` → normal RDP. Uygulamaya da aynı adresle tarayıcıdan erişilebilir (acil müdahale).
4. Müşteri IT'sine yazılı bildirilir; erişim kaydı bakım sözleşmesi ekine işlenir.

---

*Bu rehber ilk saha provasında (04.07.2026) karşılaşılan gerçek hatalarla güncellenmiştir.*
