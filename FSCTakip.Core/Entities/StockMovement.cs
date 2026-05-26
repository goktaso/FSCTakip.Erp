using System;

namespace FSCTakip.Core.Entities
{
    // Hareketin ne olduğunu belirleyen sabit liste
    public enum MovementType
    {
        ProductionEntry = 1,   // Üretimden depoya giriş (Mamul)
        WarehouseTransfer = 2,  // Depolar arası mal kaydırma
        SalesDispatch = 3,      // Müşteriye sevkiyat (İrsaliye/Fatura)
        PurchaseEntry = 4       // Tedarikçiden hammadde girişi
    }

    public class StockMovement : BaseEntity
    {
        // Hareketin tipi (Filtreleme ve raporlama için kritik)
        public MovementType Type { get; set; }

        // --- ERP SİSTEMİ İLE EŞLEŞME (ENTEGRASYON) ---
        // Gerçek ERP'deki benzersiz ID ve Evrak No
        public int? ErpReferenceId { get; set; }
        public string DocumentNo { get; set; }
        public DateTime DocumentDate { get; set; }

        // --- ÜRÜN VE MİKTAR BİLGİSİ ---
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public decimal Quantity { get; set; }
        public string Unit { get; set; } // Adet, Kg, Ton vb.

        // --- DEPO YÖNETİMİ ---
        public int? FromWarehouseId { get; set; } // Çıkış yapılan depo
        public int? ToWarehouseId { get; set; }   // Giriş yapılan depo (Transferde kullanılır)

        // --- MÜŞTERİ VE SEVKİYAT DETAYLARI ---
        // Sadece SalesDispatch (Tip 3) durumunda doldurulur
        public int? CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        public string? PlateNumber { get; set; }   // Araç Plakası
        public string? DeliveryAddress { get; set; } // Teslim Adresi

        // --- İZLENEBİLİRLİK (FSC İÇİN) ---
        // Bu hareket hangi iş emrinden kaynaklandı?
        public int? WorkOrderId { get; set; }
        public virtual WorkOrder WorkOrder { get; set; }

        public string? Description { get; set; } // "Acil sevkiyat", "Numune" vb. notlar
    }
}