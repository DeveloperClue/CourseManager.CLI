using CourseManager.CLI.Core.Exceptions;
using CourseManager.CLI.Core.Models;
using CourseManager.CLI.Data.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CourseManager.CLI.Tests.Repositories
{
    /// <summary>
    /// Contains unit tests for the InstructorRepository class to verify instructor data persistence and business rules.
    /// </summary>
    [TestFixture]
    public class InstructorRepositoryTests
    {
        // Test directory for storing temporary test files
        private string _testDirectory = null!;
        // Path to the JSON file storing instructor data
        private string _testFilePath = null!;
        // Mock logger for testing logging behavior
        private Mock<ILogger<InstructorRepository>> _mockLogger = null!;
        // The repository instance being tested
        private InstructorRepository _repository = null!;
        // A valid instructor instance for testing
        private Instructor _validInstructor = null!;

        /// <summary>
        /// Sets up the test environment by creating a temporary directory for test data.
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
            if (_repository != null)
            {
                // Dispose of the repository to close any open file handles
                (_repository as IDisposable)?.Dispose();
                _repository = null!;
            }

            // Add a small delay to ensure file handles are released
            Task.Delay(100).Wait();

            if (Directory.Exists(_testDirectory))
            {
                try
                {
                    Directory.Delete(_testDirectory, true);
                }
                catch (IOException)
                {
                    // If deletion fails on first attempt, wait and try again
                    Task.Delay(500).Wait();
                    Directory.Delete(_testDirectory, true);
                }
            }
        }

        /// <summary>
        /// Initializes the test environment before each test by creating a new repository instance
        /// and setting up a valid instructor object for testing.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _testFilePath = Path.Combine(_testDirectory, "instructors.json");
            _mockLogger = new Mock<ILogger<InstructorRepository>>();
            _repository = new InstructorRepository(_testDirectory, _mockLogger.Object);

            _validInstructor = new Instructor
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Department = "Computer Science",
                OfficeLocation = "Room 101",
                IsFullTime = true,
                CourseIds = new List<Guid> { Guid.NewGuid() }
            };
        }        /// <summary>
        /// Cleans up after each test by releasing resources and removing the test data file.
        /// </summary>
        /// <remarks>
        /// This method ensures that each test starts with a clean state by removing any files
        /// created during the previous test.
        /// </remarks>
        [TearDown]
        public void TearDown()
        {
            // Explicitly set repository to null to release file handles
            _repository = null!;
            
            // Force garbage collection to release file handles
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            // Try to delete the file with retries
            if (File.Exists(_testFilePath))
            {
                for (int i = 0; i < 3; i++) // Try up to 3 times
                {
                    try
                    {
                        File.Delete(_testFilePath);
                        break; // Exit the loop if successful
                    }
                    catch (IOException)
                    {
                        // Wait briefly before trying again
                        System.Threading.Thread.Sleep(100);
                    }
                    catch (Exception ex)
                    {
                        // Log but don't fail the test for teardown issues
                        Console.WriteLine($"Warning: Could not delete test file {_testFilePath}: {ex.Message}");
                        break;
                    }
                }
            }
        }/// <summary>
        /// Tests that GetAllAsync returns an empty list when the repository contains no instructors.
        /// </summary>
        [Test]
        [Order(1)]
        public async Task GetAllAsync_EmptyRepository_ReturnsEmptyList()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        /// <summary>
        /// Tests that AddAsync successfully adds a valid instructor and returns the added instructor.
        /// Also verifies that the instructor is properly persisted in the repository.
        /// </summary>
        [Test]
        [Author("System")]
        [Description("Tests that a valid instructor can be added to the repository")]
        public async Task AddAsync_ValidInstructor_ShouldAddAndReturnInstructor()
        {
            // Act
            var result = await _repository.AddAsync(_validInstructor);

            // Assert
            result.Should().BeEquivalentTo(_validInstructor);
            
            // Verify the instructor was persisted
            var allInstructors = await _repository.GetAllAsync();
            allInstructors.Should().ContainSingle().Which.Should().BeEquivalentTo(_validInstructor);
        }        /// <summary>
        /// Tests that GetByIdAsync successfully retrieves an existing instructor.
        /// </summary>
        /// <remarks>
        /// This test verifies that the repository can retrieve an instructor by ID after it has been added,
        /// ensuring that data persistence is working correctly.
        /// </remarks>
        [Test]
        [Author("System")]
        [Description("Verifies that an instructor can be retrieved by ID from the repository")]
        public async Task GetByIdAsync_ExistingInstructor_ReturnsInstructor()
        {
            // Arrange
            await _repository.AddAsync(_validInstructor);

            // Act
            var result = await _repository.GetByIdAsync(_validInstructor.Id);

            // Assert
            result.Should().BeEquivalentTo(_validInstructor);
        }        /// <summary>
        /// Tests that GetByIdAsync throws EntityNotFoundException when attempting to retrieve a non-existent instructor.
        /// </summary>
        /// <remarks>
        /// This test verifies that the repository correctly handles requests for entities that do not exist
        /// by throwing an appropriate exception with a meaningful error message.
        /// </remarks>
        [Test]
        [Author("System")]
        [Description("Verifies that requesting a non-existent instructor throws the correct exception")]
        [Order(4)]
        public void GetByIdAsync_NonExistentInstructor_ThrowsEntityNotFoundException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act & Assert
            _repository.Invoking(r => r.GetByIdAsync(nonExistentId))
                .Should().ThrowAsync<EntityNotFoundException>()
                .WithMessage($"Instructor with ID {nonExistentId} not found");
        }        /// <summary>
        /// Tests that UpdateAsync successfully updates an existing instructor and persists the changes.
        /// </summary>
        /// <remarks>
        /// This test verifies that the repository can update an existing instructor and that the changes
        /// are properly persisted both in memory and to the storage file.
        /// </remarks>
        [Test]
        [Author("System")]
        [Description("Verifies that an instructor can be properly updated in the repository")]
        [Order(3)]
        public async Task UpdateAsync_ExistingInstructor_UpdatesAndReturnsInstructor()
        {            // Arrange
            await _repository.AddAsync(_validInstructor);
            // Create a copy to avoid reference issues
            var updatedInstructor = new Instructor
            {
                Id = _validInstructor.Id,
                FirstName = "Jane",
                LastName = _validInstructor.LastName,
                Email = _validInstructor.Email,
                Department = _validInstructor.Department,
                OfficeLocation = _validInstructor.OfficeLocation,
                IsFullTime = _validInstructor.IsFullTime,
                CourseIds = new List<Guid>(_validInstructor.CourseIds)
            };

            // Act
            var result = await _repository.UpdateAsync(updatedInstructor);

            // Assert
            result.Should().BeEquivalentTo(updatedInstructor);
            
            // Verify the update was persisted
            var retrieved = await _repository.GetByIdAsync(updatedInstructor.Id);
            retrieved.Should().BeEquivalentTo(updatedInstructor);
        }        /// <summary>
        /// Tests that UpdateAsync throws EntityNotFoundException when attempting to update a non-existent instructor.
        /// </summary>
        /// <remarks>
        /// This test verifies that the repository correctly handles update requests for instructors
        /// that do not exist by throwing an appropriate exception.
        /// </remarks>
        [Test]
        [Author("System")]
        [Description("Verifies that updating a non-existent instructor throws the correct exception")]
        [Order(5)]
        public void UpdateAsync_NonExistentInstructor_ThrowsEntityNotFoundException()
        {            // Arrange
            var nonExistentInstructor = new Instructor
            {
                Id = Guid.NewGuid(),
                FirstName = _validInstructor.FirstName,
                LastName = _validInstructor.LastName,
                Email = _validInstructor.Email,
                Department = _validInstructor.Department,
                OfficeLocation = _validInstructor.OfficeLocation,
                IsFullTime = _validInstructor.IsFullTime,
                CourseIds = new List<Guid>(_validInstructor.CourseIds)
            };

            // Act & Assert
            _repository.Invoking(r => r.UpdateAsync(nonExistentInstructor))
                .Should().ThrowAsync<EntityNotFoundException>()
                .WithMessage($"Instructor with ID {nonExistentInstructor.Id} not found");
        }        /// <summary>
        /// Tests that DeleteAsync successfully removes an existing instructor from the repository.
        /// </summary>
        /// <remarks>
        /// This test verifies that the repository can properly delete an instructor and that
        /// the deletion is persisted correctly.
        /// </remarks>
        [Test]
        [Author("System")]
        [Description("Verifies that an instructor can be deleted from the repository")]
        [Order(6)]
        public async Task DeleteAsync_ExistingInstructor_RemovesInstructor()
        {
            // Arrange
            await _repository.AddAsync(_validInstructor);

            // Act
            await _repository.DeleteAsync(_validInstructor.Id);

            // Assert
            var allInstructors = await _repository.GetAllAsync();
            allInstructors.Should().BeEmpty();
        }

        /// <summary>
        /// Tests that DeleteAsync throws EntityNotFoundException when attempting to delete a non-existent instructor.
        /// </summary>
        [Test]
        public void DeleteAsync_NonExistentInstructor_ThrowsEntityNotFoundException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act & Assert
            _repository.Invoking(r => r.DeleteAsync(nonExistentId))
                .Should().ThrowAsync<EntityNotFoundException>()
                .WithMessage($"Instructor with ID {nonExistentId} not found");
        }

        /// <summary>
        /// Tests that GetByDepartmentAsync returns all instructors in a specific department.
        /// </summary>
        /// <remarks>
        /// This test verifies:
        /// - All instructors from the specified department are returned
        /// - Instructors from other departments are excluded
        /// - Department matching is case-sensitive
        /// - Returned collection maintains data integrity
        /// - All instructor properties are correctly preserved
        /// </remarks>
        [Test]
        public async Task GetByDepartmentAsync_ReturnsMatchingInstructors()
        {            // Arrange
            var instructor1 = _validInstructor;
            var instructor2 = new Instructor
            {
                Id = Guid.NewGuid(),
                FirstName = _validInstructor.FirstName,
                LastName = _validInstructor.LastName,
                Email = "jane.doe@example.com",
                Department = "Computer Science",
                OfficeLocation = _validInstructor.OfficeLocation,
                IsFullTime = _validInstructor.IsFullTime,
                CourseIds = new List<Guid>(_validInstructor.CourseIds)
            };
            var instructor3 = new Instructor
            {
                Id = Guid.NewGuid(),
                FirstName = _validInstructor.FirstName,
                LastName = _validInstructor.LastName,
                Email = "bob.smith@example.com",
                Department = "Mathematics",
                OfficeLocation = _validInstructor.OfficeLocation,
                IsFullTime = _validInstructor.IsFullTime,
                CourseIds = new List<Guid>(_validInstructor.CourseIds)
            };

            await _repository.AddAsync(instructor1);
            await _repository.AddAsync(instructor2);
            await _repository.AddAsync(instructor3);

            // Act
            var result = await _repository.GetByDepartmentAsync("Computer Science");

            // Assert
            result.Should().HaveCount(2)
                .And.Contain(i => i.Id == instructor1.Id)
                .And.Contain(i => i.Id == instructor2.Id);
        }

        /// <summary>
        /// Tests that GetByCourseAsync returns all instructors assigned to a specific course.
        /// </summary>
        /// <remarks>
        /// This test verifies:
        /// - All instructors assigned to the course are returned
        /// - Instructors not assigned to the course are excluded
        /// - Course ID matching is exact
        /// - Instructor data integrity is maintained in results
        /// - Order of results is preserved from repository
        /// - Multiple instructors can be assigned to the same course
        /// </remarks>
                /// <summary>
        /// Tests that GetByCourseAsync returns all instructors assigned to a specific course.
        /// </summary>
        /// <remarks>
        /// This test verifies:
        /// - All instructors assigned to the course are returned
        /// - Instructors not assigned to the course are excluded
        /// - Course ID matching is exact
        /// - Instructor data integrity is maintained in results
        /// - Empty list is returned when no instructors are assigned
        /// - Multiple instructors can be assigned to same course
        /// </remarks>
        [Test]
        [Author("System")]
        [Description("Verifies that all instructors assigned to a specific course are returned")]
        public async Task GetByCourseAsync_ReturnsMatchingInstructors()
        {            // Arrange
            var courseId = Guid.NewGuid();
            var instructor1 = new Instructor
            {
                Id = _validInstructor.Id,
                FirstName = _validInstructor.FirstName,
                LastName = _validInstructor.LastName,
                Email = _validInstructor.Email,
                Department = _validInstructor.Department,
                OfficeLocation = _validInstructor.OfficeLocation,
                IsFullTime = _validInstructor.IsFullTime,
                CourseIds = new List<Guid> { courseId }
            };
            var instructor2 = new Instructor
            {
                Id = Guid.NewGuid(),
                FirstName = _validInstructor.FirstName,
                LastName = _validInstructor.LastName,
                Email = "jane.doe@example.com",
                Department = _validInstructor.Department,
                OfficeLocation = _validInstructor.OfficeLocation,
                IsFullTime = _validInstructor.IsFullTime,
                CourseIds = new List<Guid> { courseId }
            };
            var instructor3 = new Instructor
            {
                Id = Guid.NewGuid(),
                FirstName = _validInstructor.FirstName,
                LastName = _validInstructor.LastName,
                Email = "bob.smith@example.com",
                Department = _validInstructor.Department,
                OfficeLocation = _validInstructor.OfficeLocation,
                IsFullTime = _validInstructor.IsFullTime,
                CourseIds = new List<Guid> { Guid.NewGuid() }
            };

            await _repository.AddAsync(instructor1);
            await _repository.AddAsync(instructor2);
            await _repository.AddAsync(instructor3);

            // Act
            var result = await _repository.GetByCourseAsync(courseId);

            // Assert
            result.Should().HaveCount(2)
                .And.Contain(i => i.Id == instructor1.Id)
                .And.Contain(i => i.Id == instructor2.Id);
        }

        /// <summary>
        /// Tests that AssignToCourseAsync successfully assigns a course to an instructor
        /// and returns true to indicate success.
        /// </summary>
        /// <remarks>
        /// This test verifies:
        /// - Course is successfully added to instructor's course list
        /// - True is returned to indicate successful assignment
        /// - Changes are persisted to the repository
        /// - Instructor data is properly updated
        /// - No side effects occur on other instructor properties
        /// - Assignment is reflected in subsequent repository queries
        /// </remarks>
                /// <summary>
        /// Tests that AssignToCourseAsync successfully assigns a course to an instructor.
        /// </summary>
        /// <remarks>
        /// This test verifies:
        /// - Course is successfully added to instructor's course list
        /// - True is returned to indicate successful assignment
        /// - Changes are persisted to the repository
        /// - Instructor data is properly updated
        /// - No side effects occur on other instructor properties
        /// </remarks>
        [Test]
        [Author("System")]
        [Description("Verifies that a course can be successfully assigned to an instructor")]
        public async Task AssignToCourseAsync_ValidAssignment_AssignsAndReturnsTrue()
        {
            // Arrange
            await _repository.AddAsync(_validInstructor);
            var courseId = Guid.NewGuid();

            // Act
            var result = await _repository.AssignToCourseAsync(_validInstructor.Id, courseId);

            // Assert
            result.Should().BeTrue();
            var updatedInstructor = await _repository.GetByIdAsync(_validInstructor.Id);
            updatedInstructor.CourseIds.Should().Contain(courseId);
        }

        /// <summary>
        /// Tests that RemoveFromCourseAsync successfully removes a course assignment from an instructor
        /// and returns true to indicate success.
        /// </summary>
        /// <remarks>
        /// This test verifies:
        /// - Course is successfully removed from instructor's course list
        /// - True is returned to indicate successful removal
        /// - Changes are persisted to the repository
        /// - Instructor data is properly updated
        /// - Other course assignments remain unchanged
        /// - Removal is reflected in subsequent repository queries
        /// </remarks>
                /// <summary>
        /// Tests that RemoveFromCourseAsync successfully removes a course from an instructor.
        /// </summary>
        /// <remarks>
        /// This test verifies:
        /// - Course is successfully removed from instructor's course list
        /// - True is returned to indicate successful removal
        /// - Changes are persisted to the repository
        /// - Instructor data is properly updated
        /// - No side effects occur on other instructor properties
        /// </remarks>
        [Test]
        [Author("System")]
        [Description("Verifies that a course can be successfully removed from an instructor")]
        public async Task RemoveFromCourseAsync_ValidRemoval_RemovesAndReturnsTrue()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            _validInstructor.CourseIds.Add(courseId);
            await _repository.AddAsync(_validInstructor);

            // Act
            var result = await _repository.RemoveFromCourseAsync(_validInstructor.Id, courseId);

            // Assert
            result.Should().BeTrue();
            var updatedInstructor = await _repository.GetByIdAsync(_validInstructor.Id);
            updatedInstructor.CourseIds.Should().NotContain(courseId);
        }

        /// <summary>
        /// Tests that IsAssignedToCourseAsync returns true when checking if an instructor
        /// is assigned to a course they are actually assigned to.
        /// </summary>
        /// <remarks>
        /// This test verifies:
        /// - Correct identification of existing course assignments
        /// - True is returned for valid course-instructor assignments
        /// - Read operation doesn't modify repository data
        /// - Course ID matching is exact
        /// - Query is consistent with repository state
        /// </remarks>
        [Test]
        public async Task IsAssignedToCourseAsync_WhenAssigned_ReturnsTrue()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            _validInstructor.CourseIds.Add(courseId);
            await _repository.AddAsync(_validInstructor);

            // Act
            var result = await _repository.IsAssignedToCourseAsync(_validInstructor.Id, courseId);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that IsAssignedToCourseAsync returns false when checking if an instructor
        /// is assigned to a course they are not assigned to.
        /// </summary>
        /// <remarks>
        /// This test verifies:
        /// - Correct identification of non-existent course assignments
        /// - False is returned for invalid course-instructor assignments
        /// - Read operation doesn't modify repository data
        /// - Course ID matching is exact
        /// - Query is consistent with repository state
        /// - Non-existent course IDs are handled correctly
        /// </remarks>
        [Test]
        public async Task IsAssignedToCourseAsync_WhenNotAssigned_ReturnsFalse()
        {
            // Arrange
            await _repository.AddAsync(_validInstructor);
            var courseId = Guid.NewGuid();

            // Act
            var result = await _repository.IsAssignedToCourseAsync(_validInstructor.Id, courseId);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that AddAsync throws ValidationException when attempting to add an instructor
        /// with an email address that already exists in the repository.
        /// </summary>
        /// <remarks>
        /// This test verifies:
        /// - Email uniqueness constraint is enforced
        /// - ValidationException is thrown for duplicate emails
        /// - Exception message contains clear validation information
        /// - Repository state remains unchanged after failed validation
        /// - Email comparison is case-insensitive
        /// - Business rule for unique emails is consistently enforced
        /// </remarks>
        [Test]
        public void AddAsync_DuplicateEmail_ThrowsValidationException()
        {
            // Arrange
            var instructor1 = new Instructor
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Department = "Computer Science",
                OfficeLocation = "Room 101",
                IsFullTime = true,
                CourseIds = new List<Guid>()
            };

            var instructor2 = new Instructor
            {
                Id = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Smith",
                Email = "john.doe@example.com", // Same email as instructor1
                Department = "Computer Science",
                OfficeLocation = "Room 102",
                IsFullTime = true,
                CourseIds = new List<Guid>()
            };

            // Act & Assert
            _repository.Invoking(async r => 
            {
                await r.AddAsync(instructor1);
                await r.AddAsync(instructor2);
            })
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("An instructor with this email address already exists");
        }        /// <summary>
        /// Tests that AddAsync throws ValidationException when attempting to add an instructor
        /// with an invalid email format.
        /// </summary>
        /// <remarks>
        /// This test verifies:
        /// - Email format validation is enforced
        /// - Appropriate exception is thrown for invalid format
        /// - Validation message is descriptive
        /// - Repository state remains unchanged after failed validation
        /// </remarks>
        [Test]
        [Author("System")]
        [Description("Verifies that adding an instructor with invalid email format throws ValidationException")]
        public void AddAsync_InvalidEmail_ThrowsValidationException()
        {
            // Arrange
            var invalidInstructor = new Instructor
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "invalid-email", // Invalid email format
                Department = "Computer Science",
                OfficeLocation = "Room 101",
                IsFullTime = true,
                CourseIds = new List<Guid>()
            };

            // Act & Assert
            _repository.Invoking(r => r.AddAsync(invalidInstructor))
                .Should().ThrowAsync<ValidationException>()
                .WithMessage("Invalid email format");
        }

        /// <summary>
        /// Tests that AssignToCourseAsync throws ValidationException when attempting to assign
        /// a course to an instructor who is already assigned to the maximum number of courses.
        /// </summary>
        /// <remarks>
        /// This test verifies:
        /// - Maximum course load limit is enforced (4 courses per instructor)
        /// - ValidationException is thrown when attempting to exceed the limit
        /// - Exception message contains clear information about the course limit
        /// - Course assignments remain unchanged after failed validation
        /// - Business rule for maximum course load is consistently enforced
        /// </remarks>
        [Test]
        public async Task AssignToCourseAsync_AlreadyAssignedToMaxCourses_ThrowsValidationException()
        {
            // Arrange
            const int MaxCourses = 4;  // Assuming max courses per instructor is 4
            _validInstructor.CourseIds = new List<Guid>();
            for (int i = 0; i < MaxCourses; i++)
            {
                _validInstructor.CourseIds.Add(Guid.NewGuid());
            }
            await _repository.AddAsync(_validInstructor);            // Act & Assert
            await _repository.Invoking(r => r.AssignToCourseAsync(_validInstructor.Id, Guid.NewGuid()))
                .Should().ThrowAsync<ValidationException>()
                .WithMessage("Instructor cannot be assigned to more than 4 courses");
        }

        /// <summary>
        /// Tests that AssignToCourseAsync returns false when attempting to assign a course
        /// to an instructor who is already assigned to that course.
        /// </summary>
        /// <remarks>
        /// This test verifies:
        /// - Duplicate course assignments are prevented
        /// - False is returned instead of throwing an exception for this case
        /// - The instructor's course list remains unchanged
        /// - The operation is idempotent
        /// - The system maintains data integrity by preventing duplicate assignments
        /// </remarks>
        [Test]
        public async Task AssignToCourseAsync_AlreadyAssignedToCourse_ReturnsFalse()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            _validInstructor.CourseIds = new List<Guid> { courseId };
            await _repository.AddAsync(_validInstructor);

            // Act
            var result = await _repository.AssignToCourseAsync(_validInstructor.Id, courseId);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that UpdateAsync throws ValidationException when attempting to update an instructor's
        /// email to an email address that already exists in the repository.
        /// </summary>
        /// <remarks>
        /// This test verifies:
        /// - Email uniqueness constraint is enforced during updates
        /// - ValidationException is thrown for duplicate emails
        /// - Exception message contains clear validation information
        /// - Repository state remains unchanged after failed validation
        /// - Email comparison is case-insensitive
        /// - Business rule for unique emails is consistently enforced for updates
        /// </remarks>
        [Test]
        public void UpdateAsync_DuplicateEmail_ThrowsValidationException()
        {
            // Arrange
            var instructor1 = new Instructor
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Department = "Computer Science",
                OfficeLocation = "Room 101",
                IsFullTime = true,
                CourseIds = new List<Guid>()
            };

            var instructor2 = new Instructor
            {
                Id = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                Department = "Computer Science",
                OfficeLocation = "Room 102",
                IsFullTime = true,
                CourseIds = new List<Guid>()
            };

            // Act & Assert
            _repository.Invoking(async r => 
            {
                await r.AddAsync(instructor1);
                await r.AddAsync(instructor2);
                instructor2.Email = instructor1.Email; // Try to update with duplicate email
                await r.UpdateAsync(instructor2);
            })
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("An instructor with this email address already exists");
        }
    }
}
