using CourseManager.CLI.Core.Infrastructure;
using CourseManager.CLI.Core.Services;
using CourseManager.CLI.Data.Repositories;
using CourseManager.CLI.Data.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CourseManager.CLI.ConsoleApp
{
    /// <summary>
    /// Main entry point and composition root for the CourseManager.CLI application.
    /// Handles application bootstrapping, dependency injection, and startup.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Application entry point that configures logging, dependency injection
        /// </summary>
        /// <param name="args">Command-line arguments passed to the application</param>
        static async Task Main(string[] args)
        {
            // Setup Serilog for structured logging to both console and rolling file
            // This provides both immediate feedback during development and persistent logs for troubleshooting
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("logs/coursemanager.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                // Build the host with configured dependency injection container
                // This creates all required services based on the configuration in CreateHostBuilder
                using var host = CreateHostBuilder(args).Build();
            }
            catch (Exception ex)
            {
                // Log fatal errors that cause the application to terminate
                // This ensures we have a record of any critical failures
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                // Ensure all buffered log events are written before the application exits
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        /// Creates and configures the host builder with services
        /// </summary>
        /// <param name="args">Command-line arguments for configuration</param>
        /// <returns>A configured IHostBuilder ready to build the application host</returns>
        /// <remarks>
        /// This method configures:
        /// 1. Service registrations for dependency injection
        /// 3. Logging providers
        /// </remarks>
        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                // Configure dependency injection services
                .ConfigureServices((hostContext, services) =>
                {                    
                    // Register business logic services
                    // These services implement the application's core business rules
                    // and provide a layer of abstraction over the data repositories
                    services.AddSingleton<ICourseService, CourseService>();
                })
                // Configure Serilog as the logging provider
                .UseSerilog();
    }
}