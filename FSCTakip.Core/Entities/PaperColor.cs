using FSCTakip.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace FSCTakip.Core.Entities
{
    public class PaperColor : BaseEntity
    {
        [Required(ErrorMessage = "Kağıt rengi adı zorunludur.")]
        [StringLength(50)]
        public string Name { get; set; } // Örn: White, Brown, Natural

        // [2026-03-01] Standartı: Pasife çekilen renkler listede gelmez
        public bool IsActive { get; set; } = true;

        // Bu renkteki ürünlere hızlı erişim için (Opsiyonel)
        public virtual ICollection<Product> Products { get; set; }
    }
}