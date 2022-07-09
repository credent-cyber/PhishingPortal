using PhishingPortal.Services.Notification;
using PhishingPortal.DataContext;
using Microsoft.EntityFrameworkCore;
using PhishingPortal.Common;
using Serilog;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(hostBuilder =>
    {
        hostBuilder.AddCommandLine(args);
        hostBuilder.AddEnvironmentVariables(prefix: "DOTNETCORE_");
    })
    .ConfigureAppConfiguration((hostContext, config) =>
    {

        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", true, true);

    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddLogging((builder) =>
        {
            Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(hostContext.Configuration)
            .CreateLogger();
            builder.AddSerilog();
        });

        var useSqlLite = hostContext.Configuration.GetValue<bool>("UseSqlLite");
        var connectionString = hostContext.Configuration.GetValue<string>("CentralDbConnString");

        if (useSqlLite)
        {
            var ctxBuilder = new DbContextOptionsBuilder<CentralDbContext>();
            ctxBuilder.UseSqlite(connectionString);

            services.AddSingleton(new CentralDbContext(ctxBuilder.Options));

        }
        else
        {
            var conString = hostContext.Configuration.GetConnectionString("DefaultConnection");
            var ctxBuilder = new DbContextOptionsBuilder<CentralDbContext>();
            ctxBuilder.UseMySql(conString, ServerVersion.AutoDetect(conString));
            services.AddSingleton<CentralDbContext>(new CentralDbContext(ctxBuilder.Options));
        }

        services.AddSingleton<IEmailClient, SmtpEmailClient>();
        services.AddSingleton<IEmailCampaignExecutor, EmailCampaignExecutor>();
        services.AddSingleton<ITenantDbConnManager, TenantDbConnManager>();
        services.AddHostedService<Worker>();

    })
    .Build();

await host.RunAsync();
