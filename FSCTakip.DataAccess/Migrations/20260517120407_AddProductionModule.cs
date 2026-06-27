using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSCTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionDetails_FscSerials_FscSerialId",
                table: "ProductionDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionDetails_Machines_MachineId",
                table: "ProductionDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionDetails_WorkOrders_WorkOrderId",
                table: "ProductionDetails");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "ProductCode",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "ConversionRate",
                table: "ProductionDetails");

            migrationBuilder.DropColumn(
                name: "UsedIn",
                table: "ProductionDetails");

            migrationBuilder.AlterColumn<int>(
                name: "MachineId",
                table: "WorkOrders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<decimal>(
                name: "ActualQuantity",
                table: "WorkOrders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedDate",
                table: "WorkOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "WorkOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedDate",
                table: "WorkOrders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "WorkOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "WorkOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ProductionDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_MachineId",
                table: "WorkOrders",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_ProductId",
                table: "WorkOrders",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionDetails_FscSerials_FscSerialId",
                table: "ProductionDetails",
                column: "FscSerialId",
                principalTable: "FscSerials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionDetails_Machines_MachineId",
                table: "ProductionDetails",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionDetails_WorkOrders_WorkOrderId",
                table: "ProductionDetails",
                column: "WorkOrderId",
                principalTable: "WorkOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_Machines_MachineId",
                table: "WorkOrders",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_Products_ProductId",
                table: "WorkOrders",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionDetails_FscSerials_FscSerialId",
                table: "ProductionDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionDetails_Machines_MachineId",
                table: "ProductionDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionDetails_WorkOrders_WorkOrderId",
                table: "ProductionDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_Machines_MachineId",
                table: "WorkOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_Products_ProductId",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_MachineId",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_ProductId",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "ActualQuantity",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "CompletedDate",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "PlannedDate",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ProductionDetails");

            migrationBuilder.AlterColumn<string>(
                name: "MachineId",
                table: "WorkOrders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "WorkOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProductCode",
                table: "WorkOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ConversionRate",
                table: "ProductionDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "UsedIn",
                table: "ProductionDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionDetails_FscSerials_FscSerialId",
                table: "ProductionDetails",
                column: "FscSerialId",
                principalTable: "FscSerials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionDetails_Machines_MachineId",
                table: "ProductionDetails",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionDetails_WorkOrders_WorkOrderId",
                table: "ProductionDetails",
                column: "WorkOrderId",
                principalTable: "WorkOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
