import os
import asyncio
import ssl
import edge_tts
from moviepy import VideoFileClip, AudioFileClip, CompositeAudioClip

# GÜVENLİK/SSL HATASINI TAMAMEN ENGELLEMEK İÇİN GLOBAL YAMA
try:
    ssl._create_default_https_context = ssl._create_unverified_context
except AttributeError:
    pass

# 1. SESLENDİRME METİNLERİ VE ZAMANLAMALARI (Videonun akışına göre)
VOICE_TIMINGS = [
    {"start": 0, "text": "Yarın FSC denetiminiz mi var? Gerçekten hazır mısınız?"},
    {"start": 5, "text": "İrsaliyeler, Excel listeleri, lot numaraları, faturalar... Kanıtlar dağınık, zincir kopuk mu?"},
    {"start": 12, "text": "Aradığınız çözüm: FSCTakip.Erp! Hammaddeden sevkiyata, tüm FSC süreciniz tek bir sistemde."},
    {"start": 17, "text": "Kesintisiz izlenebilirlik zinciri ile; tedarikçi kodu doğrulama, FSC lot takibi, bobin ve seri yönetimi, iş emri, üretim firesi ve sevkiyat... Hepsi kontrol altında."},
    {"start": 28, "text": "Uygulamadan canlı ekranlarla, tek bakışta tüm FSC panonuz karşınızda. FSC denetimlerine stressiz hazırlanın, zinciri koparmayın!"}
]

# 2. EDGE-TTS İLE GERÇEKÇİ SES ÜRETİMİ
async def generate_voice_segments():
    print("🎙️ Yapay zeka sesleri üretiliyor (tr-TR-AhmetNeural)...")
    audio_clips = []
    
    for i, item in enumerate(VOICE_TIMINGS):
        filename = f"segment_{i}.mp3"
        communicate = edge_tts.Communicate(item["text"], "tr-TR-AhmetNeural")
        await communicate.save(filename)
        
        # MOVIEPY V2 UYUMU: set_start yerine with_start kullanıldı
        clip = AudioFileClip(filename).with_start(item["start"])
        audio_clips.append(clip)
        
    return audio_clips

# 3. VİDEO, SES VE MÜZİK BİRLEŞTİRME
def create_final_video(video_path, voice_clips, music_path="kurumsal_fon_muzigi.mp3", output_path="FSC-Tanitim_Final.mp4"):
    print("🎬 Video kurgulanıyor ve birleştiriliyor...")
    video = VideoFileClip(video_path)
    
    if os.path.exists(music_path):
        bg_music = AudioFileClip(music_path)
        # MOVIEPY V2 UYUMU: subclip yerine with_subclip kullanıldı
        bg_music = bg_music.with_subclip(0, video.duration).volumex(0.15)
        all_audios = voice_clips + [bg_music]
    else:
        print("⚠️ 'kurumsal_fon_muzigi.mp3' bulunamadı, video sadece seslendirmeyle üretilecek.")
        all_audios = voice_clips

    final_audio = CompositeAudioClip(all_audios)
    final_video = video.with_audio(final_audio) # set_audio yerine with_audio
    
    final_video.write_videofile(
        output_path, 
        codec='libx264', 
        audio_codec='aac', 
        fps=video.fps,
        threads=4
    )
    
    video.close()
    for clip in voice_clips:
        clip.close()
    
    print(f"✅ İşlem tamamlandı! Çıktı dosyası: {output_path}")

async def main():
    video_file = "FSC-Takip-tanitim.mp4"
    background_music = "kurumsal_fon_muzigi.mp3"
    
    if not os.path.exists(video_file):
        print(f"❌ Hata: {video_file} bulunamadı! Lütfen videoyu bu klasöre taşıyın.")
        return
        
    voice_clips = await generate_voice_segments()
    create_final_video(video_file, voice_clips, background_music)
    
    for i in range(len(VOICE_TIMINGS)):
        if os.path.exists(f"segment_{i}.mp3"):
            os.remove(f"segment_{i}.mp3")

if __name__ == "__main__":
    asyncio.run(main())