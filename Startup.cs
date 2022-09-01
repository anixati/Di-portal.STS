using System.Net;
using System.Net.Http;
using System.Net.Security;
using DI.TokenService.Core;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DI.TokenService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {


            //ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) =>
            //{
            //    // local dev, just approve all certs
            //    return true;
            //   // return errors == SslPolicyErrors.None;
            //};

            //services.AddHttpClient("HttpClientWithSSLUntrusted").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            //{
            //    ClientCertificateOptions = ClientCertificateOption.Manual,
            //    ServerCertificateCustomValidationCallback =
            //        (httpRequestMessage, cert, cetChain, policyErrors) => true
            //});
            services.AddControllersWithViews();

            services.AddCors(options =>
            {
                options.AddPolicy("IdentityServer", policy =>
                    {
                        policy
                       .AllowAnyOrigin()
                       .SetIsOriginAllowedToAllowWildcardSubdomains()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                    });
                options.DefaultPolicyName = "IdentityServer";
            });

            services.AddSingleton<ICorsPolicyService>((container) =>
            {
                var logger = container.GetRequiredService<ILogger<DefaultCorsPolicyService>>();
                return new DefaultCorsPolicyService(logger)
                {
                    AllowAll = true
                };
            });


            services.Configure<CookiePolicyOptions>(options =>
            {
             //   options.Secure = CookieSecurePolicy.Always;
             options.CheckConsentNeeded = context => true;
             options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.StartIdServer(Configuration);

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            //app.UseCookiePolicy(new CookiePolicyOptions
            //{
            //   // Secure = CookieSecurePolicy.Always,
            //    MinimumSameSitePolicy = SameSiteMode.Strict
            //});
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseCors("IdentityServer");
         
          

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthorization();
            app.UseIdServer();


           
            //app.UseCookiePolicy(new CookiePolicyOptions
            //{
            //    HttpOnly = HttpOnlyPolicy.None,
            //    MinimumSameSitePolicy = SameSiteMode.None,
            //    Secure = CookieSecurePolicy.Always
            //});

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}