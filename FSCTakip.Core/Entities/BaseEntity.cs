using System;

namespace FSCTakip.Core.Entities
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }

        // DbContext içinde kullandığımız zorunlu alanlar:
        public string CreatedBy { get; set; } = "System";
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Opsiyonel alanlar
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}