using System.Collections.Concurrent;

namespace RepositoryApp.Repository
{
    /// <summary>
    /// Thread-safe in-memory repository implementation using ConcurrentDictionary
    /// Enhanced with validation, overwrite protection, and storage abstraction
    /// </summary>
    public class InMemoryRepository : IRepository
    {
        private readonly IStorageProvider<string> _storageProvider;
        private readonly IContentValidator _validator;
        private bool _isInitialized;
        private bool _hasBeenInitialized; // Track if Initialize was ever called
        private readonly object _initLock = new object();

        public InMemoryRepository() 
            : this(new InMemoryStorageProvider<string>(), new DefaultContentValidator())
        {
        }

        public InMemoryRepository(IStorageProvider<string> storageProvider, IContentValidator validator)
        {
            _storageProvider = storageProvider ?? throw new ArgumentNullException(nameof(storageProvider));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _isInitialized = false;
            _hasBeenInitialized = false;
        }

        public void Initialize()
        {
            lock (_initLock)
            {
                // Requirement #7: Initialize must be called only once after instance creation
                if (_hasBeenInitialized)
                {
                    throw new InvalidOperationException("Initialize can only be called once after repository instance is created.");
                }

                if (!_isInitialized)
                {
                    _storageProvider.Clear();
                    _isInitialized = true;
                    _hasBeenInitialized = true;
                }
            }
        }

        public void Register(string itemName, string itemContent, int itemType)
        {
            if (string.IsNullOrWhiteSpace(itemName))
                throw new ArgumentException("Item name cannot be null or empty", nameof(itemName));

            if (string.IsNullOrWhiteSpace(itemContent))
                throw new ArgumentException("Item content cannot be null or empty", nameof(itemContent));

            if (itemType != 1 && itemType != 2)
                throw new ArgumentException("Item type must be 1 (JSON) or 2 (XML)", nameof(itemType));

            // Requirement #5: Validate content based on itemType
            var validationResult = _validator.Validate(itemContent, itemType);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException($"Content validation failed: {validationResult.ErrorMessage}", nameof(itemContent));
            }

            // Requirement #6: Protect registered items from overwrite
            if (_storageProvider.Exists(itemName))
            {
                throw new InvalidOperationException($"Item '{itemName}' already exists and cannot be overwritten. Use Update method or Deregister first.");
            }

            var item = new RepositoryItem<string>
            {
                Content = itemContent,
                Type = itemType
            };

            _storageProvider.Store(itemName, item);
        }

        public string Retrieve(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName))
                return null;

            var item = _storageProvider.Retrieve(itemName);
            return item?.Content;
        }

        public int GetType(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName))
                return 0;

            var item = _storageProvider.Retrieve(itemName);
            return item?.Type ?? 0;
        }

        public void Deregister(string itemName)
        {
            if (!string.IsNullOrWhiteSpace(itemName))
            {
                _storageProvider.Remove(itemName);
            }
        }

        /// <summary>
        /// Updates an existing item (allows overwrite for update scenarios)
        /// </summary>
        public void Update(string itemName, string itemContent, int itemType)
        {
            if (string.IsNullOrWhiteSpace(itemName))
                throw new ArgumentException("Item name cannot be null or empty", nameof(itemName));

            if (!_storageProvider.Exists(itemName))
                throw new InvalidOperationException($"Item '{itemName}' does not exist. Use Register to create new items.");

            // Validate content
            var validationResult = _validator.Validate(itemContent, itemType);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException($"Content validation failed: {validationResult.ErrorMessage}", nameof(itemContent));
            }

            var item = new RepositoryItem<string>
            {
                Content = itemContent,
                Type = itemType
            };

            _storageProvider.Store(itemName, item);
        }
    }
}
