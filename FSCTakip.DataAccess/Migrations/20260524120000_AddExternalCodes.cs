using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSCTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Supplier — harici ERP kodu (Netsis CARI_KOD vb.)
            migrationBuilder.AddColumn<string>(
                name: "ExternalCode",
                table: "Suppliers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            // Customer — harici ERP kodu
            migrationBuilder.AddColumn<string>(
                name: "ExternalCode",
                table: "Customers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            // Product — harici ERP stok kodu (Netsis STOK_KODU vb.)
            migrationBuilder.AddColumn<string>(
                name: "ExternalCode",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            // SalesOrder — harici ERP sipariş/evrak numarası
            migrationBuilder.AddColumn<string>(
                name: "ExternalOrderNo",
                table: "SalesOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            // WorkOrder — harici ERP iş emri numarası
            migrationBuilder.AddColumn<string>(
                name: "ExternalOrderNo",
                table: "WorkOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalCode",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "ExternalCode",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ExternalCode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ExternalOrderNo",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "ExternalOrderNo",
                table: "WorkOrders");
        }
    }
}
