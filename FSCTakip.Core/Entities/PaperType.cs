namespace FSCTakip.Core.Entities
{
    public class PaperType:BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortCode { get; set; }
        public bool IsActive { get; set; } = true;
    }
}