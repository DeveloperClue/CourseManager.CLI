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
    /// Unit tests for the CourseService class
    /// </summary>
    /// <remarks>
    /// These tests verify the functionality of the CourseService including CRUD operations,
    /// validation, and event raising capabilities.
    /// </remarks>    
    [TestFixture]
    public class CourseServiceTests
    {
        /// <summary>
        /// Mock object for the course repository
        /// </summary>
        private Mock<ICourseRepository> _courseRepositoryMock = null!;

        /// <summary>
        /// Mock object for the logger
        /// </summary>
        private Mock<ILogger<CourseService>> _loggerMock = null!;

        /// <summary>
        /// Instance of the CourseService being tested
        /// </summary>
        private CourseService _service = null!;

        /// <summary>
        /// Sample valid course object used across tests
        /// </summary>
        private Course _validCourse = null!;

        /// <summary>
        /// Test GUID used for course identification
        /// </summary>
        private Guid _testId;

        /// <summary>
        /// Sets up the test environment before each test
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _courseRepositoryMock = new Mock<ICourseRepository>();
            _loggerMock = new Mock<ILogger<CourseService>>();
            _service = new CourseService(_courseRepositoryMock.Object, _loggerMock.Object);

            _testId = Guid.NewGuid();
            _validCourse = new Course
            {
                Id = _testId,
                Code = "CS101",
                Title = "Introduction to Computer Science",
                Credits = 3,
                MaxEnrollment = 30,
                Department = "Computer Science",
                Description = "This is a test course"
            };
        }

        /// <summary>
        /// Verifies that adding a valid course returns the added course and raises the CourseChanged event
        /// </summary>
        /// <remarks>
        /// This test ensures that the AddCourseAsync method correctly processes a valid course,
        /// interacts with the repository, and raises the appropriate notification event.
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that adding a valid course returns the added course")]
        public async Task AddCourseAsync_ValidCourse_ReturnsAddedCourse()
        {
            // Arrange
            _courseRepositoryMock.Setup(r => r.GetByCodeAsync(_validCourse.Code))
                .ThrowsAsync(new EntityNotFoundException("Course", _validCourse.Code));
            _courseRepositoryMock.Setup(r => r.AddAsync(_validCourse)).ReturnsAsync(_validCourse);

            var eventRaised = false;
            _service.CourseChanged += (sender, e) =>
            {
                eventRaised = true;
            };

            // Act
            var result = await _service.AddCourseAsync(_validCourse);

            // Assert
            Assert.That(result, Is.EqualTo(_validCourse));
            Assert.That(eventRaised, Is.True, "CourseChanged event should be raised");
            _courseRepositoryMock.Verify(r => r.AddAsync(_validCourse), Times.Once);
        }

        /// <summary>
        /// Verifies that attempting to add a null course throws an ArgumentNullException
        /// </summary>
        /// <remarks>
        /// This test ensures that proper null validation is performed before processing a course
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that adding a null course throws ArgumentNullException")]
        public void AddCourseAsync_NullCourse_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _service.AddCourseAsync(null!));
        }

        /// <summary>
        /// Verifies that attempting to add an invalid course throws a ValidationException
        /// </summary>
        /// <remarks>
        /// This test ensures that business validation rules are correctly enforced when adding a course
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that adding an invalid course throws ValidationException")]
        public void AddCourseAsync_InvalidCourse_ThrowsValidationException()
        {
            var course = new Course { Code = "", Title = "", Credits = 0, MaxEnrollment = 0, Department = "" };
            Assert.ThrowsAsync<ValidationException>(async () => await _service.AddCourseAsync(course));
        }

        /// <summary>
        /// Verifies that retrieving a course by an existing ID returns the correct course
        /// </summary>
        /// <remarks>
        /// This test ensures that the GetCourseByIdAsync method correctly retrieves a course when given a valid ID
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that getting a course by existing ID returns the correct course")]
        public async Task GetCourseByIdAsync_ExistingId_ReturnsCourse()
        {
            var id = Guid.NewGuid();
            var course = new Course { Id = id, Code = "CS101", Title = "Intro", Credits = 3, MaxEnrollment = 30, Department = "CS" };
            _courseRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(course);

            var result = await _service.GetCourseByIdAsync(id);

            Assert.That(result, Is.EqualTo(course));
        }

        /// <summary>
        /// Verifies that attempting to get a non-existent course by ID throws EntityNotFoundException
        /// </summary>
        /// <remarks>
        /// This test ensures proper error handling when requesting a course that doesn't exist in the repository
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that getting a non-existent course by ID throws EntityNotFoundException")]
        public void GetCourseByIdAsync_NotFound_ThrowsEntityNotFoundException()
        {
            var id = Guid.NewGuid();
            _courseRepositoryMock.Setup(r => r.GetByIdAsync(id)).ThrowsAsync(new EntityNotFoundException("Course", id.ToString()));
            Assert.ThrowsAsync<EntityNotFoundException>(async () => await _service.GetCourseByIdAsync(id));
        }

        /// <summary>
        /// Verifies that retrieving a course by an existing code returns the correct course
        /// </summary>
        /// <remarks>
        /// This test ensures that the GetCourseByCodeAsync method correctly interacts with the repository
        /// to retrieve a course by its unique code
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that getting a course by existing code returns the correct course")]
        public async Task GetCourseByCodeAsync_ExistingCode_ReturnsCourse()
        {
            // Arrange
            _courseRepositoryMock.Setup(r => r.GetByCodeAsync(_validCourse.Code))
                .ReturnsAsync(_validCourse);

            // Act
            var result = await _service.GetCourseByCodeAsync(_validCourse.Code);

            // Assert
            Assert.That(result, Is.EqualTo(_validCourse));
        }

        /// <summary>
        /// Verifies that updating a valid course returns the updated course and raises the CourseChanged event
        /// </summary>
        /// <remarks>
        /// This test ensures that the UpdateCourseAsync method correctly processes updates, 
        /// interacts with the repository, and notifies subscribers of changes
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that updating a valid course returns the updated course and raises event")]
        public async Task UpdateCourseAsync_ValidCourse_ReturnsUpdatedCourse()
        {
            // Arrange
            _courseRepositoryMock.Setup(r => r.GetByIdAsync(_validCourse.Id))
                .ReturnsAsync(_validCourse);
            _courseRepositoryMock.Setup(r => r.GetByCodeAsync(_validCourse.Code))
                .ThrowsAsync(new EntityNotFoundException("Course", _validCourse.Code));
            _courseRepositoryMock.Setup(r => r.UpdateAsync(_validCourse))
                .ReturnsAsync(_validCourse);

            var eventRaised = false;
            _service.CourseChanged += (sender, e) =>
            {
                eventRaised = true;
            };

            // Act
            var result = await _service.UpdateCourseAsync(_validCourse);

            // Assert
            Assert.That(result, Is.EqualTo(_validCourse));
            Assert.That(eventRaised, Is.True, "Event should be raised on update");
            _courseRepositoryMock.Verify(r => r.UpdateAsync(_validCourse), Times.Once);
        }

        /// <summary>
        /// Verifies that deleting an existing course raises the CourseChanged event and calls the repository
        /// </summary>
        /// <remarks>
        /// This test ensures that the DeleteCourseAsync method correctly processes deletion
        /// and notifies subscribers of changes
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that deleting an existing course raises event and calls repository")]
        public async Task DeleteCourseAsync_ExistingCourse_DeletesSuccessfully()
        {
            // Arrange
            _courseRepositoryMock.Setup(r => r.GetByIdAsync(_testId))
                .ReturnsAsync(_validCourse);
            _courseRepositoryMock.Setup(r => r.DeleteAsync(_testId))
                .Returns(Task.CompletedTask);

            var eventRaised = false;
            _service.CourseChanged += (sender, e) =>
            {
                eventRaised = true;
            };

            // Act
            await _service.DeleteCourseAsync(_testId);

            // Assert
            Assert.That(eventRaised, Is.True, "Event should be raised on delete");
            _courseRepositoryMock.Verify(r => r.DeleteAsync(_testId), Times.Once);
        }

        /// <summary>
        /// Verifies that retrieving all courses returns the complete collection from the repository
        /// </summary>
        /// <remarks>
        /// This test ensures that the GetAllCoursesAsync method correctly retrieves and returns 
        /// all courses from the repository
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that getting all courses returns the complete collection")]
        public async Task GetAllCoursesAsync_ReturnsAllCourses()
        {
            // Arrange
            var courses = new List<Course> { _validCourse, new Course { Id = Guid.NewGuid(), Code = "CS102" } };
            _courseRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(courses);

            // Act
            var result = await _service.GetAllCoursesAsync();

            // Assert            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(courses.Count));
            Assert.That(result, Is.EquivalentTo(courses));
        }

        /// <summary>
        /// Verifies that courses can be correctly filtered by department
        /// </summary>
        /// <remarks>
        /// This test ensures that the GetCoursesByDepartmentAsync method correctly interacts with the repository
        /// to retrieve only courses from the specified department
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that courses can be filtered by department")]
        public async Task GetCoursesByDepartmentAsync_ReturnsFilteredCourses()
        {
            // Arrange
            string department = "Computer Science";
            var courses = new List<Course> { _validCourse };
            _courseRepositoryMock.Setup(r => r.GetByDepartmentAsync(department))
                .ReturnsAsync(courses);

            // Act
            var result = await _service.GetCoursesByDepartmentAsync(department);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First(), Is.EqualTo(_validCourse));
        }

        /// <summary>
        /// Verifies that providing null or empty department names throws ArgumentException
        /// </summary>
        /// <remarks>
        /// This test ensures proper input validation when filtering courses by department
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that null or empty department names throw ArgumentException")]
        public void GetCoursesByDepartmentAsync_NullDepartment_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.GetCoursesByDepartmentAsync(null!));
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.GetCoursesByDepartmentAsync(""));
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.GetCoursesByDepartmentAsync("   "));
        }

        /// <summary>
        /// Verifies that courses can be correctly filtered by instructor
        /// </summary>
        /// <remarks>
        /// This test ensures that the GetCoursesByInstructorAsync method correctly interacts with the repository
        /// to retrieve only courses taught by the specified instructor
        /// </remarks>
        [Test]
        [Author("Mahinuddin")]
        [Description("Verifies that courses can be filtered by instructor")]
        public async Task GetCoursesByInstructorAsync_ReturnsInstructorCourses()
        {
            // Arrange
            var instructorId = Guid.NewGuid();
            var courses = new List<Course> { _validCourse };
            _courseRepositoryMock.Setup(r => r.GetByInstructorAsync(instructorId))
                .ReturnsAsync(courses);

            // Act
            var result = await _service.GetCoursesByInstructorAsync(instructorId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First(), Is.EqualTo(_validCourse));
        }
    }
}
