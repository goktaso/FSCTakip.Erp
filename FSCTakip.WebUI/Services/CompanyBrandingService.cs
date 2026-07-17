using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Services
{
    /// <summary>
    /// Beyaz-etiket marka bilgisi (firma ünvanı + logo) — login, sidebar ve belge
    /// başlıklarında kullanılır. Her istekte DB'ye gitmemek için cache'lenir; Şirket
    /// Bilgileri kaydedilince <see cref="Invalidate"/> ile düşürülür (LicenseService deseni).
    /// </summary>
    public interface ICompanyBrandingService
    {
        /// <summary>Belgelerde/arayüzde görünen firma ünvanı. Boşsa ürün adına düşer.</summary>
        string CompanyName { get; }
        /// <summary>FileStorage köküne göreli logo anahtarı (örn. "branding/xyz.png") — yoksa null.</summary>
        string? LogoKey { get; }
        bool HasLogo { get; }
        void Invalidate();
    }

    public class CompanyBrandingService : ICompanyBrandingService
    {
        public const string ProductName = "FSC Takip ERP";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly object _lock = new();
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

        private (string Name, string? Logo)? _cached;
        private DateTime _cachedAt;

        public CompanyBrandingService(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

        private (string Name, string? Logo) Current
        {
            get
            {
                lock (_lock)
                {
                    if (_cached == null || DateTime.UtcNow - _cachedAt > CacheTtl)
                    {
                        _cached   = Load();
                        _cachedAt = DateTime.UtcNow;
                    }
                    return _cached.Value;
                }
            }
        }

        private (string Name, string? Logo) Load()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var cs = ctx.CompanySettings.AsNoTracking().FirstOrDefault();
                var name = string.IsNullOrWhiteSpace(cs?.CompanyName) ? ProductName : cs!.CompanyName;
                var logo = string.IsNullOrWhiteSpace(cs?.LogoPath) ? null : cs!.LogoPath;
                return (name, logo);
            }
            catch
            {
                // DB henüz hazır değilse (ilk açılış/migration anı) ürün adına düş.
                return (ProductName, null);
            }
        }

        public string  CompanyName => Current.Name;
        public string? LogoKey     => Current.Logo;
        public bool    HasLogo     => !string.IsNullOrWhiteSpace(Current.Logo);

        public void Invalidate()
        {
            lock (_lock) { _cached = null; }
        }
    }
}
