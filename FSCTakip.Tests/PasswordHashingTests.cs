using System;
using System.Security.Cryptography;
using System.Text;
using FSCTakip.WebUI.Controllers;
using Xunit;

namespace FSCTakip.Tests;

// Parola hash'leme: PBKDF2 (yeni) + eski SHA256 formatı geriye dönük doğrulama.
// Güvenlik denetimi 2026-07-17 sonrası SHA256+sabit tuz → PBKDF2'ye geçiş.
public class PasswordHashingTests
{
    [Fact]
    public void HashPassword_YeniFormat_PbkdfPrefixli()
    {
        var h = AccountController.HashPassword("Secret123!");
        Assert.StartsWith("pbkdf2$", h);
        Assert.False(AccountController.IsLegacyHash(h));
    }

    [Fact]
    public void VerifyPassword_DogruParola_True()
    {
        var h = AccountController.HashPassword("Secret123!");
        Assert.True(AccountController.VerifyPassword("Secret123!", h));
    }

    [Fact]
    public void VerifyPassword_YanlisParola_False()
    {
        var h = AccountController.HashPassword("Secret123!");
        Assert.False(AccountController.VerifyPassword("yanlisparola", h));
    }

    [Fact]
    public void HashPassword_HerCagridaFarkliTuz_AyniParolaFarkliHash()
    {
        // Kullanıcıya özel rastgele tuz → aynı parola farklı hash üretir (rainbow-table önlemi)
        Assert.NotEqual(AccountController.HashPassword("ayniParola"),
                        AccountController.HashPassword("ayniParola"));
    }

    [Fact]
    public void VerifyPassword_EskiSha256Hash_GeriyeUyumlu()
    {
        // Eski format: SHA256(password + "FSCTakip_Salt_2026"), hex küçük harf.
        var legacy = LegacyHash("admin123");

        Assert.True(AccountController.IsLegacyHash(legacy));
        Assert.True(AccountController.VerifyPassword("admin123", legacy));
        Assert.False(AccountController.VerifyPassword("yanlis", legacy));
    }

    [Fact]
    public void VerifyPassword_BozukPbkdfHash_False()
    {
        Assert.False(AccountController.VerifyPassword("x", "pbkdf2$bozuk"));
        Assert.False(AccountController.VerifyPassword("x", ""));
    }

    private static string LegacyHash(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password + "FSCTakip_Salt_2026"));
        return Convert.ToHexString(bytes).ToLower();
    }
}
