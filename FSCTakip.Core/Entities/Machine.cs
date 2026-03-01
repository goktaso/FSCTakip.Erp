namespace FSCTakip.Core.Entities
{
    public class Machine : BaseEntity
    {
        // Örn: "M-01", "HAT-02" (ERP entegrasyonu için kod)
        public string MachineCode { get; set; }

        // Örn: "1 Nolu Torba Hattı", "Burgu Sap Makinesi"
        public string MachineName { get; set; }

        // Makine tipi (Torba, Sap, Etiket makinesi vb.)
        public string MachineType { get; set; }

        public bool IsActive { get; set; } = true;
    }
}