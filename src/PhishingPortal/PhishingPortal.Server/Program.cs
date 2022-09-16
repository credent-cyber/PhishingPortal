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
using IdentityUIServices = Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql;
using Serilog;
using System;
using PhishingPortal.Server.Services.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.OData;
using PhishingPortal.Server.Models;

var builder = WebApplication.CreateBuilder(args);

//var rsaCertificate = new X509Certificate2(
//Path.Combine(builder.Environment.ContentRootPath, "rsaCert.pfx"), "1234");

// Add services to the container.
builder.Configuration.AddEnvironmentVariables(prefix: "ASPNETCORE_");
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true);


var config = (IConfiguration)builder.Configuration;

builder.Services.AddLogging((builder) =>
{
    Serilog.Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config)
    .CreateLogger();
    builder.AddSerilog();
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();


var conString = builder.Configuration.GetValue<string>("SqlLiteConnectionString");
var useSqlLite = builder.Configuration.GetValue<bool>("UseSqlLite");
var sqlProvider = builder.Configuration.GetValue<string>("SqlProvider");

if (useSqlLite)
{
    builder.Services.AddDbContext<PhishingPortalDbContext>(options =>
        options.UseSqlite(conString));
}
else
{
    #region Use configured sql provider
    if (string.IsNullOrEmpty(sqlProvider))
    {
        sqlProvider = "mysql";
    }

    conString = builder.Configuration.GetConnectionString("DefaultConnection");
    switch (sqlProvider)
    {
        case "mysql":

            builder.Services.AddDbContext<PhishingPortalDbContext>(options =>
            {
                options.UseMySql(conString, ServerVersion.AutoDetect(conString));
            });


            break;

        case "mssql":

            builder.Services.AddDbContext<PhishingPortalDbContext>(options =>
            {
                options.UseSqlServer(conString);
            });

            break;

        default: throw new Exception($"Invalid SqlProvider configuration [{sqlProvider}]");
    }

    #endregion
}

builder.Services.AddDefaultIdentity<PhishingPortalUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<PhishingPortalDbContext>();

builder.Services.AddSingleton<IEmailClient, SmtpEmailClient>();
builder.Services.AddTransient<IdentityUIServices.IEmailSender, EmailSender>();

var dataProtectPath = "./App_Data/";

//builder.Services.AddDataProtection()
//    .PersistKeysToFileSystem(new DirectoryInfo($"{dataProtectPath}"));

builder.Services.AddIdentityServer(options =>
{
 
    options.KeyManagement.KeyPath = $"{dataProtectPath}";
    options.KeyManagement.RotationInterval = TimeSpan.FromDays(30);
    options.KeyManagement.PropagationTime = TimeSpan.FromDays(2);
    options.KeyManagement.RetentionDuration = TimeSpan.FromDays(7);
})
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
builder.Services.AddScoped<ITenantDbResolver, TenantDbResolver>();

// GridBlazor
//builder.Services.AddControllers().AddOData(opt => opt.AddRouteComponents("odata", EdmModel.GetEdmModel())
//               .Select().Expand().Filter().OrderBy().SetMaxTop(100).Count());
//builder.Services.AddOData();

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// check if picked the correct configurations
logger.LogInformation($"UseSqlLite: {useSqlLite}");
logger.LogInformation($"SqlProvider: {sqlProvider}");

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using (var scope = scopeFactory.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PhishingPortalDbContext>();
    if (db.Database.EnsureCreated())
    {
        // seed data if a single tenant application
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
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseIdentityServer();
app.UseAuthorization();

app.MapPhishingApi();
app.UseEndpoints(endpoints =>
{
    //endpoints.Select().Expand().Filter().OrderBy().MaxTop(100).Count();
    //endpoints.MapODataRoute("odata", "odata", EdmModel.GetEdmModel());

    endpoints.MapControllers();
    endpoints.MapFallbackToFile("index.html");
});
app.MapRazorPages();
//app.MapControllers();
//app.MapFallbackToFile("index.html");

app.Run();
