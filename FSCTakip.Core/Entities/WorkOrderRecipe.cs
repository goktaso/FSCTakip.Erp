namespace FSCTakip.Core.Entities
{
    public class WorkOrderRecipe : BaseEntity
    {
        public int WorkOrderId { get; set; }
        public virtual WorkOrder WorkOrder { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        // --- İZLENEBİLİRLİĞİN KALBİ ---
        // O üretimde bizzat kullanılan Bobin/Lot seçimi
        public int? FscSerialId { get; set; }
        public virtual FscSerial FscSerial { get; set; }
        // ------------------------------

        public decimal PlannedQuantity { get; set; } // Şablondan gelen miktar
        public decimal ActualConsumedQuantity { get; set; } // Gerçekte harcanan miktar

        public string Description { get; set; } // "Bu seferlik X sapı kullanıldı" gibi notlar
    }
}