namespace FSCTakip.Core.Entities
{
    /// <summary>
    /// Kilitli bir denetim dönemine ait kayıt değiştirilmeye çalışıldığında fırlatılır.
    /// Controller'lardaki try-catch bu exception'ı yakalayarak kullanıcıya
    /// anlamlı bir hata mesajı döner.
    /// </summary>
    public class PeriodLockedException : InvalidOperationException
    {
        public PeriodLockedException(string message) : base(message) { }
    }
}
