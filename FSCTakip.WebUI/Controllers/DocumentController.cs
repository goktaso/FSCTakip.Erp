using FSCTakip.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FSCTakip.WebUI.Controllers
{
    /// <summary>
    /// Yüklenmiş belgeleri (irsaliye/fatura PDF vb.) servis eder.
    /// Yapılandırılmış köke göre çözer; wwwroot dışı mutlak yolları da destekler.
    /// Oturum ŞART (SessionAuthFilter): finansal belgeler anonim erişime açılmamalı —
    /// GUID gizliliği tek savunma değildir (güvenlik denetimi 2026-07-17, bulgu #4).
    /// Girişli kullanıcı iframe/yeni sekmede zaten oturum çerezini taşır.
    /// </summary>
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
