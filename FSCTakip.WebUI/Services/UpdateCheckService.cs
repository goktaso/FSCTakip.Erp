using System.IO.Compression;
using System.Net.Http.Json;
using System.Text.Json;

namespace FSCTakip.WebUI.Services
{
    public class UpdateCheckResult
    {
        public bool    Enabled        { get; init; }
        public string  CurrentVersion { get; init; } = "";
        public string? LatestVersion  { get; init; }
        public bool    HasUpdate      { get; init; }
        public string? Notes          { get; init; }
        public string? Error          { get; init; }
    }

    /// <summary>
    /// İnternet erişimi olan müşterilerde yeni sürüm kontrolü/indirmesi.
    /// Kaynak: goktaso/FSCTakip-Releases (private, sadece derlenmiş .zip paketleri
    /// barındırır — kaynak kod hiç girmez). DB migration'ını OTOMATİK UYGULAMAZ;
    /// sadece indirip açar, gerçek uygulama adımı (site durdur/dosya değiştir/
    /// başlat) admin tarafından installer/update-engine.ps1 ile elle çalıştırılır.
    /// </summary>
    public interface IUpdateCheckService
    {
        Task<UpdateCheckResult> CheckAsync();
        Task<(bool success, string message, string? extractedPath)> DownloadLatestAsync();
    }

    public class UpdateCheckService : IUpdateCheckService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public UpdateCheckService(HttpClient http, IConfiguration config, IWebHostEnvironment env)
        {
            _http   = http;
            _config = config;
            _env    = env;
        }

        private string CurrentVersion
        {
            get
            {
                var path = Path.Combine(_env.ContentRootPath, "VERSION.txt");
                return File.Exists(path) ? File.ReadAllText(path).Trim() : "bilinmiyor";
            }
        }

        // appsettings.json'a elle kopyala-yapıştırılan Repo/Token değerleri görünmez
        // karakterler (BOM, sıfır-genişlikli boşluk, akıllı tırnak vb.) taşıyabilir -
        // .NET HttpClient header değerinde ASCII-dışı karakter görünce
        // "Request headers must contain only ASCII characters" ile patlar (2026-07-19,
        // VM testinde bulundu). Baştaki/sondaki boşluk kırpılır, ASCII-dışı karakterler
        // sessizce atılır.
        private static string? CleanConfigValue(string? value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            var trimmed = value.Trim();
            var sb = new System.Text.StringBuilder(trimmed.Length);
            foreach (var c in trimmed)
                if (c >= 32 && c <= 126) sb.Append(c);
            return sb.ToString();
        }

        private HttpRequestMessage BuildGitHubRequest(string url, string token, string accept)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Add("Authorization", $"Bearer {token}");
            req.Headers.Add("User-Agent", "FscErp-UpdateChecker");
            req.Headers.Add("Accept", accept);
            return req;
        }

        public async Task<UpdateCheckResult> CheckAsync()
        {
            var current = CurrentVersion;
            var enabled = _config.GetValue("UpdateCheck:Enabled", false);
            if (!enabled)
                return new UpdateCheckResult { Enabled = false, CurrentVersion = current };

            var repo  = CleanConfigValue(_config["UpdateCheck:Repo"]);
            var token = CleanConfigValue(_config["UpdateCheck:Token"]);
            if (string.IsNullOrWhiteSpace(repo) || string.IsNullOrWhiteSpace(token))
                return new UpdateCheckResult { Enabled = true, CurrentVersion = current, Error = "UpdateCheck:Repo veya Token appsettings.json'da eksik." };

            try
            {
                var req  = BuildGitHubRequest($"https://api.github.com/repos/{repo}/releases/latest", token, "application/vnd.github+json");
                var resp = await _http.SendAsync(req);
                if (!resp.IsSuccessStatusCode)
                    return new UpdateCheckResult { Enabled = true, CurrentVersion = current, Error = $"GitHub API hatası: HTTP {(int)resp.StatusCode}" };

                var json   = await resp.Content.ReadFromJsonAsync<JsonElement>();
                var tag    = json.GetProperty("tag_name").GetString() ?? "";
                var latest = tag.TrimStart('v', 'V');
                var notes  = json.TryGetProperty("body", out var b) ? b.GetString() : null;

                return new UpdateCheckResult
                {
                    Enabled        = true,
                    CurrentVersion = current,
                    LatestVersion  = latest,
                    HasUpdate      = !string.IsNullOrWhiteSpace(latest)
                                     && !string.Equals(latest, current, StringComparison.OrdinalIgnoreCase),
                    Notes          = notes
                };
            }
            catch (Exception ex)
            {
                return new UpdateCheckResult { Enabled = true, CurrentVersion = current, Error = ex.Message };
            }
        }

        public async Task<(bool success, string message, string? extractedPath)> DownloadLatestAsync()
        {
            var repo  = CleanConfigValue(_config["UpdateCheck:Repo"]);
            var token = CleanConfigValue(_config["UpdateCheck:Token"]);
            var downloadFolder = _config["UpdateCheck:DownloadFolder"];
            if (string.IsNullOrWhiteSpace(downloadFolder))
                downloadFolder = Path.Combine(_env.ContentRootPath, "..", "FscErpUpdates");

            if (string.IsNullOrWhiteSpace(repo) || string.IsNullOrWhiteSpace(token))
                return (false, "UpdateCheck:Repo veya Token appsettings.json'da eksik.", null);

            try
            {
                var req  = BuildGitHubRequest($"https://api.github.com/repos/{repo}/releases/latest", token, "application/vnd.github+json");
                var resp = await _http.SendAsync(req);
                if (!resp.IsSuccessStatusCode)
                    return (false, $"GitHub API hatası: HTTP {(int)resp.StatusCode}", null);

                var json   = await resp.Content.ReadFromJsonAsync<JsonElement>();
                var tag    = (json.GetProperty("tag_name").GetString() ?? "").TrimStart('v', 'V');
                var assets = json.GetProperty("assets");
                if (assets.GetArrayLength() == 0)
                    return (false, "Release'de dosya (asset) bulunamadı.", null);

                // Birden fazla asset varsa güncelleme zip'ini isimden seç (FscErpUpdate*.zip),
                // yoksa ilk asset'e düş.
                var asset = assets[0];
                foreach (var a in assets.EnumerateArray())
                {
                    var nm = a.TryGetProperty("name", out var an) ? an.GetString() : null;
                    if (nm != null && nm.StartsWith("FscErpUpdate", StringComparison.OrdinalIgnoreCase))
                    {
                        asset = a;
                        break;
                    }
                }
                // NOT: browser_download_url DEĞİL — private repo'da asset'in kendi API
                // url'i (Accept: application/octet-stream ile) kullanılmalı, aksi halde
                // tarayıcı-içi oturum ister, token ile indirilemez.
                var assetUrl  = asset.GetProperty("url").GetString()
                                 ?? throw new InvalidOperationException("Asset url alınamadı.");
                var assetName = asset.TryGetProperty("name", out var n) ? n.GetString() : null;
                assetName ??= $"FscErpUpdate-{tag}.zip";

                var dlReq  = BuildGitHubRequest(assetUrl, token, "application/octet-stream");
                var dlResp = await _http.SendAsync(dlReq);
                if (!dlResp.IsSuccessStatusCode)
                    return (false, $"Dosya indirilemedi: HTTP {(int)dlResp.StatusCode}", null);

                Directory.CreateDirectory(downloadFolder);
                var zipPath = Path.Combine(downloadFolder, assetName);
                await using (var fs = File.Create(zipPath))
                    await dlResp.Content.CopyToAsync(fs);

                var extractPath = Path.Combine(downloadFolder, $"v{tag}");
                if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
                ZipFile.ExtractToDirectory(zipPath, extractPath);

                return (true, $"İndirildi ve açıldı: {extractPath}", extractPath);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
    }
}
