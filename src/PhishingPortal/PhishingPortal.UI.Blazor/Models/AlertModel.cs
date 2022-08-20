namespace PhishingPortal.UI.Blazor.Models
{
    public class AlertModel
    {
        public bool IsSuccess { get; set; }
        public bool IsError { get; set; }
        public string Message { get; set; } = String.Empty;

        public void Clear()
        {
            IsError = false;
            IsSuccess = false;
            Message = "";
        }

        public void SetError(string msg)
        {
            IsSuccess = false;
            IsError=true;
            Message = msg;
        }

        public void SetSuccess(string msg)
        {
            IsError = false;
            IsSuccess = true;
            Message = msg;
        }
    }
}
