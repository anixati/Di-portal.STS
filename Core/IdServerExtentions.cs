using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DI.TokenService.Core
{
    public static class IdServerExtensions
    {
        public static void StartIdServer(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            IdServer.Start(serviceCollection, configuration);
        }

        public static void UseIdServer(this IApplicationBuilder application)
        {
            application.UseIdentityServer();
        }
    }
}