using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FSCTakip.Core.Entities
{
    public class Product : BaseEntity
    {
        public int? ProductGroupId { get; set; }
        public virtual ProductGroup? ProductGroup { get; set; }

        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Unit { get; set; } = "ADET";

        public int? FscTypeId { get; set; }
        public virtual FscType? FscType { get; set; }

        public int? PaperTypeId { get; set; }
        public virtual PaperType? PaperType { get; set; }

        public int? PaperColorId { get; set; }
        public virtual PaperColor? PaperColor { get; set; }

        // --- TEKNİK ÖZELLİKLER (ID Bazlı) [cite: 2026-03-04] ---
        public int? PaperWeightId { get; set; }
        [ForeignKey("PaperWeightId")]
        public virtual PaperWeight? PaperWeight { get; set; }

        public int? PaperWidthId { get; set; }
        [ForeignKey("PaperWidthId")]
        public virtual PaperWidth? PaperWidth { get; set; }

        // --- YENİ: TEDARİKÇİ İLİŞKİSİ [cite: 2026-03-04] ---
        public int? SupplierId { get; set; }
        [ForeignKey("SupplierId")]
        public virtual Supplier? Supplier { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<ProductRecipe> ParentRecipes { get; set; } = new List<ProductRecipe>();
        public virtual ICollection<ProductRecipe> ChildRecipes { get; set; } = new List<ProductRecipe>();
    }
}