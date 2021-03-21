using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HomeTicketing
{
    public class Program
    {
        private static string _env = "Sandbox";
        public static void Main(string[] args)
        {
            SetEnvironment();
            CreateHostBuilder(args).Build().Run();
        }

        private static void SetEnvironment()
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", false)
                    .Build();
                _env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logbuilder =>
                {
                    logbuilder.ClearProviders();
                    logbuilder.AddConsole();
                    logbuilder.AddTraceSource("Information, ActivityTracing");
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json");
                    config.AddJsonFile($"appsettings.{_env}.json", optional: true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
