using CourseManager.CLI.Core.Events;
using CourseManager.CLI.Core.Exceptions;
using CourseManager.CLI.Core.Infrastructure;
using CourseManager.CLI.Core.Models;
using CourseManager.CLI.Core.Services;
using Microsoft.Extensions.Logging;

namespace CourseManager.CLI.Data.Services
{
    /// <summary>
    /// Implementation of course management operations that handles CRUD operations,
    /// validation, and business logic related to courses
    /// </summary>
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<CourseService> _logger;

        /// <summary>
        /// Event raised when a course is created, updated, or deleted
        /// </summary>
        public event EventHandler<CourseEventArgs>? CourseChanged;

        /// <summary>
        /// Initializes a new instance of the CourseService class
        /// </summary>
        /// <param name="courseRepository">Repository for accessing and persisting course data</param>
        /// <param name="logger">Logger for recording service operations</param>
        /// <exception cref="ArgumentNullException">Thrown if courseRepository or logger is null</exception>
        /// <remarks>
        /// The service requires a course repository to perform data access operations
        /// and a logger to record activity and errors during operation.
        /// </remarks>
        public CourseService(ICourseRepository courseRepository, ILogger<CourseService> logger)
        {
            _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all courses from the repository
        /// </summary>
        /// <returns>A collection of all courses in the system</returns>
        /// <exception cref="DataOperationException">Thrown when there's an error retrieving courses from the repository</exception>
        /// <remarks>
        /// This method logs the operation and wraps any repository exceptions in a DataOperationException
        /// to provide a consistent exception model to callers.
        /// </remarks>
        public async Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            try
            {
                _logger.LogDebug("Getting all courses");
                return await _courseRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all courses");
                throw new DataOperationException("Failed to retrieve courses", ex);
            }
        }

        /// <summary>
        /// Retrieves a specific course by its unique identifier
        /// </summary>
        /// <param name="id">The unique identifier of the course to retrieve</param>
        /// <returns>The course with the specified ID</returns>
        /// <exception cref="EntityNotFoundException">Thrown when no course exists with the specified ID</exception>
        /// <exception cref="DataOperationException">Thrown when there's an error retrieving the course from the repository</exception>
        /// <remarks>
        /// This method preserves the EntityNotFoundException from the repository to indicate when a course doesn't exist, 
        /// while wrapping other exceptions in a DataOperationException.
        /// </remarks>
        public async Task<Course> GetCourseByIdAsync(Guid id)
        {
            try
            {
                _logger.LogDebug("Getting course by ID: {CourseId}", id);
                return await _courseRepository.GetByIdAsync(id);
            }
            catch (EntityNotFoundException)
            {
                _logger.LogWarning("Course not found with ID: {CourseId}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course by ID: {CourseId}", id);
                throw new DataOperationException($"Failed to retrieve course with ID {id}", ex);
            }
        }

        /// <summary>
        /// Retrieves a specific course by its unique course code
        /// </summary>
        /// <param name="code">The course code to search for</param>
        /// <returns>The course with the specified code</returns>
        /// <exception cref="EntityNotFoundException">Thrown when no course exists with the specified code</exception>
        /// <exception cref="DataOperationException">Thrown when there's an error retrieving the course from the repository</exception>
        /// <remarks>
        /// Course codes are expected to be unique within the system. This method preserves the EntityNotFoundException 
        /// from the repository to indicate when a course doesn't exist, while wrapping other exceptions in a DataOperationException.
        /// </remarks>
        public async Task<Course> GetCourseByCodeAsync(string code)
        {
            try
            {
                _logger.LogDebug("Getting course by code: {CourseCode}", code);
                return await _courseRepository.GetByCodeAsync(code);
            }
            catch (EntityNotFoundException)
            {
                _logger.LogWarning("Course not found with code: {CourseCode}", code);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course by code: {CourseCode}", code);
                throw new DataOperationException($"Failed to retrieve course with code {code}", ex);
            }
        }

        /// <summary>
        /// Adds a new course to the system after validation
        /// </summary>
        /// <param name="course">The course details to add</param>
        /// <returns>The added course with any system-generated properties set</returns>
        /// <exception cref="ArgumentNullException">Thrown when the course parameter is null</exception>
        /// <exception cref="ValidationException">Thrown when the course fails validation or a duplicate course code is found</exception>
        /// <exception cref="DataOperationException">Thrown when there's an error adding the course to the repository</exception>
        /// <remarks>
        /// This method:
        /// 1. Validates the course using business logic rules
        /// 2. Checks if a course with the same code already exists
        /// 3. Adds the course to the repository
        /// 4. Raises the CourseChanged event with "Added" action
        /// </remarks>
        public async Task<Course> AddCourseAsync(Course course)
        {
            if (course == null)
                throw new ArgumentNullException(nameof(course));

            ValidateCourse(course);

            try
            {
                // Check if course code already exists
                try
                {
                    var existingCourse = await _courseRepository.GetByCodeAsync(course.Code);
                    // If we found a course with the same code
                    // This prevents duplicate course codes while allowing a course to keep its own code
                    if (existingCourse != null)
                    {
                        _logger.LogWarning("Another course with code {course.Code} already exists", course.Code);
                        throw new ValidationException($"Another course with code {course.Code} already exists");
                    }
                }
                catch (EntityNotFoundException)
                {
                    // This exception means no other course has this code
                    // This is the expected path when adding to a new unique code
                    // No action needed - absence of course allows us to proceed
                }

                _logger.LogInformation("Adding new course: {CourseCode} - {CourseTitle}", course.Code, course.Title);
                var addedCourse = await _courseRepository.AddAsync(course);

                // Raise event
                OnCourseChanged(new CourseEventArgs(
                    addedCourse.Id,
                    addedCourse.Code,
                    addedCourse.Title,
                    "Added"));

                return addedCourse;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding course: {CourseCode} - {CourseTitle}", course.Code, course.Title);
                throw new DataOperationException("Failed to add course", ex);
            }
        }

        /// <summary>
        /// Updates an existing course in the system after validation
        /// </summary>
        /// <param name="course">The course with updated details</param>
        /// <returns>The updated course</returns>
        /// <exception cref="ArgumentNullException">Thrown when the course parameter is null</exception>
        /// <exception cref="ValidationException">Thrown when the course fails validation or a code conflict is found</exception>
        /// <exception cref="EntityNotFoundException">Thrown when the course to update does not exist</exception>
        /// <exception cref="DataOperationException">Thrown when there's an error updating the course in the repository</exception>
        /// <remarks>
        /// This method:
        /// 1. Validates the course data
        /// 2. Verifies the course exists in the system
        /// 3. Checks for code conflicts with other courses
        /// 4. Updates the course in the repository
        /// 5. Raises the CourseChanged event with "Updated" action
        /// </remarks>
        public async Task<Course> UpdateCourseAsync(Course course)
        {
            if (course == null)
                throw new ArgumentNullException(nameof(course)); // Validate course data including required fields and value constraints

            // This ensures the course meets all business rules before attempting to update
            ValidateCourse(course);

            try
            {
                // Verify the course exists in the repository before attempting to update
                // This will throw EntityNotFoundException if course doesn't exist
                await _courseRepository.GetByIdAsync(course.Id);

                // Check if updated course code would conflict with another existing course
                // We need to ensure course codes remain unique across the system
                try
                {
                    // Look for any course with the same code
                    var existingCourse = await _courseRepository.GetByCodeAsync(course.Code);

                    // If we found a course with the same code but different ID, it's a conflict
                    // This prevents duplicate course codes while allowing a course to keep its own code
                    if (existingCourse.Id != course.Id)
                    {
                        throw new ValidationException($"Another course with code {course.Code} already exists");
                    }
                }
                catch (EntityNotFoundException)
                {
                    // This exception means no other course has this code
                    // This is the expected path when changing to a new unique code
                    // No action needed - absence of a conflict allows us to proceed
                }

                _logger.LogInformation("Updating course: {CourseId} - {CourseCode} - {CourseTitle}",
                    course.Id, course.Code, course.Title);

                var updatedCourse = await _courseRepository.UpdateAsync(course);

                // Raise event
                OnCourseChanged(new CourseEventArgs(
                    updatedCourse.Id,
                    updatedCourse.Code,
                    updatedCourse.Title,
                    "Updated"));

                return updatedCourse;
            }
            catch (EntityNotFoundException)
            {
                _logger.LogWarning("Course not found for update: {CourseId}", course.Id);
                throw;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course: {CourseId}", course.Id);
                throw new DataOperationException($"Failed to update course with ID {course.Id}", ex);
            }
        }

        /// <summary>
        /// Deletes a course from the system by ID
        /// </summary>
        /// <param name="id">The unique identifier of the course to delete</param>
        /// <exception cref="EntityNotFoundException">Thrown when the course to delete does not exist</exception>
        /// <exception cref="DataOperationException">Thrown when there's an error deleting the course from the repository</exception>
        /// <remarks>
        /// This method:
        /// 1. Verifies the course exists and retrieves its details for the event
        /// 2. Deletes the course from the repository
        /// 3. Raises the CourseChanged event with "Deleted" action
        /// 
        /// Note: The implementation does not check for dependencies (like schedules or enrollments).
        /// Such checks might be added in future versions for referential integrity.
        /// </remarks>
        public async Task DeleteCourseAsync(Guid id)
        {
            try
            {
                // Verify the course exists and get its details for the event
                var course = await _courseRepository.GetByIdAsync(id);

                _logger.LogInformation("Deleting course: {CourseId} - {CourseCode} - {CourseTitle}",
                    id, course.Code, course.Title);

                await _courseRepository.DeleteAsync(id);

                // Raise event
                OnCourseChanged(new CourseEventArgs(
                    course.Id,
                    course.Code,
                    course.Title,
                    "Deleted"));
            }
            catch (EntityNotFoundException)
            {
                _logger.LogWarning("Course not found for deletion: {CourseId}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting course: {CourseId}", id);
                throw new DataOperationException($"Failed to delete course with ID {id}", ex);
            }
        }

        /// <summary>
        /// Retrieves all courses belonging to a specific department
        /// </summary>
        /// <param name="department">The department name to filter courses by</param>
        /// <returns>A collection of courses in the specified department</returns>
        /// <exception cref="ArgumentException">Thrown when the department parameter is null or empty</exception>
        /// <exception cref="DataOperationException">Thrown when there's an error retrieving courses from the repository</exception>
        /// <remarks>
        /// This method applies filtering at the repository level for better performance compared to
        /// retrieving all courses and filtering in memory.
        /// </remarks>
        public async Task<IEnumerable<Course>> GetCoursesByDepartmentAsync(string department)
        {
            if (string.IsNullOrWhiteSpace(department))
                throw new ArgumentException("Department cannot be null or empty", nameof(department));

            try
            {
                _logger.LogDebug("Getting courses by department: {Department}", department);
                return await _courseRepository.GetByDepartmentAsync(department);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting courses by department: {Department}", department);
                throw new DataOperationException($"Failed to retrieve courses for department {department}", ex);
            }
        }

        /// <summary>
        /// Retrieves all courses assigned to a specific instructor
        /// </summary>
        /// <param name="instructorId">The unique identifier of the instructor</param>
        /// <returns>A collection of courses assigned to the specified instructor</returns>
        /// <exception cref="DataOperationException">Thrown when there's an error retrieving courses from the repository</exception>
        /// <remarks>
        /// This method uses the repository's specialized query for better performance.
        /// It returns an empty collection if the instructor exists but has no assigned courses.
        /// </remarks>
        public async Task<IEnumerable<Course>> GetCoursesByInstructorAsync(Guid instructorId)
        {
            try
            {
                _logger.LogDebug("Getting courses by instructor: {InstructorId}", instructorId);
                return await _courseRepository.GetByInstructorAsync(instructorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting courses by instructor: {InstructorId}", instructorId);
                throw new DataOperationException($"Failed to retrieve courses for instructor {instructorId}", ex);
            }
        }

        /// <summary>
        /// Raises the CourseChanged event
        /// </summary>
        /// <param name="e">Event arguments containing information about the changed course</param>
        protected virtual void OnCourseChanged(CourseEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            _logger.LogDebug("Raising CourseChanged event: {CourseId}, {CourseCode}, Action: {Action}",
                e.CourseId, e.CourseCode, e.Action);

            CourseChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Validates a course to ensure all required fields are provided and values are within valid ranges
        /// </summary>
        /// <param name="course">The course to validate</param>
        /// <exception cref="ArgumentNullException">Thrown if course is null</exception>
        /// <exception cref="ValidationException">Thrown if validation fails with details about the failures</exception>
        private void ValidateCourse(Course course)
        {
            if (course == null)
                throw new ArgumentNullException(nameof(course));

            var validationErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(course.Code))
                validationErrors.Add("Course code is required");
            else if (course.Code.Length > 20)
                validationErrors.Add("Course code cannot exceed 20 characters");

            if (string.IsNullOrWhiteSpace(course.Title))
                validationErrors.Add("Course title is required");
            else if (course.Title.Length > 200)
                validationErrors.Add("Course title cannot exceed 200 characters");

            if (course.Credits <= 0)
                validationErrors.Add("Credits must be greater than zero");
            else if (course.Credits > 12) // Assuming typical max credits
                validationErrors.Add("Credits cannot exceed 12");

            if (course.MaxEnrollment <= 0)
                validationErrors.Add("Maximum enrollment must be greater than zero");
            else if (course.MaxEnrollment > 500) // Assuming a reasonable limit
                validationErrors.Add("Maximum enrollment cannot exceed 500 students");

            if (string.IsNullOrWhiteSpace(course.Department))
                validationErrors.Add("Department is required");
            else if (course.Department.Length > 100)
                validationErrors.Add("Department name cannot exceed 100 characters");

            // Description can be optional but shouldn't be excessively long
            if (!string.IsNullOrEmpty(course.Description) && course.Description.Length > 2000)
                validationErrors.Add("Description cannot exceed 2000 characters");

            if (validationErrors.Count > 0)
            {
                _logger.LogWarning("Course validation failed: {CourseCode} - Errors: {ValidationErrors}",
                    course.Code, string.Join(", ", validationErrors));

                throw new ValidationException(
                    "Course validation failed: " + string.Join(", ", validationErrors));
            }
        }
    }
}
