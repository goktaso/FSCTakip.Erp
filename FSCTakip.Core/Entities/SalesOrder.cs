namespace FSCTakip.Core.Entities
{
    public enum SalesOrderStatus
    {
        Taslak       = 1,
        TeslimEdildi = 2,
        Iptal        = 3
    }

    public class SalesOrder : BaseEntity
    {
        public string SalesOrderNo { get; set; } = string.Empty;
        /// <summary>Harici ERP'deki sipariş numarası (örn. Netsis EVRAK_NO). ETL eşleştirmede birincil anahtar.</summary>
        public string? ExternalOrderNo { get; set; }

        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;

        public DateTime OrderDate { get; set; } = DateTime.Today;
        public DateTime? DispatchDate { get; set; }

        public string? DispatchNo   { get; set; }
        public string? InvoiceNo    { get; set; }
        public decimal? InvoiceAmount { get; set; }
        public string Currency      { get; set; } = "TRY";
        public string? PlateNumber  { get; set; }
        public string? DeliveryAddress { get; set; }

        public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Taslak;

        public string? DispatchPdfPath { get; set; }
        public string? InvoicePdfPath  { get; set; }
        public string? Notes           { get; set; }

        public virtual ICollection<SalesOrderLine> Lines { get; set; } = new List<SalesOrderLine>();
    }
}
