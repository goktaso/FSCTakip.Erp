using System.Collections.Generic;

namespace FSCTakip.Core.Entities
{
    /// <summary>
    /// FSC Denetim kaydı - Her denetim dönemi veya denetim turu için bir kayıt
    /// </summary>
    public class FscAudit : BaseEntity
    {
        public string AuditCode { get; set; } // Örn: FSC-DNT-2025-01
        public string Title { get; set; }    // Örn: "2025 Yılı 1. Dönem İç Denetim"
        public DateTime AuditDate { get; set; }
        public string AuditorName { get; set; }  // Denetçi adı
        public string AuditType { get; set; }    // "İç Denetim" | "Dış Denetim" | "Ön Değerlendirme"
        public string Status { get; set; }       // "Planlandı" | "Devam Ediyor" | "Tamamlandı"
        public string? Notes { get; set; }
        public string? ReportPath { get; set; }  // Rapor PDF yolu

        public virtual ICollection<FscAuditItem> Items { get; set; } = new List<FscAuditItem>();
    }
}
