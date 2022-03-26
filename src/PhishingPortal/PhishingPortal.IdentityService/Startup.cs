using IdentityServer4.Models;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

using PhishingPortal.IdentityService.Configuration;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhishingPortal.IdentityService
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

            services.AddControllers();
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryApiScopes(IdentityConfiguration.ApiScopes)
                .AddInMemoryApiResources(IdentityConfiguration.ApiResources)
                .AddInMemoryClients(IdentityConfiguration.Clients);
            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication("Bearer", options =>
                {
                    // auth server base endpoint (this will be used to search for disco doc)
                    options.Authority = "https://localhost:44388";
                });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PhishingPortal.IdentityService", Version = "v1" });
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri("https://localhost:44388/connect/authorize"),
                            TokenUrl = new Uri("https://localhost:44388/connect/token"),
                            Scopes = new Dictionary<string, string>
                                {
                                    {"administrator", "My Aapi Administrator"},
                                    {"user","MyApi User" }
                                }
                        }
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PhishingPortal.IdentityService v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseSwaggerUI(sA =>
            {
                sA.OAuthClientId("demo_client");
                sA.OAuthAppName("myApi");
                sA.OAuthClientSecret("secret".Sha256());
                sA.OAuthUsePkce();
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
