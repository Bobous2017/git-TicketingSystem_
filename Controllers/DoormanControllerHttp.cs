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
    [Route("Doorman")]
    public class DoormanControllerHttp : Controller
    {
        private readonly HttpClient _httpClient;
        public DoormanControllerHttp(HttpClient httpClient)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            // Initialize HttpClient with the custom handler
            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri($"http://{NetworkUtils.GlobalIPAddress()}:5000/") // Base address for your API
            };
        }

        // GET Doorman -> GetAllDoormen() via http. GET Endpoint : api/doormen
        [HttpGet("")] // Route for the Index page //----------------------------------------------------Website Tested Ok
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("api/doormen");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var doormen = JsonSerializer.Deserialize<List<Doorman>>(json);
                if (doormen != null)
                {
                    return View(doormen);
                }
            }

            return View(new List<Doorman>());
        }

        // GET: /DoormanHttp/FilterById
        [HttpGet("FilterById")] // Filtering the Doorman by ID//----------------------------------------Website Tested Ok
        public async Task<IActionResult> FilterById(int doormanId)
        {
            var response = await _httpClient.GetAsync($"api/doormen/{doormanId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var doorman = JsonSerializer.Deserialize<Doorman>(json);

                if (doorman != null)
                {
                    // Wrap the doorman into a list so that it can be used in the view
                    return View("Index", new List<Doorman> { doorman });
                }
            }
            // If no doorman is found, return an empty list to the view
            return View("Index", new List<Doorman>());
        }

        // GET: /DoormanHttp/Create
        [Route("Create")] //-----------------------------------------------------------------------------Website Tested Ok
        public IActionResult Create()
        {
            return View(); // This will load Create.cshtml view
        }

        // POST: /DoormanHttp/Create
        [HttpPost("Create")] //-------------------------------------------------------------------------Website Tested Ok
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Doorman doorman)
        {
            if (ModelState.IsValid)
            {
                var json = JsonSerializer.Serialize(doorman);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/doormen", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(doorman);
        }

        // GET: /DoormanHttp/Edit/5
        [HttpGet("Edit/{id}")] //-----------------------------------------------------------------------Website Tested Ok
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"api/doormen/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var doorman = JsonSerializer.Deserialize<Doorman>(json);
                return View(doorman);
            }
            return View("Error");
        }

        // POST: /DoormanHttp/Edit/5
        [HttpPost("Edit/{id}")] //-----------------------------------------------------------------------Website Tested Ok
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Doorman doorman)
        {
            if (id != doorman.DoormanID)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var json = JsonSerializer.Serialize(doorman);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"api/doormen/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(doorman);
        }

        // GET: /DoormanControllerHttp/Delete/5
        [HttpGet("Delete/{id}")] //----------------------------------------------------------------------Website Tested Ok
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.GetAsync($"api/doormen/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var doorman = JsonSerializer.Deserialize<Doorman>(json);
                return View(doorman);
            }
            return View("Error");
        }

        // POST: /DoormanControllerHttp/Delete/5
        [HttpPost("DeleteConfirmed")]
        [ValidateAntiForgeryToken] //--------------------------------------------------------------------Website Tested Ok
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/doormen/{id}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            return View("Error");
        }
    }
}
