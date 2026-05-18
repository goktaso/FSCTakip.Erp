using System;

namespace FSCTakip.Core.Entities
{
    public class EtlConnection : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "Excel"; // Excel, Logo, Mikro, Netsis, Api, SqlServer
        public string? Description { get; set; }
        public string? Settings { get; set; }        // JSON: bağlantı parametreleri
        public bool IsActive { get; set; } = true;
        public DateTime? LastSyncAt { get; set; }
        public string? LastSyncStatus { get; set; }  // Success, Failed, Partial
    }
}
