using CourseManager.CLI.Core.Exceptions;
using CourseManager.CLI.Core.Services;
using Microsoft.Extensions.Logging;

namespace CourseManager.CLI.ConsoleApp.Commands
{
    /// <summary>
    /// Command to view the details of a specific course
    /// </summary>
    public class ViewCourseCommand : CommandBase
    {
        private readonly ICourseService _courseService;
        private readonly IInstructorService _instructorService;

        public ViewCourseCommand(
            ICourseService courseService,
            IInstructorService instructorService,
            ILogger<ViewCourseCommand> logger) : base(logger)
        {
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
            _instructorService = instructorService ?? throw new ArgumentNullException(nameof(instructorService));
        }

        public override async Task ExecuteAsync()
        {
            try
            {
                Console.WriteLine("=== VIEW COURSE DETAILS ===");

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
                int selection = ReadInt("Enter course number to view details: ", 1, courses.Count());
                var selectedCourse = courses.ElementAt(selection - 1);

                // Get instructors assigned to this course
                var instructors = await _instructorService.GetInstructorsByCourseAsync(selectedCourse.Id);

                // Display course details
                Console.Clear();
                Console.WriteLine($"=== COURSE DETAILS: {selectedCourse.Code} ===");
                Console.WriteLine($"Title: {selectedCourse.Title}");
                Console.WriteLine($"Code: {selectedCourse.Code}");
                Console.WriteLine($"Department: {selectedCourse.Department}");
                Console.WriteLine($"Description: {selectedCourse.Description}");
                Console.WriteLine($"Credits: {selectedCourse.Credits}");
                Console.WriteLine($"Maximum Enrollment: {selectedCourse.MaxEnrollment}");
                Console.WriteLine($"Created Date: {selectedCourse.CreatedDate.ToShortDateString()}");

                // Display assigned instructors
                Console.WriteLine("\nAssigned Instructors:");
                if (instructors.Any())
                {
                    foreach (var instructor in instructors)
                    {
                        Console.WriteLine($"- {instructor.FirstName} {instructor.LastName} ({instructor.Email})");
                    }
                }
                else
                {
                    Console.WriteLine("No instructors assigned to this course.");
                }
            }
            catch (EntityNotFoundException ex)
            {
                // Handle the case where the course to view no longer exists
                Console.WriteLine($"\nError: {ex.Message}");
                _logger.LogWarning(ex, "Entity not found in ViewCourseCommand");
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors during the view process
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
                _logger.LogError(ex, "Error in ViewCourseCommand");
            }
        }
    }
}
