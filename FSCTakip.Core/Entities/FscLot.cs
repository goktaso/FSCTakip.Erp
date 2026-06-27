using System;
using System.Collections.Generic;

namespace FSCTakip.Core.Entities
{
    public class FscLot : BaseEntity
    {
        /// <summary>Tedarikçi parti numarası (örn: 24H0604). Zorunlu.</summary>
        public string PartiNo { get; set; } = string.Empty;

        public int FscTypeId { get; set; }
        public virtual FscType FscType { get; set; } = null!;

        public int? SupplierId { get; set; }
        public virtual Supplier? Supplier { get; set; }

        // Hangi hammadde ürününe ait (stok hareketi için gerekli)
        public int? ProductId { get; set; }
        public virtual Product? Product { get; set; }

        public string? InvoiceNo { get; set; }
        public string? DispatchNo { get; set; }
        public DateTime ArrivalDate { get; set; } = DateTime.Now;
        public string? TruckPlate { get; set; }
        public decimal? InvoiceAmount { get; set; }
        public string? Currency { get; set; } = "TRY";
        public string? Notes { get; set; }

        // Dijital arşiv
        public string? InvoicePdfPath { get; set; }
        public string? DispatchPdfPath { get; set; }

        // Yarı mamül dönüşümünden oluşan lotlar için: kaynak ham/YM bobini ve dönüşüm firesi (CoC izi)
        public int? SourceSerialId { get; set; }
        public virtual FscSerial? SourceSerial { get; set; }   // navigation property — izlenebilirlik
        public decimal? ConversionFireKg { get; set; }

        public virtual ICollection<FscSerial> Serials { get; set; } = new List<FscSerial>();
    }
}