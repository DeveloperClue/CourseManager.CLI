using CourseManager.CLI.Core.Services;
using Microsoft.Extensions.Logging;

namespace CourseManager.CLI.ConsoleApp.Commands
{
    /// <summary>
    /// Command to find and display courses filtered by department
    /// </summary>
    public class FindCoursesByDepartmentCommand : CommandBase
    {
        /// <summary>
        /// Service responsible for course-related operations
        /// </summary>
        private readonly ICourseService _courseService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FindCoursesByDepartmentCommand"/> class
        /// </summary>
        /// <param name="courseService">The course service for database operations</param>
        /// <param name="logger">The logger for recording application events</param>
        /// <exception cref="ArgumentNullException">Thrown when courseService is null</exception>
        public FindCoursesByDepartmentCommand(
            ICourseService courseService,
            ILogger<FindCoursesByDepartmentCommand> logger) : base(logger)
        {
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
        }

        /// <summary>
        /// Executes the find courses by department command by allowing users to select a department 
        /// and displaying all courses in that department
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public override async Task ExecuteAsync()
        {
            try
            {
                Console.WriteLine("=== FIND COURSES BY DEPARTMENT ===");

                // Get all courses to extract available departments
                var allCourses = await _courseService.GetAllCoursesAsync();
                if (!allCourses.Any())
                {
                    Console.WriteLine("No courses found in the system.");
                    return;
                }

                // Extract unique departments
                var departments = allCourses.Select(c => c.Department).Distinct().OrderBy(d => d).ToList();

                // Display available departments
                Console.WriteLine("\nAvailable Departments:");
                for (int i = 0; i < departments.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {departments[i]}");
                }

                // Get user selection
                int selection = ReadInt("Enter department number to view courses: ", 1, departments.Count);
                string selectedDepartment = departments[selection - 1];

                // Get courses for selected department
                var courses = await _courseService.GetCoursesByDepartmentAsync(selectedDepartment);

                // Display results
                Console.WriteLine($"\n=== COURSES IN {selectedDepartment.ToUpper()} ===");

                // Show column headers for the course information display
                Console.WriteLine("ID\tCode\tTitle\tCredits");

                // Visual separator to improve readability of the output
                Console.WriteLine("----------------------------------------");

                // Iterate through each course and display its details in tabular format
                foreach (var course in courses)
                {
                    // Format each course on a single line with tab separation for column alignment
                    Console.WriteLine($"{course.Code}\t{course.Title}\t{course.Credits}");
                }

                // Display a summary with the total count of courses found in the department
                Console.WriteLine($"\nTotal Courses in {selectedDepartment}: {courses.Count()}");
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors that occur during command execution
                Console.WriteLine($"\nAn error occurred: {ex.Message}");

                // Log the error with detailed exception information for troubleshooting
                _logger.LogError(ex, "Error in FindCoursesByDepartmentCommand");
            }
        }
    }
}
