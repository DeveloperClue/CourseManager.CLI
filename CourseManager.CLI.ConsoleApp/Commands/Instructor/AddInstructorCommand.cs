using CourseManager.CLI.Core.Exceptions;
using CourseManager.CLI.Core.Services;
using CourseManager.CLI.Core.Models;
using Microsoft.Extensions.Logging;

namespace CourseManager.CLI.ConsoleApp.Commands
{
    /// <summary>
    /// Command to add a new instructor to the system
    /// </summary>
    /// <remarks>
    /// This command guides the user through entering all required information for a new instructor,
    /// validates the input, and saves the new instructor to the repository.
    /// </remarks>
    public class AddInstructorCommand : CommandBase
    {
        /// <summary>
        /// Service responsible for instructor-related operations
        /// </summary>
        private readonly IInstructorService _instructorService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddInstructorCommand"/> class
        /// </summary>
        /// <param name="instructorService">The instructor service for database operations</param>
        /// <param name="logger">The logger for recording application events</param>
        /// <exception cref="ArgumentNullException">Thrown when instructorService is null</exception>
        public AddInstructorCommand(
            IInstructorService instructorService,
            ILogger<AddInstructorCommand> logger) : base(logger)
        {
            _instructorService = instructorService ?? throw new ArgumentNullException(nameof(instructorService));
        }

        /// <summary>
        /// Executes the add instructor command by collecting instructor details from the user and saving to the database
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public override async Task ExecuteAsync()
        {
            try
            {
                Console.WriteLine("=== ADD NEW INSTRUCTOR ===");

                // Collect instructor information from user input
                var instructor = new Instructor
                {
                    Id = Guid.NewGuid(), // Generate a new unique ID for this instructor
                    FirstName = ReadString("Enter first name: ", false),
                    LastName = ReadString("Enter last name: ", false),
                    Email = ReadString("Enter email address: ", false),
                    Department = ReadString("Enter department: ", false),
                    OfficeLocation = ReadString("Enter office location: ", true) ?? string.Empty,
                    Phone = ReadString("Enter phone number: ", true) ?? string.Empty,
                    IsActive = ReadYesNo("Is this instructor active?"),
                    HireDate = ReadDate("Enter hire date (MM/DD/YYYY): ", DateTime.Now),
                    CreatedDate = DateTime.Now // Set creation timestamp to current time
                };

                // Display summary for user confirmation
                Console.WriteLine("\nInstructor Details:");
                Console.WriteLine($"Name: {instructor.FirstName} {instructor.LastName}");
                Console.WriteLine($"Email: {instructor.Email}");
                Console.WriteLine($"Department: {instructor.Department}");
                Console.WriteLine($"Office Location: {instructor.OfficeLocation}");
                Console.WriteLine($"Phone: {instructor.Phone}");
                Console.WriteLine($"Status: {(instructor.IsActive ? "Active" : "Inactive")}");
                Console.WriteLine($"Hire Date: {instructor.HireDate.ToShortDateString()}");

                // Confirm and save if user approves
                if (ReadYesNo("Save this instructor?"))
                {
                    // Save the instructor to the database using the instructor service
                    await _instructorService.AddInstructorAsync(instructor);
                    Console.WriteLine("\nInstructor added successfully!");
                    _logger.LogInformation("Added new instructor: {FirstName} {LastName} ({Email})",
                        instructor.FirstName, instructor.LastName, instructor.Email);
                }
                else
                {
                    // User chose to cancel the operation
                    Console.WriteLine("\nInstructor creation cancelled.");
                }
            }
            catch (ValidationException ex)
            {
                // Handle validation errors (like invalid email format)
                Console.WriteLine($"\nValidation error: {ex.Message}");
                _logger.LogWarning(ex, "Validation error in AddInstructorCommand");
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
                _logger.LogError(ex, "Error in AddInstructorCommand");
            }
        }

        /// <summary>
        /// Reads a date from the console with validation and default value option
        /// </summary>
        /// <param name="prompt">The prompt to display to the user</param>
        /// <param name="defaultDate">The default date to use if user enters nothing</param>
        /// <returns>The entered date or the default date if input was empty</returns>
        private DateTime ReadDate(string prompt, DateTime defaultDate)
        {
            while (true)
            {
                // Get date input from user, allowing empty input
                string? input = ReadString(prompt, true);

                // Return the default date if input is empty
                if (string.IsNullOrWhiteSpace(input))
                {
                    return defaultDate;
                }

                // Try to parse the input as a date
                if (DateTime.TryParse(input, out DateTime result))
                {
                    return result;
                }

                // If parsing fails, inform the user and prompt again
                Console.WriteLine("Please enter a valid date in MM/DD/YYYY format.");
            }
        }
    }
}
