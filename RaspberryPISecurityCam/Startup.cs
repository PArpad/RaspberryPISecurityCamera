using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RaspberryPISecurityCam.Data;
using RaspberryPISecurityCam.Models;
using RaspberryPISecurityCam.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using RaspberryPISecurityCam.Authorization;
using Microsoft.EntityFrameworkCore.Storage;
using log4net;
using System.Reflection;
using System.IO;
using log4net.Config;
using Microsoft.Extensions.Logging;
using RaspberryPISecurityCam.Logging;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.AspNetCore.Http;
using Hangfire;
using Hangfire.SQLite;
using Hangfire.MemoryStorage;
using RaspberryPISecurityCam.Classes;
using RaspberryPISecurityCam.Interfaces;

namespace RaspberryPISecurityCam
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
            services.AddSession();

            var connectionString = "Data Source = " + Path.Combine(Directory.GetCurrentDirectory(), "SecurityApp.db");
            Console.WriteLine(connectionString);
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString));


            services.AddHangfire(config =>
                config.UseMemoryStorage());

            services.AddIdentity<ApplicationUser, IdentityRole>(options=>
            {
                options.User.AllowedUserNameCharacters = "aábcdeéfghiíjklmnoóöőpqrstuúüűvwxyz" + "aábcdeéfghiíjklmnoóöőpqrstuúüűvwxyz".ToUpper() + ".-_@,; ";
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();


            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile(Path.Combine("ClientSecrets", "client_secrets.json"), false, true);
            var googleAuthSecretConfiguration = builder.Build();

            services.AddAuthentication().AddGoogle(googleOptions =>
                {
                    googleOptions.ClientId = googleAuthSecretConfiguration["web:client_id"];
                    googleOptions.ClientSecret = googleAuthSecretConfiguration["web:client_secret"];
                });


            services.AddSingleton<FileDeleter>();
            services.AddSingleton<AlarmHandler>();

            services.AddSingleton<EmailSenderSMTP>();
            services.AddSingleton<EmailSenderGmailAPI>();
            services.AddSingleton<IEmailSenderFactory, EmailSenderFactory>();

            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                                 .RequireAuthenticatedUser()
                                 .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            });

            services.AddScoped<IAuthorizationHandler,
                      SecAppUserIsOwnerAuthorizationHandler>();

            services.AddSingleton<IAuthorizationHandler,
                                  SecAppUserAdministratorsAuthorizationHandler>();

            services.AddSingleton<IAuthorizationHandler,
                                  SecAppUserManagerAuthorizationHandler>();

            services.AddCors();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseSession();
            loggerFactory.AddLog4Net();
            Log4NetLoglevelChanger.ChangeLogLevel();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            //app.UseRewriter(new RewriteOptions().AddIISUrlRewrite(env.ContentRootFileProvider, "urlRewrite.config"));

            app.UseStaticFiles();

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), "Images")),
                RequestPath = "/MediaFiles"
            });

            app.UseAuthentication();

            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                //context.Database.Migrate();
                context.Database.EnsureCreated();
            }

            app.UseHangfireServer();
            app.UseHangfireDashboard();

            app.MapWhen(IsStreamingPath, builder => builder.RunProxy(new ProxyOptions
            {
                Scheme = "http",
                Host = "raspberrypi",
                Port = "8081"
            }));

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private static bool IsStreamingPath(HttpContext httpContext)
        {
            return httpContext.Request.Path.Value.StartsWith(@"/stream/", StringComparison.OrdinalIgnoreCase);
        }
    }
}
