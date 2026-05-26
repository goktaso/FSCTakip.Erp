using System;
using System.Collections.Generic;

namespace FSCTakip.Core.Entities
{
    /// <summary>Sistem kullanıcısı. IsAdmin=true ise tüm yetki kontrolleri bypass edilir.</summary>
    public class AppUser
    {
        public int     Id           { get; set; }
        public string  Username     { get; set; } = string.Empty;
        public string  PasswordHash { get; set; } = string.Empty;  // BCrypt hash
        public string  FullName     { get; set; } = string.Empty;
        public string? Email        { get; set; }
        public bool    IsAdmin      { get; set; } = false;
        public bool    IsActive     { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string  CreatedBy    { get; set; } = "SYSTEM";
        public DateTime? LastLoginDate { get; set; }

        public virtual ICollection<UserGroup>              UserGroups          { get; set; } = new List<UserGroup>();
        public virtual ICollection<UserPermissionOverride> PermissionOverrides { get; set; } = new List<UserPermissionOverride>();
    }
}
