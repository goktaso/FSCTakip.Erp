using Microsoft.EntityFrameworkCore;
using FSCTakip.Core.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

namespace FSCTakip.DataAccess.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        #region Tanımlamalar (Parametrik Tablolar)
        public DbSet<FscType> FscTypes { get; set; }
        public DbSet<PaperType> PaperTypes { get; set; }
        public DbSet<PaperColor> PaperColors { get; set; }
        public DbSet<PaperWidth> PaperWidths { get; set; }
        public DbSet<Machine> Machines { get; set; }
        public DbSet<BagType> BagTypes { get; set; }
        public DbSet<ProductGroup> ProductGroups { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<PaperWeight> PaperWeights { get; set; }
        #endregion

        #region Ticari ve Operasyonel Tablolar
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductRecipe> ProductRecipes { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }
        public DbSet<WorkOrderRecipe> WorkOrderRecipes { get; set; }
        public DbSet<ProductionDetail> ProductionDetails { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<FscLot> FscLots { get; set; }
        public DbSet<FscSerial> FscSerials { get; set; }
        // Tanımlamalar (Parametrik Tablolar) region'ı altına:
        public DbSet<Unit> Units { get; set; }
        public DbSet<AuditPeriod> AuditPeriods { get; set; }
        public DbSet<WasteManagement> WasteManagements { get; set; }
        #endregion

        // --- MERKEZİ BÜYÜK HARF DÖNÜŞTÜRME MANTIĞI ---
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            var trCulture = new CultureInfo("tr-TR");

            foreach (var entry in entries)
            {
                var stringProperties = entry.Metadata.GetProperties()
                    .Where(p => p.ClrType == typeof(string));

                foreach (var property in stringProperties)
                {
                    var currentValue = entry.Property(property.Name).CurrentValue as string;
                    if (!string.IsNullOrEmpty(currentValue))
                    {
                        // Veriyi Türkçe büyük harfe çevirerek güncelle
                        entry.Property(property.Name).CurrentValue = currentValue.ToUpper(trCulture);
                    }
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var now = new DateTime(2026, 02, 25);
            string systemUser = "SYSTEM"; // Artık burası da büyük harf :)

            // Seed Data (FSC Tipleri)
            modelBuilder.Entity<FscType>().HasData(
                new FscType { Id = 1, Code = "FSC-100", Name = "FSC %100", Description = "TAMAMI SERTIFIKALI", IsActive = true, CreatedBy = systemUser, CreatedDate = now },
                new FscType { Id = 2, Code = "FSC-MIX", Name = "FSC MIX", Description = "KARISIM ICERIK", IsActive = true, CreatedBy = systemUser, CreatedDate = now }
            );

            // ... Diğer Seed datalarınız aynı kalabilir, yukarıdaki metod zaten bunları da kapsayacaktır ...

            // İlişki Tanımlamaları
            modelBuilder.Entity<ProductRecipe>()
                .HasOne(pr => pr.ParentProduct).WithMany(p => p.ParentRecipes)
                .HasForeignKey(pr => pr.ParentProductId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductRecipe>()
                .HasOne(pr => pr.ChildProduct).WithMany(p => p.ChildRecipes)
                .HasForeignKey(pr => pr.ChildProductId).OnDelete(DeleteBehavior.Restrict);

            // Product ve Diğer İlişkiler (Restrict yapıları)
            modelBuilder.Entity<Product>().HasOne(p => p.ProductGroup).WithMany().HasForeignKey(p => p.ProductGroupId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Product>().HasOne(p => p.FscType).WithMany().HasForeignKey(p => p.FscTypeId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Product>().HasOne(p => p.PaperType).WithMany().HasForeignKey(p => p.PaperTypeId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}