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

            // Herhangi bir tabloda seed data varsa tamamen atla
            if (await ctx.Suppliers.AnyAsync()  ||
                await ctx.BagTypes.AnyAsync()   ||
                await ctx.Warehouses.AnyAsync() ||
                await ctx.Products.AnyAsync())
                return;

            var now = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Unspecified);
            const string sys = "SISTEM";

            // ── 1. Depolar ───────────────────────────────────────────────────────
            var dep1 = new Warehouse { Name = "Hammadde Deposu", Code = "DEP-01", IsActive = true, CreatedBy = sys, CreatedDate = now };
            var dep2 = new Warehouse { Name = "Mamul Deposu",    Code = "DEP-02", IsActive = true, CreatedBy = sys, CreatedDate = now };
            ctx.Warehouses.AddRange(dep1, dep2);

            // ── 2. Torba Tipleri ─────────────────────────────────────────────────
            ctx.BagTypes.AddRange(
                new BagType { Name = "Flat",           Code = "TT-001", Description = "Edeka WLL2s/6c Flat Handle Bag 320x160x450 20664 FSC Recycled 100%",    IsActive = true, CreatedBy = sys, CreatedDate = now },
                new BagType { Name = "Twisted",          Code = "TT-002", Description = "Le Pain White Twisted Handle 320x140x425 FSC MIX Credit",   IsActive = true, CreatedBy = sys, CreatedDate = now },
                new BagType { Name = "SOS",  Code = "TT-003", Description = "Taste the Joy Brown Bag 80 gsm 250x135x300 QP-0004 ", IsActive = true, CreatedBy = sys, CreatedDate = now }
            );

            // ── 3. Ürün Grupları ─────────────────────────────────────────────────
            var grpHammadde = new ProductGroup { GroupCode = 1, GroupName = "Hammadde",   RangeStart = 1000, RangeEnd = 1999, IsActive = true, CreatedBy = sys, CreatedDate = now };
            var grpMamul    = new ProductGroup { GroupCode = 6, GroupName = "Mamul Urun", RangeStart = 6000, RangeEnd = 6999, IsActive = true, CreatedBy = sys, CreatedDate = now };
            ctx.ProductGroups.AddRange(grpHammadde, grpMamul);

            // ── 4. Makineler ─────────────────────────────────────────────────────
            var mak1 = new Machine { Name = "TORBA MAKINESI-1", Code = "MAK-01", Type = "Torba", IsActive = true, CreatedBy = sys, CreatedDate = now };
            var mak2 = new Machine { Name = "TORBA MAKINESI-2", Code = "MAK-02", Type = "Torba", IsActive = true, CreatedBy = sys, CreatedDate = now };
            ctx.Machines.AddRange(mak1, mak2);

            // ── 5. Kağıt Tipleri ─────────────────────────────────────────────────
            var ptKraft = new PaperType { Name = "Kraft Brown", ShortCode = "KB", IsActive = true, CreatedBy = sys, CreatedDate = now };
            var ptWhite = new PaperType { Name = "White",       ShortCode = "WH", IsActive = true, CreatedBy = sys, CreatedDate = now };
            ctx.PaperTypes.AddRange(ptKraft, ptWhite);

            // ── 6. Kağıt Renkleri ────────────────────────────────────────────────
            ctx.PaperColors.AddRange(
                new PaperColor { Name = "Kahverengi", IsActive = true, CreatedBy = sys, CreatedDate = now },
                new PaperColor { Name = "Beyaz",      IsActive = true, CreatedBy = sys, CreatedDate = now }
            );

            // ── 7. Gramajlar ─────────────────────────────────────────────────────
            var pw70 = new PaperWeight { Value = 70, Unit = "gr", IsActive = true };
            var pw80 = new PaperWeight { Value = 80, Unit = "gr", IsActive = true };
            var pw90 = new PaperWeight { Value = 90, Unit = "gr", IsActive = true };
            ctx.PaperWeights.AddRange(pw70, pw80, pw90);

            // ── 8. Kağıt Enleri ──────────────────────────────────────────────────
            var en920  = new PaperWidth { Code = "920",  Value = 920,  Unit = "mm", IsActive = true, CreatedBy = sys, CreatedDate = now };
            var en1040 = new PaperWidth { Code = "1040", Value = 1040, Unit = "mm", IsActive = true, CreatedBy = sys, CreatedDate = now };
            var en1080 = new PaperWidth { Code = "1080", Value = 1080, Unit = "mm", IsActive = true, CreatedBy = sys, CreatedDate = now };
            ctx.PaperWidths.AddRange(en920, en1040, en1080);

            await ctx.SaveChangesAsync(); // Depolar, BagTypes, Gruplar, Makineler, Kağıt meta

            // ── 9. Tedarikçiler ──────────────────────────────────────────────────
            var sup1 = new Supplier
            {
                SupplierCode = "TED-001", Name = "Kraft Kagit San. A.S.",
                FscCode = "FSC-C123456", FscExpiryDate = new DateTime(2026, 12, 31),
                ContactPerson = "Mehmet Yilmaz", Phone = "02121111111", Email = "info@kraftkagit.com",
                Address = "Ikitelli OSB Matbaacilar Sitesi A Blok No:1", City = "Istanbul",
                TaxNumber = "1112223334", TaxOffice = "Ikitelli",
                IsFscActive = true, IsActive = true, CreatedBy = sys, CreatedDate = now
            };
            var sup2 = new Supplier
            {
                SupplierCode = "TED-002", Name = "Seluloz Ticaret Ltd.",
                FscCode = "FSC-C789012", FscExpiryDate = new DateTime(2026, 6, 15),
                ContactPerson = "Ayse Demir", Phone = "02122222222", Email = "info@seluloz.com",
                Address = "Dudullu OSB 1. Cadde No:25", City = "Istanbul",
                TaxNumber = "5556667778", TaxOffice = "Umraniye",
                IsFscActive = true, IsActive = true, CreatedBy = sys, CreatedDate = now
            };
            ctx.Suppliers.AddRange(sup1, sup2);

            // ── 10. Müşteriler ───────────────────────────────────────────────────
            var cust1 = new Customer
            {
                CustomerCode = "MHS-001", Name = "Cimento Fabrikasi A.S.",
                TaxNumber = "1234567890", TaxOffice = "Kadikoy",
                Address = "Organize Sanayi Bolgesi No:5", City = "Istanbul",
                Email = "info@cimentofab.com", Phone = "02161111111",
                FscLicenseCode = "FSC-LIC-2026-001",
                IsFscActive = true, FscExpiryDate = new DateTime(2027, 3, 31),
                IsActive = true, CreatedBy = sys, CreatedDate = now
            };
            var cust2 = new Customer
            {
                CustomerCode = "MHS-002", Name = "Tarim Urunleri Ltd.",
                TaxNumber = "9876543210", TaxOffice = "Umraniye",
                Address = "Ataturk Cad. No:12", City = "Istanbul",
                Email = "info@tarimurunleri.com", Phone = "02162222222",
                FscLicenseCode = "", IsFscActive = false,
                IsActive = true, CreatedBy = sys, CreatedDate = now
            };
            var cust3 = new Customer
            {
                CustomerCode = "MHS-003", Name = "Kraft Tekstil San.",
                TaxNumber = "5555555555", TaxOffice = "Sisli",
                Address = "Tekstil Kenti B Blok No:8", City = "Istanbul",
                Email = "info@krafttekstil.com", Phone = "02123333333",
                FscLicenseCode = "FSC-LIC-2025-099",
                IsFscActive = true, FscExpiryDate = new DateTime(2026, 9, 30),
                IsActive = true, CreatedBy = sys, CreatedDate = now
            };
            ctx.Customers.AddRange(cust1, cust2, cust3);

            // ── 11. Ürünler ──────────────────────────────────────────────────────
            // FscType IDs: 1=FSC-100, 2=FSC-MIX (migration HasData ile seeded)
            var prod1 = new Product
            {
                ProductCode = "HM-001", ProductName = "Kraft Kagit 80gr Bobin",
                Unit = "Kg", ProductGroup = grpHammadde, FscTypeId = 2, PaperType = ptKraft,
                PaperWeight = pw80, PaperWidth = en1080, Supplier = sup1,
                IsActive = true, CreatedBy = sys, CreatedDate = now
            };
            var prod2 = new Product
            {
                ProductCode = "HM-002", ProductName = "Kraft Kagit 90gr Bobin",
                Unit = "Kg", ProductGroup = grpHammadde, FscTypeId = 1, PaperType = ptKraft,
                PaperWeight = pw90, PaperWidth = en1040, Supplier = sup2,
                IsActive = true, CreatedBy = sys, CreatedDate = now
            };
            var prod3 = new Product
            {
                ProductCode = "MM-001", ProductName = "Kraft Cuvali 50Kg",
                Unit = "Adet", ProductGroup = grpMamul, FscTypeId = 2, PaperType = ptKraft,
                PaperWeight = pw80, PaperWidth = en1080,
                IsActive = true, CreatedBy = sys, CreatedDate = now
            };
            var prod4 = new Product
            {
                ProductCode = "MM-002", ProductName = "Ventil Torba 25Kg",
                Unit = "Adet", ProductGroup = grpMamul, FscTypeId = 2, PaperType = ptKraft,
                PaperWeight = pw90, PaperWidth = en1040,
                IsActive = true, CreatedBy = sys, CreatedDate = now
            };
            ctx.Products.AddRange(prod1, prod2, prod3, prod4);

            // ── 11b. Ürün Reçeteleri (BOM) ──────────────────────────────────────
            ctx.ProductRecipes.AddRange(
                new ProductRecipe
                {
                    ParentProduct = prod3, ChildProduct = prod1,
                    StandardQuantity = 0.082m, Unit = "kg",
                    IsActive = true, CreatedBy = sys, CreatedDate = now
                },
                new ProductRecipe
                {
                    ParentProduct = prod4, ChildProduct = prod2,
                    StandardQuantity = 0.065m, Unit = "kg",
                    IsActive = true, CreatedBy = sys, CreatedDate = now
                }
            );

            await ctx.SaveChangesAsync(); // Tedarikçiler, Müşteriler, Ürünler, Reçeteler

            // ── 12. FSC Lotları ──────────────────────────────────────────────────
            var lot1 = new FscLot
            {
                PartiNo = "L2026-001", Supplier = sup1, FscTypeId = 2, Product = prod1,
                ArrivalDate = new DateTime(2026, 2, 10),
                InvoiceNo = "FAT-2026-0142", DispatchNo = "IRS-2026-0088",
                TruckPlate = "34 ABC 001", InvoiceAmount = 125000, Currency = "TRY",
                InvoicePdfPath  = "uploads/invoices/2026/FAT-2026-0142.pdf",
                DispatchPdfPath = "uploads/dispatches/2026/IRS-2026-0088.pdf",
                Notes = "Ilk parti teslimat",
                CreatedBy = sys, CreatedDate = now
            };
            var lot2 = new FscLot
            {
                PartiNo = "L2026-002", Supplier = sup2, FscTypeId = 1, Product = prod2,
                ArrivalDate = new DateTime(2026, 3, 5),
                InvoiceNo = "FAT-2026-0201", DispatchNo = "IRS-2026-0104",
                TruckPlate = "34 XYZ 002", InvoiceAmount = 98000, Currency = "TRY",
                InvoicePdfPath  = "uploads/invoices/2026/FAT-2026-0201.pdf",
                DispatchPdfPath = "uploads/dispatches/2026/IRS-2026-0104.pdf",
                Notes = "FSC-100 parti",
                CreatedBy = sys, CreatedDate = now
            };
            ctx.FscLots.AddRange(lot1, lot2);

            // ── 13. FSC Seriler (Bobinler) ───────────────────────────────────────
            var ser1 = new FscSerial { Lot = lot1, SerialNo = "S2026-001-01", InitialWeight = 2500, CurrentWeight = 800,  IsOpeningStock = false, CreatedBy = sys, CreatedDate = now };
            var ser2 = new FscSerial { Lot = lot1, SerialNo = "S2026-001-02", InitialWeight = 2500, CurrentWeight = 0,    IsOpeningStock = false, CreatedBy = sys, CreatedDate = now };
            var ser3 = new FscSerial { Lot = lot1, SerialNo = "S2026-001-03", InitialWeight = 2500, CurrentWeight = 2500, IsOpeningStock = false, CreatedBy = sys, CreatedDate = now };
            var ser4 = new FscSerial { Lot = lot1, SerialNo = "S2026-001-04", InitialWeight = 2500, CurrentWeight = 1200, IsOpeningStock = false, CreatedBy = sys, CreatedDate = now };
            var ser5 = new FscSerial { Lot = lot2, SerialNo = "S2026-002-01", InitialWeight = 3000, CurrentWeight = 3000, IsOpeningStock = false, CreatedBy = sys, CreatedDate = now };
            var ser6 = new FscSerial { Lot = lot2, SerialNo = "S2026-002-02", InitialWeight = 3000, CurrentWeight = 1500, IsOpeningStock = false, CreatedBy = sys, CreatedDate = now };
            var ser7 = new FscSerial { Lot = lot2, SerialNo = "S2026-002-03", InitialWeight = 3000, CurrentWeight = 3000, IsOpeningStock = false, CreatedBy = sys, CreatedDate = now };
            ctx.FscSerials.AddRange(ser1, ser2, ser3, ser4, ser5, ser6, ser7);

            await ctx.SaveChangesAsync(); // Lotlar, Seriler

            // ── 14. İş Emirleri ──────────────────────────────────────────────────
            var wo1 = new WorkOrder
            {
                WorkOrderNo = "IE2026-001", Product = prod3, Machine = mak1,
                PlannedDate = new DateTime(2026, 2, 20), CompletedDate = new DateTime(2026, 2, 25),
                PlannedQuantity = 15000, ActualQuantity = 14800,
                Status = WorkOrderStatus.Tamamlandi,
                Notes = "Cimento fabrikasi siparisi icin",
                CreatedBy = sys, CreatedDate = now
            };
            var wo2 = new WorkOrder
            {
                WorkOrderNo = "IE2026-002", Product = prod4, Machine = mak1,
                PlannedDate = new DateTime(2026, 3, 10), CompletedDate = new DateTime(2026, 3, 14),
                PlannedQuantity = 10000, ActualQuantity = 9600,
                Status = WorkOrderStatus.Tamamlandi,
                Notes = "Tarim sektoru siparisi",
                CreatedBy = sys, CreatedDate = now
            };
            var wo3 = new WorkOrder
            {
                WorkOrderNo = "IE2026-003", Product = prod3, Machine = mak2,
                PlannedDate = new DateTime(2026, 5, 20),
                PlannedQuantity = 20000, ActualQuantity = 0,
                Status = WorkOrderStatus.Uretimde,
                Notes = "Buyuk parti uretim",
                CreatedBy = sys, CreatedDate = now
            };
            ctx.WorkOrders.AddRange(wo1, wo2, wo3);

            // ── 15. Üretim Detayları ─────────────────────────────────────────────
            ctx.ProductionDetails.AddRange(
                new ProductionDetail
                {
                    WorkOrder = wo1, FscSerial = ser1, Machine = mak1,
                    ProductionDate = new DateTime(2026, 2, 22),
                    ConsumedWeight = 1700, WasteWeight = 85, ProducedQuantity = 7400,
                    CreatedBy = sys, CreatedDate = now
                },
                new ProductionDetail
                {
                    WorkOrder = wo1, FscSerial = ser2, Machine = mak1,
                    ProductionDate = new DateTime(2026, 2, 24),
                    ConsumedWeight = 2500, WasteWeight = 125, ProducedQuantity = 7400,
                    CreatedBy = sys, CreatedDate = now
                },
                new ProductionDetail
                {
                    WorkOrder = wo2, FscSerial = ser6, Machine = mak1,
                    ProductionDate = new DateTime(2026, 3, 12),
                    ConsumedWeight = 1500, WasteWeight = 75, ProducedQuantity = 9600,
                    CreatedBy = sys, CreatedDate = now
                }
            );

            await ctx.SaveChangesAsync(); // İş Emirleri, Üretim Detayları

            // ── 16. Satış Siparişleri ────────────────────────────────────────────
            var so1 = new SalesOrder
            {
                SalesOrderNo = "SIP2026-001", Customer = cust1,
                OrderDate    = new DateTime(2026, 3, 1),
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
                SalesOrderNo = "SIP2026-002", Customer = cust2,
                OrderDate    = new DateTime(2026, 3, 15),
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
                SalesOrderNo = "SIP2026-003", Customer = cust3,
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
                    SalesOrder = so1, Product = prod3, WorkOrder = wo1,
                    Quantity = 8000, UnitPrice = 7.40m, Unit = "Adet",
                    CreatedBy = sys, CreatedDate = now
                },
                new SalesOrderLine
                {
                    SalesOrder = so2, Product = prod4, WorkOrder = wo2,
                    Quantity = 4800, UnitPrice = 5.80m, Unit = "Adet",
                    CreatedBy = sys, CreatedDate = now
                },
                new SalesOrderLine
                {
                    SalesOrder = so3, Product = prod3, WorkOrder = null,
                    Quantity = 5000, UnitPrice = 7.80m, Unit = "Adet",
                    CreatedBy = sys, CreatedDate = now
                }
            );

            // ── 18. Stok Hareketleri ─────────────────────────────────────────────
            ctx.StockMovements.AddRange(
                new StockMovement
                {
                    Type = MovementType.PurchaseEntry,
                    DocumentNo = "L2026-001", DocumentDate = new DateTime(2026, 2, 10),
                    Product = prod1, Quantity = 10000, Unit = "Kg",
                    ToWarehouseId = dep1.Id,
                    PlateNumber = "", DeliveryAddress = "",
                    Description = "Lot 1 hammadde girisi",
                    CreatedBy = sys, CreatedDate = now
                },
                new StockMovement
                {
                    Type = MovementType.PurchaseEntry,
                    DocumentNo = "L2026-002", DocumentDate = new DateTime(2026, 3, 5),
                    Product = prod2, Quantity = 9000, Unit = "Kg",
                    ToWarehouseId = dep1.Id,
                    PlateNumber = "", DeliveryAddress = "",
                    Description = "Lot 2 hammadde girisi",
                    CreatedBy = sys, CreatedDate = now
                },
                new StockMovement
                {
                    Type = MovementType.ProductionEntry,
                    DocumentNo = "IE2026-001", DocumentDate = new DateTime(2026, 2, 25),
                    Product = prod3, Quantity = 14800, Unit = "Adet",
                    ToWarehouseId = dep2.Id, WorkOrder = wo1,
                    PlateNumber = "", DeliveryAddress = "",
                    Description = "Uretim ciktisi - Kraft Cuvali 50Kg",
                    CreatedBy = sys, CreatedDate = now
                },
                new StockMovement
                {
                    Type = MovementType.ProductionEntry,
                    DocumentNo = "IE2026-002", DocumentDate = new DateTime(2026, 3, 14),
                    Product = prod4, Quantity = 9600, Unit = "Adet",
                    ToWarehouseId = dep2.Id, WorkOrder = wo2,
                    PlateNumber = "", DeliveryAddress = "",
                    Description = "Uretim ciktisi - Ventil Torba 25Kg",
                    CreatedBy = sys, CreatedDate = now
                },
                new StockMovement
                {
                    Type = MovementType.SalesDispatch,
                    DocumentNo = "SIP2026-001", DocumentDate = new DateTime(2026, 3, 5),
                    Product = prod3, Quantity = 8000, Unit = "Adet",
                    Customer = cust1, PlateNumber = "34 DEF 123",
                    DeliveryAddress = "Organize Sanayi Bolgesi No:5 Istanbul",
                    WorkOrder = wo1,
                    Description = "Sevkiyat: Cimento Fabrikasi A.S.",
                    CreatedBy = sys, CreatedDate = now
                },
                new StockMovement
                {
                    Type = MovementType.SalesDispatch,
                    DocumentNo = "SIP2026-002", DocumentDate = new DateTime(2026, 3, 20),
                    Product = prod4, Quantity = 4800, Unit = "Adet",
                    Customer = cust2, PlateNumber = "34 GHI 456",
                    DeliveryAddress = "Ataturk Cad. No:12 Istanbul",
                    WorkOrder = wo2,
                    Description = "Sevkiyat: Tarim Urunleri Ltd.",
                    CreatedBy = sys, CreatedDate = now
                }
            );

            // ── 19. İmha Kayıtları ───────────────────────────────────────────────
            ctx.WasteManagements.AddRange(
                new WasteManagement
                {
                    WasteCode = "ATK2026-001", WorkOrder = wo1,
                    Category = WasteCategory.KesimArtigi,
                    Description = "Kenar kesim artiklari",
                    Quantity = 85, Unit = "kg",
                    DisposalDate = new DateTime(2026, 2, 25),
                    DisposalMethod = "Geri Donusum", DisposedBy = "Depo Sorumlusu",
                    Notes = "IE2026-001 kesim artigi",
                    CreatedBy = sys, CreatedDate = now
                },
                new WasteManagement
                {
                    WasteCode = "ATK2026-002", WorkOrder = wo1,
                    Category = WasteCategory.BaskiArtigi,
                    Description = "Baski hatalari ve baski artigi",
                    Quantity = 40, Unit = "kg",
                    DisposalDate = new DateTime(2026, 2, 25),
                    DisposalMethod = "Geri Donusum", DisposedBy = "Uretim Sorumlusu",
                    CreatedBy = sys, CreatedDate = now
                },
                new WasteManagement
                {
                    WasteCode = "ATK2026-003", WorkOrder = wo2,
                    Category = WasteCategory.MakineHatasi,
                    Description = "Makine kalibrasyonu sirasinda cikan fire",
                    Quantity = 75, Unit = "kg",
                    DisposalDate = new DateTime(2026, 3, 12),
                    DisposalMethod = "Geri Donusum", DisposedBy = "Makine Operatoru",
                    Notes = "IE2026-002 makine hatasi",
                    CreatedBy = sys, CreatedDate = now
                }
            );

            await ctx.SaveChangesAsync(); // Satış kalemleri, Stok hareketleri, İmha
        }
    }
}
