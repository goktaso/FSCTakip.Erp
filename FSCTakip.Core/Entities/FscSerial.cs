namespace FSCTakip.Core.Entities
{
    public class FscSerial : BaseEntity
    {
        public int LotId { get; set; }
        public string SerialNo { get; set; } // Örn: S2026-001-01
        public decimal InitialWeight { get; set; } // Giriş Kilogramı
        public decimal CurrentWeight { get; set; } // Kalan Kilogram

        // Geçen yıldan mı devretti? (Denetim karşılaştırması için)
        public bool IsOpeningStock { get; set; }

        public virtual FscLot Lot { get; set; }
    }
}