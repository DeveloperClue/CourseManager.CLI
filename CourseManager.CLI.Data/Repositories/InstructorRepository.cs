using CourseManager.CLI.Core.Infrastructure;
using CourseManager.CLI.Core.Models;
using Microsoft.Extensions.Logging;

namespace CourseManager.CLI.Data.Repositories
{
    /// <summary>
    /// JSON file-based repository for Instructor entities
    /// </summary>
    public class InstructorRepository : JsonFileRepository<Instructor>, IInstructorRepository
    {
        /// <summary>
        /// Initializes a new instance of the InstructorRepository class
        /// </summary>
        /// <param name="dataDirectory">Directory where the instructors.json file is stored</param>
        /// <param name="logger">Logger for recording repository operations</param>
        public InstructorRepository(string dataDirectory, ILogger<InstructorRepository> logger)
            : base(Path.Combine(dataDirectory, "instructors.json"), logger)
        {
        }

        /// <summary>
        /// Retrieves all instructors in a specific department
        /// </summary>
        /// <param name="department">The department name to filter by</param>
        /// <returns>A collection of instructors in the specified department</returns>
        public async Task<IEnumerable<Instructor>> GetByDepartmentAsync(string department)
        {
            // Filter instructors by department name (case-insensitive) and convert to list
            return await Task.FromResult(_entities
                .Where(i => i.Department.Equals(department, StringComparison.OrdinalIgnoreCase))
                .ToList());
        }

        /// <summary>
        /// Retrieves all instructors assigned to a specific course
        /// </summary>
        /// <param name="courseId">The unique identifier of the course</param>
        /// <returns>A collection of instructors teaching the specified course</returns>
        public async Task<IEnumerable<Instructor>> GetByCourseAsync(Guid courseId)
        {
            // Find all instructors that have the courseId in their CourseIds collection
            return await Task.FromResult(_entities
                .Where(i => i.CourseIds.Contains(courseId))
                .ToList());
        }

        /// <summary>
        /// Retrieves an instructor by their email address
        /// </summary>
        /// <param name="email">The email address of the instructor</param>
        /// <returns>The instructor with the specified email</returns>
        /// <exception cref="InvalidOperationException">Thrown when an instructor with the specified email is not found</exception>
        public async Task<Instructor> GetByEmailAsync(string email)
        {
            // Find the first instructor with a matching email (case-insensitive)
            return await Task.FromResult(_entities
                .FirstOrDefault(i => i.Email.Equals(email, StringComparison.OrdinalIgnoreCase)) ?? throw new InvalidOperationException($"Instructor with email {email} not found"));
        }

        /// <summary>
        /// Assigns an instructor to a course
        /// </summary>
        /// <param name="instructorId">The unique identifier of the instructor</param>
        /// <param name="courseId">The unique identifier of the course</param>
        /// <returns>True if the assignment was successful (instructor wasn't already assigned), false otherwise</returns>
        /// <exception cref="Core.Exceptions.EntityNotFoundException">Thrown when the instructor with the specified ID is not found</exception>
        /// <exception cref="Core.Exceptions.ValidationException">Thrown when the instructor is already assigned to maximum allowed courses</exception>
        public async Task<bool> AssignToCourseAsync(Guid instructorId, Guid courseId)
        {
            // Get the instructor entity
            var instructor = await GetByIdAsync(instructorId);

            // Check if the instructor is not already assigned to this course
            if (!instructor.CourseIds.Contains(courseId))
            {
                // Validate maximum course limit (4 courses per instructor)
                const int MaxCourses = 4;
                if (instructor.CourseIds.Count >= MaxCourses)
                {
                    throw new Core.Exceptions.ValidationException("Instructor cannot be assigned to more than 4 courses");
                }

                // Add the course to the instructor's list of courses
                instructor.CourseIds.Add(courseId);

                // Update the instructor in the repository
                await UpdateAsync(instructor);
                return true;
            }

            // Instructor was already assigned to this course, so no change was made
            return false;
        }

        /// <summary>
        /// Removes an instructor from a course
        /// </summary>
        /// <param name="instructorId">The unique identifier of the instructor</param>
        /// <param name="courseId">The unique identifier of the course</param>
        /// <returns>True if the removal was successful (instructor was assigned to the course), false otherwise</returns>
        /// <exception cref="Core.Exceptions.EntityNotFoundException">Thrown when the instructor with the specified ID is not found</exception>
        public async Task<bool> RemoveFromCourseAsync(Guid instructorId, Guid courseId)
        {
            // Get the instructor entity
            var instructor = await GetByIdAsync(instructorId);

            // Check if the instructor is currently assigned to this course
            if (instructor.CourseIds.Contains(courseId))
            {
                // Remove the course from the instructor's list
                instructor.CourseIds.Remove(courseId);

                // Update the instructor in the repository
                await UpdateAsync(instructor);
                return true;
            }

            // Instructor wasn't assigned to this course, so no change was made
            return false;
        }

        /// <summary>
        /// Checks if an instructor is assigned to a specific course
        /// </summary>
        /// <param name="instructorId">The unique identifier of the instructor</param>
        /// <param name="courseId">The unique identifier of the course</param>
        /// <returns>True if the instructor is assigned to the course, false otherwise</returns>
        /// <exception cref="Core.Exceptions.EntityNotFoundException">Thrown when the instructor with the specified ID is not found</exception>
        public async Task<bool> IsAssignedToCourseAsync(Guid instructorId, Guid courseId)
        {
            var instructor = await GetByIdAsync(instructorId);
            return instructor.CourseIds.Contains(courseId);
        }
    }
}
