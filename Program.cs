using RepositoryApp.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register the repository as a singleton for thread-safe multi-threaded environment
// You can choose different storage providers:
// 1. In-Memory Storage (default, fast but non-persistent)
// 2. File Storage (persists to disk)
// 3. Database Storage (persists to SQL Server or other database)

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
