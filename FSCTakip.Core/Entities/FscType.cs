using System;

namespace FSCTakip.Core.Entities
{
    public class FscType : BaseEntity
    {
        // Örn: "FSC-100", "MIX-01", "NON-FSC"
        // Sistem içi benzersiz kodlama için kullanılır.
        public string Code { get; set; }

        // Örn: "FSC %100", "FSC Mix", "FSC-SIZ (Kapsam Dışı)"
        // Arayüzde (Dropdown/Liste) kullanıcının göreceği isim.
        public string Name { get; set; }

        // Örn: "Bu içerik %100 sertifikalı ormanlardan gelen lifleri temsil eder."
        // Veya "Ürün FSC sertifikası kapsamında değildir, logo basılamaz."
        // Operatörü ve denetçiyi bilgilendiren kritik alan.
        public string Description { get; set; }

        // Sertifika tipi hala geçerli mi? (Denetim kuralları değişirse pasife çekmek için)
        public bool IsActive { get; set; } = true;
    }
}