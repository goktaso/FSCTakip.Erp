using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Zaten veri varsa atla
            if (await ctx.Suppliers.AnyAsync()) return;

            var now = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Unspecified);
            const string sys = "SISTEM";

            // ── 1. Depolar ───────────────────────────────────────────────────────
            ctx.Warehouses.AddRange(
                new Warehouse { Id = 1, Name = "Hammadde Deposu", Code = "DEP-01", IsActive = true, CreatedBy = sys, CreatedDate = now },
                new Warehouse { Id = 2, Name = "Mamul Deposu",    Code = "DEP-02", IsActive = true, CreatedBy = sys, CreatedDate = now }
            );

            // ── 2. Torba Tipleri ─────────────────────────────────────────────────
            ctx.BagTypes.AddRange(
                new BagType { Id = 1, Name = "V Kesim",           Code = "TT-001", Description = "Alt kismi V kesim torba", IsActive = true, CreatedBy = sys, CreatedDate = now },
                new BagType { Id = 2, Name = "Kare Dip",          Code = "TT-002", Description = "Alt kismi kare dip torba", IsActive = true, CreatedBy = sys, CreatedDate = now },
                new BagType { Id = 3, Name = "Koruklu Kare Dip",  Code = "TT-003", Description = "Yan koruklu kare dip torba", IsActive = true, CreatedBy = sys, CreatedDate = now }
            );

            // ── 3. Ürün Grupları ─────────────────────────────────────────────────
            ctx.ProductGroups.AddRange(
                new ProductGroup { Id = 1, GroupCode = 1, GroupName = "Hammadde",     RangeStart = 1000, RangeEnd = 1999, IsActive = true, CreatedBy = sys, CreatedDate = now },
                new ProductGroup { Id = 2, GroupCode = 6, GroupName = "Mamul Urun",   RangeStart = 6000, RangeEnd = 6999, IsActive = true, CreatedBy = sys, CreatedDate = now }
            );

            // ── 4. Makineler ─────────────────────────────────────────────────────
            ctx.Machines.AddRange(
                new Machine { Id = 1, Name = "TORBA MAKINESI-1", Code = "MAK-01", Type = "Torba", IsActive = true, CreatedBy = sys, CreatedDate = now },
                new Machine { Id = 2, Name = "TORBA MAKINESI-2", Code = "MAK-02", Type = "Torba", IsActive = true, CreatedBy = sys, CreatedDate = now }
            );

            // ── 5. Kağıt Tipleri ─────────────────────────────────────────────────
            ctx.PaperTypes.AddRange(
                new PaperType { Id = 1, Name = "Kraft Brown", ShortCode = "KB", IsActive = true, CreatedBy = sys, CreatedDate = now },
                new PaperType { Id = 2, Name = "White",       ShortCode = "WH", IsActive = true, CreatedBy = sys, CreatedDate = now }
            );

            // ── 6. Kağıt Renkleri ────────────────────────────────────────────────
            ctx.PaperColors.AddRange(
                new PaperColor { Id = 1, Name = "Kahverengi", IsActive = true, CreatedBy = sys, CreatedDate = now },
                new PaperColor { Id = 2, Name = "Beyaz",      IsActive = true, CreatedBy = sys, CreatedDate = now }
            );

            // ── 7. Gramajlar ─────────────────────────────────────────────────────
            ctx.PaperWeights.AddRange(
                new PaperWeight { Id = 1, Value = 70, Unit = "gr", IsActive = true },
                new PaperWeight { Id = 2, Value = 80, Unit = "gr", IsActive = true },
                new PaperWeight { Id = 3, Value = 90, Unit = "gr", IsActive = true }
            );

            // ── 8. Kağıt Enleri ──────────────────────────────────────────────────
            ctx.PaperWidths.AddRange(
                new PaperWidth { Id = 1, Code = "920",  Value = 920,  Unit = "mm", IsActive = true, CreatedBy = sys, CreatedDate = now },
                new PaperWidth { Id = 2, Code = "1040", Value = 1040, Unit = "mm", IsActive = true, CreatedBy = sys, CreatedDate = now },
                new PaperWidth { Id = 3, Code = "1080", Value = 1080, Unit = "mm", IsActive = true, CreatedBy = sys, CreatedDate = now }
            );

            await ctx.SaveChangesAsync();

            // ── 9. Tedarikçiler ──────────────────────────────────────────────────
            ctx.Suppliers.AddRange(
                new Supplier
                {
                    Id = 1, SupplierCode = "TED-001", Name = "Kraft Kagit San. A.S.",
                    FscCode = "FSC-C123456", FscExpiryDate = new DateTime(2026, 12, 31),
                    ContactPerson = "Mehmet Yilmaz", Phone = "02121111111", Email = "info@kraftkagit.com",
                    IsFscActive = true, IsActive = true, CreatedBy = sys, CreatedDate = now
                },
                new Supplier
                {
                    Id = 2, SupplierCode = "TED-002", Name = "Seluloz Ticaret Ltd.",
                    FscCode = "FSC-C789012", FscExpiryDate = new DateTime(2026, 6, 15),
                    ContactPerson = "Ayse Demir", Phone = "02122222222", Email = "info@seluloz.com",
                    IsFscActive = true, IsActive = true, CreatedBy = sys, CreatedDate = now
                }
            );

            // ── 10. Müşteriler ───────────────────────────────────────────────────
            ctx.Customers.AddRange(
                new Customer
                {
                    Id = 1, CustomerCode = "MHS-001", Name = "Cimento Fabrikasi A.S.",
                    TaxNumber = "1234567890", TaxOffice = "Kadikoy",
                    Address = "Organize Sanayi Bolgesi No:5", City = "Istanbul",
                    Email = "info@cimentofab.com", Phone = "02161111111",
                    FscLicenseCode = "FSC-LIC-2026-001",
                    IsFscActive = true, FscExpiryDate = new DateTime(2027, 3, 31),
                    IsActive = true, CreatedBy = sys, CreatedDate = now
                },
                new Customer
                {
                    Id = 2, CustomerCode = "MHS-002", Name = "Tarim Urunleri Ltd.",
                    TaxNumber = "9876543210", TaxOffice = "Umraniye",
                    Address = "Ataturk Cad. No:12", City = "Istanbul",
                    Email = "info@tarimurunleri.com", Phone = "02162222222",
                    FscLicenseCode = "", IsFscActive = false,
                    IsActive = true, CreatedBy = sys, CreatedDate = now
                },
                new Customer
                {
                    Id = 3, CustomerCode = "MHS-003", Name = "Kraft Tekstil San.",
                    TaxNumber = "5555555555", TaxOffice = "Sisli",
                    Address = "Tekstil Kenti B Blok No:8", City = "Istanbul",
                    Email = "info@krafttekstil.com", Phone = "02123333333",
                    FscLicenseCode = "FSC-LIC-2025-099",
                    IsFscActive = true, FscExpiryDate = new DateTime(2026, 9, 30),
                    IsActive = true, CreatedBy = sys, CreatedDate = now
                }
            );

            // ── 11. Ürünler ──────────────────────────────────────────────────────
            ctx.Products.AddRange(
                // Hammadde
                new Product
                {
                    Id = 1, ProductCode = "HM-001", ProductName = "Kraft Kagit 80gr Bobin",
                    Unit = "Kg", ProductGroupId = 1, FscTypeId = 2, PaperTypeId = 1,
                    PaperWeightId = 2, PaperWidthId = 3, SupplierId = 1,
                    IsActive = true, CreatedBy = sys, CreatedDate = now
                },
                new Product
                {
                    Id = 2, ProductCode = "HM-002", ProductName = "Kraft Kagit 90gr Bobin",
                    Unit = "Kg", ProductGroupId = 1, FscTypeId = 1, PaperTypeId = 1,
                    PaperWeightId = 3, PaperWidthId = 2, SupplierId = 2,
                    IsActive = true, CreatedBy = sys, CreatedDate = now
                },
                // Mamul
                new Product
                {
                    Id = 3, ProductCode = "MM-001", ProductName = "Kraft Cuvali 50Kg",
                    Unit = "Adet", ProductGroupId = 2, FscTypeId = 2, PaperTypeId = 1,
                    PaperWeightId = 2, PaperWidthId = 3,
                    IsActive = true, CreatedBy = sys, CreatedDate = now
                },
                new Product
                {
                    Id = 4, ProductCode = "MM-002", ProductName = "Ventil Torba 25Kg",
                    Unit = "Adet", ProductGroupId = 2, FscTypeId = 2, PaperTypeId = 1,
                    PaperWeightId = 3, PaperWidthId = 2,
                    IsActive = true, CreatedBy = sys, CreatedDate = now
                }
            );

            await ctx.SaveChangesAsync();

            // ── 12. FSC Lotları ──────────────────────────────────────────────────
            ctx.FscLots.AddRange(
                new FscLot
                {
                    Id = 1, LotNo = "L2026-001", SupplierId = 1, FscTypeId = 2, ProductId = 1,
                    ArrivalDate = new DateTime(2026, 2, 10),
                    InvoiceNo = "FAT-2026-0142", DispatchNo = "IRS-2026-0088",
                    TruckPlate = "34 ABC 001", InvoiceAmount = 125000, Currency = "TRY",
                    Notes = "Ilk parti teslimat",
                    CreatedBy = sys, CreatedDate = now
                },
                new FscLot
                {
                    Id = 2, LotNo = "L2026-002", SupplierId = 2, FscTypeId = 1, ProductId = 2,
                    ArrivalDate = new DateTime(2026, 3, 5),
                    InvoiceNo = "FAT-2026-0201", DispatchNo = "IRS-2026-0104",
                    TruckPlate = "34 XYZ 002", InvoiceAmount = 98000, Currency = "TRY",
                    Notes = "FSC-100 parti",
                    CreatedBy = sys, CreatedDate = now
                }
            );

            // ── 13. FSC Seriler (Bobinler) ───────────────────────────────────────
            ctx.FscSerials.AddRange(
                // Lot 1 — 4 seri
                new FscSerial { Id = 1, LotId = 1, SerialNo = "S2026-001-01", InitialWeight = 2500, CurrentWeight = 800,  IsOpeningStock = false, CreatedBy = sys, CreatedDate = now },
                new FscSerial { Id = 2, LotId = 1, SerialNo = "S2026-001-02", InitialWeight = 2500, CurrentWeight = 0,    IsOpeningStock = false, CreatedBy = sys, CreatedDate = now },
                new FscSerial { Id = 3, LotId = 1, SerialNo = "S2026-001-03", InitialWeight = 2500, CurrentWeight = 2500, IsOpeningStock = false, CreatedBy = sys, CreatedDate = now },
                new FscSerial { Id = 4, LotId = 1, SerialNo = "S2026-001-04", InitialWeight = 2500, CurrentWeight = 1200, IsOpeningStock = false, CreatedBy = sys, CreatedDate = now },
                // Lot 2 — 3 seri
                new FscSerial { Id = 5, LotId = 2, SerialNo = "S2026-002-01", InitialWeight = 3000, CurrentWeight = 3000, IsOpeningStock = false, CreatedBy = sys, CreatedDate = now },
                new FscSerial { Id = 6, LotId = 2, SerialNo = "S2026-002-02", InitialWeight = 3000, CurrentWeight = 1500, IsOpeningStock = false, CreatedBy = sys, CreatedDate = now },
                new FscSerial { Id = 7, LotId = 2, SerialNo = "S2026-002-03", InitialWeight = 3000, CurrentWeight = 3000, IsOpeningStock = false, CreatedBy = sys, CreatedDate = now }
            );

            await ctx.SaveChangesAsync();

            // ── 14. İş Emirleri ──────────────────────────────────────────────────
            ctx.WorkOrders.AddRange(
                new WorkOrder
                {
                    Id = 1, WorkOrderNo = "IE2026-001", ProductId = 3, MachineId = 1,
                    PlannedDate = new DateTime(2026, 2, 20), CompletedDate = new DateTime(2026, 2, 25),
                    PlannedQuantity = 15000, ActualQuantity = 14800,
                    Status = WorkOrderStatus.Tamamlandi,
                    Notes = "Cimento fabrikasi siparisi icin",
                    CreatedBy = sys, CreatedDate = now
                },
                new WorkOrder
                {
                    Id = 2, WorkOrderNo = "IE2026-002", ProductId = 4, MachineId = 1,
                    PlannedDate = new DateTime(2026, 3, 10), CompletedDate = new DateTime(2026, 3, 14),
                    PlannedQuantity = 10000, ActualQuantity = 9600,
                    Status = WorkOrderStatus.Tamamlandi,
                    Notes = "Tarim sektoru siparisi",
                    CreatedBy = sys, CreatedDate = now
                },
                new WorkOrder
                {
                    Id = 3, WorkOrderNo = "IE2026-003", ProductId = 3, MachineId = 2,
                    PlannedDate = new DateTime(2026, 5, 20),
                    PlannedQuantity = 20000, ActualQuantity = 0,
                    Status = WorkOrderStatus.Uretimde,
                    Notes = "Buyuk parti uretim",
                    CreatedBy = sys, CreatedDate = now
                }
            );

            // ── 15. Üretim Detayları ─────────────────────────────────────────────
            ctx.ProductionDetails.AddRange(
                // IE2026-001 uses serials 1 and 2 from Lot 1
                new ProductionDetail
                {
                    Id = 1, WorkOrderId = 1, FscSerialId = 1, MachineId = 1,
                    ProductionDate = new DateTime(2026, 2, 22),
                    ConsumedWeight = 1700, WasteWeight = 85, ProducedQuantity = 7400,
                    CreatedBy = sys, CreatedDate = now
                },
                new ProductionDetail
                {
                    Id = 2, WorkOrderId = 1, FscSerialId = 2, MachineId = 1,
                    ProductionDate = new DateTime(2026, 2, 24),
                    ConsumedWeight = 2500, WasteWeight = 125, ProducedQuantity = 7400,
                    CreatedBy = sys, CreatedDate = now
                },
                // IE2026-002 uses serial 6 from Lot 2
                new ProductionDetail
                {
                    Id = 3, WorkOrderId = 2, FscSerialId = 6, MachineId = 1,
                    ProductionDate = new DateTime(2026, 3, 12),
                    ConsumedWeight = 1500, WasteWeight = 75, ProducedQuantity = 9600,
                    CreatedBy = sys, CreatedDate = now
                }
            );

            await ctx.SaveChangesAsync();

            // ── 16. Satış Siparişleri ────────────────────────────────────────────
            var so1 = new SalesOrder
            {
                Id = 1, SalesOrderNo = "SIP2026-001", CustomerId = 1,
                OrderDate  = new DateTime(2026, 3, 1),
                DispatchDate = new DateTime(2026, 3, 5),
                DispatchNo = "SEV-2026-001", InvoiceNo = "SFT-2026-001",
                InvoiceAmount = 59200, Currency = "TRY",
                PlateNumber = "34 DEF 123", DeliveryAddress = "Organize Sanayi Bolgesi No:5 Istanbul",
                Status = SalesOrderStatus.TeslimEdildi,
                Notes = "Ilk sevkiyat",
                CreatedBy = sys, CreatedDate = now
            };
            var so2 = new SalesOrder
            {
                Id = 2, SalesOrderNo = "SIP2026-002", CustomerId = 2,
                OrderDate = new DateTime(2026, 3, 15),
                DispatchDate = new DateTime(2026, 3, 20),
                DispatchNo = "SEV-2026-002", InvoiceNo = "SFT-2026-002",
                InvoiceAmount = 27840, Currency = "TRY",
                PlateNumber = "34 GHI 456", DeliveryAddress = "Ataturk Cad. No:12 Istanbul",
                Status = SalesOrderStatus.TeslimEdildi,
                Notes = "Ventil torba sevkiyati",
                CreatedBy = sys, CreatedDate = now
            };
            var so3 = new SalesOrder
            {
                Id = 3, SalesOrderNo = "SIP2026-003", CustomerId = 3,
                OrderDate = new DateTime(2026, 5, 10),
                Status = SalesOrderStatus.Taslak,
                Currency = "TRY",
                Notes = "Hazirlanıyor",
                CreatedBy = sys, CreatedDate = now
            };
            ctx.SalesOrders.AddRange(so1, so2, so3);
            await ctx.SaveChangesAsync();

            // ── 17. Satış Kalemleri ──────────────────────────────────────────────
            ctx.SalesOrderLines.AddRange(
                new SalesOrderLine
                {
                    Id = 1, SalesOrderId = 1, ProductId = 3, WorkOrderId = 1,
                    Quantity = 8000, UnitPrice = 7.40m, Unit = "Adet",
                    CreatedBy = sys, CreatedDate = now
                },
                new SalesOrderLine
                {
                    Id = 2, SalesOrderId = 2, ProductId = 4, WorkOrderId = 2,
                    Quantity = 4800, UnitPrice = 5.80m, Unit = "Adet",
                    CreatedBy = sys, CreatedDate = now
                },
                new SalesOrderLine
                {
                    Id = 3, SalesOrderId = 3, ProductId = 3, WorkOrderId = null,
                    Quantity = 5000, UnitPrice = 7.80m, Unit = "Adet",
                    CreatedBy = sys, CreatedDate = now
                }
            );

            // ── 18. Stok Hareketleri ─────────────────────────────────────────────
            ctx.StockMovements.AddRange(
                // Hammadde girişleri
                new StockMovement
                {
                    Id = 1, Type = MovementType.PurchaseEntry,
                    DocumentNo = "L2026-001", DocumentDate = new DateTime(2026, 2, 10),
                    ProductId = 1, Quantity = 10000, Unit = "Kg",
                    ToWarehouseId = 1, Description = "Lot 1 hammadde girisi",
                    CreatedBy = sys, CreatedDate = now
                },
                new StockMovement
                {
                    Id = 2, Type = MovementType.PurchaseEntry,
                    DocumentNo = "L2026-002", DocumentDate = new DateTime(2026, 3, 5),
                    ProductId = 2, Quantity = 9000, Unit = "Kg",
                    ToWarehouseId = 1, Description = "Lot 2 hammadde girisi",
                    CreatedBy = sys, CreatedDate = now
                },
                // Üretim girişleri (mamul stoğa alınır)
                new StockMovement
                {
                    Id = 3, Type = MovementType.ProductionEntry,
                    DocumentNo = "IE2026-001", DocumentDate = new DateTime(2026, 2, 25),
                    ProductId = 3, Quantity = 14800, Unit = "Adet",
                    ToWarehouseId = 2, WorkOrderId = 1,
                    Description = "Uretim ciktisi - Kraft Cuvali 50Kg",
                    CreatedBy = sys, CreatedDate = now
                },
                new StockMovement
                {
                    Id = 4, Type = MovementType.ProductionEntry,
                    DocumentNo = "IE2026-002", DocumentDate = new DateTime(2026, 3, 14),
                    ProductId = 4, Quantity = 9600, Unit = "Adet",
                    ToWarehouseId = 2, WorkOrderId = 2,
                    Description = "Uretim ciktisi - Ventil Torba 25Kg",
                    CreatedBy = sys, CreatedDate = now
                },
                // Satış çıkışları
                new StockMovement
                {
                    Id = 5, Type = MovementType.SalesDispatch,
                    DocumentNo = "SIP2026-001", DocumentDate = new DateTime(2026, 3, 5),
                    ProductId = 3, Quantity = 8000, Unit = "Adet",
                    CustomerId = 1, PlateNumber = "34 DEF 123", WorkOrderId = 1,
                    Description = "Sevkiyat: Cimento Fabrikasi A.S.",
                    CreatedBy = sys, CreatedDate = now
                },
                new StockMovement
                {
                    Id = 6, Type = MovementType.SalesDispatch,
                    DocumentNo = "SIP2026-002", DocumentDate = new DateTime(2026, 3, 20),
                    ProductId = 4, Quantity = 4800, Unit = "Adet",
                    CustomerId = 2, PlateNumber = "34 GHI 456", WorkOrderId = 2,
                    Description = "Sevkiyat: Tarim Urunleri Ltd.",
                    CreatedBy = sys, CreatedDate = now
                }
            );

            await ctx.SaveChangesAsync();
        }
    }
}
