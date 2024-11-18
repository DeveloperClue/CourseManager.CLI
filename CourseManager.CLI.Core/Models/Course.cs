namespace CourseManager.CLI.Core.Models
{
    /// <summary>
    /// Represents an academic course in the educational system
    /// </summary>
    /// <remarks>
    /// A Course is the fundamental unit of instruction that can be scheduled across
    /// multiple time slots and assigned to multiple instructors. Each course has
    /// a unique identifier and course code.
    /// </remarks>
    public class Course
    {
        /// <summary>
        /// Unique technical identifier for the course
        /// </summary>
        /// <remarks>
        /// This GUID is auto-generated and used as the primary key in the data store.
        /// </remarks>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Course code that serves as a business identifier (e.g., "CS101", "MATH200")
        /// </summary>
        /// <remarks>
        /// Course codes are unique across the system and follow the department's
        /// naming conventions. Format is typically a department prefix followed by a number.
        /// </remarks>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Official title of the course as it appears in the course catalog
        /// </summary>
        /// <remarks>
        /// Example: "Introduction to Computer Science" or "Advanced Calculus"
        /// </remarks>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description of the course content, objectives, and outcomes
        /// </summary>
        /// <remarks>
        /// This field contains the comprehensive course description used 
        /// for the course catalog and syllabus.
        /// </remarks>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Number of credit hours awarded for completing the course
        /// </summary>
        /// <remarks>
        /// Most courses are 3-4 credit hours. Used for tuition calculation and degree progress.
        /// </remarks>
        public int Credits { get; set; }

        /// <summary>
        /// Maximum number of students that can enroll in the course
        /// </summary>
        /// <remarks>
        /// This represents the course capacity and is used for enrollment management.
        /// It may be determined by factors such as room size or instructional resources.
        /// </remarks>
        public int MaxEnrollment { get; set; }

        /// <summary>
        /// Academic department offering the course
        /// </summary>
        /// <remarks>
        /// Examples include "Computer Science", "Mathematics", "English", etc.
        /// This field is used for filtering and organizing courses by department.
        /// </remarks>
        public string Department { get; set; } = string.Empty;

        /// <summary>
        /// Collection of instructor IDs assigned to teach this course
        /// </summary>
        /// <remarks>
        /// Maintains a many-to-many relationship between courses and instructors.
        /// A course may have multiple instructors (professor, assistant, etc.).
        /// </remarks>
        public List<Guid> InstructorIds { get; set; } = new();

        /// <summary>
        /// Collection of schedule IDs representing the scheduled class sessions for this course
        /// </summary>
        /// <remarks>
        /// Each schedule ID references a Schedule entity that contains the time, day,
        /// and location information for a class session.
        /// </remarks>
        public List<Guid> ScheduleIds { get; set; } = new();

        /// <summary>
        /// Date when the course record was created in the system
        /// </summary>
        /// <remarks>
        /// Automatically set to the current date/time when a new course is created.
        /// Used for auditing and tracking purposes.
        /// </remarks>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Number of credit hours for the course (alias for Credits property)
        /// </summary>
        /// <remarks>
        /// This is an alternative accessor for the Credits property to accommodate
        /// different terminologies used across systems.
        /// </remarks>
        public int CreditHours { get => Credits; set => Credits = value; }

        /// <summary>
        /// Maximum enrollment capacity for the course (alias for MaxEnrollment property)
        /// </summary>
        /// <remarks>
        /// This is an alternative accessor for the MaxEnrollment property to accommodate
        /// different terminologies used across systems.
        /// </remarks>
        public int EnrollmentCap { get => MaxEnrollment; set => MaxEnrollment = value; }
    }
}
