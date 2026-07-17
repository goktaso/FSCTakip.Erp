using System;
using FSCTakip.WebUI.Services;
using Xunit;

namespace FSCTakip.Tests;

// Lisans doğrulama çekirdeği: RSA imza + makine bağlama + süre kontrolü.
// Testteki örnek lisans, gerçek ARD özel anahtarıyla imzalanmış GELİŞTİRME lisansıdır
// (makine: b6c87ee3c2563e19, süresiz) — imza matematiğini gerçek anahtar çiftiyle doğrular.
public class LicenseValidationTests
{
    private static string LoadRealLicense()
    {
        // Repo kökündeki gerçek dev lisansı (gitignored) — CI'da yoksa test SKIP mantığıyla geçer.
        var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "license.lic");
        return File.Exists(path) ? File.ReadAllText(path) : "";
    }

    [Fact]
    public void GecerliLisans_DogruMakinede_ValidDoner()
    {
        var lic = LoadRealLicense();
        if (string.IsNullOrEmpty(lic)) return; // dev lisansı yoksa atla (CI)
        var info = LicenseService.ValidateLicenseContent(lic, "b6c87ee3c2563e19", DateTime.Today);
        Assert.Equal(LicenseState.Valid, info.State);
        Assert.Contains("ARD", info.LicensedTo);
    }

    [Fact]
    public void GecerliLisans_YanlisMakinede_MachineMismatchDoner()
    {
        var lic = LoadRealLicense();
        if (string.IsNullOrEmpty(lic)) return;
        var info = LicenseService.ValidateLicenseContent(lic, "ffffffffffffffff", DateTime.Today);
        Assert.Equal(LicenseState.MachineMismatch, info.State);
    }

    [Fact]
    public void ImzasiBozukLisans_InvalidDoner()
    {
        var lic = LoadRealLicense();
        if (string.IsNullOrEmpty(lic)) return;
        // Payload'ın tek karakterini değiştir — imza artık tutmamalı
        var tampered = "A" + lic.Substring(1);
        var info = LicenseService.ValidateLicenseContent(tampered, "b6c87ee3c2563e19", DateTime.Today);
        Assert.Equal(LicenseState.Invalid, info.State);
    }

    [Fact]
    public void BicimiBozukIcerik_InvalidDoner()
    {
        var info = LicenseService.ValidateLicenseContent("bu-bir-lisans-degil", "b6c87ee3c2563e19", DateTime.Today);
        Assert.Equal(LicenseState.Invalid, info.State);
    }
}

// Deneme sürümü çekirdeği: lisans dosyası hiç yokken 30 gün boyunca sistem açık kalır.
// Başlangıç tarihi ProgramData işaret dosyası ile DB oluşturma tarihinin erken olanıdır
// (kaynak seçimi Evaluate tarafında; burada süre matematiği doğrulanır).
public class TrialLicenseTests
{
    private const string Machine = "b6c87ee3c2563e19";

    [Fact]
    public void YeniKurulum_TrialDoner_30GunKalir()
    {
        var now  = new DateTime(2026, 7, 16, 10, 0, 0, DateTimeKind.Utc);
        var info = LicenseService.EvaluateTrialCore(now, now, 30, Machine);

        Assert.Equal(LicenseState.Trial, info.State);
        Assert.Equal(30, info.TrialDaysLeft);
        Assert.True(info.IsUsable);
        Assert.Equal(new DateTime(2026, 8, 15), info.ValidUntil);
    }

    [Fact]
    public void DenemeOrtasi_KalanGunDogruHesaplanir()
    {
        var start = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var now   = new DateTime(2026, 7, 21, 23, 0, 0, DateTimeKind.Utc);
        var info  = LicenseService.EvaluateTrialCore(start, now, 30, Machine);

        Assert.Equal(LicenseState.Trial, info.State);
        Assert.Equal(10, info.TrialDaysLeft);
    }

    [Fact]
    public void SonGun_HalaKullanilabilir()
    {
        var start = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var info  = LicenseService.EvaluateTrialCore(start, new DateTime(2026, 7, 30), 30, Machine);

        Assert.Equal(LicenseState.Trial, info.State);
        Assert.Equal(1, info.TrialDaysLeft);
    }

    [Fact]
    public void SureDolduktanSonra_MissingDoner_SistemKapanir()
    {
        var start = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var info  = LicenseService.EvaluateTrialCore(start, new DateTime(2026, 7, 31), 30, Machine);

        Assert.Equal(LicenseState.Missing, info.State);
        Assert.False(info.IsUsable);
        Assert.Contains("deneme süresi", info.Error);
    }

    // Sunucu saati geriye alınarak / DB tarihi ileri görünerek deneme uzatılamamalı.
    [Fact]
    public void IleriTarihliBaslangic_SureyiUzatmaz()
    {
        var now   = new DateTime(2026, 7, 16, 0, 0, 0, DateTimeKind.Utc);
        var start = now.AddDays(90);
        var info  = LicenseService.EvaluateTrialCore(start, now, 30, Machine);

        Assert.Equal(30, info.TrialDaysLeft);
    }

    [Fact]
    public void MakineKodu_DenemedeDeGorunur()
    {
        var now  = new DateTime(2026, 7, 16, 0, 0, 0, DateTimeKind.Utc);
        var info = LicenseService.EvaluateTrialCore(now, now, 30, Machine);

        Assert.Equal(Machine, info.MachineKey);
    }
}
