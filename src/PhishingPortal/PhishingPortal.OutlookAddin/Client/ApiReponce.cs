namespace PhishingPortal.OutlookAddin.Client
{

    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T Result { get; set; }
    }

    public class GenericApiRequest<T>
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public T Param { get; set; }
    }
}
