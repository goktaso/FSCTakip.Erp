namespace FSCTakip.Core.Entities
{
    public class ProductRecipe : BaseEntity
    {
        public int ParentProductId { get; set; }
        public virtual Product ParentProduct { get; set; }

        public int ChildProductId { get; set; }
        public virtual Product ChildProduct { get; set; }

        public decimal StandardQuantity { get; set; }
        public string Unit { get; set; }
        public bool IsActive { get; set; }
    }
}