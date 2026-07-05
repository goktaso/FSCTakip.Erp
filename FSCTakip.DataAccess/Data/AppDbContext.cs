using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.AspNetCore.Http;
using FSCTakip.Core.Entities;
using System.Globalization;
using System.Text.Json;

namespace FSCTakip.DataAccess.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IHttpContextAccessor? _http;
        private bool _isAuditing = false;

        // AuditLog tabloları ve hassas alanlar audit dışı
        private static readonly HashSet<string> _skipAuditTables = new(StringComparer.OrdinalIgnoreCase)
        {
            nameof(AuditLog)
        };
        private static readonly HashSet<string> _skipAuditProps = new(StringComparer.OrdinalIgnoreCase)
        {
            "PasswordHash"
        };

        // Türkçe büyük harf dönüşümünden hariç tutulan alanlar: hash/yol/e-posta gibi
        // teknik değerler büyük harfe çevrilirse kullanılamaz hale gelir
        // (ör. PasswordHash büyütülünce case-sensitive C# karşılaştırmaları kırılır).
        private static readonly HashSet<string> _skipUppercaseProps = new(StringComparer.OrdinalIgnoreCase)
        {
            "PasswordHash", "Email", "InvoicePdfPath", "DispatchPdfPath", "LogoPath", "FilePath",
            "FileExtension", "FileName", "Username"
        };

        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor? http = null)
            : base(options)
        {
            _http = http;
        }

        #region Tanımlamalar
        public DbSet<FscType>        FscTypes        { get; set; }
        public DbSet<PaperType>      PaperTypes      { get; set; }
        public DbSet<PaperColor>     PaperColors     { get; set; }
        public DbSet<PaperWidth>     PaperWidths     { get; set; }
        public DbSet<Machine>        Machines        { get; set; }
        public DbSet<BagType>        BagTypes        { get; set; }
        public DbSet<ProductGroup>   ProductGroups   { get; set; }
        public DbSet<Warehouse>      Warehouses      { get; set; }
        public DbSet<PaperWeight>    PaperWeights    { get; set; }
        #endregion

        #region Ticari ve Operasyonel
        public DbSet<Customer>         Customers         { get; set; }
        public DbSet<Supplier>         Suppliers         { get; set; }
        public DbSet<Product>          Products          { get; set; }
        public DbSet<ProductRecipe>    ProductRecipes    { get; set; }
        public DbSet<WorkOrder>        WorkOrders        { get; set; }
        public DbSet<WorkOrderRecipe>  WorkOrderRecipes  { get; set; }
        public DbSet<ProductionDetail> ProductionDetails { get; set; }
        public DbSet<ProductionDetailAudit> ProductionDetailAudits { get; set; }
        public DbSet<ConversionAudit>       ConversionAudits       { get; set; }
        public DbSet<StockMovement>    StockMovements    { get; set; }
        public DbSet<FscLot>           FscLots           { get; set; }
        public DbSet<FscSerial>        FscSerials        { get; set; }
        public DbSet<WasteManagement>  WasteManagements  { get; set; }
        public DbSet<SalesOrder>       SalesOrders       { get; set; }
        public DbSet<SalesOrderLine>   SalesOrderLines   { get; set; }
        #endregion

        #region Denetim Dönemleri
        public DbSet<AuditPeriod> AuditPeriods { get; set; }
        #endregion

        #region Şirket Bilgileri (beyaz etiket)
        public DbSet<CompanySetting> CompanySettings { get; set; }
        #endregion

        #region Kullanıcı & Yetki
        public DbSet<AppUser>                AppUsers                { get; set; }
        public DbSet<PermissionGroup>        PermissionGroups        { get; set; }
        public DbSet<PermissionModule>       PermissionModules       { get; set; }
        public DbSet<UserGroup>              UserGroups              { get; set; }
        public DbSet<GroupPermission>        GroupPermissions        { get; set; }
        public DbSet<UserPermissionOverride> UserPermissionOverrides { get; set; }
        #endregion

        #region ETL
        public DbSet<EtlConnection> EtlConnections { get; set; }
        public DbSet<EtlJob>        EtlJobs        { get; set; }
        #endregion

        public DbSet<FscDocument> FscDocuments { get; set; }

        #region Denetim Günlüğü
        public DbSet<AuditLog> AuditLogs { get; set; }
        #endregion

        #region Birim Dönüşüm
        public DbSet<UnitConversion> UnitConversions { get; set; }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        //  SaveChangesAsync — uppercase + audit log
        // ─────────────────────────────────────────────────────────────────────
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Audit log kaydedilirken tekrar bu metoda girişi engelle
            if (_isAuditing)
                return await base.SaveChangesAsync(cancellationToken);

            var trCulture = new CultureInfo("tr-TR");

            // 1. Türkçe büyük harf dönüşümü
            var changedEntries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .ToList();

            foreach (var entry in changedEntries)
            {
                foreach (var prop in entry.Metadata.GetProperties().Where(p => p.ClrType == typeof(string)))
                {
                    var propEntry = entry.Property(prop.Name);

                    // NOT NULL string kolona null gelirse (MVC boş form alanını null'a çevirir)
                    // "" yap → SQL 515 (Cannot insert NULL) hatasını kökten önle.
                    if (propEntry.CurrentValue is null && !prop.IsNullable)
                    {
                        propEntry.CurrentValue = string.Empty;
                        continue;
                    }

                    if (_skipUppercaseProps.Contains(prop.Name))
                        continue;

                    if (propEntry.CurrentValue is string val && !string.IsNullOrEmpty(val))
                        propEntry.CurrentValue = val.ToUpper(trCulture);
                }
            }

            // 2. Dönem kilidi kontrolü (admin değilse kilitli dönemlere yazma engellenir)
            var isAdmin = _http?.HttpContext?.Session.GetString("IsAdmin") == "1";
            if (!isAdmin)
                await CheckPeriodLockAsync(cancellationToken);

            // 3. Audit snapshot (uppercase uygulandıktan SONRA, kayıttan ÖNCE)
            var auditItems = BuildAuditItems();

            // 4. Ana kayıt
            var result = await base.SaveChangesAsync(cancellationToken);

            // 5. INSERT kayıtlarının DB'den gelen Id'sini yaz
            foreach (var item in auditItems.Where(a => a.NeedsId))
            {
                var idProp = item.EntityRef?.GetType().GetProperty("Id");
                if (idProp != null)
                    item.RecordId = idProp.GetValue(item.EntityRef) as int?;
            }

            // 6. AuditLog'ları kaydet
            var logs = auditItems
                .Where(a => a.NewValues != null || a.OldValues != null)
                .Select(a => a.ToLog())
                .ToList();

            if (logs.Any())
            {
                _isAuditing = true;
                try
                {
                    AuditLogs.AddRange(logs);
                    await base.SaveChangesAsync(cancellationToken);
                }
                finally
                {
                    _isAuditing = false;
                }
            }

            return result;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Doğrudan audit satırı yazar — uppercase ve re-audit tetiklemez
        //  (WriteAuditAsync gibi explicit loglama için kullanılır)
        // ─────────────────────────────────────────────────────────────────────
        public async Task WriteAuditDirectAsync(AuditLog log)
        {
            _isAuditing = true;
            try
            {
                AuditLogs.Add(log);
                await base.SaveChangesAsync();
            }
            finally
            {
                _isAuditing = false;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Dönem kilidi kontrolü
        // ─────────────────────────────────────────────────────────────────────

        // Hangi tablolar dönem kilidi kapsamındadır?
        private static readonly HashSet<string> _periodLockedTables = new(StringComparer.OrdinalIgnoreCase)
        {
            "FscLots", "FscSerials", "ProductionDetails", "StockMovements",
            "SalesOrders", "SalesOrderLines", "WasteManagements", "WorkOrders"
        };

        // Entity'nin dönem tarihini hangi alanından okuyacağız?
        private static readonly string[] _dateFieldPriority =
        {
            "LotDate", "ArrivalDate", "ProductionDate", "MovementDate",
            "OrderDate", "WasteDate", "PlannedDate", "CreatedDate"
        };

        private async Task CheckPeriodLockAsync(CancellationToken ct)
        {
            // Kilitli dönemleri çek
            var lockedPeriods = await AuditPeriods
                .Where(p => p.IsLocked)
                .AsNoTracking()
                .ToListAsync(ct);

            if (!lockedPeriods.Any()) return;

            foreach (var entry in ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added
                                    or EntityState.Modified
                                    or EntityState.Deleted))
            {
                var tableName = entry.Metadata.GetTableName() ?? "";
                if (!_periodLockedTables.Contains(tableName)) continue;

                // Tarihi bul
                DateTime? recordDate = null;
                foreach (var field in _dateFieldPriority)
                {
                    var prop = entry.Metadata.FindProperty(field);
                    if (prop == null) continue;
                    var raw = entry.State == EntityState.Deleted
                        ? entry.Property(field).OriginalValue
                        : entry.Property(field).CurrentValue;
                    if (raw is DateTime dt) { recordDate = dt; break; }
                }

                if (recordDate == null) continue;

                var locked = lockedPeriods.FirstOrDefault(p =>
                    recordDate.Value.Date >= p.StartDate.Date &&
                    recordDate.Value.Date <= p.EndDate.Date);

                if (locked != null)
                    throw new PeriodLockedException(
                        $"'{locked.Year}' denetim dönemi ({locked.StartDate:dd.MM.yyyy}–{locked.EndDate:dd.MM.yyyy}) " +
                        $"kilitlidir. Bu tarihe ait kayıtlarda değişiklik yapamazsınız.");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Yardımcı: audit item'larını oluştur
        // ─────────────────────────────────────────────────────────────────────
        private List<AuditItem> BuildAuditItems()
        {
            var user = _http?.HttpContext?.Session.GetString("Username") ?? "SYSTEM";
            var ip   = _http?.HttpContext?.Connection.RemoteIpAddress?.ToString();
            var now  = DateTime.Now;

            var items = new List<AuditItem>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var tableName = entry.Metadata.GetTableName() ?? entry.Metadata.ClrType.Name;

                // AuditLog tablosunu atla
                if (_skipAuditTables.Contains(entry.Metadata.ClrType.Name))
                    continue;

                var item = new AuditItem
                {
                    TableName = tableName,
                    ChangedBy = user,
                    ChangedAt = now,
                    IpAddress = ip,
                    EntityRef = entry.Entity
                };

                switch (entry.State)
                {
                    case EntityState.Added:
                        item.Action   = "INSERT";
                        item.NeedsId  = true;
                        item.NewValues = SerializeProperties(entry, entry.Properties, useOriginal: false);
                        break;

                    case EntityState.Deleted:
                        item.Action    = "DELETE";
                        item.RecordId  = GetId(entry);
                        item.OldValues = SerializeProperties(entry, entry.Properties, useOriginal: true);
                        break;

                    case EntityState.Modified:
                        item.Action   = "UPDATE";
                        item.RecordId = GetId(entry);
                        var changed   = entry.Properties
                            .Where(p => p.IsModified && !_skipAuditProps.Contains(p.Metadata.Name))
                            .ToList();
                        if (!changed.Any()) continue;
                        item.OldValues = SerializeChangedProperties(changed, useOriginal: true);
                        item.NewValues = SerializeChangedProperties(changed, useOriginal: false);
                        break;

                    default:
                        continue;
                }

                items.Add(item);
            }

            return items;
        }

        private static int? GetId(EntityEntry entry)
        {
            var idProp = entry.Metadata.FindProperty("Id");
            if (idProp == null) return null;
            var val = entry.Property("Id").CurrentValue;
            return val is int i ? i : null;
        }

        private static string? SerializeProperties(EntityEntry entry,
            IEnumerable<PropertyEntry> props, bool useOriginal)
        {
            var dict = new Dictionary<string, object?>();
            foreach (var p in props)
            {
                if (_skipAuditProps.Contains(p.Metadata.Name)) continue;
                dict[p.Metadata.Name] = useOriginal ? p.OriginalValue : p.CurrentValue;
            }
            return dict.Any() ? JsonSerializer.Serialize(dict) : null;
        }

        private static string? SerializeChangedProperties(
            IEnumerable<PropertyEntry> props, bool useOriginal)
        {
            var dict = new Dictionary<string, object?>();
            foreach (var p in props)
                dict[p.Metadata.Name] = useOriginal ? p.OriginalValue : p.CurrentValue;
            return dict.Any() ? JsonSerializer.Serialize(dict) : null;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Yardımcı sınıf
        // ─────────────────────────────────────────────────────────────────────
        private class AuditItem
        {
            public string   TableName { get; set; } = "";
            public string   Action    { get; set; } = "";
            public int?     RecordId  { get; set; }
            public string?  OldValues { get; set; }
            public string?  NewValues { get; set; }
            public string?  ChangedBy { get; set; }
            public DateTime ChangedAt { get; set; }
            public string?  IpAddress { get; set; }
            public bool     NeedsId   { get; set; }
            public object?  EntityRef { get; set; }

            public AuditLog ToLog() => new()
            {
                TableName = TableName,
                RecordId  = RecordId,
                Action    = Action,
                OldValues = OldValues,
                NewValues = NewValues,
                ChangedBy = ChangedBy,
                ChangedAt = ChangedAt,
                IpAddress = IpAddress
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        //  OnModelCreating
        // ─────────────────────────────────────────────────────────────────────
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var now        = new DateTime(2026, 02, 25);
            string sysUser = "SYSTEM";

            // Seed: FSC Tipleri
            modelBuilder.Entity<FscType>().HasData(
                new FscType { Id = 1, Code = "FSC-100", Name = "FSC %100", Description = "TAMAMI SERTIFIKALI", IsActive = true, CreatedBy = sysUser, CreatedDate = now },
                new FscType { Id = 2, Code = "FSC-MIX", Name = "FSC MIX",  Description = "KARISIM ICERIK",    IsActive = true, CreatedBy = sysUser, CreatedDate = now }
            );

            // İlişki Tanımlamaları
            modelBuilder.Entity<ProductRecipe>()
                .HasOne(pr => pr.ParentProduct).WithMany(p => p.ParentRecipes)
                .HasForeignKey(pr => pr.ParentProductId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProductRecipe>()
                .HasOne(pr => pr.ChildProduct).WithMany(p => p.ChildRecipes)
                .HasForeignKey(pr => pr.ChildProductId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProductRecipe>()
                .Property(pr => pr.StandardQuantity)
                .HasColumnType("decimal(18,6)");

            modelBuilder.Entity<Product>().HasOne(p => p.ProductGroup).WithMany().HasForeignKey(p => p.ProductGroupId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Product>().HasOne(p => p.FscType).WithMany().HasForeignKey(p => p.FscTypeId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Product>().HasOne(p => p.PaperType).WithMany().HasForeignKey(p => p.PaperTypeId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkOrder>()
                .HasOne(w => w.Product).WithMany().HasForeignKey(w => w.ProductId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<WorkOrder>()
                .HasOne(w => w.Machine).WithMany().HasForeignKey(w => w.MachineId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductionDetail>()
                .HasOne(d => d.WorkOrder).WithMany(w => w.ProductionDetails).HasForeignKey(d => d.WorkOrderId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProductionDetail>()
                .HasOne(d => d.FscSerial).WithMany(s => s.ProductionDetails).HasForeignKey(d => d.FscSerialId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProductionDetail>()
                .HasOne(d => d.Machine).WithMany().HasForeignKey(d => d.MachineId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProductionDetail>()
                .HasOne(d => d.WorkOrderRecipe).WithMany()
                .HasForeignKey(d => d.WorkOrderRecipeId).OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<WorkOrderRecipe>()
                .HasOne(r => r.WorkOrder).WithMany(w => w.WorkOrderRecipes)
                .HasForeignKey(r => r.WorkOrderId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<WorkOrderRecipe>()
                .HasOne(r => r.Product).WithMany().HasForeignKey(r => r.ProductId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<WorkOrderRecipe>()
                .HasOne(r => r.FscSerial).WithMany().HasForeignKey(r => r.FscSerialId).OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SalesOrder>()
                .HasOne(s => s.Customer).WithMany().HasForeignKey(s => s.CustomerId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SalesOrderLine>()
                .HasOne(l => l.SalesOrder).WithMany(s => s.Lines).HasForeignKey(l => l.SalesOrderId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SalesOrderLine>()
                .HasOne(l => l.Product).WithMany().HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SalesOrderLine>()
                .HasOne(l => l.WorkOrder).WithMany().HasForeignKey(l => l.WorkOrderId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WasteManagement>()
                .HasOne(w => w.WorkOrder).WithMany().HasForeignKey(w => w.WorkOrderId).OnDelete(DeleteBehavior.SetNull);

            // Birim Dönüşüm
            modelBuilder.Entity<UnitConversion>()
                .HasOne(u => u.ProductGroup).WithMany().HasForeignKey(u => u.ProductGroupId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<UnitConversion>()
                .HasOne(u => u.Product).WithMany().HasForeignKey(u => u.ProductId).OnDelete(DeleteBehavior.SetNull);
            // Factor: 0.0000001 gibi çok küçük değerleri saklayabilmek için decimal(18,7)
            modelBuilder.Entity<UnitConversion>()
                .Property(u => u.Factor).HasColumnType("decimal(18,7)");

            // RBAC
            modelBuilder.Entity<UserGroup>().HasKey(ug => new { ug.UserId, ug.GroupId });
            modelBuilder.Entity<UserGroup>()
                .HasOne(ug => ug.User).WithMany(u => u.UserGroups).HasForeignKey(ug => ug.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<UserGroup>()
                .HasOne(ug => ug.Group).WithMany(g => g.UserGroups).HasForeignKey(ug => ug.GroupId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupPermission>().HasKey(gp => new { gp.GroupId, gp.ModuleId });
            modelBuilder.Entity<GroupPermission>()
                .HasOne(gp => gp.Group).WithMany(g => g.Permissions).HasForeignKey(gp => gp.GroupId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<GroupPermission>()
                .HasOne(gp => gp.Module).WithMany(m => m.GroupPermissions).HasForeignKey(gp => gp.ModuleId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserPermissionOverride>().HasKey(up => new { up.UserId, up.ModuleId });
            modelBuilder.Entity<UserPermissionOverride>()
                .HasOne(up => up.User).WithMany(u => u.PermissionOverrides).HasForeignKey(up => up.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<UserPermissionOverride>()
                .HasOne(up => up.Module).WithMany(m => m.UserOverrides).HasForeignKey(up => up.ModuleId).OnDelete(DeleteBehavior.Cascade);

            // H5 -- StockMovements composite index: ProductId + DocumentDate sorgularini hizlandirir
            modelBuilder.Entity<StockMovement>()
                .HasIndex(s => new { s.ProductId, s.DocumentDate })
                .HasDatabaseName("IX_StockMovements_ProductId_DocumentDate");

            // H7 -- FscSerial decimal precision: agirlik alanlari 4 hane ondalik hassasiyetle
            modelBuilder.Entity<FscSerial>()
                .Property(s => s.InitialWeight).HasColumnType("decimal(18,4)");
            modelBuilder.Entity<FscSerial>()
                .Property(s => s.CurrentWeight).HasColumnType("decimal(18,4)");
            modelBuilder.Entity<FscSerial>()
                .Property(s => s.OriginalQuantity).HasColumnType("decimal(18,4)");

            // M11 -- FscLot.SourceSerialId FK + index: donusum izi icin CoC FK tanimi
            // Navigation property (SourceSerial) ile eslestirildi — anonim HasOne<> yerine
            modelBuilder.Entity<FscLot>()
                .HasOne(l => l.SourceSerial).WithMany()
                .HasForeignKey(l => l.SourceSerialId).OnDelete(DeleteBehavior.ClientSetNull);
            modelBuilder.Entity<FscLot>()
                .HasIndex(l => l.SourceSerialId)
                .HasDatabaseName("IX_FscLots_SourceSerialId");

            modelBuilder.Entity<AppUser>().HasIndex(u => u.Username).IsUnique();

            // Veri bütünlüğü: aynı lot içinde aynı seri numarası girilemesin
            modelBuilder.Entity<FscSerial>()
                .HasIndex(s => new { s.LotId, s.SerialNo })
                .IsUnique()
                .HasDatabaseName("IX_FscSerials_LotId_SerialNo_Unique");

            // Negatif stok koruması: CurrentWeight sıfırın altına inemez
            modelBuilder.Entity<FscSerial>()
                .ToTable(t => t.HasCheckConstraint("CK_FscSerials_CurrentWeight", "[CurrentWeight] >= -0.001"));

            // AuditLog: büyük Id (long) için IDENTITY
            modelBuilder.Entity<AuditLog>().HasKey(a => a.Id);

            // Modül seed
            modelBuilder.Entity<PermissionModule>().HasData(
                new PermissionModule { Id =  1, Code = "PURCHASE",     DisplayName = "Hammadde Girişi",     IconClass = "fas fa-boxes",         SortOrder =  1 },
                new PermissionModule { Id =  2, Code = "PRODUCTION",   DisplayName = "Üretim",              IconClass = "fas fa-industry",       SortOrder =  2 },
                new PermissionModule { Id =  3, Code = "SALES",        DisplayName = "Satış / Sevkiyat",    IconClass = "fas fa-truck",          SortOrder =  3 },
                new PermissionModule { Id =  4, Code = "STOCK",        DisplayName = "Stok Yönetimi",       IconClass = "fas fa-warehouse",      SortOrder =  4 },
                new PermissionModule { Id =  5, Code = "CUSTOMERS",    DisplayName = "Müşteriler",          IconClass = "fas fa-handshake",      SortOrder =  5 },
                new PermissionModule { Id =  6, Code = "SUPPLIERS",    DisplayName = "Tedarikçiler",        IconClass = "fas fa-truck-loading",  SortOrder =  6 },
                new PermissionModule { Id =  7, Code = "PRODUCTS",     DisplayName = "Ürünler",             IconClass = "fas fa-tag",            SortOrder =  7 },
                new PermissionModule { Id =  8, Code = "REPORTS",      DisplayName = "Raporlar",            IconClass = "fas fa-chart-bar",      SortOrder =  8 },
                new PermissionModule { Id =  9, Code = "AUDIT_PERIOD", DisplayName = "Denetim Dönemleri",  IconClass = "fas fa-calendar-check", SortOrder =  9 },
                new PermissionModule { Id = 10, Code = "SETTINGS",     DisplayName = "Ayarlar / Tanımlar", IconClass = "fas fa-cog",            SortOrder = 10 },
                new PermissionModule { Id = 11, Code = "ETL",          DisplayName = "ERP Entegrasyon",    IconClass = "fas fa-sync-alt",       SortOrder = 11 },
                new PermissionModule { Id = 12, Code = "USERS",        DisplayName = "Kullanıcı Yönetimi", IconClass = "fas fa-users-cog",      SortOrder = 12 }
            );
        }
    }
}

