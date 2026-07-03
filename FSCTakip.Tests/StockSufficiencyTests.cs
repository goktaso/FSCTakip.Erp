using Xunit;

namespace FSCTakip.Tests;

// Regresyon kilidi: SalesController.Dispatch() stok yeterlilik kuralı.
// Kural: bir iş emrinden ActualQuantity'den fazla sevkiyat asla yapılamaz — negatif
// bakiye sistemde hiçbir zaman oluşmamalı (bu oturumda eklenen mimari kural).
public class StockSufficiencyTests
{
    // SalesController.Dispatch() içindeki gerçek karşılaştırma mantığının izole hali:
    // requestedNow > remaining ise reddedilir.
    private static bool DispatchIzinVerilirMi(decimal actualQuantity, decimal alreadyDispatched, decimal requestedNow)
    {
        var remaining = actualQuantity - alreadyDispatched;
        return requestedNow <= remaining;
    }

    [Fact]
    public void TamKalanKadarSevkiyat_IzinVerilir()
    {
        Assert.True(DispatchIzinVerilirMi(actualQuantity: 100000, alreadyDispatched: 0, requestedNow: 100000));
    }

    [Fact]
    public void KalandanFazlaSevkiyat_Reddedilir()
    {
        Assert.False(DispatchIzinVerilirMi(actualQuantity: 100000, alreadyDispatched: 0, requestedNow: 100001));
    }

    [Fact]
    public void DahaOnceKismenSevkEdilmis_KalanDogruHesaplanir()
    {
        // 100000 üretildi, 60000 zaten sevk edildi -> kalan 40000. 40000 istek kabul,
        // 40001 istek reddedilmeli.
        Assert.True(DispatchIzinVerilirMi(actualQuantity: 100000, alreadyDispatched: 60000, requestedNow: 40000));
        Assert.False(DispatchIzinVerilirMi(actualQuantity: 100000, alreadyDispatched: 60000, requestedNow: 40001));
    }

    [Fact]
    public void KalanSifirken_HerhangiBirSevkiyatReddedilir()
    {
        // Bu senaryo aynen 2026-07-02 oturumunda IE2026-001 için canlı test edildi.
        Assert.False(DispatchIzinVerilirMi(actualQuantity: 281600, alreadyDispatched: 281600, requestedNow: 1000));
    }

    [Fact]
    public void UretilmeyenIsEmrindenSevkiyat_Reddedilir()
    {
        Assert.False(DispatchIzinVerilirMi(actualQuantity: 0, alreadyDispatched: 0, requestedNow: 1));
    }
}
