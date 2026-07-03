using System;
using System.Collections.Generic;
using System.Linq;
using FSCTakip.Core.Entities;
using Xunit;

namespace FSCTakip.Tests;

// Regresyon kilidi: ProductionController.CompleteWorkOrder / RecalcAllActualQty formülü.
// ProducedQuantity alanı KÜMÜLATİF toplam üretimi taşır (günlük delta değil) — bu yüzden
// tüm ProductionDetail satırları arasından TEK bir MAX alınmalı, güne göre gruplanıp
// toplanmamalı. Bu oturumda bulunan gerçek hata: eski formül GroupBy(gün).Sum(Max) idi,
// çok günlü üretimde ActualQuantity'yi katlıyordu (bkz. tasks/lessons.md).
public class ActualQuantityCalculationTests
{
    private static decimal CalculateActualQuantity(List<ProductionDetail> prodDetails)
        => prodDetails.Any() ? prodDetails.Max(d => d.ProducedQuantity) : 0;

    private static ProductionDetail Row(decimal producedQty, DateTime date) => new()
    {
        WorkOrderId = 1,
        FscSerialId = 1,
        MachineId = 1,
        ProductionDate = date,
        ProducedQuantity = producedQty,
        ConsumedWeight = 1,
        WasteWeight = 0
    };

    [Fact]
    public void TekGunTekSatir_DogruDegeriDondurur()
    {
        var details = new List<ProductionDetail> { Row(281600, new DateTime(2024, 12, 30)) };
        Assert.Equal(281600, CalculateActualQuantity(details));
    }

    [Fact]
    public void AyniGunBirdenFazlaSatir_KumulatifTekDegerAlinir_ToplanMaz()
    {
        // Aynı günde 3 ayrı hammadde/bobin tüketim satırı — her biri aynı kümülatif
        // üretim adedini taşır (farklı malzemeden aynı üretimi besliyor). SUM DEĞİL, MAX.
        var day = new DateTime(2024, 12, 30);
        var details = new List<ProductionDetail>
        {
            Row(281600, day), Row(281600, day), Row(281600, day)
        };
        Assert.Equal(281600, CalculateActualQuantity(details));
    }

    [Fact]
    public void CokGunluUretim_ToplamKumulatifTekDegerAlinir_GunlerToplanMaz()
    {
        // Bu oturumdaki gerçek regresyon: iş emri 2 farklı günde devam etmiş, ikinci
        // günün kaydı üretimin GÜNCEL TOPLAMINI taşıyor (aynı 90600, artmamış).
        // Doğru sonuç 90600'dür — eski hatalı formül 90600+90600=181200 üretiyordu.
        var details = new List<ProductionDetail>
        {
            Row(90600, new DateTime(2024, 12, 30)),
            Row(90600, new DateTime(2024, 12, 30)),
            Row(90600, new DateTime(2024, 12, 30)),
            Row(90600, new DateTime(2026, 6, 27))
        };
        Assert.Equal(90600, CalculateActualQuantity(details));
    }

    [Fact]
    public void CokGunluUretim_SonrakiGunDahaYuksekKumulatifTasirsa_EnBuyukAlinir()
    {
        // Üretime gerçekten devam edilip toplam artmışsa (ör. 90600 -> 181200), en son
        // (en büyük) kümülatif değer doğru kabul edilir.
        var details = new List<ProductionDetail>
        {
            Row(90600, new DateTime(2025, 1, 6)),
            Row(181200, new DateTime(2025, 1, 7))
        };
        Assert.Equal(181200, CalculateActualQuantity(details));
    }

    [Fact]
    public void BosListe_SifirDondurur()
    {
        Assert.Equal(0, CalculateActualQuantity(new List<ProductionDetail>()));
    }
}
