using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepositoryApp.Repository;
using System.Text.Json;

namespace RepositoryApp.Tests
{
    [TestClass]
    public class EnhancedRepositoryTests
    {
        private InMemoryRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            var storageProvider = new InMemoryStorageProvider<string>();
            var validator = new DefaultContentValidator();
            _repository = new InMemoryRepository(storageProvider, validator);
            _repository.Initialize();
        }

        #region Validation Tests (Feature #5)

        [TestMethod]
        public void Register_ValidJson_ShouldSucceed()
        {
            // Arrange
            var validJson = "{\"Name\":\"Test\",\"Price\":10.99,\"StockQuantity\":5}";

            // Act
            _repository.Register("ValidProduct", validJson, 1);
            var retrieved = _repository.Retrieve("ValidProduct");

            // Assert
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(validJson, retrieved);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Register_InvalidJson_ShouldThrowValidationException()
        {
            // Arrange
            var invalidJson = "{invalid json content";

            // Act
            _repository.Register("InvalidProduct", invalidJson, 1);

            // Assert - Exception expected
        }

        [TestMethod]
        public void Register_ValidXml_ShouldSucceed()
        {
            // Arrange
            var validXml = "<Product><Name>Test</Name><Price>10.99</Price></Product>";

            // Act
            _repository.Register("ValidXmlProduct", validXml, 2);
            var retrieved = _repository.Retrieve("ValidXmlProduct");

            // Assert
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(validXml, retrieved);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Register_InvalidXml_ShouldThrowValidationException()
        {
            // Arrange
            var invalidXml = "<Product><Name>Test</Product>";

            // Act
            _repository.Register("InvalidXmlProduct", invalidXml, 2);

            // Assert - Exception expected
        }

        #endregion

        #region Overwrite Protection Tests (Feature #6)

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Register_DuplicateItemName_ShouldThrowException()
        {
            // Arrange
            var content1 = "{\"Name\":\"Product1\",\"Price\":10.99,\"StockQuantity\":5}";
            var content2 = "{\"Name\":\"Product1\",\"Price\":20.99,\"StockQuantity\":10}";

            // Act
            _repository.Register("Product1", content1, 1);
            _repository.Register("Product1", content2, 1); // Should throw

            // Assert - Exception expected
        }

        [TestMethod]
        public void Update_ExistingItem_ShouldSucceed()
        {
            // Arrange
            var originalContent = "{\"Name\":\"Product1\",\"Price\":10.99,\"StockQuantity\":5}";
            var updatedContent = "{\"Name\":\"Product1\",\"Price\":20.99,\"StockQuantity\":10}";

            // Act
            _repository.Register("Product1", originalContent, 1);
            _repository.Update("Product1", updatedContent, 1);
            var retrieved = _repository.Retrieve("Product1");

            // Assert
            Assert.AreEqual(updatedContent, retrieved);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Update_NonExistentItem_ShouldThrowException()
        {
            // Arrange
            var content = "{\"Name\":\"Product1\",\"Price\":10.99,\"StockQuantity\":5}";

            // Act
            _repository.Update("NonExistent", content, 1); // Should throw

            // Assert - Exception expected
        }

        [TestMethod]
        public void DeregisterAndRegister_SameItemName_ShouldSucceed()
        {
            // Arrange
            var content1 = "{\"Name\":\"Product1\",\"Price\":10.99,\"StockQuantity\":5}";
            var content2 = "{\"Name\":\"Product1\",\"Price\":20.99,\"StockQuantity\":10}";

            // Act
            _repository.Register("Product1", content1, 1);
            _repository.Deregister("Product1");
            _repository.Register("Product1", content2, 1); // Should succeed after deregister

            var retrieved = _repository.Retrieve("Product1");

            // Assert
            Assert.AreEqual(content2, retrieved);
        }

        #endregion

        #region Initialize Once Tests (Feature #7)

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Initialize_CalledTwice_ShouldThrowException()
        {
            // Arrange
            var storageProvider = new InMemoryStorageProvider<string>();
            var validator = new DefaultContentValidator();
            var repository = new InMemoryRepository(storageProvider, validator);

            // Act
            repository.Initialize();
            repository.Initialize(); // Should throw

            // Assert - Exception expected
        }

        [TestMethod]
        public void Initialize_CalledOnce_ShouldClearRepository()
        {
            // Arrange
            var storageProvider = new InMemoryStorageProvider<string>();
            var validator = new DefaultContentValidator();
            var repository = new InMemoryRepository(storageProvider, validator);

            // Add some data before initialization
            var item = new RepositoryItem<string> { Content = "test", Type = 1 };
            storageProvider.Store("PreInitItem", item);

            // Act
            repository.Initialize();
            var retrieved = repository.Retrieve("PreInitItem");

            // Assert
            Assert.IsNull(retrieved); // Should be cleared by Initialize
        }

        #endregion

        #region Generic Type Support Tests (Feature #8)

        [TestMethod]
        public void GenericStorageProvider_StringType_ShouldWork()
        {
            // Arrange
            var storageProvider = new InMemoryStorageProvider<string>();
            var item = new RepositoryItem<string>
            {
                Content = "test content",
                Type = 1
            };

            // Act
            storageProvider.Store("key1", item);
            var retrieved = storageProvider.Retrieve("key1");

            // Assert
            Assert.IsNotNull(retrieved);
            Assert.AreEqual("test content", retrieved.Content);
            Assert.AreEqual(1, retrieved.Type);
        }

        [TestMethod]
        public void GenericStorageProvider_ByteArrayType_ShouldWork()
        {
            // Arrange
            var storageProvider = new InMemoryStorageProvider<byte[]>();
            byte[] data = new byte[] { 1, 2, 3, 4, 5 };
            var item = new RepositoryItem<byte[]>
            {
                Content = data,
                Type = 3
            };

            // Act
            storageProvider.Store("binary1", item);
            var retrieved = storageProvider.Retrieve("binary1");

            // Assert
            Assert.IsNotNull(retrieved);
            CollectionAssert.AreEqual(data, retrieved.Content);
            Assert.AreEqual(3, retrieved.Type);
        }

        #endregion

        #region Storage Provider Tests (Feature #9)

        [TestMethod]
        public void InMemoryStorageProvider_BasicOperations_ShouldWork()
        {
            // Arrange
            var provider = new InMemoryStorageProvider<string>();
            var item = new RepositoryItem<string> { Content = "test", Type = 1 };

            // Act & Assert
            Assert.IsFalse(provider.Exists("key1"));

            provider.Store("key1", item);
            Assert.IsTrue(provider.Exists("key1"));

            var retrieved = provider.Retrieve("key1");
            Assert.IsNotNull(retrieved);
            Assert.AreEqual("test", retrieved.Content);

            provider.Remove("key1");
            Assert.IsFalse(provider.Exists("key1"));
        }

        [TestMethod]
        public void InMemoryStorageProvider_GetAllKeys_ShouldReturnAllKeys()
        {
            // Arrange
            var provider = new InMemoryStorageProvider<string>();
            var item1 = new RepositoryItem<string> { Content = "content1", Type = 1 };
            var item2 = new RepositoryItem<string> { Content = "content2", Type = 2 };

            // Act
            provider.Store("key1", item1);
            provider.Store("key2", item2);
            var keys = provider.GetAllKeys().ToList();

            // Assert
            Assert.AreEqual(2, keys.Count);
            Assert.IsTrue(keys.Contains("key1"));
            Assert.IsTrue(keys.Contains("key2"));
        }

        [TestMethod]
        public void InMemoryStorageProvider_Clear_ShouldRemoveAllItems()
        {
            // Arrange
            var provider = new InMemoryStorageProvider<string>();
            provider.Store("key1", new RepositoryItem<string> { Content = "test1", Type = 1 });
            provider.Store("key2", new RepositoryItem<string> { Content = "test2", Type = 1 });

            // Act
            provider.Clear();
            var keys = provider.GetAllKeys().ToList();

            // Assert
            Assert.AreEqual(0, keys.Count);
        }

        [TestMethod]
        public void FileStorageProvider_BasicOperations_ShouldWork()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), "RepositoryTest_" + Guid.NewGuid());
            var provider = new FileStorageProvider<string>(tempPath);
            var item = new RepositoryItem<string> { Content = "file test", Type = 1 };

            try
            {
                // Act & Assert
                provider.Store("filekey1", item);
                Assert.IsTrue(provider.Exists("filekey1"));

                var retrieved = provider.Retrieve("filekey1");
                Assert.IsNotNull(retrieved);
                Assert.AreEqual("file test", retrieved.Content);

                // Verify persistence - create new provider with same path
                var provider2 = new FileStorageProvider<string>(tempPath);
                var persistedItem = provider2.Retrieve("filekey1");
                Assert.IsNotNull(persistedItem);
                Assert.AreEqual("file test", persistedItem.Content);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        #endregion

        #region Thread Safety Tests

        [TestMethod]
        public void Repository_ConcurrentRegistrations_WithValidation_ShouldSucceed()
        {
            // Arrange
            var tasks = new List<Task>();
            var itemCount = 100;
            var exceptions = new List<Exception>();

            // Act - Multiple threads registering items with validation
            for (int i = 0; i < itemCount; i++)
            {
                var index = i;
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var itemName = $"Product{index}";
                        var itemContent = $"{{\"Name\":\"Product{index}\",\"Price\":{index}.99,\"StockQuantity\":{index}}}";
                        _repository.Register(itemName, itemContent, 1);
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            // Assert - All items should be retrievable, no exceptions
            Assert.AreEqual(0, exceptions.Count, $"Unexpected exceptions: {string.Join(", ", exceptions.Select(e => e.Message))}");
            
            for (int i = 0; i < itemCount; i++)
            {
                var itemName = $"Product{i}";
                var retrieved = _repository.Retrieve(itemName);
                Assert.IsNotNull(retrieved, $"Item {itemName} should exist");
            }
        }

        #endregion
    }
}
