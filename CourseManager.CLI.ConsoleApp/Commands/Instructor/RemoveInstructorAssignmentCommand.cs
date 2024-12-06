using CourseManager.CLI.Core.Exceptions;
using CourseManager.CLI.Core.Services;
using CourseManager.CLI.Core.Models;
using Microsoft.Extensions.Logging;

namespace CourseManager.CLI.ConsoleApp.Commands
{
    /// <summary>
    /// Command to remove an instructor assignment from a course
    /// </summary>
    public class RemoveInstructorAssignmentCommand : CommandBase
    {
        private readonly IInstructorService _instructorService;
        private readonly ICourseService _courseService;

        public RemoveInstructorAssignmentCommand(
            IInstructorService instructorService,
            ICourseService courseService,
            ILogger<RemoveInstructorAssignmentCommand> logger) : base(logger)
        {
            _instructorService = instructorService ?? throw new ArgumentNullException(nameof(instructorService));
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
        }

        public override async Task ExecuteAsync()
        {
            try
            {
                Console.WriteLine("=== REMOVE INSTRUCTOR ASSIGNMENT ===");

                // Get all instructors
                var instructors = await _instructorService.GetAllInstructorsAsync();

                // Filter to only include instructors with assigned courses
                instructors = instructors.Where(i => i.CourseIds.Any()).ToList();

                if (!instructors.Any())
                {
                    Console.WriteLine("No instructors with course assignments found in the system.");
                    return;
                }

                // Display list of instructors with index for selection
                Console.WriteLine("\nSelect an Instructor:");
                Console.WriteLine("ID\tName\t\tNumber of Assignments");
                Console.WriteLine("----------------------------------------");

                int index = 1;
                foreach (var instructor in instructors)
                {
                    Console.WriteLine($"{index++}.\t{instructor.FirstName} {instructor.LastName}\t{instructor.CourseIds.Count}");
                }

                // Get user selection for instructor
                int instructorSelection = ReadInt("Enter instructor number: ", 1, instructors.Count());
                var selectedInstructor = instructors.ElementAt(instructorSelection - 1);

                // Get courses assigned to the selected instructor
                var assignedCourseIds = selectedInstructor.CourseIds;
                if (!assignedCourseIds.Any())
                {
                    Console.WriteLine($"Instructor '{selectedInstructor.FullName}' has no course assignments.");
                    return;
                }

                // Get details for each assigned course
                List<Course> assignedCourses = new List<Course>();
                foreach (var courseId in assignedCourseIds)
                {
                    try
                    {
                        var course = await _courseService.GetCourseByIdAsync(courseId);
                        assignedCourses.Add(course);
                    }
                    catch (EntityNotFoundException)
                    {
                        // Skip courses that might have been deleted
                        _logger.LogWarning("Course with ID {CourseId} not found but referenced by instructor", courseId);
                    }
                }

                // Display assigned courses
                Console.WriteLine($"\nCourses assigned to {selectedInstructor.FullName}:");
                Console.WriteLine("ID\tCode\tTitle\tDepartment");
                Console.WriteLine("----------------------------------------");

                index = 1;
                foreach (var course in assignedCourses)
                {
                    Console.WriteLine($"{index++}.\t{course.Code}\t{course.Title}\t{course.Department}");
                }

                // Get user selection for course
                int courseSelection = ReadInt("Enter course number to remove assignment: ", 1, assignedCourses.Count);
                var selectedCourse = assignedCourses[courseSelection - 1];

                // Confirm removal
                if (ReadYesNo($"Remove {selectedInstructor.FullName} from course {selectedCourse.Code}?"))
                {
                    await _instructorService.RemoveInstructorFromCourseAsync(selectedInstructor.Id, selectedCourse.Id);
                    Console.WriteLine("\nInstructor removed from course successfully!");
                    _logger.LogInformation("Instructor {InstructorName} removed from course {CourseCode}",
                        selectedInstructor.FullName, selectedCourse.Code);
                }
                else
                {
                    Console.WriteLine("\nRemoval cancelled.");
                }
            }
            catch (EntityNotFoundException ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                _logger.LogWarning(ex, "Entity not found in RemoveInstructorAssignmentCommand");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
                _logger.LogError(ex, "Error in RemoveInstructorAssignmentCommand");
            }
        }
    }
}
