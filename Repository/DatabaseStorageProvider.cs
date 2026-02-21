using System.Data;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace RepositoryApp.Repository
{
    /// <summary>
    /// SQL Server database storage provider
    /// This is a template - connection string and table schema should be configured
    /// </summary>
    /// <typeparam name="TContent">Type of content being stored</typeparam>
    public class DatabaseStorageProvider<TContent> : IStorageProvider<TContent>
    {
        private readonly string _connectionString;
        private readonly string _tableName;

        public DatabaseStorageProvider(string connectionString, string tableName = "RepositoryItems")
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _tableName = tableName;
            EnsureTableExists();
        }

        private void EnsureTableExists()
        {
            // This would create the table if it doesn't exist
            // SQL: CREATE TABLE IF NOT EXISTS RepositoryItems (
            //   ItemKey NVARCHAR(255) PRIMARY KEY,
            //   Content NVARCHAR(MAX),
            //   ItemType INT,
            //   CreatedAt DATETIME,
            //   UpdatedAt DATETIME
            // )
        }

        public void Store(string key, IRepositoryItem<TContent> item)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand($@"
                    MERGE {_tableName} AS target
                    USING (SELECT @Key AS ItemKey) AS source
                    ON target.ItemKey = source.ItemKey
                    WHEN MATCHED THEN
                        UPDATE SET Content = @Content, ItemType = @Type, UpdatedAt = GETDATE()
                    WHEN NOT MATCHED THEN
                        INSERT (ItemKey, Content, ItemType, CreatedAt, UpdatedAt)
                        VALUES (@Key, @Content, @Type, GETDATE(), GETDATE());
                ", connection);

                command.Parameters.AddWithValue("@Key", key);
                command.Parameters.AddWithValue("@Content", SerializeContent(item.Content));
                command.Parameters.AddWithValue("@Type", item.Type);
                command.ExecuteNonQuery();
            }
        }

        public IRepositoryItem<TContent> Retrieve(string key)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand($@"
                    SELECT Content, ItemType FROM {_tableName} WHERE ItemKey = @Key
                ", connection);
                command.Parameters.AddWithValue("@Key", key);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new RepositoryItem<TContent>
                        {
                            Content = DeserializeContent(reader.GetString(0)),
                            Type = reader.GetInt32(1)
                        };
                    }
                }
            }
            return null;
        }

        public void Remove(string key)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand($@"
                    DELETE FROM {_tableName} WHERE ItemKey = @Key
                ", connection);
                command.Parameters.AddWithValue("@Key", key);
                command.ExecuteNonQuery();
            }
        }

        public bool Exists(string key)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand($@"
                    SELECT COUNT(*) FROM {_tableName} WHERE ItemKey = @Key
                ", connection);
                command.Parameters.AddWithValue("@Key", key);
                return (int)command.ExecuteScalar() > 0;
            }
        }

        public IEnumerable<string> GetAllKeys()
        {
            var keys = new List<string>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand($@"
                    SELECT ItemKey FROM {_tableName}
                ", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        keys.Add(reader.GetString(0));
                    }
                }
            }
            return keys;
        }

        public void Clear()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand($@"
                    DELETE FROM {_tableName}
                ", connection);
                command.ExecuteNonQuery();
            }
        }

        private string SerializeContent(TContent content)
        {
            if (content is string str)
                return str;
            return JsonSerializer.Serialize(content);
        }

        private TContent DeserializeContent(string content)
        {
            if (typeof(TContent) == typeof(string))
                return (TContent)(object)content;
            return JsonSerializer.Deserialize<TContent>(content);
        }
    }
}
