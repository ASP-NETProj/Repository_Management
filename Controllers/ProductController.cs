using Microsoft.AspNetCore.Mvc;
using RepositoryApp.Models;
using RepositoryApp.Repository;
using System.Text.Json;
using System.Xml.Linq;

namespace RepositoryApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly ExtendedRepository _repository;

        public ProductController(IRepository repository)
        {
            _repository = (ExtendedRepository)repository;
        }

        // GET: Product
        public IActionResult Index()
        {
            var products = GetAllProducts();
            
            // Create a dictionary to store item types for each product
            var itemTypes = new Dictionary<string, int>();
            foreach (var product in products)
            {
                itemTypes[product.Name] = _repository.GetType(product.Name);
            }
            
            ViewBag.ItemTypes = itemTypes;
            return View(products);
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product, string storageFormat = "json")
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string itemContent;
                    int itemType;

                    if (storageFormat.ToLower() == "xml")
                    {
                        // Store as XML
                        itemContent = ConvertToXml(product);
                        itemType = 2;
                    }
                    else
                    {
                        // Store as JSON
                        itemContent = ConvertToJson(product);
                        itemType = 1;
                    }

                    _repository.Register(product.Name, itemContent, itemType);
                    TempData["Success"] = $"Product '{product.Name}' created successfully as {storageFormat.ToUpper()}!";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    // Item already exists - overwrite protection
                    ModelState.AddModelError("", ex.Message);
                }
                catch (ArgumentException ex)
                {
                    // Validation failed
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating product: {ex.Message}");
                }
            }
            return View(product);
        }

        // GET: Product/Edit/ProductName
        public IActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var product = GetProductByName(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.ItemType = _repository.GetType(id);
            return View(product);
        }

        // POST: Product/Edit/ProductName
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, Product product, string storageFormat = "json")
        {
            if (id != product.Name)
            {
                ModelState.AddModelError("", "Product name cannot be changed.");
                return View(product);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string itemContent;
                    int itemType;

                    if (storageFormat.ToLower() == "xml")
                    {
                        itemContent = ConvertToXml(product);
                        itemType = 2;
                    }
                    else
                    {
                        itemContent = ConvertToJson(product);
                        itemType = 1;
                    }

                    // Use Update method to allow overwrite for existing items
                    ((InMemoryRepository)_repository).Update(product.Name, itemContent, itemType);
                    TempData["Success"] = $"Product '{product.Name}' updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    // Validation failed
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating product: {ex.Message}");
                }
            }
            return View(product);
        }

        // GET: Product/Delete/ProductName
        public IActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var product = GetProductByName(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.ItemType = _repository.GetType(id);
            return View(product);
        }

        // POST: Product/Delete/ProductName
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            try
            {
                _repository.Deregister(id);
                TempData["Success"] = $"Product '{id}' deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting product: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Product/Details/ProductName
        public IActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var product = GetProductByName(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.ItemType = _repository.GetType(id);
            ViewBag.RawContent = _repository.Retrieve(id);
            return View(product);
        }

        #region Helper Methods

        private List<Product> GetAllProducts()
        {
            var products = new List<Product>();
            var itemNames = _repository.GetAllItemNames();
            
            foreach (var name in itemNames)
            {
                var product = GetProductByName(name);
                if (product != null)
                {
                    products.Add(product);
                }
            }
            
            return products;
        }

        private Product GetProductByName(string name)
        {
            var itemContent = _repository.Retrieve(name);
            if (string.IsNullOrEmpty(itemContent))
            {
                return null;
            }

            var itemType = _repository.GetType(name);
            
            try
            {
                if (itemType == 1) // JSON
                {
                    return JsonSerializer.Deserialize<Product>(itemContent);
                }
                else if (itemType == 2) // XML
                {
                    return ParseXmlToProduct(itemContent);
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private string ConvertToJson(Product product)
        {
            return JsonSerializer.Serialize(product, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
        }

        private string ConvertToXml(Product product)
        {
            var xml = new XElement("Product",
                new XElement("Name", product.Name),
                new XElement("Price", product.Price),
                new XElement("StockQuantity", product.StockQuantity)
            );
            return xml.ToString();
        }

        private Product ParseXmlToProduct(string xml)
        {
            var xElement = XElement.Parse(xml);
            return new Product
            {
                Name = xElement.Element("Name")?.Value,
                Price = decimal.Parse(xElement.Element("Price")?.Value ?? "0"),
                StockQuantity = int.Parse(xElement.Element("StockQuantity")?.Value ?? "0")
            };
        }

        #endregion
    }
}
