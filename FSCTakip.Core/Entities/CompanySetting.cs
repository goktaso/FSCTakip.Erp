namespace FSCTakip.Core.Entities
{
    /// <summary>
    /// Kurulum sahibi firmanın kimlik bilgileri (beyaz etiket).
    /// Tek satırlık tablo — irsaliye/fatura/iş emri formu gibi dışa dönük belgelerde
    /// belge ünvanı olarak bu kayıt kullanılır (ürün markası ARD sabit kalır).
    /// </summary>
    public class CompanySetting : BaseEntity
    {
        public string  CompanyName    { get; set; } = string.Empty; // Belgelerde görünen tam ünvan
        public string? Address        { get; set; }
        public string? City           { get; set; }
        public string? TaxNumber      { get; set; }
        public string? TaxOffice      { get; set; }
        public string? Phone          { get; set; }
        public string? Email          { get; set; }

        /// <summary>Firmanın kendi FSC CoC sertifika kodu (örn. GFA-COC-005968).</summary>
        public string? FscCocCode     { get; set; }
        /// <summary>Firmanın FSC lisans kodu (örn. FSC-C172867) — belge FSC rozetinde basılır.</summary>
        public string? FscLicenseCode { get; set; }

        public string? LogoPath       { get; set; } // opsiyonel: belge başlığına firma logosu
    }
}
