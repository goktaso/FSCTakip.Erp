using Microsoft.AspNetCore.Mvc;

namespace FSCTakip.WebUI.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string username, string password, string? returnUrl)
        {
            // Geçici sabit kullanıcı — ilerleyen fazda ASP.NET Identity ile değiştirilecek
            if (username == "admin" && password == "admin123")
            {
                HttpContext.Session.SetString("UserName", "Özay Göktaş");
                HttpContext.Session.SetString("UserRole", "Admin");
                return Redirect(returnUrl ?? "/Home/Index");
            }
            ViewBag.Error = "Kullanıcı adı veya şifre hatalı.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
