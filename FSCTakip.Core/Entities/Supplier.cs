using System;

namespace FSCTakip.Core.Entities
{
    public class Supplier : BaseEntity
    {
        public string SupplierCode { get; set; }
        /// <summary>Harici ERP'deki tedarikçi kodu (örn. Netsis CARI_KOD). ETL eşleştirmede birincil anahtar.</summary>
        public string? ExternalCode { get; set; }
        public string Name { get; set; }
        public string FscCode { get; set; }
        public DateTime? FscExpiryDate { get; set; } // Sertifika Bitiş Tarihi
        public string ContactPerson { get; set; }    // Yetkili Kişi
        public string Phone { get; set; }            // Telefon
        public string Email { get; set; }            // E-Posta
        public string Address { get; set; }          // Adres
        public string City { get; set; }             // Şehir
        public string TaxNumber { get; set; }        // Vergi Numarası
        public string TaxOffice { get; set; }        // Vergi Dairesi
        public bool IsFscActive { get; set; } = true;
        public bool IsActive { get; set; } = true;    // Genel Aktiflik
    }
}