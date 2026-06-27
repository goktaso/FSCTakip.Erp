using FSCTakip.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FSCTakip.WebUI.Controllers
{
    /// <summary>
    /// Yüklenmiş belgeleri (irsaliye/fatura PDF vb.) servis eder.
    /// Yapılandırılmış köke göre çözer; wwwroot dışı mutlak yolları da destekler.
    /// [AllowAnonymous] — iframe/yeni sekme görüntülemede oturum yönlendirmesi olmaması için
    /// (dosyalar zaten tahmin edilemez GUID adlıdır).
    /// </summary>
    [AllowAnonymous]
    public class DocumentController : Controller
    {
        private readonly IFileStorageService _storage;

        public DocumentController(IFileStorageService storage)
        {
            _storage = storage;
        }

        // GET /Document/Serve?key=invoices/2026/{guid}.pdf
        [HttpGet]
        public IActionResult Serve(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return NotFound();
            if (!_storage.TryResolve(key, out var path, out var mime))
                return NotFound("Belge bulunamadı.");
            return PhysicalFile(path, mime);
        }
    }
}
