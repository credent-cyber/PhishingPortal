using IdentityServer4.Models;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using PhishingPortal.IdentityServer.Configuration;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using System.Security.Cryptography.X509Certificates;

namespace PhishingPortal.IdentityServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //var rsaCertificate = new X509Certificate2("rsaCert.pfx", "1234");
            var connString = Configuration.GetValue<string>("StoreConnectionString");
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddControllers();
            services.AddIdentityServer()
                  .AddDeveloperSigningCredential()
                  //.AddSigningCredential(rsaCertificate) // rsaCertificate
                 .AddConfigurationStore(options =>
                 {
                     options.ConfigureDbContext = builder =>
                         builder.UseSqlServer(connString,
                             sql => sql.MigrationsAssembly(migrationsAssembly));
                 })
                 .AddOperationalStore(options =>
                 {
                     options.ConfigureDbContext = builder =>
                         builder.UseSqlServer(connString,
                             sql => sql.MigrationsAssembly(migrationsAssembly));

                     // this enables automatic token cleanup. this is optional.
                     options.EnableTokenCleanup = true;
                     options.TokenCleanupInterval = 30;
                 });
            //.AddInMemoryApiScopes(IdentityConfiguration.ApiScopes)
            //.AddInMemoryApiResources(IdentityConfiguration.ApiResources)
            //.AddInMemoryClients(IdentityConfiguration.Clients);
            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication("Bearer", options =>
                {
                    // auth server base endpoint (this will be used to search for disco doc)
                    options.Authority = Configuration.GetValue<string>("Authority");
                });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PhishingPortal.IdentityProvider", Version = "v1" });
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri(Configuration.GetValue<string>("AuthorizationUrl")),
                            TokenUrl = new Uri(Configuration.GetValue<string>("TokenUrl")),
                            Scopes = Configuration.GetSection("Scopes").GetChildren()
                                            .ToDictionary(x => x.Key, x => x.Value)
                            //Scopes = new Dictionary<string, string>
                            //    {
                            //        {"administrator", "My Aapi Administrator"},
                            //        {"user","MyApi User" }
                            //    }
                        }
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            InitializeDatabase(app);

            if (env.IsDevelopment()) 
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PhishingPortal.IdentityProvider v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseSwaggerUI(sA =>
            {
                sA.OAuthClientId(Configuration.GetValue<string>("Swagger:OAuthClientId"));
                sA.OAuthAppName(Configuration.GetValue<string>("Swagger:OAuthAppName"));
                sA.OAuthClientSecret(Configuration.GetValue<string>("Swagger:OAuthClientSecret").Sha256());
                sA.OAuthUsePkce();
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }


        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                if (!context.Clients.Any())
                {
                    foreach (var client in IdentityConfiguration.Clients)
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in IdentityConfiguration.IdentityResources)
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in IdentityConfiguration.ApiResources)
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
                if (!context.ApiScopes.Any())
                {
                    foreach (var scope in IdentityConfiguration.ApiScopes)
                    {
                        context.ApiScopes.Add(scope.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }
    }

    
}
