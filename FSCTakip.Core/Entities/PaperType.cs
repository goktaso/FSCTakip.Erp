using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FSCTakip.Core.Entities // Namespace'i CORE olarak güncelledik
{
    public class PaperType : BaseEntity
    {
        [Required(ErrorMessage = "Kağıt tipi adı zorunludur.")]
        [StringLength(50)]
        public string Name { get; set; }

        public bool IsActive { get; set; } = true;

        // ICollection için yukarıya using System.Collections.Generic ekledik
        public virtual ICollection<Product> Products { get; set; }
    }
}