using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FSCTakip.WebUI.Filters
{
    /// <summary>
    /// Her istekte session'da UserId olup olmadığını kontrol eder.
    /// Yoksa /Account/Login'e yönlendirir.
    /// AccountController (login/logout) bu filtreden muaftır.
    /// </summary>
    public class SessionAuthFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.RouteData.Values["controller"]?.ToString();

            // Login/Logout sayfaları filtreden muaf
            if (string.Equals(controller, "Account", StringComparison.OrdinalIgnoreCase))
                return;

            var userId = context.HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectToActionResult("Login", "Account",
                    new { returnUrl });
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
