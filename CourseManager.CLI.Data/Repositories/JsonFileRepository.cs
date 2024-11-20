using System.Text.Json;
using CourseManager.CLI.Core.Exceptions;
using CourseManager.CLI.Core.Infrastructure;
using Microsoft.Extensions.Logging;

namespace CourseManager.CLI.Data.Repositories
{
    /// <summary>
    /// Base JSON file repository implementation that provides persistent storage of entities in JSON files
    /// </summary>
    /// <remarks>
    /// This class handles the core CRUD operations for entity persistence using JSON files.
    /// It loads entities on startup and saves changes back to the file after each mutation operation.
    /// All repository implementations should inherit from this class.
    /// </remarks>
    /// <typeparam name="T">Entity type that must have an Id property of type Guid</typeparam>
    public abstract class JsonFileRepository<T> : IRepository<T> where T : class
    {
        /// <summary>
        /// Path to the JSON file where entities are stored
        /// </summary>
        protected readonly string _filePath;

        /// <summary>
        /// In-memory collection of entities loaded from the JSON file
        /// </summary>
        protected List<T> _entities = new();

        /// <summary>
        /// Logger for repository operations
        /// </summary>
        protected readonly ILogger<JsonFileRepository<T>> _logger;

        /// <summary>
        /// JSON serialization options for reading/writing entities
        /// </summary>
        protected readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the repository with the specified file path
        /// </summary>
        /// <param name="filePath">Path to the JSON file for entity storage</param>
        /// <param name="logger">Logger for recording repository operations</param>
        protected JsonFileRepository(string filePath, ILogger<JsonFileRepository<T>> logger)
        {
            _filePath = filePath;
            _logger = logger;

            // Configure JSON serialization options
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,  // Makes the JSON file human-readable
                PropertyNameCaseInsensitive = true  // Allows case-insensitive property matching
            };

            // Load entities from file on initialization
            // Note: .Wait() is used here as constructors cannot be async
            InitializeAsync().Wait();
        }

        /// <summary>
        /// Initializes the repository by loading entities from the JSON file
        /// </summary>
        /// <remarks>
        /// This method is called during repository construction to load the initial data.
        /// If the file doesn't exist, it creates an empty entity collection and ensures
        /// the directory structure exists.
        /// </remarks>
        protected async Task InitializeAsync()
        {
            try
            {
                // Check if the data file exists
                if (File.Exists(_filePath))
                {
                    // Read the file content
                    var json = await File.ReadAllTextAsync(_filePath);

                    // Deserialize the JSON into entities
                    // The null-coalescing operator ensures we never have a null collection
                    _entities = JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
                    _logger.LogInformation("Successfully loaded {Count} entities from {FilePath}", _entities.Count, _filePath);
                }
                else
                {                    // Initialize with an empty collection if file doesn't exist
                    _entities = new List<T>();

                    // Don't create the file until the first entity is added
                    _logger.LogInformation("Using empty repository (file will be created when first entity is added)");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing repository from {FilePath}", _filePath);
                throw new DataOperationException($"Failed to initialize repository from {_filePath}", ex);
            }
        }

        /// <summary>
        /// Retrieves all entities from the repository
        /// </summary>
        /// <returns>A collection of all entities in the repository</returns>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await Task.FromResult(_entities.ToList());
        }

        /// <summary>
        /// Retrieves a specific entity by its unique identifier
        /// </summary>
        /// <param name="id">The unique identifier of the entity to retrieve</param>
        /// <returns>The entity with the specified ID</returns>
        /// <exception cref="InvalidOperationException">Thrown when the entity type doesn't have an Id property</exception>
        /// <exception cref="EntityNotFoundException">Thrown when an entity with the specified ID is not found</exception>
        public virtual async Task<T> GetByIdAsync(Guid id)
        {
            // Use reflection to get the Id property of the entity type
            // This allows the repository to work with any entity type that has an Id property
            // without requiring the entity to implement a specific interface
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty == null)
                throw new InvalidOperationException($"Entity {typeof(T).Name} does not have an Id property");

            // Find the entity with the matching ID by using reflection to compare property values
            // This approach works with any entity type as long as it has an Id property of type Guid
            var entity = _entities.FirstOrDefault(e =>
                id.Equals(idProperty.GetValue(e)));

            // Throw a specific exception if no matching entity was found
            // This provides a more meaningful error than just returning null
            // and allows callers to distinguish between different error cases
            if (entity == null)
                throw new EntityNotFoundException(typeof(T).Name, id.ToString());

            return await Task.FromResult(entity);
        }

        /// <summary>
        /// Adds a new entity to the repository
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <returns>The added entity (same instance)</returns>
        /// <exception cref="InvalidOperationException">Thrown if the entity type doesn't have an Id property</exception>
        /// <exception cref="ArgumentNullException">Thrown if the entity is null</exception>
        /// <exception cref="DataOperationException">Thrown if saving the changes fails</exception>
        public virtual async Task<T> AddAsync(T entity)
        {
            // Input validation
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Verify that the entity type has an Id property
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty == null)
                throw new InvalidOperationException($"Entity {typeof(T).Name} does not have an Id property");

            // Add the entity to the in-memory collection
            _entities.Add(entity);

            // Persist the changes to the file
            await SaveChangesAsync();

            return entity;
        }

        /// <summary>
        /// Updates an existing entity in the repository
        /// </summary>
        /// <param name="entity">The entity with updated values</param>
        /// <returns>The updated entity (same instance)</returns>
        /// <exception cref="InvalidOperationException">Thrown when the entity type doesn't have an Id property or the Id value is null</exception>
        /// <exception cref="EntityNotFoundException">Thrown when an entity with the specified ID is not found</exception>
        /// <exception cref="DataOperationException">Thrown when saving the changes fails</exception>
        public virtual async Task<T> UpdateAsync(T entity)
        {
            // Use reflection to get the Id property of the entity type
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty == null)
                throw new InvalidOperationException($"Entity {typeof(T).Name} does not have an Id property");

            // Get the ID value from the entity and ensure it's not null
            var idValue = idProperty.GetValue(entity);
            if (idValue == null)
                throw new InvalidOperationException($"Entity {typeof(T).Name} has a null Id value");

            // Cast the ID to Guid
            var id = (Guid)idValue;

            // Find the index of the entity with the matching ID
            var existingIndex = _entities.FindIndex(e => id.Equals(idProperty.GetValue(e)));

            // Throw exception if no matching entity was found
            if (existingIndex == -1)
                throw new EntityNotFoundException(typeof(T).Name, id.ToString());

            // Replace the existing entity with the updated one
            _entities[existingIndex] = entity;

            // Persist the changes to the file
            await SaveChangesAsync();

            return entity;
        }

        /// <summary>
        /// Deletes an entity from the repository by its unique identifier
        /// </summary>
        /// <param name="id">The unique identifier of the entity to delete</param>
        /// <exception cref="InvalidOperationException">Thrown when the entity type doesn't have an Id property</exception>
        /// <exception cref="EntityNotFoundException">Thrown when an entity with the specified ID is not found</exception>
        /// <exception cref="DataOperationException">Thrown when saving the changes fails</exception>
        public virtual async Task DeleteAsync(Guid id)
        {
            // Use reflection to get the Id property of the entity type
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty == null)
                throw new InvalidOperationException($"Entity {typeof(T).Name} does not have an Id property");

            // Find the index of the entity with the matching ID
            var existingIndex = _entities.FindIndex(e => id.Equals(idProperty.GetValue(e)));

            // Throw exception if no matching entity was found
            if (existingIndex == -1)
                throw new EntityNotFoundException(typeof(T).Name, id.ToString());

            // Remove the entity from the collection
            _entities.RemoveAt(existingIndex);

            // Persist the changes to the file
            await SaveChangesAsync();
        }

        /// <summary>
        /// Persists the current state of entities to the JSON file
        /// </summary>
        /// <remarks>
        /// This method is called after each mutation operation (Add, Update, Delete)
        /// to make sure the file is always in sync with the in-memory collection.
        /// </remarks>
        /// <returns>A task representing the asynchronous save operation</returns>
        /// <exception cref="DataOperationException">Thrown when saving to the file fails</exception>
        public virtual async Task SaveChangesAsync()
        {
            try
            {
                // Convert the entities collection to JSON
                var json = JsonSerializer.Serialize(_entities, _jsonOptions);

                // Write the JSON to the file, overwriting any existing content
                await File.WriteAllTextAsync(_filePath, json);

                _logger.LogInformation("Successfully saved {Count} entities to {FilePath}", _entities.Count, _filePath);
            }
            catch (Exception ex)
            {
                // Log the error and wrap it in a DataOperationException
                _logger.LogError(ex, "Error saving repository to {FilePath}", _filePath);
                throw new DataOperationException($"Failed to save repository to {_filePath}", ex);
            }
        }
    }
}
