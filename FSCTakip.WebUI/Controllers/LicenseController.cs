using FSCTakip.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FSCTakip.WebUI.Controllers
{
    /// <summary>
    /// Lisans durumu ve dosya yükleme. [AllowAnonymous]: lisans geçersizken
    /// müşteri parmak izini görüp ARD'ye iletebilmeli, gelen lisansı yükleyebilmelidir.
    /// </summary>
    [AllowAnonymous]
    public class LicenseController : Controller
    {
        private readonly ILicenseService _license;

        public LicenseController(ILicenseService license) => _license = license;

        // GET /License/Status
        public IActionResult Status()
        {
            ViewData["Title"] = "Lisans Durumu";
            return View(_license.Current);
        }

        // POST /License/Upload
        // IgnoreAntiforgeryToken: lisanssız durumda Layout/token yok; dosya zaten RSA imzalıdır,
        // imzasız/yabancı dosya ValidateLicenseContent'te reddedilir.
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0 || file.Length > 64 * 1024)
                return Json(new { success = false, message = "Geçerli bir lisans dosyası seçiniz (.lic)." });

            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();

            // Kaydetmeden önce doğrula — bozuk dosya mevcut geçerli lisansın üzerine yazılmasın.
            var machineKey = LicenseService.GetMachineKey();
            var expectedProduct = (_license as LicenseService)?.ExpectedProduct;
            var check = LicenseService.ValidateLicenseContent(content, machineKey, DateTime.Today, expectedProduct);
            if (check.State != LicenseState.Valid)
                return Json(new { success = false, message = $"Lisans doğrulanamadı: {check.State} {check.Error}".Trim() });

            var path = ((LicenseService)_license).LicensePath;
            await System.IO.File.WriteAllTextAsync(path, content.Trim());
            _license.Invalidate();

            return Json(new { success = true, message = $"Lisans etkinleştirildi — {check.LicensedTo}" });
        }
    }
}
