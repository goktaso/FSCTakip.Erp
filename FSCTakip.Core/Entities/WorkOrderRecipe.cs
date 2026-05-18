namespace FSCTakip.Core.Entities
{
    public class WorkOrderRecipe : BaseEntity
    {
        public int WorkOrderId { get; set; }
        public virtual WorkOrder WorkOrder { get; set; } = null!;

        /// <summary>Reçete bileşeni — ChildProduct (hammadde veya yarı mamül)</summary>
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        /// <summary>Bu bileşen için seçilen FSC bobini/serisi</summary>
        public int? FscSerialId { get; set; }
        public virtual FscSerial? FscSerial { get; set; }

        public decimal PlannedQuantity { get; set; }        // Reçeteden gelen standart miktar (kg)
        public decimal ActualConsumedQuantity { get; set; } // Gerçekte harcanan hammadde (kg)
        public decimal WasteQuantity { get; set; }          // Bu bileşende verilen fire (kg)
        public decimal ProducedQuantity { get; set; }       // Bu bileşenden elde edilen ürün (adet)

        public string? Description { get; set; }            // Opsiyonel not
    }
}