using System.Collections.Generic;

namespace FSCTakip.Core.Entities
{
    public class Product : BaseEntity
    {
        // Temel Ürün Bilgileri
        public string Code { get; set; }          // Ürün / Stok Kodu
        public string Name { get; set; }          // Ürün Adı
        public string Unit { get; set; }          // Birim (Adet, Kg, Paket vb.)

        // İlişkili Tanımlar
        public int? ProductGroupId { get; set; }
        public virtual ProductGroup ProductGroup { get; set; }

        public int? FscTypeId { get; set; }
        public virtual FscType FscType { get; set; }

        public int? PaperTypeId { get; set; }
        public virtual PaperType PaperType { get; set; }

        public int? PaperColorId { get; set; }
        public virtual PaperColor PaperColor { get; set; }

        // Aktif / Pasif Durumu
        public bool IsActive { get; set; } = true;

        // Reçete ilişkileri için eklenen alanlar:
        public virtual ICollection<ProductRecipe> ParentRecipes { get; set; } = new List<ProductRecipe>();
        public virtual ICollection<ProductRecipe> ChildRecipes { get; set; } = new List<ProductRecipe>();
    }
}