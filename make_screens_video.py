# make_screens_video.py — FSC Takip ERP ekran tanıtım videosu + seslendirme
# Kaynak: docs/denetim_gunu_2026-07-03/img ekranları + canlı login ekranı
# Ses: edge-tts (tr-TR-AhmetNeural), sahne süreleri ses uzunluğuna göre belirlenir.
# Çıktı: FSC-Tanitim-Ekranlar.mp4 (1920x1080, 30fps) + FSC-Tanitim-Seslendirme.mp3
import asyncio
import os
import ssl

import edge_tts
import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont
from moviepy import (AudioFileClip, CompositeAudioClip, CompositeVideoClip,
                     ImageClip, VideoClip, concatenate_videoclips, vfx)

try:
    ssl._create_default_https_context = ssl._create_unverified_context
except AttributeError:
    pass

BASE = os.path.dirname(os.path.abspath(__file__))
IMG = os.path.join(BASE, "img")
OUT_MP4 = os.path.join(BASE, "FSC-Tanitim-Ekranlar.mp4")
OUT_MP3 = os.path.join(BASE, "FSC-Tanitim-Seslendirme.mp3")
W, H = 1920, 1080
FPS = 30
BG = (15, 22, 33)          # --bg-dark #0f1621
BRAND = (25, 118, 210)     # --brand #1976d2
FONT_B = "C:/Windows/Fonts/segoeuib.ttf"
FONT_R = "C:/Windows/Fonts/segoeui.ttf"
VOICE = "tr-TR-AhmetNeural"
SCROLL_SPEED = 500  # px/sn üst sınır (titreme önleme + tempo)
SCROLL_HOLD = 0.6   # başta bekleme
SCROLL_TAIL = 0.5   # sonda bekleme
CONTACT = ["ARD SİSTEM VE DANIŞMANLIK", "Tel: 0532 302 02 50", "ozaygoktas@ardsistem.net.tr"]
MUSIC = os.path.join(BASE, "kurumsal_fon_muzigi.mp3")  # varsa %15 sesle eklenir

BRAND_LINE = "FSCTakip.Erp · ARD Sistem Danışmanlık"

# voice: parçalı seslendirme — t=metin, rate=hız, pitch=perde, pause=parça sonrası es (sn)
SCENES = [
    dict(kind="card", title="FSCTakip.Erp",
         sub="Kraft Torba Üreticileri için FSC İzlenebilirlik",
         sub2="Kare Dip · V Kesim · Burgu Saplı Torba Üretim Hatları",
         voice=[
             dict(t="Fe Se Ce Takip İ Ar Pi.", rate="-8%", pitch="-3Hz", pause=0.45),
             dict(t="Kraft kağıt torba üreticileri için, uçtan uca FSC izlenebilirlik.",
                  rate="-4%", pause=0.35),
             dict(t="Bobinden bitmiş torbaya, üretim hattınızın tamamı...", rate="+2%", pause=0.25),
             dict(t="Tek sistemde!", rate="-8%", pitch="+6Hz", pause=0.5),
             dict(t="A Re De Sistem ve Danışmanlık güvencesiyle.", rate="+0%"),
         ]),
    dict(kind="shot", img="login.png", caption="Güvenli Kurumsal Giriş",
         voice=[
             dict(t="Kurumsal ve güvenli giriş ekranı.", rate="-4%", pitch="-2Hz", pause=0.3),
             dict(t="Kullanıcı bazlı yetkilendirme ve çoklu şirket veritabanı desteğiyle.",
                  rate="+8%"),
         ]),
    dict(kind="shot", img="talep0_denetim_donemleri.png", caption="Denetim Dönemleri",
         focus=dict(box=(1096, 485, 1222, 692), part=1),
         voice=[
             dict(t="FSC denetim dönemlerinizi tanımlayın.", rate="+2%", pause=0.25),
             dict(t="Kapanan dönemler, otomatik kilitlenir!", rate="-5%", pitch="+4Hz", pause=0.3),
             dict(t="Torba üretim kayıtlarınız denetime hazır kalır.", rate="+2%"),
         ]),
    dict(kind="shot", img="talep3_supplier_fsc.png", caption="Tedarikçi FSC Sertifika Doğrulama",
         focus=dict(box=(660, 195, 1417, 298), part=1),
         voice=[
             dict(t="Kraft kağıt aldığınız firmaların FSC sertifika kodları ve geçerlilik "
                    "tarihleri tek ekranda doğrulanır.", rate="+4%", pause=0.3),
             dict(t="Süresi yaklaşan sertifikalar için, uyarı alırsınız!",
                  rate="-3%", pitch="+5Hz"),
         ]),
    dict(kind="shot", img="talep1_urun_gruplari.png", caption="Ürün Grupları ve Kod Sistemi",
         voice=[
             dict(t="Hammadde, yarı mamul ve burgu sap grupları; otomatik kod aralıklarıyla "
                    "torba üretimine uygun, düzenli ürün kartları.", rate="+10%"),
         ]),
    dict(kind="shot", img="talep1_makineler2.png", caption="Torba Makineleri ve İş Emirleri",
         voice=[
             dict(t="Torba makinelerinizi tanımlayın; iş emirleri, bobin ve seri tüketimi ve "
                    "fire takibiyle ilişkilendirin.", rate="+10%"),
         ]),
    dict(kind="shot", img="talep1_kullanicilar.png", caption="Kullanıcı ve Yetki Yönetimi",
         voice=[
             dict(t="Kullanıcı ve rol yönetimi. Kim neyi görür, kim neyi değiştirir; "
                    "kontrol tamamen sizde.", rate="+8%"),
         ]),
    dict(kind="scroll", img="talep2_denetim_ozeti_2024.png", caption="Yıllık Denetim Özeti Raporu",
         voice=[
             dict(t="Ve denetim günü geldiğinde...", rate="-15%", pitch="-2Hz", pause=0.7),
             dict(t="Bobin girişinden torba sevkiyatına kadar; kesintisiz lot zinciri, üretim, "
                    "fire ve kütle dengesi kanıtları...", rate="+0%", pause=0.5),
             dict(t="Hepsi tek raporda, tam da denetçinin istediği düzende!",
                  rate="-6%", pitch="+5Hz"),
         ]),
    dict(kind="etl", caption="Mevcut ERP'nize ETL ile Bağlanır",
         voice=[
             dict(t="Mevcut ERP sisteminizden vazgeçmenize gerek yok.", rate="+0%", pause=0.3),
             dict(t="Kullandığınız ERP'deki stok, irsaliye ve üretim verileri, E Te Le "
                    "aktarımıyla Fe Se Ce Takip'e otomatik taşınır.", rate="+4%", pause=0.25),
             dict(t="Çift veri girişi olmadan FSC izlenebilirliğiniz başlar.", rate="+0%"),
         ]),
    dict(kind="card", title="FSCTakip.Erp",
         sub="by ARD Sistem Danışmanlık",
         sub2="Denetime stressiz hazırlanın, zinciri koparmayın.",
         contact=True,
         voice=[
             dict(t="Fe Se Ce Takip İ Ar Pi, A Re De Sistem Danışmanlık firması tarafından, "
                    "sizler için geliştirilmiştir.", rate="-4%", pause=0.45),
             dict(t="Denetime stressiz hazırlanın... zinciri koparmayın.",
                  rate="-10%", pitch="-2Hz"),
         ]),
]


def make_card(title, sub, sub2=None, contact=False, size=(W, H)):
    cw, ch = size
    im = Image.new("RGB", (cw, ch), BG)
    d = ImageDraw.Draw(im)
    # Koyu mavi zemin üzerine ince grid deseni
    grid = (23, 38, 60)
    for x in range(0, cw, 80):
        d.line([(x, 0), (x, ch)], fill=grid, width=1)
    for y in range(0, ch, 80):
        d.line([(0, y), (cw, y)], fill=grid, width=1)
    logo = Image.open(os.path.join(IMG, "ard_logo.png")).convert("RGBA")
    lw = int(cw * 0.19)
    logo = logo.resize((lw, int(logo.height * lw / logo.width)), Image.LANCZOS)
    im.paste(logo, ((cw - lw) // 2, int(ch * 0.14)), logo)
    scl = min(cw / W, 1.0) if cw >= ch else cw / 1400
    f1 = ImageFont.truetype(FONT_B, int(92 * scl) + (0 if cw >= ch else 8))
    f2 = ImageFont.truetype(FONT_R, int(42 * scl) + (0 if cw >= ch else 6))
    f2b = ImageFont.truetype(FONT_R, int(30 * scl) + (0 if cw >= ch else 4))
    f3 = ImageFont.truetype(FONT_B, int(30 * scl) + (0 if cw >= ch else 4))
    fc = ImageFont.truetype(FONT_B, int(38 * scl) + (0 if cw >= ch else 8))
    y0 = int(ch * 0.44)
    tw = d.textlength(title, font=f1)
    d.text(((cw - tw) / 2, y0), title, font=f1, fill=(255, 255, 255))
    ly = y0 + int(f1.size * 1.45)
    d.rectangle([(cw - 160) / 2, ly, (cw + 160) / 2, ly + 6], fill=BRAND)
    sw = d.textlength(sub, font=f2)
    d.text(((cw - sw) / 2, ly + 36), sub, font=f2, fill=(230, 238, 248))
    yy = ly + 36 + int(f2.size * 1.9)
    if sub2:
        s2w = d.textlength(sub2, font=f2b)
        d.text(((cw - s2w) / 2, yy), sub2, font=f2b, fill=(120, 165, 220))
        yy += int(f2b.size * 2.2)
    if contact:
        yy = max(yy + 14, int(ch * 0.70))
        for k, line in enumerate(CONTACT):
            f = f3 if k == 0 else fc
            col = (140, 165, 200) if k == 0 else (255, 255, 255)
            lw2 = d.textlength(line, font=f)
            d.text(((cw - lw2) / 2, yy), line, font=f, fill=col)
            yy += int(f.size * 1.55)
    else:
        foot = "FSCTakip.Erp  ·  ARD SİSTEM VE DANIŞMANLIK"
        fw = d.textlength(foot, font=f3)
        d.text(((cw - fw) / 2, ch - 110), foot, font=f3, fill=(140, 165, 200))
    return im


def make_zoom_frame(img_path, box, caption):
    """Ekranın 'box' bölgesine yakınlaşmış 1920x1080 kare + kırmızı vurgu çerçevesi."""
    src = Image.open(os.path.join(IMG, img_path)).convert("RGB")
    x0, y0, x1, y1 = box
    bw, bh = x1 - x0, y1 - y0
    crop_w = max(int(bw * 1.5), int(bh * 1.5 * 16 / 9), 640)
    crop_h = int(crop_w * 9 / 16)
    cx, cy = (x0 + x1) // 2, (y0 + y1) // 2
    cl = max(0, min(src.width - crop_w, cx - crop_w // 2))
    ct = max(0, min(src.height - crop_h, cy - crop_h // 2))
    crop = src.crop((cl, ct, cl + crop_w, ct + crop_h)).resize((W, H), Image.LANCZOS)
    d = ImageDraw.Draw(crop)
    s = W / crop_w
    rb = [(x0 - cl) * s - 8, (y0 - ct) * s - 8, (x1 - cl) * s + 8, (y1 - ct) * s + 8]
    d.rounded_rectangle(rb, radius=14, outline=(220, 38, 38), width=6)
    cap = caption_bar(caption)
    crop.paste(cap, (0, H - 110), cap)
    return crop


def make_etl_card(caption):
    """ETL entegrasyon diyagramı: Mevcut ERP -> ETL -> FSCTakip.Erp"""
    im = Image.new("RGB", (W, H), BG)
    d = ImageDraw.Draw(im)
    grid = (23, 38, 60)
    for x in range(0, W, 80):
        d.line([(x, 0), (x, H)], fill=grid, width=1)
    for y in range(0, H, 80):
        d.line([(0, y), (W, y)], fill=grid, width=1)
    fT = ImageFont.truetype(FONT_B, 64)
    fB = ImageFont.truetype(FONT_B, 40)
    fS = ImageFont.truetype(FONT_R, 28)
    fA = ImageFont.truetype(FONT_B, 34)
    tw = d.textlength(caption, font=fT)
    d.text(((W - tw) / 2, 130), caption, font=fT, fill=(255, 255, 255))
    d.rectangle([(W - 160) / 2, 230, (W + 160) / 2, 236], fill=BRAND)

    def box(cx, cy, bw, bh, title, subs, accent):
        d.rounded_rectangle([cx - bw / 2, cy - bh / 2, cx + bw / 2, cy + bh / 2],
                            radius=20, fill=(26, 32, 53), outline=accent, width=3)
        t = d.textlength(title, font=fB)
        d.text((cx - t / 2, cy - bh / 2 + 34), title, font=fB, fill=(255, 255, 255))
        yy = cy - bh / 2 + 108
        for s in subs:
            sw = d.textlength(s, font=fS)
            d.text((cx - sw / 2, yy), s, font=fS, fill=(150, 180, 215))
            yy += 42

    cy = 560
    box(360, cy, 470, 300, "MEVCUT ERP'NİZ", ["Stok  ·  İrsaliye", "Üretim  ·  Cari"], (100, 116, 139))
    box(1560, cy, 470, 300, "FSCTakip.Erp", ["FSC Lot  ·  Bobin/Seri", "Kütle Dengesi  ·  CoC"], BRAND)
    # Orta: ETL rozeti + oklar
    d.rounded_rectangle([850, cy - 70, 1070, cy + 70], radius=18,
                        fill=(25, 118, 210), outline=None)
    t = d.textlength("ETL", font=fT)
    d.text((960 - t / 2, cy - 44), "ETL", font=fT, fill=(255, 255, 255))
    for x0, x1 in [(600, 840), (1080, 1320)]:
        d.line([(x0, cy), (x1 - 26, cy)], fill=(120, 165, 220), width=6)
        d.polygon([(x1 - 26, cy - 16), (x1, cy), (x1 - 26, cy + 16)], fill=(120, 165, 220))
    alt = "Otomatik veri aktarımı  ·  Çift giriş yok  ·  Kurulu düzeniniz bozulmaz"
    aw = d.textlength(alt, font=fA)
    d.text(((W - aw) / 2, 810), alt, font=fA, fill=(230, 238, 248))
    foot = "FSCTakip.Erp  ·  ARD SİSTEM VE DANIŞMANLIK"
    f3 = ImageFont.truetype(FONT_B, 30)
    fw = d.textlength(foot, font=f3)
    d.text(((W - fw) / 2, H - 110), foot, font=f3, fill=(140, 165, 200))
    return im


def caption_bar(text):
    """Alt bant: yarı saydam koyu şerit + marka çizgisi + başlık (RGBA)."""
    bar_h = 110
    im = Image.new("RGBA", (W, bar_h), (10, 15, 24, 215))
    d = ImageDraw.Draw(im)
    d.rectangle([0, 0, 8, bar_h], fill=BRAND + (255,))
    f = ImageFont.truetype(FONT_B, 44)
    d.text((44, (bar_h - 60) / 2), text, font=f, fill=(255, 255, 255, 255))
    fb = ImageFont.truetype(FONT_R, 26)
    bw = d.textlength(BRAND_LINE, font=fb)
    d.text((W - bw - 40, (bar_h - 36) / 2), BRAND_LINE, font=fb, fill=(130, 170, 220, 255))
    return im


def fit_on_bg(img_path):
    """Ekran görüntüsünü 1920x1080 koyu zemine oturt (hafif gölgeli)."""
    shot = Image.open(img_path).convert("RGB")
    scale = min((W - 120) / shot.width, (H - 120) / shot.height)
    shot = shot.resize((int(shot.width * scale), int(shot.height * scale)), Image.LANCZOS)
    bg = Image.new("RGB", (W, H), BG)
    x, y = (W - shot.width) // 2, (H - shot.height) // 2
    shadow = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    ImageDraw.Draw(shadow).rounded_rectangle(
        [x - 10, y - 6, x + shot.width + 10, y + shot.height + 14], radius=18, fill=(0, 0, 0, 160))
    shadow = shadow.filter(ImageFilter.GaussianBlur(18))
    bg.paste(Image.alpha_composite(bg.convert("RGBA"), shadow).convert("RGB"), (0, 0))
    bg.paste(shot, (x, y))
    return bg


async def tts_all():
    """Her sahne için parça listesi: [(dosya, süre, es), ...]"""
    per_scene = []
    for i, sc in enumerate(SCENES):
        parts = sc.get("voice") or [dict(t=sc["text"])]
        infos = []
        for j, p in enumerate(parts):
            fn = os.path.join(BASE, f"segment_{i}_{j}.mp3")
            await edge_tts.Communicate(
                p["t"], VOICE, rate=p.get("rate", "+2%"), pitch=p.get("pitch", "+0Hz")
            ).save(fn)
            with AudioFileClip(fn) as a:
                d = a.duration
            infos.append((fn, d, p.get("pause", 0.2)))
        per_scene.append(infos)
    return per_scene


def build_scene(sc, dur, tmp_i, focus_t=None):
    if sc["kind"] == "card":
        frame = make_card(sc["title"], sc["sub"], sc.get("sub2"), contact=sc.get("contact", False))
        p = os.path.join(BASE, f"_card_{tmp_i}.png")
        frame.save(p)
        clip = ImageClip(p).with_duration(dur)
    elif sc["kind"] == "etl":
        frame = make_etl_card(sc["caption"])
        p = os.path.join(BASE, f"_card_{tmp_i}.png")
        frame.save(p)
        clip = ImageClip(p).with_duration(dur)
    elif sc["kind"] == "shot":
        frame = fit_on_bg(os.path.join(IMG, sc["img"]))
        frame.paste(caption_bar(sc["caption"]), (0, H - 110), caption_bar(sc["caption"]))
        p = os.path.join(BASE, f"_shot_{tmp_i}.png")
        frame.save(p)
        base = ImageClip(p).with_duration(dur)
        # Ken Burns: %100 -> %105 yumuşak zoom, merkez sabit
        zoomed = base.resized(lambda t: 1.0 + 0.05 * (t / dur))
        layers = [zoomed.with_position("center")]
        if sc.get("focus") and focus_t is not None and focus_t < dur - 1.0:
            zf = make_zoom_frame(sc["img"], sc["focus"]["box"], sc["caption"])
            zp = os.path.join(BASE, f"_zoom_{tmp_i}.png")
            zf.save(zp)
            layers.append(ImageClip(zp).with_start(focus_t)
                          .with_duration(dur - focus_t)
                          .with_effects([vfx.CrossFadeIn(0.5)]))
        clip = CompositeVideoClip(layers, size=(W, H)).with_duration(dur)
    else:  # scroll — uzun sayfayı tam sayı piksel adımlarıyla kaydır (titreme yok)
        img = Image.open(os.path.join(IMG, sc["img"])).convert("RGB")
        scale = W / img.width
        img = img.resize((W, int(img.height * scale)), Image.LANCZOS)
        arr = np.array(img)
        total = img.height - H
        scroll_t = max(0.1, dur - SCROLL_HOLD - SCROLL_TAIL)

        def prog(u, e=0.12):
            """Trapez hız profili: kısa hızlanma/yavaşlama, ortada sabit hız."""
            v = 1.0 / (1.0 - e)
            if u <= 0:
                return 0.0
            if u >= 1:
                return 1.0
            if u < e:
                return v * u * u / (2 * e)
            if u > 1 - e:
                w = 1 - u
                return 1 - v * w * w / (2 * e)
            return v * (u - e / 2)

        def frame(t):
            u = (t - SCROLL_HOLD) / scroll_t
            y = int(round(total * prog(u)))
            return arr[y:y + H]

        scroll_clip = VideoClip(frame, duration=dur)
        cap = caption_bar(sc["caption"])
        cp = os.path.join(BASE, f"_cap_{tmp_i}.png")
        cap.save(cp)
        cap_clip = ImageClip(cp).with_duration(dur).with_position((0, H - 110))
        clip = CompositeVideoClip([scroll_clip, cap_clip], size=(W, H)).with_duration(dur)
    return clip.with_effects([vfx.FadeIn(0.4), vfx.FadeOut(0.4)])


def scroll_min_dur(sc):
    """Scroll sahnesi için hız sınırına göre asgari süre."""
    img = Image.open(os.path.join(IMG, sc["img"]))
    total = int(img.height * (W / img.width)) - H
    return total / SCROLL_SPEED + SCROLL_HOLD + SCROLL_TAIL


def main():
    per_scene = asyncio.run(tts_all())
    pad = 1.4  # ses bitince nefes payı
    scene_durs = [max(sum(d + pz for _, d, pz in infos) + pad, 4.0) for infos in per_scene]
    for i, sc in enumerate(SCENES):
        if sc["kind"] == "scroll":
            scene_durs[i] = max(scene_durs[i], scroll_min_dur(sc))

    clips, audio_clips, t = [], [], 0.0
    for i, sc in enumerate(SCENES):
        ft = None
        if sc.get("focus"):
            k = sc["focus"]["part"]
            ft = 0.5 + sum(d + pz for _, d, pz in per_scene[i][:k])
        clips.append(build_scene(sc, scene_durs[i], i, focus_t=ft))
        off = t + 0.5
        for fn, d, pz in per_scene[i]:
            audio_clips.append(AudioFileClip(fn).with_start(off))
            off += d + pz
        t += scene_durs[i]

    video = concatenate_videoclips(clips, method="compose")
    audios = list(audio_clips)
    if os.path.exists(MUSIC):
        m = AudioFileClip(MUSIC)
        if m.duration < video.duration:
            m = m.with_effects([vfx.Loop(duration=video.duration)])
        audios.append(m.subclipped(0, video.duration).with_volume_scaled(0.15))
    final_audio = CompositeAudioClip(audios).with_duration(video.duration)
    final_audio.write_audiofile(OUT_MP3, fps=44100)
    video = video.with_audio(final_audio)
    video.write_videofile(OUT_MP4, codec="libx264", audio_codec="aac", fps=FPS,
                          threads=8, preset="faster", bitrate="5000k")
    print(f"OK video={OUT_MP4} sure={video.duration:.1f}s ses={OUT_MP3}")


if __name__ == "__main__":
    main()
