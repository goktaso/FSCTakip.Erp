using FSCTakip.DataAccess.Data;
using FSCTakip.WebUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FSCTakip.WebUI.Controllers
{
    /// <summary>Yalnız admin — internet erişimi olan müşterilerde yeni sürüm kontrolü/indirmesi.</summary>
    public class UpdateController : BaseController
    {
        private readonly IUpdateCheckService _updateCheck;

        public UpdateController(AppDbContext context, IUpdateCheckService updateCheck) : base(context)
        {
            _updateCheck = updateCheck;
        }

        // GET /Update/Index
        public IActionResult Index()
        {
            if (!IsAdminUser) return RedirectToAction("Index", "Home");
            ViewData["Title"] = "Güncellemeler";
            return View();
        }

        // POST /Update/Check
        [HttpPost]
        public async Task<IActionResult> Check()
        {
            if (!IsAdminUser) return Json(new { success = false, message = "Yetkisiz." });
            var result = await _updateCheck.CheckAsync();
            return Json(new
            {
                success = true,
                enabled = result.Enabled,
                currentVersion = result.CurrentVersion,
                latestVersion = result.LatestVersion,
                hasUpdate = result.HasUpdate,
                notes = result.Notes,
                error = result.Error
            });
        }

        // POST /Update/Download
        [HttpPost]
        public async Task<IActionResult> Download()
        {
            if (!IsAdminUser) return Json(new { success = false, message = "Yetkisiz." });
            var (success, message, path) = await _updateCheck.DownloadLatestAsync();
            return Json(new { success, message, path });
        }
    }
}
