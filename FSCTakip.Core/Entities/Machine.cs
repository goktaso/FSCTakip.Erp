using System.ComponentModel.DataAnnotations;

namespace FSCTakip.Core.Entities
{
    public class Machine
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Code { get; set; } // SQL'de NOT NULL olduğu için zorunlu

        [Required]
        public string Type { get; set; } // SQL'de NOT NULL olduğu için zorunlu

        public bool IsActive { get; set; } = true;

        // SQL'deki datetime2(7) ve nvarchar(max) alanların karşılığı
        public string CreatedBy { get; set; } = "System";
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // SQL'de olmayan ama senin daha önce bahsettiğin alanlar (isteğe bağlı)
        // Eğer bunları da kullanacaksan SQL'e ALTER TABLE ile eklemelisin:
        // public string? Brand { get; set; } 
        // public string? Capacity { get; set; }
    }
}