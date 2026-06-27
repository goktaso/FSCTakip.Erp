using System;

namespace FSCTakip.Core.Entities
{
    public class EtlJob : BaseEntity
    {
        public int? EtlConnectionId { get; set; }
        public virtual EtlConnection? EtlConnection { get; set; }

        public string JobType { get; set; } = string.Empty;   // ProductImport, SupplierImport, CustomerImport
        public string Source { get; set; } = string.Empty;    // Manuel, Logo, Mikro vb.
        public string Status { get; set; } = "Running";       // Running, Completed, Failed, Partial
        public DateTime StartedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; }
        public int TotalRecords { get; set; }
        public int InsertedCount { get; set; }
        public int UpdatedCount { get; set; }
        public int SkippedCount { get; set; }
        public int ErrorCount { get; set; }
        public string? SourceFile { get; set; }
        public string? Notes { get; set; }
        public string? ErrorDetails { get; set; }
    }
}
