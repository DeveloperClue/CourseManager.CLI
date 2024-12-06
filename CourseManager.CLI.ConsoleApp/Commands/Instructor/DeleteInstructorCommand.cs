using CourseManager.CLI.Core.Exceptions;
using CourseManager.CLI.Core.Services;
using Microsoft.Extensions.Logging;

namespace CourseManager.CLI.ConsoleApp.Commands
{
    /// <summary>
    /// Command to delete an existing instructor
    /// </summary>
    public class DeleteInstructorCommand : CommandBase
    {
        private readonly IInstructorService _instructorService;

        public DeleteInstructorCommand(
            IInstructorService instructorService,
            ILogger<DeleteInstructorCommand> logger) : base(logger)
        {
            _instructorService = instructorService ?? throw new ArgumentNullException(nameof(instructorService));
        }

        public override async Task ExecuteAsync()
        {
            try
            {
                Console.WriteLine("=== DELETE INSTRUCTOR ===");

                // Get all instructors first to display a list
                var instructors = await _instructorService.GetAllInstructorsAsync();
                if (!instructors.Any())
                {
                    Console.WriteLine("No instructors found in the system.");
                    return;
                }

                // Display list of instructors with index for selection
                Console.WriteLine("\nAvailable Instructors:");
                Console.WriteLine("ID\tName\t\tDepartment");
                Console.WriteLine("----------------------------------------");

                int index = 1;
                foreach (var instructor in instructors)
                {
                    Console.WriteLine($"{index++}.\t{instructor.FirstName} {instructor.LastName}\t{instructor.Department}");
                }

                // Get user selection
                int selection = ReadInt("Enter instructor number to delete: ", 1, instructors.Count());
                var selectedInstructor = instructors.ElementAt(selection - 1);

               

                // Check if instructor has assigned courses
                bool hasAssignedCourses = selectedInstructor.CourseIds.Any();

                // Warn about dependencies
                if (hasAssignedCourses)
                {
                    Console.WriteLine("\nWARNING: This instructor has dependencies in the system:");
                    if (hasAssignedCourses)
                        Console.WriteLine($"- {selectedInstructor.CourseIds.Count} course assignments");
                    Console.WriteLine("Deleting this instructor will affect these relationships.");
                }

                // Confirm deletion
                Console.WriteLine($"\nYou are about to delete: {selectedInstructor.FirstName} {selectedInstructor.LastName}");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("This action cannot be undone!");
                Console.ResetColor();

                if (ReadYesNo("Are you sure you want to delete this instructor?"))
                {
                    // Delete the instructor
                    await _instructorService.DeleteInstructorAsync(selectedInstructor.Id);
                    Console.WriteLine("\nInstructor deleted successfully!");
                    _logger.LogInformation("Instructor deleted: {Name}",
                        $"{selectedInstructor.FirstName} {selectedInstructor.LastName}");
                }
                else
                {
                    Console.WriteLine("\nDeletion cancelled.");
                }
            }
            catch (EntityNotFoundException ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                _logger.LogWarning(ex, "Entity not found in DeleteInstructorCommand");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
                _logger.LogError(ex, "Error in DeleteInstructorCommand");
            }
        }
    }
}
