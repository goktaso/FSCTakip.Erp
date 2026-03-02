using System.ComponentModel.DataAnnotations;

namespace FSCTakip.Core.Entities
{
    public class PaperWeight
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public decimal Value { get; set; } // Örn: 70, 80.5

        public string Unit { get; set; } = "gr"; // Varsayılan birim

        public bool IsActive { get; set; } = true;
    }
}