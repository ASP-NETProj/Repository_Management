namespace RepositoryApp.Repository
{
    /// <summary>
    /// Repository interface for managing items in memory with thread-safe operations
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Store an item to the repository in-memory storage.
        /// </summary>
        /// <param name="itemName">Unique identifier for the item</param>
        /// <param name="itemContent">JSON or XML string content</param>
        /// <param name="itemType">1 = JSON, 2 = XML</param>
        void Register(string itemName, string itemContent, int itemType);

        /// <summary>
        /// Retrieve an item from the repository.
        /// </summary>
        /// <param name="itemName">Unique identifier for the item</param>
        /// <returns>Item content as string, or null if not found</returns>
        string Retrieve(string itemName);

        /// <summary>
        /// Retrieve the type of the item (JSON or XML).
        /// </summary>
        /// <param name="itemName">Unique identifier for the item</param>
        /// <returns>1 = JSON, 2 = XML, 0 = not found</returns>
        int GetType(string itemName);

        /// <summary>
        /// Remove an item from the repository.
        /// </summary>
        /// <param name="itemName">Unique identifier for the item</param>
        void Deregister(string itemName);

        /// <summary>
        /// Initialize the repository for use.
        /// </summary>
        void Initialize();
    }
}
