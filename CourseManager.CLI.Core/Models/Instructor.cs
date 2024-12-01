namespace CourseManager.CLI.Core.Models
{
    /// <summary>
    /// Represents a faculty member who can teach courses in the educational system
    /// </summary>
    /// <remarks>
    /// The Instructor class contains personal and professional information about faculty members,
    /// including their contact information, department affiliation, and course assignments.
    /// </remarks>
    public class Instructor
    {
        /// <summary>
        /// Unique technical identifier for the instructor
        /// </summary>
        /// <remarks>
        /// This GUID is auto-generated and used as the primary key in the data store.
        /// </remarks>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// First name of the instructor
        /// </summary>
        /// <remarks>
        /// Legal first name as it appears in official records.
        /// </remarks>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Last name of the instructor
        /// </summary>
        /// <remarks>
        /// Legal surname as it appears in official records.
        /// </remarks>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Email address for contacting the instructor
        /// </summary>
        /// <remarks>
        /// Typically the institutional email address that serves as a unique
        /// business identifier for the instructor in the system.
        /// </remarks>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Phone number of the instructor for contact purposes
        /// </summary>
        /// <remarks>
        /// May include office phone, mobile phone, or preferred contact number.
        /// </remarks>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Academic department the instructor is primarily affiliated with
        /// </summary>
        /// <remarks>
        /// Examples include "Computer Science", "Mathematics", "English", etc.
        /// This field is used for filtering and organizing instructors by department.
        /// </remarks>
        public string Department { get; set; } = string.Empty;

        /// <summary>
        /// Instructor's academic or professional title
        /// </summary>
        /// <remarks>
        /// Examples include "Professor", "Associate Professor", "Assistant Professor",
        /// "Lecturer", "Adjunct Professor", etc. This reflects their rank or position.
        /// </remarks>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Collection of course IDs this instructor is assigned to teach
        /// </summary>
        /// <remarks>
        /// Maintains a many-to-many relationship between instructors and courses.
        /// Each GUID references a Course entity in the system.
        /// </remarks>
        public List<Guid> CourseIds { get; set; } = new();

        /// <summary>
        /// Full name property combining first and last name
        /// </summary>
        /// <remarks>
        /// Computed property that concatenates FirstName and LastName.
        /// Used for display purposes throughout the system.
        /// </remarks>
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Office location of the instructor on campus
        /// </summary>
        /// <remarks>
        /// Identifies the building and room number where the instructor's office is located.
        /// Example: "Science Building 305" or "Liberal Arts 212B"
        /// </remarks>
        public string OfficeLocation { get; set; } = string.Empty;

        /// <summary>
        /// Alternative property name for OfficeLocation (alias)
        /// </summary>
        /// <remarks>
        /// This is an alternative accessor for the OfficeLocation property to accommodate
        /// different terminologies used across systems.
        /// </remarks>
        public string Office { get => OfficeLocation; set => OfficeLocation = value; }

        /// <summary>
        /// Indicates whether the instructor is currently active in the system
        /// </summary>
        /// <remarks>
        /// When set to false, the instructor is considered inactive and may not be assigned to
        /// new courses but remains in the system for historical records. Used for instructors
        /// on leave, retired, or otherwise not currently teaching.
        /// </remarks>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Date when the instructor was hired by the institution
        /// </summary>
        /// <remarks>
        /// Used for seniority calculations, employment anniversary notifications,
        /// and historical reporting.
        /// </remarks>
        public DateTime HireDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Date when the instructor record was created in the system
        /// </summary>
        /// <remarks>
        /// Automatically set to the current date/time when a new instructor record is created.
        /// Used for auditing and tracking purposes.
        /// </remarks>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Indicates whether the instructor has a full-time or part-time position
        /// </summary>
        /// <remarks>
        /// Affects course load calculations and eligibility for certain assignments.
        /// True means full-time, false means part-time or adjunct.
        /// </remarks>
        public bool IsFullTime { get; set; } = true;
    }
}
