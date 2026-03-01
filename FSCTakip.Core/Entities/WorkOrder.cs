namespace FSCTakip.Core.Entities
{
    public class WorkOrder : BaseEntity
    {
        public string WorkOrderNo { get; set; } // İş Emri No
        public string ProductCode { get; set; } // Mamul Kodu
        public string MachineId { get; set; } // Hangi Makine?
        public decimal PlannedQuantity { get; set; } // Planlanan Adet
        public bool IsCompleted { get; set; } // Tamamlandı mı?

        // Bu iş emrinde yapılan tüm üretim hareketleri
        public virtual ICollection<ProductionDetail> ProductionDetails { get; set; }
    }
}