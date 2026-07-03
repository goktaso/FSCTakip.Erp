using System;
using System.Linq;
using System.Threading.Tasks;
using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FSCTakip.Tests;

// Regresyon kilidi: ReportsController.Warnings() / CriticalSummary() "sertifikasız
// tedarikçiden gelen FSC iddialı hammadde" tespiti. Bu oturumda bulunan gerçek hata:
// eski mantık yalnızca IsFscActive=true VE FscCode boş olan tedarikçileri yakalıyordu;
// IsFscActive=false (tamamen pasif) tedarikçiler hiç uyarıya girmiyordu (MIMCORD vakası).
public class WarningsCriticalDetectionTests
{
    private static AppDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    // ReportsController.CriticalSummary() / Warnings() içindeki gerçek sorgunun izole hali.
    private static async Task<int> UncertifiedFscLotCountAsync(AppDbContext ctx)
    {
        var lots = await ctx.FscLots
            .Include(l => l.Supplier)
            .Include(l => l.FscType)
            .Where(l => l.SourceSerialId == null &&
                        l.FscType != null && l.FscType.Code != "FSC-NONE" &&
                        (l.Supplier == null || !l.Supplier.IsFscActive || string.IsNullOrEmpty(l.Supplier.FscCode)))
            .ToListAsync();
        return lots.Count;
    }

    private static FSCTakip.Core.Entities.FscType FscMixCredit() => new() { Id = 2, Name = "FSC MIX_CREDIT", Code = "FSC-MIX-CREDIT" };
    private static FSCTakip.Core.Entities.FscType FscNone()       => new() { Id = 5, Name = "FSC'siz",       Code = "FSC-NONE" };

    [Fact]
    public async Task PasifTedarikciliFscIddialiLot_KritikOlarakYakalanir()
    {
        // MIMCORD vakasının aynısı: IsFscActive=false, FscCode boş.
        using var ctx = NewContext();
        var supplier = new Supplier { Id = 1, SupplierCode = "TED-X", Name = "Test Pasif", IsFscActive = false, FscCode = null, TaxNumber = "", TaxOffice = "", Address = "", City = "", Email = "", Phone = "" };
        ctx.Suppliers.Add(supplier);
        ctx.FscLots.Add(new FscLot { Id = 1, PartiNo = "T-1", SupplierId = 1, FscTypeId = 2, ArrivalDate = DateTime.Today, InvoiceNo = "", DispatchNo = "" });
        ctx.FscTypes.Add(FscMixCredit());
        await ctx.SaveChangesAsync();

        Assert.Equal(1, await UncertifiedFscLotCountAsync(ctx));
    }

    [Fact]
    public async Task SertifikaliTedarikciliFscIddialiLot_YakalanmaZ()
    {
        using var ctx = NewContext();
        var supplier = new Supplier { Id = 1, SupplierCode = "TED-Y", Name = "Test Sertifikali", IsFscActive = true, FscCode = "FSC-C999999", TaxNumber = "", TaxOffice = "", Address = "", City = "", Email = "", Phone = "" };
        ctx.Suppliers.Add(supplier);
        ctx.FscLots.Add(new FscLot { Id = 1, PartiNo = "T-2", SupplierId = 1, FscTypeId = 2, ArrivalDate = DateTime.Today, InvoiceNo = "", DispatchNo = "" });
        ctx.FscTypes.Add(FscMixCredit());
        await ctx.SaveChangesAsync();

        Assert.Equal(0, await UncertifiedFscLotCountAsync(ctx));
    }

    [Fact]
    public async Task DonusumleUretilenYmLot_KaynakSerialVarsaYakalanmaZ()
    {
        // Traceability düzeltmesiyle aynı ilke: SourceSerialId dolu olan (dönüşüm) lotlar
        // doğrudan tedarikçisi olmasa bile "sertifikasız" sayılmamalı — provenance kaynak
        // hammaddeden gelir, bu sorgu kapsamı dışıdır (SourceSerialId == null şartı).
        using var ctx = NewContext();
        ctx.FscTypes.Add(FscMixCredit());
        ctx.FscLots.Add(new FscLot { Id = 1, PartiNo = "T-3", SupplierId = null, FscTypeId = 2, SourceSerialId = 5, ArrivalDate = DateTime.Today, InvoiceNo = "", DispatchNo = "" });
        await ctx.SaveChangesAsync();

        Assert.Equal(0, await UncertifiedFscLotCountAsync(ctx));
    }

    [Fact]
    public async Task FscsizIddiaTasiyanLot_TedarikciSertifikasizOlsaBileYakalanmaZ()
    {
        // "FSC'siz" (FSC-NONE) zaten bir iddia taşımıyor — kontrol dışı.
        using var ctx = NewContext();
        var supplier = new Supplier { Id = 1, SupplierCode = "TED-Z", Name = "Test Pasif 2", IsFscActive = false, FscCode = null, TaxNumber = "", TaxOffice = "", Address = "", City = "", Email = "", Phone = "" };
        ctx.Suppliers.Add(supplier);
        ctx.FscTypes.Add(FscNone());
        ctx.FscLots.Add(new FscLot { Id = 1, PartiNo = "T-4", SupplierId = 1, FscTypeId = 5, ArrivalDate = DateTime.Today, InvoiceNo = "", DispatchNo = "" });
        await ctx.SaveChangesAsync();

        Assert.Equal(0, await UncertifiedFscLotCountAsync(ctx));
    }
}
