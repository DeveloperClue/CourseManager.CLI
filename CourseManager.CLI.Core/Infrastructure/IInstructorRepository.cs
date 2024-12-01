using CourseManager.CLI.Core.Models;

namespace CourseManager.CLI.Core.Infrastructure
{
    /// <summary>
    /// Repository interface for Instructor entities with instructor-specific data access operations
    /// extending the base repository capabilities with additional querying and assignment methods.
    /// </summary>
    public interface IInstructorRepository : IRepository<Instructor>
    {
        /// <summary>
        /// Retrieves all instructors belonging to a specific academic department
        /// </summary>
        /// <param name="department">The department code or name (e.g., "CS", "Computer Science")</param>
        /// <returns>A collection of instructors in the specified department</returns>
        Task<IEnumerable<Instructor>> GetByDepartmentAsync(string department);

        /// <summary>
        /// Retrieves all instructors assigned to teach a specific course
        /// </summary>
        /// <param name="courseId">The unique identifier of the course</param>
        /// <returns>A collection of instructors assigned to the specified course</returns>
        Task<IEnumerable<Instructor>> GetByCourseAsync(Guid courseId);

        /// <summary>
        /// Retrieves an instructor by their email address
        /// </summary>
        /// <param name="email">The email address of the instructor</param>
        /// <returns>The instructor with the specified email, or null if not found</returns>
        /// <remarks>
        /// Email addresses are unique across the system and serve as business identifiers
        /// in addition to the technical GUID identifier.
        /// </remarks>
        Task<Instructor> GetByEmailAsync(string email);

        /// <summary>
        /// Assigns an instructor to teach a specific course
        /// </summary>
        /// <param name="instructorId">The unique identifier of the instructor</param>
        /// <param name="courseId">The unique identifier of the course</param>
        /// <returns>True if the assignment was successful, false otherwise</returns>
        /// <remarks>
        /// This creates a many-to-many relationship between instructors and courses,
        /// allowing multiple instructors to be assigned to the same course.
        /// </remarks>
        Task<bool> AssignToCourseAsync(Guid instructorId, Guid courseId);

        /// <summary>
        /// Removes an instructor's assignment from a specific course
        /// </summary>
        /// <param name="instructorId">The unique identifier of the instructor</param>
        /// <param name="courseId">The unique identifier of the course</param>
        /// <returns>True if the removal was successful, false if the assignment didn't exist or couldn't be removed</returns>
        Task<bool> RemoveFromCourseAsync(Guid instructorId, Guid courseId);

        /// <summary>
        /// Checks if an instructor is currently assigned to teach a specific course
        /// </summary>
        /// <param name="instructorId">The unique identifier of the instructor</param>
        /// <param name="courseId">The unique identifier of the course</param>
        /// <returns>True if the instructor is assigned to the course, false otherwise</returns>
        Task<bool> IsAssignedToCourseAsync(Guid instructorId, Guid courseId);
    }
}
