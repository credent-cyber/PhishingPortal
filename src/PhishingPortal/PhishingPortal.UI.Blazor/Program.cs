using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PhishingPortal.UI.Blazor;
using PhishingPortal.UI.Blazor.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});


builder.Services.AddScoped<PhishingPortalClientState>();

builder.Services.AddHttpClient<WeatherClient>(client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

builder.Services.AddHttpClient<TenantAdminClient>(client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

builder.Services.AddHttpClient<TenantClient>(client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

builder.Services.AddApiAuthorization<PhishingPortalAuthState>(options =>
{
    options.AuthenticationPaths.LogOutSucceededPath = "";
}).AddAccountClaimsPrincipalFactory<PhishingPortalAuthState, UserFactory>();


await builder.Build().RunAsync();
