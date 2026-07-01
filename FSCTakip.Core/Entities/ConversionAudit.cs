namespace FSCTakip.Core.Entities
{
    /// <summary>YM dönüşüm kaydı düzeltme/silme denetim izi. FSC CoC denetimi için kim/ne zaman/neden.</summary>
    public class ConversionAudit
    {
        public int Id { get; set; }

        public int SerialId { get; set; }     // FscSerial.Id (YM bobini)
        public string PartiNo { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty; // "Edit" | "Delete"
        public string Reason { get; set; } = string.Empty;

        public DateTime OldTarih { get; set; }
        public decimal OldFireKg { get; set; }

        public DateTime? NewTarih { get; set; }
        public decimal? NewFireKg { get; set; }

        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedDate { get; set; } = DateTime.Now;
    }
}
