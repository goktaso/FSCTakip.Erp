namespace FSCTakip.Core.Entities
{
    public enum DocumentCategory
    {
        CocSertifika    = 1,   // CoC Sertifikası (firma)
        TedarikciSert   = 2,   // Tedarikçi FSC Sertifikaları
        FscAgreement    = 3,   // FSC Anlaşmaları (Müşteri)
        ElKitabi        = 4,   // FSC El Kitabı
        OrgSema         = 5,   // Organizasyon Şeması
        Talimat         = 6,   // Talimatlar & Prosedürler
        Egitim          = 7,   // Eğitim Kayıtları
        Artwork         = 8,   // Artworkler
        AtamaYazisi     = 9,   // Atama Yazıları
        DuzelticiF      = 10,  // Düzeltici Faaliyetler (DOF)
        IsAkis          = 11,  // İş Akış Şeması
        Standart        = 12,  // FSC Standartları
        Form            = 13,  // Formlar & Kayıtlar
        Diger           = 99   // Diğer
    }

    public class FscDocument : BaseEntity
    {
        public string Title { get; set; } = "";
        public DocumentCategory Category { get; set; }
        public int Year { get; set; }
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public long FileSize { get; set; }
        public string FileExtension { get; set; } = "";
        public string? Notes { get; set; }
        public string? Tags { get; set; }
    }
}
