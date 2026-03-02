using Microsoft.EntityFrameworkCore;
using FSCTakip.Core.Entities;
using System;

namespace FSCTakip.DataAc.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        #region Tanımlamalar (Parametrik Tablolar)
        public DbSet<FscType> FscTypes { get; set; }
        public DbSet<PaperType> PaperTypes { get; set; }
        public DbSet<PaperColor> PaperColors { get; set; }
        // public DbSet<ProductGrammage> PaperWeights { get; set; } // Aynı entity iki farklı DbSet olamaz, gerekirse aşağıdakini kullanın.
        public DbSet<PaperWidth> PaperWidths { get; set; }
        public DbSet<Machine> Machines { get; set; }
        public DbSet<BagType> BagTypes { get; set; }
        public DbSet<ProductGroup> ProductGroups { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        #endregion
        public DbSet<PaperWeight> PaperWeights { get; set; }
        #region Ticari ve Operasyonel Tablolar
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductGrammage> ProductGrammages { get; set; }
        public DbSet<ProductRecipe> ProductRecipes { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }
        public DbSet<WorkOrderRecipe> WorkOrderRecipes { get; set; }
        public DbSet<ProductionDetail> ProductionDetails { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<FscLot> FscLots { get; set; }
        public DbSet<FscSerial> FscSerials { get; set; }
        public DbSet<WasteManagement> WasteManagements { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- SEED DATA (Zorunlu Alanlar Eklendi) ---
            var now = new DateTime(2026, 02, 25); // Sabit bir tarih veya DateTime.Now
            string systemUser = "System";

            // FSC Tipleri
            modelBuilder.Entity<FscType>().HasData(
                new FscType { Id = 1, Code = "FSC-100", Name = "FSC %100", Description = "Tamamı sertifikalı", IsActive = true, CreatedBy = systemUser, CreatedDate = now },
                new FscType { Id = 2, Code = "FSC-MIX", Name = "FSC Mix", Description = "Karışım içerik", IsActive = true, CreatedBy = systemUser, CreatedDate = now }
            );

            // Kağıt Tipleri
            modelBuilder.Entity<PaperType>().HasData(
                new PaperType { Id = 1, Name = "Kraft Kağıt", ShortCode = "KRT", IsActive = true, CreatedBy = systemUser, CreatedDate = now },
                new PaperType { Id = 2, Name = "Sülfit Kağıt", ShortCode = "SLF", IsActive = true, CreatedBy = systemUser, CreatedDate = now }
            );

            // Makineler
            modelBuilder.Entity<Machine>().HasData(
                new Machine { Id = 1, Name = "8 Renk Flexo", Code = "M-01", Type = "Matbaa", IsActive = true, CreatedBy = systemUser, CreatedDate = now },
                new Machine { Id = 2, Name = "Kare Dip Kesim", Code = "K-01", Type = "Kesim", IsActive = true, CreatedBy = systemUser, CreatedDate = now }
            );

            // Depolar
            modelBuilder.Entity<Warehouse>().HasData(
                new Warehouse { Id = 1, Name = "Hammadde Deposu", Code = "DEP-01", IsActive = true, CreatedBy = systemUser, CreatedDate = now },
                new Warehouse { Id = 2, Name = "Mamul Deposu", Code = "DEP-02", IsActive = true, CreatedBy = systemUser, CreatedDate = now }
            );

            // ProductRecipes Tablosu İçin Kritik Düzenleme
            modelBuilder.Entity<ProductRecipe>()
                .HasOne(pr => pr.ParentProduct)
                .WithMany(p => p.ParentRecipes) // Navigation property isimlerinize göre güncelleyin
                .HasForeignKey(pr => pr.ParentProductId)
                .OnDelete(DeleteBehavior.Restrict); // Cascade yerine Restrict

            modelBuilder.Entity<ProductRecipe>()
                .HasOne(pr => pr.ChildProduct)
                .WithMany(p => p.ChildRecipes) // Navigation property isimlerinize göre güncelleyin
                .HasForeignKey(pr => pr.ChildProductId)
                .OnDelete(DeleteBehavior.Restrict); // Cascade yerine Restrict
        }
    }
}