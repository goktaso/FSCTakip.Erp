using System;

namespace FSCTakip.Core.Entities
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }

        // Veriyi ilk giren (Operatör veya Planlamacı)
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; }

        // Planlamacı değişiklik yaparsa dolacak alanlar
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }

        // Denetçi için veriyi silmiyoruz, sadece işaretliyoruz (Soft Delete)
        public bool IsDeleted { get; set; } = false;
    }
}