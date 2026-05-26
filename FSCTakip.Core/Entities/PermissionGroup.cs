using System;
using System.Collections.Generic;

namespace FSCTakip.Core.Entities
{
    /// <summary>Yetki grubu (Operatör, Muhasebe, Yönetici vb.)</summary>
    public class PermissionGroup
    {
        public int     Id          { get; set; }
        public string  Name        { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool    IsActive    { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string  CreatedBy   { get; set; } = "SYSTEM";

        public virtual ICollection<UserGroup>       UserGroups  { get; set; } = new List<UserGroup>();
        public virtual ICollection<GroupPermission> Permissions { get; set; } = new List<GroupPermission>();
    }
}
