using PhishingPortal.Dto.Auth;
using System.Net;
using System.Net.Http.Json;

namespace PhishingPortal.UI.Blazor.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(bool, string)> ChangePassword(ChangePassword param)
        {
            var result = await _httpClient.PostAsJsonAsync("api/acl/changepassword", param);

            if (!result.IsSuccessStatusCode)
            {
                var errorMessage = await result.Content.ReadAsStringAsync();
                return (false, errorMessage);
            }

            return (true, "Success!");
        }
        public async Task<CurrentUser> CurrentUserInfo()
        {
            var result = await _httpClient.GetFromJsonAsync<CurrentUser>("api/acl/currentuserinfo");
            return result ?? new CurrentUser { UserName = "guest" };
        }

        public async Task<(bool, string)> ForgetPassword(ForgetPasswordRequest param)
        {
            var result = await _httpClient.PostAsJsonAsync("api/acl/forgetpassword", param);

            if (result.IsSuccessStatusCode)
            {
                return (true, "Success"); 
            }
            else if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorMessage = await result.Content.ReadAsStringAsync();
                Console.WriteLine(errorMessage); // Output the error message
                return (false, errorMessage); // Return failure and the error message
            }
            else
            {
                // Handle other status codes
                return (false, "Unexpected error occurred."); // Return failure with a generic error message
            }
        }



        public async Task Login(LoginRequest loginRequest)
        {
            var result = await _httpClient.PostAsJsonAsync("api/acl/login", loginRequest);
            if (result.StatusCode == System.Net.HttpStatusCode.BadRequest) throw new Exception(await result.Content.ReadAsStringAsync());
        }
        public async Task Logout()
        {
            var result = await _httpClient.PostAsync("api/acl/logout", null);
            
            result.EnsureSuccessStatusCode();

        }

        public async Task<bool> ResetPassword(ResetPasswordRequest param)
        {
            var result = await _httpClient.PostAsJsonAsync("api/acl/resetpassword", param);

            result.EnsureSuccessStatusCode();

            return result.IsSuccessStatusCode;
        }
    }
}
