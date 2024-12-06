using CourseManager.CLI.Core.Services;
using Microsoft.Extensions.Logging;

namespace CourseManager.CLI.ConsoleApp.Commands
{
    /// <summary>
    /// Command to find instructors filtered by department
    /// </summary>
    public class FindInstructorsByDepartmentCommand : CommandBase
    {
        private readonly IInstructorService _instructorService;

        public FindInstructorsByDepartmentCommand(
            IInstructorService instructorService,
            ILogger<FindInstructorsByDepartmentCommand> logger) : base(logger)
        {
            _instructorService = instructorService ?? throw new ArgumentNullException(nameof(instructorService));
        }

        public override async Task ExecuteAsync()
        {
            try
            {
                Console.WriteLine("=== FIND INSTRUCTORS BY DEPARTMENT ===");

                // Get all instructors to extract available departments
                var allInstructors = await _instructorService.GetAllInstructorsAsync();
                if (!allInstructors.Any())
                {
                    Console.WriteLine("No instructors found in the system.");
                    return;
                }

                // Extract unique departments
                var departments = allInstructors.Select(i => i.Department).Distinct().OrderBy(d => d).ToList();

                // Display available departments
                Console.WriteLine("\nAvailable Departments:");
                for (int i = 0; i < departments.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {departments[i]}");
                }

                // Get user selection
                int selection = ReadInt("Enter department number to view instructors: ", 1, departments.Count);
                string selectedDepartment = departments[selection - 1];

                // Get instructors for selected department
                var instructors = await _instructorService.GetInstructorsByDepartmentAsync(selectedDepartment);

                // Display results
                Console.WriteLine($"\n=== INSTRUCTORS IN {selectedDepartment.ToUpper()} ===");
                Console.WriteLine("Name\t\t\tEmail\t\t\tStatus\t\tTitle");
                Console.WriteLine("----------------------------------------------------------------");

                foreach (var instructor in instructors)
                {
                    Console.WriteLine($"{instructor.FirstName} {instructor.LastName}\t{instructor.Email}\t" +
                                      $"{(instructor.IsActive ? "Active" : "Inactive")}\t\t{instructor.Title}");
                }

                Console.WriteLine($"\nTotal Instructors in {selectedDepartment}: {instructors.Count()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
                _logger.LogError(ex, "Error in FindInstructorsByDepartmentCommand");
            }
        }
    }
}
