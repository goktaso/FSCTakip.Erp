using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSCTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductWithWeightAndWidth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Width",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Grammage",
                table: "Products",
                newName: "PaperWidthId");

            migrationBuilder.AddColumn<int>(
                name: "PaperWeightId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_PaperWeightId",
                table: "Products",
                column: "PaperWeightId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_PaperWidthId",
                table: "Products",
                column: "PaperWidthId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_PaperWeights_PaperWeightId",
                table: "Products",
                column: "PaperWeightId",
                principalTable: "PaperWeights",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_PaperWidths_PaperWidthId",
                table: "Products",
                column: "PaperWidthId",
                principalTable: "PaperWidths",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_PaperWeights_PaperWeightId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_PaperWidths_PaperWidthId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_PaperWeightId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_PaperWidthId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PaperWeightId",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "PaperWidthId",
                table: "Products",
                newName: "Grammage");

            migrationBuilder.AddColumn<decimal>(
                name: "Width",
                table: "Products",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
