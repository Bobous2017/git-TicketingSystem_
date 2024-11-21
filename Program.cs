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


using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net;
using TicketingSystemLibrary;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Add authentication services
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login";
                    options.LogoutPath = "/Logout";
                    options.AccessDeniedPath = "/AccessDenied";
                });

// Set the URLs to listen on
// Configure Kestrel to listen on the specified IP address and port using HTTP
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Parse(NetworkUtils.GlobalIPAddress()), 5003); // Use HTTP
});

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
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        //builder.WithOrigins("http://172.16.3.57:5000") // API origin URL (update to match your actual API URL)
        //       .AllowAnyHeader()
        //       .AllowAnyMethod()
        //       .AllowCredentials(); // If you use cookies or any kind of credentials, make sure to include this
        builder.AllowAnyOrigin()    // Allow requests from any origin (e.g., other domains, servers).
               .AllowAnyMethod()    // Allow all HTTP methods (GET, POST, PUT, DELETE, etc.).
               .AllowAnyHeader();   // Allow all HTTP headers.
    });
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
app.UseCors("AllowSpecificOrigin");
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();