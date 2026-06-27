namespace FSCTakip.Core.Entities
{
    public class SalesOrderLine : BaseEntity
    {
        public int SalesOrderId { get; set; }
        public virtual SalesOrder SalesOrder { get; set; } = null!;

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        // FSC CoC zinciri için: hangi iş emrinden üretildi?
        public int? WorkOrderId { get; set; }
        public virtual WorkOrder? WorkOrder { get; set; }

        public decimal Quantity  { get; set; }
        public decimal UnitPrice { get; set; }
        public string  Unit      { get; set; } = "Adet";
        public string? Notes     { get; set; }
    }
}
