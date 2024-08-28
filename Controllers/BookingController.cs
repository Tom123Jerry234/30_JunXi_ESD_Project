using _31_JunXi_Project.Data;
using _31_JunXi_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace _31_JunXi_Project.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = UserRoles.Admin)]

        // GET: api/Booking
        [HttpGet]
        public IActionResult x(string sortBy = "BookingDateFrom", string status = null)
        {
            var bookings = _context.bookings.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                bookings = bookings.Where(b => b.BookingStatus == status);
            }

            switch (sortBy.ToLower())
            {
                case "bookingid":
                    bookings = bookings.OrderBy(b => b.BookingId);
                    break;
                case "bookingdateto":
                    bookings = bookings.OrderBy(b => b.BookingDateTo);
                    break;
                case "bookedby":
                    bookings = bookings.OrderBy(b => b.BookedBy);
                    break;
                default:
                    bookings = bookings.OrderBy(b => b.BookingDateFrom);
                    break;
            }

            return Ok(bookings.ToList());
        }



        // GET: api/Booking/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var booking = _context.bookings.FirstOrDefault(b => b.BookingId == id);
            if (booking == null)
            {
                return Problem(detail: "Booking with id " + id + " is not found. ", statusCode: 404);
            }
            return Ok(booking);
        }

        // GET: api/Booking/search
        [HttpGet("search")]
        public IActionResult Search(string query, string fromDate = null, string toDate = null)
        {
            var bookings = _context.bookings.AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                bookings = bookings.Where(b => b.BookedBy.Contains(query) ||
                                               b.FacilityDescription.Contains(query));
            }

            if (!string.IsNullOrEmpty(fromDate))
            {
                bookings = bookings.Where(b => string.Compare(b.BookingDateFrom, fromDate) >= 0);
            }

            if (!string.IsNullOrEmpty(toDate))
            {
                bookings = bookings.Where(b => string.Compare(b.BookingDateTo, toDate) <= 0);
            }

            return Ok(bookings.ToList());
        }



        // POST: api/Booking
        [HttpPost]
        public IActionResult Create([FromBody] Booking booking)
        {
            if (booking == null)
            {
                return BadRequest();
            }

            _context.bookings.Add(booking);
                _context.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = booking.BookingId }, booking);
        }

        // PUT: api/Booking/{id}
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Booking updatedBooking)
        {
            if (updatedBooking == null)
            {
                return BadRequest("Booking data is required.");
            }

            if (updatedBooking.BookingId != id)
            {
                return BadRequest("ID in URL does not match ID in body.");
            }

            var existingBooking = _context.bookings.FirstOrDefault(b => b.BookingId == id);
            if (existingBooking == null)
            {
                return NotFound($"Booking with id {id} not found.");
            }

            // Check model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            existingBooking.FacilityDescription = updatedBooking.FacilityDescription;
            existingBooking.BookingDateFrom = updatedBooking.BookingDateFrom;
            existingBooking.BookingDateTo = updatedBooking.BookingDateTo;
            existingBooking.BookedBy = updatedBooking.BookedBy;
            existingBooking.BookingStatus = updatedBooking.BookingStatus;

            try
            {
                _context.bookings.Update(existingBooking);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                // Log the exception message for debugging purposes
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }

            return NoContent();
        }


        // DELETE: api/Booking/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var booking = _context.bookings.FirstOrDefault(b => b.BookingId == id);
            if (booking == null)
            {
                return Problem(detail: "Booking with id " + id + " is not found. ", statusCode: 404);
            }

            _context.bookings.Remove(booking);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
