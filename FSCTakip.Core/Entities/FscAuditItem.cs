using FSCTakip.Core.Entities;

public class FscAuditItem
{
    public int Id { get; set; }
    public int SortOrder { get; set; }
    public string Category { get; set; }
    public string Requirement { get; set; }
    public string Evidence { get; set; }
    public string ConformStatus { get; set; }
    public string NonConformity { get; set; }
    public string CorrectiveAction { get; set; }

    // Standart alanlar (Customer sayfasındaki gibi izlenebilirlik için)
    public bool IsPass { get; set; }

    // İlişki Tanımı
    public int FscAuditId { get; set; }
    public FscAudit FscAudit { get; set; }
}