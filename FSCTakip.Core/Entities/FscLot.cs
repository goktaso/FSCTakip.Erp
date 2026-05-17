using System;
using System.Collections.Generic;

namespace FSCTakip.Core.Entities
{
    public class FscLot : BaseEntity
    {
        public string LotNo { get; set; } = string.Empty;

        public int FscTypeId { get; set; }
        public virtual FscType FscType { get; set; } = null!;

        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; } = null!;

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

        public virtual ICollection<FscSerial> Serials { get; set; } = new List<FscSerial>();
    }
}