using System;

namespace FSCTakip.Core.Entities
{
    // BAŞINA 'public' EKLEMEYİ UNUTMA!
    public class WasteManagement : BaseEntity
    {
        public string WasteCode { get; set; } // Örn: Atık Kodu
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public DateTime DisposalDate { get; set; }
        // Diğer alanların...
    }
}