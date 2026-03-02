using System;

namespace FSCTakip.Core.Entities
{
    public class BagType : BaseEntity
    {
        // Örn: "BT-KD", "BT-VK"
        public string Code { get; set; }

        // Örn: "Kare Dip Torba", "V Kesim Kese", "Dip Takviyeli"
        public string Name { get; set; }

        // Torba tipine özel teknik notlar (Örn: "Körük payı min 5cm olmalı")
        public string Description { get; set; }

        // Üretimde aktif kullanılan bir tip mi?
        public bool IsActive { get; set; } = true;
    }
}