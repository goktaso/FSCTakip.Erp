# kisa_video.py — dikey kisa tanitim: eski bolum (slogan bitisli) + CTA kapanis karti
import asyncio
import os
import ssl
import subprocess

import edge_tts
from moviepy import AudioFileClip, CompositeAudioClip, ImageClip, vfx

try:
    ssl._create_default_https_context = ssl._create_unverified_context
except AttributeError:
    pass

import make_screens_video as m

BASE = os.path.dirname(os.path.abspath(__file__))
VOICE = "tr-TR-AhmetNeural"
PARTS = [
    dict(t="Fe Se Ce Takip İ Ar Pi, A Re De Sistem Danışmanlık firması tarafından, "
           "sizler için geliştirilmiştir.", rate="-4%", pause=0.45),
    dict(t="Detaylı bilgi ve demo için bizi arayın.", rate="-6%", pitch="-2Hz"),
]


def trim(fn):
    tr = fn.replace(".mp3", "_t.wav")
    subprocess.run(["ffmpeg", "-v", "error", "-y", "-i", fn, "-af",
                    "silenceremove=start_periods=1:start_threshold=-45dB,"
                    "areverse,silenceremove=start_periods=1:start_threshold=-45dB,areverse",
                    tr], check=True)
    return tr


async def gen():
    clips, off = [], 0.8
    for j, p in enumerate(PARTS):
        fn = os.path.join(BASE, f"cta_{j}.mp3")
        await edge_tts.Communicate(p["t"], VOICE, rate=p.get("rate", "+0%"),
                                   pitch=p.get("pitch", "+0Hz")).save(fn)
        c = AudioFileClip(trim(fn))
        clips.append(c.with_start(off))
        off += c.duration + p.get("pause", 0.2)
    return clips, off


def main():
    card = m.make_card("FSCTakip.Erp", "Kraft Torba Üreticileri için FSC İzlenebilirlik",
                       "Denetime stressiz hazırlanın, zinciri koparmayın.",
                       contact=True, size=(1080, 1350))
    cp = os.path.join(BASE, "_cta_dikey.png")
    card.save(cp)
    clips, voice_end = asyncio.run(gen())
    dur = voice_end + 1.2
    clip = (ImageClip(cp).with_duration(dur)
            .with_audio(CompositeAudioClip(clips).with_duration(dur))
            .with_effects([vfx.FadeIn(0.5), vfx.FadeOut(0.6)]))
    cta_mp4 = os.path.join(BASE, "_cta_dikey.mp4")
    clip.write_videofile(cta_mp4, codec="libx264", audio_codec="aac", fps=30,
                         preset="faster", bitrate="4000k", threads=4)
    out = os.path.join(BASE, "FSC-Tanitim-Kisa.mp4")
    old = os.path.join(BASE, "FSC-Tanitim_Final_yeni.mp4")
    subprocess.run(["ffmpeg", "-v", "error", "-y", "-i", old, "-i", cta_mp4,
                    "-filter_complex",
                    "[0:v]fps=30,setsar=1,fade=t=out:st=39.7:d=0.6[v0];"
                    "[0:a]afade=t=out:st=39.7:d=0.6,aresample=44100[a0];"
                    "[1:v]setsar=1[v1];[1:a]aresample=44100[a1];"
                    "[v0][a0][v1][a1]concat=n=2:v=1:a=1[v][a]",
                    "-map", "[v]", "-map", "[a]", "-c:v", "libx264", "-preset", "faster",
                    "-b:v", "4000k", "-c:a", "aac", "-b:a", "192k", out], check=True)
    print("OK", out)


if __name__ == "__main__":
    main()
