using CourseManager.CLI.Core.Exceptions;
using CourseManager.CLI.Core.Services;
using CourseManager.CLI.Core.Models;
using Microsoft.Extensions.Logging;

namespace CourseManager.CLI.ConsoleApp.Commands
{
    /// <summary>
    /// Command to update an existing course in the system
    /// </summary>
    public class UpdateCourseCommand : CommandBase
    {
        /// <summary>
        /// Service responsible for course-related operations
        /// </summary>
        private readonly ICourseService _courseService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateCourseCommand"/> class
        /// </summary>
        /// <param name="courseService">The course service for database operations</param>
        /// <param name="logger">The logger for recording application events</param>
        /// <exception cref="ArgumentNullException">Thrown when courseService is null</exception>
        public UpdateCourseCommand(
            ICourseService courseService,
            ILogger<UpdateCourseCommand> logger) : base(logger)
        {
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
        }

        /// <summary>
        /// Executes the update course command by guiding the user through selecting and modifying course details
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public override async Task ExecuteAsync()
        {
            try
            {
                Console.WriteLine("=== UPDATE COURSE ===");

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
                int selection = ReadInt("Enter course number to update: ", 1, courses.Count());
                var selectedCourse = courses.ElementAt(selection - 1);

                Console.WriteLine($"\nUpdating course: {selectedCourse.Code} - {selectedCourse.Title}");
                Console.WriteLine("(Press Enter to keep current values)");

                // Get updated information
                var updatedCourse = new Course
                {
                    Id = selectedCourse.Id,
                    Code = selectedCourse.Code, // Code cannot be changed
                    Title = ReadStringWithDefault($"Title [{selectedCourse.Title}]: ", selectedCourse.Title),
                    Description = ReadStringWithDefault($"Description [{selectedCourse.Description}]: ", selectedCourse.Description),
                    Department = ReadStringWithDefault($"Department [{selectedCourse.Department}]: ", selectedCourse.Department),
                    Credits = ReadIntOrDefault($"Credits [{selectedCourse.Credits}]: ", 1, 12, selectedCourse.Credits),
                    MaxEnrollment = ReadIntOrDefault($"Maximum Enrollment [{selectedCourse.MaxEnrollment}]: ", 1, 500, selectedCourse.MaxEnrollment),
                    // Preserve the original creation date when updating
                    CreatedDate = selectedCourse.CreatedDate
                };

                //ReadStringWithDefault($"Semester [{selectedSchedule.Semester}]: ", selectedSchedule.Semester);

                // Display a summary of the updated course information for user verification
                Console.WriteLine("\nUpdated Course Details:");
                Console.WriteLine($"Code: {updatedCourse.Code}");
                Console.WriteLine($"Title: {updatedCourse.Title}");
                Console.WriteLine($"Department: {updatedCourse.Department}");
                Console.WriteLine($"Description: {updatedCourse.Description}");
                Console.WriteLine($"Credits: {updatedCourse.Credits}");
                Console.WriteLine($"Maximum Enrollment: {updatedCourse.MaxEnrollment}");

                // Confirm with the user before saving changes to the database
                if (ReadYesNo("Save these changes?"))
                {
                    // Perform the update operation through the service layer
                    await _courseService.UpdateCourseAsync(updatedCourse);

                    // Provide feedback about successful operation
                    Console.WriteLine("\nCourse updated successfully!");

                    // Log the update action for audit purposes
                    _logger.LogInformation("Course updated: {CourseCode}", updatedCourse.Code);
                }
                else
                {
                    // User chose not to proceed with the update
                    Console.WriteLine("\nUpdate cancelled.");
                }
            }
            catch (EntityNotFoundException ex)
            {
                // Handle the case where the course to update no longer exists
                Console.WriteLine($"\nError: {ex.Message}");
                _logger.LogWarning(ex, "Entity not found in UpdateCourseCommand");
            }
            catch (ValidationException ex)
            {
                // Handle validation errors (e.g., invalid credit value)
                Console.WriteLine($"\nValidation error: {ex.Message}");
                _logger.LogWarning(ex, "Validation error in UpdateCourseCommand");
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors during the update process
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
                _logger.LogError(ex, "Error in UpdateCourseCommand");
            }
        }
    }
}
