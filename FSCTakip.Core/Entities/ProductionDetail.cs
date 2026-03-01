using System;
using System.Collections.Generic;

namespace FSCTakip.Core.Entities
{
    // Enum'u namespace içinde ama sınıfın dışında tanımlıyoruz
    public enum ConsumptionArea
    {
        TorbaGovde = 1,
        Sap = 2,
        Etiket = 3,
        Yapiskan = 4
    }

    public class ProductionDetail : BaseEntity
    {
        public int WorkOrderId { get; set; }
        public int FscSerialId { get; set; }
        public DateTime ProductionDate { get; set; }

        // Artık burası hata vermeyecektir
        public int MachineId { get; set; }
        public virtual Machine Machine { get; set; }

        public ConsumptionArea UsedIn { get; set; }

        public decimal ConsumedWeight { get; set; }
        public decimal WasteWeight { get; set; } // Eklediğimiz fire alanı
        public decimal ProducedQuantity { get; set; }
        public decimal ConversionRate { get; set; }

        public virtual WorkOrder WorkOrder { get; set; }
        public virtual FscSerial FscSerial { get; set; }
    }
}