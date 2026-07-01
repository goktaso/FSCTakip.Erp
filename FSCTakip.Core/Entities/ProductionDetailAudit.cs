namespace FSCTakip.Core.Entities
{
    /// <summary>Tüketim kaydı düzeltme/silme denetim izi. FSC CoC denetimi için kim/ne zaman/neden.</summary>
    public class ProductionDetailAudit
    {
        public int Id { get; set; }

        public int ProductionDetailId { get; set; }
        public int WorkOrderId { get; set; }

        public string Action { get; set; } = string.Empty; // "Edit" | "Delete"
        public string Reason { get; set; } = string.Empty;

        public decimal OldConsumedWeight { get; set; }
        public decimal OldWasteWeight { get; set; }
        public decimal OldProducedQuantity { get; set; }

        public decimal? NewConsumedWeight { get; set; }
        public decimal? NewWasteWeight { get; set; }
        public decimal? NewProducedQuantity { get; set; }

        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedDate { get; set; } = DateTime.Now;
    }
}
