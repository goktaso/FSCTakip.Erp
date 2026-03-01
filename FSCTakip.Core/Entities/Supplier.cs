using FSCTakip.Core.Entities;

public class Supplier : BaseEntity
{
    // Id alanı BaseEntity'den (int) otomatik geliyor.

    public string SupplierCode { get; set; } // Örn: TED-001 (Senin istediğin ID/Kod alanı)
    public string Name { get; set; }
    public string FscCode { get; set; }
    public DateTime? FscExpiryDate { get; set; }
    public bool IsFscActive { get; set; } = true;
}