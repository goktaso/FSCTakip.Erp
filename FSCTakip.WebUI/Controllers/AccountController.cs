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
            // Kullanıcıyı önce çek, parolayı KODDA doğrula: PBKDF2 tuzu kullanıcıya özel
            // olduğu için SQL'de "PasswordHash == hash" karşılaştırması yapılamaz.
            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
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
            HttpContext.Session.SetString("MustChangePassword", user.MustChangePassword ? "1" : "0");
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

            // Eski SHA256 hash ile giriş yapıldıysa PBKDF2'ye yükselt (kademeli geçiş)
            if (IsLegacyHash(user.PasswordHash))
                user.PasswordHash = HashPassword(password);

            // Son giriş tarihi güncelle
            user.LastLoginDate = DateTime.Now;
            await _context.SaveChangesAsync();

            // Varsayılan/geçici parola ile girildiyse önce değiştirtmeye yönlendir
            if (user.MustChangePassword)
                return RedirectToAction("ChangePassword");

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

            if (newPassword.Length < 8)
            { ViewBag.Error = "Şifre en az 8 karakter olmalıdır."; return View(); }

            var user = await _context.AppUsers.FindAsync(int.Parse(userId));
            if (user == null || !VerifyPassword(currentPassword, user.PasswordHash))
            { ViewBag.Error = "Mevcut şifre hatalı."; return View(); }

            // Aynı parolayı tekrar koymak zorunlu değişimi anlamsız kılar — engelle
            if (VerifyPassword(newPassword, user.PasswordHash))
            { ViewBag.Error = "Yeni şifre mevcut şifreden farklı olmalıdır."; return View(); }

            user.PasswordHash = HashPassword(newPassword);
            user.MustChangePassword = false;         // zorunlu değişim tamamlandı
            await _context.SaveChangesAsync();
            HttpContext.Session.SetString("MustChangePassword", "0");
            TempData["Success"] = "Şifre başarıyla değiştirildi.";
            return RedirectToAction("Login");
        }

        // ── Parola hash'leme ─────────────────────────────────────────────────
        // PBKDF2-HMAC-SHA256, kullanıcıya özel rastgele tuz. Eski kayıtlar SHA256+sabit
        // tuz formatındaydı (zayıf); VerifyPassword ikisini de tanır, giriş anında
        // yeni formata yükseltilir (Login içinde IsLegacyHash kontrolü).
        private const int Pbkdf2Iterations = 120_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        /// <summary>Yeni parola hash'i üretir: pbkdf2$iter$base64(salt)$base64(hash).</summary>
        public static string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256, HashSize);
            return $"pbkdf2${Pbkdf2Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        /// <summary>Parolayı saklanan hash'e karşı doğrular — hem PBKDF2 hem eski SHA256 formatı.</summary>
        public static bool VerifyPassword(string password, string stored)
        {
            if (string.IsNullOrEmpty(stored)) return false;

            if (stored.StartsWith("pbkdf2$", StringComparison.Ordinal))
            {
                var parts = stored.Split('$');
                if (parts.Length != 4 || !int.TryParse(parts[1], out var iter)) return false;
                byte[] salt, hash;
                try { salt = Convert.FromBase64String(parts[2]); hash = Convert.FromBase64String(parts[3]); }
                catch { return false; }
                var computed = Rfc2898DeriveBytes.Pbkdf2(password, salt, iter, HashAlgorithmName.SHA256, hash.Length);
                return CryptographicOperations.FixedTimeEquals(computed, hash);
            }

            // Eski format: SHA256 + sabit tuz. Sabit-zaman karşılaştırma.
            var legacy = Encoding.UTF8.GetBytes(LegacySha256(password));
            var storedBytes = Encoding.UTF8.GetBytes(stored.ToLowerInvariant());
            return legacy.Length == storedBytes.Length &&
                   CryptographicOperations.FixedTimeEquals(legacy, storedBytes);
        }

        /// <summary>Saklanan hash eski (SHA256) formatta mı? Giriş sonrası yükseltme için.</summary>
        public static bool IsLegacyHash(string stored) =>
            !string.IsNullOrEmpty(stored) && !stored.StartsWith("pbkdf2$", StringComparison.Ordinal);

        private static string LegacySha256(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password + "FSCTakip_Salt_2026"));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}
