using Microsoft.Extensions.Logging;

namespace CourseManager.CLI.ConsoleApp.Commands
{
    /// <summary>
    /// Base class for commands that provides common functionality for user interaction and input validation
    /// </summary>
    /// <remarks>
    /// This abstract class implements the Command pattern and provides helper methods 
    /// for reading and validating user input from the console. All concrete command classes
    /// should inherit from this base class to leverage the common functionality.
    /// </remarks>
    public abstract class CommandBase : ICommand
    {
        /// <summary>
        /// Logger for recording command execution details and errors
        /// </summary>
        protected readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CommandBase class with the specified logger
        /// </summary>
        /// <param name="logger">The logger instance for this command</param>
        /// <exception cref="ArgumentNullException">Thrown if logger is null</exception>
        protected CommandBase(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes the command's business logic asynchronously
        /// </summary>
        /// <returns>A task representing the asynchronous command execution</returns>
        /// <remarks>
        /// This is the main entry point for executing a command's functionality.
        /// Each derived command class must implement this method with its specific behavior.
        /// </remarks>
        public abstract Task ExecuteAsync();

        /// <summary>
        /// Reads a string from the console with the given prompt
        /// </summary>
        protected string ReadString(string prompt, bool allowEmpty = false)
        {
            while (true)
            {
                Console.Write(prompt);
                var value = Console.ReadLine() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(value) && !allowEmpty)
                {
                    Console.WriteLine("Value cannot be empty. Please try again.");
                    continue;
                }

                return value;
            }
        }

        /// <summary>
        /// Reads a string input from the user with a default value if the input is empty
        /// </summary>
        /// <param name="prompt">The message to display to the user</param>
        /// <param name="defaultValue">The default value to use if input is empty</param>
        /// <returns>The parsed string or the default value</returns>        
        protected string ReadStringWithDefault(string prompt, string defaultValue)
        {
            string? input = ReadString(prompt, true);
            return string.IsNullOrWhiteSpace(input) ? defaultValue : input;
        }

        /// <summary>
        /// Reads an integer from the console with the given prompt
        /// </summary>
        /// <param name="prompt">The message to display to the user</param>
        /// <param name="minValue">The minimum acceptable value</param>
        /// <param name="maxValue">The maximum acceptable value</param>
        /// <returns>A valid integer within the specified range</returns>
        protected int ReadInt(string prompt, int minValue = int.MinValue, int maxValue = int.MaxValue)
        {
            // Continue prompting until valid input is received
            while (true)
            {
                // Display the prompt to the user
                Console.Write(prompt);

                // Get the user's input
                var input = Console.ReadLine();

                // Attempt to parse the input as an integer and validate against the range
                if (int.TryParse(input, out var value) && value >= minValue && value <= maxValue)
                {
                    // Return the valid input
                    return value;
                }

                // Display an error message and loop to prompt again
                Console.WriteLine($"Please enter a valid number between {minValue} and {maxValue}.");
            }
        }


        /// <summary>
        /// Reads an integer input from the user with a default value if the input is empty
        /// </summary>
        /// <param name="prompt">The message to display to the user</param>
        /// <param name="min">The minimum acceptable value</param>
        /// <param name="max">The maximum acceptable value</param>
        /// <param name="defaultValue">The default value to use if input is empty</param>
        /// <returns>The parsed integer or the default value</returns>
        protected int ReadIntOrDefault(string prompt, int min, int max, int defaultValue)
        {
            string? input = ReadString(prompt, true);
            if (string.IsNullOrWhiteSpace(input))
            {
                return defaultValue;
            }

            if (int.TryParse(input, out int result) && result >= min && result <= max)
            {
                return result;
            }

            Console.WriteLine($"Please enter a valid number between {min} and {max}.");
            return ReadIntOrDefault(prompt, min, max, defaultValue);
        }

        /// <summary>
        /// Reads a GUID from the console with the given prompt
        /// </summary>
        /// <param name="prompt">The message to display to the user</param>
        /// <returns>A valid GUID entered by the user</returns>
        /// <remarks>
        /// This method repeatedly prompts the user until a valid GUID format is entered.
        /// It's typically used for selecting entities by their ID in list-based commands.
        /// </remarks>
        protected Guid ReadGuid(string prompt)
        {
            // Continue prompting until valid input is received
            while (true)
            {
                // Display the prompt to the user
                Console.Write(prompt);

                // Get the user's input
                var input = Console.ReadLine();

                // Attempt to parse the input as a GUID
                if (Guid.TryParse(input, out var value))
                {
                    // Return the valid GUID
                    return value;
                }

                // Display an error message and loop to prompt again
                Console.WriteLine("Please enter a valid ID.");
            }
        }

        /// <summary>
        /// Reads a date from the console with the given prompt
        /// </summary>
        protected DateOnly ReadDate(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var input = Console.ReadLine();

                if (DateOnly.TryParse(input, out var value))
                {
                    return value;
                }

                Console.WriteLine("Please enter a valid date in the format MM/DD/YYYY.");
            }
        }

        /// <summary>
        /// Reads a time from the console with the given prompt
        /// </summary>
        protected TimeOnly ReadTime(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var input = Console.ReadLine();

                if (TimeOnly.TryParse(input, out var value))
                {
                    return value;
                }

                Console.WriteLine("Please enter a valid time in the format HH:MM or HH:MM AM/PM.");
            }
        }

        /// <summary>
        /// Reads a boolean (yes/no) from the console with the given prompt
        /// </summary>
        protected bool ReadBoolean(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt} (y/n): ");
                var input = Console.ReadLine()?.ToLower();

                if (input == "y" || input == "yes")
                {
                    return true;
                }
                else if (input == "n" || input == "no")
                {
                    return false;
                }

                Console.WriteLine("Please enter 'y' for yes or 'n' for no.");
            }
        }

        /// <summary>
        /// Alternative name for ReadBoolean method for more readable code
        /// </summary>
        protected bool ReadYesNo(string prompt) => ReadBoolean(prompt);

        /// <summary>
        /// Reads a day of week from the console
        /// </summary>
        protected DayOfWeek ReadDayOfWeek(string prompt)
        {
            Console.WriteLine("\nDays of Week:");
            Console.WriteLine("0: Sunday");
            Console.WriteLine("1: Monday");
            Console.WriteLine("2: Tuesday");
            Console.WriteLine("3: Wednesday");
            Console.WriteLine("4: Thursday");
            Console.WriteLine("5: Friday");
            Console.WriteLine("6: Saturday");

            while (true)
            {
                Console.Write(prompt);
                var input = Console.ReadLine();

                if (int.TryParse(input, out var value) && value >= 0 && value <= 6)
                {
                    return (DayOfWeek)value;
                }

                Console.WriteLine("Please enter a number between 0 and 6.");
            }
        }
    }
}
