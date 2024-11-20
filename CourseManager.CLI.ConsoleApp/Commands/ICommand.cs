namespace CourseManager.CLI.ConsoleApp.Commands
{
    /// <summary>
    /// Interface for command operations following the Command design pattern
    /// </summary>
    /// <remarks>
    /// This interface defines the contract for all command classes in the application.
    /// It follows the Command design pattern, which encapsulates a request as an object,
    /// allowing for parameterization of clients with different requests, queuing of requests,
    /// and logging of the operations. Each command represents a specific user action or operation.
    /// </remarks>
    public interface ICommand
    {
        /// <summary>
        /// Executes the command asynchronously
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <remarks>
        /// This method serves as the entry point for executing a command's functionality.
        /// It should be implemented by concrete command classes to perform their specific actions.
        /// The asynchronous design allows for non-blocking I/O operations and better responsiveness.
        /// </remarks>
        Task ExecuteAsync();
    }
}
