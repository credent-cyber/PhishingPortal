namespace PhishingPortal.Dto
{
    public class AppLog : Auditable
    {
        public string Type { get; set; } = "CriticalError";
        public string Message { get; set; }
        public string ErrorDetail { get; set; }
        public bool IsEmailed { get; set; }
    }
}
