using FSCTakip.WebUI.Services;
using Xunit;

namespace FSCTakip.Tests;

// FscMassBalance türetilmiş alanları: Giriş = Tüketim + Kalan formülünün tutarlılığı.
// Denetim Özeti sayfasındaki "Kütle Dengesi Sağlandı" kontrolünün temel matematiği.
public class MassBalanceTests
{
    [Fact]
    public void ToplamGiris_FscliVeFscsizToplamidir()
    {
        var mb = new FscMassBalance { FscliGiris = 100, FscsizGiris = 50, FscliKalan = 30, FscsizKalan = 10 };
        Assert.Equal(150, mb.ToplamGiris);
    }

    [Fact]
    public void ToplamTuketim_GirisEksiKalanaEsittir()
    {
        var mb = new FscMassBalance { FscliGiris = 100, FscsizGiris = 50, FscliKalan = 30, FscsizKalan = 10 };
        // Giris(150) = Tuketim(110) + Kalan(40) denkleminin diğer tarafı
        Assert.Equal(110, mb.ToplamTuketim);
        Assert.Equal(mb.ToplamGiris, mb.ToplamTuketim + mb.ToplamKalan);
    }

    [Fact]
    public void FscliVeFscsizTuketim_AyriAyriDogruHesaplanir()
    {
        var mb = new FscMassBalance { FscliGiris = 100, FscsizGiris = 50, FscliKalan = 30, FscsizKalan = 10 };
        Assert.Equal(70, mb.FscliTuketim);
        Assert.Equal(40, mb.FscsizTuketim);
    }

    [Fact]
    public void GirisSifirsa_TumAlanlarSifir()
    {
        var mb = new FscMassBalance();
        Assert.Equal(0, mb.ToplamGiris);
        Assert.Equal(0, mb.ToplamTuketim);
        Assert.Equal(0, mb.ToplamKalan);
    }

    [Fact]
    public void KalanGiristenBuyukOlamaz_DengeBozulursaTuketimNegatifCikar()
    {
        // Veri bütünlüğü sınır durumu: Kalan > Giriş ise (imkansız olması gereken durum)
        // ToplamTuketim negatif çıkar — bu, denetim raporunun "Dengesiz" işaretlemesi
        // gereken senaryodur. Formülün kendisi sessizce yanlış sonuç üretmemeli.
        var mb = new FscMassBalance { FscliGiris = 100, FscsizGiris = 0, FscliKalan = 150, FscsizKalan = 0 };
        Assert.True(mb.ToplamTuketim < 0);
    }
}
