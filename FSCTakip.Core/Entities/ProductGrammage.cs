namespace FSCTakip.Core.Entities
{
    public class ProductGrammage : BaseEntity
    {
        // Gramaj Değeri (Örn: 70, 80, 90, 100, 110)
        public int Value { get; set; }

        // Tanım (Örn: "70 gr/m2 Standart Kraft")
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}