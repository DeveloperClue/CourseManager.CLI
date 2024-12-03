using CourseManager.CLI.Core.Events;
using CourseManager.CLI.Core.Exceptions;
using CourseManager.CLI.Core.Infrastructure;
using CourseManager.CLI.Core.Models;
using CourseManager.CLI.Core.Services;
using Microsoft.Extensions.Logging;

namespace CourseManager.CLI.Data.Services
{
    /// <summary>
    /// Implementation of instructor management operations
    /// </summary>
    public class InstructorService : IInstructorService
    {
        private readonly IInstructorRepository _instructorRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<InstructorService> _logger;

        /// <summary>
        /// Event raised when an instructor is created, updated, or has course assignments changed
        /// </summary>
        public event EventHandler<InstructorEventArgs>? InstructorChanged;

        /// <summary>
        /// Initializes a new instance of the InstructorService class
        /// </summary>
        /// <param name="instructorRepository">Repository for accessing and persisting instructor data</param>
        /// <param name="courseRepository">Repository for accessing course data</param>
        /// <param name="logger">Logger for recording service operations</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the required dependencies are null</exception>
        /// <remarks>
        /// The service requires both instructor and course repositories because it manages
        /// the relationships between instructors and courses in addition to instructor data.
        /// </remarks>
        public InstructorService(IInstructorRepository instructorRepository, ICourseRepository courseRepository, ILogger<InstructorService> logger)
        {
            _instructorRepository = instructorRepository ?? throw new ArgumentNullException(nameof(instructorRepository));
            _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all instructors from the repository
        /// </summary>
        /// <returns>A collection of all instructors in the system</returns>
        /// <exception cref="DataOperationException">Thrown when there's an error retrieving instructors from the repository</exception>
        /// <remarks>
        /// This method logs the operation and wraps any repository exceptions in a DataOperationException
        /// to provide a consistent exception model to callers.
        /// </remarks>
        public async Task<IEnumerable<Instructor>> GetAllInstructorsAsync()
        {
            try
            {
                _logger.LogDebug("Getting all instructors");
                return await _instructorRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all instructors");
                throw new DataOperationException("Failed to retrieve instructors", ex);
            }
        }

        /// <summary>
        /// Retrieves a specific instructor by their unique identifier
        /// </summary>
        /// <param name="id">The unique identifier of the instructor to retrieve</param>
        /// <returns>The instructor with the specified ID</returns>
        /// <exception cref="EntityNotFoundException">Thrown when no instructor exists with the specified ID</exception>
        /// <exception cref="DataOperationException">Thrown when there's an error retrieving the instructor from the repository</exception>
        /// <remarks>
        /// This method preserves the EntityNotFoundException from the repository to indicate when an instructor doesn't exist, 
        /// while wrapping other exceptions in a DataOperationException.
        /// </remarks>
        public async Task<Instructor> GetInstructorByIdAsync(Guid id)
        {
            try
            {
                _logger.LogDebug("Getting instructor by ID: {InstructorId}", id);
                return await _instructorRepository.GetByIdAsync(id);
            }
            catch (EntityNotFoundException)
            {
                _logger.LogWarning("Instructor not found with ID: {InstructorId}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting instructor by ID: {InstructorId}", id);
                throw new DataOperationException($"Failed to retrieve instructor with ID {id}", ex);
            }
        }

        /// <summary>
        /// Retrieves all instructors belonging to a specific department
        /// </summary>
        /// <param name="department">The department name to filter instructors by</param>
        /// <returns>A collection of instructors in the specified department</returns>
        /// <exception cref="ArgumentException">Thrown when the department parameter is null or empty</exception>
        /// <exception cref="DataOperationException">Thrown when there's an error retrieving instructors from the repository</exception>
        /// <remarks>
        /// This method applies filtering at the repository level for better performance compared to
        /// retrieving all instructors and filtering in memory.
        /// </remarks>
        public async Task<IEnumerable<Instructor>> GetInstructorsByDepartmentAsync(string department)
        {
            if (string.IsNullOrWhiteSpace(department))
                throw new ArgumentException("Department cannot be null or empty", nameof(department));

            try
            {
                _logger.LogDebug("Getting instructors by department: {Department}", department);
                return await _instructorRepository.GetByDepartmentAsync(department);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting instructors by department: {Department}", department);
                throw new DataOperationException($"Failed to retrieve instructors for department {department}", ex);
            }
        }

        /// <summary>
        /// Adds a new instructor to the system after validation
        /// </summary>
        /// <param name="instructor">The instructor details to add</param>
        /// <returns>The added instructor with any system-generated properties set</returns>
        /// <exception cref="ArgumentNullException">Thrown when the instructor parameter is null</exception>
        /// <exception cref="ValidationException">Thrown when the instructor fails validation</exception>
        /// <exception cref="DataOperationException">Thrown when there's an error adding the instructor to the repository</exception>
        /// <remarks>
        /// This method:
        /// 1. Validates the instructor using business logic rules
        /// 2. Adds the instructor to the repository
        /// 3. Raises the InstructorChanged event with "Added" action
        /// </remarks>
        public async Task<Instructor> AddInstructorAsync(Instructor instructor)
        {
            if (instructor == null)
                throw new ArgumentNullException(nameof(instructor));

            ValidateInstructor(instructor);

            try
            {
                _logger.LogInformation("Adding new instructor: {InstructorName}", instructor.FullName);
                var addedInstructor = await _instructorRepository.AddAsync(instructor);

                // Raise event
                OnInstructorChanged(new InstructorEventArgs(
                    addedInstructor.Id,
                    addedInstructor.FullName,
                    "Added"));

                return addedInstructor;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding instructor: {InstructorName}", instructor.FullName);
                throw new DataOperationException("Failed to add instructor", ex);
            }
        }

        /// <summary>
        /// Updates an existing instructor in the system after validation
        /// </summary>
        /// <param name="instructor">The instructor with updated details</param>
        /// <returns>The updated instructor</returns>
        /// <exception cref="ArgumentNullException">Thrown when the instructor parameter is null</exception>
        /// <exception cref="ValidationException">Thrown when the instructor fails validation</exception>
        /// <exception cref="EntityNotFoundException">Thrown when the instructor to update does not exist</exception>
        /// <exception cref="DataOperationException">Thrown when there's an error updating the instructor in the repository</exception>
        /// <remarks>
        /// This method:
        /// 1. Validates the instructor data
        /// 2. Verifies the instructor exists in the system
        /// 3. Updates the instructor in the repository
        /// 4. Raises the InstructorChanged event with "Updated" action
        /// </remarks>
        public async Task<Instructor> UpdateInstructorAsync(Instructor instructor)
        {
            if (instructor == null)
                throw new ArgumentNullException(nameof(instructor));

            ValidateInstructor(instructor);

            try
            {
                // Verify the instructor exists
                await _instructorRepository.GetByIdAsync(instructor.Id);

                _logger.LogInformation("Updating instructor: {InstructorId} - {InstructorName}",
                    instructor.Id, instructor.FullName);

                var updatedInstructor = await _instructorRepository.UpdateAsync(instructor);

                // Raise event
                OnInstructorChanged(new InstructorEventArgs(
                    updatedInstructor.Id,
                    updatedInstructor.FullName,
                    "Updated"));

                return updatedInstructor;
            }
            catch (EntityNotFoundException)
            {
                _logger.LogWarning("Instructor not found for update: {InstructorId}", instructor.Id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating instructor: {InstructorId}", instructor.Id);
                throw new DataOperationException($"Failed to update instructor with ID {instructor.Id}", ex);
            }
        }

        /// <summary>
        /// Deletes an instructor from the system by ID
        /// </summary>
        /// <param name="id">The unique identifier of the instructor to delete</param>
        /// <exception cref="EntityNotFoundException">Thrown when the instructor to delete does not exist</exception>
        /// <exception cref="DataOperationException">Thrown when there's an error deleting the instructor from the repository</exception>
        /// <remarks>
        /// This method:
        /// 1. Verifies the instructor exists and retrieves their details for the event
        /// 2. Deletes the instructor from the repository
        /// 3. Raises the InstructorChanged event with "Deleted" action
        /// 
        /// Note: This method does not automatically remove the instructor from assigned courses.
        /// Such operations should be performed separately for proper cleanup.
        /// </remarks>
        public async Task DeleteInstructorAsync(Guid id)
        {
            try
            {
                // Verify the instructor exists and get its details for the event
                var instructor = await _instructorRepository.GetByIdAsync(id);

                _logger.LogInformation("Deleting instructor: {InstructorId} - {InstructorName}",
                    id, instructor.FullName);

                await _instructorRepository.DeleteAsync(id);

                // Raise event
                OnInstructorChanged(new InstructorEventArgs(
                    instructor.Id,
                    instructor.FullName,
                    "Deleted"));
            }
            catch (EntityNotFoundException)
            {
                _logger.LogWarning("Instructor not found for deletion: {InstructorId}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting instructor: {InstructorId}", id);
                throw new DataOperationException($"Failed to delete instructor with ID {id}", ex);
            }
        }

        /// <summary>
        /// Assigns an instructor to a course by updating both entities' references
        /// </summary>
        /// <param name="instructorId">The unique identifier of the instructor</param>
        /// <param name="courseId">The unique identifier of the course</param>
        /// <returns>True if the assignment was successful, false if already assigned</returns>
        /// <exception cref="EntityNotFoundException">Thrown when either the instructor or course does not exist</exception>
        /// <exception cref="DataOperationException">Thrown when there's an error updating the repositories</exception>
        /// <remarks>
        /// This method maintains the bidirectional relationship between instructors and courses by:
        /// 1. Verifying both entities exist
        /// 2. Checking if the assignment already exists to avoid duplication
        /// 3. Updating both the instructor's CourseIds collection and the course's InstructorIds collection
        /// 4. Raising the InstructorChanged event with assignment details
        /// </remarks>
        public async Task<bool> AssignInstructorToCourseAsync(Guid instructorId, Guid courseId)
        {
            try
            {
                // Verify both instructor and course exist
                var instructor = await _instructorRepository.GetByIdAsync(instructorId);
                var course = await _courseRepository.GetByIdAsync(courseId);

                // Check if the assignment already exists
                if (instructor.CourseIds.Contains(courseId) && course.InstructorIds.Contains(instructorId))
                {
                    _logger.LogWarning("Instructor {InstructorId} is already assigned to course {CourseId}",
                        instructorId, courseId);
                    return false;
                }

                // Update both instructor and course
                if (!instructor.CourseIds.Contains(courseId))
                {
                    instructor.CourseIds.Add(courseId);
                    await _instructorRepository.UpdateAsync(instructor);
                }

                if (!course.InstructorIds.Contains(instructorId))
                {
                    course.InstructorIds.Add(instructorId);
                    await _courseRepository.UpdateAsync(course);
                }
                _logger.LogInformation("Assigned instructor {InstructorName} to course {CourseCode}",
                  instructor.FullName, course.Code);

                // Raise event for both instructor and course
                OnInstructorChanged(new InstructorEventArgs(
                    instructor.Id,
                    instructor.FullName,
                    $"Assigned to course {course.Code}"));

                return true;
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning instructor {InstructorId} to course {CourseId}",
                    instructorId, courseId);
                throw new DataOperationException(
                    $"Failed to assign instructor {instructorId} to course {courseId}", ex);
            }
        }

        /// <summary>
        /// Removes an instructor's assignment from a course by updating both entities' references
        /// </summary>
        /// <param name="instructorId">The unique identifier of the instructor</param>
        /// <param name="courseId">The unique identifier of the course</param>
        /// <returns>True if the removal was successful, false if not assigned</returns>
        /// <exception cref="EntityNotFoundException">Thrown when either the instructor or course does not exist</exception>
        /// <exception cref="DataOperationException">Thrown when there's an error updating the repositories</exception>
        /// <remarks>
        /// This method maintains the bidirectional relationship between instructors and courses by:
        /// 1. Verifying both entities exist
        /// 2. Checking if the assignment exists before attempting removal
        /// 3. Updating both the instructor's CourseIds collection and the course's InstructorIds collection
        /// 4. Raising the InstructorChanged event with removal details
        /// 
        /// Note: This method does not check if the instructor is assigned to any schedules for the course.
        /// Such validations should be performed at a higher level if needed.
        /// </remarks>
        public async Task<bool> RemoveInstructorFromCourseAsync(Guid instructorId, Guid courseId)
        {
            try
            {
                // Verify both instructor and course exist
                var instructor = await _instructorRepository.GetByIdAsync(instructorId);
                var course = await _courseRepository.GetByIdAsync(courseId);

                // Check if the assignment exists
                if (!instructor.CourseIds.Contains(courseId) || !course.InstructorIds.Contains(instructorId))
                {
                    _logger.LogWarning("Instructor {InstructorId} is not assigned to course {CourseId}",
                        instructorId, courseId);
                    return false;
                }

                // Update both instructor and course
                instructor.CourseIds.Remove(courseId);
                await _instructorRepository.UpdateAsync(instructor);

                course.InstructorIds.Remove(instructorId);
                await _courseRepository.UpdateAsync(course);
                _logger.LogInformation("Removed instructor {InstructorName} from course {CourseCode}",
                  instructor.FullName, course.Code);

                // Raise event for instructor
                OnInstructorChanged(new InstructorEventArgs(
                    instructor.Id,
                    instructor.FullName,
                    $"Removed from course {course.Code}"));

                return true;
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing instructor {InstructorId} from course {CourseId}",
                    instructorId, courseId);
                throw new DataOperationException(
                    $"Failed to remove instructor {instructorId} from course {courseId}", ex);
            }
        }

        /// <summary>
        /// Retrieves all instructors assigned to a specific course
        /// </summary>
        /// <param name="courseId">The unique identifier of the course</param>
        /// <returns>A collection of instructors assigned to the specified course</returns>
        /// <exception cref="DataOperationException">Thrown when there's an error retrieving instructors from the repository</exception>
        /// <remarks>
        /// This method uses the repository's specialized query for better performance.
        /// It returns an empty collection if the course exists but has no assigned instructors.
        /// </remarks>
        public async Task<IEnumerable<Instructor>> GetInstructorsByCourseAsync(Guid courseId)
        {
            try
            {
                _logger.LogDebug("Getting instructors by course: {CourseId}", courseId);
                return await _instructorRepository.GetByCourseAsync(courseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting instructors by course: {CourseId}", courseId);
                throw new DataOperationException($"Failed to retrieve instructors for course {courseId}", ex);
            }
        }

        /// <summary>
        /// Raises the InstructorChanged event
        /// </summary>
        /// <param name="e">Event arguments containing information about the changed instructor</param>
        /// <remarks>
        /// This protected virtual method allows derived classes to override the event raising behavior
        /// while still using the same event mechanism.
        /// </remarks>
        protected virtual void OnInstructorChanged(InstructorEventArgs e)
        {
            InstructorChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Validates an instructor to ensure all required fields are provided and formatted correctly
        /// </summary>
        /// <param name="instructor">The instructor to validate</param>
        /// <exception cref="ValidationException">Thrown if validation fails with details about the failures</exception>
        /// <remarks>
        /// This method checks for:
        /// - Required fields (first name, last name, email, department)
        /// - Valid email format (basic check for @ symbol)
        /// 
        /// All validation errors are collected and returned together rather than failing on the first error.
        /// </remarks>
        private void ValidateInstructor(Instructor instructor)
        {
            var validationErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(instructor.FirstName))
                validationErrors.Add("First name is required");

            if (string.IsNullOrWhiteSpace(instructor.LastName))
                validationErrors.Add("Last name is required");

            if (string.IsNullOrWhiteSpace(instructor.Email))
                validationErrors.Add("Email is required");
            else if (!instructor.Email.Contains('@'))
                validationErrors.Add("Email must be a valid email address");

            if (string.IsNullOrWhiteSpace(instructor.Department))
                validationErrors.Add("Department is required");

            if (validationErrors.Count > 0)
            {
                throw new ValidationException(
                    "Instructor validation failed: " + string.Join(", ", validationErrors));
            }
        }
    }
}
