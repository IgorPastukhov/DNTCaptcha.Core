﻿using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using DNTCaptcha.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters.Json;
using Newtonsoft.Json.Serialization;

namespace DNTCaptcha.TestWebApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });
            services.AddMvc(options => { options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()); })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_0)
                .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());
            services.AddDirectoryBrowser();

            services.AddDNTCaptcha(options =>
                // options.UseSessionStorageProvider()
                options.UseMemoryCacheStorageProvider()
                );            
            services.AddSession();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors(policyName: "CorsPolicy");

            app.UseDeveloperExceptionPage();

            // Serve wwwroot as root
            app.UseFileServer();

            app.UseFileServer(new FileServerOptions
            {
                // Set root of file server
                FileProvider =
                    new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "bower_components")),
                // Only react to requests that match this path
                RequestPath = "/bower_components",
                // Don't expose file system
                EnableDirectoryBrowsing = false
            });

            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}