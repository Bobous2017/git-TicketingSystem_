using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Claims;
using TicketingSystemLibrary;
using System.Text.Json;
using TicketingSystemLibrary.Models;
using System.Text;

namespace TicketingSystemAPI.Controllers
{
    //-------------------------WEBSITE TICKETING SYSTEM------HomeController---------------------
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;

        public HomeController(ILogger<HomeController> logger, HttpClient httpClient)
        {
            _logger = logger;
            //_httpClient = httpClient;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            // Initialize HttpClient with the custom handler
            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri($"http://{NetworkUtils.GlobalIPAddress()}:5000/")  // Using HTTPS here
            };
        }
        
        [HttpGet("ip")] //  Get   the Global Ip address
        public IActionResult GetGlobalIpAddress()
        {
            var ipAddress = NetworkUtils.GlobalIPAddress();
            return Ok(new { ip = ipAddress });
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Fetch all events via the API
                var response = await _httpClient.GetAsync("api/events");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var events = JsonSerializer.Deserialize<List<Event>>(json);

                    if (events != null)
                    {
                        // Pass the model (list of events) to the view
                        return View(events);
                    }
                }

                // If the API call fails or returns nothing, return an empty list to the view
                return View(new List<Event>());
            }
            catch (Exception ex)
            {
                // Handle errors (log them, or show an error message)
                Console.WriteLine($"Error fetching events: {ex.Message}");
                ViewBag.ErrorMessage = "Unable to load events at the moment.";
                return View(new List<Event>());
            }
        }

        [Route("Login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(string email)
        {
            try
            {
                // Make a POST request to your API to authenticate the administrator
                var response = await _httpClient.PostAsJsonAsync("api/administrators/login", new { email });

                if (response.IsSuccessStatusCode)
                {
                    // Deserialize the response to get the "admin" object
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var jsonDocument = JsonDocument.Parse(responseJson);

                    if (jsonDocument.RootElement.TryGetProperty("admin", out JsonElement adminElement))
                    {
                        var admin = JsonSerializer.Deserialize<Administrator>(adminElement.ToString());

                        if (admin == null)
                        {
                            ViewBag.ErrorMessage = "Invalid credentials. Please try again.";
                            return View();
                        }

                        // Create claims with the user's information
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, admin.Name),
                            new Claim("AdminID", admin.AdminId.ToString()),  // Custom claim to hold AdminID
                            new Claim("AdminEmail", admin.Email) // Custom claim to hold Admin Email
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                        // Redirect to Administrator Dashboard
                        return Redirect("/Administrator");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Invalid credentials. Please try again.";
                        return View();
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid credentials. Please try again.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"An error occurred: {ex.Message}";
                return View();
            }
        }


        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }


        [Route("")]
        [Route("Privacy")]
        public IActionResult Privacy()
        {
            return View();
        }


    }
}
