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
    [Route("Event")]
    public class EventControllerHttp : Controller
    {
        private readonly HttpClient _httpClient;
        public EventControllerHttp(HttpClient httpClient)
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

        // GET Event -> GetAllEvents() via http. GET Endpoint : api/events
        [HttpGet("")] // Route for the Index page //----------------------------------------------------Website Tested  Ok
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("api/events");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var events = JsonSerializer.Deserialize<List<Event>>(json);
                if (events != null)
                {
                    return View(events);
                }
            }

            return View(new List<Event>());
        }

        // GET: /EventHttp/FilterById
        [HttpGet("FilterById")] // Filtering the Event by ID //-----------------------------------------Website Tested  Ok
        public async Task<IActionResult> FilterById(int eventId)
        {
            var response = await _httpClient.GetAsync($"api/events/{eventId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var eventObj = JsonSerializer.Deserialize<Event>(json);

                if (eventObj != null)
                {
                    // Wrap the event into a list so that it can be used in the view
                    return View("Index", new List<Event> { eventObj });
                }
            }
            // If no event is found, return an empty list to the view
            return View("Index", new List<Event>());
        }

        // GET: /EventHttp/Create
        [Route("Create")] //----------------------------------------------------------------------------Website Tested Ok
        public IActionResult Create()
        {
            return View(); // This will load Create.cshtml view
        }

        // POST: /EventHttp/Create
        [HttpPost("Create")] //--------------------------------------------------------------------------Website Tested Ok
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event eventObj)
        {
            if (ModelState.IsValid)
            {
                var json = JsonSerializer.Serialize(eventObj);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/events", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(eventObj);
        }

        // GET: /EventHttp/Edit/5
        [HttpGet("Edit/{id}")] //------------------------------------------------------------------------Website Tested  Ok
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"api/events/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var eventObj = JsonSerializer.Deserialize<Event>(json);
                return View(eventObj);
            }
            return View("Error");
        }

        // POST: /EventHttp/Edit/5
        [HttpPost("Edit/{id}")] //-----------------------------------------------------------------------Website Tested  Ok
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event eventObj)
        {
            if (id != eventObj.EventID)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var json = JsonSerializer.Serialize(eventObj);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"api/events/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(eventObj);
        }

        // GET: /EventControllerHttp/Delete/5
        [HttpGet("Delete/{id}")] //----------------------------------------------------------------------Website Tested Ok
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.GetAsync($"api/events/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var eventObj = JsonSerializer.Deserialize<Event>(json);
                return View(eventObj);
            }
            return View("Error");
        }

        // POST: /EventControllerHttp/Delete/5
        [HttpPost("DeleteConfirmed")]
        [ValidateAntiForgeryToken] //--------------------------------------------------------------------Website Tested Ok
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/events/{id}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            return View("Error");
        }
    }
}
