using CourseManager.CLI.Core.Services;
using Microsoft.Extensions.Logging;

namespace CourseManager.CLI.ConsoleApp.Commands
{
    /// <summary>
    /// Command to list all instructors in the system in a tabular format
    /// </summary>
    /// <remarks>
    /// This command displays all instructors with their basic information
    /// including ID, name, email and department affiliation.
    /// </remarks>
    public class ListInstructorsCommand : CommandBase
    {
        /// <summary>
        /// Service responsible for instructor-related operations
        /// </summary>
        private readonly IInstructorService _instructorService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListInstructorsCommand"/> class
        /// </summary>
        /// <param name="instructorService">The instructor service for database operations</param>
        /// <param name="logger">The logger for recording application events</param>
        /// <exception cref="ArgumentNullException">Thrown when instructorService is null</exception>
        public ListInstructorsCommand(
            IInstructorService instructorService,
            ILogger<ListInstructorsCommand> logger) : base(logger)
        {
            _instructorService = instructorService ?? throw new ArgumentNullException(nameof(instructorService));
        }

        /// <summary>
        /// Executes the list instructors command, retrieving and displaying all instructors in a formatted table
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public override async Task ExecuteAsync()
        {
            try
            {
                Console.WriteLine("=== INSTRUCTOR LISTING ===");

                // Retrieve all instructors from the database
                var instructors = await _instructorService.GetAllInstructorsAsync();

                // Check if we have any instructors to display
                if (!instructors.Any())
                {
                    Console.WriteLine("No instructors found.");
                    return;
                }

                // Display instructor information in a tabular format with aligned columns
                Console.WriteLine("{0,-36} {1,-15} {2,-15} {3,-25} {4}",
                    "ID", "First Name", "Last Name", "Email", "Department");
                Console.WriteLine(new string('-', 100));

                // Iterate through each instructor and display their information
                foreach (var instructor in instructors)
                {
                    Console.WriteLine("{0,-36} {1,-15} {2,-15} {3,-25} {4}",
                        instructor.Id,
                        instructor.FirstName,
                        instructor.LastName,
                        instructor.Email,
                        instructor.Department);
                }

                // Display a summary count of all instructors
                Console.WriteLine($"\nTotal Instructors: {instructors.Count()}");
            }
            catch (Exception ex)
            {
                // Display a user-friendly error message
                Console.WriteLine($"\nError retrieving instructors: {ex.Message}");

                // Log the detailed error for troubleshooting purposes
                _logger.LogError(ex, "Error retrieving instructors");
            }
        }
    }
}
