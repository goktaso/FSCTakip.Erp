using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Win32;

namespace FSCTakip.WebUI.Services
{
    public enum LicenseState { Valid, Missing, Invalid, Expired, MachineMismatch }

    public class LicenseInfo
    {
        public LicenseState State       { get; init; }
        public string?      LicensedTo  { get; init; }
        public DateTime?    ValidUntil  { get; init; }
        public string?      LicenseId   { get; init; }
        public string       MachineKey  { get; init; } = "";
        public string?      Error       { get; init; }
    }

    /// <summary>
    /// Çevrimdışı lisans doğrulama: RSA-2048 imzalı license.lic dosyası + makine bağlama.
    /// Özel anahtar YALNIZ ARD'dedir (repo dışı); buradaki genel anahtar yalnız imza doğrular.
    /// Lisans üretimi: tools/license_gen.py
    /// </summary>
    public interface ILicenseService
    {
        LicenseInfo Current { get; }
        void Invalidate(); // yeni dosya yüklendiğinde cache'i düşür
    }

    public class LicenseService : ILicenseService
    {
        // ARD genel anahtarı (imza doğrulama) — özel anahtar repoya girmez.
        private const string PublicKeyPem = @"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAkwNq23eO//X3NftQgnAU
OQ6Oeb5KAvGLoQnG58nvejlCdOpIJ9/cMgnD7EGtbtVy+Z2oI+DGcBqc1H8s6qQV
UxedhhtzSkyFlunppgiI+OV2lFaK+fUyRN1SyR8WruOFvEvI7n3sleY5PpyUDX42
Or8+yvGHA5BljEsEVh407sc+qt3X1/eegk1B/7nmQyu+syyQW9ZwRteMIT5ndh4b
nDLi21t2RgaaQytv6WBdKX4mqXOZwl0pjmeV96PL0DAkT8db90tuE+17gy6kEHjw
qCFALiMJ+EvPCl6Nma7EG125rONt9P1w9oAr+0vY9yYMvlSWJhPBB9wrAu9tWEx1
awIDAQAB
-----END PUBLIC KEY-----";

        private readonly string _licensePath;
        private readonly object _lock = new();
        private LicenseInfo? _cached;
        private DateTime _cachedAt;
        private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

        public LicenseService(IConfiguration cfg, IWebHostEnvironment env)
        {
            var configured = cfg["License:Path"];
            _licensePath = string.IsNullOrWhiteSpace(configured)
                ? Path.Combine(env.ContentRootPath, "license.lic")
                : configured;
        }

        public LicenseInfo Current
        {
            get
            {
                lock (_lock)
                {
                    if (_cached == null || DateTime.UtcNow - _cachedAt > CacheTtl)
                    {
                        _cached = Evaluate();
                        _cachedAt = DateTime.UtcNow;
                    }
                    return _cached;
                }
            }
        }

        public void Invalidate()
        {
            lock (_lock) { _cached = null; }
        }

        /// <summary>Makine parmak izi: SHA256(Windows MachineGuid) ilk 16 hex karakter.</summary>
        public static string GetMachineKey()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
                var guid = key?.GetValue("MachineGuid")?.ToString() ?? Environment.MachineName;
                var hash = SHA256.HashData(Encoding.UTF8.GetBytes(guid));
                return Convert.ToHexString(hash)[..16].ToLowerInvariant();
            }
            catch
            {
                var hash = SHA256.HashData(Encoding.UTF8.GetBytes(Environment.MachineName));
                return Convert.ToHexString(hash)[..16].ToLowerInvariant();
            }
        }

        private LicenseInfo Evaluate()
        {
            var machineKey = GetMachineKey();
            if (!File.Exists(_licensePath))
                return new LicenseInfo { State = LicenseState.Missing, MachineKey = machineKey };

            try
            {
                var result = ValidateLicenseContent(File.ReadAllText(_licensePath), machineKey, DateTime.Today);
                return result;
            }
            catch (Exception ex)
            {
                return new LicenseInfo { State = LicenseState.Invalid, MachineKey = machineKey, Error = ex.Message };
            }
        }

        /// <summary>Test edilebilir çekirdek: içerik + makine + tarih ile durum döndürür.</summary>
        public static LicenseInfo ValidateLicenseContent(string content, string machineKey, DateTime today)
        {
            // Dosya biçimi: base64(payloadJson) + "." + base64(imza)
            var parts = content.Trim().Split('.');
            if (parts.Length != 2)
                return new LicenseInfo { State = LicenseState.Invalid, MachineKey = machineKey, Error = "Biçim hatalı" };

            byte[] payloadBytes, signature;
            try
            {
                payloadBytes = Convert.FromBase64String(parts[0]);
                signature    = Convert.FromBase64String(parts[1]);
            }
            catch
            {
                return new LicenseInfo { State = LicenseState.Invalid, MachineKey = machineKey, Error = "Base64 çözülemedi" };
            }

            using var rsa = RSA.Create();
            rsa.ImportFromPem(PublicKeyPem);
            if (!rsa.VerifyData(payloadBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                return new LicenseInfo { State = LicenseState.Invalid, MachineKey = machineKey, Error = "İmza geçersiz" };

            var doc = JsonSerializer.Deserialize<JsonElement>(payloadBytes);
            string? licensedTo = doc.TryGetProperty("licensedTo", out var lt) ? lt.GetString() : null;
            string? licMachine = doc.TryGetProperty("machineKey", out var mk) && mk.ValueKind == JsonValueKind.String ? mk.GetString() : null;
            string? licId      = doc.TryGetProperty("licenseId", out var li) ? li.GetString() : null;
            DateTime? validUntil = doc.TryGetProperty("validUntil", out var vu) && vu.ValueKind == JsonValueKind.String
                ? DateTime.Parse(vu.GetString()!) : null;

            if (!string.IsNullOrEmpty(licMachine) && !string.Equals(licMachine, machineKey, StringComparison.OrdinalIgnoreCase))
                return new LicenseInfo { State = LicenseState.MachineMismatch, LicensedTo = licensedTo, LicenseId = licId, ValidUntil = validUntil, MachineKey = machineKey };

            if (validUntil.HasValue && validUntil.Value.Date < today)
                return new LicenseInfo { State = LicenseState.Expired, LicensedTo = licensedTo, LicenseId = licId, ValidUntil = validUntil, MachineKey = machineKey };

            return new LicenseInfo { State = LicenseState.Valid, LicensedTo = licensedTo, LicenseId = licId, ValidUntil = validUntil, MachineKey = machineKey };
        }

        public string LicensePath => _licensePath;
    }
}
