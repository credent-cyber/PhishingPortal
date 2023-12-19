using System.Text.Json.Serialization;

namespace PhishingPortal.OutlookAddin.Client
{

    public class ApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("result")]
        public T Result { get; set; }
    }


    public class GenericApiRequest<T>
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public T Param { get; set; }
    }
}
