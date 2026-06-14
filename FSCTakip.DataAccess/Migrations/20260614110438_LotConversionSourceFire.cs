using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSCTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class LotConversionSourceFire : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ConversionFireKg",
                table: "FscLots",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceSerialId",
                table: "FscLots",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConversionFireKg",
                table: "FscLots");

            migrationBuilder.DropColumn(
                name: "SourceSerialId",
                table: "FscLots");
        }
    }
}
