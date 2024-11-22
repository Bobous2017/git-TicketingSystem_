

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

// ** Add Session Services **
builder.Services.AddDistributedMemoryCache(); // Use in-memory cache for storing session data
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true; // Security: Make session cookie HTTP-only
    options.Cookie.IsEssential = true; // Make session cookie essential
});

// Set the URLs to listen on
// Configure Kestrel to listen on the specified IP address and port using HTTP
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Parse(NetworkUtils.GlobalIPAddress()), 5003); // Use HTTP
});

// Get database type from configuration
var databaseType = builder.Configuration.GetSection("DatabaseSettings:DataSourceType").Value;
Console.WriteLine($"Database Type: {databaseType}");  // Debugging purpose

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        
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
app.UseSession();
app.UseCors("AllowSpecificOrigin");
app.UseAuthentication(); // Include UseAuthentication() here, which is missing
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();