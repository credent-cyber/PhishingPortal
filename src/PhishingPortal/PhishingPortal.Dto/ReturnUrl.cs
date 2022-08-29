using Newtonsoft.Json;

namespace PhishingPortal.Dto
{
    public class Settings
    {
        [JsonProperty(PropertyName = "return_urls")]
        public string[] ReturnUrls { get; set; }
    }
}
