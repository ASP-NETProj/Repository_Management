namespace RepositoryApp.Repository
{
    /// <summary>
    /// Storage provider interface to support multiple storage backends
    /// </summary>
    /// <typeparam name="TContent">Type of content being stored</typeparam>
    public interface IStorageProvider<TContent>
    {
        /// <summary>
        /// Store an item
        /// </summary>
        void Store(string key, IRepositoryItem<TContent> item);

        /// <summary>
        /// Retrieve an item
        /// </summary>
        IRepositoryItem<TContent> Retrieve(string key);

        /// <summary>
        /// Remove an item
        /// </summary>
        void Remove(string key);

        /// <summary>
        /// Check if item exists
        /// </summary>
        bool Exists(string key);

        /// <summary>
        /// Get all keys
        /// </summary>
        IEnumerable<string> GetAllKeys();

        /// <summary>
        /// Clear all items
        /// </summary>
        void Clear();
    }
}
