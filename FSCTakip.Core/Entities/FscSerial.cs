namespace FSCTakip.Core.Entities
{
    public class FscSerial : BaseEntity
    {
        public int LotId { get; set; }
        public string SerialNo { get; set; } = string.Empty;
        /// <summary>Bobin bazında lot numarası (opsiyonel). Tedarikçi bazı durumlarda bobin başına lot kodu verir.</summary>
        public string? LotNo { get; set; }
        public decimal InitialWeight { get; set; }
        public decimal CurrentWeight { get; set; }
        public bool IsOpeningStock { get; set; }
        public string? Notes { get; set; }

        public virtual FscLot Lot { get; set; } = null!;
        public virtual ICollection<ProductionDetail> ProductionDetails { get; set; } = new List<ProductionDetail>();
    }
}