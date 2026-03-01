namespace FSCTakip.Core.Entities
{
    public class ProductGroup : BaseEntity
    {
        // Örn: 1, 2, 3, 4, 5, 6
        public int GroupCode { get; set; }

        // Örn: "Hammadde", "Yarı Mamul", "Burgu Sapı"
        public string GroupName { get; set; }

        // Bu gruba ait ürünlerin kod aralığı başlangıcı (Örn: 10000)
        public int RangeStart { get; set; }

        // Bu gruba ait ürünlerin kod aralığı bitişi (Örn: 19999)
        public int RangeEnd { get; set; }
    }
}