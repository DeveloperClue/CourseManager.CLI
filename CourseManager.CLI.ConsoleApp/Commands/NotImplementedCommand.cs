using Microsoft.Extensions.Logging;

namespace CourseManager.CLI.ConsoleApp.Commands
{
    /// <summary>
    /// Placeholder command for features that are not yet implemented
    /// </summary>
    /// <remarks>
    /// This command serves as a graceful fallback for menu options that have been
    /// defined in the interface but not yet implemented in the codebase. Instead
    /// of throwing an exception or causing an error, it displays a friendly message
    /// to the user and logs a warning for developers. This follows the Null Object
    /// pattern by providing a safe default behavior.
    /// </remarks>
    public class NotImplementedCommand : CommandBase
    {
        /// <summary>
        /// The name of the command that was attempted but is not implemented
        /// </summary>
        private readonly string _commandName;

        /// <summary>
        /// Initializes a new instance of the NotImplementedCommand class
        /// </summary>
        /// <param name="commandName">The name of the command that was attempted</param>
        /// <param name="logger">Logger for recording command execution</param>
        public NotImplementedCommand(string commandName, ILogger<NotImplementedCommand> logger)
            : base(logger)
        {
            _commandName = commandName;
        }

        /// <summary>
        /// Executes the not-implemented command by displaying a message to the user
        /// </summary>
        /// <returns>A completed task</returns>
        /// <remarks>
        /// This implementation provides user feedback about the unavailable feature 
        /// and logs a warning message for tracking which unimplemented features
        /// are being frequently requested by users. This can help prioritize
        /// development efforts.
        /// </remarks>
        public override Task ExecuteAsync()
        {
            // Inform the user that the requested command is not available
            Console.WriteLine($"The command '{_commandName}' has not been implemented yet.");

            // Log a warning for development tracking purposes
            _logger.LogWarning("Attempted to execute unimplemented command: {CommandName}", _commandName);

            // Return a completed task as this operation is synchronous
            return Task.CompletedTask;
        }
    }
}
