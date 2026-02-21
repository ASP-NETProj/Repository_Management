using System.Text.Json;

namespace RepositoryApp.Repository
{
    /// <summary>
    /// File-based storage provider that persists data to JSON files
    /// </summary>
    /// <typeparam name="TContent">Type of content being stored</typeparam>
    public class FileStorageProvider<TContent> : IStorageProvider<TContent>
    {
        private readonly string _storagePath;
        private readonly object _fileLock = new object();
        private readonly Dictionary<string, IRepositoryItem<TContent>> _cache;

        public FileStorageProvider(string storagePath)
        {
            _storagePath = storagePath ?? throw new ArgumentNullException(nameof(storagePath));
            _cache = new Dictionary<string, IRepositoryItem<TContent>>();
            
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }

            LoadFromDisk();
        }

        public void Store(string key, IRepositoryItem<TContent> item)
        {
            lock (_fileLock)
            {
                _cache[key] = item;
                SaveToDisk();
            }
        }

        public IRepositoryItem<TContent> Retrieve(string key)
        {
            lock (_fileLock)
            {
                return _cache.TryGetValue(key, out var item) ? item : null;
            }
        }

        public void Remove(string key)
        {
            lock (_fileLock)
            {
                _cache.Remove(key);
                SaveToDisk();
            }
        }

        public bool Exists(string key)
        {
            lock (_fileLock)
            {
                return _cache.ContainsKey(key);
            }
        }

        public IEnumerable<string> GetAllKeys()
        {
            lock (_fileLock)
            {
                return _cache.Keys.ToList();
            }
        }

        public void Clear()
        {
            lock (_fileLock)
            {
                _cache.Clear();
                SaveToDisk();
            }
        }

        private void LoadFromDisk()
        {
            var filePath = Path.Combine(_storagePath, "repository.json");
            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    var items = JsonSerializer.Deserialize<Dictionary<string, RepositoryItem<TContent>>>(json);
                    if (items != null)
                    {
                        foreach (var kvp in items)
                        {
                            _cache[kvp.Key] = kvp.Value;
                        }
                    }
                }
                catch
                {
                    // If file is corrupted, start fresh
                    _cache.Clear();
                }
            }
        }

        private void SaveToDisk()
        {
            var filePath = Path.Combine(_storagePath, "repository.json");
            var json = JsonSerializer.Serialize(_cache, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }
    }
}
