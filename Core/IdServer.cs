using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DI.TokenService.Core
{
    public class IdServer
    {
        private readonly IIdentityServerBuilder _builder;

        private IdServer(IServiceCollection serviceCollection)
        {
            _builder = serviceCollection.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.EmitStaticAudienceClaim = true;
            });
        }

        private IdServer AddIdentityResources()
        {
            var resources = new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
            _builder.AddInMemoryIdentityResources(resources);

            return this;
        }

        private IdServer AddApiResources()
        {
            var apiResources = new List<ApiResource>
            {
                new("boardsapi") {Scopes = {"boardsapi"}}
            };

            _builder.AddInMemoryApiResources(apiResources);

            return this;
        }

        private IdServer AddApiScopes()
        {
            var apiScopes = new List<ApiScope>
            {
                new("boardsapi", "DOTARS Boards API")
            };

            _builder.AddInMemoryApiScopes(apiScopes);
            return this;
        }


        private IdServer AddClients()
        {
            var clients = new List<Client>();
            clients.Add(new Client
            {
                ClientId = "dotars_boards",
                ClientName = "DOTARS Boards Application",
                ClientUri = "http://localhost:4200",

                AllowedGrantTypes = GrantTypes.Implicit,
                //ClientSecrets =
                //{
                //    new Secret("dotars".Sha256())
                //},

                RequireClientSecret = false,
                RedirectUris = { "http://localhost:4200/login_complete" },
                PostLogoutRedirectUris = { "http://localhost:4200/logout" },
                AllowedCorsOrigins = { "http://localhost:4200" },
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "boardsapi"
                },
                AllowAccessTokensViaBrowser = true
            });
            _builder.AddInMemoryClients(clients);
            return this;
        }

        private IdServer AddUsers()
        {
            _builder.AddTestUsers(TestUsers.Users);
            return this;
        }

        private IdServer AddCredentials()
        {
            _builder.AddDeveloperSigningCredential();
            return this;
        }

        public static IdServer Start(IServiceCollection serviceCollection)
        {
            var idServer = new IdServer(serviceCollection);

            idServer.AddIdentityResources();
            idServer.AddApiResources();
            idServer.AddClients();
            idServer.AddApiScopes();
            idServer.AddUsers();
            idServer.AddCredentials();


            return idServer;
        }
    }
}