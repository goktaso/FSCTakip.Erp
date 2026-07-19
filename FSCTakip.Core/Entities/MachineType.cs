namespace FSCTakip.Core.Entities
{
    public class MachineType : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
