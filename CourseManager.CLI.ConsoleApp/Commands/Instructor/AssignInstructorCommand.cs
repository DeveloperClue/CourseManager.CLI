using CourseManager.CLI.Core.Exceptions;
using CourseManager.CLI.Core.Services;
using Microsoft.Extensions.Logging;

namespace CourseManager.CLI.ConsoleApp.Commands
{
    /// <summary>
    /// Command to assign an instructor to a course
    /// </summary>
    /// <remarks>
    /// This command manages the many-to-many relationship between instructors and courses.
    /// It allows administrators to assign teaching responsibilities to faculty members
    /// while preventing duplicate assignments. The command guides users through:
    /// 1. Selecting an instructor from the faculty roster
    /// 2. Selecting a course from available courses 
    /// 3. Confirming and creating the assignment
    /// </remarks>
    public class AssignInstructorCommand : CommandBase
    {
        /// <summary>
        /// Service for instructor-related operations
        /// </summary>
        private readonly IInstructorService _instructorService;

        /// <summary>
        /// Service for course-related operations
        /// </summary>
        private readonly ICourseService _courseService;

        /// <summary>
        /// Initializes a new instance of the AssignInstructorCommand class
        /// </summary>
        /// <param name="instructorService">Service for instructor operations</param>
        /// <param name="courseService">Service for course operations</param>
        /// <param name="logger">Logger for recording command execution</param>
        /// <exception cref="ArgumentNullException">Thrown if any required service is null</exception>
        public AssignInstructorCommand(
            IInstructorService instructorService,
            ICourseService courseService,
            ILogger<AssignInstructorCommand> logger) : base(logger)
        {
            _instructorService = instructorService ?? throw new ArgumentNullException(nameof(instructorService));
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
        }

        /// <summary>
        /// Executes the command to assign an instructor to a course
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public override async Task ExecuteAsync()
        {
            try
            {
                // Display header for the command
                Console.WriteLine("=== ASSIGN INSTRUCTOR TO COURSE ===");

                // Step 1: Load and display instructors for selection
                // Retrieve all available instructors from the repository
                var instructors = await _instructorService.GetAllInstructorsAsync();
                if (!instructors.Any())
                {
                    // Early exit if no instructors are available
                    Console.WriteLine("No instructors found in the system.");
                    return;
                }                // Display list of instructors with index for selection
                // This tabular format makes it easier for users to identify instructors
                Console.WriteLine("\nSelect an Instructor:");
                Console.WriteLine("ID\tName\t\tDepartment");
                Console.WriteLine("----------------------------------------");

                // Number each instructor with a 1-based index for selection
                int index = 1;
                foreach (var instructor in instructors)
                {
                    Console.WriteLine($"{index++}.\t{instructor.FirstName} {instructor.LastName}\t{instructor.Department}");
                }

                // Get user selection for instructor using the displayed index
                int instructorSelection = ReadInt("Enter instructor number: ", 1, instructors.Count());
                var selectedInstructor = instructors.ElementAt(instructorSelection - 1);

                // Step 2: Load and display courses for selection
                // Retrieve all available courses from the repository
                var courses = await _courseService.GetAllCoursesAsync();
                if (!courses.Any())
                {
                    // Early exit if no courses are available
                    Console.WriteLine("No courses found in the system.");
                    return;
                }                // Display list of courses with index for selection
                // Highlight courses that are already assigned to the instructor
                Console.WriteLine("\nSelect a Course:");
                Console.WriteLine("ID\tCode\tTitle\tDepartment");
                Console.WriteLine("----------------------------------------");

                // Reset the index for the course listing
                index = 1;
                foreach (var course in courses)
                {
                    // Check if instructor is already assigned to this course and mark it accordingly
                    // This helps prevent duplicate assignments and provides visual feedback
                    bool isAssigned = selectedInstructor.CourseIds.Contains(course.Id);
                    Console.WriteLine($"{index++}.\t{course.Code}\t{course.Title}\t{course.Department}" +
                                     (isAssigned ? " (already assigned)" : ""));
                }

                // Get user selection for course using the displayed index
                int courseSelection = ReadInt("Enter course number: ", 1, courses.Count());
                var selectedCourse = courses.ElementAt(courseSelection - 1);

                // Step 3: Validate the assignment
                // Check if the instructor is already assigned to the selected course
                if (selectedInstructor.CourseIds.Contains(selectedCourse.Id))
                {
                    // Inform the user and exit if it's a duplicate assignment
                    Console.WriteLine($"\nInstructor '{selectedInstructor.FullName}' is already assigned to course '{selectedCourse.Code}'.");
                    return;
                }                // Step 4: Confirm and create the assignment
                // Ask for confirmation before making the assignment
                if (ReadYesNo($"Assign {selectedInstructor.FullName} to {selectedCourse.Code}?"))
                {
                    // Perform the actual assignment operation using the service
                    await _instructorService.AssignInstructorToCourseAsync(selectedInstructor.Id, selectedCourse.Id);

                    // Provide feedback on successful assignment
                    Console.WriteLine("\nInstructor assigned to course successfully!");

                    // Log the successful assignment for audit purposes
                    _logger.LogInformation("Instructor {InstructorName} assigned to course {CourseCode}",
                        selectedInstructor.FullName, selectedCourse.Code);
                }
                else
                {
                    // Respect the user's decision to cancel
                    Console.WriteLine("\nAssignment cancelled.");
                }
            }
            catch (EntityNotFoundException ex)
            {
                // Handle the specific case where an entity (instructor or course) is not found
                // This might happen if an entity was deleted by another user during the operation
                Console.WriteLine($"\nError: {ex.Message}");
                _logger.LogWarning(ex, "Entity not found in AssignInstructorCommand");
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors during command execution
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
                _logger.LogError(ex, "Error in AssignInstructorCommand");
            }
        }
    }
}
