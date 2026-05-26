using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSCTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceWasteManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "WasteManagements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DisposalMethod",
                table: "WasteManagements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisposedBy",
                table: "WasteManagements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "WasteManagements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "WasteManagements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "WorkOrderId",
                table: "WasteManagements",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WasteManagements_WorkOrderId",
                table: "WasteManagements",
                column: "WorkOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_WasteManagements_WorkOrders_WorkOrderId",
                table: "WasteManagements",
                column: "WorkOrderId",
                principalTable: "WorkOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WasteManagements_WorkOrders_WorkOrderId",
                table: "WasteManagements");

            migrationBuilder.DropIndex(
                name: "IX_WasteManagements_WorkOrderId",
                table: "WasteManagements");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "WasteManagements");

            migrationBuilder.DropColumn(
                name: "DisposalMethod",
                table: "WasteManagements");

            migrationBuilder.DropColumn(
                name: "DisposedBy",
                table: "WasteManagements");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "WasteManagements");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "WasteManagements");

            migrationBuilder.DropColumn(
                name: "WorkOrderId",
                table: "WasteManagements");
        }
    }
}
