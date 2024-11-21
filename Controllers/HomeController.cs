using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Claims;
using TicketingSystemLibrary;

namespace TicketingSystemAPI.Controllers
{
    
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


        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("Login")]
        public IActionResult Login()
        {
            return View();
        }
        //[HttpPost("Login")]  
        [HttpPost]
        [Route("Login")] // 
        public async Task<IActionResult> Login(string email)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/administrators/login", new { email });

                if (response.IsSuccessStatusCode)
                {
                    // Simulate creating user identity after successful login
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, email)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Administrator");
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

        [Route("Administrator")]
        public IActionResult Administrator()
        {
            return View();
        }

        [Route("Privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

      
    }
}