using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PhishingPortal.OutlookAddInVSTO
{
    public class ApiResponce
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
}
