using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Namaya.Model;

namespace Namaya.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public AttendanceController(ApplicationDbContext context) { _context = context; }

        [HttpGet("students-to-mark")]
        public async Task<IActionResult> GetStudentsToMark(string branch, string batch)
        {
            var list = await _context.Students
                .Where(s => s.Branch == branch && s.Batch == batch)
                .Select(s => new { s.Id, s.Name, s.StudentCustomId })
                .ToListAsync();
            return Ok(list);
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitAttendance([FromBody] AttendanceSubmissionDTO data)
        {
            if (data == null || data.Records == null)
                return BadRequest(new { message = "Data is empty bro!" });

            var alertStudents = new List<string>();

            try
            {
                foreach (var record in data.Records)
                {
                    // Update or Add today's record
                    var existing = await _context.Attendances
                        .FirstOrDefaultAsync(a => a.StudentId == record.StudentId && a.Date.Date == data.Date.Date);

                    if (existing != null)
                    {
                        existing.Status = record.Status;
                    }
                    else
                    {
                        _context.Attendances.Add(new Attendance
                        {
                            StudentId = record.StudentId,
                            Date = data.Date,
                            Status = record.Status,
                            Branch = data.Branch,
                            Batch = data.Batch
                        });
                    }

                    // 3-Day Absence Alert Logic
                    if (record.Status == "Absent")
                    {
                        var history = await _context.Attendances
                            .Where(a => a.StudentId == record.StudentId && a.Date.Date < data.Date.Date)
                            .OrderByDescending(a => a.Date)
                            .Take(6)
                            .ToListAsync();

                        if (history.Count == 6 && history.All(h => h.Status == "Absent"))
                        {
                            var name = _context.Students.Find(record.StudentId)?.Name;
                            alertStudents.Add(name);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Success", alerts = alertStudents });
            }
            catch (Exception ex)
            {
                // This helps us see the real SQL error in the React console
                return StatusCode(500, new { message = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        // GET: api/attendance/history/5
        [HttpGet("history/{studentId}")]
        public async Task<IActionResult> GetStudentHistory(int studentId)
        {
            var records = await _context.Attendances
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.Date)
                .Take(30) // Show last 30 training days
                .ToListAsync();

            // Calculate percentage
            double total = records.Count;
            double present = records.Count(r => r.Status == "Present");
            double percentage = total > 0 ? (present / total) * 100 : 0;

            return Ok(new
            {
                history = records,
                percentage = Math.Round(percentage, 1),
                consecutiveAbsences = CalculateConsecutiveAbsences(records)
            });
        }

        // Helper logic for the 3-day warning
        private int CalculateConsecutiveAbsences(List<Attendance> records)
        {
            int count = 0;
            foreach (var r in records.OrderByDescending(x => x.Date))
            {
                if (r.Status == "Absent") count++;
                else break;
            }
            return count;
        }

        // GET: api/attendance/report?month=3&year=2024
        [HttpGet("report")]
        public async Task<IActionResult> GetAttendanceReport(int month, int year)
        {
            var report = await _context.Attendances
                .Where(a => a.Date.Month == month && a.Date.Year == year)
                .Join(_context.Students,
                      att => att.StudentId,
                      stu => stu.Id,
                      (att, stu) => new {
                          stu.StudentCustomId,
                          stu.Name,
                          stu.Branch,
                          stu.Batch,
                          att.Status,
                          att.Date // We need the date now since it's a monthly view
                      })
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            // Summary calculation for the whole month
            var summary = new
            {
                TotalPresent = report.Count(r => r.Status == "Present"),
                TotalAbsent = report.Count(r => r.Status == "Absent"),
                TotalMarked = report.Count
            };

            return Ok(new { data = report, summary = summary });
        }
    }

    // MAKE SURE THESE CLASSES ARE HERE
    public class AttendanceSubmissionDTO
    {
        public DateTime Date { get; set; }
        public string Branch { get; set; }
        public string Batch { get; set; }
        public List<StudentStatusDTO> Records { get; set; }
    }

    public class StudentStatusDTO
    {
        public int StudentId { get; set; }
        public string Status { get; set; }
    }
}