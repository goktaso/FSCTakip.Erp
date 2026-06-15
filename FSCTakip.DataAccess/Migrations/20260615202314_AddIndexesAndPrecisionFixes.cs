using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSCTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesAndPrecisionFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockMovements_ProductId",
                table: "StockMovements");

            migrationBuilder.AlterColumn<decimal>(
                name: "OriginalQuantity",
                table: "FscSerials",
                type: "decimal(18,4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "InitialWeight",
                table: "FscSerials",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentWeight",
                table: "FscSerials",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ProductId_DocumentDate",
                table: "StockMovements",
                columns: new[] { "ProductId", "DocumentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FscLots_SourceSerialId",
                table: "FscLots",
                column: "SourceSerialId");

            migrationBuilder.AddForeignKey(
                name: "FK_FscLots_FscSerials_SourceSerialId",
                table: "FscLots",
                column: "SourceSerialId",
                principalTable: "FscSerials",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FscLots_FscSerials_SourceSerialId",
                table: "FscLots");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_ProductId_DocumentDate",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_FscLots_SourceSerialId",
                table: "FscLots");

            migrationBuilder.AlterColumn<decimal>(
                name: "OriginalQuantity",
                table: "FscSerials",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "InitialWeight",
                table: "FscSerials",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentWeight",
                table: "FscSerials",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ProductId",
                table: "StockMovements",
                column: "ProductId");
        }
    }
}
