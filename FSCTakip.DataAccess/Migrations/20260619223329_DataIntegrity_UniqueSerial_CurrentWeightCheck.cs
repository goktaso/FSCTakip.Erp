using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSCTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class DataIntegrity_UniqueSerial_CurrentWeightCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FscSerials_LotId",
                table: "FscSerials");

            migrationBuilder.AlterColumn<string>(
                name: "SerialNo",
                table: "FscSerials",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_FscSerials_LotId_SerialNo_Unique",
                table: "FscSerials",
                columns: new[] { "LotId", "SerialNo" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_FscSerials_CurrentWeight",
                table: "FscSerials",
                sql: "[CurrentWeight] >= -0.001");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FscSerials_LotId_SerialNo_Unique",
                table: "FscSerials");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FscSerials_CurrentWeight",
                table: "FscSerials");

            migrationBuilder.AlterColumn<string>(
                name: "SerialNo",
                table: "FscSerials",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_FscSerials_LotId",
                table: "FscSerials",
                column: "LotId");
        }
    }
}
