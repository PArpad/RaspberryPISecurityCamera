using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using RaspberryPISecurityCam.Data;
using RaspberryPISecurityCam.Authorization;
using Hangfire;
using RaspberryPISecurityCam.Classes;
using log4net;

namespace RaspberryPISecurityCam
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);
            using (var scope = host.Services.CreateScope())
            {
                var logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                var services = scope.ServiceProvider;
                try
                {
                    var testUserPw = "Admin_0";
                    SeedData.Initialize(services, testUserPw).Wait();
                }
                catch (Exception ex)
                {
                    logger.Error("An error occurred seeding the DB.", ex);
                }
                try
                {
                    FileDeleter fileDeleter = services.GetService<FileDeleter>();
                    AlarmHandler alarmHandler = services.GetService<AlarmHandler>();
                    RecurringJob.AddOrUpdate(() => alarmHandler.RunOperations(), Cron.Minutely);
                    RecurringJob.AddOrUpdate(() => fileDeleter.DeleteFiles(), Cron.Daily);
                }
                catch (Exception ex)
                {
                    logger.Error("An error occurred while setting up RecurringJobs with Hangfire.", ex);
                }

            }

            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://*:5000")
                .UseStartup<Startup>()
                .Build();
    }
}
