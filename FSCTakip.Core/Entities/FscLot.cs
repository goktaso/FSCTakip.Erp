using System.Collections.Generic;

namespace FSCTakip.Core.Entities
{
    public class FscLot : BaseEntity
    {
        public string LotNo { get; set; } // Örn: L2026-001

        // --- İlişkisel Alanlar (JOIN Kolaylığı İçin) ---

        // FscStatus yerine: Mix, %100 gibi tipleri ID ile bağlıyoruz
        public int FscTypeId { get; set; }
        public virtual FscType FscType { get; set; }

        // VendorName yerine: Tedarikçi ID'si ile bağlıyoruz
        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }

        // ----------------------------------------------

        public string InvoiceNo { get; set; }
        public string DispatchNo { get; set; } // İrsaliye No

        // Dijital Arşiv: PDF Dosyalarının sunucudaki yolu
        public string InvoicePdfPath { get; set; }
        public string DispatchPdfPath { get; set; }

        // Bir Lot'un altında birden fazla Bobin (Seri) olur
        public virtual ICollection<FscSerial> Serials { get; set; }
    }
}