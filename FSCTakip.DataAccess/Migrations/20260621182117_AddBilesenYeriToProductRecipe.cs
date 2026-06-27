using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSCTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddBilesenYeriToProductRecipe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // QuantityKg zaten StockMovements tablosunda mevcut — atla
            migrationBuilder.AddColumn<string>(
                name: "BilesenYeri",
                table: "ProductRecipes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BilesenYeri",
                table: "ProductRecipes");
        }
    }
}
