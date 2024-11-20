using CourseManager.CLI.Core.Exceptions;
using CourseManager.CLI.Core.Services;
using Microsoft.Extensions.Logging;

namespace CourseManager.CLI.ConsoleApp.Commands
{
    /// <summary>
    /// Command to delete an existing course from the system
    /// </summary>
    public class DeleteCourseCommand : CommandBase
    {
        /// <summary>
        /// Service responsible for course-related operations
        /// </summary>
        private readonly ICourseService _courseService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteCourseCommand"/> class
        /// </summary>
        /// <param name="courseService">The course service for database operations</param>
        /// <param name="logger">The logger for recording application events</param>
        /// <exception cref="ArgumentNullException">Thrown when courseService is null</exception>
        public DeleteCourseCommand(
            ICourseService courseService,
            ILogger<DeleteCourseCommand> logger) : base(logger)
        {
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
        }

        /// <summary>
        /// Executes the delete course command by guiding the user through the deletion process
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public override async Task ExecuteAsync()
        {
            try
            {
                Console.WriteLine("=== DELETE COURSE ===");

                // Get all courses first to display a list
                var courses = await _courseService.GetAllCoursesAsync();
                if (!courses.Any())
                {
                    Console.WriteLine("No courses found in the system.");
                    return;
                }

                // Display list of courses with index for selection
                Console.WriteLine("\nAvailable Courses:");
                Console.WriteLine("ID\tCode\tTitle");
                Console.WriteLine("----------------------------------------");

                int index = 1;
                foreach (var course in courses)
                {
                    Console.WriteLine($"{index++}.\t{course.Code}\t{course.Title}");
                }

                // Get user selection
                int selection = ReadInt("Enter course number to delete: ", 1, courses.Count());
                var selectedCourse = courses.ElementAt(selection - 1);

                // Confirm deletion
                Console.WriteLine($"\nYou are about to delete: {selectedCourse.Code} - {selectedCourse.Title}");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("This action cannot be undone!");
                Console.ResetColor();

                if (ReadYesNo("Are you sure you want to delete this course?"))
                {
                    // Delete the course
                    await _courseService.DeleteCourseAsync(selectedCourse.Id);

                    // Provide feedback to the user about successful deletion
                    Console.WriteLine("\nCourse deleted successfully!");

                    // Log the deletion action with relevant information for auditing purposes
                    _logger.LogInformation("Course deleted: {CourseCode}", selectedCourse.Code);
                }
                else
                {
                    // User chose not to proceed with deletion
                    Console.WriteLine("\nDeletion cancelled.");
                }
            }
            catch (EntityNotFoundException ex)
            {
                // Handle the case when the course to delete doesn't exist (possibly deleted by another user)
                Console.WriteLine($"\nError: {ex.Message}");
                _logger.LogWarning(ex, "Entity not found in DeleteCourseCommand");
            }
            catch (Exception ex)
            {
                // Handle unexpected errors during the deletion process
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
                _logger.LogError(ex, "Error in DeleteCourseCommand");
            }
        }
    }
}
