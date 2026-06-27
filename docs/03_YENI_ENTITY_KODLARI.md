# Yeni Entity Kodları ve Migration Adımları

Bu dosya Faz 1-4 için eklenmesi gereken entity'leri içerir.
Kodları direkt olarak ilgili dosyalara ekleyip migration oluşturabilirsiniz.

---

## 1. FAZ 1 — İşlem Modülü Entity'leri

### FscLot Güncellemesi
`FSCTakip.Core/Entities/FscLot.cs` dosyasına aşağıdaki alanları ekle:

```csharp
// Mevcut alanların altına ekle
public decimal TotalWeight { get; set; }
public int SerialCount { get; set; }
public DateTime ArrivalDate { get; set; } = DateTime.Now;
public string? TruckPlate { get; set; }
public decimal? InvoiceAmount { get; set; }
public string? Currency { get; set; } = "TRY";
public string? Notes { get; set; }
```

### WorkOrder Güncellemesi
`FSCTakip.Core/Entities/WorkOrder.cs` dosyasına ekle:

```csharp
public DateTime PlannedDate { get; set; } = DateTime.Now;
public DateTime? StartDate { get; set; }
public DateTime? EndDate { get; set; }
public string Status { get; set; } = "Bekliyor"; // Bekliyor, DevamEdiyor, Tamamlandi, Iptal
public string? Notes { get; set; }
public int? CustomerId { get; set; }
public Customer? Customer { get; set; }
```

### Yeni: SalesOrder.cs
`FSCTakip.Core/Entities/SalesOrder.cs` dosyası oluştur:

```csharp
using FSCTakip.Core.Entities;

namespace FSCTakip.Core.Entities
{
    public class SalesOrder : BaseEntity
    {
        public string SalesOrderNo { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime? ShipmentDate { get; set; }
        public string Status { get; set; } = "Bekliyor";
        public string? DispatchNo { get; set; }
        public string? InvoiceNo { get; set; }
        public string? DispatchPdfPath { get; set; }
        public string? InvoicePdfPath { get; set; }
        public string? TruckPlate { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? Notes { get; set; }
        public bool IsFscRequired { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? Currency { get; set; } = "TRY";

        public Customer Customer { get; set; } = null!;
        public ICollection<SalesOrderLine> Lines { get; set; } = new List<SalesOrderLine>();
    }

    public class SalesOrderLine : BaseEntity
    {
        public int SalesOrderId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "ADET";
        public decimal? UnitPrice { get; set; }
        public decimal? TotalPrice { get; set; }
        public int? WorkOrderId { get; set; }
        public string? Notes { get; set; }

        public SalesOrder SalesOrder { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public WorkOrder? WorkOrder { get; set; }
    }
}
```

### Yeni: Document.cs
`FSCTakip.Core/Entities/Document.cs` dosyası oluştur:

```csharp
namespace FSCTakip.Core.Entities
{
    public class Document : BaseEntity
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty; // Invoice, Dispatch, Production, Other
        public string ReferenceType { get; set; } = string.Empty; // FscLot, SalesOrder, WorkOrder
        public int ReferenceId { get; set; }
        public long FileSizeBytes { get; set; }
        public string ContentType { get; set; } = "application/pdf";
        public string? Description { get; set; }
    }
}
```

---

## 2. FAZ 4 — ETL Entity'leri

### Yeni: ErpConnection.cs
`FSCTakip.Core/Entities/Etl/ErpConnection.cs`:

```csharp
namespace FSCTakip.Core.Entities.Etl
{
    public class ErpConnection : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string ErpType { get; set; } = string.Empty; // Logo, SAP, Dynamics, Mikro, Custom, Excel
        public string ConnectionMethod { get; set; } = string.Empty; // SqlServer, RestApi, ExcelImport

        // SQL Server bağlantısı
        public string? SqlServer { get; set; }
        public string? SqlDatabase { get; set; }
        public string? SqlUsername { get; set; }
        public string? SqlPasswordEncrypted { get; set; }
        public string? SqlFirmaNo { get; set; } // Logo için firma numarası

        // REST API
        public string? ApiBaseUrl { get; set; }
        public string? ApiKey { get; set; }
        public string? ApiSecretEncrypted { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime? LastSyncDate { get; set; }
        public string? LastSyncStatus { get; set; }

        public ICollection<ErpSyncLog> SyncLogs { get; set; } = new List<ErpSyncLog>();
        public ICollection<ErpFieldMapping> FieldMappings { get; set; } = new List<ErpFieldMapping>();
    }
}
```

### Yeni: ErpSyncLog.cs
`FSCTakip.Core/Entities/Etl/ErpSyncLog.cs`:

```csharp
namespace FSCTakip.Core.Entities.Etl
{
    public class ErpSyncLog : BaseEntity
    {
        public int ErpConnectionId { get; set; }
        public string SyncType { get; set; } = "Incremental"; // Full, Incremental
        public string EntityType { get; set; } = string.Empty;
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = "Çalışıyor"; // Çalışıyor, Başarılı, Hatalı
        public int RecordsRead { get; set; }
        public int RecordsInserted { get; set; }
        public int RecordsUpdated { get; set; }
        public int RecordsFailed { get; set; }
        public string? ErrorDetails { get; set; }
        public string? TriggeredBy { get; set; } = "Otomatik";

        public ErpConnection ErpConnection { get; set; } = null!;
    }
}
```

### Yeni: ErpFieldMapping.cs
`FSCTakip.Core/Entities/Etl/ErpFieldMapping.cs`:

```csharp
namespace FSCTakip.Core.Entities.Etl
{
    public class ErpFieldMapping : BaseEntity
    {
        public int ErpConnectionId { get; set; }
        public string MappingType { get; set; } = string.Empty; // Product, Supplier, Customer, Purchase, Sales
        public string ErpCode { get; set; } = string.Empty;     // ERP'deki kod
        public string FscCode { get; set; } = string.Empty;     // FSC'deki kod
        public string? Notes { get; set; }

        public ErpConnection ErpConnection { get; set; } = null!;
    }
}
```

### Yeni: ErpStagingPurchase.cs
`FSCTakip.Core/Entities/Etl/ErpStagingPurchase.cs`:

```csharp
namespace FSCTakip.Core.Entities.Etl
{
    public class ErpStagingPurchase
    {
        public int Id { get; set; }
        public int SyncLogId { get; set; }

        // ERP'den gelen veriler
        public string ErpDocumentNo { get; set; } = string.Empty;
        public string ErpDocumentType { get; set; } = "Receipt";
        public DateTime DocumentDate { get; set; }
        public string ErpSupplierCode { get; set; } = string.Empty;
        public string? ErpSupplierName { get; set; }
        public string ErpProductCode { get; set; } = string.Empty;
        public string? ErpProductName { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "KG";
        public decimal? UnitWeight { get; set; }
        public decimal? TotalWeight { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? Currency { get; set; }
        public string? LotNumber { get; set; }
        public string? SerialNumber { get; set; }
        public string? WarehouseCode { get; set; }

        // Eşleştirme
        public int? MappedSupplierId { get; set; }
        public int? MappedProductId { get; set; }
        public int? MappedWarehouseId { get; set; }

        // Durum
        public string Status { get; set; } = "Bekliyor";
        public string? ValidationErrors { get; set; }
        public int? FscLotId { get; set; }
        public DateTime ImportedAt { get; set; } = DateTime.Now;
    }
}
```

### Yeni: ErpStagingSale.cs
`FSCTakip.Core/Entities/Etl/ErpStagingSale.cs`:

```csharp
namespace FSCTakip.Core.Entities.Etl
{
    public class ErpStagingSale
    {
        public int Id { get; set; }
        public int SyncLogId { get; set; }

        public string ErpDocumentNo { get; set; } = string.Empty;
        public string ErpDocumentType { get; set; } = "SalesDispatch";
        public DateTime DocumentDate { get; set; }
        public string ErpCustomerCode { get; set; } = string.Empty;
        public string? ErpCustomerName { get; set; }
        public string ErpProductCode { get; set; } = string.Empty;
        public string? ErpProductName { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "ADET";
        public decimal? UnitPrice { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? Currency { get; set; }
        public string? WarehouseCode { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? TruckPlate { get; set; }

        public int? MappedCustomerId { get; set; }
        public int? MappedProductId { get; set; }

        public string Status { get; set; } = "Bekliyor";
        public string? ValidationErrors { get; set; }
        public int? FscSalesOrderId { get; set; }
        public DateTime ImportedAt { get; set; } = DateTime.Now;
    }
}
```

---

## 3. AppDbContext Güncellemeleri

`FSCTakip.DataAccess/Data/AppDbContext.cs` dosyasına ekle:

```csharp
// Mevcut DbSet'lerin altına:
public DbSet<SalesOrder> SalesOrders { get; set; }
public DbSet<SalesOrderLine> SalesOrderLines { get; set; }
public DbSet<Document> Documents { get; set; }

// ETL tabloları
public DbSet<ErpConnection> ErpConnections { get; set; }
public DbSet<ErpSyncLog> ErpSyncLogs { get; set; }
public DbSet<ErpFieldMapping> ErpFieldMappings { get; set; }
public DbSet<ErpStagingPurchase> ErpStagingPurchases { get; set; }
public DbSet<ErpStagingSale> ErpStagingSales { get; set; }

// OnModelCreating içine ekle:
modelBuilder.Entity<SalesOrderLine>()
    .HasOne(l => l.SalesOrder)
    .WithMany(o => o.Lines)
    .HasForeignKey(l => l.SalesOrderId)
    .OnDelete(DeleteBehavior.Cascade);

modelBuilder.Entity<SalesOrder>()
    .HasOne(o => o.Customer)
    .WithMany()
    .HasForeignKey(o => o.CustomerId)
    .OnDelete(DeleteBehavior.Restrict);

// Staging tablolarında primary key belirleme (BaseEntity'den türemiyorlar)
modelBuilder.Entity<ErpStagingPurchase>().HasKey(x => x.Id);
modelBuilder.Entity<ErpStagingSale>().HasKey(x => x.Id);
```

---

## 4. Migration Adımları

```bash
# Tüm yeni entity'leri ekledikten sonra:
cd FSCTakip.DataAccess
dotnet ef migrations add AddSalesOrderAndDocuments --startup-project ../FSCTakip.WebUI
dotnet ef database update --startup-project ../FSCTakip.WebUI

# ETL tabloları için ayrı migration (isteğe bağlı):
dotnet ef migrations add AddEtlTables --startup-project ../FSCTakip.WebUI
dotnet ef database update --startup-project ../FSCTakip.WebUI
```
