using CourseManager.CLI.ConsoleApp.Commands;
using CourseManager.CLI.ConsoleApp.Menu;
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
        /// Application entry point that configures logging, dependency injection,
        /// and starts the menu-driven user interface.
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

                // Run initial data setup to ensure repository files exist with sample data if needed
                // This populates empty repositories with initial data for testing and demonstration
                await DataInitializer.EnsureInitialDataAsync(host.Services);

                // Start the application by launching the menu-driven interface
                // The menu manager will handle all user interaction from this point forward
                var menuManager = host.Services.GetRequiredService<IMenuManager>();
                await menuManager.StartAsync();
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
        /// Creates and configures the host builder with application settings and services
        /// </summary>
        /// <param name="args">Command-line arguments for configuration</param>
        /// <returns>A configured IHostBuilder ready to build the application host</returns>
        /// <remarks>
        /// This method configures:
        /// 1. Application configuration sources (JSON, environment variables, command line)
        /// 2. Service registrations for dependency injection
        /// 3. Logging providers
        /// </remarks>
        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                // Configure application settings from multiple sources with precedence order
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true) // Base configuration
                        .AddEnvironmentVariables() // Override with environment variables
                        .AddCommandLine(args);     // Override with command-line arguments
                })
                // Configure dependency injection services
                .ConfigureServices((hostContext, services) =>
                {
                    // Get application configuration
                    var configuration = hostContext.Configuration;

                    // Determine data directory from config or default to a "Data" subdirectory
                    // This is where all JSON repository files will be stored
                    var dataDirectory = configuration.GetValue<string>("DataDirectory")
                            ?? Path.Combine(Directory.GetCurrentDirectory(), "Data");

                    // Ensure data directory exists to prevent file operation errors
                    Directory.CreateDirectory(dataDirectory);

                    // Register repository implementations with their file paths
                    // Each repository is created with its own file path and typed logger

                    // Course repository - reads/writes from "courses.json"
                    services.AddSingleton<ICourseRepository>(provider =>
                            new CourseRepository(dataDirectory, provider.GetRequiredService<ILogger<CourseRepository>>()));

                    // Instructor repository - reads/writes from "instructors.json"
                    services.AddSingleton<IInstructorRepository>(provider =>
                            new InstructorRepository(dataDirectory, provider.GetRequiredService<ILogger<InstructorRepository>>()));

                    // Register business logic services
                    // These services implement the application's core business rules
                    // and provide a layer of abstraction over the data repositories
                    services.AddSingleton<ICourseService, CourseService>();
                    services.AddSingleton<IInstructorService, InstructorService>();

                    // Register the menu system that provides the user interface
                    // The MenuManager is the main controller for user interaction
                    services.AddSingleton<IMenuManager, MenuManager>();

                    // Register the command factory for creating command objects
                    // This factory creates the appropriate command handler for each menu selection
                    services.AddSingleton<ICommandFactory, CommandFactory>();
                })
                // Configure Serilog as the logging provider
                .UseSerilog();
    }
}