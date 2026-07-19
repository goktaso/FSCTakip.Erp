IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE TABLE [BagTypes] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(max) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_BagTypes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE TABLE [Customers] (
        [Id] int NOT NULL IDENTITY,
        [CustomerCode] nvarchar(max) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [TaxNumber] nvarchar(max) NOT NULL,
        [TaxOffice] nvarchar(max) NOT NULL,
        [Address] nvarchar(max) NOT NULL,
        [City] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [Phone] nvarchar(max) NOT NULL,
        [FscLicenseCode] nvarchar(max) NOT NULL,
        [IsFscActive] bit NOT NULL,
        [FscExpiryDate] datetime2 NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE TABLE [FscTypes] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(max) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_FscTypes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE TABLE [Machines] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Code] nvarchar(max) NOT NULL,
        [Type] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_Machines] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE TABLE [PaperColors] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_PaperColors] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE TABLE [PaperTypes] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [ShortCode] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_PaperTypes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE TABLE [PaperWeights] (
        [Id] int NOT NULL IDENTITY,
        [Value] decimal(18,2) NOT NULL,
        [Unit] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_PaperWeights] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE TABLE [PaperWidths] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(max) NOT NULL,
        [Value] decimal(18,2) NOT NULL,
        [Unit] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_PaperWidths] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE TABLE [ProductGroups] (
        [Id] int NOT NULL IDENTITY,
        [GroupCode] int NOT NULL,
        [GroupName] nvarchar(max) NOT NULL,
        [RangeStart] int NOT NULL,
        [RangeEnd] int NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_ProductGroups] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE TABLE [Suppliers] (
        [Id] int NOT NULL IDENTITY,
        [SupplierCode] nvarchar(max) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [FscCode] nvarchar(max) NOT NULL,
        [FscExpiryDate] datetime2 NULL,
        [IsFscActive] bit NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_Suppliers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE TABLE [Warehouses] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Code] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_Warehouses] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE TABLE [WasteManagements] (
        [Id] int NOT NULL IDENTITY,
        [WasteCode] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Quantity] decimal(18,2) NOT NULL,
        [DisposalDate] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_WasteManagements] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE TABLE [WorkOrders] (
        [Id] int NOT NULL IDENTITY,
        [WorkOrderNo] nvarchar(max) NOT NULL,
        [ProductCode] nvarchar(max) NOT NULL,
        [MachineId] nvarchar(max) NOT NULL,
        [PlannedQuantity] decimal(18,2) NOT NULL,
        [IsCompleted] bit NOT NULL,
        [CustomerId] int NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_WorkOrders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WorkOrders_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE TABLE [Products] (
        [Id] int NOT NULL IDENTITY,
        [PaperColorId] int NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Products_PaperColors_PaperColorId] FOREIGN KEY ([PaperColorId]) REFERENCES [PaperColors] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE TABLE [FscLots] (
        [Id] int NOT NULL IDENTITY,
        [LotNo] nvarchar(max) NOT NULL,
        [FscTypeId] int NOT NULL,
        [SupplierId] int NOT NULL,
        [InvoiceNo] nvarchar(max) NOT NULL,
        [DispatchNo] nvarchar(max) NOT NULL,
        [InvoicePdfPath] nvarchar(max) NOT NULL,
        [DispatchPdfPath] nvarchar(max) NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_FscLots] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FscLots_FscTypes_FscTypeId] FOREIGN KEY ([FscTypeId]) REFERENCES [FscTypes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_FscLots_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE TABLE [ProductRecipes] (
        [Id] int NOT NULL IDENTITY,
        [ParentProductId] int NOT NULL,
        [ChildProductId] int NOT NULL,
        [StandardQuantity] decimal(18,2) NOT NULL,
        [Unit] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_ProductRecipes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductRecipes_Products_ChildProductId] FOREIGN KEY ([ChildProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductRecipes_Products_ParentProductId] FOREIGN KEY ([ParentProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE TABLE [StockMovements] (
        [Id] int NOT NULL IDENTITY,
        [Type] int NOT NULL,
        [ErpReferenceId] int NOT NULL,
        [DocumentNo] nvarchar(max) NOT NULL,
        [DocumentDate] datetime2 NOT NULL,
        [ProductId] int NOT NULL,
        [Quantity] decimal(18,2) NOT NULL,
        [Unit] nvarchar(max) NOT NULL,
        [FromWarehouseId] int NULL,
        [ToWarehouseId] int NULL,
        [CustomerId] int NULL,
        [PlateNumber] nvarchar(max) NOT NULL,
        [DeliveryAddress] nvarchar(max) NOT NULL,
        [WorkOrderId] int NULL,
        [Description] nvarchar(max) NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_StockMovements] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StockMovements_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]),
        CONSTRAINT [FK_StockMovements_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_StockMovements_WorkOrders_WorkOrderId] FOREIGN KEY ([WorkOrderId]) REFERENCES [WorkOrders] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE TABLE [FscSerials] (
        [Id] int NOT NULL IDENTITY,
        [LotId] int NOT NULL,
        [SerialNo] nvarchar(max) NOT NULL,
        [InitialWeight] decimal(18,2) NOT NULL,
        [CurrentWeight] decimal(18,2) NOT NULL,
        [IsOpeningStock] bit NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_FscSerials] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FscSerials_FscLots_LotId] FOREIGN KEY ([LotId]) REFERENCES [FscLots] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE TABLE [ProductionDetails] (
        [Id] int NOT NULL IDENTITY,
        [WorkOrderId] int NOT NULL,
        [FscSerialId] int NOT NULL,
        [ProductionDate] datetime2 NOT NULL,
        [MachineId] int NOT NULL,
        [UsedIn] int NOT NULL,
        [ConsumedWeight] decimal(18,2) NOT NULL,
        [WasteWeight] decimal(18,2) NOT NULL,
        [ProducedQuantity] decimal(18,2) NOT NULL,
        [ConversionRate] decimal(18,2) NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_ProductionDetails] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductionDetails_FscSerials_FscSerialId] FOREIGN KEY ([FscSerialId]) REFERENCES [FscSerials] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ProductionDetails_Machines_MachineId] FOREIGN KEY ([MachineId]) REFERENCES [Machines] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ProductionDetails_WorkOrders_WorkOrderId] FOREIGN KEY ([WorkOrderId]) REFERENCES [WorkOrders] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE TABLE [WorkOrderRecipes] (
        [Id] int NOT NULL IDENTITY,
        [WorkOrderId] int NOT NULL,
        [ProductId] int NOT NULL,
        [FscSerialId] int NULL,
        [PlannedQuantity] decimal(18,2) NOT NULL,
        [ActualConsumedQuantity] decimal(18,2) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_WorkOrderRecipes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WorkOrderRecipes_FscSerials_FscSerialId] FOREIGN KEY ([FscSerialId]) REFERENCES [FscSerials] ([Id]),
        CONSTRAINT [FK_WorkOrderRecipes_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_WorkOrderRecipes_WorkOrders_WorkOrderId] FOREIGN KEY ([WorkOrderId]) REFERENCES [WorkOrders] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedBy', N'CreatedDate', N'Description', N'IsActive', N'Name', N'UpdatedBy', N'UpdatedDate') AND [object_id] = OBJECT_ID(N'[FscTypes]'))
        SET IDENTITY_INSERT [FscTypes] ON;
    EXEC(N'INSERT INTO [FscTypes] ([Id], [Code], [CreatedBy], [CreatedDate], [Description], [IsActive], [Name], [UpdatedBy], [UpdatedDate])
    VALUES (1, N''FSC-100'', N''System'', ''2026-02-25T00:00:00.0000000'', N''Tamamı sertifikalı'', CAST(1 AS bit), N''FSC %100'', NULL, NULL),
    (2, N''FSC-MIX'', N''System'', ''2026-02-25T00:00:00.0000000'', N''Karışım içerik'', CAST(1 AS bit), N''FSC Mix'', NULL, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedBy', N'CreatedDate', N'Description', N'IsActive', N'Name', N'UpdatedBy', N'UpdatedDate') AND [object_id] = OBJECT_ID(N'[FscTypes]'))
        SET IDENTITY_INSERT [FscTypes] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedBy', N'CreatedDate', N'IsActive', N'Name', N'Type', N'UpdatedBy', N'UpdatedDate') AND [object_id] = OBJECT_ID(N'[Machines]'))
        SET IDENTITY_INSERT [Machines] ON;
    EXEC(N'INSERT INTO [Machines] ([Id], [Code], [CreatedBy], [CreatedDate], [IsActive], [Name], [Type], [UpdatedBy], [UpdatedDate])
    VALUES (1, N''M-01'', N''System'', ''2026-02-25T00:00:00.0000000'', CAST(1 AS bit), N''8 Renk Flexo'', N''Matbaa'', NULL, NULL),
    (2, N''K-01'', N''System'', ''2026-02-25T00:00:00.0000000'', CAST(1 AS bit), N''Kare Dip Kesim'', N''Kesim'', NULL, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedBy', N'CreatedDate', N'IsActive', N'Name', N'Type', N'UpdatedBy', N'UpdatedDate') AND [object_id] = OBJECT_ID(N'[Machines]'))
        SET IDENTITY_INSERT [Machines] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedBy', N'CreatedDate', N'IsActive', N'Name', N'ShortCode', N'UpdatedBy', N'UpdatedDate') AND [object_id] = OBJECT_ID(N'[PaperTypes]'))
        SET IDENTITY_INSERT [PaperTypes] ON;
    EXEC(N'INSERT INTO [PaperTypes] ([Id], [CreatedBy], [CreatedDate], [IsActive], [Name], [ShortCode], [UpdatedBy], [UpdatedDate])
    VALUES (1, N''System'', ''2026-02-25T00:00:00.0000000'', CAST(1 AS bit), N''Kraft Kağıt'', N''KRT'', NULL, NULL),
    (2, N''System'', ''2026-02-25T00:00:00.0000000'', CAST(1 AS bit), N''Sülfit Kağıt'', N''SLF'', NULL, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedBy', N'CreatedDate', N'IsActive', N'Name', N'ShortCode', N'UpdatedBy', N'UpdatedDate') AND [object_id] = OBJECT_ID(N'[PaperTypes]'))
        SET IDENTITY_INSERT [PaperTypes] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedBy', N'CreatedDate', N'IsActive', N'Name', N'UpdatedBy', N'UpdatedDate') AND [object_id] = OBJECT_ID(N'[Warehouses]'))
        SET IDENTITY_INSERT [Warehouses] ON;
    EXEC(N'INSERT INTO [Warehouses] ([Id], [Code], [CreatedBy], [CreatedDate], [IsActive], [Name], [UpdatedBy], [UpdatedDate])
    VALUES (1, N''DEP-01'', N''System'', ''2026-02-25T00:00:00.0000000'', CAST(1 AS bit), N''Hammadde Deposu'', NULL, NULL),
    (2, N''DEP-02'', N''System'', ''2026-02-25T00:00:00.0000000'', CAST(1 AS bit), N''Mamul Deposu'', NULL, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedBy', N'CreatedDate', N'IsActive', N'Name', N'UpdatedBy', N'UpdatedDate') AND [object_id] = OBJECT_ID(N'[Warehouses]'))
        SET IDENTITY_INSERT [Warehouses] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE INDEX [IX_FscLots_FscTypeId] ON [FscLots] ([FscTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE INDEX [IX_FscLots_SupplierId] ON [FscLots] ([SupplierId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE INDEX [IX_FscSerials_LotId] ON [FscSerials] ([LotId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE INDEX [IX_ProductionDetails_FscSerialId] ON [ProductionDetails] ([FscSerialId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE INDEX [IX_ProductionDetails_MachineId] ON [ProductionDetails] ([MachineId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE INDEX [IX_ProductionDetails_WorkOrderId] ON [ProductionDetails] ([WorkOrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE INDEX [IX_ProductRecipes_ChildProductId] ON [ProductRecipes] ([ChildProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE INDEX [IX_ProductRecipes_ParentProductId] ON [ProductRecipes] ([ParentProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE INDEX [IX_Products_PaperColorId] ON [Products] ([PaperColorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE INDEX [IX_StockMovements_CustomerId] ON [StockMovements] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE INDEX [IX_StockMovements_ProductId] ON [StockMovements] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE INDEX [IX_StockMovements_WorkOrderId] ON [StockMovements] ([WorkOrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE INDEX [IX_WorkOrderRecipes_FscSerialId] ON [WorkOrderRecipes] ([FscSerialId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE INDEX [IX_WorkOrderRecipes_ProductId] ON [WorkOrderRecipes] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE INDEX [IX_WorkOrderRecipes_WorkOrderId] ON [WorkOrderRecipes] ([WorkOrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    CREATE INDEX [IX_WorkOrders_CustomerId] ON [WorkOrders] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260302180301_Initial_Full_Setup'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260302180301_Initial_Full_Setup', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306022451_AddProductFields'
)
BEGIN
    ALTER TABLE [Products] ADD [FscTypeId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306022451_AddProductFields'
)
BEGIN
    ALTER TABLE [Products] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306022451_AddProductFields'
)
BEGIN
    ALTER TABLE [Products] ADD [PaperTypeId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306022451_AddProductFields'
)
BEGIN
    ALTER TABLE [Products] ADD [ProductCode] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306022451_AddProductFields'
)
BEGIN
    ALTER TABLE [Products] ADD [ProductGroupId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306022451_AddProductFields'
)
BEGIN
    ALTER TABLE [Products] ADD [ProductName] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306022451_AddProductFields'
)
BEGIN
    ALTER TABLE [Products] ADD [Unit] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306022451_AddProductFields'
)
BEGIN
    CREATE INDEX [IX_Products_FscTypeId] ON [Products] ([FscTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306022451_AddProductFields'
)
BEGIN
    CREATE INDEX [IX_Products_PaperTypeId] ON [Products] ([PaperTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306022451_AddProductFields'
)
BEGIN
    CREATE INDEX [IX_Products_ProductGroupId] ON [Products] ([ProductGroupId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306022451_AddProductFields'
)
BEGIN
    ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_FscTypes_FscTypeId] FOREIGN KEY ([FscTypeId]) REFERENCES [FscTypes] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306022451_AddProductFields'
)
BEGIN
    ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_PaperTypes_PaperTypeId] FOREIGN KEY ([PaperTypeId]) REFERENCES [PaperTypes] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306022451_AddProductFields'
)
BEGIN
    ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_ProductGroups_ProductGroupId] FOREIGN KEY ([ProductGroupId]) REFERENCES [ProductGroups] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260306022451_AddProductFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260306022451_AddProductFields', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307202554_AddIsActiveToProductGroup'
)
BEGIN
    EXEC(N'DELETE FROM [Machines]
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307202554_AddIsActiveToProductGroup'
)
BEGIN
    EXEC(N'DELETE FROM [Machines]
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307202554_AddIsActiveToProductGroup'
)
BEGIN
    EXEC(N'DELETE FROM [PaperTypes]
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307202554_AddIsActiveToProductGroup'
)
BEGIN
    EXEC(N'DELETE FROM [PaperTypes]
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307202554_AddIsActiveToProductGroup'
)
BEGIN
    EXEC(N'DELETE FROM [Warehouses]
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307202554_AddIsActiveToProductGroup'
)
BEGIN
    EXEC(N'DELETE FROM [Warehouses]
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307202554_AddIsActiveToProductGroup'
)
BEGIN
    ALTER TABLE [ProductGroups] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307202554_AddIsActiveToProductGroup'
)
BEGIN
    EXEC(N'UPDATE [FscTypes] SET [CreatedBy] = N''SYSTEM'', [Description] = N''TAMAMI SERTIFIKALI''
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307202554_AddIsActiveToProductGroup'
)
BEGIN
    EXEC(N'UPDATE [FscTypes] SET [CreatedBy] = N''SYSTEM'', [Description] = N''KARISIM ICERIK'', [Name] = N''FSC MIX''
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307202554_AddIsActiveToProductGroup'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260307202554_AddIsActiveToProductGroup', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307205859_AddGrammageAndWidthToProduct'
)
BEGIN
    ALTER TABLE [Products] ADD [Grammage] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307205859_AddGrammageAndWidthToProduct'
)
BEGIN
    ALTER TABLE [Products] ADD [Width] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307205859_AddGrammageAndWidthToProduct'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260307205859_AddGrammageAndWidthToProduct', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307211434_UpdateProductWithWeightAndWidth'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'Width');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Products] DROP COLUMN [Width];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307211434_UpdateProductWithWeightAndWidth'
)
BEGIN
    EXEC sp_rename N'[Products].[Grammage]', N'PaperWidthId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307211434_UpdateProductWithWeightAndWidth'
)
BEGIN
    ALTER TABLE [Products] ADD [PaperWeightId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307211434_UpdateProductWithWeightAndWidth'
)
BEGIN
    CREATE INDEX [IX_Products_PaperWeightId] ON [Products] ([PaperWeightId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307211434_UpdateProductWithWeightAndWidth'
)
BEGIN
    CREATE INDEX [IX_Products_PaperWidthId] ON [Products] ([PaperWidthId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307211434_UpdateProductWithWeightAndWidth'
)
BEGIN
    ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_PaperWeights_PaperWeightId] FOREIGN KEY ([PaperWeightId]) REFERENCES [PaperWeights] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307211434_UpdateProductWithWeightAndWidth'
)
BEGIN
    ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_PaperWidths_PaperWidthId] FOREIGN KEY ([PaperWidthId]) REFERENCES [PaperWidths] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307211434_UpdateProductWithWeightAndWidth'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260307211434_UpdateProductWithWeightAndWidth', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307214211_AddSupplierToProduct'
)
BEGIN
    ALTER TABLE [Products] ADD [SupplierId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307214211_AddSupplierToProduct'
)
BEGIN
    CREATE INDEX [IX_Products_SupplierId] ON [Products] ([SupplierId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307214211_AddSupplierToProduct'
)
BEGIN
    ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307214211_AddSupplierToProduct'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260307214211_AddSupplierToProduct', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307215927_UpdateSupplierFields'
)
BEGIN
    ALTER TABLE [Suppliers] ADD [ContactPerson] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307215927_UpdateSupplierFields'
)
BEGIN
    ALTER TABLE [Suppliers] ADD [Email] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307215927_UpdateSupplierFields'
)
BEGIN
    ALTER TABLE [Suppliers] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307215927_UpdateSupplierFields'
)
BEGIN
    ALTER TABLE [Suppliers] ADD [Phone] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260307215927_UpdateSupplierFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260307215927_UpdateSupplierFields', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517111651_AddPurchaseLotFields'
)
BEGIN
    ALTER TABLE [FscSerials] ADD [Notes] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517111651_AddPurchaseLotFields'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FscLots]') AND [c].[name] = N'InvoicePdfPath');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [FscLots] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [FscLots] ALTER COLUMN [InvoicePdfPath] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517111651_AddPurchaseLotFields'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FscLots]') AND [c].[name] = N'InvoiceNo');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [FscLots] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [FscLots] ALTER COLUMN [InvoiceNo] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517111651_AddPurchaseLotFields'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FscLots]') AND [c].[name] = N'DispatchPdfPath');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [FscLots] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [FscLots] ALTER COLUMN [DispatchPdfPath] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517111651_AddPurchaseLotFields'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FscLots]') AND [c].[name] = N'DispatchNo');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [FscLots] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [FscLots] ALTER COLUMN [DispatchNo] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517111651_AddPurchaseLotFields'
)
BEGIN
    ALTER TABLE [FscLots] ADD [ArrivalDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517111651_AddPurchaseLotFields'
)
BEGIN
    ALTER TABLE [FscLots] ADD [Currency] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517111651_AddPurchaseLotFields'
)
BEGIN
    ALTER TABLE [FscLots] ADD [InvoiceAmount] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517111651_AddPurchaseLotFields'
)
BEGIN
    ALTER TABLE [FscLots] ADD [Notes] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517111651_AddPurchaseLotFields'
)
BEGIN
    ALTER TABLE [FscLots] ADD [ProductId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517111651_AddPurchaseLotFields'
)
BEGIN
    ALTER TABLE [FscLots] ADD [TruckPlate] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517111651_AddPurchaseLotFields'
)
BEGIN
    CREATE INDEX [IX_FscLots_ProductId] ON [FscLots] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517111651_AddPurchaseLotFields'
)
BEGIN
    ALTER TABLE [FscLots] ADD CONSTRAINT [FK_FscLots_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517111651_AddPurchaseLotFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260517111651_AddPurchaseLotFields', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517120407_AddProductionModule'
)
BEGIN
    ALTER TABLE [ProductionDetails] DROP CONSTRAINT [FK_ProductionDetails_FscSerials_FscSerialId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517120407_AddProductionModule'
)
BEGIN
    ALTER TABLE [ProductionDetails] DROP CONSTRAINT [FK_ProductionDetails_Machines_MachineId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517120407_AddProductionModule'
)
BEGIN
    ALTER TABLE [ProductionDetails] DROP CONSTRAINT [FK_ProductionDetails_WorkOrders_WorkOrderId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517120407_AddProductionModule'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WorkOrders]') AND [c].[name] = N'IsCompleted');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [WorkOrders] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [WorkOrders] DROP COLUMN [IsCompleted];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517120407_AddProductionModule'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WorkOrders]') AND [c].[name] = N'ProductCode');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [WorkOrders] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [WorkOrders] DROP COLUMN [ProductCode];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517120407_AddProductionModule'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductionDetails]') AND [c].[name] = N'ConversionRate');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [ProductionDetails] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [ProductionDetails] DROP COLUMN [ConversionRate];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517120407_AddProductionModule'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductionDetails]') AND [c].[name] = N'UsedIn');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [ProductionDetails] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [ProductionDetails] DROP COLUMN [UsedIn];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517120407_AddProductionModule'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WorkOrders]') AND [c].[name] = N'MachineId');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [WorkOrders] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [WorkOrders] ALTER COLUMN [MachineId] int NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517120407_AddProductionModule'
)
BEGIN
    ALTER TABLE [WorkOrders] ADD [ActualQuantity] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517120407_AddProductionModule'
)
BEGIN
    ALTER TABLE [WorkOrders] ADD [CompletedDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517120407_AddProductionModule'
)
BEGIN
    ALTER TABLE [WorkOrders] ADD [Notes] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517120407_AddProductionModule'
)
BEGIN
    ALTER TABLE [WorkOrders] ADD [PlannedDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517120407_AddProductionModule'
)
BEGIN
    ALTER TABLE [WorkOrders] ADD [ProductId] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517120407_AddProductionModule'
)
BEGIN
    ALTER TABLE [WorkOrders] ADD [Status] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517120407_AddProductionModule'
)
BEGIN
    ALTER TABLE [ProductionDetails] ADD [Notes] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517120407_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_WorkOrders_MachineId] ON [WorkOrders] ([MachineId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517120407_AddProductionModule'
)
BEGIN
    CREATE INDEX [IX_WorkOrders_ProductId] ON [WorkOrders] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517120407_AddProductionModule'
)
BEGIN
    ALTER TABLE [ProductionDetails] ADD CONSTRAINT [FK_ProductionDetails_FscSerials_FscSerialId] FOREIGN KEY ([FscSerialId]) REFERENCES [FscSerials] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517120407_AddProductionModule'
)
BEGIN
    ALTER TABLE [ProductionDetails] ADD CONSTRAINT [FK_ProductionDetails_Machines_MachineId] FOREIGN KEY ([MachineId]) REFERENCES [Machines] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517120407_AddProductionModule'
)
BEGIN
    ALTER TABLE [ProductionDetails] ADD CONSTRAINT [FK_ProductionDetails_WorkOrders_WorkOrderId] FOREIGN KEY ([WorkOrderId]) REFERENCES [WorkOrders] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517120407_AddProductionModule'
)
BEGIN
    ALTER TABLE [WorkOrders] ADD CONSTRAINT [FK_WorkOrders_Machines_MachineId] FOREIGN KEY ([MachineId]) REFERENCES [Machines] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517120407_AddProductionModule'
)
BEGIN
    ALTER TABLE [WorkOrders] ADD CONSTRAINT [FK_WorkOrders_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517120407_AddProductionModule'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260517120407_AddProductionModule', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517170939_AddSalesOrderTables'
)
BEGIN
    CREATE TABLE [SalesOrders] (
        [Id] int NOT NULL IDENTITY,
        [SalesOrderNo] nvarchar(max) NOT NULL,
        [CustomerId] int NOT NULL,
        [OrderDate] datetime2 NOT NULL,
        [DispatchDate] datetime2 NULL,
        [DispatchNo] nvarchar(max) NULL,
        [InvoiceNo] nvarchar(max) NULL,
        [InvoiceAmount] decimal(18,2) NULL,
        [Currency] nvarchar(max) NOT NULL,
        [PlateNumber] nvarchar(max) NULL,
        [DeliveryAddress] nvarchar(max) NULL,
        [Status] int NOT NULL,
        [DispatchPdfPath] nvarchar(max) NULL,
        [InvoicePdfPath] nvarchar(max) NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_SalesOrders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SalesOrders_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517170939_AddSalesOrderTables'
)
BEGIN
    CREATE TABLE [SalesOrderLines] (
        [Id] int NOT NULL IDENTITY,
        [SalesOrderId] int NOT NULL,
        [ProductId] int NOT NULL,
        [WorkOrderId] int NULL,
        [Quantity] decimal(18,2) NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [Unit] nvarchar(max) NOT NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_SalesOrderLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SalesOrderLines_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SalesOrderLines_SalesOrders_SalesOrderId] FOREIGN KEY ([SalesOrderId]) REFERENCES [SalesOrders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SalesOrderLines_WorkOrders_WorkOrderId] FOREIGN KEY ([WorkOrderId]) REFERENCES [WorkOrders] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517170939_AddSalesOrderTables'
)
BEGIN
    CREATE INDEX [IX_SalesOrderLines_ProductId] ON [SalesOrderLines] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517170939_AddSalesOrderTables'
)
BEGIN
    CREATE INDEX [IX_SalesOrderLines_SalesOrderId] ON [SalesOrderLines] ([SalesOrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517170939_AddSalesOrderTables'
)
BEGIN
    CREATE INDEX [IX_SalesOrderLines_WorkOrderId] ON [SalesOrderLines] ([WorkOrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517170939_AddSalesOrderTables'
)
BEGIN
    CREATE INDEX [IX_SalesOrders_CustomerId] ON [SalesOrders] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517170939_AddSalesOrderTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260517170939_AddSalesOrderTables', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518172048_EnhanceWasteManagement'
)
BEGIN
    ALTER TABLE [WasteManagements] ADD [Category] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518172048_EnhanceWasteManagement'
)
BEGIN
    ALTER TABLE [WasteManagements] ADD [DisposalMethod] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518172048_EnhanceWasteManagement'
)
BEGIN
    ALTER TABLE [WasteManagements] ADD [DisposedBy] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518172048_EnhanceWasteManagement'
)
BEGIN
    ALTER TABLE [WasteManagements] ADD [Notes] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518172048_EnhanceWasteManagement'
)
BEGIN
    ALTER TABLE [WasteManagements] ADD [Unit] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518172048_EnhanceWasteManagement'
)
BEGIN
    ALTER TABLE [WasteManagements] ADD [WorkOrderId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518172048_EnhanceWasteManagement'
)
BEGIN
    CREATE INDEX [IX_WasteManagements_WorkOrderId] ON [WasteManagements] ([WorkOrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518172048_EnhanceWasteManagement'
)
BEGIN
    ALTER TABLE [WasteManagements] ADD CONSTRAINT [FK_WasteManagements_WorkOrders_WorkOrderId] FOREIGN KEY ([WorkOrderId]) REFERENCES [WorkOrders] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518172048_EnhanceWasteManagement'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260518172048_EnhanceWasteManagement', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518182052_AddEtlTables'
)
BEGIN
    CREATE TABLE [EtlConnections] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Type] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Settings] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [LastSyncAt] datetime2 NULL,
        [LastSyncStatus] nvarchar(max) NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_EtlConnections] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518182052_AddEtlTables'
)
BEGIN
    CREATE TABLE [EtlJobs] (
        [Id] int NOT NULL IDENTITY,
        [EtlConnectionId] int NULL,
        [JobType] nvarchar(max) NOT NULL,
        [Source] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [StartedAt] datetime2 NOT NULL,
        [CompletedAt] datetime2 NULL,
        [TotalRecords] int NOT NULL,
        [InsertedCount] int NOT NULL,
        [UpdatedCount] int NOT NULL,
        [SkippedCount] int NOT NULL,
        [ErrorCount] int NOT NULL,
        [SourceFile] nvarchar(max) NULL,
        [Notes] nvarchar(max) NULL,
        [ErrorDetails] nvarchar(max) NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_EtlJobs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EtlJobs_EtlConnections_EtlConnectionId] FOREIGN KEY ([EtlConnectionId]) REFERENCES [EtlConnections] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518182052_AddEtlTables'
)
BEGIN
    CREATE INDEX [IX_EtlJobs_EtlConnectionId] ON [EtlJobs] ([EtlConnectionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518182052_AddEtlTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260518182052_AddEtlTables', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518223531_AddBomComponentTracking'
)
BEGIN
    ALTER TABLE [WorkOrderRecipes] DROP CONSTRAINT [FK_WorkOrderRecipes_FscSerials_FscSerialId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518223531_AddBomComponentTracking'
)
BEGIN
    ALTER TABLE [WorkOrderRecipes] DROP CONSTRAINT [FK_WorkOrderRecipes_Products_ProductId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518223531_AddBomComponentTracking'
)
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WorkOrderRecipes]') AND [c].[name] = N'Description');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [WorkOrderRecipes] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [WorkOrderRecipes] ALTER COLUMN [Description] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518223531_AddBomComponentTracking'
)
BEGIN
    ALTER TABLE [WorkOrderRecipes] ADD [ProducedQuantity] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518223531_AddBomComponentTracking'
)
BEGIN
    ALTER TABLE [WorkOrderRecipes] ADD [WasteQuantity] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518223531_AddBomComponentTracking'
)
BEGIN
    ALTER TABLE [ProductionDetails] ADD [WorkOrderRecipeId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518223531_AddBomComponentTracking'
)
BEGIN
    CREATE INDEX [IX_ProductionDetails_WorkOrderRecipeId] ON [ProductionDetails] ([WorkOrderRecipeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518223531_AddBomComponentTracking'
)
BEGIN
    ALTER TABLE [ProductionDetails] ADD CONSTRAINT [FK_ProductionDetails_WorkOrderRecipes_WorkOrderRecipeId] FOREIGN KEY ([WorkOrderRecipeId]) REFERENCES [WorkOrderRecipes] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518223531_AddBomComponentTracking'
)
BEGIN
    ALTER TABLE [WorkOrderRecipes] ADD CONSTRAINT [FK_WorkOrderRecipes_FscSerials_FscSerialId] FOREIGN KEY ([FscSerialId]) REFERENCES [FscSerials] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518223531_AddBomComponentTracking'
)
BEGIN
    ALTER TABLE [WorkOrderRecipes] ADD CONSTRAINT [FK_WorkOrderRecipes_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518223531_AddBomComponentTracking'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260518223531_AddBomComponentTracking', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519192317_AddSupplierAddressFields'
)
BEGIN
    ALTER TABLE [Suppliers] ADD [Address] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519192317_AddSupplierAddressFields'
)
BEGIN
    ALTER TABLE [Suppliers] ADD [City] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519192317_AddSupplierAddressFields'
)
BEGIN
    ALTER TABLE [Suppliers] ADD [TaxNumber] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519192317_AddSupplierAddressFields'
)
BEGIN
    ALTER TABLE [Suppliers] ADD [TaxOffice] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260519192317_AddSupplierAddressFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260519192317_AddSupplierAddressFields', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260524120001_FixMissingExternalCodesColumns'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Suppliers') AND name = 'ExternalCode')
        ALTER TABLE [Suppliers] ADD [ExternalCode] nvarchar(100) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'ExternalCode')
        ALTER TABLE [Customers] ADD [ExternalCode] nvarchar(100) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'ExternalCode')
        ALTER TABLE [Products] ADD [ExternalCode] nvarchar(100) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SalesOrders') AND name = 'ExternalOrderNo')
        ALTER TABLE [SalesOrders] ADD [ExternalOrderNo] nvarchar(100) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WorkOrders') AND name = 'ExternalOrderNo')
        ALTER TABLE [WorkOrders] ADD [ExternalOrderNo] nvarchar(100) NULL;

END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260524120001_FixMissingExternalCodesColumns'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260524120001_FixMissingExternalCodesColumns', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260526000003_FixUnregisteredManualMigrations'
)
BEGIN

    -- 1) FscLots.LotNo -> PartiNo rename (yalnızca eski ad duruyorsa)
    IF COL_LENGTH('FscLots','PartiNo') IS NULL AND COL_LENGTH('FscLots','LotNo') IS NOT NULL
        EXEC sp_rename 'FscLots.LotNo', 'PartiNo', 'COLUMN';

    -- 2) FscSerials.LotNo (bobin bazlı lot no) yoksa ekle
    IF COL_LENGTH('FscSerials','LotNo') IS NULL
        ALTER TABLE [FscSerials] ADD [LotNo] nvarchar(max) NULL;

    -- 2b) StockMovements.QuantityKg — entity'de var (StockMovement.QuantityKg), ama hiçbir
    --     migration'a girmemiş (doğrudan SSMS ile eklenmiş). Sıfır kurulumda DbSeeder
    --     Invalid column name QuantityKg hatasıyla çöküyordu. Tam şema diff'i ile bulundu.
    IF COL_LENGTH('StockMovements','QuantityKg') IS NULL
        ALTER TABLE [StockMovements] ADD [QuantityKg] decimal(18,2) NULL;

    -- 3) FscLots.SupplierId nullable değilse: FK'yı (adı ne olursa olsun) bırak, nullable yap, FK'yı geri kur
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FscLots') AND name = 'SupplierId' AND is_nullable = 0)
    BEGIN
        DECLARE @fk sysname;
        SELECT @fk = fk.name
        FROM sys.foreign_keys fk
        JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
        JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
        WHERE fk.parent_object_id = OBJECT_ID('FscLots') AND c.name = 'SupplierId';

        IF @fk IS NOT NULL
            EXEC('ALTER TABLE [FscLots] DROP CONSTRAINT [' + @fk + ']');

        ALTER TABLE [FscLots] ALTER COLUMN [SupplierId] int NULL;

        ALTER TABLE [FscLots] ADD CONSTRAINT [FK_FscLots_Suppliers_SupplierId]
            FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]);
    END

END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260526000003_FixUnregisteredManualMigrations'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260526000003_FixUnregisteredManualMigrations', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    ALTER TABLE [FscLots] DROP CONSTRAINT [FK_FscLots_Suppliers_SupplierId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WorkOrders]') AND [c].[name] = N'ExternalOrderNo');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [WorkOrders] DROP CONSTRAINT [' + @var11 + '];');
    ALTER TABLE [WorkOrders] ALTER COLUMN [ExternalOrderNo] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    DECLARE @var12 sysname;
    SELECT @var12 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Suppliers]') AND [c].[name] = N'TaxOffice');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Suppliers] DROP CONSTRAINT [' + @var12 + '];');
    ALTER TABLE [Suppliers] ALTER COLUMN [TaxOffice] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    DECLARE @var13 sysname;
    SELECT @var13 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Suppliers]') AND [c].[name] = N'TaxNumber');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Suppliers] DROP CONSTRAINT [' + @var13 + '];');
    ALTER TABLE [Suppliers] ALTER COLUMN [TaxNumber] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    DECLARE @var14 sysname;
    SELECT @var14 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Suppliers]') AND [c].[name] = N'Phone');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Suppliers] DROP CONSTRAINT [' + @var14 + '];');
    ALTER TABLE [Suppliers] ALTER COLUMN [Phone] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    DECLARE @var15 sysname;
    SELECT @var15 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Suppliers]') AND [c].[name] = N'FscCode');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [Suppliers] DROP CONSTRAINT [' + @var15 + '];');
    ALTER TABLE [Suppliers] ALTER COLUMN [FscCode] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    DECLARE @var16 sysname;
    SELECT @var16 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Suppliers]') AND [c].[name] = N'ExternalCode');
    IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [Suppliers] DROP CONSTRAINT [' + @var16 + '];');
    ALTER TABLE [Suppliers] ALTER COLUMN [ExternalCode] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    DECLARE @var17 sysname;
    SELECT @var17 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Suppliers]') AND [c].[name] = N'Email');
    IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [Suppliers] DROP CONSTRAINT [' + @var17 + '];');
    ALTER TABLE [Suppliers] ALTER COLUMN [Email] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    DECLARE @var18 sysname;
    SELECT @var18 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Suppliers]') AND [c].[name] = N'ContactPerson');
    IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [Suppliers] DROP CONSTRAINT [' + @var18 + '];');
    ALTER TABLE [Suppliers] ALTER COLUMN [ContactPerson] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    DECLARE @var19 sysname;
    SELECT @var19 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Suppliers]') AND [c].[name] = N'City');
    IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [Suppliers] DROP CONSTRAINT [' + @var19 + '];');
    ALTER TABLE [Suppliers] ALTER COLUMN [City] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    DECLARE @var20 sysname;
    SELECT @var20 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Suppliers]') AND [c].[name] = N'Address');
    IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [Suppliers] DROP CONSTRAINT [' + @var20 + '];');
    ALTER TABLE [Suppliers] ALTER COLUMN [Address] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    DECLARE @var21 sysname;
    SELECT @var21 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StockMovements]') AND [c].[name] = N'PlateNumber');
    IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [StockMovements] DROP CONSTRAINT [' + @var21 + '];');
    ALTER TABLE [StockMovements] ALTER COLUMN [PlateNumber] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    DECLARE @var22 sysname;
    SELECT @var22 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StockMovements]') AND [c].[name] = N'ErpReferenceId');
    IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [StockMovements] DROP CONSTRAINT [' + @var22 + '];');
    ALTER TABLE [StockMovements] ALTER COLUMN [ErpReferenceId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    DECLARE @var23 sysname;
    SELECT @var23 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StockMovements]') AND [c].[name] = N'Description');
    IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [StockMovements] DROP CONSTRAINT [' + @var23 + '];');
    ALTER TABLE [StockMovements] ALTER COLUMN [Description] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    DECLARE @var24 sysname;
    SELECT @var24 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StockMovements]') AND [c].[name] = N'DeliveryAddress');
    IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [StockMovements] DROP CONSTRAINT [' + @var24 + '];');
    ALTER TABLE [StockMovements] ALTER COLUMN [DeliveryAddress] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    DECLARE @var25 sysname;
    SELECT @var25 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SalesOrders]') AND [c].[name] = N'ExternalOrderNo');
    IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [SalesOrders] DROP CONSTRAINT [' + @var25 + '];');
    ALTER TABLE [SalesOrders] ALTER COLUMN [ExternalOrderNo] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    DECLARE @var26 sysname;
    SELECT @var26 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'ExternalCode');
    IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var26 + '];');
    ALTER TABLE [Products] ALTER COLUMN [ExternalCode] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    DECLARE @var27 sysname;
    SELECT @var27 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'ExternalCode');
    IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var27 + '];');
    ALTER TABLE [Customers] ALTER COLUMN [ExternalCode] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    CREATE TABLE [AppUsers] (
        [Id] int NOT NULL IDENTITY,
        [Username] nvarchar(450) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NULL,
        [IsAdmin] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [LastLoginDate] datetime2 NULL,
        CONSTRAINT [PK_AppUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] bigint NOT NULL IDENTITY,
        [TableName] nvarchar(max) NOT NULL,
        [RecordId] int NULL,
        [Action] nvarchar(max) NOT NULL,
        [OldValues] nvarchar(max) NULL,
        [NewValues] nvarchar(max) NULL,
        [ChangedBy] nvarchar(max) NULL,
        [ChangedAt] datetime2 NOT NULL,
        [IpAddress] nvarchar(max) NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    CREATE TABLE [AuditPeriods] (
        [Id] int NOT NULL IDENTITY,
        [Year] int NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [Description] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [IsLocked] bit NOT NULL,
        [LockedAt] datetime2 NULL,
        [LockedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_AuditPeriods] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    CREATE TABLE [PermissionGroups] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_PermissionGroups] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    CREATE TABLE [PermissionModules] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(max) NOT NULL,
        [DisplayName] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [IconClass] nvarchar(max) NOT NULL,
        [SortOrder] int NOT NULL,
        CONSTRAINT [PK_PermissionModules] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    CREATE TABLE [UserGroups] (
        [UserId] int NOT NULL,
        [GroupId] int NOT NULL,
        CONSTRAINT [PK_UserGroups] PRIMARY KEY ([UserId], [GroupId]),
        CONSTRAINT [FK_UserGroups_AppUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AppUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserGroups_PermissionGroups_GroupId] FOREIGN KEY ([GroupId]) REFERENCES [PermissionGroups] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    CREATE TABLE [GroupPermissions] (
        [GroupId] int NOT NULL,
        [ModuleId] int NOT NULL,
        [CanRead] bit NOT NULL,
        [CanWrite] bit NOT NULL,
        [CanDelete] bit NOT NULL,
        CONSTRAINT [PK_GroupPermissions] PRIMARY KEY ([GroupId], [ModuleId]),
        CONSTRAINT [FK_GroupPermissions_PermissionGroups_GroupId] FOREIGN KEY ([GroupId]) REFERENCES [PermissionGroups] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_GroupPermissions_PermissionModules_ModuleId] FOREIGN KEY ([ModuleId]) REFERENCES [PermissionModules] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    CREATE TABLE [UserPermissionOverrides] (
        [UserId] int NOT NULL,
        [ModuleId] int NOT NULL,
        [CanRead] bit NULL,
        [CanWrite] bit NULL,
        [CanDelete] bit NULL,
        CONSTRAINT [PK_UserPermissionOverrides] PRIMARY KEY ([UserId], [ModuleId]),
        CONSTRAINT [FK_UserPermissionOverrides_AppUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AppUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserPermissionOverrides_PermissionModules_ModuleId] FOREIGN KEY ([ModuleId]) REFERENCES [PermissionModules] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'Description', N'DisplayName', N'IconClass', N'SortOrder') AND [object_id] = OBJECT_ID(N'[PermissionModules]'))
        SET IDENTITY_INSERT [PermissionModules] ON;
    EXEC(N'INSERT INTO [PermissionModules] ([Id], [Code], [Description], [DisplayName], [IconClass], [SortOrder])
    VALUES (1, N''PURCHASE'', NULL, N''Hammadde Girişi'', N''fas fa-boxes'', 1),
    (2, N''PRODUCTION'', NULL, N''Üretim'', N''fas fa-industry'', 2),
    (3, N''SALES'', NULL, N''Satış / Sevkiyat'', N''fas fa-truck'', 3),
    (4, N''STOCK'', NULL, N''Stok Yönetimi'', N''fas fa-warehouse'', 4),
    (5, N''CUSTOMERS'', NULL, N''Müşteriler'', N''fas fa-handshake'', 5),
    (6, N''SUPPLIERS'', NULL, N''Tedarikçiler'', N''fas fa-truck-loading'', 6),
    (7, N''PRODUCTS'', NULL, N''Ürünler'', N''fas fa-tag'', 7),
    (8, N''REPORTS'', NULL, N''Raporlar'', N''fas fa-chart-bar'', 8),
    (9, N''AUDIT_PERIOD'', NULL, N''Denetim Dönemleri'', N''fas fa-calendar-check'', 9),
    (10, N''SETTINGS'', NULL, N''Ayarlar / Tanımlar'', N''fas fa-cog'', 10),
    (11, N''ETL'', NULL, N''ERP Entegrasyon'', N''fas fa-sync-alt'', 11),
    (12, N''USERS'', NULL, N''Kullanıcı Yönetimi'', N''fas fa-users-cog'', 12)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'Description', N'DisplayName', N'IconClass', N'SortOrder') AND [object_id] = OBJECT_ID(N'[PermissionModules]'))
        SET IDENTITY_INSERT [PermissionModules] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AppUsers_Username] ON [AppUsers] ([Username]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    CREATE INDEX [IX_GroupPermissions_ModuleId] ON [GroupPermissions] ([ModuleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    CREATE INDEX [IX_UserGroups_GroupId] ON [UserGroups] ([GroupId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    CREATE INDEX [IX_UserPermissionOverrides_ModuleId] ON [UserPermissionOverrides] ([ModuleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    ALTER TABLE [FscLots] ADD CONSTRAINT [FK_FscLots_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527083623_AddRbacAndAuditLog'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260527083623_AddRbacAndAuditLog', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527110614_AddUnitConversion'
)
BEGIN
    ALTER TABLE [FscSerials] ADD [OriginalQuantity] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527110614_AddUnitConversion'
)
BEGIN
    ALTER TABLE [FscSerials] ADD [OriginalUnit] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527110614_AddUnitConversion'
)
BEGIN
    CREATE TABLE [UnitConversions] (
        [Id] int NOT NULL IDENTITY,
        [FromUnit] nvarchar(max) NOT NULL,
        [ToUnit] nvarchar(max) NOT NULL,
        [Factor] decimal(18,2) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [ProductGroupId] int NULL,
        [ProductId] int NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_UnitConversions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UnitConversions_ProductGroups_ProductGroupId] FOREIGN KEY ([ProductGroupId]) REFERENCES [ProductGroups] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_UnitConversions_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527110614_AddUnitConversion'
)
BEGIN
    CREATE INDEX [IX_UnitConversions_ProductGroupId] ON [UnitConversions] ([ProductGroupId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527110614_AddUnitConversion'
)
BEGIN
    CREATE INDEX [IX_UnitConversions_ProductId] ON [UnitConversions] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527110614_AddUnitConversion'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260527110614_AddUnitConversion', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527110919_AddProductionDetailUnitConverted'
)
BEGIN
    ALTER TABLE [ProductionDetails] ADD [UnitConverted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527110919_AddProductionDetailUnitConverted'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260527110919_AddProductionDetailUnitConverted', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260606223008_FixProductRecipeStandardQuantityScale'
)
BEGIN
    DECLARE @var28 sysname;
    SELECT @var28 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UnitConversions]') AND [c].[name] = N'Factor');
    IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [UnitConversions] DROP CONSTRAINT [' + @var28 + '];');
    ALTER TABLE [UnitConversions] ALTER COLUMN [Factor] decimal(18,7) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260606223008_FixProductRecipeStandardQuantityScale'
)
BEGIN
    DECLARE @var29 sysname;
    SELECT @var29 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductRecipes]') AND [c].[name] = N'StandardQuantity');
    IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [ProductRecipes] DROP CONSTRAINT [' + @var29 + '];');
    ALTER TABLE [ProductRecipes] ALTER COLUMN [StandardQuantity] decimal(18,6) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260606223008_FixProductRecipeStandardQuantityScale'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260606223008_FixProductRecipeStandardQuantityScale', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260614110438_LotConversionSourceFire'
)
BEGIN
    ALTER TABLE [FscLots] ADD [ConversionFireKg] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260614110438_LotConversionSourceFire'
)
BEGIN
    ALTER TABLE [FscLots] ADD [SourceSerialId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260614110438_LotConversionSourceFire'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260614110438_LotConversionSourceFire', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615202314_AddIndexesAndPrecisionFixes'
)
BEGIN
    DROP INDEX [IX_StockMovements_ProductId] ON [StockMovements];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615202314_AddIndexesAndPrecisionFixes'
)
BEGIN
    DECLARE @var30 sysname;
    SELECT @var30 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FscSerials]') AND [c].[name] = N'OriginalQuantity');
    IF @var30 IS NOT NULL EXEC(N'ALTER TABLE [FscSerials] DROP CONSTRAINT [' + @var30 + '];');
    ALTER TABLE [FscSerials] ALTER COLUMN [OriginalQuantity] decimal(18,4) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615202314_AddIndexesAndPrecisionFixes'
)
BEGIN
    DECLARE @var31 sysname;
    SELECT @var31 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FscSerials]') AND [c].[name] = N'InitialWeight');
    IF @var31 IS NOT NULL EXEC(N'ALTER TABLE [FscSerials] DROP CONSTRAINT [' + @var31 + '];');
    ALTER TABLE [FscSerials] ALTER COLUMN [InitialWeight] decimal(18,4) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615202314_AddIndexesAndPrecisionFixes'
)
BEGIN
    DECLARE @var32 sysname;
    SELECT @var32 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FscSerials]') AND [c].[name] = N'CurrentWeight');
    IF @var32 IS NOT NULL EXEC(N'ALTER TABLE [FscSerials] DROP CONSTRAINT [' + @var32 + '];');
    ALTER TABLE [FscSerials] ALTER COLUMN [CurrentWeight] decimal(18,4) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615202314_AddIndexesAndPrecisionFixes'
)
BEGIN
    CREATE INDEX [IX_StockMovements_ProductId_DocumentDate] ON [StockMovements] ([ProductId], [DocumentDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615202314_AddIndexesAndPrecisionFixes'
)
BEGIN
    CREATE INDEX [IX_FscLots_SourceSerialId] ON [FscLots] ([SourceSerialId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615202314_AddIndexesAndPrecisionFixes'
)
BEGIN
    ALTER TABLE [FscLots] ADD CONSTRAINT [FK_FscLots_FscSerials_SourceSerialId] FOREIGN KEY ([SourceSerialId]) REFERENCES [FscSerials] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615202314_AddIndexesAndPrecisionFixes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260615202314_AddIndexesAndPrecisionFixes', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615212409_AddFscDocuments'
)
BEGIN
    ALTER TABLE [FscLots] DROP CONSTRAINT [FK_FscLots_FscSerials_SourceSerialId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615212409_AddFscDocuments'
)
BEGIN
    CREATE TABLE [FscDocuments] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Category] int NOT NULL,
        [Year] int NOT NULL,
        [FileName] nvarchar(max) NOT NULL,
        [FilePath] nvarchar(max) NOT NULL,
        [FileSize] bigint NOT NULL,
        [FileExtension] nvarchar(max) NOT NULL,
        [Notes] nvarchar(max) NULL,
        [Tags] nvarchar(max) NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_FscDocuments] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615212409_AddFscDocuments'
)
BEGIN
    ALTER TABLE [FscLots] ADD CONSTRAINT [FK_FscLots_FscSerials_SourceSerialId] FOREIGN KEY ([SourceSerialId]) REFERENCES [FscSerials] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615212409_AddFscDocuments'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260615212409_AddFscDocuments', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619223329_DataIntegrity_UniqueSerial_CurrentWeightCheck'
)
BEGIN
    DROP INDEX [IX_FscSerials_LotId] ON [FscSerials];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619223329_DataIntegrity_UniqueSerial_CurrentWeightCheck'
)
BEGIN
    DECLARE @var33 sysname;
    SELECT @var33 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FscSerials]') AND [c].[name] = N'SerialNo');
    IF @var33 IS NOT NULL EXEC(N'ALTER TABLE [FscSerials] DROP CONSTRAINT [' + @var33 + '];');
    ALTER TABLE [FscSerials] ALTER COLUMN [SerialNo] nvarchar(450) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619223329_DataIntegrity_UniqueSerial_CurrentWeightCheck'
)
BEGIN
    CREATE UNIQUE INDEX [IX_FscSerials_LotId_SerialNo_Unique] ON [FscSerials] ([LotId], [SerialNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619223329_DataIntegrity_UniqueSerial_CurrentWeightCheck'
)
BEGIN
    EXEC(N'ALTER TABLE [FscSerials] ADD CONSTRAINT [CK_FscSerials_CurrentWeight] CHECK ([CurrentWeight] >= -0.001)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619223329_DataIntegrity_UniqueSerial_CurrentWeightCheck'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260619223329_DataIntegrity_UniqueSerial_CurrentWeightCheck', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260621182117_AddBilesenYeriToProductRecipe'
)
BEGIN
    ALTER TABLE [ProductRecipes] ADD [BilesenYeri] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260621182117_AddBilesenYeriToProductRecipe'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260621182117_AddBilesenYeriToProductRecipe', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630201438_AddProductionDetailAudit'
)
BEGIN
    CREATE TABLE [ProductionDetailAudits] (
        [Id] int NOT NULL IDENTITY,
        [ProductionDetailId] int NOT NULL,
        [WorkOrderId] int NOT NULL,
        [Action] nvarchar(max) NOT NULL,
        [Reason] nvarchar(max) NOT NULL,
        [OldConsumedWeight] decimal(18,2) NOT NULL,
        [OldWasteWeight] decimal(18,2) NOT NULL,
        [OldProducedQuantity] decimal(18,2) NOT NULL,
        [NewConsumedWeight] decimal(18,2) NULL,
        [NewWasteWeight] decimal(18,2) NULL,
        [NewProducedQuantity] decimal(18,2) NULL,
        [ChangedBy] nvarchar(max) NOT NULL,
        [ChangedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_ProductionDetailAudits] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630201438_AddProductionDetailAudit'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260630201438_AddProductionDetailAudit', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630204921_AddConversionAudit'
)
BEGIN
    CREATE TABLE [ConversionAudits] (
        [Id] int NOT NULL IDENTITY,
        [SerialId] int NOT NULL,
        [PartiNo] nvarchar(max) NOT NULL,
        [Action] nvarchar(max) NOT NULL,
        [Reason] nvarchar(max) NOT NULL,
        [OldTarih] datetime2 NOT NULL,
        [OldFireKg] decimal(18,2) NOT NULL,
        [NewTarih] datetime2 NULL,
        [NewFireKg] decimal(18,2) NULL,
        [ChangedBy] nvarchar(max) NOT NULL,
        [ChangedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_ConversionAudits] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260630204921_AddConversionAudit'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260630204921_AddConversionAudit', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701194729_AddSalesOrderInvoiceDate'
)
BEGIN
    ALTER TABLE [SalesOrders] ADD [InvoiceDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260701194729_AddSalesOrderInvoiceDate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260701194729_AddSalesOrderInvoiceDate', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704093714_AddCompanySettings'
)
BEGIN
    CREATE TABLE [CompanySettings] (
        [Id] int NOT NULL IDENTITY,
        [CompanyName] nvarchar(max) NOT NULL,
        [Address] nvarchar(max) NULL,
        [City] nvarchar(max) NULL,
        [TaxNumber] nvarchar(max) NULL,
        [TaxOffice] nvarchar(max) NULL,
        [Phone] nvarchar(max) NULL,
        [Email] nvarchar(max) NULL,
        [FscCocCode] nvarchar(max) NULL,
        [FscLicenseCode] nvarchar(max) NULL,
        [LogoPath] nvarchar(max) NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_CompanySettings] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704093714_AddCompanySettings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260704093714_AddCompanySettings', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183923_AddMustChangePassword'
)
BEGIN
    ALTER TABLE [AppUsers] ADD [MustChangePassword] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717183923_AddMustChangePassword'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260717183923_AddMustChangePassword', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN

                    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FscLots_SourceSerialId_Filtered' AND object_id = OBJECT_ID('FscLots'))
                        DROP INDEX IX_FscLots_SourceSerialId_Filtered ON FscLots;
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var34 sysname;
    SELECT @var34 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WorkOrders]') AND [c].[name] = N'PlannedQuantity');
    IF @var34 IS NOT NULL EXEC(N'ALTER TABLE [WorkOrders] DROP CONSTRAINT [' + @var34 + '];');
    ALTER TABLE [WorkOrders] ALTER COLUMN [PlannedQuantity] decimal(18,4) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var35 sysname;
    SELECT @var35 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WorkOrders]') AND [c].[name] = N'ActualQuantity');
    IF @var35 IS NOT NULL EXEC(N'ALTER TABLE [WorkOrders] DROP CONSTRAINT [' + @var35 + '];');
    ALTER TABLE [WorkOrders] ALTER COLUMN [ActualQuantity] decimal(18,4) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var36 sysname;
    SELECT @var36 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WorkOrderRecipes]') AND [c].[name] = N'WasteQuantity');
    IF @var36 IS NOT NULL EXEC(N'ALTER TABLE [WorkOrderRecipes] DROP CONSTRAINT [' + @var36 + '];');
    ALTER TABLE [WorkOrderRecipes] ALTER COLUMN [WasteQuantity] decimal(18,4) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var37 sysname;
    SELECT @var37 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WorkOrderRecipes]') AND [c].[name] = N'ProducedQuantity');
    IF @var37 IS NOT NULL EXEC(N'ALTER TABLE [WorkOrderRecipes] DROP CONSTRAINT [' + @var37 + '];');
    ALTER TABLE [WorkOrderRecipes] ALTER COLUMN [ProducedQuantity] decimal(18,4) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var38 sysname;
    SELECT @var38 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WorkOrderRecipes]') AND [c].[name] = N'PlannedQuantity');
    IF @var38 IS NOT NULL EXEC(N'ALTER TABLE [WorkOrderRecipes] DROP CONSTRAINT [' + @var38 + '];');
    ALTER TABLE [WorkOrderRecipes] ALTER COLUMN [PlannedQuantity] decimal(18,4) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var39 sysname;
    SELECT @var39 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WorkOrderRecipes]') AND [c].[name] = N'ActualConsumedQuantity');
    IF @var39 IS NOT NULL EXEC(N'ALTER TABLE [WorkOrderRecipes] DROP CONSTRAINT [' + @var39 + '];');
    ALTER TABLE [WorkOrderRecipes] ALTER COLUMN [ActualConsumedQuantity] decimal(18,4) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var40 sysname;
    SELECT @var40 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WasteManagements]') AND [c].[name] = N'Quantity');
    IF @var40 IS NOT NULL EXEC(N'ALTER TABLE [WasteManagements] DROP CONSTRAINT [' + @var40 + '];');
    ALTER TABLE [WasteManagements] ALTER COLUMN [Quantity] decimal(18,4) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var41 sysname;
    SELECT @var41 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StockMovements]') AND [c].[name] = N'QuantityKg');
    IF @var41 IS NOT NULL EXEC(N'ALTER TABLE [StockMovements] DROP CONSTRAINT [' + @var41 + '];');
    ALTER TABLE [StockMovements] ALTER COLUMN [QuantityKg] decimal(18,4) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var42 sysname;
    SELECT @var42 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StockMovements]') AND [c].[name] = N'Quantity');
    IF @var42 IS NOT NULL EXEC(N'ALTER TABLE [StockMovements] DROP CONSTRAINT [' + @var42 + '];');
    ALTER TABLE [StockMovements] ALTER COLUMN [Quantity] decimal(18,4) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var43 sysname;
    SELECT @var43 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SalesOrders]') AND [c].[name] = N'InvoiceAmount');
    IF @var43 IS NOT NULL EXEC(N'ALTER TABLE [SalesOrders] DROP CONSTRAINT [' + @var43 + '];');
    ALTER TABLE [SalesOrders] ALTER COLUMN [InvoiceAmount] decimal(18,4) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var44 sysname;
    SELECT @var44 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SalesOrderLines]') AND [c].[name] = N'UnitPrice');
    IF @var44 IS NOT NULL EXEC(N'ALTER TABLE [SalesOrderLines] DROP CONSTRAINT [' + @var44 + '];');
    ALTER TABLE [SalesOrderLines] ALTER COLUMN [UnitPrice] decimal(18,4) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var45 sysname;
    SELECT @var45 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SalesOrderLines]') AND [c].[name] = N'Quantity');
    IF @var45 IS NOT NULL EXEC(N'ALTER TABLE [SalesOrderLines] DROP CONSTRAINT [' + @var45 + '];');
    ALTER TABLE [SalesOrderLines] ALTER COLUMN [Quantity] decimal(18,4) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var46 sysname;
    SELECT @var46 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductionDetails]') AND [c].[name] = N'WasteWeight');
    IF @var46 IS NOT NULL EXEC(N'ALTER TABLE [ProductionDetails] DROP CONSTRAINT [' + @var46 + '];');
    ALTER TABLE [ProductionDetails] ALTER COLUMN [WasteWeight] decimal(18,4) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var47 sysname;
    SELECT @var47 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductionDetails]') AND [c].[name] = N'ProducedQuantity');
    IF @var47 IS NOT NULL EXEC(N'ALTER TABLE [ProductionDetails] DROP CONSTRAINT [' + @var47 + '];');
    ALTER TABLE [ProductionDetails] ALTER COLUMN [ProducedQuantity] decimal(18,4) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var48 sysname;
    SELECT @var48 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductionDetails]') AND [c].[name] = N'ConsumedWeight');
    IF @var48 IS NOT NULL EXEC(N'ALTER TABLE [ProductionDetails] DROP CONSTRAINT [' + @var48 + '];');
    ALTER TABLE [ProductionDetails] ALTER COLUMN [ConsumedWeight] decimal(18,4) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var49 sysname;
    SELECT @var49 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductionDetailAudits]') AND [c].[name] = N'OldWasteWeight');
    IF @var49 IS NOT NULL EXEC(N'ALTER TABLE [ProductionDetailAudits] DROP CONSTRAINT [' + @var49 + '];');
    ALTER TABLE [ProductionDetailAudits] ALTER COLUMN [OldWasteWeight] decimal(18,4) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var50 sysname;
    SELECT @var50 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductionDetailAudits]') AND [c].[name] = N'OldProducedQuantity');
    IF @var50 IS NOT NULL EXEC(N'ALTER TABLE [ProductionDetailAudits] DROP CONSTRAINT [' + @var50 + '];');
    ALTER TABLE [ProductionDetailAudits] ALTER COLUMN [OldProducedQuantity] decimal(18,4) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var51 sysname;
    SELECT @var51 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductionDetailAudits]') AND [c].[name] = N'OldConsumedWeight');
    IF @var51 IS NOT NULL EXEC(N'ALTER TABLE [ProductionDetailAudits] DROP CONSTRAINT [' + @var51 + '];');
    ALTER TABLE [ProductionDetailAudits] ALTER COLUMN [OldConsumedWeight] decimal(18,4) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var52 sysname;
    SELECT @var52 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductionDetailAudits]') AND [c].[name] = N'NewWasteWeight');
    IF @var52 IS NOT NULL EXEC(N'ALTER TABLE [ProductionDetailAudits] DROP CONSTRAINT [' + @var52 + '];');
    ALTER TABLE [ProductionDetailAudits] ALTER COLUMN [NewWasteWeight] decimal(18,4) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var53 sysname;
    SELECT @var53 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductionDetailAudits]') AND [c].[name] = N'NewProducedQuantity');
    IF @var53 IS NOT NULL EXEC(N'ALTER TABLE [ProductionDetailAudits] DROP CONSTRAINT [' + @var53 + '];');
    ALTER TABLE [ProductionDetailAudits] ALTER COLUMN [NewProducedQuantity] decimal(18,4) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var54 sysname;
    SELECT @var54 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductionDetailAudits]') AND [c].[name] = N'NewConsumedWeight');
    IF @var54 IS NOT NULL EXEC(N'ALTER TABLE [ProductionDetailAudits] DROP CONSTRAINT [' + @var54 + '];');
    ALTER TABLE [ProductionDetailAudits] ALTER COLUMN [NewConsumedWeight] decimal(18,4) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var55 sysname;
    SELECT @var55 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaperWidths]') AND [c].[name] = N'Value');
    IF @var55 IS NOT NULL EXEC(N'ALTER TABLE [PaperWidths] DROP CONSTRAINT [' + @var55 + '];');
    ALTER TABLE [PaperWidths] ALTER COLUMN [Value] decimal(18,4) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var56 sysname;
    SELECT @var56 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaperWeights]') AND [c].[name] = N'Value');
    IF @var56 IS NOT NULL EXEC(N'ALTER TABLE [PaperWeights] DROP CONSTRAINT [' + @var56 + '];');
    ALTER TABLE [PaperWeights] ALTER COLUMN [Value] decimal(18,4) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    ALTER TABLE [FscSerials] ADD [RowVersion] rowversion NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var57 sysname;
    SELECT @var57 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FscLots]') AND [c].[name] = N'InvoiceAmount');
    IF @var57 IS NOT NULL EXEC(N'ALTER TABLE [FscLots] DROP CONSTRAINT [' + @var57 + '];');
    ALTER TABLE [FscLots] ALTER COLUMN [InvoiceAmount] decimal(18,4) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var58 sysname;
    SELECT @var58 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FscLots]') AND [c].[name] = N'ConversionFireKg');
    IF @var58 IS NOT NULL EXEC(N'ALTER TABLE [FscLots] DROP CONSTRAINT [' + @var58 + '];');
    ALTER TABLE [FscLots] ALTER COLUMN [ConversionFireKg] decimal(18,4) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var59 sysname;
    SELECT @var59 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConversionAudits]') AND [c].[name] = N'OldFireKg');
    IF @var59 IS NOT NULL EXEC(N'ALTER TABLE [ConversionAudits] DROP CONSTRAINT [' + @var59 + '];');
    ALTER TABLE [ConversionAudits] ALTER COLUMN [OldFireKg] decimal(18,4) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    DECLARE @var60 sysname;
    SELECT @var60 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConversionAudits]') AND [c].[name] = N'NewFireKg');
    IF @var60 IS NOT NULL EXEC(N'ALTER TABLE [ConversionAudits] DROP CONSTRAINT [' + @var60 + '];');
    ALTER TABLE [ConversionAudits] ALTER COLUMN [NewFireKg] decimal(18,4) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717193558_AddRowVersionAndDecimalPrecision'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260717193558_AddRowVersionAndDecimalPrecision', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718210305_AddMachineType'
)
BEGIN
    CREATE TABLE [MachineTypes] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_MachineTypes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718210305_AddMachineType'
)
BEGIN
    ALTER TABLE [Machines] ADD [MachineTypeId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718210305_AddMachineType'
)
BEGIN

                    INSERT INTO MachineTypes (Name, IsActive, CreatedBy, CreatedDate)
                    SELECT DISTINCT LTRIM(RTRIM(m.Type)), 1, 'SISTEM', GETDATE()
                    FROM Machines m
                    WHERE m.Type IS NOT NULL AND LTRIM(RTRIM(m.Type)) <> ''
                      AND NOT EXISTS (SELECT 1 FROM MachineTypes mt WHERE mt.Name = LTRIM(RTRIM(m.Type)));

                    UPDATE m
                    SET m.MachineTypeId = mt.Id
                    FROM Machines m
                    INNER JOIN MachineTypes mt ON mt.Name = LTRIM(RTRIM(m.Type));

                    -- Type boş/null kalan satırlar için 'Tanımsız' türü oluştur ve ata
                    INSERT INTO MachineTypes (Name, IsActive, CreatedBy, CreatedDate)
                    SELECT 'Tanımsız', 1, 'SISTEM', GETDATE()
                    WHERE EXISTS (SELECT 1 FROM Machines WHERE MachineTypeId IS NULL)
                      AND NOT EXISTS (SELECT 1 FROM MachineTypes WHERE Name = 'Tanımsız');

                    UPDATE Machines
                    SET MachineTypeId = (SELECT Id FROM MachineTypes WHERE Name = 'Tanımsız')
                    WHERE MachineTypeId IS NULL;
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718210305_AddMachineType'
)
BEGIN
    DECLARE @var61 sysname;
    SELECT @var61 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Machines]') AND [c].[name] = N'Type');
    IF @var61 IS NOT NULL EXEC(N'ALTER TABLE [Machines] DROP CONSTRAINT [' + @var61 + '];');
    ALTER TABLE [Machines] DROP COLUMN [Type];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718210305_AddMachineType'
)
BEGIN
    DECLARE @var62 sysname;
    SELECT @var62 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Machines]') AND [c].[name] = N'MachineTypeId');
    IF @var62 IS NOT NULL EXEC(N'ALTER TABLE [Machines] DROP CONSTRAINT [' + @var62 + '];');
    ALTER TABLE [Machines] ALTER COLUMN [MachineTypeId] int NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718210305_AddMachineType'
)
BEGIN
    CREATE INDEX [IX_Machines_MachineTypeId] ON [Machines] ([MachineTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718210305_AddMachineType'
)
BEGIN
    ALTER TABLE [Machines] ADD CONSTRAINT [FK_Machines_MachineTypes_MachineTypeId] FOREIGN KEY ([MachineTypeId]) REFERENCES [MachineTypes] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718210305_AddMachineType'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260718210305_AddMachineType', N'8.0.24');
END;
GO

COMMIT;
GO

