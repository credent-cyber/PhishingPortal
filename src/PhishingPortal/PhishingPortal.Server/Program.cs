using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using PhishingPortal.Server;
using Microsoft.AspNetCore.Authentication;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);
//var rsaCertificate = new X509Certificate2(
//    Path.Combine(builder.Environment.ContentRootPath, "cert_rsa512.pfx"), "1234");

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


builder.Services.AddDbContext<PhishingPortalDbContext>(options =>
        options.UseSqlite("Data Source=phishsim-db.db"));

builder.Services.AddDefaultIdentity<PhishingPortalUser>(options => {
    options.SignIn.RequireConfirmedAccount = true;
    })
    .AddEntityFrameworkStores<PhishingPortalDbContext>();

builder.Services.AddIdentityServer()
    //.AddSigningCredential(rsaCertificate)
    .AddApiAuthorization<PhishingPortalUser, PhishingPortalDbContext>();

builder.Services.AddAuthentication()
    .AddIdentityServerJwt();

builder.Services.AddSingleton<WeatherForecastService>();


var app = builder.Build();

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using (var scope = scopeFactory.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PhishingPortalDbContext>();
    if (db.Database.EnsureCreated())
    {
        //SeedData.Initialize(db);
    }

}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseIdentityServer();
app.UseAuthorization();

app.MapPhishingApi();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
