using System;

namespace FSCTakip.Core.Entities
{
    public class AuditPeriod
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Description { get; set; } // Nullable yapıldı
        public bool IsActive { get; set; }
    }
}