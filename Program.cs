using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace DI.TokenService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            SetupLogger();
            try
            {
                Log.Information("Starting STS Server ..");
                var hostBuilder = CreateHostBuilder(args);
                var host = hostBuilder.Build();
                Log.Information("Started");
                await RunHostStartUp(host);
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to start ");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static async Task RunHostStartUp(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            await Task.Delay(0); //TODO:Add any startup
        }

        public static void SetupLogger()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var _config = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("logSettings.json")
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_config)
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
#if DEBUG
                    webBuilder.ConfigureKestrel(options =>
                    {
                        options.Listen(IPAddress.Any, 5001, listenOptions =>
                        {
                            listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                            listenOptions.UseHttps(@"C:\Temp\Certs\akdev.pfx", "Welcome1");
                        });
                    });
#endif

                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}