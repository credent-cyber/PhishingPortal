using Microsoft.EntityFrameworkCore;
using PhishingPortal.Server;
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
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddControllers()
    .AddODataControllers()
    .AddNewtonsoftJson();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PhishsimsODataDemo", Version = "v1" });
});



var conString = builder.Configuration.GetValue<string>("SqlLiteConnectionString");
var useSqlLite = builder.Configuration.GetValue<bool>("UseSqlLite");
var sqlProvider = builder.Configuration.GetValue<string>("SqlProvider");

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

    conString = builder.Configuration.GetConnectionString("DefaultConnection");
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

builder.Services.AddIdentity<PhishingPortalUser, IdentityRole>()
    .AddEntityFrameworkStores<PhishingPortalDbContext2>()
    .AddTokenProvider<DataProtectorTokenProvider<PhishingPortalUser>>(TokenOptions.DefaultProvider);

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = false;
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.WithOrigins("https://localhost:7002")
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});


builder.Services.AddSingleton<IEmailClient, SmtpEmailClient>();
builder.Services.AddTransient<IdentityUIServices.IEmailSender, EmailSender>();

builder.Services.AddAuthentication()
    .AddCookie();


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

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ODataDemo v1"));


app.UseCors("AllowSpecificOrigin");
//app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();



app.UseAuthentication();

app.UseAuthorization();


app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();

    endpoints.MapFallbackToFile("index.html");
});


app.Run();
