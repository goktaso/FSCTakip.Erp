using System;

namespace FSCTakip.Core.Entities
{
    public class Unit : BaseEntity
    {
        // Örn: "ADET", "KG", "METRE"
        public string Name { get; set; } = string.Empty;

        // Örn: "PCS", "KGS", "MTR" (Opsiyonel uluslararası kodlar için)
        public string? ShortCode { get; set; }

        public bool IsActive { get; set; } = true;
    }
}