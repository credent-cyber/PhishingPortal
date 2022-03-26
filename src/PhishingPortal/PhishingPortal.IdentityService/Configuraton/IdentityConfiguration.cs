using IdentityModel;

using IdentityServer4.Models;
using IdentityServer4.Test;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PhishingPortal.IdentityService.Configuration
{
    public class IdentityConfiguration
    {
        public static IEnumerable<IdentityResource> IdentityResources => new IdentityResource[]
                                                    {
                                                        new IdentityResources.OpenId(),
                                                        new IdentityResources.Profile(),
                                                    };

        public static IEnumerable<ApiScope> ApiScopes => new ApiScope[]
        {
            new ApiScope("tenant.admins","Tenant Admin"),
            new ApiScope("tenant.users","Tenant User"),
            new ApiScope("tenant.reports","Tenant User"),

            new ApiScope("phishingPortalApi.admins", "PhishingPortalWebApi Admin"),
            new ApiScope("phishingPortalApi.users", "PhishingPortalWebApi Admin"),
            new ApiScope("phishingPortalApi.reports", "PhishingPortalWebApi Report User"),

        };

        public static IEnumerable<ApiResource> ApiResources => new ApiResource[]
        {
            new ApiResource
            {
                Name = "adminPortal",
                DisplayName = "PhishingPortal Web Api",
                Description = "Web api to support production meta information and production api",
                Scopes = new List<string>{
                    "phishingPortalApi.admin",
                    "phishingPortalApi.users",
                    "phishingPortalApi.reports",
                },
                ApiSecrets = new List<Secret>{ new Secret("secret".Sha256())  }
            },
             new ApiResource
            {
                Name = "tenantPortal",
                DisplayName = "PhishingPortal Web Api",
                Description = "Web api to support production meta information and production api",
                Scopes = new List<string>{

                       "tenant.admins",
                       "tenant.users",
                       "tenant.reports"
                },
                ApiSecrets = new List<Secret>{ new Secret("tenant_secret".Sha256())  }
            }
        };


        public static IEnumerable<Client> Clients => new Client[]
        {
            // for swagger ui
            new Client
            {
                ClientId = "swagger",
                ClientName = "Swagger UI",
                ClientSecrets = {new Secret("swagger_secret".Sha256())},
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,
                RedirectUris = {"https://localhost:5001/swagger/oauth2-redirect.html"},
                AllowedCorsOrigins = {"https://localhost:44388"},
                AllowedScopes = {  
                        "tenant.admins",
                        "tenant.users",
                        "tenant.reports",
                        "phishingPortalApi.admin",
                        "phishingPortalApi.users",
                        "phishingPortalApi.reports" }
            },

            // custom grant for admin portal
            new Client
            {
                ClientId = "adminPortal",
                ClientName = "Admin Portal",
                ClientSecrets = {new Secret("D14D16AE-59DC-41DE-AC97-024474F13A9A".Sha256())},
                AllowedGrantTypes = { GrantType.ClientCredentials },
                RequirePkce = true,
                RequireClientSecret = false,
                AllowedScopes = {  
                    "phishingPortalApi.admin",
                    "phishingPortalApi.users",
                    "phishingPortalApi.reports", 
                }
            },
            
           // client for each tenant, this can used in 
            new Client
            {
                ClientId = "TenantPortal",
                ClientName = "Tenant Portal",
                ClientSecrets = {new Secret("21868970-2366-4137-800D-6FC7AAFDB71C".Sha256())},
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                RequirePkce = true,
                RequireClientSecret = true,
                AllowedScopes = { 
                       "tenant.admins",
                       "tenant.users",
                       "tenant.reports" 
                }
            }
        };
    }
}
