using System;

namespace FSCTakip.Core.Entities
{
    public class Warehouse : BaseEntity
    {
        // Hata veren eksik parça buydu:
        public string Name { get; set; } // Örn: "Hammadde Deposu"

        // Depo kodu (Örn: "DEP-01")
        public string Code { get; set; }

        // Deponun aktiflik durumu
        public bool IsActive { get; set; } = true;
    }
}