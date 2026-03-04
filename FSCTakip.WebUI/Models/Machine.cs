public class Machine
{
    public int Id { get; set; }
    public string Name { get; set; } // Örn: T-1, Garanti, NewLong
    public string Brand { get; set; } // Marka
    public string Capacity { get; set; } // Kapasite bilgisi
    public bool IsActive { get; set; } = true;
}