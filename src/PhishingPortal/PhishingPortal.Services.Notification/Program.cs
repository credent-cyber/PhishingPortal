using PhishingPortal.Services.Notification;
using PhishingPortal.DataContext;
using Microsoft.EntityFrameworkCore;
using PhishingPortal.Common;
using Serilog;
using PhishingPortal.Services.Notification.Email;
using PhishingPortal.Services.Notification.Helper;
using PhishingPortal.Services.Notification.Sms;
using PhishingPortal.Services.Notification.Whatsapp;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(hostBuilder =>
    {
        hostBuilder.AddCommandLine(args);
        hostBuilder.AddEnvironmentVariables(prefix: "DOTNETCORE_");
    })
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        Console.WriteLine($"Enivronment: {hostContext.HostingEnvironment.EnvironmentName}");
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
        var sqlProvider = hostContext.Configuration.GetValue<string>("SqlProvider");
        if (string.IsNullOrEmpty(sqlProvider))
        {
            sqlProvider = "mysql";
        }
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


            switch (sqlProvider)
            {
                case "mysql":

                    ctxBuilder.UseMySql(conString, ServerVersion.AutoDetect(conString));

                    break;

                case "mssql":

                    ctxBuilder.UseSqlServer(conString);
                    break;

                default: throw new Exception($"Invalid SqlProvider configuration [{sqlProvider}]");
            }

            services.AddSingleton<CentralDbContext>(new CentralDbContext(ctxBuilder.Options));
        }

        services.AddSingleton<IEmailClient, SmtpEmailClient>();
        services.AddSingleton<IEmailCampaignExecutor, EmailCampaignExecutor>();
        services.AddSingleton<ITenantDbConnManager, TenantDbConnManager>();
       
        services.AddSingleton<AmyntraSmsGatewayConfig>();
        services.AddSingleton<ISmsCampaignExecutor, SmsCampaignExecutor>();
        services.AddSingleton<ISmsGatewayClient, DefaultSmsGatewayClient>();

        services.AddSingleton<WhatsappGatewayConfig>();
        services.AddSingleton<IWhatsappCampaignExecutor, WhatsappCampaignExecutor>();
        services.AddSingleton<IWhatsappGatewayClient, WhatsappMateGatewayClient>();
        
        services.AddHostedService<Worker>();

    })
    .Build();

await host.RunAsync();
