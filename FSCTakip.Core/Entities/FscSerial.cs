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

        /// <summary>
        /// Sisteme girildiği orijinal miktar (birim dönüşümünden önce).
        /// Null ise giriş zaten KG'dir.
        /// </summary>
        public decimal? OriginalQuantity { get; set; }

        /// <summary>Orijinal birim (örn: "MT"). Null ise giriş zaten KG.</summary>
        public string? OriginalUnit { get; set; }

        public bool IsOpeningStock { get; set; }
        public string? Notes { get; set; }

        /// <summary>
        /// Eşzamanlılık damgası (optimistic locking). İki kullanıcı aynı bobini aynı anda
        /// tüketmeye çalışırsa ikincinin kaydı DbUpdateConcurrencyException ile reddedilir;
        /// kütle dengesi sessizce bozulmaz. SQL Server tarafından otomatik yönetilir.
        /// </summary>
        public byte[]? RowVersion { get; set; }

        public virtual FscLot Lot { get; set; } = null!;
        public virtual ICollection<ProductionDetail> ProductionDetails { get; set; } = new List<ProductionDetail>();
    }
}