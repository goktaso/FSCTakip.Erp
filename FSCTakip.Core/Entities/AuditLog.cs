namespace FSCTakip.Core.Entities
{
    public class AuditLog
    {
        public long   Id          { get; set; }
        public string TableName   { get; set; } = "";
        public int?   RecordId    { get; set; }
        public string Action      { get; set; } = "";   // INSERT | UPDATE | DELETE
        public string? OldValues  { get; set; }         // JSON — sadece UPDATE ve DELETE
        public string? NewValues  { get; set; }         // JSON — sadece INSERT ve UPDATE
        public string? ChangedBy  { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.Now;
        public string? IpAddress  { get; set; }
    }
}
