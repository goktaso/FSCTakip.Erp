using System.ComponentModel.DataAnnotations;

namespace FSCTakip.Models
{
    public class PaperType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } // Örn: Kraft, Sülfit

        [StringLength(20)]
        public string ShortCode { get; set; } // Örn: KRT, SLF

        public bool IsActive { get; set; } = true;
    }
}