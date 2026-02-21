# Architecture Diagram

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         Browser (User)                          │
│                    Bootstrap 5 + jQuery UI                      │
└──────────────────────────────┬──────────────────────────────────┘
                               │ HTTP/HTTPS
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│                      ASP.NET Core 8.0                           │
│                                                                 │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │              ProductController (MVC)                      │ │
│  │  ┌─────────────────────────────────────────────────────┐ │ │
│  │  │  Index() - List products                            │ │ │
│  │  │  Create() - Create new product                      │ │ │
│  │  │  Edit() - Update product                            │ │ │
│  │  │  Delete() - Remove product                          │ │ │
│  │  │  Details() - View product details                   │ │ │
│  │  └─────────────────────────────────────────────────────┘ │ │
│  └───────────────────────────┬─────────────────────────────────┘ │
│                              │ Dependency Injection              │
│                              ▼                                   │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │           IRepository Interface (Thread-Safe API)         │ │
│  │  ┌─────────────────────────────────────────────────────┐ │ │
│  │  │  Register(name, content, type)                      │ │ │
│  │  │  Retrieve(name) → string                            │ │ │
│  │  │  GetType(name) → int                                │ │ │
│  │  │  Deregister(name)                                   │ │ │
│  │  │  Initialize()                                       │ │ │
│  │  └─────────────────────────────────────────────────────┘ │ │
│  └───────────────────────────┬─────────────────────────────────┘ │
│                              │ Implementation                    │
│                              ▼                                   │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │          ExtendedRepository (Singleton)                   │ │
│  │  ┌─────────────────────────────────────────────────────┐ │ │
│  │  │  ConcurrentDictionary<string, RepositoryItem>       │ │ │
│  │  │  ┌────────────────────────────────────────────────┐ │ │ │
│  │  │  │  Key: ItemName (string)                        │ │ │ │
│  │  │  │  Value: {Content: string, Type: int}           │ │ │ │
│  │  │  │                                                 │ │ │ │
│  │  │  │  Type 1 = JSON                                 │ │ │ │
│  │  │  │  Type 2 = XML                                  │ │ │ │
│  │  │  └────────────────────────────────────────────────┘ │ │ │
│  │  └─────────────────────────────────────────────────────┘ │ │
│  └───────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

## Data Flow - Create Product

```
1. User Input
   ┌──────────────────────┐
   │ Name: "Laptop"       │
   │ Price: 999.99        │
   │ Stock: 50            │
   │ Format: JSON         │
   └──────────┬───────────┘
              │ POST
              ▼
2. ProductController.Create()
   ┌──────────────────────────────────┐
   │ Convert Product to JSON/XML      │
   │ json = JsonSerializer.Serialize()│
   └──────────┬───────────────────────┘
              │
              ▼
3. Repository.Register()
   ┌──────────────────────────────────┐
   │ _storage.AddOrUpdate(            │
   │   "Laptop",                      │
   │   { Content: json, Type: 1 }     │
   │ )                                │
   └──────────┬───────────────────────┘
              │
              ▼
4. ConcurrentDictionary (Thread-Safe)
   ┌──────────────────────────────────┐
   │ ["Laptop"] = {                   │
   │   Content: "{ Name:..., }",      │
   │   Type: 1                        │
   │ }                                │
   └──────────┬───────────────────────┘
              │
              ▼
5. Response
   ┌──────────────────────────────────┐
   │ Redirect to Index                │
   │ Success Message Displayed        │
   └──────────────────────────────────┘
```

## Thread Safety Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                   Multi-Threaded Environment                │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Thread 1              Thread 2              Thread 3      │
│     │                     │                     │          │
│     ▼                     ▼                     ▼          │
│  Register()           Retrieve()            GetType()      │
│     │                     │                     │          │
│     └─────────────────────┴─────────────────────┘          │
│                           │                                │
│                           ▼                                │
│              ┌───────────────────────────┐                 │
│              │  ExtendedRepository       │                 │
│              │  (Singleton Instance)     │                 │
│              └───────────┬───────────────┘                 │
│                          │                                 │
│                          ▼                                 │
│              ┌───────────────────────────┐                 │
│              │  ConcurrentDictionary     │                 │
│              │  ┌─────────────────────┐  │                 │
│              │  │  Lock-Free Reads   │  │                 │
│              │  │  Atomic Writes     │  │                 │
│              │  │  No Race Conditions│  │                 │
│              │  └─────────────────────┘  │                 │
│              └───────────────────────────┘                 │
│                                                             │
│  ✓ Thread-Safe Operations                                  │
│  ✓ No Deadlocks                                            │
│  ✓ High Concurrency                                        │
│  ✓ Lock-Free Reads                                         │
└─────────────────────────────────────────────────────────────┘
```

## Repository Storage Structure

```
ConcurrentDictionary<string, RepositoryItem>
├── "Laptop" ──────────► { Content: "{ Name: 'Laptop', ... }", Type: 1 }
├── "Mouse" ───────────► { Content: "<Product>...</Product>", Type: 2 }
├── "Keyboard" ────────► { Content: "{ Name: 'Keyboard', ... }", Type: 1 }
└── "Monitor" ─────────► { Content: "<Product>...</Product>", Type: 2 }

Legend:
  Type 1 = JSON format
  Type 2 = XML format
```

## MVC Pattern Flow

```
┌─────────────┐       ┌──────────────┐       ┌────────────┐
│    View     │◄──────│  Controller  │──────►│   Model    │
│  (Razor)    │       │  (Product)   │       │ (Product)  │
└─────────────┘       └──────┬───────┘       └────────────┘
      ▲                      │
      │                      │
      │                      ▼
      │              ┌───────────────┐
      │              │  Repository   │
      │              │  (Business    │
      │              │   Logic)      │
      └──────────────┴───────────────┘
          Render Result
```

## User Interface Flow

```
┌─────────────────────────────────────────────────────────────┐
│                        Index Page                           │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  [Add New Product]                                    │  │
│  │                                                       │  │
│  │  ┌──────────┬────────┬────────┬────────┬──────────┐  │  │
│  │  │   Name   │  Price │  Stock │ Format │  Actions │  │  │
│  │  ├──────────┼────────┼────────┼────────┼──────────┤  │  │
│  │  │ Laptop   │ $999   │   50   │  JSON  │ [👁][✏][🗑]│  │  │
│  │  │ Mouse    │ $25    │  200   │  XML   │ [👁][✏][🗑]│  │  │
│  │  └──────────┴────────┴────────┴────────┴──────────┘  │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
        │              │              │              │
        ▼              ▼              ▼              ▼
    Details        Create          Edit           Delete
```

## Project Dependencies

```
RepositoryApp.csproj
├── Microsoft.NET.Sdk.Web
│   ├── ASP.NET Core 8.0
│   ├── Razor View Engine
│   └── Built-in DI Container
├── System.Text.Json (JSON serialization)
├── System.Xml.Linq (XML processing)
└── System.Collections.Concurrent (Thread-safety)

RepositoryApp.Tests.csproj
├── Microsoft.NET.Test.Sdk
├── MSTest.TestAdapter
├── MSTest.TestFramework
└── coverlet.collector (Code coverage)
```

## Deployment Architecture

```
Development:
  ┌──────────────────────────┐
  │  Visual Studio / VS Code │
  │  dotnet run              │
  │  localhost:5000/5001     │
  └──────────────────────────┘

Production (Example):
  ┌──────────────────────────┐
  │  IIS / Kestrel Server    │
  │  Linux/Windows Server    │
  │  Reverse Proxy (Nginx)   │
  │  HTTPS with SSL Cert     │
  └──────────────────────────┘
```

## Memory Management

```
Application Startup
        │
        ▼
┌───────────────────┐
│  Program.cs       │
│  Initialize DI    │
└────────┬──────────┘
         │
         ▼
┌───────────────────────────┐
│  Create Repository        │
│  (Singleton)              │
│  repository.Initialize()  │
└────────┬──────────────────┘
         │
         ▼
┌───────────────────────────────────┐
│  ExtendedRepository Instance      │
│  Lives for entire app lifetime    │
│  Shared across all requests       │
│  Thread-safe ConcurrentDictionary │
└───────────────────────────────────┘
         │
         ▼
┌───────────────────────────────────┐
│  All data stored in memory        │
│  Lost on application restart      │
│  Fast access (no disk I/O)        │
└───────────────────────────────────┘
```

---

**Legend:**
- ► : Data flow direction
- ├── : Hierarchy/dependency
- ┌──┐ : Component/module
- ✓ : Feature/capability
- [👁] : View button
- [✏] : Edit button
- [🗑] : Delete button
