using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSCTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixMissingExternalCodesColumns : Migration
    {
        /// <inheritdoc />
        // NOT: 20260524120000_AddExternalCodes migration'ının eşleşen .Designer.cs dosyası
        // repoda eksikti — bu yüzden sıfırdan (boş) bir veritabanına migration zincirini tek
        // seferde uygularken bu kolonlar hiç oluşturulmuyor, sıradaki migration (AddRbacAndAuditLog)
        // ExternalOrderNo'yu ALTER etmeye çalışınca "column does not exist" hatası veriyordu.
        // ACORE'un gerçek/canlı veritabanında bu kolonlar zaten mevcut (o migration geçmişte
        // zaten çalıştı) — bu yüzden burada IF NOT EXISTS ile idempotent (var olan DB'de no-op,
        // eksik olan DB'de tamamlayıcı) bir yama uygulanıyor. Eski migration'a dokunulmadı.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Suppliers') AND name = 'ExternalCode')
    ALTER TABLE [Suppliers] ADD [ExternalCode] nvarchar(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'ExternalCode')
    ALTER TABLE [Customers] ADD [ExternalCode] nvarchar(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'ExternalCode')
    ALTER TABLE [Products] ADD [ExternalCode] nvarchar(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SalesOrders') AND name = 'ExternalOrderNo')
    ALTER TABLE [SalesOrders] ADD [ExternalOrderNo] nvarchar(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WorkOrders') AND name = 'ExternalOrderNo')
    ALTER TABLE [WorkOrders] ADD [ExternalOrderNo] nvarchar(100) NULL;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WorkOrders') AND name = 'ExternalOrderNo')
    ALTER TABLE [WorkOrders] DROP COLUMN [ExternalOrderNo];

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SalesOrders') AND name = 'ExternalOrderNo')
    ALTER TABLE [SalesOrders] DROP COLUMN [ExternalOrderNo];

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'ExternalCode')
    ALTER TABLE [Products] DROP COLUMN [ExternalCode];

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'ExternalCode')
    ALTER TABLE [Customers] DROP COLUMN [ExternalCode];

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Suppliers') AND name = 'ExternalCode')
    ALTER TABLE [Suppliers] DROP COLUMN [ExternalCode];
");
        }
    }
}
