namespace FSCTakip.Core.Entities
{
    /// <summary>
    /// Kullanıcıya özgü yetki istisnası.
    /// null  = gruptan devral (override yok)
    /// true  = gruptan bağımsız olarak zorla AÇ
    /// false = gruptan bağımsız olarak zorla KAPAT (deny)
    /// </summary>
    public class UserPermissionOverride
    {
        public int    UserId   { get; set; }
        public int    ModuleId { get; set; }
        public bool?  CanRead   { get; set; }   // null = devral
        public bool?  CanWrite  { get; set; }
        public bool?  CanDelete { get; set; }

        public virtual AppUser         User   { get; set; } = null!;
        public virtual PermissionModule Module { get; set; } = null!;
    }
}
