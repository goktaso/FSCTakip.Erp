using FSCTakip.Business.Services;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace FSCTakip.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext      _context;
        private readonly PermissionService _permService;

        public AccountController(AppDbContext context, PermissionService permService)
        {
            _context     = context;
            _permService = permService;
        }

        // GET /Account/Login
        public IActionResult Login(string? returnUrl)
        {
            if (HttpContext.Session.GetString("UserId") != null)
                return Redirect(returnUrl ?? "/");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl)
        {
            var hash = HashPassword(password);
            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == hash && u.IsActive);

            if (user == null)
            {
                ViewBag.Error     = "Kullanıcı adı veya şifre hatalı.";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            // Session'a kullanıcı bilgilerini yaz
            HttpContext.Session.SetString("UserId",   user.Id.ToString());
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("IsAdmin",  user.IsAdmin ? "1" : "0");
            HttpContext.Session.Remove("CriticalPopupShown");

            // Tüm yetkiler Session'a önbelleğe al (her request'te DB sorgusu yapılmasın)
            var perms = await _permService.GetAllEffectiveAsync(user.Id);
            var permJson = System.Text.Json.JsonSerializer.Serialize(
                perms.ToDictionary(k => k.Key, v => new {
                    r = v.Value.CanRead,
                    w = v.Value.CanWrite,
                    d = v.Value.CanDelete
                }));
            HttpContext.Session.SetString("Perms", permJson);

            // Son giriş tarihi güncelle
            user.LastLoginDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return Redirect(returnUrl ?? "/");
        }

        // GET /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // GET /Account/ChangePassword
        public IActionResult ChangePassword() => View();

        // POST /Account/ChangePassword
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login");

            if (newPassword != confirmPassword)
            { ViewBag.Error = "Yeni şifreler eşleşmiyor."; return View(); }

            if (newPassword.Length < 6)
            { ViewBag.Error = "Şifre en az 6 karakter olmalıdır."; return View(); }

            var user = await _context.AppUsers.FindAsync(int.Parse(userId));
            if (user == null || user.PasswordHash != HashPassword(currentPassword))
            { ViewBag.Error = "Mevcut şifre hatalı."; return View(); }

            user.PasswordHash = HashPassword(newPassword);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Şifre başarıyla değiştirildi.";
            return RedirectToAction("Login");
        }

        /// <summary>SHA-256 + sabit tuz ile şifre hash'i üretir.</summary>
        public static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password + "FSCTakip_Salt_2026"));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}
