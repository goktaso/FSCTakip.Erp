namespace FSCTakip.Core.Entities
{
    public class ProductGroup : BaseEntity
    {
        public int GroupCode { get; set; }
        public string GroupName { get; set; }
        public int RangeStart { get; set; }
        public int RangeEnd { get; set; }

        // --- YENİ EKLENDİ ---
        public bool IsActive { get; set; } = true;
    }
}