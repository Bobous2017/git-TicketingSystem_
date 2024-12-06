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
    [Route("Customer")]
    public class CustomerControllerHttp : Controller
    {
        private readonly HttpClient _httpClient;
        public CustomerControllerHttp(HttpClient httpClient)
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

        // GET Customer -> GetAllCustomers() via http. GET Endpoint : api/customers
        [HttpGet("")] // Route for the Index page //----------------------------------------------------Website Tested Ok
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("api/customers");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var customers = JsonSerializer.Deserialize<List<Customer>>(json);
                if (customers != null)
                {
                    return View(customers);
                }
            }

            return View(new List<Customer>());
        }

        // GET: /CustomerHttp/FilterById
        [HttpGet("FilterById")] // Filtering the Customer by ID//----------------------------------------Website Tested Ok
        public async Task<IActionResult> FilterById(int customerId)
        {
            var response = await _httpClient.GetAsync($"api/customers/{customerId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var customer = JsonSerializer.Deserialize<Customer>(json);

                if (customer != null)
                {
                    // Wrap the customer into a list so that it can be used in the view
                    return View("Index", new List<Customer> { customer });
                }
            }
            // If no customer is found, return an empty list to the view
            return View("Index", new List<Customer>());
        }

        // GET: /CustomerHttp/Create
        [Route("Create")] //-----------------------------------------------------------------------------Website Tested Ok
        public IActionResult Create()
        {
            return View(); // This will load Create.cshtml view
        }

        // POST: /CustomerHttp/Create
        [HttpPost("Create")] //-------------------------------------------------------------------------Website Tested Ok
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                var json = JsonSerializer.Serialize(customer);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/customers", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(customer);
        }

        // GET: /CustomerHttp/Edit/5
        [HttpGet("Edit/{id}")] //-----------------------------------------------------------------------Website Tested Ok
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"api/customers/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var customer = JsonSerializer.Deserialize<Customer>(json);
                return View(customer);
            }
            return View("Error");
        }

        // POST: /CustomerHttp/Edit/5
        [HttpPost("Edit/{id}")] //-----------------------------------------------------------------------Website Tested Ok
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.CustomerID)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var json = JsonSerializer.Serialize(customer);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"api/customers/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(customer);
        }

        // GET: /CustomerControllerHttp/Delete/5
        [HttpGet("Delete/{id}")] //----------------------------------------------------------------------Website Tested Ok
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.GetAsync($"api/customers/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var customer = JsonSerializer.Deserialize<Customer>(json);
                return View(customer);
            }
            return View("Error");
        }

        // POST: /CustomerControllerHttp/Delete/5
        [HttpPost("DeleteConfirmed")]
        [ValidateAntiForgeryToken] //--------------------------------------------------------------------Website Tested Ok
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/customers/{id}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            return View("Error");
        }
    }
}
