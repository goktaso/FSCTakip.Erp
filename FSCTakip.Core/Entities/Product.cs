using FSCTakip.Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FSCTakip.Core.Entities
{
    public class Product : BaseEntity
    {
        // --- 1. TEMEL KİMLİK BİLGİLERİ ---
        [Required]
        [StringLength(100)]
        public string StockCode { get; set; }    // STOK_KODU

        [Required]
        [StringLength(500)]
        public string ProductName { get; set; }  // STOK_ADI

        public bool IsActive { get; set; } = true; // [2026-03-01] Aktif/Pasif Yönetimi


        // --- 2. PARAMETRİK İLİŞKİLER (DIŞ TABLOLAR) ---

        // Ürün Grubu (Hammadde, Mamül vb.)
        public int ProductGroupId { get; set; }
        [ForeignKey("ProductGroupId")]
        public virtual ProductGroup ProductGroup { get; set; }

        // Kağıt Tipi (Virgin, Recycle vb.)
        public int PaperTypeId { get; set; }
        [ForeignKey("PaperTypeId")]
        public virtual PaperType PaperType { get; set; }

        // Kağıt Rengi (White, Brown vb.)
        public int PaperColorId { get; set; }
        [ForeignKey("PaperColorId")]
        public virtual PaperColor PaperColor { get; set; }

        // FSC Sertifika Tipi (Mix, 100% vb.)
        public int FscTypeId { get; set; }
        [ForeignKey("FscTypeId")]
        public virtual FscType FscType { get; set; }


        // --- 3. TEKNİK ÖZELLİKLER (KOD_4 ve Ölçüler) ---

        [StringLength(50)]
        public string Grammage { get; set; }       // KOD_4 (Örn: 90 G)

        // Hammadde ve Yarı Mamül için Kritik Alan
        public decimal? BobbinWidth { get; set; }  // BOBIN_ENI

        // Mamül için Ölçü Alanları (En x Boy x Körük)
        public decimal? Width { get; set; }        // EN
        public decimal? Height { get; set; }       // BOY
        public decimal? Gusset { get; set; }       // GENISLIK (Körük)


        // --- 4. LOJİSTİK VE PAKETLEME DETAYLARI ---

        public int? InBoxQuantity { get; set; }     // KOLIICI (Koli içindeki adet)

        public int? PalletBoxQuantity { get; set; } // KUP (Bir paletteki koli sayısı)

        [Column(TypeName = "decimal(18,3)")]
        public decimal? NetWeight { get; set; }     // Birim veya Koli Net Kg

        [Column(TypeName = "decimal(18,3)")]
        public decimal? GrossWeight { get; set; }   // Birim veya Koli Brüt Kg
    }
}