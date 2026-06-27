namespace FSCTakip.Core.Entities
{
    public enum WasteCategory
    {
        KesimArtigi   = 1,  // Kesim / trim artığı
        BaskiArtigi   = 2,  // Baskı hatası / renk ayarı
        IslanmaHasari = 3,  // Islak/nem hasarı
        NakliyeHasari = 4,  // Nakliye/taşıma hasarı
        MakineHatasi  = 5,  // Makine arızası kaynaklı
        Diger         = 99  // Diğer
    }

    public class WasteManagement : BaseEntity
    {
        public string WasteCode { get; set; } = string.Empty;

        // Hangi iş emrinden kaynaklandı (opsiyonel)
        public int? WorkOrderId { get; set; }
        public virtual WorkOrder? WorkOrder { get; set; }

        public WasteCategory Category { get; set; } = WasteCategory.Diger;
        public string Description { get; set; } = string.Empty;

        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "kg";

        public DateTime DisposalDate { get; set; } = DateTime.Today;
        public string? DisposalMethod { get; set; }  // İmha, Geri Dönüşüm, Satış...
        public string? DisposedBy { get; set; }
        public string? Notes { get; set; }
    }
}
