namespace FSCTakip.Core.Entities
{
    /// <summary>Kullanıcı ↔ Grup çoka-çok bağlantı tablosu</summary>
    public class UserGroup
    {
        public int UserId  { get; set; }
        public int GroupId { get; set; }

        public virtual AppUser        User  { get; set; } = null!;
        public virtual PermissionGroup Group { get; set; } = null!;
    }
}
