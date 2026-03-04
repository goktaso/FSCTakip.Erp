using System.Collections.Generic;

namespace FSCTakip.Core.Entities
{
    public class Product : BaseEntity
    {
        // Temel Ürün Alanları (Customer sayfası standardına göre)
        public string ProductCode { get; set; }  // Örn: KRT-70-G50-MIX
        public string ProductName { get; set; }  // Örn: 70gr Kraft Torba

        // Grup İlişkisi
        public int? ProductGroupId { get; set; }
        public virtual ProductGroup? ProductGroup { get; set; }

        // Kağıt İlişkileri
        public int? PaperTypeId { get; set; }
        public virtual PaperType? PaperType { get; set; }

        public int? PaperWeightId { get; set; }
        public virtual PaperWeight? PaperWeight { get; set; }

        public int? PaperWidthId { get; set; }
        public virtual PaperWidth? PaperWidth { get; set; }

        // FSC Tipi
        public int? FscTypeId { get; set; }
        public virtual FscType? FscType { get; set; }

        public bool IsActive { get; set; } = true;

        // Reçete ilişkileri
        public virtual ICollection<ProductRecipe> ParentRecipes { get; set; } = new List<ProductRecipe>();
        public virtual ICollection<ProductRecipe> ChildRecipes { get; set; } = new List<ProductRecipe>();
    }
}