using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using PhishingPortal.Server;
using Microsoft.AspNetCore.Authentication;
using System.Security.Cryptography.X509Certificates;
using PhishingPortal.Repositories;
using PhishingPortal.Core;
using PhishingPortal.Server.Services;
using PhishingPortal.DataContext;
using PhishingPortal.Domain;
using PhishingPortal.Common;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);
//var rsaCertificate = new X509Certificate2(
//    Path.Combine(builder.Environment.ContentRootPath, "cert_rsa512.pfx"), "1234");

// Add services to the container.

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true);

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();


builder.Services.AddDbContext<PhishingPortalDbContext>(options =>
        options.UseSqlite("Data Source=./App_Data/phishsim-db.db"));

//builder.Services.AddDbContext<TenantDbContext>(options =>
//        options.UseSqlite("Data Source=T334343.db"));

builder.Services.AddDefaultIdentity<PhishingPortalUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<PhishingPortalDbContext>();


SmtpMessageSenderOptions options = new();
builder.Configuration.GetSection(nameof(SmtpMessageSenderOptions))
                 .Bind(options);

builder.Services.Configure<SmtpMessageSenderOptions>(o =>
{
    o.Server = options.Server;
    o.Port = options.Port;
    o.Password = options.Password;
    o.FromEmail = options.FromEmail;
});

builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddIdentityServer()
    //.AddSigningCredential(rsaCertificate)
    .AddApiAuthorization<PhishingPortalUser, PhishingPortalDbContext>(options =>
    {
        options.IdentityResources["openid"].UserClaims.Add("name");
        options.ApiResources.Single().UserClaims.Add("name");
        options.IdentityResources["openid"].UserClaims.Add("role");
        options.ApiResources.Single().UserClaims.Add("role");
    });

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("role");

builder.Services.AddAuthentication()
    .AddIdentityServerJwt();

// services and custom dependencies
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddScoped<ITenantAdmin, TenantAdmin>();
builder.Services.AddSingleton<TenantAdminRepoConfig>();
builder.Services.AddScoped<ITenantAdminRepository, TenantAdminRepository>();
builder.Services.AddSingleton<INsLookupHelper, NsLookupHelper>();


var app = builder.Build();

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using (var scope = scopeFactory.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PhishingPortalDbContext>();
    if (db.Database.EnsureCreated())
    {
        // seed data
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
