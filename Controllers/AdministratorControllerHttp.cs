using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using TicketingSystemLibrary.Models;
using TicketingSystemLibrary;

namespace TicketingSystem.Controllers
{
    [Route("Administrator")]
    public class AdministratorControllerHttp : Controller
    {
        private readonly HttpClient _httpClient;

        public AdministratorControllerHttp(HttpClient httpClient)
        {
            
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

        //GET Administrator -> GetAllAdministrators()  via http. GET Endpoint :  api/administrators
        [HttpGet("")] //-----------------------------------------------------------------------------------------------------Website Tested
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("api/administrators");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine(json); // Add logging here to inspect the JSON data
                var administrators = JsonSerializer.Deserialize<List<Administrator>>(json);

                if (administrators != null)
                {
                    return View(administrators);
                }
            }

            return View(new List<Administrator>());
        }

        // GET: /AdministratorHttp/FilterById
        [HttpGet("FilterById")]//-------------------------------------------------------------------------------------------Website Tested
        public async Task<IActionResult> FilterById(int adminId)
        {
            var response = await _httpClient.GetAsync($"api/administrators/{adminId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var admin = JsonSerializer.Deserialize<Administrator>(json);

                if (admin != null)
                {
                    // Wrap the admin into a list so that it can be used in the view
                    return View("Index", new List<Administrator> { admin });
                }
            }
            // If no administrator is found, return an empty list to the view
            return View("Index", new List<Administrator>());
        }

        // GET: /AdministratorHttp/Details/5 //-----------------------------------------------------------------------------Website Tested
        public async Task<IActionResult> Details(int id)
        {
            var response = await _httpClient.GetAsync($"api/administrators/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var admin = JsonSerializer.Deserialize<Administrator>(json);
                return View(admin);
            }
            return View("Error");
        }

        // GET: /AdministratorHttp/Create //-------------------------------------------------------------------------------Website Tested
        [Route("Create")]
        public IActionResult Create()
        {
            return View(); // This will load Create.cshtml view
        }

        // POST: /AdministratorHttp/Create //-------------------------------------------------------------------------------Website Tested
        [HttpPost("Create")] // Adding the route explicitly
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Administrator admin)
        {
            if (ModelState.IsValid)
            {
                var json = JsonSerializer.Serialize(admin);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/administrators", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(admin);
        }

        // GET: /AdministratorHttp/Edit/5 //-------------------------------------------------------------------------------Website Tested
        [HttpGet("Edit/{id}")] // Adding route for GET request to edit an administrator.
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"api/administrators/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var admin = JsonSerializer.Deserialize<Administrator>(json);
                return View(admin);
            }
            return View("Error");
        }

        // POST: /AdministratorHttp/Edit/5//-------------------------------------------------------------------------------Website Tested
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Administrator admin)
        {
            if (id != admin.AdminId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var json = JsonSerializer.Serialize(admin);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"api/administrators/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(admin);
        }

        // GET: /AdministratorControllerHttp/Delete/5//---------------------------------------------------------------------Website Tested
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.GetAsync($"api/administrators/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var admin = JsonSerializer.Deserialize<Administrator>(json);
                return View(admin);
            }
            return View("Error");
        }

        // POST: /AdministratorControllerHttp/Delete/5//---------------------------------------------------------------------Website Tested
        [HttpPost("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/administrators/{id}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            return View("Error");
        }

    }
}
