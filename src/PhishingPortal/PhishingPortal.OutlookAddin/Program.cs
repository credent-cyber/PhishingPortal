using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PhishingPortal.OutlookAddin;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");



builder.Services.AddSingleton(sp =>
    new HttpClient
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    });


//builder.Configuration.AddJsonFile("appsettings.json");
builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();

// Add the configuration
await builder.Build().RunAsync();








