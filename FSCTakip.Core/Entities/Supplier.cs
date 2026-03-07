using System;

namespace FSCTakip.Core.Entities
{
    public class Supplier : BaseEntity
    {
        public string SupplierCode { get; set; }
        public string Name { get; set; }
        public string FscCode { get; set; }
        public DateTime? FscExpiryDate { get; set; } // Sertifika Bitiş Tarihi
        public string ContactPerson { get; set; }    // Yetkili Kişi
        public string Phone { get; set; }            // Telefon
        public string Email { get; set; }            // E-Posta
        public bool IsFscActive { get; set; } = true;
        public bool IsActive { get; set; } = true;    // Genel Aktiflik
    }
}