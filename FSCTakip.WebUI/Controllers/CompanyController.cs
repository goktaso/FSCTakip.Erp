using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using FSCTakip.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Controllers
{
    /// <summary>Şirket bilgileri (beyaz etiket) — belgelerde görünen ünvan/FSC kodları/logo. Yalnız admin.</summary>
    public class CompanyController : BaseController
    {
        private readonly IFileStorageService _storage;
        private readonly ICompanyBrandingService _branding;

        public CompanyController(AppDbContext context, IFileStorageService storage, ICompanyBrandingService branding)
            : base(context)
        {
            _storage  = storage;
            _branding = branding;
        }

        // GET /Company/Settings
        public async Task<IActionResult> Settings()
        {
            if (!IsAdminUser) return RedirectToAction("Index", "Home");

            var setting = await _context.CompanySettings.FirstOrDefaultAsync()
                          ?? new CompanySetting { CompanyName = "" };
            ViewData["Title"] = "Şirket Bilgileri";
            return View(setting);
        }

        // POST /Company/Save
        [HttpPost]
        public async Task<IActionResult> Save(CompanySetting model, IFormFile? logo)
        {
            if (!IsAdminUser)
                return Json(new { success = false, message = "Bu işlem yalnızca admin tarafından yapılabilir." });
            if (string.IsNullOrWhiteSpace(model.CompanyName))
                return Json(new { success = false, message = "Şirket ünvanı zorunludur." });

            // Logo yüklendiyse önce kaydet (FileStorage magic-byte + MIME + boyut doğrular).
            // Yalnız görsel kabul et — PDF logo olmaz.
            string? newLogoKey = null;
            if (logo != null && logo.Length > 0)
            {
                var mime = logo.ContentType?.ToLowerInvariant() ?? "";
                if (mime != "image/png" && mime != "image/jpeg")
                    return Json(new { success = false, message = "Logo yalnızca PNG veya JPEG olabilir." });
                try
                {
                    newLogoKey = await _storage.SaveAsync(logo, "Branding");
                }
                catch (InvalidOperationException ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }

            var setting = await _context.CompanySettings.FirstOrDefaultAsync();
            if (setting == null)
            {
                model.LogoPath    = newLogoKey;
                model.CreatedBy   = User.Identity?.Name ?? "Admin";
                model.CreatedDate = DateTime.Now;
                _context.CompanySettings.Add(model);
            }
            else
            {
                setting.CompanyName    = model.CompanyName;
                setting.Address        = model.Address;
                setting.City           = model.City;
                setting.TaxNumber      = model.TaxNumber;
                setting.TaxOffice      = model.TaxOffice;
                setting.Phone          = model.Phone;
                setting.Email          = model.Email;
                setting.FscCocCode     = model.FscCocCode;
                setting.FscLicenseCode = model.FscLicenseCode;
                if (newLogoKey != null) setting.LogoPath = newLogoKey; // yeni logo yoksa mevcudu koru
                setting.UpdatedBy      = User.Identity?.Name ?? "Admin";
                setting.UpdatedDate    = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            _branding.Invalidate(); // login/sidebar/belge cache'i tazelensin
            return Json(new { success = true, message = "Şirket bilgileri kaydedildi. Belgelere anında yansır." });
        }

        // GET /Company/Logo — firma logosunu servis eder. Login (girişten önce) de gösterebilmeli.
        [AllowAnonymous]
        public IActionResult Logo()
        {
            var key = _branding.LogoKey;
            if (string.IsNullOrWhiteSpace(key) || !_storage.TryResolve(key, out var path, out var contentType))
                return NotFound();
            return PhysicalFile(path, contentType);
        }
    }
}
