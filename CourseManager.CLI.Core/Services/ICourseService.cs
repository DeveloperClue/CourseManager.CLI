using CourseManager.CLI.Core.Events;
using CourseManager.CLI.Core.Models;

namespace CourseManager.CLI.Core.Services
{
    /// <summary>
    /// Interface for course management operations.
    /// Provides methods for creating, reading, updating, and deleting course information,
    /// as well as specialized queries for course data.
    /// </summary>
    public interface ICourseService
    {
        /// <summary>
        /// Event raised when a course is added, updated, or deleted.
        /// Subscribers can use this event to react to course data changes.
        /// </summary>
        event EventHandler<CourseEventArgs> CourseChanged;

        /// <summary>
        /// Retrieves all courses in the system
        /// </summary>
        /// <returns>A collection of all courses</returns>
        /// <exception cref="Core.Exceptions.DataOperationException">Thrown when the retrieval operation fails</exception>
        Task<IEnumerable<Course>> GetAllCoursesAsync();

        /// <summary>
        /// Retrieves a specific course by its unique identifier
        /// </summary>
        /// <param name="id">The unique identifier of the course to retrieve</param>
        /// <returns>The course with the specified ID</returns>
        /// <exception cref="Core.Exceptions.EntityNotFoundException">Thrown when the course with the specified ID is not found</exception>
        /// <exception cref="Core.Exceptions.DataOperationException">Thrown when the retrieval operation fails</exception>
        Task<Course> GetCourseByIdAsync(Guid id);

        /// <summary>
        /// Retrieves a specific course by its unique course code
        /// </summary>
        /// <param name="code">The course code to search for (e.g., "CS101")</param>
        /// <returns>The course with the specified code</returns>
        /// <exception cref="Core.Exceptions.EntityNotFoundException">Thrown when the course with the specified code is not found</exception>
        /// <exception cref="Core.Exceptions.DataOperationException">Thrown when the retrieval operation fails</exception>
        Task<Course> GetCourseByCodeAsync(string code);

        /// <summary>
        /// Adds a new course to the system with validation
        /// </summary>
        /// <param name="course">The course information to add</param>
        /// <returns>The added course with generated ID</returns>
        /// <exception cref="ArgumentNullException">Thrown when the course parameter is null</exception>
        /// <exception cref="Core.Exceptions.ValidationException">Thrown when the course data fails validation</exception>
        /// <exception cref="Core.Exceptions.DataOperationException">Thrown when the add operation fails</exception>
        Task<Course> AddCourseAsync(Course course);

        /// <summary>
        /// Updates an existing course in the system with validation
        /// </summary>
        /// <param name="course">The course with updated information</param>
        /// <returns>The updated course</returns>
        /// <exception cref="ArgumentNullException">Thrown when the course parameter is null</exception>
        /// <exception cref="Core.Exceptions.EntityNotFoundException">Thrown when the course to update is not found</exception>
        /// <exception cref="Core.Exceptions.ValidationException">Thrown when the course data fails validation</exception>
        /// <exception cref="Core.Exceptions.DataOperationException">Thrown when the update operation fails</exception>
        Task<Course> UpdateCourseAsync(Course course);

        /// <summary>
        /// Deletes a course from the system by its unique identifier.
        /// This will also remove all associated schedules.
        /// </summary>
        /// <param name="id">The unique identifier of the course to delete</param>        /// <exception cref="Core.Exceptions.EntityNotFoundException">Thrown when the course with the specified ID is not found</exception>
        /// <exception cref="Core.Exceptions.DataOperationException">Thrown when the delete operation fails</exception>
        Task DeleteCourseAsync(Guid id);

        /// <summary>
        /// Retrieves all courses belonging to a specific department
        /// </summary>
        /// <param name="department">The name of the department to filter by (e.g., "Computer Science")</param>
        /// <returns>A collection of courses in the specified department</returns>
        /// <exception cref="ArgumentNullException">Thrown when the department parameter is null</exception>
        /// <exception cref="Core.Exceptions.DataOperationException">Thrown when the retrieval operation fails</exception>
        Task<IEnumerable<Course>> GetCoursesByDepartmentAsync(string department);

        /// <summary>
        /// Retrieves all courses taught by a specific instructor
        /// </summary>
        /// <param name="instructorId">The unique identifier of the instructor</param>
        /// <returns>A collection of courses taught by the specified instructor</returns>
        /// <exception cref="Core.Exceptions.EntityNotFoundException">Thrown when the instructor with the specified ID is not found</exception>
        /// <exception cref="Core.Exceptions.DataOperationException">Thrown when the retrieval operation fails</exception>
        Task<IEnumerable<Course>> GetCoursesByInstructorAsync(Guid instructorId);
    }
}
