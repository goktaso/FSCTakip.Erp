using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FSCTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToProductGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Machines",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Machines",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PaperTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PaperTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ProductGroups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "FscTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedBy", "Description" },
                values: new object[] { "SYSTEM", "TAMAMI SERTIFIKALI" });

            migrationBuilder.UpdateData(
                table: "FscTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedBy", "Description", "Name" },
                values: new object[] { "SYSTEM", "KARISIM ICERIK", "FSC MIX" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ProductGroups");

            migrationBuilder.UpdateData(
                table: "FscTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedBy", "Description" },
                values: new object[] { "System", "Tamamı sertifikalı" });

            migrationBuilder.UpdateData(
                table: "FscTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedBy", "Description", "Name" },
                values: new object[] { "System", "Karışım içerik", "FSC Mix" });

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
        }
    }
}
