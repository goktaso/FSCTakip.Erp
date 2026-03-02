using System;

namespace FSCTakip.Core.Entities
{
    public class Machine : BaseEntity
    {
        // Hata veren kısım burasıydı, bu satırı eklediğinden emin ol:
        public string Name { get; set; } // Örn: "8 Renk Flexo"

        public string Code { get; set; } // Örn: "M-01"

        public string Type { get; set; } // Örn: "Matbaa"

        public bool IsActive { get; set; } = true;
    }
}