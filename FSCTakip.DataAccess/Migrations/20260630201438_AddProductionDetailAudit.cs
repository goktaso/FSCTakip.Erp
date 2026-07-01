using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSCTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionDetailAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductionDetailAudits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionDetailId = table.Column<int>(type: "int", nullable: false),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldConsumedWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OldWasteWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OldProducedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NewConsumedWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NewWasteWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NewProducedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ChangedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionDetailAudits", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionDetailAudits");
        }
    }
}
