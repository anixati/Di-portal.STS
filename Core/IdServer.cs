using System.Collections.Generic;
using DI.TokenService.Store;
using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DI.TokenService.Core
{
    public class IdServer
    {
        public static void Start(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddSingleton<DapperContext>();

            var builder = serviceCollection.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.EmitStaticAudienceClaim = true;
            });
            builder.AddDeveloperSigningCredential();
            builder.AddInMemoryIdentityResources(new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            });
            builder.AddInMemoryApiResources(new List<ApiResource>
            {
                new("boardsapi") {Scopes = {"boardsapi"}}
            });
            builder.AddInMemoryApiScopes(new List<ApiScope>
            {
                new("boardsapi", "DOTARS Boards API")
            });


            //-- clients

            var clientUri = configuration["Client:ClientUri"];
            var clients = new List<Client>
            {
                new Client
                {
                    ClientId = configuration["Client:ClientId"],
                    ClientName = configuration["Client:ClientName"],
                    ClientUri =clientUri,
                    AllowedGrantTypes = GrantTypes.Implicit,
                    //ClientSecrets =
                    //{
                    //    new Secret("dotars".Sha256())
                    //},
                    RequireConsent=false,
                    RequireClientSecret = false,
                    RedirectUris = { $"{clientUri}/{configuration["Client:RedirectUri"]}" },
                    PostLogoutRedirectUris = { $"{clientUri}/{configuration["Client:PostLogoutRedirectUri"]}" },
                    AllowedCorsOrigins = { clientUri},
                    AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "boardsapi"
                },
                   AllowAccessTokensViaBrowser = true
                }
            };
            builder.AddInMemoryClients(clients);
            //--users
            builder.Services.AddSingleton<IUserRepository, UserRepository>();
            builder.AddProfileService<CustomProfileService>();
            builder.AddResourceOwnerValidator<CustomRopValidator>();
        }
    }
}