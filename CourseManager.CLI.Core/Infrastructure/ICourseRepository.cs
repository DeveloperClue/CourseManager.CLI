using CourseManager.CLI.Core.Models;

namespace CourseManager.CLI.Core.Infrastructure
{
    /// <summary>
    /// Repository interface for Course entities with course-specific data access operations 
    /// extending the base repository capabilities with additional querying methods.
    /// </summary>
    public interface ICourseRepository : IRepository<Course>
    {
        /// <summary>
        /// Retrieves a course by its unique course code
        /// </summary>
        /// <param name="code">The course code (e.g., "CS101", "MATH200")</param>
        /// <returns>The course with the specified code, or null if not found</returns>
        /// <remarks>
        /// Course codes are unique across the system and serve as business identifiers
        /// in addition to the technical GUID identifier.
        /// </remarks>
        Task<Course> GetByCodeAsync(string code);

        /// <summary>
        /// Retrieves all courses offered by a specific academic department
        /// </summary>
        /// <param name="department">The department code or name (e.g., "CS", "Computer Science")</param>
        /// <returns>A collection of courses in the specified department</returns>
        Task<IEnumerable<Course>> GetByDepartmentAsync(string department);

        /// <summary>
        /// Retrieves all courses taught by a specific instructor
        /// </summary>
        /// <param name="instructorId">The unique identifier of the instructor</param>
        /// <returns>A collection of courses taught by the specified instructor</returns>
        /// <remarks>
        /// This returns courses where the instructor is assigned as either the primary 
        /// instructor or a co-instructor.
        /// </remarks>
        Task<IEnumerable<Course>> GetByInstructorAsync(Guid instructorId);
    }
}
