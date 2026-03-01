namespace FSCTakip.Core.Entities
{
    public class Customer : BaseEntity
    {
        // Örn: "M-00001", "C-10050" (Sayısal kod standartımıza uygun)
        public string CustomerCode { get; set; }

        public string Name { get; set; } // Şirket Tam Ünvanı

        public string TaxNumber { get; set; } // Vergi Numarası
        public string TaxOffice { get; set; } // Vergi Dairesi

        public string Address { get; set; }
        public string City { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        // --- FSC Sertifika Takip Alanları (Kritik) ---

        // Bazı müşterilerin kendi FSC lisans kodu olabilir (Faturada belirtmek gerekebilir)
        public string FscLicenseCode { get; set; }

        // Müşterinin FSC sertifikası aktif mi?
        public bool IsFscActive { get; set; } = true;

        // Müşterinin sertifika bitiş tarihi (Takip için)
        public DateTime? FscExpiryDate { get; set; }

        // --------------------------------------------

        public bool IsActive { get; set; } = true;

        // Bu müşteriye yapılan sevkiyatlar ve iş emirleri
        public virtual ICollection<WorkOrder> WorkOrders { get; set; }
        public virtual ICollection<StockMovement> StockMovements { get; set; }
    }
}