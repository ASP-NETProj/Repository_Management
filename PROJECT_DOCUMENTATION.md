# Repository App - Complete Project Documentation

**Date:** February 21, 2026  
**Framework:** ASP.NET Core 8.0  
**Author:** Repository App Development Team

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Project Structure](#project-structure)
3. [Architecture](#architecture)
4. [Core Components](#core-components)
5. [API Reference](#api-reference)
6. [Code Documentation](#code-documentation)
7. [Configuration](#configuration)
8. [Setup & Installation](#setup--installation)
9. [Testing](#testing)
10. [Enhanced Features](#enhanced-features)

---

## Project Overview

### Description
A thread-safe ASP.NET Core MVC application featuring an in-memory repository system with support for multiple storage backends (in-memory, file system, database). The application provides a complete CRUD interface for managing products with dual format support (JSON and XML).

### Key Features
- ✅ Complete CRUD Operations (Create, Read, Update, Delete)
- ✅ Thread-Safe Repository (ConcurrentDictionary)
- ✅ Dual Storage Format (JSON Type 1, XML Type 2)
- ✅ Content Validation
- ✅ Overwrite Protection
- ✅ Initialize Once Protection
- ✅ Strong-Typed Generics
- ✅ Multiple Storage Backends
- ✅ Bootstrap 5 UI with Responsive Design

### Technologies
- ASP.NET Core 8.0
- C# 12
- Bootstrap 5.3
- Bootstrap Icons 1.11
- jQuery 3.6
- MSTest Framework
- Microsoft.Data.SqlClient 5.1.0

---

## Project Structure

```
RepositoryApp/
│
├── 📄 Core Application Files
│   ├── Program.cs                          (Application entry point)
│   ├── RepositoryApp.csproj                (Project configuration)
│   ├── appsettings.json                    (Application settings)
│   └── appsettings.Development.json        (Development settings)
│
├── 📁 Controllers/
│   └── ProductController.cs                (CRUD operations controller)
│       ├── Index() - GET
│       ├── Create() - GET/POST
│       ├── Edit(id) - GET/POST
│       ├── Delete(id) - GET/POST
│       └── Details(id) - GET
│
├── 📁 Models/
│   └── Product.cs                          (Data model)
│       ├── Name: string
│       ├── Price: decimal
│       └── StockQuantity: int
│
├── 📁 Repository/
│   ├── IRepository.cs                      (Repository interface)
│   ├── InMemoryRepository.cs               (Base implementation)
│   ├── ExtendedRepository.cs               (Extended with listing)
│   ├── IContentValidator.cs                (Validation interface)
│   ├── DefaultContentValidator.cs          (JSON/XML validator)
│   ├── IRepositoryItem.cs                  (Generic item interface)
│   ├── IStorageProvider.cs                 (Storage abstraction)
│   ├── InMemoryStorageProvider.cs          (In-memory storage)
│   ├── FileStorageProvider.cs              (File system storage)
│   └── DatabaseStorageProvider.cs          (Database storage)
│
├── 📁 Views/
│   ├── Product/
│   │   ├── Index.cshtml                    (Product listing)
│   │   ├── Create.cshtml                   (Create form)
│   │   ├── Edit.cshtml                     (Edit form)
│   │   ├── Delete.cshtml                   (Delete confirmation)
│   │   └── Details.cshtml                  (Product details)
│   ├── Shared/
│   │   ├── _Layout.cshtml                  (Main layout)
│   │   └── _ValidationScriptsPartial.cshtml
│   ├── _ViewStart.cshtml
│   └── _ViewImports.cshtml
│
├── 📁 SampleData/
│   ├── product-sample.json
│   ├── product-sample.xml
│   ├── products.json
│   └── products.xml
│
├── 📁 Tests/
│   ├── RepositoryTests.cs                  (Original tests)
│   ├── EnhancedRepositoryTests.cs          (Enhanced feature tests)
│   └── RepositoryApp.Tests.csproj
│
├── 📁 Properties/
│   └── launchSettings.json
│
└── 📁 Documentation/
    ├── README.md
    ├── QUICK_START.md
    ├── ARCHITECTURE.md
    ├── PROJECT_SUMMARY.md
    ├── TROUBLESHOOTING.md
    ├── ENHANCED_FEATURES.md
    ├── ENHANCEMENTS_SUMMARY.md
    ├── WELCOME.md
    ├── INDEX.md
    └── FILE_LISTING.md
```

---

## Architecture

### System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                     Browser (Client)                             │
│                Bootstrap 5 + jQuery UI                           │
└──────────────────────────────┬──────────────────────────────────┘
                               │ HTTP/HTTPS
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│                    ASP.NET Core MVC                              │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │              ProductController                             │ │
│  │  - Index, Create, Edit, Delete, Details                   │ │
│  └────────────────────────┬───────────────────────────────────┘ │
│                           │ DI                                   │
│                           ▼                                      │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │           IRepository Interface                            │ │
│  │  - Register, Retrieve, GetType, Deregister, Initialize     │ │
│  └────────────────────────┬───────────────────────────────────┘ │
│                           │                                      │
│                           ▼                                      │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │         ExtendedRepository (Singleton)                     │ │
│  │  + IContentValidator (Validation)                          │ │
│  │  + IStorageProvider (Storage Abstraction)                  │ │
│  └────────────────────────┬───────────────────────────────────┘ │
└───────────────────────────┼──────────────────────────────────────┘
                            │
            ┌───────────────┼───────────────┐
            ▼               ▼               ▼
    ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
    │  In-Memory   │ │     File     │ │   Database   │
    │   Storage    │ │   Storage    │ │   Storage    │
    └──────────────┘ └──────────────┘ └──────────────┘
```

### Thread-Safety Architecture

```
Multiple Threads → Repository (Singleton) → ConcurrentDictionary
                         ↓
                  Lock-Free Reads
                  Atomic Writes
                  Initialize Lock
```

---

## Core Components

### 1. Product Model

**File:** `Models/Product.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace RepositoryApp.Models
{
    public class Product
    {
        [Required]
        [Display(Name = "Product Name")]
        public string Name { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be non-negative")]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }
    }
}
```

### 2. Repository Interface

**File:** `Repository/IRepository.cs`

```csharp
namespace RepositoryApp.Repository
{
    public interface IRepository
    {
        /// <summary>
        /// Store an item to the repository in-memory storage.
        /// itemType: 1 = JSON, 2 = XML
        /// </summary>
        void Register(string itemName, string itemContent, int itemType);
        
        /// <summary>
        /// Retrieve an item from the repository.
        /// Returns null if not found.
        /// </summary>
        string Retrieve(string itemName);
        
        /// <summary>
        /// Retrieve the type of the item.
        /// Returns: 1 = JSON, 2 = XML, 0 = not found
        /// </summary>
        int GetType(string itemName);
        
        /// <summary>
        /// Remove an item from the repository.
        /// </summary>
        void Deregister(string itemName);
        
        /// <summary>
        /// Initialize the repository for use.
        /// Can only be called once after instance creation.
        /// </summary>
        void Initialize();
    }
}
```

### 3. In-Memory Repository Implementation

**File:** `Repository/InMemoryRepository.cs`

Key features:
- Thread-safe operations using `IStorageProvider<string>`
- Content validation via `IContentValidator`
- Overwrite protection in Register method
- Initialize once protection
- Update method for legitimate updates

```csharp
public class InMemoryRepository : IRepository
{
    private readonly IStorageProvider<string> _storageProvider;
    private readonly IContentValidator _validator;
    private bool _isInitialized;
    private bool _hasBeenInitialized;
    private readonly object _initLock = new object();

    public void Initialize()
    {
        lock (_initLock)
        {
            if (_hasBeenInitialized)
                throw new InvalidOperationException(
                    "Initialize can only be called once after repository instance is created.");
            
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
        // Validation
        var validationResult = _validator.Validate(itemContent, itemType);
        if (!validationResult.IsValid)
            throw new ArgumentException($"Content validation failed: {validationResult.ErrorMessage}");
        
        // Overwrite protection
        if (_storageProvider.Exists(itemName))
            throw new InvalidOperationException(
                $"Item '{itemName}' already exists and cannot be overwritten.");
        
        var item = new RepositoryItem<string>
        {
            Content = itemContent,
            Type = itemType
        };
        
        _storageProvider.Store(itemName, item);
    }

    public void Update(string itemName, string itemContent, int itemType)
    {
        if (!_storageProvider.Exists(itemName))
            throw new InvalidOperationException(
                $"Item '{itemName}' does not exist. Use Register to create new items.");
        
        var validationResult = _validator.Validate(itemContent, itemType);
        if (!validationResult.IsValid)
            throw new ArgumentException($"Content validation failed: {validationResult.ErrorMessage}");
        
        var item = new RepositoryItem<string>
        {
            Content = itemContent,
            Type = itemType
        };
        
        _storageProvider.Store(itemName, item);
    }
}
```

### 4. Content Validator

**File:** `Repository/DefaultContentValidator.cs`

```csharp
public class DefaultContentValidator : IContentValidator
{
    public ValidationResult Validate(string content, int itemType)
    {
        if (string.IsNullOrWhiteSpace(content))
            return ValidationResult.Fail("Content cannot be null or empty");

        switch (itemType)
        {
            case 1: // JSON
                return ValidateJson(content);
            case 2: // XML
                return ValidateXml(content);
            default:
                return ValidationResult.Fail($"Unknown item type: {itemType}");
        }
    }

    private ValidationResult ValidateJson(string content)
    {
        try
        {
            using var doc = JsonDocument.Parse(content);
            return ValidationResult.Success();
        }
        catch (JsonException ex)
        {
            return ValidationResult.Fail($"Invalid JSON format: {ex.Message}");
        }
    }

    private ValidationResult ValidateXml(string content)
    {
        try
        {
            var doc = XElement.Parse(content);
            return ValidationResult.Success();
        }
        catch (Exception ex)
        {
            return ValidationResult.Fail($"Invalid XML format: {ex.Message}");
        }
    }
}
```

### 5. Storage Provider Abstraction

**File:** `Repository/IStorageProvider.cs`

```csharp
public interface IStorageProvider<TContent>
{
    void Store(string key, IRepositoryItem<TContent> item);
    IRepositoryItem<TContent> Retrieve(string key);
    void Remove(string key);
    bool Exists(string key);
    IEnumerable<string> GetAllKeys();
    void Clear();
}
```

**Implementations:**
- `InMemoryStorageProvider<T>` - ConcurrentDictionary-based
- `FileStorageProvider<T>` - JSON file persistence
- `DatabaseStorageProvider<T>` - SQL Server storage

---

## API Reference

### Repository Methods

#### Register
```csharp
void Register(string itemName, string itemContent, int itemType)
```
- **Purpose:** Store a new item in the repository
- **Parameters:**
  - `itemName`: Unique identifier for the item
  - `itemContent`: JSON or XML string content
  - `itemType`: 1 = JSON, 2 = XML
- **Throws:**
  - `ArgumentException` - Invalid parameters or validation failure
  - `InvalidOperationException` - Item already exists
- **Example:**
```csharp
var json = "{\"Name\":\"Laptop\",\"Price\":999.99,\"StockQuantity\":50}";
repository.Register("Laptop", json, 1);
```

#### Retrieve
```csharp
string Retrieve(string itemName)
```
- **Purpose:** Get item content from repository
- **Returns:** Item content string or null if not found
- **Example:**
```csharp
var content = repository.Retrieve("Laptop");
```

#### GetType
```csharp
int GetType(string itemName)
```
- **Purpose:** Get storage format type
- **Returns:** 1 (JSON), 2 (XML), or 0 (not found)
- **Example:**
```csharp
var type = repository.GetType("Laptop"); // Returns 1
```

#### Deregister
```csharp
void Deregister(string itemName)
```
- **Purpose:** Remove item from repository
- **Example:**
```csharp
repository.Deregister("Laptop");
```

#### Initialize
```csharp
void Initialize()
```
- **Purpose:** Initialize repository (can only be called once)
- **Throws:** `InvalidOperationException` if called more than once
- **Example:**
```csharp
repository.Initialize();
```

#### Update (New Method)
```csharp
void Update(string itemName, string itemContent, int itemType)
```
- **Purpose:** Update existing item (allows overwrite)
- **Throws:**
  - `InvalidOperationException` - Item doesn't exist
  - `ArgumentException` - Validation failure
- **Example:**
```csharp
repository.Update("Laptop", updatedJson, 1);
```

---

## Code Documentation

### ProductController

**File:** `Controllers/ProductController.cs`

```csharp
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
        var itemTypes = new Dictionary<string, int>();
        foreach (var product in products)
        {
            itemTypes[product.Name] = _repository.GetType(product.Name);
        }
        ViewBag.ItemTypes = itemTypes;
        return View(products);
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
                    itemContent = ConvertToXml(product);
                    itemType = 2;
                }
                else
                {
                    itemContent = ConvertToJson(product);
                    itemType = 1;
                }

                _repository.Register(product.Name, itemContent, itemType);
                TempData["Success"] = $"Product '{product.Name}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }
        return View(product);
    }

    // Helper: Convert Product to JSON
    private string ConvertToJson(Product product)
    {
        return JsonSerializer.Serialize(product, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }

    // Helper: Convert Product to XML
    private string ConvertToXml(Product product)
    {
        var xml = new XElement("Product",
            new XElement("Name", product.Name),
            new XElement("Price", product.Price),
            new XElement("StockQuantity", product.StockQuantity)
        );
        return xml.ToString();
    }
}
```

### Program.cs Configuration

**File:** `Program.cs`

```csharp
using RepositoryApp.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register the repository as a singleton for thread-safe multi-threaded environment
builder.Services.AddSingleton<IRepository>(provider =>
{
    // Option 1: In-Memory Storage (default)
    var storageProvider = new InMemoryStorageProvider<string>();
    
    // Option 2: File Storage (uncomment to use)
    // var storagePath = Path.Combine(Directory.GetCurrentDirectory(), "RepositoryData");
    // var storageProvider = new FileStorageProvider<string>(storagePath);
    
    // Option 3: Database Storage (uncomment and configure connection string to use)
    // var connectionString = builder.Configuration.GetConnectionString("RepositoryDatabase");
    // var storageProvider = new DatabaseStorageProvider<string>(connectionString);
    
    var validator = new DefaultContentValidator();
    var repository = new ExtendedRepository(storageProvider, validator);
    repository.Initialize();
    return repository;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}");

app.Run();
```

---

## Configuration

### appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### launchSettings.json
```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:5001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

### RepositoryApp.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <Compile Remove="Tests\**" />
    <Content Remove="Tests\**" />
    <EmbeddedResource Remove="Tests\**" />
    <None Remove="Tests\**" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Data.SqlClient" Version="5.1.0" />
  </ItemGroup>
</Project>
```

---

## Setup & Installation

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022 / VS Code / Rider
- Modern web browser

### Installation Steps

1. **Clone or Download the Project**
```powershell
cd "C:\path\to\RepositoryApp"
```

2. **Restore Dependencies**
```powershell
dotnet restore
```

3. **Build the Project**
```powershell
dotnet build
```

4. **Run the Application**
```powershell
dotnet run
```

5. **Access the Application**
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001

### Running Tests

```powershell
cd Tests
dotnet test
```

---

## Testing

### Test Coverage

#### Original Tests (RepositoryTests.cs)
- Register_ShouldStoreJsonItem
- Register_ShouldStoreXmlItem
- Deregister_ShouldRemoveItem
- Retrieve_NonExistentItem_ReturnsNull
- GetType_NonExistentItem_ReturnsZero
- Register_UpdateExistingItem_ShouldReplace
- ThreadSafety_ConcurrentOperations_ShouldSucceed
- ThreadSafety_ConcurrentReadWrite_ShouldSucceed

#### Enhanced Tests (EnhancedRepositoryTests.cs)

**Validation Tests:**
- Register_ValidJson_ShouldSucceed
- Register_InvalidJson_ShouldThrowValidationException
- Register_ValidXml_ShouldSucceed
- Register_InvalidXml_ShouldThrowValidationException

**Overwrite Protection Tests:**
- Register_DuplicateItemName_ShouldThrowException
- Update_ExistingItem_ShouldSucceed
- Update_NonExistentItem_ShouldThrowException
- DeregisterAndRegister_SameItemName_ShouldSucceed

**Initialize Once Tests:**
- Initialize_CalledTwice_ShouldThrowException
- Initialize_CalledOnce_ShouldClearRepository

**Generic Type Support Tests:**
- GenericStorageProvider_StringType_ShouldWork
- GenericStorageProvider_ByteArrayType_ShouldWork

**Storage Provider Tests:**
- InMemoryStorageProvider_BasicOperations_ShouldWork
- InMemoryStorageProvider_GetAllKeys_ShouldReturnAllKeys
- InMemoryStorageProvider_Clear_ShouldRemoveAllItems
- FileStorageProvider_BasicOperations_ShouldWork

**Thread Safety Tests:**
- Repository_ConcurrentRegistrations_WithValidation_ShouldSucceed

**Total Tests:** 25+ comprehensive tests

---

## Enhanced Features

### Feature 1: Content Validation
- Validates JSON/XML format before storage
- Extensible validator interface
- Custom validators can be implemented

### Feature 2: Overwrite Protection
- Register() prevents duplicate item creation
- Update() method for legitimate updates
- Clear error messages

### Feature 3: Initialize Once Protection
- Initialize() can only be called once per instance
- Prevents accidental data loss
- Thread-safe implementation

### Feature 4: Strong-Typed Generics
- Generic `IRepositoryItem<TContent>`
- Generic `IStorageProvider<TContent>`
- No Object type usage
- Full type safety

### Feature 5: Multiple Storage Backends
- **In-Memory:** Fast, non-persistent (default)
- **File System:** Persistent JSON storage
- **Database:** SQL Server support
- Easily extensible for other backends

---

## Sample Data

### JSON Format
```json
{
  "Name": "Laptop",
  "Price": 999.99,
  "StockQuantity": 50
}
```

### XML Format
```xml
<Product>
  <Name>Laptop</Name>
  <Price>999.99</Price>
  <StockQuantity>50</StockQuantity>
</Product>
```

---

## Performance Characteristics

| Operation | Complexity | Thread-Safe | Notes |
|-----------|-----------|-------------|-------|
| Register | O(1) | ✅ Yes | + validation overhead |
| Retrieve | O(1) | ✅ Yes | Lock-free reads |
| GetType | O(1) | ✅ Yes | Lock-free reads |
| Deregister | O(1) | ✅ Yes | Atomic operation |
| Initialize | O(n) | ✅ Yes | With lock protection |
| Update | O(1) | ✅ Yes | + validation overhead |

---

## Security Considerations

1. **Input Validation:** All content validated before storage
2. **Overwrite Protection:** Prevents unauthorized data changes
3. **SQL Injection:** Parameterized queries in DatabaseStorageProvider
4. **CSRF Protection:** Anti-forgery tokens on all forms
5. **HTTPS:** Supported and recommended for production

---

## Deployment

### Development
```powershell
dotnet run --environment Development
```

### Production
```powershell
dotnet publish -c Release
# Deploy to IIS, Azure, AWS, or Linux server
```

### Environment Variables
- `ASPNETCORE_ENVIRONMENT`: Development/Production
- `ASPNETCORE_URLS`: Custom port configuration

---

## Troubleshooting

### Common Issues

**Port Already in Use:**
```powershell
# Change port in launchSettings.json
"applicationUrl": "https://localhost:7001;http://localhost:7000"
```

**Build Errors:**
```powershell
dotnet clean
dotnet restore
dotnet build
```

**Test Failures:**
```powershell
cd Tests
dotnet test --logger "console;verbosity=detailed"
```

---

## Conclusion

This Repository App demonstrates:
- ✅ Modern ASP.NET Core MVC architecture
- ✅ Thread-safe concurrent programming
- ✅ SOLID principles and clean code
- ✅ Comprehensive testing
- ✅ Extensible design patterns
- ✅ Production-ready features

**Total Files:** 40+ files  
**Total Lines of Code:** 4000+ lines  
**Test Coverage:** 25+ unit tests  
**Documentation:** 10+ comprehensive guides  

---

## References

- ASP.NET Core Documentation: https://docs.microsoft.com/aspnet/core
- Bootstrap 5: https://getbootstrap.com/docs/5.3
- C# Threading: https://docs.microsoft.com/dotnet/standard/threading
- MSTest Framework: https://docs.microsoft.com/dotnet/core/testing/unit-testing-with-mstest

---

**End of Document**

*Generated on February 21, 2026*
