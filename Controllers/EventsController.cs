using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Namaya.Model;

namespace Namaya.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public EventsController(ApplicationDbContext context) { _context = context; }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllEvents()
        {
            return Ok(await _context.Events.ToListAsync());
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcoming()
        {
            var events = await _context.Events.Where(e => e.Status == "Upcoming").ToListAsync();
            return Ok(events);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromForm] EventUploadDTO dto)
        {
            try
            {
                string imagePath = "";
                if (dto.Image != null)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Image.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/events", fileName);

                    // Ensure directory exists
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.Image.CopyToAsync(stream);
                    }
                    imagePath = "/events/" + fileName;
                }

                var newEvent = new Event
                {
                    Title = dto.Title,
                    EventDate = dto.Date,
                    Location = dto.Loc,
                    EventType = dto.Type,
                    Status = "Upcoming",
                    ImagePath = imagePath
                };

                _context.Events.Add(newEvent);
                await _context.SaveChangesAsync();
                return Ok(newEvent);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterForEvent([FromBody] EventRegistration reg)
        {
            var exists = await _context.EventRegistrations
                .AnyAsync(r => r.EventId == reg.EventId && r.StudentId == reg.StudentId);

            if (exists) return BadRequest(new { message = "Already registered bro!" });

            _context.EventRegistrations.Add(reg);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Successfully Enlisted!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();
            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // GET: api/events/participants/5
        [HttpGet("participants/{eventId}")]
        public async Task<IActionResult> GetParticipants(int eventId)
        {
            var list = await _context.EventRegistrations
                .Where(r => r.EventId == eventId)
                .Join(_context.Students,
                      reg => reg.StudentId,
                      stu => stu.Id,
                      (reg, stu) => new {
                          stu.StudentCustomId,
                          stu.Name,
                          stu.Contact,
                          stu.Branch,
                          reg.RegistrationDate
                      })
                .ToListAsync();

            return Ok(list);
        }
    }



    // THIS DTO FIXES THE SWAGGER 500 ERROR
    public class EventUploadDTO
    {
        public IFormFile? Image { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Loc { get; set; }
        public string Type { get; set; }
    }
}