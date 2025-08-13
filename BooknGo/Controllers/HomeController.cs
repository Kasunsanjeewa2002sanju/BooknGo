using BooknGo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace BooknGo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BooknGODbContext _context;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, BooknGODbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Bookings()
        {
            var allBookings = _context.Bookings.ToList(); // Works now
            return View(allBookings);
        }

        public IActionResult DeleteBooking(int id)
        {
            var bookingInDb = _context.Bookings.SingleOrDefault(b => b.EventId == id);
            if (bookingInDb != null)
            {
                _context.Bookings.Remove(bookingInDb);
                _context.SaveChanges();
            }
            return RedirectToAction("Bookings");
        }

        [HttpGet]
        public IActionResult CreateEditBooking()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateEditBookingForm(Booking model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            // Optional: check DB connection
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                var cmd = new SqlCommand("SELECT DB_NAME()", con);
                string currentDb = (string)cmd.ExecuteScalar();
                Console.WriteLine("Connected to database: " + currentDb);
            }

            // Insert booking
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    INSERT INTO Booking (EventId, Name, Description, StartDate, EndDate, Location, Capacity)
                    VALUES (@EventId, @Name, @Description, @StartDate, @EndDate, @Location, @Capacity)";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@EventId", model.EventId);
                    cmd.Parameters.AddWithValue("@Name", model.Name);
                    cmd.Parameters.AddWithValue("@Description", model.Description);
                    cmd.Parameters.AddWithValue("@StartDate", model.StartDate);
                    cmd.Parameters.AddWithValue("@EndDate", model.EndDate);
                    cmd.Parameters.AddWithValue("@Location", model.Location);
                    cmd.Parameters.AddWithValue("@Capacity", model.Capacity);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            TempData["Success"] = "Booking created successfully!";
            return RedirectToAction("Bookings");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
