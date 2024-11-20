using CourseManager.CLI.Core.Exceptions;
using CourseManager.CLI.Core.Models;
using CourseManager.CLI.Core.Services;
using Microsoft.Extensions.Logging;

namespace CourseManager.CLI.ConsoleApp.Commands
{
    /// <summary>
    /// Command to add a new course
    /// </summary>
    public class AddCourseCommand : CommandBase
    {
        /// <summary>
        /// Service responsible for course operations
        /// </summary>
        private readonly ICourseService _courseService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddCourseCommand"/> class.
        /// </summary>
        /// <param name="courseService">The course service for database operations</param>
        /// <param name="logger">The logger for recording application events</param>
        /// <exception cref="ArgumentNullException">Thrown if courseService is null</exception>
        public AddCourseCommand(ICourseService courseService, ILogger<AddCourseCommand> logger)
            : base(logger)
        {
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
        }

        /// <summary>
        /// Executes the add course command by collecting course details from the user and saving to the database
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public override async Task ExecuteAsync()
        {
            Console.WriteLine("\n=== ADD NEW COURSE ===\n"); try
            {
                // Get course details from user by prompting for each required field
                // Creating a new Course object with properties initialized from user input
                var course = new Course
                {
                    // Course code follows a specific format (e.g., CS101, MATH200)
                    Code = ReadString("Enter course code (e.g., CS101): "),

                    // Course title is a required field
                    Title = ReadString("Enter course title: "),

                    // Description is optional, so we allow empty strings
                    Description = ReadString("Enter course description: ", true),

                    // Department is used for grouping and reporting
                    Department = ReadString("Enter department: "),

                    // Credits must be between 1-12 as per academic policy
                    Credits = ReadInt("Enter credits: ", 1, 12),

                    // MaxEnrollment has a reasonable upper limit of 300
                    MaxEnrollment = ReadInt("Enter maximum enrollment: ", 1, 300)
                };

                // Confirm with user before saving to database
                Console.WriteLine("\nCourse Details:");
                Console.WriteLine($"Code: {course.Code}");
                Console.WriteLine($"Title: {course.Title}");
                Console.WriteLine($"Department: {course.Department}");
                Console.WriteLine($"Credits: {course.Credits}");
                Console.WriteLine($"Maximum Enrollment: {course.MaxEnrollment}");

                if (ReadBoolean("\nSave this course?"))
                {
                    // Save the course to the database using the course service
                    var addedCourse = await _courseService.AddCourseAsync(course);

                    // Inform the user of successful operation
                    Console.WriteLine($"\nCourse added successfully with ID: {addedCourse.Id}");

                    // Log the successful course addition
                    _logger.LogInformation("Course added: {CourseCode} - {CourseTitle}",
                        addedCourse.Code, addedCourse.Title);
                }
                else
                {
                    // User chose to cancel the operation
                    Console.WriteLine("\nCourse addition cancelled.");
                }
            }
            catch (ValidationException ex)
            {
                // Handle validation errors (like invalid course code format)
                Console.WriteLine($"\nValidation error: {ex.Message}");
                _logger.LogWarning(ex, "Validation error in AddCourseCommand");
            }
            catch (Exception ex)
            {
                // Handle unexpected errors (database connection issues, etc.)
                Console.WriteLine($"\nError adding course: {ex.Message}");
                _logger.LogError(ex, "Error in AddCourseCommand");
            }
        }
    }
}
