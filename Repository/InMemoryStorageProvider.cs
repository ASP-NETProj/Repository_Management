using System.Collections.Concurrent;

namespace RepositoryApp.Repository
{
    /// <summary>
    /// In-memory storage provider using ConcurrentDictionary
    /// </summary>
    /// <typeparam name="TContent">Type of content being stored</typeparam>
    public class InMemoryStorageProvider<TContent> : IStorageProvider<TContent>
    {
        private readonly ConcurrentDictionary<string, IRepositoryItem<TContent>> _storage;

        public InMemoryStorageProvider()
        {
            _storage = new ConcurrentDictionary<string, IRepositoryItem<TContent>>();
        }

        public void Store(string key, IRepositoryItem<TContent> item)
        {
            _storage[key] = item;
        }

        public IRepositoryItem<TContent> Retrieve(string key)
        {
            return _storage.TryGetValue(key, out var item) ? item : null;
        }

        public void Remove(string key)
        {
            _storage.TryRemove(key, out _);
        }

        public bool Exists(string key)
        {
            return _storage.ContainsKey(key);
        }

        public IEnumerable<string> GetAllKeys()
        {
            return _storage.Keys.ToList();
        }

        public void Clear()
        {
            _storage.Clear();
        }
    }
}
