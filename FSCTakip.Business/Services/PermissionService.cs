using FSCTakip.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.Business.Services
{
    /// <summary>
    /// Bir kullanıcının belirli bir modüldeki efektif yetkisini hesaplar.
    ///
    /// Hesaplama sırası:
    ///   1. IsAdmin = true → her şey açık
    ///   2. Kullanıcının tüm gruplarından gelen yetkiler OR ile birleştirilir
    ///   3. Kullanıcıya özgü override uygulanır:
    ///      - true  = zorla aç (gruptan bağımsız)
    ///      - false = zorla kapat / deny (gruptan bağımsız)
    ///      - null  = grubun değerini kullan
    /// </summary>
    public class PermissionService
    {
        private readonly AppDbContext _context;

        public PermissionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<EffectivePermission> GetEffectiveAsync(int userId, string moduleCode)
        {
            var user = await _context.AppUsers
                .Include(u => u.UserGroups)
                    .ThenInclude(ug => ug.Group)
                        .ThenInclude(g => g.Permissions)
                            .ThenInclude(gp => gp.Module)
                .Include(u => u.PermissionOverrides)
                    .ThenInclude(o => o.Module)
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

            if (user == null)
                return EffectivePermission.None;

            // Admin → tümü açık
            if (user.IsAdmin)
                return EffectivePermission.All;

            // Tüm grupların OR'u
            bool groupRead   = false;
            bool groupWrite  = false;
            bool groupDelete = false;

            foreach (var ug in user.UserGroups.Where(ug => ug.Group.IsActive))
            {
                var gp = ug.Group.Permissions
                    .FirstOrDefault(p => p.Module?.Code == moduleCode);
                if (gp != null)
                {
                    groupRead   |= gp.CanRead;
                    groupWrite  |= gp.CanWrite;
                    groupDelete |= gp.CanDelete;
                }
            }

            // Kullanıcıya özgü override
            var over = user.PermissionOverrides
                .FirstOrDefault(o => o.Module?.Code == moduleCode);

            return new EffectivePermission
            {
                CanRead   = over?.CanRead   ?? groupRead,
                CanWrite  = over?.CanWrite  ?? groupWrite,
                CanDelete = over?.CanDelete ?? groupDelete
            };
        }

        /// <summary>Kullanıcının tüm modüllerdeki efektif yetkilerini döner.</summary>
        public async Task<Dictionary<string, EffectivePermission>> GetAllEffectiveAsync(int userId)
        {
            var modules = await _context.PermissionModules.ToListAsync();
            var result  = new Dictionary<string, EffectivePermission>();
            foreach (var m in modules)
                result[m.Code] = await GetEffectiveAsync(userId, m.Code);
            return result;
        }
    }

    public class EffectivePermission
    {
        public bool CanRead   { get; set; }
        public bool CanWrite  { get; set; }
        public bool CanDelete { get; set; }

        /// <summary>Hiç yetkisi yok</summary>
        public static EffectivePermission None => new();

        /// <summary>Tüm yetkiler açık (admin)</summary>
        public static EffectivePermission All  => new() { CanRead = true, CanWrite = true, CanDelete = true };
    }
}
