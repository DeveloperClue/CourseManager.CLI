using CourseManager.CLI.Core.Exceptions;
using CourseManager.CLI.Core.Infrastructure;
using CourseManager.CLI.Core.Models;
using Microsoft.Extensions.Logging;

namespace CourseManager.CLI.Data.Repositories
{
    /// <summary>
    /// JSON file-based repository for Course entities
    /// </summary>
    public class CourseRepository : JsonFileRepository<Course>, ICourseRepository
    {
        /// <summary>
        /// Initializes a new instance of the CourseRepository class
        /// </summary>
        /// <param name="dataDirectory">Directory where the courses.json file is stored</param>
        /// <param name="logger">Logger for recording repository operations</param>
        public CourseRepository(string dataDirectory, ILogger<CourseRepository> logger)
            : base(Path.Combine(dataDirectory, "courses.json"), logger)
        {
        }

        /// <summary>
        /// Adds a new course to the repository
        /// </summary>
        /// <param name="entity">The course to add</param>
        /// <returns>The added course</returns>
        /// <exception cref="ValidationException">Thrown when a course with the same code already exists</exception>
        public override async Task<Course> AddAsync(Course entity)
        {
            // Check for duplicate course code
            var existingCourse = _entities.FirstOrDefault(c => 
                c.Code.Equals(entity.Code, StringComparison.OrdinalIgnoreCase));
                
            if (existingCourse != null)
            {
                throw new ValidationException($"Course with code {entity.Code} already exists");
            }
            
            return await base.AddAsync(entity);
        }

        /// <summary>
        /// Retrieves a course by its unique course code
        /// </summary>
        /// <param name="code">The course code (e.g., "CS101", "MATH200")</param>
        /// <returns>The course with the specified code</returns>
        /// <exception cref="EntityNotFoundException">Thrown when a course with the specified code is not found</exception>
        public async Task<Course> GetByCodeAsync(string code)
        {
            // Find the first course with a matching code (case-insensitive)
            var course = _entities.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

            if (course == null)
                throw new EntityNotFoundException("Course", $"Code: {code}");

            return await Task.FromResult(course);
        }

        /// <summary>
        /// Retrieves all courses offered by a specific academic department
        /// </summary>
        /// <param name="department">The department code or name (e.g., "CS", "Computer Science")</param>
        /// <returns>A collection of courses in the specified department</returns>
        public async Task<IEnumerable<Course>> GetByDepartmentAsync(string department)
        {
            // Filter courses by department name (case-insensitive) and convert to list
            return await Task.FromResult(_entities
                .Where(c => c.Department.Equals(department, StringComparison.OrdinalIgnoreCase))
                .ToList());
        }

        /// <summary>
        /// Retrieves all courses taught by a specific instructor
        /// </summary>
        /// <param name="instructorId">The unique identifier of the instructor</param>
        /// <returns>A collection of courses taught by the specified instructor</returns>
        public async Task<IEnumerable<Course>> GetByInstructorAsync(Guid instructorId)
        {
            // Find all courses that have the instructorId in their InstructorIds collection
            return await Task.FromResult(_entities
                .Where(c => c.InstructorIds.Contains(instructorId))
                .ToList());
        }
    }
}
