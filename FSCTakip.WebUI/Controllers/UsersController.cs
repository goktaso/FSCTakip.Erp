using FSCTakip.Business.Services;
using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Controllers
{
    /// <summary>Kullanıcı yönetimi — yalnızca admin erişebilir.</summary>
    public class UsersController : BaseController
    {
        private readonly PermissionService _permService;

        public UsersController(AppDbContext context, PermissionService permService) : base(context)
        {
            _permService = permService;
        }

        private IActionResult? AdminOnly()
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            if (!IsAdminUser) return View("~/Views/Shared/AccessDenied.cshtml");
            return null;
        }

        // GET /Users
        public async Task<IActionResult> Index()
        {
            var guard = AdminOnly(); if (guard != null) return guard;

            var users = await _context.AppUsers
                .Include(u => u.UserGroups).ThenInclude(ug => ug.Group)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.AllGroups = await _context.PermissionGroups
                .Where(g => g.IsActive).OrderBy(g => g.Name).ToListAsync();
            ViewData["Title"] = "Kullanıcı Yönetimi";
            return View(users);
        }

        // GET /Users/GetUser/{id}
        [HttpGet]
        public async Task<IActionResult> GetUser(int id)
        {
            if (!IsAdminUser) return Json(new { success = false });
            var u = await _context.AppUsers
                .Include(u => u.UserGroups)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (u == null) return Json(new { success = false });
            return Json(new {
                success = true,
                id = u.Id, username = u.Username, fullName = u.FullName,
                email = u.Email, isAdmin = u.IsAdmin, isActive = u.IsActive,
                groupIds = u.UserGroups.Select(ug => ug.GroupId).ToList()
            });
        }

        // POST /Users/Save
        [HttpPost]
        public async Task<IActionResult> Save(
            int id, string username, string fullName, string? email,
            bool isAdmin, bool isActive, string? newPassword, int[]? groupIds)
        {
            if (!IsAdminUser) return Json(new { success = false, message = "Yetkisiz." });
            try
            {
                if (id == 0)
                {
                    // Yeni kullanıcı
                    if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                        return Json(new { success = false, message = "Şifre en az 6 karakter olmalıdır." });

                    if (await _context.AppUsers.AnyAsync(u => u.Username == username))
                        return Json(new { success = false, message = "Bu kullanıcı adı zaten kullanımda." });

                    var user = new AppUser
                    {
                        Username     = username,
                        PasswordHash = AccountController.HashPassword(newPassword),
                        FullName     = fullName,
                        Email        = email,
                        IsAdmin      = isAdmin,
                        IsActive     = isActive,
                        CreatedDate  = DateTime.Now,
                        CreatedBy    = HttpContext.Session.GetString("Username") ?? "SYSTEM"
                    };
                    _context.AppUsers.Add(user);
                    await _context.SaveChangesAsync();

                    // Grup atamaları
                    if (groupIds != null)
                        foreach (var gid in groupIds)
                            _context.UserGroups.Add(new UserGroup { UserId = user.Id, GroupId = gid });
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Kullanıcı oluşturuldu." });
                }
                else
                {
                    var user = await _context.AppUsers
                        .Include(u => u.UserGroups)
                        .FirstOrDefaultAsync(u => u.Id == id);
                    if (user == null) return Json(new { success = false, message = "Kullanıcı bulunamadı." });

                    // Başka kullanıcıda aynı username var mı?
                    if (await _context.AppUsers.AnyAsync(u => u.Username == username && u.Id != id))
                        return Json(new { success = false, message = "Bu kullanıcı adı zaten kullanımda." });

                    user.Username = username;
                    user.FullName = fullName;
                    user.Email    = email;
                    user.IsAdmin  = isAdmin;
                    user.IsActive = isActive;
                    if (!string.IsNullOrWhiteSpace(newPassword))
                    {
                        if (newPassword.Length < 6)
                            return Json(new { success = false, message = "Şifre en az 6 karakter olmalıdır." });
                        user.PasswordHash = AccountController.HashPassword(newPassword);
                    }

                    // Grup güncellemesi
                    _context.UserGroups.RemoveRange(user.UserGroups);
                    if (groupIds != null)
                        foreach (var gid in groupIds)
                            _context.UserGroups.Add(new UserGroup { UserId = user.Id, GroupId = gid });

                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Kullanıcı güncellendi." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET /Users/Detail/{id} — kullanıcı yetki detay sayfası
        public async Task<IActionResult> Detail(int id)
        {
            var guard = AdminOnly(); if (guard != null) return guard;

            var user = await _context.AppUsers
                .Include(u => u.UserGroups).ThenInclude(ug => ug.Group)
                .Include(u => u.PermissionOverrides).ThenInclude(o => o.Module)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            var allModules   = await _context.PermissionModules.OrderBy(m => m.SortOrder).ToListAsync();
            var allGroups    = await _context.PermissionGroups.Where(g => g.IsActive).ToListAsync();
            var effectiveAll = await _permService.GetAllEffectiveAsync(id);

            ViewBag.AllModules   = allModules;
            ViewBag.AllGroups    = allGroups;
            ViewBag.EffectiveAll = effectiveAll;
            ViewData["Title"]    = $"Kullanıcı Yetkisi — {user.FullName}";
            return View(user);
        }

        // POST /Users/SaveOverride — tek modülün override'ını kaydet
        [HttpPost]
        public async Task<IActionResult> SaveOverride(int userId, int moduleId,
            bool? canRead, bool? canWrite, bool? canDelete)
        {
            if (!IsAdminUser) return Json(new { success = false, message = "Yetkisiz." });
            try
            {
                var existing = await _context.UserPermissionOverrides
                    .FirstOrDefaultAsync(o => o.UserId == userId && o.ModuleId == moduleId);

                // Üçü de null ise satırı sil (gruptan devral)
                if (canRead == null && canWrite == null && canDelete == null)
                {
                    if (existing != null)
                    {
                        _context.UserPermissionOverrides.Remove(existing);
                        await _context.SaveChangesAsync();
                    }
                    return Json(new { success = true, message = "Override kaldırıldı, gruptan devralıyor." });
                }

                if (existing == null)
                {
                    _context.UserPermissionOverrides.Add(new UserPermissionOverride
                    {
                        UserId = userId, ModuleId = moduleId,
                        CanRead = canRead, CanWrite = canWrite, CanDelete = canDelete
                    });
                }
                else
                {
                    existing.CanRead   = canRead;
                    existing.CanWrite  = canWrite;
                    existing.CanDelete = canDelete;
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Bireysel yetki kaydedildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST /Users/ToggleActive
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            if (!IsAdminUser) return Json(new { success = false });
            var u = await _context.AppUsers.FindAsync(id);
            if (u == null) return Json(new { success = false });
            u.IsActive = !u.IsActive;
            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = u.IsActive });
        }
    }
}
