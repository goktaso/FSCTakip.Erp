namespace FSCTakip.Core.Entities
{
    /// <summary>
    /// Birim dönüşüm parametresi. Örn: 1 MT (metre) BURGU SAP = 0.0045 KG.
    /// Her zaman hedef birim KG'dir; sistem içi stok/bakiye hesapları KG üzerinden yapılır.
    /// </summary>
    public class UnitConversion : BaseEntity
    {
        /// <summary>Kaynak birim (örn: MT, ADET, M2)</summary>
        public string FromUnit { get; set; } = "";

        /// <summary>Hedef birim — şimdilik her zaman KG</summary>
        public string ToUnit { get; set; } = "KG";

        /// <summary>Çarpan: KG = FromUnit_Miktarı × Factor</summary>
        public decimal Factor { get; set; }

        /// <summary>Tanım açıklaması (örn: BURGU SAP metre → kg)</summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Grup bazlı kapsam (null = tüm gruplar).
        /// Daha dar kapsam (ProductId) her zaman daha geniş kapsamı (ProductGroupId / null) ezer.
        /// </summary>
        public int? ProductGroupId { get; set; }
        public virtual ProductGroup? ProductGroup { get; set; }

        /// <summary>Ürün bazlı kapsam (null = tüm ürünler bu birimde)</summary>
        public int? ProductId { get; set; }
        public virtual Product? Product { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
