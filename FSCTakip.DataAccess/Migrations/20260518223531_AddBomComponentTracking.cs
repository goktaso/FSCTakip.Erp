using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSCTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddBomComponentTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderRecipes_FscSerials_FscSerialId",
                table: "WorkOrderRecipes");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderRecipes_Products_ProductId",
                table: "WorkOrderRecipes");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WorkOrderRecipes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<decimal>(
                name: "ProducedQuantity",
                table: "WorkOrderRecipes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "WasteQuantity",
                table: "WorkOrderRecipes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "WorkOrderRecipeId",
                table: "ProductionDetails",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionDetails_WorkOrderRecipeId",
                table: "ProductionDetails",
                column: "WorkOrderRecipeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionDetails_WorkOrderRecipes_WorkOrderRecipeId",
                table: "ProductionDetails",
                column: "WorkOrderRecipeId",
                principalTable: "WorkOrderRecipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderRecipes_FscSerials_FscSerialId",
                table: "WorkOrderRecipes",
                column: "FscSerialId",
                principalTable: "FscSerials",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderRecipes_Products_ProductId",
                table: "WorkOrderRecipes",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionDetails_WorkOrderRecipes_WorkOrderRecipeId",
                table: "ProductionDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderRecipes_FscSerials_FscSerialId",
                table: "WorkOrderRecipes");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderRecipes_Products_ProductId",
                table: "WorkOrderRecipes");

            migrationBuilder.DropIndex(
                name: "IX_ProductionDetails_WorkOrderRecipeId",
                table: "ProductionDetails");

            migrationBuilder.DropColumn(
                name: "ProducedQuantity",
                table: "WorkOrderRecipes");

            migrationBuilder.DropColumn(
                name: "WasteQuantity",
                table: "WorkOrderRecipes");

            migrationBuilder.DropColumn(
                name: "WorkOrderRecipeId",
                table: "ProductionDetails");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WorkOrderRecipes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderRecipes_FscSerials_FscSerialId",
                table: "WorkOrderRecipes",
                column: "FscSerialId",
                principalTable: "FscSerials",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderRecipes_Products_ProductId",
                table: "WorkOrderRecipes",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
