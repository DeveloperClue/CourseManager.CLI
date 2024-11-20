using CourseManager.CLI.Core.Services;
using Microsoft.Extensions.Logging;

namespace CourseManager.CLI.ConsoleApp.Commands
{
    /// <summary>
    /// Command to list all courses in the system in a tabular format
    /// </summary>
    public class ListCoursesCommand : CommandBase
    {
        /// <summary>
        /// Service responsible for course-related operations
        /// </summary>
        private readonly ICourseService _courseService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListCoursesCommand"/> class
        /// </summary>
        /// <param name="courseService">The course service for database operations</param>
        /// <param name="logger">The logger for recording application events</param>
        /// <exception cref="ArgumentNullException">Thrown when courseService is null</exception>
        public ListCoursesCommand(ICourseService courseService, ILogger<ListCoursesCommand> logger)
            : base(logger)
        {
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
        }

        /// <summary>
        /// Executes the list courses command, retrieving and displaying all courses in a formatted table
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public override async Task ExecuteAsync()
        {
            try
            {
                var courses = await _courseService.GetAllCoursesAsync();

                Console.WriteLine("\n=== COURSE LISTING ===\n");

                if (!courses.Any())
                {
                    Console.WriteLine("No courses found.");
                    return;
                }

                // Create a formatted header row with fixed column widths for better readability
                Console.WriteLine($"{"ID",-36} {"Code",-10} {"Title",-30} {"Credits",-10} {"Department",-20}");

                // Add a visual separator line between the header and data rows
                Console.WriteLine(new string('-', 110));

                // Sort courses by department first, then by course code for logical grouping
                // and display each course with consistent column alignment
                foreach (var course in courses.OrderBy(c => c.Department).ThenBy(c => c.Code))
                {
                    // Format each course with fixed-width columns using the -N notation
                    // where N is the width and the minus sign indicates left alignment
                    Console.WriteLine($"{course.Id,-36} {course.Code,-10} {course.Title,-30} {course.Credits,-10} {course.Department,-20}");
                }

                // Display a summary count of all courses at the bottom of the list
                Console.WriteLine($"\nTotal Courses: {courses.Count()}");
            }
            catch (Exception ex)
            {
                // Display a user-friendly error message
                Console.WriteLine($"Error retrieving courses: {ex.Message}");

                // Log the detailed error for troubleshooting purposes
                _logger.LogError(ex, "Error in ListCoursesCommand");
            }
        }
    }
}
