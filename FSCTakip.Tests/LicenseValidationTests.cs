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
