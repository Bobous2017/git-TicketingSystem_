using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using TicketingSystemLibrary.Models;
using TicketingSystemLibrary;
using SkiaSharp;
using System.Diagnostics;
using System.Net.Sockets;

namespace TicketingSystem.Controllers
{
    [Route("Ticket")]
    public class TicketControllerHttp : Controller
    {
        private readonly HttpClient _httpClient;

        public TicketControllerHttp(HttpClient httpClient)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri($"http://{NetworkUtils.GlobalIPAddress()}:5000/") // Base address for your API
            };
        }

        
        [HttpGet("")]  // GET: /Ticket/AllTickets ----GET ALL TICKETS FOR ADMINISTRATOR--------Website Tested  Ok
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("api/tickets");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine("JSON Response: " + json); // Debugging line to see the raw JSON response

                var tickets = JsonSerializer.Deserialize<List<Ticket>>(json);

                if (tickets != null)
                {
                    Console.WriteLine("Deserialized Tickets: " + tickets.Count); // Debugging line to see the count of tickets
                    foreach (var ticket in tickets)
                    {
                        Console.WriteLine($"TicketID: {ticket.TicketID}, CustomerID: {ticket.CustomerID}, EventID: {ticket.EventID}");
                    }
                    tickets = tickets.OrderByDescending(t => t.TicketID).ToList();
                    return View(tickets);
                }
            }

            return View(new List<Ticket>());
        }


        // GET: /Ticket/Create/{eventId} //---------------GET TICKET BY ID----------------------Website Tested  Ok
        [HttpGet("Create/{eventId}")] // Correct GET route to load the Create form
        public async Task<IActionResult> Create(int eventId)
        {
            var response = await _httpClient.GetAsync($"api/events/{eventId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var eventObj = JsonSerializer.Deserialize<Event>(json);
                if (eventObj != null)
                {
                    var ticketBuying = new Ticket
                    {
                        EventID = eventId,
                        EventName = eventObj.EventName,
                    };

                    return View(ticketBuying); // Load the Create.cshtml form with event details
                }
            }
            return View("Error");
        }


        [HttpPost("Create")]    //------------------------Buy Tickets---------------------------Website Tested  Ok
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ticket ticket)
        {
            try
            {
                Console.WriteLine("Received POST request to Create method");
                Console.WriteLine($"CustomerName: {ticket.CustomerName}");
                Console.WriteLine($"Email: {ticket.Email}");
                Console.WriteLine($"PhoneNumber: {ticket.PhoneNumber}");
                Console.WriteLine($"EventID: {ticket.EventID}");
                Console.WriteLine($"EventName: {ticket.EventName}");

                if (ModelState.IsValid)
                {
                    // Generate AuthCode and QRCode
                    string authCode = new Random().Next(100000, 999999).ToString();
                    Console.WriteLine($"Generated AuthCode: {authCode}");

                    string content = $"{ticket.Email} {authCode}\n";

                    // Generate QR code using the GenerateQRCode class
                    var generateQR = new GenerateQRCode();
                    var qrCodeBitmap = new SKBitmap(300, 300);
                    var qrCodeStream = generateQR.GenerateQRCodeGen(content, qrCodeBitmap);

                    if (qrCodeStream == null)
                    {
                        Console.WriteLine("Failed to generate QR Code");
                        return StatusCode(500, "QR code generation failed.");
                    }

                    // Convert the QR code stream to a Base64 string
                    string qrCodeBase64;
                    using (var memoryStream = new MemoryStream())
                    {
                        qrCodeStream.CopyTo(memoryStream);
                        qrCodeBase64 = Convert.ToBase64String(memoryStream.ToArray());
                    }

                    Console.WriteLine("QR Code generated successfully");

                    // Prepare the Ticket object to send to the API
                    ticket.AuthCode = authCode;
                    ticket.QRCode = qrCodeBase64;
                    ticket.PurchaseDate = DateTime.Now;
                   
                    
                    // Serialize and send Ticket data to the API
                    var json = JsonSerializer.Serialize(ticket);
                    Console.WriteLine("Serialized Ticket JSON: " + json);

                    var contentData = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync("api/tickets", contentData);
                    Console.WriteLine("API Response Status Code: " + response.StatusCode);

                    string responseContent = await response.Content.ReadAsStringAsync();
                    var createdTicket = JsonSerializer.Deserialize<Ticket>(responseContent);
                    ticket.EventName = createdTicket.EventName;
                    ticket.EventDate = createdTicket.EventDate;
                    ticket.Location = createdTicket.Location;

                    Console.WriteLine("API Response Content: " + responseContent);
                    //string eventDateString = ticket.EventDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Date not available";


                    await generateQR.SendEmailWithQRCode(ticket.Email, ticket.EventName, ticket.Location, ticket.EventDate,  qrCodeBase64, authCode);
                    Console.WriteLine("Email with QR Code sent to: " + ticket.Email);



                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Success");
                    }
                }
                else
                {
                    Console.WriteLine("ModelState is invalid. Errors:");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        Console.WriteLine(" - " + error.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred in Create method: " + ex.Message);
            }

            return View(ticket);
        }


        [HttpGet("TicketsDetails")]  //---------------------Tickets Ticket Detail--------------Website Tested  Ok
        public async Task<IActionResult> TicketsDetails()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/tickets/details");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var ticketDetails = JsonSerializer.Deserialize<List<TicketDetails>>(json);

                    if (ticketDetails != null)
                    {
                        return View(ticketDetails);
                    }
                }

                return View(new List<TicketDetails>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("ScanTicketView")]  //---------------------Tickets Ticket Detail--------------Website Tested  Ok
        public async Task<IActionResult> ScanTicketView()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/tickets/scaned");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var scanTicketView = JsonSerializer.Deserialize<List<ScanTicketView>>(json);

                    if (scanTicketView != null)
                    {
                        return View(scanTicketView);
                    }
                }

                return View(new List<ScanTicketView>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }



        [Route("Success")]   //------------------------------------------------------------------Website Tested  Ok
        public IActionResult Success()
        {
            return View();
        }
    }
}
