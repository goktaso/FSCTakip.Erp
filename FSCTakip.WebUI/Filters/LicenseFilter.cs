using FSCTakip.WebUI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FSCTakip.WebUI.Filters
{
    /// <summary>
    /// Lisans geçersizse (eksik/bozuk/süresi dolmuş/yanlış makine) tüm istekleri
    /// /License/Status sayfasına yönlendirir. LicenseController muaftır — müşteri
    /// makine parmak izini görebilmeli ve yeni lisans dosyası yükleyebilmelidir.
    /// SessionAuthFilter'dan ÖNCE koşar (Program.cs kayıt sırası).
    /// </summary>
    public class LicenseFilter : IActionFilter
    {
        private readonly ILicenseService _license;

        public LicenseFilter(ILicenseService license) => _license = license;

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.RouteData.Values["controller"]?.ToString();
            if (string.Equals(controller, "License", StringComparison.OrdinalIgnoreCase))
                return;

            if (_license.Current.State != LicenseState.Valid)
                context.Result = new RedirectToActionResult("Status", "License", null);
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
