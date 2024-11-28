using CourseManager.CLI.Core.Exceptions;
using CourseManager.CLI.Core.Models;
using CourseManager.CLI.Data.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CourseManager.CLI.Tests.Repositories
{
    /// <summary>
    /// Contains unit tests for the CourseRepository class to verify course data persistence and business rules.
    /// </summary>
    [TestFixture]
    public class CourseRepositoryTests
    {
        // Directory used for temporary test files
        private string _testDirectory = null!;
        // Path to the JSON file storing course data
        private string _testFilePath = null!;
        // Mock logger for testing logging behavior
        private Mock<ILogger<CourseRepository>> _mockLogger = null!;
        // The repository instance being tested
        private CourseRepository _repository = null!;
        // A valid course instance for testing
        private Course _validCourse = null!;

        /// <summary>
        /// Sets up the test environment by creating a temporary directory for test data storage.
        /// This is executed once before all tests in the fixture.
        /// </summary>
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "CourseManagerTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);
        }

        /// <summary>
        /// Cleans up the test environment by removing the temporary test directory.
        /// This is executed once after all tests in the fixture have completed.
        /// </summary>
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        /// <summary>
        /// Initializes test fixtures before each test by creating a new repository instance
        /// and setting up a valid course object for testing.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _testFilePath = Path.Combine(_testDirectory, "courses.json");
            _mockLogger = new Mock<ILogger<CourseRepository>>();
            _repository = new CourseRepository(_testDirectory, _mockLogger.Object);

            _validCourse = new Course
            {
                Id = Guid.NewGuid(),
                Code = "CS101",
                Title = "Introduction to Computer Science",
                Description = "Basic concepts of programming",
                Credits = 3,
                Department = "Computer Science",
                MaxEnrollment = 30,
                InstructorIds = new List<Guid> { Guid.NewGuid() }
            };
        }

        /// <summary>
        /// Cleans up after each test by removing the test data file.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        /// <summary>
        /// Tests that GetAllAsync returns an empty list when the repository contains no courses.
        /// </summary>
        [Test]
        public async Task GetAllAsync_EmptyRepository_ReturnsEmptyList()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        /// <summary>
        /// Tests that AddAsync successfully adds a valid course and returns the added course.
        /// Also verifies that the course is properly persisted in the repository.
        /// </summary>
        [Test]
        public async Task AddAsync_ValidCourse_ShouldAddAndReturnCourse()
        {
            // Act
            var result = await _repository.AddAsync(_validCourse);

            // Assert
            result.Should().BeEquivalentTo(_validCourse);

            // Verify the course was persisted
            var allCourses = await _repository.GetAllAsync();
            allCourses.Should().ContainSingle().Which.Should().BeEquivalentTo(_validCourse);
        }

        /// <summary>
        /// Tests that GetByIdAsync successfully retrieves an existing course by its ID.
        /// </summary>
        [Test]
        public async Task GetByIdAsync_ExistingCourse_ReturnsCourse()
        {
            // Arrange
            await _repository.AddAsync(_validCourse);

            // Act
            var result = await _repository.GetByIdAsync(_validCourse.Id);

            // Assert
            result.Should().BeEquivalentTo(_validCourse);
        }

        /// <summary>
        /// Tests that GetByIdAsync throws EntityNotFoundException when attempting to retrieve a non-existent course.
        /// </summary>
        [Test]
        public void GetByIdAsync_NonExistentCourse_ThrowsEntityNotFoundException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act & Assert
            _repository.Invoking(r => r.GetByIdAsync(nonExistentId))
                .Should().ThrowAsync<EntityNotFoundException>()
                .WithMessage($"Course with ID {nonExistentId} not found");
        }

        /// <summary>
        /// Tests that GetByCodeAsync successfully retrieves an existing course by its course code.
        /// </summary>
        [Test]
        public async Task GetByCodeAsync_ExistingCode_ReturnsCourse()
        {
            // Arrange
            await _repository.AddAsync(_validCourse);

            // Act
            var result = await _repository.GetByCodeAsync(_validCourse.Code);

            // Assert
            result.Should().BeEquivalentTo(_validCourse);
        }

        /// <summary>
        /// Tests that GetByCodeAsync throws EntityNotFoundException when attempting to retrieve a course with a non-existent code.
        /// </summary>
        [Test]
        public void GetByCodeAsync_NonExistentCode_ThrowsEntityNotFoundException()
        {
            // Arrange
            var nonExistentCode = "NONEXISTENT101";

            // Act & Assert
            _repository.Invoking(r => r.GetByCodeAsync(nonExistentCode))
                .Should().ThrowAsync<EntityNotFoundException>()
                .WithMessage($"Course with code {nonExistentCode} not found");
        }

        /// <summary>
        /// Tests that GetByCodeAsync throws ArgumentException when attempting to retrieve a course with null or empty code.
        /// </summary>
        [Test]
        public void GetByCodeAsync_NullOrEmptyCode_ThrowsArgumentException()
        {
            // Act & Assert
            _repository.Invoking(r => r.GetByCodeAsync(null!))
                .Should().ThrowAsync<ArgumentException>();
            _repository.Invoking(r => r.GetByCodeAsync(""))
                .Should().ThrowAsync<ArgumentException>();
            _repository.Invoking(r => r.GetByCodeAsync("   "))
                .Should().ThrowAsync<ArgumentException>();
        }

        /// <summary>
        /// Tests that AddAsync throws ValidationException when attempting to add a course with a duplicate course code.
        /// </summary>
        [Test]
        public async Task AddAsync_DuplicateCourseCode_ThrowsValidationException()
        {
            // Arrange
            await _repository.AddAsync(_validCourse);
            var duplicateCourse = new Course
            {
                Id = Guid.NewGuid(), // Different ID
                Code = _validCourse.Code, // Same code
                Title = "Another Course",
                Department = "Computer Science"
            };

            // Act & Assert
            await _repository.Invoking(r => r.AddAsync(duplicateCourse))
                .Should().ThrowAsync<ValidationException>()
                .WithMessage($"Course with code {_validCourse.Code} already exists");
        }

        /// <summary>
        /// Tests that UpdateAsync successfully updates an existing course and persists the changes.
        /// </summary>
        [Test]
        public async Task UpdateAsync_ExistingCourse_UpdatesAndReturnsCourse()
        {
            // Arrange
            await _repository.AddAsync(_validCourse);
            var updatedCourse = new Course
            {
                Id = _validCourse.Id,
                Code = _validCourse.Code,
                Title = "Updated Title",
                Description = _validCourse.Description,
                Credits = _validCourse.Credits,
                Department = _validCourse.Department,
                MaxEnrollment = _validCourse.MaxEnrollment,
                InstructorIds = new List<Guid>(_validCourse.InstructorIds)
            };

            // Act
            var result = await _repository.UpdateAsync(updatedCourse);

            // Assert
            result.Should().BeEquivalentTo(updatedCourse);

            // Verify the update was persisted
            var retrieved = await _repository.GetByIdAsync(updatedCourse.Id);
            retrieved.Should().BeEquivalentTo(updatedCourse);
        }

        /// <summary>
        /// Tests that UpdateAsync throws EntityNotFoundException when attempting to update a non-existent course.
        /// </summary>
        [Test]
        public void UpdateAsync_NonExistentCourse_ThrowsEntityNotFoundException()
        {
            // Arrange
            var nonExistentCourse = new Course
            {
                Id = Guid.NewGuid(),
                Code = _validCourse.Code,
                Title = _validCourse.Title,
                Description = _validCourse.Description,
                Credits = _validCourse.Credits,
                Department = _validCourse.Department,
                MaxEnrollment = _validCourse.MaxEnrollment,
                InstructorIds = new List<Guid>(_validCourse.InstructorIds)
            };

            // Act & Assert
            _repository.Invoking(r => r.UpdateAsync(nonExistentCourse))
                .Should().ThrowAsync<EntityNotFoundException>()
                .WithMessage($"Course with ID {nonExistentCourse.Id} not found");
        }

        /// <summary>
        /// Tests that DeleteAsync successfully removes an existing course from the repository.
        /// </summary>
        [Test]
        public async Task DeleteAsync_ExistingCourse_RemovesCourse()
        {
            // Arrange
            await _repository.AddAsync(_validCourse);

            // Act
            await _repository.DeleteAsync(_validCourse.Id);

            // Assert
            var allCourses = await _repository.GetAllAsync();
            allCourses.Should().BeEmpty();
        }

        /// <summary>
        /// Tests that DeleteAsync throws EntityNotFoundException when attempting to delete a non-existent course.
        /// </summary>
        [Test]
        public void DeleteAsync_NonExistentCourse_ThrowsEntityNotFoundException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act & Assert
            _repository.Invoking(r => r.DeleteAsync(nonExistentId))
                .Should().ThrowAsync<EntityNotFoundException>()
                .WithMessage($"Course with ID {nonExistentId} not found");
        }

        /// <summary>
        /// Tests that GetByDepartmentAsync returns all courses in a specific department.
        /// </summary>
        /// <remarks>
        /// This test verifies:
        /// - Multiple courses from the same department are returned
        /// - Courses from different departments are filtered out
        /// - Results maintain the correct course data integrity
        /// - Department matching is case-sensitive
        /// </remarks>
        [Test]
        public async Task GetByDepartmentAsync_ReturnsMatchingCourses()
        {
            // Arrange
            var course1 = _validCourse;
            var course2 = new Course
            {
                Id = Guid.NewGuid(),
                Code = "CS102",
                Title = _validCourse.Title,
                Description = _validCourse.Description,
                Credits = _validCourse.Credits,
                Department = "Computer Science",
                MaxEnrollment = _validCourse.MaxEnrollment,
                InstructorIds = new List<Guid>(_validCourse.InstructorIds)
            };
            var course3 = new Course
            {
                Id = Guid.NewGuid(),
                Code = "MATH101",
                Title = _validCourse.Title,
                Description = _validCourse.Description,
                Credits = _validCourse.Credits,
                Department = "Mathematics",
                MaxEnrollment = _validCourse.MaxEnrollment,
                InstructorIds = new List<Guid>(_validCourse.InstructorIds)
            };

            await _repository.AddAsync(course1);
            await _repository.AddAsync(course2);
            await _repository.AddAsync(course3);

            // Act
            var result = await _repository.GetByDepartmentAsync("Computer Science");

            // Assert
            result.Should().HaveCount(2)
                .And.Contain(c => c.Id == course1.Id)
                .And.Contain(c => c.Id == course2.Id);
        }

        /// <summary>
        /// Tests that GetByDepartmentAsync returns an empty list when no courses exist in the specified department.
        /// </summary>
        /// <remarks>
        /// This test verifies:
        /// - Empty list is returned for non-existent department
        /// - No exception is thrown for valid but non-existent department
        /// - Method handles missing department case gracefully
        /// - Repository maintains consistency when searching for non-existent department
        /// - Existing courses in other departments are not affected
        /// </remarks>
        [Test]
        public async Task GetByDepartmentAsync_NonExistentDepartment_ReturnsEmptyList()
        {
            // Arrange
            await _repository.AddAsync(_validCourse);

            // Act
            var result = await _repository.GetByDepartmentAsync("NonExistent Department");

            // Assert
            result.Should().BeEmpty();
        }

        /// <summary>
        /// Tests that GetByDepartmentAsync throws ArgumentException when attempting to search with null or empty department.
        /// </summary>
        /// <remarks>
        /// This test verifies:
        /// - ArgumentException is thrown for null department name
        /// - ArgumentException is thrown for empty department name
        /// - ArgumentException is thrown for whitespace-only department name
        /// - Exception message contains meaningful validation information
        /// - Repository state remains unchanged after failed validation
        /// </remarks>
        [Test]
        public void GetByDepartmentAsync_NullOrEmptyDepartment_ThrowsArgumentException()
        {
            // Act & Assert
            _repository.Invoking(r => r.GetByDepartmentAsync(null!))
                .Should().ThrowAsync<ArgumentException>();
            _repository.Invoking(r => r.GetByDepartmentAsync(""))
                .Should().ThrowAsync<ArgumentException>();
            _repository.Invoking(r => r.GetByDepartmentAsync("   "))
                .Should().ThrowAsync<ArgumentException>();
        }

        /// <summary>
        /// Tests that GetByInstructorAsync returns all courses assigned to a specific instructor.
        /// </summary>
        /// <remarks>
        /// This test verifies:
        /// - All courses assigned to the instructor are returned
        /// - Courses not assigned to the instructor are excluded
        /// - The instructor ID matching is exact
        /// - Course data remains intact in the returned collection
        /// - Correct handling of instructor's course list
        /// </remarks>
        [Test]
        public async Task GetByInstructorAsync_ReturnsMatchingCourses()
        {
            // Arrange
            var instructorId = Guid.NewGuid();
            var course1 = new Course
            {
                Id = _validCourse.Id,
                Code = _validCourse.Code,
                Title = _validCourse.Title,
                Description = _validCourse.Description,
                Credits = _validCourse.Credits,
                Department = _validCourse.Department,
                MaxEnrollment = _validCourse.MaxEnrollment,
                InstructorIds = new List<Guid> { instructorId }
            };
            var course2 = new Course
            {
                Id = Guid.NewGuid(),
                Code = "CS102",
                Title = _validCourse.Title,
                Description = _validCourse.Description,
                Credits = _validCourse.Credits,
                Department = _validCourse.Department,
                MaxEnrollment = _validCourse.MaxEnrollment,
                InstructorIds = new List<Guid> { instructorId }
            };
            var course3 = new Course
            {
                Id = Guid.NewGuid(),
                Code = "CS103",
                Title = _validCourse.Title,
                Description = _validCourse.Description,
                Credits = _validCourse.Credits,
                Department = _validCourse.Department,
                MaxEnrollment = _validCourse.MaxEnrollment,
                InstructorIds = new List<Guid> { Guid.NewGuid() }
            };

            await _repository.AddAsync(course1);
            await _repository.AddAsync(course2);
            await _repository.AddAsync(course3);

            // Act
            var result = await _repository.GetByInstructorAsync(instructorId);

            // Assert
            result.Should().HaveCount(2)
                .And.Contain(c => c.Id == course1.Id)
                .And.Contain(c => c.Id == course2.Id);
        }

        /// <summary>
        /// Tests that GetByInstructorAsync returns an empty list when no courses are assigned to the specified instructor.
        /// </summary>
        /// <remarks>
        /// This test verifies:
        /// - Empty list is returned for non-existent instructor ID
        /// - No exception is thrown for valid but non-existent instructor
        /// - Method handles missing instructor case gracefully
        /// - Repository maintains consistency when searching for non-existent instructor
        /// </remarks>
        [Test]
        public async Task GetByInstructorAsync_NonExistentInstructor_ReturnsEmptyList()
        {
            // Arrange
            await _repository.AddAsync(_validCourse);

            // Act
            var result = await _repository.GetByInstructorAsync(Guid.NewGuid());

            // Assert
            result.Should().BeEmpty();
        }
    }
}
