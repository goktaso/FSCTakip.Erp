# redub_old_video.py — FSC-Tanitim_Final.mp4'un ses katmanini rejili/telaffuz duzeltmeli
# yeni TTS ile degistirir. Video izi aynen korunur (-c:v copy, render yok).
import asyncio
import os
import ssl
import subprocess

import edge_tts
from moviepy import AudioFileClip, CompositeAudioClip

try:
    ssl._create_default_https_context = ssl._create_unverified_context
except AttributeError:
    pass

BASE = os.path.dirname(os.path.abspath(__file__))
SRC = r"C:\Users\User\Desktop\FSC_ERP_Blackboxai\FSC-Tanitim_Final.mp4"
VOICE = "tr-TR-AhmetNeural"
DUR = 40.315

# Hedef baslangiclar (orijinal kurguya sadik); tasma olursa ardisik kaydirilir
SEGMENTS = [
    dict(start=0.0, parts=[
        dict(t="Yarın FSC denetiminiz mi var?", rate="+10%", pitch="+3Hz", pause=0.25),
        dict(t="Gerçekten hazır mısınız?", rate="+2%", pitch="-2Hz"),
    ]),
    dict(start=5.0, parts=[
        dict(t="İrsaliyeler, Excel listeleri, lot numaraları, faturalar...", rate="+16%", pause=0.25),
        dict(t="Kanıtlar dağınık, zincir kopuk mu?", rate="+4%", pitch="+4Hz"),
    ]),
    dict(start=12.0, parts=[
        dict(t="Aradığınız çözüm: Fe Se Ce Takip İ Ar Pi!", rate="+4%", pitch="+4Hz", pause=0.2),
        dict(t="Hammaddeden sevkiyata, tüm FSC süreciniz, tek bir sistemde!", rate="+14%"),
    ]),
    dict(start=17.0, parts=[
        dict(t="Kesintisiz izlenebilirlik zinciri ile; firma FSC kodu doğrulama, FSC lot takibi, "
               "bobin ve seri yönetimi, iş emri, üretim firesi ve sevkiyat...", rate="+16%", pause=0.25),
        dict(t="Hepsi kontrol altında!", rate="+2%", pitch="+5Hz"),
    ]),
    dict(start=28.0, parts=[
        dict(t="Uygulamadan canlı ekranlarla, tek bakışta tüm FSC panonuz karşınızda.",
             rate="+10%", pause=0.3),
        dict(t="FSC denetimlerine stressiz hazırlanın... zinciri koparmayın!",
             rate="+0%", pitch="-2Hz"),
    ]),
]


async def gen():
    clips = []
    prev_end = 0.0
    for i, seg in enumerate(SEGMENTS):
        off = max(seg["start"], prev_end + 0.3)
        seg_start = off
        for j, p in enumerate(seg["parts"]):
            fn = os.path.join(BASE, f"old_{i}_{j}.mp3")
            await edge_tts.Communicate(p["t"], VOICE, rate=p.get("rate", "+0%"),
                                       pitch=p.get("pitch", "+0Hz")).save(fn)
            # edge-tts'in bas/son sessizligini kirp
            tr = os.path.join(BASE, f"old_{i}_{j}_trim.wav")
            subprocess.run(["ffmpeg", "-v", "error", "-y", "-i", fn, "-af",
                            "silenceremove=start_periods=1:start_threshold=-45dB,"
                            "areverse,silenceremove=start_periods=1:start_threshold=-45dB,areverse",
                            tr], check=True)
            c = AudioFileClip(tr)
            clips.append(c.with_start(off))
            off += c.duration + p.get("pause", 0.2)
        prev_end = off
        print(f"seg{i}: {seg_start:.1f} -> {prev_end:.1f} (hedef {seg['start']})")
    if prev_end > DUR:
        print(f"UYARI: toplam {prev_end:.1f}s > video {DUR}s")
    return clips


def main():
    clips = asyncio.run(gen())
    mix = CompositeAudioClip(clips).with_duration(DUR)
    wav = os.path.join(BASE, "old_voice.wav")
    mix.write_audiofile(wav, fps=44100)
    out = os.path.join(BASE, "FSC-Tanitim_Final_yeni.mp4")
    subprocess.run(["ffmpeg", "-v", "error", "-y", "-i", SRC, "-i", wav,
                    "-map", "0:v", "-map", "1:a", "-c:v", "copy",
                    "-c:a", "aac", "-b:a", "192k", "-shortest", out], check=True)
    print("OK", out)


if __name__ == "__main__":
    main()
