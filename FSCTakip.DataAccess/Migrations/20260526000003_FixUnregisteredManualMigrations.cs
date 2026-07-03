using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSCTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixUnregisteredManualMigrations : Migration
    {
        /// <inheritdoc />
        // NOT: 20260524130000_RenamePartiNoAddSerialLotNo ve 20260526000001_FscLotSupplierIdNullable
        // migration'ları elle yazılmış, eşleşen .Designer.cs dosyaları hiç oluşturulmamıştı — EF,
        // [Migration] attribute'ü olmayan sınıfları migration listesine almaz, bu yüzden sıfırdan
        // (boş) bir veritabanı kurulumunda bu değişiklikler sessizce ATLANIYORDU:
        //   • FscLots.LotNo → PartiNo rename hiç çalışmıyor (DbSeeder "Invalid column name 'PartiNo'" ile çöküyor)
        //   • FscSerials.LotNo kolonu hiç eklenmiyor
        //   • FscLots.SupplierId NOT NULL kalıyor (dönüşüm lotları SupplierId=NULL yazamıyor)
        // ACORE'un canlı DB'sinde bu değişiklikler geçmişte elle/SSMS ile uygulanmış durumda —
        // aşağıdaki yama idempotent: var olan DB'de no-op, eksik DB'de tamamlayıcı.
        // (Aynı desen: 20260524120001_FixMissingExternalCodesColumns.)
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- 1) FscLots.LotNo -> PartiNo rename (yalnızca eski ad duruyorsa)
IF COL_LENGTH('FscLots','PartiNo') IS NULL AND COL_LENGTH('FscLots','LotNo') IS NOT NULL
    EXEC sp_rename 'FscLots.LotNo', 'PartiNo', 'COLUMN';

-- 2) FscSerials.LotNo (bobin bazlı lot no) yoksa ekle
IF COL_LENGTH('FscSerials','LotNo') IS NULL
    ALTER TABLE [FscSerials] ADD [LotNo] nvarchar(max) NULL;

-- 2b) StockMovements.QuantityKg — entity'de var (StockMovement.QuantityKg), ama hiçbir
--     migration'a girmemiş (doğrudan SSMS ile eklenmiş). Sıfır kurulumda DbSeeder
--     Invalid column name QuantityKg hatasıyla çöküyordu. Tam şema diff'i ile bulundu.
IF COL_LENGTH('StockMovements','QuantityKg') IS NULL
    ALTER TABLE [StockMovements] ADD [QuantityKg] decimal(18,2) NULL;

-- 3) FscLots.SupplierId nullable değilse: FK'yı (adı ne olursa olsun) bırak, nullable yap, FK'yı geri kur
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FscLots') AND name = 'SupplierId' AND is_nullable = 0)
BEGIN
    DECLARE @fk sysname;
    SELECT @fk = fk.name
    FROM sys.foreign_keys fk
    JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
    JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
    WHERE fk.parent_object_id = OBJECT_ID('FscLots') AND c.name = 'SupplierId';

    IF @fk IS NOT NULL
        EXEC('ALTER TABLE [FscLots] DROP CONSTRAINT [' + @fk + ']');

    ALTER TABLE [FscLots] ALTER COLUMN [SupplierId] int NULL;

    ALTER TABLE [FscLots] ADD CONSTRAINT [FK_FscLots_Suppliers_SupplierId]
        FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Geri alma bilinçli olarak boş: bu yama, elle uygulanmış geçmiş değişikliklerin
            // eksik kalmış kopyalarını tamamlar; geri almak canlı ACORE şemasını bozar.
        }
    }
}
