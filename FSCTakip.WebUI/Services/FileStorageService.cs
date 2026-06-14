namespace FSCTakip.WebUI.Services
{
    /// <summary>
    /// Belge (PDF/JPEG/PNG) yükleme ve servis için merkezi servis.
    /// Kök yol appsettings:FileStorage:Root ile yapılandırılır; mutlak yol (örn. D:\ErpBelgeler)
    /// veya wwwroot'a göre göreli (varsayılan "uploads") olabilir. Klasör adları yapılandırılabilir.
    /// DB'de saklanan değer = köke göre göreli anahtar (örn. "invoices/2026/{guid}.pdf").
    /// </summary>
    public interface IFileStorageService
    {
        Task<string> SaveAsync(IFormFile file, string folderKey);
        bool TryResolve(string storedKey, out string physicalPath, out string contentType);
    }

    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _cfg;

        private static readonly string[] AllowedMime = { "application/pdf", "image/jpeg", "image/png" };
        private const long MaxSize = 20 * 1024 * 1024; // 20 MB

        public FileStorageService(IWebHostEnvironment env, IConfiguration cfg)
        {
            _env = env;
            _cfg = cfg;
        }

        private string Root => string.IsNullOrWhiteSpace(_cfg["FileStorage:Root"]) ? "uploads" : _cfg["FileStorage:Root"]!;

        private string RootPhysical =>
            Path.IsPathRooted(Root) ? Root : Path.Combine(_env.WebRootPath, Root);

        private string FolderName(string key) =>
            _cfg[$"FileStorage:Folders:{key}"] ?? key.ToLowerInvariant();

        public async Task<string> SaveAsync(IFormFile file, string folderKey)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("Dosya boş.");
            if (!AllowedMime.Contains(file.ContentType.ToLowerInvariant()))
                throw new InvalidOperationException("Yalnızca PDF, JPEG veya PNG dosyaları kabul edilmektedir.");
            if (file.Length > MaxSize)
                throw new InvalidOperationException("Dosya boyutu 20 MB sınırını aşıyor.");

            // Magic bytes
            using (var peek = file.OpenReadStream())
            {
                var h = new byte[4];
                await peek.ReadAsync(h, 0, 4);
                bool pdf  = h[0] == 0x25 && h[1] == 0x50 && h[2] == 0x44 && h[3] == 0x46;
                bool jpeg = h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF;
                bool png  = h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47;
                if (!pdf && !jpeg && !png)
                    throw new InvalidOperationException("Dosya içeriği geçersiz. Lütfen gerçek bir PDF, JPEG veya PNG yükleyin.");
            }

            var ext = file.ContentType.ToLowerInvariant() switch
            {
                "application/pdf" => ".pdf",
                "image/jpeg"      => ".jpg",
                "image/png"       => ".png",
                _                 => ".bin"
            };

            var folder = FolderName(folderKey);
            var year   = DateTime.Now.ToString("yyyy");
            var dir    = Path.Combine(RootPhysical, folder, year);
            Directory.CreateDirectory(dir);

            var name = $"{Guid.NewGuid():N}{ext}";
            var full = Path.Combine(dir, name);
            using (var stream = new FileStream(full, FileMode.Create))
                await file.CopyToAsync(stream);

            // Köke göre göreli anahtar (ileri eğik çizgi)
            return $"{folder}/{year}/{name}";
        }

        public bool TryResolve(string storedKey, out string physicalPath, out string contentType)
        {
            physicalPath = string.Empty;
            contentType  = "application/octet-stream";
            if (string.IsNullOrWhiteSpace(storedKey)) return false;

            var key = storedKey.Replace('\\', '/').TrimStart('/');

            // Geriye dönük uyumluluk: eski kayıtlar "/uploads/..." (wwwroot altında) saklanmıştı
            if (key.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
                physicalPath = Path.Combine(_env.WebRootPath, key.Replace('/', Path.DirectorySeparatorChar));
            else
                physicalPath = Path.Combine(RootPhysical, key.Replace('/', Path.DirectorySeparatorChar));

            // Path traversal koruması — yalnız yapılandırılmış kök veya wwwroot (legacy) altı
            var fullPath = Path.GetFullPath(physicalPath);
            var allowedRoots = new[] { Path.GetFullPath(RootPhysical), Path.GetFullPath(_env.WebRootPath) };
            if (!allowedRoots.Any(r => fullPath.StartsWith(r, StringComparison.OrdinalIgnoreCase)))
                return false;

            if (!File.Exists(physicalPath)) return false;

            contentType = Path.GetExtension(physicalPath).ToLowerInvariant() switch
            {
                ".pdf"  => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png"  => "image/png",
                _       => "application/octet-stream"
            };
            return true;
        }
    }
}
