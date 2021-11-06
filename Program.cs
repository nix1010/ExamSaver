using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamSaver.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ExamSaver
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();

            SetupEnvironment(host);

            host.Run();
        }

        private static void SetupEnvironment(IHost host)
        {
            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider serviceProvider = serviceScope.ServiceProvider;

            IWebHostEnvironment webHostEnvironment = serviceProvider.GetService<IWebHostEnvironment>();

            if (webHostEnvironment.IsDevelopment())
            {
                InitializeDatabaseIfNotExists(serviceProvider);
            }
        }

        private static void InitializeDatabaseIfNotExists(IServiceProvider serviceProvider)
        {
            try
            {
                DatabaseContext databaseContext = serviceProvider.GetRequiredService<DatabaseContext>();

                DatabaseInitializer.Initialize(databaseContext);
            }
            catch (Exception ex)
            {
                ILogger logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred creating the DB.");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
