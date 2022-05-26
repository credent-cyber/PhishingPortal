using PhishingPortal.Services.Notification;
using Microsoft.Extensions.Configuration.Json;
using PhishingPortal.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Duende.IdentityServer.EntityFramework.Options;

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
        services.AddLogging();

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
            //TODO: 
        }

        services.AddSingleton<IEmailSender, Office365SmtpClient>();
        services.AddSingleton<IEmailCampaignExecutor, EmailCampaignExecutor>();
        services.AddSingleton<ITenantDbConnManager, TenantDbConnManager>();
        services.AddHostedService<Worker>();

    })
    .Build();

await host.RunAsync();
