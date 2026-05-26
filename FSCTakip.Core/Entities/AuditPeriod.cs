using System;

namespace FSCTakip.Core.Entities
{
    /// <summary>
    /// FSC CoC denetim dönemlerini tanımlar.
    /// Her dönem bir öncekinin bitişinden başlar.
    /// Örnek: 2024 dönemi → 17.12.2023 – 22.11.2024
    /// </summary>
    public class AuditPeriod
    {
        public int      Id          { get; set; }
        public int      Year        { get; set; }          // Denetim yılı (etiket)
        public DateTime StartDate   { get; set; }          // Dönem başlangıcı (önceki denetim + 1 gün)
        public DateTime EndDate     { get; set; }          // Dönem bitişi (mevcut denetim tarihi)
        public string?  Description { get; set; }          // İsteğe bağlı not
        public bool     IsActive    { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string   CreatedBy   { get; set; } = "SYSTEM";

        // ── Dönem Kilidi ──────────────────────────────────────────────
        public bool      IsLocked   { get; set; } = false;
        public DateTime? LockedAt   { get; set; }
        public string?   LockedBy   { get; set; }
    }
}
