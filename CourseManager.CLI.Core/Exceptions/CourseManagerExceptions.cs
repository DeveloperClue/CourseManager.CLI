namespace CourseManager.CLI.Core.Exceptions
{
    /// <summary>
    /// Base exception for all CourseManager application exceptions
    /// </summary>
    /// <remarks>
    /// This abstract class serves as the base for a hierarchy of application-specific exceptions.
    /// It provides a common type that can be used to catch all CourseManager exceptions.
    /// </remarks>
    public abstract class CourseManagerException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the CourseManagerException class with a specified error message
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        protected CourseManagerException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the CourseManagerException class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        protected CourseManagerException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when a requested entity is not found in the repository
    /// </summary>
    /// <remarks>
    /// This exception indicates that the client requested an entity with a specific
    /// identifier, but no entity with that identifier exists in the data store.
    /// </remarks>
    public class EntityNotFoundException : CourseManagerException
    {
        /// <summary>
        /// Initializes a new instance of the EntityNotFoundException class
        /// </summary>
        /// <param name="entityType">The type of entity that was not found (e.g., "Course", "Instructor")</param>
        /// <param name="id">The identifier of the entity that was not found</param>
        public EntityNotFoundException(string entityType, string id)
            : base($"Entity with ID {id} not found") { }
    }

    /// <summary>
    /// Exception thrown for validation errors during entity operations
    /// </summary>
    /// <remarks>
    /// This exception is raised when an entity fails business rule validation,
    /// such as required fields, format constraints, or business logic rules.
    /// </remarks>
    public class ValidationException : CourseManagerException
    {
        /// <summary>
        /// Initializes a new instance of the ValidationException class
        /// </summary>
        /// <param name="message">The validation error message</param>
        public ValidationException(string message)
            : base(message) { }
    }

    /// <summary>
    /// Exception thrown when a data operation fails
    /// </summary>
    /// <remarks>
    /// This exception is used for failures in data access operations such as
    /// retrieval, storage, or deletion of entities, typically due to infrastructure issues.
    /// </remarks>
    public class DataOperationException : CourseManagerException
    {
        /// <summary>
        /// Initializes a new instance of the DataOperationException class
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public DataOperationException(string message)
            : base(message) { }

        /// <summary>
        /// Initializes a new instance of the DataOperationException class
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public DataOperationException(string message, Exception innerException)
            : base(message, innerException) { }
    }

}
