//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddControllersWithViews();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();
//app.UseRouting();


//app.UseAuthorization();

//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllerRoute(
//        name: "default",
//       pattern: "{controller=Home}/{action=Index}/{id?}");
//});


//app.Run();


using TicketingSystemLibrary;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


// Set the URLs to listen on
builder.WebHost.UseUrls($"https://{NetworkUtils.GlobalIPAddress()}:5003", "https://127.0.0.1:5003");
//builder.WebHost.UseUrls($"https://");

//builder.Services.AddScoped<DatabaseService>();
//builder.Services.AddScoped<IDataAccess, SqliteDataAccess>();
// Get database type from configuration
var databaseType = builder.Configuration.GetSection("DatabaseSettings:DataSourceType").Value;
Console.WriteLine($"Database Type: {databaseType}");  // Debugging purpose

//switch (databaseType?.ToLower())
//{
//    case "sql":
//        //builder.Services.AddScoped<IDataAccess<HistoryGenerateSql>, SqlDataAccess>(); // Register SQL database service with Scoped lifetime for SQL.
//        break;
//    default:
//        throw new Exception("Invalid data source type in configuration."); // Throw an exception if the data source type is invalid.
//}

builder.Services.AddControllers();
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
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();