namespace CourseManager.CLI.Core.Events
{
    /// <summary>
    /// Base class for all event args in the course management system
    /// </summary>
    public abstract class CourseManagerEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the timestamp when the event occurred
        /// </summary>
        /// <remarks>
        /// Automatically set to the current date/time when the event is created
        /// </remarks>
        public DateTime Timestamp { get; } = DateTime.Now;
    }

    /// <summary>
    /// Event arguments for when a course is added, updated or removed
    /// </summary>
    public class CourseEventArgs : CourseManagerEventArgs
    {
        /// <summary>
        /// Gets the unique identifier of the course
        /// </summary>
        public Guid CourseId { get; }

        /// <summary>
        /// Gets the course code (e.g., "CS101")
        /// </summary>
        public string CourseCode { get; }

        /// <summary>
        /// Gets the title of the course
        /// </summary>
        public string CourseTitle { get; }

        /// <summary>
        /// Gets the action performed on the course (e.g., "Added", "Updated", "Deleted")
        /// </summary>
        public string Action { get; }

        /// <summary>
        /// Initializes a new instance of the CourseEventArgs class
        /// </summary>
        /// <param name="courseId">The unique identifier of the course</param>
        /// <param name="courseCode">The course code</param>
        /// <param name="courseTitle">The title of the course</param>
        /// <param name="action">The action performed on the course</param>
        public CourseEventArgs(Guid courseId, string courseCode, string courseTitle, string action)
        {
            CourseId = courseId;
            CourseCode = courseCode;
            CourseTitle = courseTitle;
            Action = action;
        }
    }

    /// <summary>
    /// Event arguments for when an instructor is added, updated or removed
    /// </summary>
    public class InstructorEventArgs : CourseManagerEventArgs
    {
        /// <summary>
        /// Gets the unique identifier of the instructor
        /// </summary>
        public Guid InstructorId { get; }

        /// <summary>
        /// Gets the full name of the instructor
        /// </summary>
        public string InstructorName { get; }

        /// <summary>
        /// Gets the action performed on the instructor (e.g., "Added", "Updated", "Deleted")
        /// </summary>
        public string Action { get; }

        /// <summary>
        /// Initializes a new instance of the InstructorEventArgs class
        /// </summary>
        /// <param name="instructorId">The unique identifier of the instructor</param>
        /// <param name="instructorName">The name of the instructor</param>
        /// <param name="action">The action performed on the instructor</param>
        public InstructorEventArgs(Guid instructorId, string instructorName, string action)
        {
            InstructorId = instructorId;
            InstructorName = instructorName;
            Action = action;
        }
    }
}
