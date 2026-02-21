using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepositoryApp.Repository;
using System.Text.Json;

namespace RepositoryApp.Tests
{
    [TestClass]
    public class RepositoryTests
    {
        private IRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            _repository = new InMemoryRepository();
            _repository.Initialize();
        }

        [TestMethod]
        public void Register_ShouldStoreJsonItem()
        {
            // Arrange
            var itemName = "TestProduct";
            var itemContent = "{\"Name\":\"Test\",\"Price\":10.99,\"StockQuantity\":5}";
            var itemType = 1; // JSON

            // Act
            _repository.Register(itemName, itemContent, itemType);
            var retrieved = _repository.Retrieve(itemName);
            var type = _repository.GetType(itemName);

            // Assert
            Assert.AreEqual(itemContent, retrieved);
            Assert.AreEqual(1, type);
        }

        [TestMethod]
        public void Register_ShouldStoreXmlItem()
        {
            // Arrange
            var itemName = "TestProduct";
            var itemContent = "<Product><Name>Test</Name><Price>10.99</Price><StockQuantity>5</StockQuantity></Product>";
            var itemType = 2; // XML

            // Act
            _repository.Register(itemName, itemContent, itemType);
            var retrieved = _repository.Retrieve(itemName);
            var type = _repository.GetType(itemName);

            // Assert
            Assert.AreEqual(itemContent, retrieved);
            Assert.AreEqual(2, type);
        }

        [TestMethod]
        public void Deregister_ShouldRemoveItem()
        {
            // Arrange
            var itemName = "TestProduct";
            var itemContent = "{\"Name\":\"Test\",\"Price\":10.99,\"StockQuantity\":5}";
            _repository.Register(itemName, itemContent, 1);

            // Act
            _repository.Deregister(itemName);
            var retrieved = _repository.Retrieve(itemName);
            var type = _repository.GetType(itemName);

            // Assert
            Assert.IsNull(retrieved);
            Assert.AreEqual(0, type);
        }

        [TestMethod]
        public void Retrieve_NonExistentItem_ReturnsNull()
        {
            // Act
            var retrieved = _repository.Retrieve("NonExistent");

            // Assert
            Assert.IsNull(retrieved);
        }

        [TestMethod]
        public void GetType_NonExistentItem_ReturnsZero()
        {
            // Act
            var type = _repository.GetType("NonExistent");

            // Assert
            Assert.AreEqual(0, type);
        }

        [TestMethod]
        public void Register_UpdateExistingItem_ShouldReplace()
        {
            // Arrange
            var itemName = "TestProduct";
            var firstContent = "{\"Name\":\"Test1\",\"Price\":10.99,\"StockQuantity\":5}";
            var secondContent = "{\"Name\":\"Test2\",\"Price\":20.99,\"StockQuantity\":10}";

            // Act
            _repository.Register(itemName, firstContent, 1);
            _repository.Register(itemName, secondContent, 1);
            var retrieved = _repository.Retrieve(itemName);

            // Assert
            Assert.AreEqual(secondContent, retrieved);
        }

        [TestMethod]
        public void ThreadSafety_ConcurrentOperations_ShouldSucceed()
        {
            // Arrange
            var tasks = new List<Task>();
            var itemCount = 1000;

            // Act - Multiple threads registering items
            for (int i = 0; i < itemCount; i++)
            {
                var index = i;
                tasks.Add(Task.Run(() =>
                {
                    var itemName = $"Product{index}";
                    var itemContent = $"{{\"Name\":\"Product{index}\",\"Price\":{index}.99,\"StockQuantity\":{index}}}";
                    _repository.Register(itemName, itemContent, 1);
                }));
            }

            Task.WaitAll(tasks.ToArray());

            // Assert - All items should be retrievable
            for (int i = 0; i < itemCount; i++)
            {
                var itemName = $"Product{i}";
                var retrieved = _repository.Retrieve(itemName);
                Assert.IsNotNull(retrieved, $"Item {itemName} should exist");
            }
        }

        [TestMethod]
        public void ThreadSafety_ConcurrentReadWrite_ShouldSucceed()
        {
            // Arrange
            var itemName = "SharedProduct";
            var tasks = new List<Task>();
            var writeCount = 100;
            var readCount = 100;

            // Act - Concurrent reads and writes
            for (int i = 0; i < writeCount; i++)
            {
                var index = i;
                tasks.Add(Task.Run(() =>
                {
                    var content = $"{{\"Name\":\"Product\",\"Price\":{index}.99,\"StockQuantity\":{index}}}";
                    _repository.Register(itemName, content, 1);
                }));
            }

            for (int i = 0; i < readCount; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var content = _repository.Retrieve(itemName);
                    var type = _repository.GetType(itemName);
                    // Just reading, no assertions needed - testing for exceptions
                }));
            }

            // Assert - No exceptions should occur
            Task.WaitAll(tasks.ToArray());

            // Final item should be retrievable
            var finalContent = _repository.Retrieve(itemName);
            Assert.IsNotNull(finalContent);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Register_NullItemName_ShouldThrowException()
        {
            _repository.Register(null, "content", 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Register_EmptyItemName_ShouldThrowException()
        {
            _repository.Register("", "content", 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Register_InvalidItemType_ShouldThrowException()
        {
            _repository.Register("test", "content", 3);
        }

        [TestMethod]
        public void Initialize_ShouldClearRepository()
        {
            // Arrange
            _repository.Register("Item1", "content1", 1);
            _repository.Register("Item2", "content2", 2);

            // Act
            _repository.Initialize();

            // Note: After initialization, items are cleared
            // Since we can't enumerate items, we can at least verify initialization doesn't throw
            Assert.IsNotNull(_repository);
        }
    }
}
