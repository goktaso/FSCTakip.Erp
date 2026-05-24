namespace FSCTakip.Core.Entities
{
    public enum WorkOrderStatus
    {
        Taslak     = 1,
        Uretimde   = 2,
        Tamamlandi = 3,
        Iptal      = 4
    }

    public class WorkOrder : BaseEntity
    {
        public string WorkOrderNo { get; set; } = string.Empty;
        /// <summary>Harici ERP'deki iş emri numarası. ETL veya manuel eşleştirmede referans.</summary>
        public string? ExternalOrderNo { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public int MachineId { get; set; }
        public virtual Machine Machine { get; set; } = null!;

        public DateTime PlannedDate { get; set; } = DateTime.Today;
        public DateTime? CompletedDate { get; set; }

        public decimal PlannedQuantity { get; set; }
        public decimal ActualQuantity { get; set; }

        public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Taslak;
        public string? Notes { get; set; }

        public virtual ICollection<ProductionDetail>  ProductionDetails  { get; set; } = new List<ProductionDetail>();
        public virtual ICollection<WorkOrderRecipe>   WorkOrderRecipes   { get; set; } = new List<WorkOrderRecipe>();
    }
}
