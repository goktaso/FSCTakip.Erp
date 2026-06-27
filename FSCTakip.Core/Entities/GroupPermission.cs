namespace FSCTakip.Core.Entities
{
    /// <summary>Grup × Modül yetki satırı</summary>
    public class GroupPermission
    {
        public int  GroupId  { get; set; }
        public int  ModuleId { get; set; }
        public bool CanRead   { get; set; } = false;
        public bool CanWrite  { get; set; } = false;
        public bool CanDelete { get; set; } = false;

        public virtual PermissionGroup  Group  { get; set; } = null!;
        public virtual PermissionModule Module { get; set; } = null!;
    }
}
