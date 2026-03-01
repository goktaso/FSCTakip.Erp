using FSCTakip.Core.Entities;

public class Warehouse : BaseEntity
{
    public string WarehouseCode { get; set; } // WH-01
    public string WarehouseName { get; set; } // Örn: Ana Hammadde Deposu
    public bool IsActive { get; set; } = true;
}