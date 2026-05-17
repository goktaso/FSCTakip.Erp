namespace FSCTakip.Core.Entities
{
    public class ProductionDetail : BaseEntity
    {
        public int WorkOrderId { get; set; }
        public virtual WorkOrder WorkOrder { get; set; } = null!;

        public int FscSerialId { get; set; }
        public virtual FscSerial FscSerial { get; set; } = null!;

        public int MachineId { get; set; }
        public virtual Machine Machine { get; set; } = null!;

        public DateTime ProductionDate { get; set; } = DateTime.Today;

        public decimal ConsumedWeight { get; set; }
        public decimal WasteWeight { get; set; }
        public decimal ProducedQuantity { get; set; }

        public string? Notes { get; set; }
    }
}
