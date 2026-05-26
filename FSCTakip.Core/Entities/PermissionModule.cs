using System.Collections.Generic;

namespace FSCTakip.Core.Entities
{
    /// <summary>
    /// Sistemdeki her yetkilendirilebilir modül.
    /// Seed data ile önceden doldurulur, kullanıcı ekleyemez.
    /// </summary>
    public class PermissionModule
    {
        public int    Id          { get; set; }
        public string Code        { get; set; } = string.Empty;   // "PURCHASE", "PRODUCTION"...
        public string DisplayName { get; set; } = string.Empty;   // "Hammadde Girişi"
        public string? Description { get; set; }
        public string IconClass   { get; set; } = "fas fa-circle"; // FontAwesome
        public int    SortOrder   { get; set; }

        public virtual ICollection<GroupPermission>        GroupPermissions    { get; set; } = new List<GroupPermission>();
        public virtual ICollection<UserPermissionOverride> UserOverrides       { get; set; } = new List<UserPermissionOverride>();
    }
}
