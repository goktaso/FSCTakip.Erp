using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSCTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RenamePartiNoAddSerialLotNo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // FscLots.LotNo → FscLots.PartiNo
            migrationBuilder.RenameColumn(
                name: "LotNo",
                table: "FscLots",
                newName: "PartiNo");

            // FscSerials'a bobin bazlı LotNo ekle (opsiyonel)
            migrationBuilder.AddColumn<string>(
                name: "LotNo",
                table: "FscSerials",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LotNo",
                table: "FscSerials");

            migrationBuilder.RenameColumn(
                name: "PartiNo",
                table: "FscLots",
                newName: "LotNo");
        }
    }
}
