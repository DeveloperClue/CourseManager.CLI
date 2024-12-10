using CourseManager.CLI.Core.Exceptions;
using CourseManager.CLI.Core.Infrastructure;
using CourseManager.CLI.Core.Models;
using CourseManager.CLI.Data.Services;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Internal;

namespace CourseManager.CLI.Tests.Services
{
    /// <summary>
    /// Unit tests for the InstructorService class
    /// </summary>
    /// <remarks>
    /// These tests verify the functionality of the InstructorService including CRUD operations,
    /// instructor-course relationships, validation, and event raising capabilities.
    /// </remarks>
    [TestFixture]
    public class InstructorServiceTests
    {
        /// <summary>
        /// Mock object for the instructor repository
        /// </summary>
        private Mock<IInstructorRepository> _instructorRepositoryMock = null!;

        /// <summary>
        /// Mock object for the course repository
        /// </summary>
        private Mock<ICourseRepository> _courseRepositoryMock = null!;

        /// <summary>
        /// Mock object for the logger
        /// </summary>
        private Mock<ILogger<InstructorService>> _loggerMock = null!;

        /// <summary>
        /// Instance of the InstructorService being tested
        /// </summary>
        private InstructorService _service = null!;

        /// <summary>
        /// Sample valid instructor object used across tests
        /// </summary>
        private Instructor _validInstructor = null!;

        /// <summary>
        /// Sample valid course object used across tests
        /// </summary>
        private Course _validCourse = null!;

        /// <summary>
        /// Test GUID used for instructor identification
        /// </summary>
        private Guid _testId;

        /// <summary>
        /// Sets up the test environment before each test
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _instructorRepositoryMock = new Mock<IInstructorRepository>();
            _courseRepositoryMock = new Mock<ICourseRepository>();
            _loggerMock = new Mock<ILogger<InstructorService>>();
            _service = new InstructorService(
                _instructorRepositoryMock.Object,
                _courseRepositoryMock.Object,
                _loggerMock.Object);

            _testId = Guid.NewGuid();
            _validInstructor = new Instructor
            {
                Id = _testId,
                FirstName = "Mahi",
                LastName = "Mansoori",
                Email = "Mahi.Mansoori@example.com",
                Department = "Computer Science",
                IsFullTime = true,
                Office = "Building A, Room 101"
            };

            _validCourse = new Course
            {
                Id = Guid.NewGuid(),
                Code = "CS101",
                Title = "Introduction to Computer Science",
                Department = "Computer Science"
            };
        }

        /// <summary>
        /// Verifies that adding a valid instructor returns the added instructor and raises the InstructorChanged event
        /// </summary>
        /// <remarks>
        /// This test ensures that the AddInstructorAsync method correctly processes a valid instructor,
        /// interacts with the repository, and raises the appropriate notification event.
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that adding a valid instructor returns the added instructor and raises event")]
        public async Task AddInstructorAsync_ValidInstructor_ReturnsAddedInstructor()
        {
            // Arrange
            _instructorRepositoryMock.Setup(r => r.AddAsync(_validInstructor))
                .ReturnsAsync(_validInstructor);

            var eventRaised = false;
            _service.InstructorChanged += (sender, e) =>
            {
                eventRaised = true;
            };

            // Act
            var result = await _service.AddInstructorAsync(_validInstructor);

            // Assert
            Assert.That(result, Is.EqualTo(_validInstructor));
            Assert.That(eventRaised, Is.True, "InstructorChanged event should be raised");
            _instructorRepositoryMock.Verify(r => r.AddAsync(_validInstructor), Times.Once);
        }

        /// <summary>
        /// Verifies that attempting to add a null instructor throws an ArgumentNullException
        /// </summary>
        /// <remarks>
        /// This test ensures that proper null validation is performed before processing an instructor
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that adding a null instructor throws ArgumentNullException")]
        public void AddInstructorAsync_NullInstructor_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _service.AddInstructorAsync(null!));
        }

        /// <summary>
        /// Verifies that attempting to add an invalid instructor throws a ValidationException with appropriate error messages
        /// </summary>
        /// <remarks>
        /// This test ensures that business validation rules are correctly enforced when adding an instructor
        /// and that validation error messages are properly aggregated in the exception
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that adding an invalid instructor throws ValidationException with correct messages")]
        public void AddInstructorAsync_InvalidInstructor_ThrowsValidationException()
        {            // Arrange
            var invalidInstructor = new Instructor
            {
                Id = Guid.NewGuid(),
                FirstName = "",
                LastName = "",
                Email = "invalid-email",
                Department = ""
            };

            // Act & Assert            
            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _service.AddInstructorAsync(invalidInstructor));

            // Ensure ex is not null before checking its properties
            Assert.That(ex, Is.Not.Null);

            // Verify validation errors are in the message
            Assert.That(ex!.Message, Does.Contain("First name is required"));
            Assert.That(ex.Message, Does.Contain("Last name is required"));
            Assert.That(ex.Message, Does.Contain("valid email address"));
            Assert.That(ex.Message, Does.Contain("Department is required"));
        }

        /// <summary>
        /// Verifies that retrieving an instructor by an existing ID returns the correct instructor
        /// </summary>
        /// <remarks>
        /// This test ensures that the GetInstructorByIdAsync method correctly retrieves an instructor when given a valid ID
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that getting an instructor by existing ID returns the correct instructor")]
        public async Task GetInstructorByIdAsync_ExistingId_ReturnsInstructor()
        {
            // Arrange
            _instructorRepositoryMock.Setup(r => r.GetByIdAsync(_testId))
                .ReturnsAsync(_validInstructor);

            // Act
            var result = await _service.GetInstructorByIdAsync(_testId);

            // Assert
            Assert.That(result, Is.EqualTo(_validInstructor));
        }

        /// <summary>
        /// Verifies that attempting to get a non-existent instructor by ID throws EntityNotFoundException
        /// </summary>
        /// <remarks>
        /// This test ensures proper error handling when requesting an instructor that doesn't exist in the repository
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that requesting a non-existent instructor throws EntityNotFoundException")]
        public void GetInstructorByIdAsync_NotFound_ThrowsEntityNotFoundException()
        {
            // Arrange
            _instructorRepositoryMock.Setup(r => r.GetByIdAsync(_testId))
                .ThrowsAsync(new EntityNotFoundException("Instructor", _testId.ToString()));

            // Act & Assert
            Assert.ThrowsAsync<EntityNotFoundException>(async () =>
                await _service.GetInstructorByIdAsync(_testId));
        }

        /// <summary>
        /// Verifies that updating a valid instructor returns the updated instructor and raises the InstructorChanged event
        /// </summary>
        /// <remarks>
        /// This test ensures that the UpdateInstructorAsync method correctly processes updates,
        /// interacts with the repository, and notifies subscribers of changes
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that updating a valid instructor returns the updated instructor and raises event")]
        public async Task UpdateInstructorAsync_ValidInstructor_ReturnsUpdatedInstructor()
        {
            // Arrange
            _instructorRepositoryMock.Setup(r => r.GetByIdAsync(_validInstructor.Id))
                .ReturnsAsync(_validInstructor);
            _instructorRepositoryMock.Setup(r => r.UpdateAsync(_validInstructor))
                .ReturnsAsync(_validInstructor);

            var eventRaised = false;
            _service.InstructorChanged += (sender, e) =>
            {
                eventRaised = true;
            };

            // Act
            var result = await _service.UpdateInstructorAsync(_validInstructor);

            // Assert
            Assert.That(result, Is.EqualTo(_validInstructor));
            Assert.That(eventRaised, Is.True, "InstructorChanged event should be raised");
            _instructorRepositoryMock.Verify(r => r.UpdateAsync(_validInstructor), Times.Once);
        }

        /// <summary>
        /// Verifies that attempting to update a null instructor throws an ArgumentNullException
        /// </summary>
        /// <remarks>
        /// This test ensures that proper null validation is performed before processing an instructor update
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that updating a null instructor throws ArgumentNullException")]
        public void UpdateInstructorAsync_NullInstructor_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _service.UpdateInstructorAsync(null!));
        }

        /// <summary>
        /// Verifies that attempting to update a non-existent instructor throws EntityNotFoundException
        /// </summary>
        /// <remarks>
        /// This test ensures that the service validates the instructor exists before attempting to update it
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that updating a non-existent instructor throws EntityNotFoundException")]
        public void UpdateInstructorAsync_InstructorNotFound_ThrowsEntityNotFoundException()
        {
            // Arrange
            _instructorRepositoryMock.Setup(r => r.GetByIdAsync(_validInstructor.Id))
                .ThrowsAsync(new EntityNotFoundException("Instructor", _validInstructor.Id.ToString()));

            // Act & Assert
            Assert.ThrowsAsync<EntityNotFoundException>(async () =>
                await _service.UpdateInstructorAsync(_validInstructor));
        }

        /// <summary>
        /// Verifies that deleting an existing instructor raises the InstructorChanged event and calls the repository
        /// </summary>
        /// <remarks>
        /// This test ensures that the DeleteInstructorAsync method correctly processes deletion
        /// and notifies subscribers of changes with the appropriate action type
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that deleting an instructor raises event and calls repository correctly")]
        public async Task DeleteInstructorAsync_ExistingId_DeletesInstructor()
        {
            // Arrange
            _instructorRepositoryMock.Setup(r => r.GetByIdAsync(_testId))
                .ReturnsAsync(_validInstructor);

            var eventRaised = false;
            _service.InstructorChanged += (sender, e) =>
            {
                eventRaised = true;
                Assert.That(e.Action, Is.EqualTo("Deleted"));
            };

            // Act
            await _service.DeleteInstructorAsync(_testId);

            // Assert
            Assert.That(eventRaised, Is.True, "InstructorChanged event should be raised");
            _instructorRepositoryMock.Verify(r => r.DeleteAsync(_testId), Times.Once);
        }

        /// <summary>
        /// Verifies that attempting to delete a non-existent instructor throws EntityNotFoundException
        /// </summary>
        /// <remarks>
        /// This test ensures that the service validates the instructor exists before attempting to delete it
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that deleting a non-existent instructor throws EntityNotFoundException")]
        public void DeleteInstructorAsync_NotFound_ThrowsEntityNotFoundException()
        {
            // Arrange
            _instructorRepositoryMock.Setup(r => r.GetByIdAsync(_testId))
                .ThrowsAsync(new EntityNotFoundException("Instructor", _testId.ToString()));

            // Act & Assert
            Assert.ThrowsAsync<EntityNotFoundException>(async () =>
                await _service.DeleteInstructorAsync(_testId));
        }

        /// <summary>
        /// Verifies that assigning an instructor to a course updates both entities correctly
        /// </summary>
        /// <remarks>
        /// This test ensures that the AssignInstructorToCourseAsync method properly updates the relationship
        /// between instructors and courses, modifying both entities and persisting changes through both repositories
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that instructor-course assignment updates both entities correctly")]
        public async Task AssignInstructorToCourseAsync_ValidAssignment_UpdatesBothEntities()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var instructorWithoutCourse = new Instructor
            {
                Id = _testId,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                Department = "Computer Science",
                CourseIds = new List<Guid>()
            };

            var courseWithoutInstructor = new Course
            {
                Id = courseId,
                Code = "CS102",
                Title = "Advanced Programming",
                Department = "Computer Science",
                InstructorIds = new List<Guid>()
            };

            _instructorRepositoryMock.Setup(r => r.GetByIdAsync(_testId))
                .ReturnsAsync(instructorWithoutCourse);
            _courseRepositoryMock.Setup(r => r.GetByIdAsync(courseId))
                .ReturnsAsync(courseWithoutInstructor);

            var instructorAfterUpdate = new Instructor
            {
                Id = _testId,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                Department = "Computer Science",
                CourseIds = new List<Guid> { courseId }
            };

            var courseAfterUpdate = new Course
            {
                Id = courseId,
                Code = "CS102",
                Title = "Advanced Programming",
                Department = "Computer Science",
                InstructorIds = new List<Guid> { _testId }
            };

            _instructorRepositoryMock.Setup(r => r.UpdateAsync(It.Is<Instructor>(i => i.CourseIds.Contains(courseId))))
                .ReturnsAsync(instructorAfterUpdate);
            _courseRepositoryMock.Setup(r => r.UpdateAsync(It.Is<Course>(c => c.InstructorIds.Contains(_testId))))
                .ReturnsAsync(courseAfterUpdate);

            var eventRaised = false;
            _service.InstructorChanged += (sender, e) =>
            {
                eventRaised = true;
            };

            // Act
            var result = await _service.AssignInstructorToCourseAsync(_testId, courseId);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(eventRaised, Is.True, "InstructorChanged event should be raised");
            _instructorRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Instructor>(i => i.CourseIds.Contains(courseId))), Times.Once);
            _courseRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Course>(c => c.InstructorIds.Contains(_testId))), Times.Once);
        }

        /// <summary>
        /// Verifies that attempting to assign an instructor to a course they're already assigned to returns false
        /// </summary>
        /// <remarks>
        /// This test ensures that the AssignInstructorToCourseAsync method avoids redundant assignments
        /// and doesn't make unnecessary repository calls when the relationship already exists
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that trying to assign an already-assigned instructor-course pair returns false")]
        public async Task AssignInstructorToCourseAsync_AlreadyAssigned_ReturnsFalse()
        {
            // Arrange
            var courseId = _validCourse.Id;
            var instructorId = _validInstructor.Id;

            // Create a clean setup with already assigned instructor/course
            var instructorWithCourse = new Instructor
            {
                Id = instructorId,
                FirstName = "Mahi",
                LastName = "Mansoori",
                Email = "Mahi.Mansoori@example.com",
                Department = "Computer Science",
                CourseIds = new List<Guid> { courseId }
            };

            var courseWithInstructor = new Course
            {
                Id = courseId,
                Code = "CS101",
                Title = "Introduction to Computer Science",
                Credits = 3,
                Department = "Computer Science",
                InstructorIds = new List<Guid> { instructorId }
            };

            _instructorRepositoryMock.Setup(r => r.GetByIdAsync(instructorId))
                .ReturnsAsync(instructorWithCourse);
            _courseRepositoryMock.Setup(r => r.GetByIdAsync(courseId))
                .ReturnsAsync(courseWithInstructor);

            // Act
            var result = await _service.AssignInstructorToCourseAsync(instructorId, courseId);

            // Assert
            Assert.That(result, Is.False);
            _instructorRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Instructor>()), Times.Never);
            _courseRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Course>()), Times.Never);
        }

        /// <summary>
        /// Verifies that removing an instructor from a course updates both entities correctly
        /// </summary>
        /// <remarks>
        /// This test ensures that the RemoveInstructorFromCourseAsync method properly updates the relationship
        /// between instructors and courses, removing the link from both entities and persisting changes
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that removing an instructor from a course updates both entities")]
        public async Task RemoveInstructorFromCourseAsync_ValidRemoval_UpdatesBothEntities()
        {
            // Arrange
            var instructorId = _testId;
            var courseId = _validCourse.Id;

            // Create instructor and course with the association
            var instructorWithCourse = new Instructor
            {
                Id = instructorId,
                FirstName = "Mahi",
                LastName = "Mansoori",
                Email = "Mahi.Mansoori@example.com",
                Department = "Computer Science",
                CourseIds = new List<Guid> { courseId }
            };

            var courseWithInstructor = new Course
            {
                Id = courseId,
                Code = "CS101",
                Title = "Introduction to Computer Science",
                Department = "Computer Science",
                InstructorIds = new List<Guid> { instructorId }
            };

            _instructorRepositoryMock.Setup(r => r.GetByIdAsync(instructorId))
                .ReturnsAsync(instructorWithCourse);
            _courseRepositoryMock.Setup(r => r.GetByIdAsync(courseId))
                .ReturnsAsync(courseWithInstructor);

            var instructorAfterUpdate = new Instructor
            {
                Id = instructorId,
                FirstName = "Mahi",
                LastName = "Mansoori",
                Email = "Mahi.Mansoori@example.com",
                Department = "Computer Science",
                CourseIds = new List<Guid>()
            };

            var courseAfterUpdate = new Course
            {
                Id = courseId,
                Code = "CS101",
                Title = "Introduction to Computer Science",
                Department = "Computer Science",
                InstructorIds = new List<Guid>()
            };

            _instructorRepositoryMock.Setup(r => r.UpdateAsync(It.Is<Instructor>(i => !i.CourseIds.Contains(courseId))))
                .ReturnsAsync(instructorAfterUpdate);
            _courseRepositoryMock.Setup(r => r.UpdateAsync(It.Is<Course>(c => !c.InstructorIds.Contains(instructorId))))
                .ReturnsAsync(courseAfterUpdate);

            var eventRaised = false;
            _service.InstructorChanged += (sender, e) =>
            {
                eventRaised = true;
            };

            // Act
            var result = await _service.RemoveInstructorFromCourseAsync(instructorId, courseId);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(eventRaised, Is.True, "InstructorChanged event should be raised");
            _instructorRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Instructor>(i => !i.CourseIds.Contains(courseId))), Times.Once);
            _courseRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Course>(c => !c.InstructorIds.Contains(instructorId))), Times.Once);
        }

        /// <summary>
        /// Verifies that attempting to remove an instructor from a course they're not assigned to causes no changes
        /// </summary>
        /// <remarks>
        /// This test ensures that the RemoveInstructorFromCourseAsync method handles non-existent relationships gracefully
        /// and doesn't make unnecessary repository calls when the relationship doesn't exist
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that removing an instructor from a course they're not assigned to causes no changes")]
        public async Task RemoveInstructorFromCourseAsync_NotAssigned_NoChanges()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var instructorWithoutCourse = new Instructor
            {
                Id = _testId,
                FirstName = "Mahi",
                LastName = "Mansoori",
                Email = "Mahi.Mansoori@example.com",
                Department = "Computer Science",
                CourseIds = new List<Guid>()
            };

            var courseWithoutInstructor = new Course
            {
                Id = courseId,
                Code = "CS102",
                Title = "Advanced Programming",
                Department = "Computer Science",
                InstructorIds = new List<Guid>()
            };

            _instructorRepositoryMock.Setup(r => r.GetByIdAsync(_testId))
                .ReturnsAsync(instructorWithoutCourse);
            _courseRepositoryMock.Setup(r => r.GetByIdAsync(courseId))
                .ReturnsAsync(courseWithoutInstructor);

            // Act
            await _service.RemoveInstructorFromCourseAsync(_testId, courseId);

            // Assert
            _instructorRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Instructor>()), Times.Never);
            _courseRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Course>()), Times.Never);
        }

        /// <summary>
        /// Verifies that instructors assigned to a specific course can be retrieved correctly
        /// </summary>
        /// <remarks>
        /// This test ensures that the GetInstructorsByCourseAsync method correctly interacts with the repository
        /// to retrieve instructors associated with a specific course
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that instructors assigned to a specific course can be retrieved")]
        public async Task GetInstructorsByCourseAsync_ValidCourseId_ReturnsInstructors()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var instructors = new List<Instructor> { _validInstructor };

            _instructorRepositoryMock.Setup(r => r.GetByCourseAsync(courseId))
                .ReturnsAsync(instructors);

            // Act
            var result = await _service.GetInstructorsByCourseAsync(courseId);

            // Assert
            Assert.That(result, Is.EqualTo(instructors));
        }

        /// <summary>
        /// Verifies that retrieving all instructors returns the complete collection from the repository
        /// </summary>
        /// <remarks>
        /// This test ensures that the GetAllInstructorsAsync method correctly retrieves and returns
        /// all instructors from the repository
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that all instructors can be retrieved")]
        public async Task GetAllInstructorsAsync_ReturnsAllInstructors()
        {
            // Arrange
            var instructors = new List<Instructor> { _validInstructor, new Instructor { Id = Guid.NewGuid() } };

            _instructorRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(instructors);

            // Act
            var result = await _service.GetAllInstructorsAsync();

            // Assert
            Assert.That(result, Is.EqualTo(instructors));
        }

        /// <summary>
        /// Verifies that instructors can be correctly filtered by department
        /// </summary>
        /// <remarks>
        /// This test ensures that the GetInstructorsByDepartmentAsync method correctly interacts with the repository
        /// to retrieve only instructors from the specified department
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that instructors can be filtered by department")]
        public async Task GetInstructorsByDepartmentAsync_ValidDepartment_ReturnsInstructors()
        {
            // Arrange
            var department = "Computer Science";
            var instructors = new List<Instructor> { _validInstructor };

            _instructorRepositoryMock.Setup(r => r.GetByDepartmentAsync(department))
                .ReturnsAsync(instructors);

            // Act
            var result = await _service.GetInstructorsByDepartmentAsync(department);

            // Assert
            Assert.That(result, Is.EqualTo(instructors));
        }

        /// <summary>
        /// Verifies that providing null or empty department names throws ArgumentException
        /// </summary>
        /// <remarks>
        /// This test ensures proper input validation when filtering instructors by department,
        /// checking for null, empty string, and whitespace-only department names
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that null or empty department parameter throws ArgumentException")]
        public void GetInstructorsByDepartmentAsync_NullOrEmptyDepartment_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _service.GetInstructorsByDepartmentAsync(null!));

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _service.GetInstructorsByDepartmentAsync(string.Empty));

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _service.GetInstructorsByDepartmentAsync("   "));
        }
    }
}