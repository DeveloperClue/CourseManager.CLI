using CourseManager.CLI.Core.Services;
using CourseManager.CLI.Core.Infrastructure;
using Microsoft.Extensions.Logging;

namespace CourseManager.CLI.ConsoleApp.Commands
{
    /// <summary>
    /// Interface for the command factory that creates command objects based on user input
    /// </summary>
    /// <remarks>
    /// This interface provides a factory method pattern for creating command objects,
    /// decoupling command creation from command execution.
    /// </remarks>
    public interface ICommandFactory
    {
        /// <summary>
        /// Creates a command instance based on the specified command name
        /// </summary>
        /// <param name="commandName">The name of the command to create (case-insensitive)</param>
        /// <returns>
        /// An ICommand instance that can execute the requested action, or a
        /// NotImplementedCommand if the command name is not recognized
        /// </returns>
        ICommand CreateCommand(string commandName);
    }

    /// <summary>
    /// Implementation of the command factory that creates command objects using dependency injection
    /// </summary>
    /// <remarks>
    /// The CommandFactory uses the Command pattern to encapsulate each user action in a separate object.
    /// It leverages dependency injection to provide each command with its required services.
    /// </remarks>
    public class CommandFactory : ICommandFactory
    {
        /// <summary>
        /// The service provider used to resolve dependencies for command objects
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the CommandFactory with the specified service provider
        /// </summary>
        /// <param name="serviceProvider">The DI service provider for resolving command dependencies</param>
        /// <exception cref="ArgumentNullException">Thrown when serviceProvider is null</exception>
        public CommandFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Creates a command instance based on the specified command name
        /// </summary>
        /// <param name="commandName">The name of the command to create (case-insensitive)</param>
        /// <returns>
        /// An ICommand instance that can execute the requested action, or a
        /// NotImplementedCommand if the command name is not recognized or null
        /// </returns>
        /// <remarks>
        /// This method uses a switch expression to map command names to their implementing classes,
        /// and resolves dependencies from the DI container. This allows for a clean, declarative
        /// mapping of command names to their implementations.
        /// </remarks>
        public ICommand CreateCommand(string commandName)
        {
            // Handle null command name by returning a NotImplementedCommand
            if (commandName == null)
            {
                return new NotImplementedCommand("null", GetService<ILogger<NotImplementedCommand>>());
            }

            // Convert to lowercase for case-insensitive matching
            // The switch expression maps command names to their concrete implementations
            return commandName.ToLower() switch
            {
                // Basic commands
                "help" => new NotImplementedCommand(commandName, GetService<ILogger<NotImplementedCommand>>()),

                "exit" => new NotImplementedCommand(commandName, GetService<ILogger<NotImplementedCommand>>()),

                // Course commands
                "list-courses" => new ListCoursesCommand(
                    GetService<ICourseService>(), GetService<ILogger<ListCoursesCommand>>()),

                "view-course" => new ViewCourseCommand(
                    GetService<ICourseService>(), 
                    GetService<ILogger<ViewCourseCommand>>()),

                "add-course" => new AddCourseCommand(GetService<ICourseService>(), GetService<ILogger<AddCourseCommand>>()),

                "update-course" => new UpdateCourseCommand(GetService<ICourseService>(), GetService<ILogger<UpdateCourseCommand>>()),

                "delete-course" => new DeleteCourseCommand(
                    GetService<ICourseService>(), 
                    GetService<ILogger<DeleteCourseCommand>>()),

                "find-courses-by-department" => new FindCoursesByDepartmentCommand(GetService<ICourseService>(), GetService<ILogger<FindCoursesByDepartmentCommand>>()),

                // Default/Unknown command
                _ => new NotImplementedCommand(commandName, GetService<ILogger<NotImplementedCommand>>())
            };
        }

        /// <summary>
        /// Helper method to retrieve a service of the specified type from the DI container
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve</typeparam>
        /// <returns>The requested service instance</returns>
        /// <remarks>
        /// This method simplifies the syntax for resolving dependencies from the DI container.
        /// It assumes the service is registered and will throw an exception if it's not found.
        /// </remarks>
        private T GetService<T>() where T : notnull
        {
            // Get the service from the DI container and force non-null with the ! operator
            // This is safe because all required services should be registered at startup
            return (T)_serviceProvider.GetService(typeof(T))!;
        }
    }
}
