using BooknGo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
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
            var allBookings = _context.Bookings
                .Select(b => new Booking
                {
                    EventId = b.EventId,
                    Name = b.Name ?? string.Empty,
                    Description = b.Description ?? string.Empty,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    Location = b.Location ?? string.Empty,
                    Capacity = b.Capacity,
                    Category = b.Category ?? string.Empty
                }).ToList();

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
        
        [HttpPost]
        public IActionResult CreateEditBookingForm(Booking model)
        {
            if (!ModelState.IsValid)
            {
                return View("CreateEditBooking", model);
            }

            string? connectionString = _configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                ModelState.AddModelError("", "Database connection string is not configured.");
                return View("CreateEditBooking", model);
            }

            try
            {
                // Check DB connection
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    var cmd = new SqlCommand("SELECT DB_NAME()", con);
                    string currentDb = (string)cmd.ExecuteScalar();
                    Console.WriteLine("Connected to database: " + currentDb);
                }

                if (model.EventId > 0)
                {
                    // Update booking
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        string updateQuery = @"
                    UPDATE Booking
                    SET Name = @Name,
                        Description = @Description,
                        StartDate = @StartDate,
                        EndDate = @EndDate,
                        Location = @Location,
                        Capacity = @Capacity,
                        Category = @Category
                    WHERE EventId = @EventId";

                        using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                        {
                            cmd.Parameters.Add("@EventId", SqlDbType.Int).Value = model.EventId;
                            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 200).Value = model.Name ?? (object)DBNull.Value;
                            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value = model.Description ?? (object)DBNull.Value;
                            cmd.Parameters.Add("@StartDate", SqlDbType.DateTime).Value = model.StartDate;
                            cmd.Parameters.Add("@EndDate", SqlDbType.DateTime).Value = model.EndDate;
                            cmd.Parameters.Add("@Location", SqlDbType.NVarChar, 200).Value = model.Location ?? (object)DBNull.Value;
                            cmd.Parameters.Add("@Capacity", SqlDbType.Int).Value = model.Capacity;
                            cmd.Parameters.Add("@Category", SqlDbType.NVarChar, 100).Value = model.Category ?? (object)DBNull.Value;

                            con.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }

                    TempData["Success"] = "Booking updated successfully!";
                }
                else
                {
                    // Insert booking
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        string insertQuery = @"
                    INSERT INTO Booking (Name, Description, StartDate, EndDate, Location, Capacity, Category)
                    VALUES (@Name, @Description, @StartDate, @EndDate, @Location, @Capacity, @Category)";

                        using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                        {
                            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 200).Value = model.Name ?? (object)DBNull.Value;
                            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value = model.Description ?? (object)DBNull.Value;
                            cmd.Parameters.Add("@StartDate", SqlDbType.DateTime).Value = model.StartDate;
                            cmd.Parameters.Add("@EndDate", SqlDbType.DateTime).Value = model.EndDate;
                            cmd.Parameters.Add("@Location", SqlDbType.NVarChar, 200).Value = model.Location ?? (object)DBNull.Value;
                            cmd.Parameters.Add("@Capacity", SqlDbType.Int).Value = model.Capacity;
                            cmd.Parameters.Add("@Category", SqlDbType.NVarChar, 100).Value = model.Category ?? (object)DBNull.Value;

                            con.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }

                    TempData["Success"] = "Booking created successfully!";
                }

                return RedirectToAction("Bookings");
            }   
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
                return View("CreateEditBooking", model);
            }
        }
    }
}
