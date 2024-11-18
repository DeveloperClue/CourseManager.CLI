namespace CourseManager.CLI.Core.Infrastructure
{
    /// <summary>
    /// Generic repository interface for data access operations.
    /// Provides common CRUD operations for entity persistence.
    /// </summary>
    /// <typeparam name="T">The entity type that must be a reference type</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Retrieves all entities of type T from the data store
        /// </summary>
        /// <returns>A collection of all entities</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Retrieves a specific entity by its unique identifier
        /// </summary>
        /// <param name="id">The unique identifier of the entity to retrieve</param>
        /// <returns>The entity with the specified ID</returns>
        /// <exception cref="Core.Exceptions.EntityNotFoundException">Thrown when the entity with the specified ID is not found</exception>
        Task<T> GetByIdAsync(Guid id);

        /// <summary>
        /// Adds a new entity to the data store
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <returns>The added entity, potentially with generated ID or other system-populated fields</returns>
        /// <exception cref="ArgumentNullException">Thrown when the entity is null</exception>
        /// <exception cref="Core.Exceptions.DataOperationException">Thrown when the add operation fails</exception>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Updates an existing entity in the data store
        /// </summary>
        /// <param name="entity">The entity with updated values</param>
        /// <returns>The updated entity</returns>
        /// <exception cref="ArgumentNullException">Thrown when the entity is null</exception>
        /// <exception cref="Core.Exceptions.EntityNotFoundException">Thrown when the entity to update is not found</exception>
        /// <exception cref="Core.Exceptions.DataOperationException">Thrown when the update operation fails</exception>        
        Task<T> UpdateAsync(T entity);

        /// <summary>
        /// Deletes an entity from the data store by its unique identifier
        /// </summary>
        /// <param name="id">The unique identifier of the entity to delete</param>
        /// <exception cref="Core.Exceptions.EntityNotFoundException">Thrown when the entity with the specified ID is not found</exception>
        /// <exception cref="Core.Exceptions.DataOperationException">Thrown when the delete operation fails</exception>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Saves all pending changes to the data store.
        /// This method ensures that all changes made to the repository are committed to the underlying storage.
        /// </summary>
        /// <returns>A task representing the asynchronous save operation</returns>
        /// <exception cref="Core.Exceptions.DataOperationException">Thrown when the save operation fails</exception>
        Task SaveChangesAsync();
    }
}
