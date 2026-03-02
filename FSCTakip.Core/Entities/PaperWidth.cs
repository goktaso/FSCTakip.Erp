using System;

namespace FSCTakip.Core.Entities
{
    public class PaperWidth : BaseEntity
    {
        // Örn: "EN-600", "EN-1050"
        // Stok takibinde ve aramalarda kolaylık sağlar.
        public string Code { get; set; }

        // Örn: 600, 1050.50
        // Matematiksel hesaplamalar (fire hesabı vb.) için decimal uygundur.
        public decimal Value { get; set; }

        // Örn: "mm", "cm" (Standart mm olması önerilir)
        public string Unit { get; set; } = "mm";

        // Bu ölçü hala aktif olarak tedarik ediliyor mu?
        public bool IsActive { get; set; } = true;
    }
}