using CourseManager.CLI.Core.Infrastructure;
using CourseManager.CLI.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CourseManager.CLI.ConsoleApp;

/// <summary>
/// Initializes sample data for the application when the repositories are empty
/// </summary>
/// <remarks>
/// This static class populates the application with realistic sample data for testing and demonstration purposes.
/// It creates sample instructors, courses, and schedules with meaningful relationships between them.
/// Data is only seeded if the repositories are empty to avoid duplicate data on application restart.
/// </remarks>
public static class DataInitializer
{
    /// <summary>
    /// Ensures the application has initial sample data by checking if repositories are empty
    /// and populating them if needed
    /// </summary>
    /// <param name="serviceProvider">The DI service provider to resolve repository dependencies</param>
    /// <returns>A task representing the asynchronous data initialization operation</returns>
    /// <remarks>
    /// This method follows a pattern of:
    /// 1. Check if data already exists in all repositories
    /// 2. If any repository has data, skip initialization to avoid duplicates
    /// 3. Otherwise, create and persist sample data in a logical order:
    ///    - First instructors
    ///    - Then courses
    ///    - Then instructor-course assignments
    ///    - Finally schedules that reference both instructors and courses
    /// </remarks>
    public static async Task EnsureInitialDataAsync(IServiceProvider serviceProvider)
    {
        // Get logger for recording initialization operations
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Initializing sample data...");

        // Get repositories from the service provider
        var courseRepo = serviceProvider.GetRequiredService<ICourseRepository>();
        var instructorRepo = serviceProvider.GetRequiredService<IInstructorRepository>();

        // Only seed data if there are no existing records
        var courses = await courseRepo.GetAllAsync();
        var instructors = await instructorRepo.GetAllAsync();

        if (courses.Any() && instructors.Any())
        {
            logger.LogInformation("Sample data already exists. Skipping initialization.");
            return;
        }

        logger.LogInformation("Creating sample data...");

        // Create departments
        var departments = new[] { "Computer Science", "Mathematics", "Physics", "English", "History" };

        // Create instructors
        var instructorsList = new Instructor[]
        {
            new Instructor
            {
                FirstName = "Mahi",
                LastName = "Mansoori",
                Email = "Mahi.Mansoori@university.edu",
                Phone = "555-123-4567",
                Department = departments[0],
                Title = "Professor"
            },
            new Instructor
            {
                FirstName = "Emily",
                LastName = "Johnson",
                Email = "emily.johnson@university.edu",
                Phone = "555-234-5678",
                Department = departments[0],
                Title = "Assistant Professor"
            },
            new Instructor
            {
                FirstName = "Michael",
                LastName = "Brown",
                Email = "michael.brown@university.edu",
                Phone = "555-345-6789",
                Department = departments[1],
                Title = "Associate Professor"
            },
            new Instructor
            {
                FirstName = "Sarah",
                LastName = "Davis",
                Email = "sarah.davis@university.edu",
                Phone = "555-456-7890",
                Department = departments[2],
                Title = "Professor"
            },
            new Instructor
            {
                FirstName = "Robert",
                LastName = "Wilson",
                Email = "robert.wilson@university.edu",
                Phone = "555-567-8901",
                Department = departments[3],
                Title = "Professor"
            }
        };

        foreach (var instructor in instructorsList)
        {
            await instructorRepo.AddAsync(instructor);
        }

        // Create courses
        var coursesList = new Course[]
        {
            new Course
            {
                Code = "CS101",
                Title = "Introduction to Computer Science",
                Description = "Fundamental concepts of programming and computer science",
                Department = departments[0],
                Credits = 3,
                MaxEnrollment = 30
            },
            new Course
            {
                Code = "CS201",
                Title = "Data Structures and Algorithms",
                Description = "Study of common data structures and algorithms",
                Department = departments[0],
                Credits = 4,
                MaxEnrollment = 25
            },
            new Course
            {
                Code = "MATH101",
                Title = "Calculus I",
                Description = "Introduction to differential calculus",
                Department = departments[1],
                Credits = 4,
                MaxEnrollment = 35
            },
            new Course
            {
                Code = "PHYS101",
                Title = "Introduction to Physics",
                Description = "Mechanics, energy, and thermodynamics",
                Department = departments[2],
                Credits = 4,
                MaxEnrollment = 30
            },
            new Course
            {
                Code = "ENG101",
                Title = "College Composition",
                Description = "Introduction to academic writing",
                Department = departments[3],
                Credits = 3,
                MaxEnrollment = 25
            }
        };

        foreach (var course in coursesList)
        {
            await courseRepo.AddAsync(course);
        }

        // Assign instructors to courses
        await AssignInstructorToCourse(
            instructorsList[0].Id, coursesList[0].Id, instructorRepo, courseRepo);
        await AssignInstructorToCourse(
            instructorsList[1].Id, coursesList[1].Id, instructorRepo, courseRepo);
        await AssignInstructorToCourse(
            instructorsList[2].Id, coursesList[2].Id, instructorRepo, courseRepo);
        await AssignInstructorToCourse(
            instructorsList[3].Id, coursesList[3].Id, instructorRepo, courseRepo);
        await AssignInstructorToCourse(
            instructorsList[4].Id, coursesList[4].Id, instructorRepo, courseRepo);

        logger.LogInformation("Sample data initialization complete. Created {CourseCount} courses, {InstructorCount} instructors.",
            coursesList.Length, instructorsList.Length);
    }

    private static async Task AssignInstructorToCourse(
        Guid instructorId, Guid courseId,
        IInstructorRepository instructorRepo,
        ICourseRepository courseRepo)
    {
        var instructor = await instructorRepo.GetByIdAsync(instructorId);
        var course = await courseRepo.GetByIdAsync(courseId);

        instructor.CourseIds.Add(courseId);
        course.InstructorIds.Add(instructorId);

        await instructorRepo.UpdateAsync(instructor);
        await courseRepo.UpdateAsync(course);
    }
}
