using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Namaya.Model;

namespace Namaya.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. GET: api/students (Get all students for the table)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var students = await _context.Students.ToListAsync();
            return Ok(students);
        }

        // 2. POST: api/students (Add new student)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Student student)
        {
            try
            {
                // Check if student already exists
                var existingStudent = await _context.Students
                    .FirstOrDefaultAsync(s => s.StudentCustomId == student.StudentCustomId
                                           || s.Contact == student.Contact);

                if (existingStudent != null)
                {
                    return BadRequest(new { message = "Student already registered" });
                }

                // Create User Login
                var newUser = new User
                {
                    Email = student.StudentCustomId,
                    PasswordHash = student.Contact,
                    Role = "Student"
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // Link student with user
                student.UserId = newUser.Id;

                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Student registered successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 3. PUT: api/students/{id} (Update student belt/progress)
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Student student)
        {
            if (id != student.Id) return BadRequest();

            _context.Entry(student).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Students.Any(e => e.Id == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // 4. DELETE: api/students/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            // Also delete the linked User Login
            var user = await _context.Users.FindAsync(student.UserId);
            if (user != null) _context.Users.Remove(user);

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // GET: api/students/profile/STU1234
        [HttpGet("profile/{customId}")]
        public async Task<IActionResult> GetProfile(string customId)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentCustomId == customId);

            if (student == null) return NotFound();

            return Ok(student);
        }
    }
}