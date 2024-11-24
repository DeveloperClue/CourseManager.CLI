using CourseManager.CLI.ConsoleApp.Commands;
using Microsoft.Extensions.Logging;

namespace CourseManager.CLI.ConsoleApp.Menu
{
    /// <summary>
    /// Implementation of menu management operations providing the user interface for the application
    /// </summary>
    /// <remarks>
    /// The MenuManager handles all user interaction through a hierarchical menu system.
    /// It displays menus, processes user input, and executes the appropriate commands
    /// based on user selections.
    /// </remarks>
    public class MenuManager : IMenuManager
    {
        /// <summary>
        /// Factory for creating command objects based on user input
        /// </summary>
        private readonly ICommandFactory _commandFactory;

        /// <summary>
        /// Logger for recording menu operations
        /// </summary>
        private readonly ILogger<MenuManager> _logger;

        /// <summary>
        /// Flag indicating whether the application should exit
        /// </summary>
        private bool _exitApplication = false;

        /// <summary>
        /// Initializes a new instance of the MenuManager class
        /// </summary>
        /// <param name="commandFactory">The command factory for creating command objects</param>
        /// <param name="logger">The logger for recording menu operations</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
        public MenuManager(ICommandFactory commandFactory, ILogger<MenuManager> logger)
        {
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Starts the menu system and runs the main application loop
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <remarks>
        /// This method begins by displaying a welcome message and then enters the main
        /// application loop, continuously showing the main menu until the user chooses to exit.
        /// </remarks>
        public async Task StartAsync()
        {
            // Display welcome message when the application starts
            DisplayWelcomeMessage();

            // Continue showing the main menu until the exit flag is set
            while (!_exitApplication)
            {
                await ShowMainMenuAsync();
            }
        }

        /// <summary>
        /// Displays the main menu and processes the user's selection
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task ShowMainMenuAsync()
        {
            // Clear the console for a clean display
            Console.Clear();

            // Display the main menu options
            Console.WriteLine("========================================");
            Console.WriteLine("       COURSE MANAGER - MAIN MENU       ");
            Console.WriteLine("========================================");
            Console.WriteLine("1. Course Management");
            Console.WriteLine("2. Instructor Management");
            Console.WriteLine("3. Schedule Management");
            Console.WriteLine("4. Reporting");
            Console.WriteLine("0. Exit Application");
            Console.WriteLine("========================================");

            // Get the user's menu selection
            var choice = GetUserChoice("Enter your choice: ");

            // Execute the selected menu option
            await ExecuteMainMenuChoiceAsync(choice);
        }

        /// <summary>
        /// Displays the course management menu and processes the user's selection
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <remarks>
        /// This method shows a submenu with course management options and handles user input.
        /// The menu includes options for listing, viewing, adding, updating, and deleting courses,
        /// as well as finding courses by department.
        /// </remarks>
        public async Task ShowCourseMenuAsync()
        {
            bool returnToMain = false;

            while (!returnToMain)
            {
                // Clear the console for a clean display
                Console.Clear();

                // Display the course management menu
                Console.WriteLine("========================================");
                Console.WriteLine("        COURSE MANAGEMENT MENU         ");
                Console.WriteLine("========================================");
                Console.WriteLine("1. List All Courses");
                Console.WriteLine("2. View Course Details");
                Console.WriteLine("3. Add New Course");
                Console.WriteLine("4. Update Existing Course");
                Console.WriteLine("5. Delete Course");
                Console.WriteLine("6. Find Courses by Department");
                Console.WriteLine("0. Return to Main Menu");
                Console.WriteLine("========================================");

                // Get the user's menu selection
                var choice = GetUserChoice("Enter your choice: ");

                // Process the user's choice
                switch (choice)
                {
                    case 1:
                        // Execute the list courses command and wait for completion
                        await _commandFactory.CreateCommand("list-courses").ExecuteAsync();
                        WaitForKeyPress();
                        break;
                    case 2:
                        // Execute the view course command and wait for completion
                        await _commandFactory.CreateCommand("view-course").ExecuteAsync();
                        WaitForKeyPress();
                        break;
                    case 3:
                        // Execute the add course command and wait for completion
                        await _commandFactory.CreateCommand("add-course").ExecuteAsync();
                        WaitForKeyPress();
                        break;
                    case 4:
                        // Execute the update course command and wait for completion
                        await _commandFactory.CreateCommand("update-course").ExecuteAsync();
                        WaitForKeyPress();
                        break;
                    case 5:
                        // Execute the delete course command and wait for completion
                        await _commandFactory.CreateCommand("delete-course").ExecuteAsync();
                        WaitForKeyPress();
                        break;
                    case 6:
                        // Execute the find courses by department command and wait for completion
                        await _commandFactory.CreateCommand("find-courses-by-department").ExecuteAsync();
                        WaitForKeyPress();
                        break;
                    case 0:
                        // Return to the main menu
                        returnToMain = true;
                        break;
                    default:
                        // Handle invalid menu choices
                        Console.WriteLine("Invalid option. Please try again.");
                        WaitForKeyPress();
                        break;
                }
            }
        }

        /// <summary>
        /// Executes the action corresponding to the user's main menu choice
        /// </summary>
        /// <param name="choice">The user's menu choice</param>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task ExecuteMainMenuChoiceAsync(int choice)
        {
            switch (choice)
            {
                case 1:
                    await ShowCourseMenuAsync();
                    break;
                default:
                    DisplayInvalidChoiceMessage();
                    break;
            }
        }

        /// <summary>
        /// Displays the welcome message at the start of the application
        /// </summary>
        private void DisplayWelcomeMessage()
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("          COURSE MANAGER CLI            ");
            Console.WriteLine("========================================");
            Console.WriteLine("Welcome to the Course Manager CLI Tool");
            Console.WriteLine("This application helps manage courses,");
            Console.WriteLine("instructors and class schedules.");
            Console.WriteLine("========================================");
            WaitForKeyPress("Press any key to continue...");
        }

        /// <summary>
        /// Prompts the user for a choice and retrieves the input
        /// </summary>
        /// <param name="prompt">The prompt message to display</param>
        /// <returns>The user's choice as an integer</returns>
        private int GetUserChoice(string prompt)
        {
            Console.Write(prompt);
            string input = Console.ReadLine() ?? string.Empty;

            if (int.TryParse(input, out int result))
            {
                return result;
            }

            return -1;
        }

        /// <summary>
        /// Displays an invalid choice message and waits for a key press
        /// </summary>
        private void DisplayInvalidChoiceMessage()
        {
            Console.WriteLine("\nInvalid selection. Please try again.");
            WaitForKeyPress();
        }

        /// <summary>
        /// Waits for a key press from the user
        /// </summary>
        /// <param name="message">The message to display while waiting</param>
        private void WaitForKeyPress(string message = "Press any key to continue...")
        {
            Console.WriteLine($"\n{message}");
            Console.ReadKey(true);
        }

        /// <summary>
        /// Exits the application and displays a goodbye message
        /// </summary>
        private void ExitApplication()
        {
            Console.Clear();
            Console.WriteLine("Thank you for using Course Manager CLI!");
            Console.WriteLine("Goodbye!");
            _exitApplication = true;
        }
    }
}
