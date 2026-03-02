using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FSCTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixProductRecipeRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BagTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BagTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaxNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaxOffice = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FscLicenseCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsFscActive = table.Column<bool>(type: "bit", nullable: false),
                    FscExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FscTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FscTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Machines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaperColors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaperColors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaperTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShortCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaperTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaperWidths",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaperWidths", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductGrammages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductGrammages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupCode = table.Column<int>(type: "int", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RangeStart = table.Column<int>(type: "int", nullable: false),
                    RangeEnd = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FscCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FscExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsFscActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WasteManagements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WasteCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DisposalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WasteManagements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MachineId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlannedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaperColorId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_PaperColors_PaperColorId",
                        column: x => x.PaperColorId,
                        principalTable: "PaperColors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FscLots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LotNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FscTypeId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    InvoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DispatchNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InvoicePdfPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DispatchPdfPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FscLots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FscLots_FscTypes_FscTypeId",
                        column: x => x.FscTypeId,
                        principalTable: "FscTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FscLots_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductRecipes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentProductId = table.Column<int>(type: "int", nullable: false),
                    ChildProductId = table.Column<int>(type: "int", nullable: false),
                    StandardQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductRecipes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductRecipes_Products_ChildProductId",
                        column: x => x.ChildProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductRecipes_Products_ParentProductId",
                        column: x => x.ParentProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockMovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ErpReferenceId = table.Column<int>(type: "int", nullable: false),
                    DocumentNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromWarehouseId = table.Column<int>(type: "int", nullable: true),
                    ToWarehouseId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    PlateNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkOrderId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockMovements_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StockMovements_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockMovements_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FscSerials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LotId = table.Column<int>(type: "int", nullable: false),
                    SerialNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InitialWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsOpeningStock = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FscSerials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FscSerials_FscLots_LotId",
                        column: x => x.LotId,
                        principalTable: "FscLots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    FscSerialId = table.Column<int>(type: "int", nullable: false),
                    ProductionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    UsedIn = table.Column<int>(type: "int", nullable: false),
                    ConsumedWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WasteWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProducedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ConversionRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionDetails_FscSerials_FscSerialId",
                        column: x => x.FscSerialId,
                        principalTable: "FscSerials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionDetails_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionDetails_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderRecipes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    FscSerialId = table.Column<int>(type: "int", nullable: true),
                    PlannedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualConsumedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderRecipes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderRecipes_FscSerials_FscSerialId",
                        column: x => x.FscSerialId,
                        principalTable: "FscSerials",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkOrderRecipes_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrderRecipes_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "FscTypes",
                columns: new[] { "Id", "Code", "CreatedBy", "CreatedDate", "Description", "IsActive", "Name", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, "FSC-100", "System", new DateTime(2026, 2, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tamamı sertifikalı", true, "FSC %100", null, null },
                    { 2, "FSC-MIX", "System", new DateTime(2026, 2, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Karışım içerik", true, "FSC Mix", null, null }
                });

            migrationBuilder.InsertData(
                table: "Machines",
                columns: new[] { "Id", "Code", "CreatedBy", "CreatedDate", "IsActive", "Name", "Type", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, "M-01", "System", new DateTime(2026, 2, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "8 Renk Flexo", "Matbaa", null, null },
                    { 2, "K-01", "System", new DateTime(2026, 2, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Kare Dip Kesim", "Kesim", null, null }
                });

            migrationBuilder.InsertData(
                table: "PaperTypes",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "IsActive", "Name", "ShortCode", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, "System", new DateTime(2026, 2, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Kraft Kağıt", "KRT", null, null },
                    { 2, "System", new DateTime(2026, 2, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Sülfit Kağıt", "SLF", null, null }
                });

            migrationBuilder.InsertData(
                table: "Warehouses",
                columns: new[] { "Id", "Code", "CreatedBy", "CreatedDate", "IsActive", "Name", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, "DEP-01", "System", new DateTime(2026, 2, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Hammadde Deposu", null, null },
                    { 2, "DEP-02", "System", new DateTime(2026, 2, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Mamul Deposu", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_FscLots_FscTypeId",
                table: "FscLots",
                column: "FscTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FscLots_SupplierId",
                table: "FscLots",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_FscSerials_LotId",
                table: "FscSerials",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionDetails_FscSerialId",
                table: "ProductionDetails",
                column: "FscSerialId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionDetails_MachineId",
                table: "ProductionDetails",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionDetails_WorkOrderId",
                table: "ProductionDetails",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRecipes_ChildProductId",
                table: "ProductRecipes",
                column: "ChildProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRecipes_ParentProductId",
                table: "ProductRecipes",
                column: "ParentProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_PaperColorId",
                table: "Products",
                column: "PaperColorId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_CustomerId",
                table: "StockMovements",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ProductId",
                table: "StockMovements",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_WorkOrderId",
                table: "StockMovements",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderRecipes_FscSerialId",
                table: "WorkOrderRecipes",
                column: "FscSerialId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderRecipes_ProductId",
                table: "WorkOrderRecipes",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderRecipes_WorkOrderId",
                table: "WorkOrderRecipes",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_CustomerId",
                table: "WorkOrders",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BagTypes");

            migrationBuilder.DropTable(
                name: "PaperTypes");

            migrationBuilder.DropTable(
                name: "PaperWidths");

            migrationBuilder.DropTable(
                name: "ProductGrammages");

            migrationBuilder.DropTable(
                name: "ProductGroups");

            migrationBuilder.DropTable(
                name: "ProductionDetails");

            migrationBuilder.DropTable(
                name: "ProductRecipes");

            migrationBuilder.DropTable(
                name: "StockMovements");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropTable(
                name: "WasteManagements");

            migrationBuilder.DropTable(
                name: "WorkOrderRecipes");

            migrationBuilder.DropTable(
                name: "Machines");

            migrationBuilder.DropTable(
                name: "FscSerials");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "WorkOrders");

            migrationBuilder.DropTable(
                name: "FscLots");

            migrationBuilder.DropTable(
                name: "PaperColors");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "FscTypes");

            migrationBuilder.DropTable(
                name: "Suppliers");
        }
    }
}
