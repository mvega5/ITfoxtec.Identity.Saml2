﻿using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ITfoxtec.Identity.Saml2.MvcCore.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore;
using System.ServiceModel.Security;
using System.Security.Cryptography.X509Certificates;
using ITfoxtec.Identity.Saml2.Util;

namespace TestIdPCore
{
    public class Startup
    {
        public static IHostingEnvironment AppEnvironment { get; private set; }

        public Startup(IHostingEnvironment env)
        {
            AppEnvironment = env;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            services.Configure<Saml2Configuration>(Configuration.GetSection("Saml2"));
            services.Configure<Saml2Configuration>(saml2Configuration =>
            {
                saml2Configuration.SigningCertificate = CertificateUtil.Load(AppEnvironment.MapToPhysicalFilePath(Configuration["Saml2:SigningCertificateFile"]), Configuration["Saml2:SigningCertificatePassword"]);
                saml2Configuration.AllowedAudienceUris.Add(saml2Configuration.Issuer);
            });

            services.AddSaml2();

            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseSaml2();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
