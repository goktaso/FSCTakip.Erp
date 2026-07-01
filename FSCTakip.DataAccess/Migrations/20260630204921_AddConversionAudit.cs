using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSCTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddConversionAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConversionAudits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerialId = table.Column<int>(type: "int", nullable: false),
                    PartiNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldTarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OldFireKg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NewTarih = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NewFireKg = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ChangedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversionAudits", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversionAudits");
        }
    }
}
