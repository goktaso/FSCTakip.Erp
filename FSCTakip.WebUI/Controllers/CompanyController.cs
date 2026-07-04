using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Controllers
{
    /// <summary>Şirket bilgileri (beyaz etiket) — belgelerde görünen ünvan/FSC kodları. Yalnız admin.</summary>
    public class CompanyController : BaseController
    {
        public CompanyController(AppDbContext context) : base(context) { }

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
        public async Task<IActionResult> Save(CompanySetting model)
        {
            if (!IsAdminUser)
                return Json(new { success = false, message = "Bu işlem yalnızca admin tarafından yapılabilir." });
            if (string.IsNullOrWhiteSpace(model.CompanyName))
                return Json(new { success = false, message = "Şirket ünvanı zorunludur." });

            var setting = await _context.CompanySettings.FirstOrDefaultAsync();
            if (setting == null)
            {
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
                setting.UpdatedBy      = User.Identity?.Name ?? "Admin";
                setting.UpdatedDate    = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Şirket bilgileri kaydedildi. Belgelere anında yansır." });
        }
    }
}
