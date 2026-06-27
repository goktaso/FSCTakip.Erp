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

        /// <summary>
        /// Hangi reçete bileşeni için tüketildi? (nullable — eski kayıtlarda boş olabilir)
        /// FSC CoC mass-balance için: gövde / sap / etiket vb. ayrımı
        /// </summary>
        public int? WorkOrderRecipeId { get; set; }
        public virtual WorkOrderRecipe? WorkOrderRecipe { get; set; }

        public DateTime ProductionDate { get; set; } = DateTime.Today;

        public decimal ConsumedWeight { get; set; }
        public decimal WasteWeight { get; set; }
        public decimal ProducedQuantity { get; set; }

        public string? Notes { get; set; }

        /// <summary>
        /// True ise bu satırdaki ConsumedWeight / WasteWeight birim dönüşümü uygulanmış (KG).
        /// False/null ise orijinal birimde (örn. MT) olabilir.
        /// </summary>
        public bool UnitConverted { get; set; }
    }
}
