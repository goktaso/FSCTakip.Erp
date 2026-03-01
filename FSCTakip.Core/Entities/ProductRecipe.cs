using System.ComponentModel.DataAnnotations.Schema;

namespace FSCTakip.Core.Entities
{
    public class ProductRecipe : BaseEntity
    {
        // Üretilecek Ana Ürün
        public int ParentProductId { get; set; }
        public virtual Product ParentProduct { get; set; }

        // İçine Girecek Malzeme
        public int ChildProductId { get; set; }
        public virtual Product ChildProduct { get; set; }

        // Standart Tüketim Miktarı
        [Column(TypeName = "decimal(18,5)")]
        public decimal StandardQuantity { get; set; }

        public string Unit { get; set; } // Kg, Adet vb.
        public bool IsActive { get; set; } = true;
    }
}