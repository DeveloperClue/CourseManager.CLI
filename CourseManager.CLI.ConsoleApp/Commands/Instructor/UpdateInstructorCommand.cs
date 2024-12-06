using CourseManager.CLI.Core.Exceptions;
using CourseManager.CLI.Core.Services;
using CourseManager.CLI.Core.Models;
using Microsoft.Extensions.Logging;

namespace CourseManager.CLI.ConsoleApp.Commands
{
    /// <summary>
    /// Command to update an existing instructor
    /// </summary>
    public class UpdateInstructorCommand : CommandBase
    {
        private readonly IInstructorService _instructorService;

        public UpdateInstructorCommand(
            IInstructorService instructorService,
            ILogger<UpdateInstructorCommand> logger) : base(logger)
        {
            _instructorService = instructorService ?? throw new ArgumentNullException(nameof(instructorService));
        }

        public override async Task ExecuteAsync()
        {
            try
            {
                Console.WriteLine("=== UPDATE INSTRUCTOR ===");

                // Get all instructors first to display a list
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

                int index = 1;
                foreach (var instructor in instructors)
                {
                    Console.WriteLine($"{index++}.\t{instructor.FirstName} {instructor.LastName}\t{instructor.Email}");
                }

                // Get user selection
                int selection = ReadInt("Enter instructor number to update: ", 1, instructors.Count());
                var selectedInstructor = instructors.ElementAt(selection - 1);

                Console.WriteLine($"\nUpdating instructor: {selectedInstructor.FirstName} {selectedInstructor.LastName}");
                Console.WriteLine("(Press Enter to keep current values)");

                // Get updated information
                var updatedInstructor = new Instructor
                {
                    Id = selectedInstructor.Id,
                    FirstName = ReadStringWithDefault($"First Name [{selectedInstructor.FirstName}]: ", selectedInstructor.FirstName),
                    LastName = ReadStringWithDefault($"Last Name [{selectedInstructor.LastName}]: ", selectedInstructor.LastName),
                    Email = ReadStringWithDefault($"Email [{selectedInstructor.Email}]: ", selectedInstructor.Email),
                    Department = ReadStringWithDefault($"Department [{selectedInstructor.Department}]: ", selectedInstructor.Department),
                    OfficeLocation = ReadStringWithDefault($"Office Location [{selectedInstructor.OfficeLocation}]: ", selectedInstructor.OfficeLocation),
                    Phone = ReadStringWithDefault($"Phone [{selectedInstructor.Phone}]: ", selectedInstructor.Phone),
                    IsActive = ReadYesNo($"Is Active [{(selectedInstructor.IsActive ? "Yes" : "No")}]? "),
                    HireDate = ReadDateOrDefault($"Hire Date [{selectedInstructor.HireDate:MM/dd/yyyy}]: ", selectedInstructor.HireDate),
                    CreatedDate = selectedInstructor.CreatedDate,
                    CourseIds = selectedInstructor.CourseIds, // Maintain course assignments
                    Title = ReadStringWithDefault($"Title [{selectedInstructor.Title}]: ", selectedInstructor.Title)
                };

                // Confirm update
                Console.WriteLine("\nUpdated Instructor Details:");
                Console.WriteLine($"Name: {updatedInstructor.FirstName} {updatedInstructor.LastName}");
                Console.WriteLine($"Email: {updatedInstructor.Email}");
                Console.WriteLine($"Department: {updatedInstructor.Department}");
                Console.WriteLine($"Office Location: {updatedInstructor.OfficeLocation}");
                Console.WriteLine($"Phone: {updatedInstructor.Phone}");
                Console.WriteLine($"Active: {(updatedInstructor.IsActive ? "Yes" : "No")}");
                Console.WriteLine($"Hire Date: {updatedInstructor.HireDate.ToShortDateString()}");
                Console.WriteLine($"Title: {updatedInstructor.Title}");

                if (ReadYesNo("Save these changes?"))
                {
                    await _instructorService.UpdateInstructorAsync(updatedInstructor);
                    Console.WriteLine("\nInstructor updated successfully!");
                    _logger.LogInformation("Instructor updated: {Name}", updatedInstructor.FullName);
                }
                else
                {
                    Console.WriteLine("\nUpdate cancelled.");
                }
            }
            catch (EntityNotFoundException ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                _logger.LogWarning(ex, "Entity not found in UpdateInstructorCommand");
            }
            catch (ValidationException ex)
            {
                Console.WriteLine($"\nValidation error: {ex.Message}");
                _logger.LogWarning(ex, "Validation error in UpdateInstructorCommand");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
                _logger.LogError(ex, "Error in UpdateInstructorCommand");
            }
        }

        private DateTime ReadDateOrDefault(string prompt, DateTime defaultValue)
        {
            string? input = ReadString(prompt, true);
            if (string.IsNullOrWhiteSpace(input))
            {
                return defaultValue;
            }

            if (DateTime.TryParse(input, out DateTime result))
            {
                return result;
            }

            Console.WriteLine("Please enter a valid date in MM/DD/YYYY format.");
            return ReadDateOrDefault(prompt, defaultValue);
        }
    }
}
