using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DI.TokenService.Store;
using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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
            var ts = Convert.ToInt32(new TimeSpan(2, 0, 0).TotalSeconds);
            var clientUri = configuration["Client:ClientUri"];
            var clients = new List<Client>
            {

                new Client
                {
                    ClientId = configuration["Client:ClientId"],
                    ClientName = configuration["Client:ClientName"],
                    ClientUri =clientUri,
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AbsoluteRefreshTokenLifetime = ts,
                    SlidingRefreshTokenLifetime = ts,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    AccessTokenLifetime = ts,
                    RequirePkce = false,
                    AllowOfflineAccess = true,

                    //ClientSecrets =
                    //{
                    //    new Secret("dotars".Sha256())
                    //},
                    RequireConsent=false,
                    RequireClientSecret = false,
                    RedirectUris = { $"{clientUri}/{configuration["Client:RedirectUri"]}" },
                    //PostLogoutRedirectUris = { $"{clientUri}/{configuration["Client:PostLogoutRedirectUri"]}" },
                    AllowedCorsOrigins = {clientUri},
                    AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
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


            serviceCollection.AddAuthentication()
                .AddWsFederation("WsFederation","Single sign on", options => {

                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.Wtrealm = $"{configuration["ADFS3:Realm"]}";
                    options.MetadataAddress = $"{configuration["ADFS3:Authority"]}/FederationMetadata/2007-06/FederationMetadata.xml";
                   options.Events.OnTicketReceived += OnTicketReceived;
                })
                //.AddOpenIdConnect("adfs", "Login using AD", options =>
                //{
                //    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                //    options.SignOutScheme = IdentityServerConstants.SignoutScheme;
                //    options.Authority = $"{configuration["ADFS4:Authority"]}";
                //    options.ClientId = $"{configuration["ADFS4:ClientId"]}";
                //    options.ClientSecret = $"{configuration["ADFS4:Secret"]}";
                //    options.ResponseType = "id_token";
                //    options.Scope.Add("profile");
                //    options.Scope.Add("email");
                //    options.ResponseMode = "form_post";
                //    options.CallbackPath = "/signin-adfs";
                //    options.SignedOutCallbackPath = "/signout-callback-adfs";
                //    options.RemoteSignOutPath = "/signout-adfs";
                //    options.GetClaimsFromUserInfoEndpoint = true;
                //    options.TokenValidationParameters = new TokenValidationParameters
                //    {
                //        NameClaimType = "name",
                //        RoleClaimType = "role"
                //    };
                //    options.RequireHttpsMetadata = false;
                //})
                ;




        }


        private async static Task OnTicketReceived(TicketReceivedContext ticketReceivedContext)
        {
            await Task.Delay(0);
            var identity = ticketReceivedContext.Principal.Identities.First();
            identity.AddClaim(new Claim("sub", ticketReceivedContext.Principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")));

        }

    }
}