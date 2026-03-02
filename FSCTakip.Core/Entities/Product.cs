using System.Collections.Generic;

namespace FSCTakip.Core.Entities
{
    public class Product : BaseEntity
    {
        // ... Mevcut diğer alanların (Name, Code vb.) ...

        // Reçete ilişkileri için eklenen alanlar:
        public virtual ICollection<ProductRecipe> ParentRecipes { get; set; } = new List<ProductRecipe>();
        public virtual ICollection<ProductRecipe> ChildRecipes { get; set; } = new List<ProductRecipe>();
    }
}