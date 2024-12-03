using CourseManager.CLI.Core.Events;
using CourseManager.CLI.Core.Models;

namespace CourseManager.CLI.Core.Services
{
    /// <summary>
    /// Interface for instructor management operations.
    /// Provides methods for creating, reading, updating, and deleting instructor information,
    /// as well as handling instructor assignments to courses.
    /// </summary>
    public interface IInstructorService
    {
        /// <summary>
        /// Event raised when an instructor is added, updated, or deleted.
        /// Subscribers can use this event to react to instructor data changes.
        /// </summary>
        event EventHandler<InstructorEventArgs> InstructorChanged;

        /// <summary>
        /// Retrieves all instructors in the system
        /// </summary>
        /// <returns>A collection of all instructors</returns>
        /// <exception cref="Core.Exceptions.DataOperationException">Thrown when the retrieval operation fails</exception>
        Task<IEnumerable<Instructor>> GetAllInstructorsAsync();

        /// <summary>
        /// Retrieves a specific instructor by their unique identifier
        /// </summary>
        /// <param name="id">The unique identifier of the instructor to retrieve</param>
        /// <returns>The instructor with the specified ID</returns>
        /// <exception cref="Core.Exceptions.EntityNotFoundException">Thrown when the instructor with the specified ID is not found</exception>
        /// <exception cref="Core.Exceptions.DataOperationException">Thrown when the retrieval operation fails</exception>
        Task<Instructor> GetInstructorByIdAsync(Guid id);

        /// <summary>
        /// Retrieves all instructors belonging to a specific department
        /// </summary>
        /// <param name="department">The name of the department to filter by</param>
        /// <returns>A collection of instructors in the specified department</returns>
        /// <exception cref="ArgumentNullException">Thrown when the department parameter is null</exception>
        /// <exception cref="Core.Exceptions.DataOperationException">Thrown when the retrieval operation fails</exception>        
        Task<IEnumerable<Instructor>> GetInstructorsByDepartmentAsync(string department);

        /// <summary>
        /// Adds a new instructor to the system with validation
        /// </summary>
        /// <param name="instructor">The instructor information to add</param>
        /// <returns>The added instructor with generated ID</returns>
        /// <exception cref="ArgumentNullException">Thrown when the instructor parameter is null</exception>
        /// <exception cref="Core.Exceptions.ValidationException">Thrown when the instructor data fails validation</exception>
        /// <exception cref="Core.Exceptions.DataOperationException">Thrown when the add operation fails</exception>
        Task<Instructor> AddInstructorAsync(Instructor instructor);

        /// <summary>
        /// Updates an existing instructor in the system with validation
        /// </summary>
        /// <param name="instructor">The instructor with updated information</param>
        /// <returns>The updated instructor</returns>
        /// <exception cref="ArgumentNullException">Thrown when the instructor parameter is null</exception>
        /// <exception cref="Core.Exceptions.EntityNotFoundException">Thrown when the instructor to update is not found</exception>
        /// <exception cref="Core.Exceptions.ValidationException">Thrown when the instructor data fails validation</exception>
        /// <exception cref="Core.Exceptions.DataOperationException">Thrown when the update operation fails</exception>
        Task<Instructor> UpdateInstructorAsync(Instructor instructor);

        /// <summary>
        /// Deletes an instructor from the system by their unique identifier.
        /// This will also remove all associated course assignments.
        /// </summary>
        /// <param name="id">The unique identifier of the instructor to delete</param>        /// <exception cref="Core.Exceptions.EntityNotFoundException">Thrown when the instructor with the specified ID is not found</exception>
        /// <exception cref="Core.Exceptions.DataOperationException">Thrown when the delete operation fails</exception>
        Task DeleteInstructorAsync(Guid id);

        /// <summary>
        /// Assigns an instructor to teach a specific course
        /// </summary>
        /// <param name="instructorId">The unique identifier of the instructor to assign</param>
        /// <param name="courseId">The unique identifier of the course to assign the instructor to</param>
        /// <returns>True if the assignment was created, false if it already existed</returns>
        /// <exception cref="Core.Exceptions.EntityNotFoundException">Thrown when the instructor or course is not found</exception>        /// <exception cref="Core.Exceptions.DataOperationException">Thrown when the assignment operation fails</exception>
        Task<bool> AssignInstructorToCourseAsync(Guid instructorId, Guid courseId);

        /// <summary>
        /// Removes an instructor from teaching a specific course
        /// </summary>
        /// <param name="instructorId">The unique identifier of the instructor to remove</param>
        /// <param name="courseId">The unique identifier of the course to remove the instructor from</param>
        /// <returns>True if the assignment was removed, false if it didn't exist</returns>
        /// <exception cref="Core.Exceptions.EntityNotFoundException">Thrown when the instructor or course is not found</exception>
        /// <exception cref="Core.Exceptions.DataOperationException">Thrown when the removal operation fails</exception>
        Task<bool> RemoveInstructorFromCourseAsync(Guid instructorId, Guid courseId);

        /// <summary>
        /// Retrieves all instructors assigned to teach a specific course
        /// </summary>
        /// <param name="courseId">The unique identifier of the course</param>
        /// <returns>A collection of instructors assigned to the specified course</returns>
        /// <exception cref="Core.Exceptions.EntityNotFoundException">Thrown when the course with the specified ID is not found</exception>
        /// <exception cref="Core.Exceptions.DataOperationException">Thrown when the retrieval operation fails</exception>
        Task<IEnumerable<Instructor>> GetInstructorsByCourseAsync(Guid courseId);
    }
}
