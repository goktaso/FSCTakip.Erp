using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSCTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseLotFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "FscSerials",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InvoicePdfPath",
                table: "FscLots",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceNo",
                table: "FscLots",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DispatchPdfPath",
                table: "FscLots",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DispatchNo",
                table: "FscLots",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "ArrivalDate",
                table: "FscLots",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "FscLots",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InvoiceAmount",
                table: "FscLots",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "FscLots",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "FscLots",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TruckPlate",
                table: "FscLots",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FscLots_ProductId",
                table: "FscLots",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_FscLots_Products_ProductId",
                table: "FscLots",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FscLots_Products_ProductId",
                table: "FscLots");

            migrationBuilder.DropIndex(
                name: "IX_FscLots_ProductId",
                table: "FscLots");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "FscSerials");

            migrationBuilder.DropColumn(
                name: "ArrivalDate",
                table: "FscLots");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "FscLots");

            migrationBuilder.DropColumn(
                name: "InvoiceAmount",
                table: "FscLots");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "FscLots");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "FscLots");

            migrationBuilder.DropColumn(
                name: "TruckPlate",
                table: "FscLots");

            migrationBuilder.AlterColumn<string>(
                name: "InvoicePdfPath",
                table: "FscLots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceNo",
                table: "FscLots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DispatchPdfPath",
                table: "FscLots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DispatchNo",
                table: "FscLots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
