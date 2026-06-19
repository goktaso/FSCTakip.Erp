using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSCTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddFscDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FscLots_FscSerials_SourceSerialId",
                table: "FscLots");

            migrationBuilder.CreateTable(
                name: "FscDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FscDocuments", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_FscLots_FscSerials_SourceSerialId",
                table: "FscLots",
                column: "SourceSerialId",
                principalTable: "FscSerials",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FscLots_FscSerials_SourceSerialId",
                table: "FscLots");

            migrationBuilder.DropTable(
                name: "FscDocuments");

            migrationBuilder.AddForeignKey(
                name: "FK_FscLots_FscSerials_SourceSerialId",
                table: "FscLots",
                column: "SourceSerialId",
                principalTable: "FscSerials",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
