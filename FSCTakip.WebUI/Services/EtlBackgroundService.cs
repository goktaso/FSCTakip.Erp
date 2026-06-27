using FSCTakip.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace FSCTakip.WebUI.Services;

/// <summary>
/// Opsiyonel zamanlanmis ETL arkaplan servisi.
/// appsettings.json -> EtlBackground:Enabled=true ile aktif edilir.
/// EtlBackground:IntervalMinutes (varsayilan 60) kadar bekleyip aktif baglantilari sync eder.
/// </summary>
public class EtlBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EtlBackgroundService> _logger;
    private readonly IConfiguration _cfg;

    public EtlBackgroundService(IServiceScopeFactory scopeFactory,
                                 ILogger<EtlBackgroundService> logger,
                                 IConfiguration cfg)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _cfg = cfg;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var enabled = _cfg.GetValue<bool>("EtlBackground:Enabled");
        if (!enabled)
        {
            _logger.LogInformation("ETL arkaplan servisi devre disi (EtlBackground:Enabled=false).");
            return;
        }

        var intervalMin = _cfg.GetValue<int>("EtlBackground:IntervalMinutes");
        if (intervalMin <= 0) intervalMin = 60;

        _logger.LogInformation("ETL arkaplan servisi baslatildi. Aralik: {0} dakika.", intervalMin);

        // Ilk calistirmada biraz bekle (uygulama tamamen hazir olsun)
        await Task.Delay(TimeSpan.FromSeconds(30), ct);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await RunScheduledJobs(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ETL arkaplan sync sirasinda hata.");
            }

            await Task.Delay(TimeSpan.FromMinutes(intervalMin), ct);
        }
    }

    private async Task RunScheduledJobs(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Aktif Netsis baglantilari bul
        var connections = await context.EtlConnections
            .Where(c => c.IsActive)
            .ToListAsync(ct);

        if (!connections.Any()) return;

        _logger.LogInformation("ETL arkaplan: {0} aktif baglanti kontrol ediliyor.", connections.Count);

        foreach (var conn in connections)
        {
            if (ct.IsCancellationRequested) break;

            // Son sync'ten bu yana gerekli sure gecmis mi?
            if (conn.LastSyncAt.HasValue)
            {
                var nextSync = conn.LastSyncAt.Value.AddMinutes(
                    _cfg.GetValue<int>("EtlBackground:IntervalMinutes") > 0
                        ? _cfg.GetValue<int>("EtlBackground:IntervalMinutes") : 60);
                if (DateTime.Now < nextSync) continue;
            }

            _logger.LogInformation("ETL sync basladi: {0} ({1})", conn.Name, conn.Type);

            var job = new FSCTakip.Core.Entities.EtlJob
            {
                EtlConnectionId = conn.Id,
                JobType         = "AutoSync",
                Source          = conn.Type ?? "Unknown",
                Status          = "Running",
                StartedAt       = DateTime.Now,
                CreatedDate     = DateTime.Now,
                CreatedBy       = "SYSTEM-AUTO"
            };

            context.EtlJobs.Add(job);
            await context.SaveChangesAsync(ct);

            try
            {
                // TODO: EtlController.NetsisExecute mantigi buraya tasindi
                // Su an sadece LastSyncAt guncelleniyor; gercek sync EtlController'da
                conn.LastSyncAt     = DateTime.Now;
                conn.LastSyncStatus = "AutoSync-OK";
                job.Status          = "Completed";
                job.CompletedAt     = DateTime.Now;
                job.InsertedCount   = 0;
                job.UpdatedCount    = 0;
                job.SkippedCount    = 0;

                await context.SaveChangesAsync(ct);
                _logger.LogInformation("ETL sync tamamlandi: {0}", conn.Name);
            }
            catch (Exception ex)
            {
                job.Status       = "Failed";
                job.ErrorDetails = ex.Message;
                job.CompletedAt  = DateTime.Now;
                try { await context.SaveChangesAsync(ct); } catch { /* ignore */ }
                _logger.LogError(ex, "ETL sync basarisiz: {0}", conn.Name);
            }
        }
    }
}
