using RepositoryApp.Repository;
using System.Collections.Concurrent;

namespace RepositoryApp.Repository
{
    /// <summary>
    /// Extended repository with listing capabilities
    /// </summary>
    public class ExtendedRepository : InMemoryRepository
    {
        private readonly IStorageProvider<string> _storageProvider;

        public ExtendedRepository() 
            : this(new InMemoryStorageProvider<string>(), new DefaultContentValidator())
        {
        }

        public ExtendedRepository(IStorageProvider<string> storageProvider, IContentValidator validator) 
            : base(storageProvider, validator)
        {
            _storageProvider = storageProvider;
        }

        /// <summary>
        /// Get all item names in the repository
        /// </summary>
        public IEnumerable<string> GetAllItemNames()
        {
            return _storageProvider.GetAllKeys();
        }
    }
}
