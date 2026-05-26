using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Controllers
{
    /// <summary>Yetki grubu yönetimi — yalnızca admin.</summary>
    public class GroupsController : BaseController
    {
        public GroupsController(AppDbContext context) : base(context) { }

        private IActionResult? AdminOnly()
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            if (!IsAdminUser) return View("~/Views/Shared/AccessDenied.cshtml");
            return null;
        }

        // GET /Groups
        public async Task<IActionResult> Index()
        {
            var guard = AdminOnly(); if (guard != null) return guard;

            var groups = await _context.PermissionGroups
                .Include(g => g.UserGroups)
                .Include(g => g.Permissions)
                .OrderBy(g => g.Name)
                .ToListAsync();

            ViewData["Title"] = "Yetki Grupları";
            return View(groups);
        }

        // GET /Groups/Detail/{id} — grup × modül yetki matrisi
        public async Task<IActionResult> Detail(int id)
        {
            var guard = AdminOnly(); if (guard != null) return guard;

            var group = await _context.PermissionGroups
                .Include(g => g.Permissions).ThenInclude(gp => gp.Module)
                .Include(g => g.UserGroups).ThenInclude(ug => ug.User)
                .FirstOrDefaultAsync(g => g.Id == id);
            if (group == null) return NotFound();

            var allModules = await _context.PermissionModules.OrderBy(m => m.SortOrder).ToListAsync();
            ViewBag.AllModules = allModules;
            ViewData["Title"]  = $"Grup Yetkileri — {group.Name}";
            return View(group);
        }

        // GET /Groups/GetGroup/{id}
        [HttpGet]
        public async Task<IActionResult> GetGroup(int id)
        {
            if (!IsAdminUser) return Json(new { success = false });
            var g = await _context.PermissionGroups.FindAsync(id);
            if (g == null) return Json(new { success = false });
            return Json(new { success = true, id = g.Id, name = g.Name, description = g.Description, isActive = g.IsActive });
        }

        // POST /Groups/Save
        [HttpPost]
        public async Task<IActionResult> Save(int id, string name, string? description, bool isActive)
        {
            if (!IsAdminUser) return Json(new { success = false, message = "Yetkisiz." });
            try
            {
                if (id == 0)
                {
                    _context.PermissionGroups.Add(new PermissionGroup
                    {
                        Name = name, Description = description, IsActive = isActive,
                        CreatedDate = DateTime.Now,
                        CreatedBy   = HttpContext.Session.GetString("Username") ?? "SYSTEM"
                    });
                }
                else
                {
                    var g = await _context.PermissionGroups.FindAsync(id);
                    if (g == null) return Json(new { success = false, message = "Grup bulunamadı." });
                    g.Name = name; g.Description = description; g.IsActive = isActive;
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Grup kaydedildi." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // POST /Groups/SavePermissions — tüm modül yetkilerini tek seferde kaydet
        [HttpPost]
        public async Task<IActionResult> SavePermissions(int groupId,
            [FromBody] List<ModulePermissionDto> permissions)
        {
            if (!IsAdminUser) return Json(new { success = false, message = "Yetkisiz." });
            try
            {
                var existing = await _context.GroupPermissions
                    .Where(gp => gp.GroupId == groupId).ToListAsync();
                _context.GroupPermissions.RemoveRange(existing);

                foreach (var p in permissions)
                {
                    if (p.CanRead || p.CanWrite || p.CanDelete)
                    {
                        _context.GroupPermissions.Add(new GroupPermission
                        {
                            GroupId   = groupId,
                            ModuleId  = p.ModuleId,
                            CanRead   = p.CanRead,
                            CanWrite  = p.CanWrite,
                            CanDelete = p.CanDelete
                        });
                    }
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Grup yetkileri kaydedildi." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // POST /Groups/ToggleActive
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            if (!IsAdminUser) return Json(new { success = false });
            var g = await _context.PermissionGroups.FindAsync(id);
            if (g == null) return Json(new { success = false });
            g.IsActive = !g.IsActive;
            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = g.IsActive });
        }
    }

    public class ModulePermissionDto
    {
        public int  ModuleId  { get; set; }
        public bool CanRead   { get; set; }
        public bool CanWrite  { get; set; }
        public bool CanDelete { get; set; }
    }
}
