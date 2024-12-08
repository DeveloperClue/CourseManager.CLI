namespace CourseManager.CLI.ConsoleApp.Menu
{
    /// <summary>
    /// Interface for menu management operations
    /// </summary>
    public interface IMenuManager
    {
        /// <summary>
        /// Starts the menu system
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// Displays the main menu
        /// </summary>
        Task ShowMainMenuAsync();

        /// <summary>
        /// Displays the course management menu
        /// </summary>
        Task ShowCourseMenuAsync();

        /// <summary>
        /// Displays the instructor management menu
        /// </summary>
        Task ShowInstructorMenuAsync();
    }
}
