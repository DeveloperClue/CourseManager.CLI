using CourseManager.CLI.Core.Exceptions;
using CourseManager.CLI.Core.Services;
using Microsoft.Extensions.Logging;

namespace CourseManager.CLI.ConsoleApp.Commands
{
    /// <summary>
    /// Command to view a specific instructor's details including personal information and assigned courses
    /// </summary>
    /// <remarks>
    /// This command provides detailed information about an instructor selected by the user from a list,
    /// including contact information, department affiliation, and a list of courses they teach.
    /// </remarks>
    public class ViewInstructorCommand : CommandBase
    {
        /// <summary>
        /// Service responsible for instructor-related operations
        /// </summary>
        private readonly IInstructorService _instructorService;

        /// <summary>
        /// Service responsible for course-related operations
        /// </summary>
        private readonly ICourseService _courseService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewInstructorCommand"/> class
        /// </summary>
        /// <param name="instructorService">The instructor service for retrieving instructor information</param>
        /// <param name="courseService">The course service for retrieving courses taught by the instructor</param>
        /// <param name="logger">The logger for recording application events</param>
        /// <exception cref="ArgumentNullException">Thrown when any service parameter is null</exception>
        public ViewInstructorCommand(
            IInstructorService instructorService,
            ICourseService courseService,
            ILogger<ViewInstructorCommand> logger) : base(logger)
        {
            _instructorService = instructorService ?? throw new ArgumentNullException(nameof(instructorService));
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
        }

        /// <summary>
        /// Executes the view instructor command by displaying detailed information about a selected instructor
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public override async Task ExecuteAsync()
        {
            try
            {
                Console.WriteLine("=== VIEW INSTRUCTOR DETAILS ===");

                // Get all instructors first to display a list for selection
                var instructors = await _instructorService.GetAllInstructorsAsync();
                if (!instructors.Any())
                {
                    Console.WriteLine("No instructors found in the system.");
                    return;
                }

                // Display list of instructors with index for selection
                Console.WriteLine("\nAvailable Instructors:");
                Console.WriteLine("ID\tName\t\tEmail");
                Console.WriteLine("----------------------------------------");

                // List all instructors with a sequential number for user selection
                int index = 1;
                foreach (var instructor in instructors)
                {
                    Console.WriteLine($"{index++}.\t{instructor.FirstName} {instructor.LastName}\t{instructor.Email}");
                }

                // Get user's instructor selection (1-based index)
                int selection = ReadInt("Enter instructor number to view details: ", 1, instructors.Count());
                var selectedInstructor = instructors.ElementAt(selection - 1);

                // Get courses taught by this instructor for displaying related information
                var courses = await _courseService.GetCoursesByInstructorAsync(selectedInstructor.Id);

                // Clear screen and display detailed instructor information
                Console.Clear();
                Console.WriteLine($"=== INSTRUCTOR DETAILS: {selectedInstructor.FirstName} {selectedInstructor.LastName} ===");
                Console.WriteLine($"ID: {selectedInstructor.Id}");
                Console.WriteLine($"Name: {selectedInstructor.FirstName} {selectedInstructor.LastName}");
                Console.WriteLine($"Email: {selectedInstructor.Email}");
                Console.WriteLine($"Department: {selectedInstructor.Department}");
                Console.WriteLine($"Office Location: {selectedInstructor.OfficeLocation}");
                Console.WriteLine($"Phone: {selectedInstructor.Phone}");
                Console.WriteLine($"Status: {(selectedInstructor.IsActive ? "Active" : "Inactive")}");
                Console.WriteLine($"Hire Date: {selectedInstructor.HireDate.ToShortDateString()}");

                // Display courses taught by this instructor
                Console.WriteLine("\nAssigned Courses:");
                if (courses.Any())
                {
                    foreach (var course in courses)
                    {
                        Console.WriteLine($"- {course.Code}: {course.Title}");
                    }
                }
                else
                {
                    Console.WriteLine("No courses assigned to this instructor.");
                }
            }
            catch (EntityNotFoundException ex)
            {
                // Handle case where an instructor or course couldn't be found
                Console.WriteLine($"\nError: {ex.Message}");
                _logger.LogWarning(ex, "Entity not found in ViewInstructorCommand");
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
                _logger.LogError(ex, "Error in ViewInstructorCommand");
            }
        }
    }
}
