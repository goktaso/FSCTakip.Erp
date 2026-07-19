using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSCTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddMachineType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MachineTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineTypes", x => x.Id);
                });

            migrationBuilder.AddColumn<int>(
                name: "MachineTypeId",
                table: "Machines",
                type: "int",
                nullable: true);

            // Mevcut Machines.Type metin değerlerini olduğu gibi MachineTypes'a taşı —
            // Türkçe etiket varsayımı (Baskı/Kesim/Dilimleme) yapmadan, gerçek geçmiş
            // veriyi (ör. "Matbaa", "Kesim") aynen korur. Böylece hangi müşteri hangi
            // metni girmiş olursa olsun veri kaybı olmaz.
            migrationBuilder.Sql(@"
                INSERT INTO MachineTypes (Name, IsActive, CreatedBy, CreatedDate)
                SELECT DISTINCT LTRIM(RTRIM(m.Type)), 1, 'SISTEM', GETDATE()
                FROM Machines m
                WHERE m.Type IS NOT NULL AND LTRIM(RTRIM(m.Type)) <> ''
                  AND NOT EXISTS (SELECT 1 FROM MachineTypes mt WHERE mt.Name = LTRIM(RTRIM(m.Type)));

                UPDATE m
                SET m.MachineTypeId = mt.Id
                FROM Machines m
                INNER JOIN MachineTypes mt ON mt.Name = LTRIM(RTRIM(m.Type));

                -- Type boş/null kalan satırlar için 'Tanımsız' türü oluştur ve ata
                INSERT INTO MachineTypes (Name, IsActive, CreatedBy, CreatedDate)
                SELECT 'Tanımsız', 1, 'SISTEM', GETDATE()
                WHERE EXISTS (SELECT 1 FROM Machines WHERE MachineTypeId IS NULL)
                  AND NOT EXISTS (SELECT 1 FROM MachineTypes WHERE Name = 'Tanımsız');

                UPDATE Machines
                SET MachineTypeId = (SELECT Id FROM MachineTypes WHERE Name = 'Tanımsız')
                WHERE MachineTypeId IS NULL;
            ");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Machines");

            migrationBuilder.AlterColumn<int>(
                name: "MachineTypeId",
                table: "Machines",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Machines_MachineTypeId",
                table: "Machines",
                column: "MachineTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_MachineTypes_MachineTypeId",
                table: "Machines",
                column: "MachineTypeId",
                principalTable: "MachineTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Machines",
                type: "nvarchar(max)",
                nullable: true);

            // MachineTypes tablosu silinmeden önce isimleri geri Type sütununa yaz
            migrationBuilder.Sql(@"
                UPDATE m
                SET m.Type = mt.Name
                FROM Machines m
                INNER JOIN MachineTypes mt ON mt.Id = m.MachineTypeId;
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_Machines_MachineTypes_MachineTypeId",
                table: "Machines");

            migrationBuilder.DropTable(
                name: "MachineTypes");

            migrationBuilder.DropIndex(
                name: "IX_Machines_MachineTypeId",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "MachineTypeId",
                table: "Machines");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Machines",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
