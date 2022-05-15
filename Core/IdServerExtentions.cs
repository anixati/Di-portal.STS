using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DI.TokenService.Core
{
    public static class IdServerExtensions
    {
        public static void StartIdServer(this IServiceCollection serviceCollection)
        {
            IdServer.Start(serviceCollection);
        }

        public static void UseIdServer(this IApplicationBuilder application)
        {
            application.UseIdentityServer();
        }
    }
}