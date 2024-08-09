using Microsoft.EntityFrameworkCore;
using PhishingPortal.Repositories;
using PhishingPortal.Core;
using PhishingPortal.Server.Services;
using PhishingPortal.DataContext;
using PhishingPortal.Domain;
using PhishingPortal.Common;
using IdentityUIServices = Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Serilog;
using PhishingPortal.Server.Services.Interfaces;
using PhishingPortal.Server.Intrastructure;
using Microsoft.Extensions.FileProviders;
using PhishingPortal.Server;
using Microsoft.AspNetCore.Authentication.Negotiate;
using System.Security.Claims;
using PhishingPortal.Server.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables(prefix: "ASPNETCORE_");

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true);


var config = (IConfiguration)builder.Configuration;

var aadClientId = config.GetValue<string>("AzureAD:ClientId");
var aadClientSecret = config.GetValue<string>("AzureAD:ClientSecret");
var aadTenant = config.GetValue<string>("AzureAD:Tenant");
var aadRedirectUri = config.GetValue<string>("AzureAD:RedirectUri");
var authority = string.Format(System.Globalization.CultureInfo.InvariantCulture, config.GetValue<string>("AzureAD:Authority"));

var googleClientId = config.GetValue<string>("Google:ClientId");
var googleClientSecret = config.GetValue<string>("Google:ClientSecret");

var useWindowsAuthentication = config.GetValue<bool>("UseWindowsAuthentication");
var conString = builder.Configuration.GetValue<string>("SqlLiteConnectionString");
var useSqlLite = builder.Configuration.GetValue<bool>("UseSqlLite");
var sqlProvider = builder.Configuration.GetValue<string>("SqlProvider");

if (!useSqlLite)
{
    conString = builder.Configuration.GetConnectionString("DefaultConnection");
}

builder.Services.AddLogging((builder) =>
{
    Serilog.Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config)
    .WriteTo.DbSink(conString, useSqlLite)
    .CreateLogger();


    builder.AddSerilog();
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers()
    .AddODataControllers()
    .AddNewtonsoftJson();

//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PhishsimsODataDemo", Version = "v1" });
//});


if (useSqlLite)
{
    builder.Services.AddDbContext<PhishingPortalDbContext2>(options =>
        options.UseSqlite(conString));
}
else
{
    #region Use configured sql provider
    if (string.IsNullOrEmpty(sqlProvider))
    {
        sqlProvider = "mysql";
    }

    switch (sqlProvider)
    {
        case "mysql":

            //builder.Services.AddDbContext<PhishingPortalDbContext2>(options =>
            //{
            //    options.UseMySql(conString, ServerVersion.AutoDetect(conString));
            //});
            builder.Services.AddDbContext<PhishingPortalDbContext2>(options =>
            {
                options.UseMySql(conString, ServerVersion.AutoDetect(conString),
                    mySqlOptions =>
                    {
                        mySqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });
            });


            break;

        case "mssql":

            builder.Services.AddDbContext<PhishingPortalDbContext2>(options =>
            {
                options.UseSqlServer(conString);
            });

            break;

        default: throw new Exception($"Invalid SqlProvider configuration [{sqlProvider}]");
    }

    #endregion
}

builder.Services.AddIdentityCore<PhishingPortalUser>()
   .AddRoles<IdentityRole>()
   .AddEntityFrameworkStores<PhishingPortalDbContext2>()
   .AddSignInManager()
   .AddDefaultTokenProviders();


if (!useWindowsAuthentication)
{

   
    builder.Services
        .AddAuthentication(IdentityConstants.ApplicationScheme)
        .AddMicrosoftAccount(options =>
        {
            options.SignInScheme = IdentityConstants.ExternalScheme;
            options.ClientId = aadClientId;
            options.ClientSecret = aadClientSecret;
            options.AuthorizationEndpoint = $"https://login.microsoftonline.com/{aadTenant}/oauth2/v2.0/authorize";
            options.TokenEndpoint = $"https://login.microsoftonline.com/{aadTenant}/oauth2/v2.0/token";
        })
        .AddGoogle(googleOptions =>
        {
            googleOptions.ClientId = googleClientId;
            googleOptions.ClientSecret = googleClientSecret;
            googleOptions.SignInScheme = IdentityConstants.ExternalScheme;
            googleOptions.CallbackPath = new PathString("/signin-google");
        })
        .AddIdentityCookies();
}
else
{

    builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
          .AddNegotiate();

    builder.Services.AddAuthorization(options =>
    {
        // By default, all incoming requests will be authorized according to the default policy.
        options.FallbackPolicy = options.DefaultPolicy;
    });
}


//.AddOpenIdConnect("oidc", options =>
//{
//    options.Authority = authority;
//    options.ClientId = aadClientId;
//    options.ClientSecret = aadClientSecret;
//    options.ResponseType = "code";
//    options.SaveTokens = true;
//    options.GetClaimsFromUserInfoEndpoint = true;
//    options.UseTokenLifetime = false;
//    options.CallbackPath = new Microsoft.AspNetCore.Http.PathString("/signin-oidc");
//    options.Scope.Add("openid");
//    options.Scope.Add("profile");
//    options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = "name" };
//    options.Events = new OpenIdConnectEvents
//    {
//        OnAccessDenied = context =>
//        {
//            context.HandleResponse();
//            context.Response.Redirect("/");
//            return Task.CompletedTask;
//        },

//        OnUserInformationReceived = userInfo =>
//        {
//            return Task.CompletedTask;
//        }

//    };
//});

builder.Services.AddSingleton<IEmailClient, SmtpEmailClient>();
builder.Services.AddTransient<IdentityUIServices.IEmailSender, EmailSender>();

builder.Services.AddScoped<ITenantAdmin, TenantAdmin>();
builder.Services.AddSingleton<TenantAdminRepoConfig>();
builder.Services.AddScoped<ITenantAdminRepository, TenantAdminRepository>();
builder.Services.AddSingleton<INsLookupHelper, NsLookupHelper>();
builder.Services.AddScoped<ITenantDbResolver, TenantDbResolver>();


var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// check if picked the correct configurations
logger.LogInformation($"UseSqlLite: {useSqlLite}");
logger.LogInformation($"SqlProvider: {sqlProvider}");
var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using (var scope = scopeFactory.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PhishingPortalDbContext2>();
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

//app.UseSwagger();
//app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ODataDemo v1"));

//app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), @"App_Data", "trainingvideo")),

    RequestPath = new PathString("/trainingvideo")
});

app.UseRouting();

if(!useWindowsAuthentication) 
    app.UseAuthentication();
else
    app.UseAdAuthentication();

app.UseAuthorization();

app.Use((context, next) => {

    var cookie = context.Request.Cookies;
    return next(context);

});

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();

    endpoints.MapFallbackToFile("index.html");
});


app.Run();
