using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Namaya.Model;

namespace Namaya.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context) { _context = context; }

        //[HttpGet("stats")]
        //public async Task<IActionResult> GetDashboardStats()
        //{
        //    var today = DateTime.Now.Date;

        //    // 1. Total Students
        //    var totalStudents = await _context.Students.CountAsync();

        //    // 2. Active Events (Missions)
        //    var activeEvents = await _context.Events.CountAsync(e => e.Status == "Upcoming");

        //    // 3. Pending Fee Approvals (from your FeesController logic)
        //    var pendingFees = await _context.Payments.CountAsync(p => p.Status == "Verification");

        //    // 4. Today's Attendance Summary
        //    var presentToday = await _context.Attendances.CountAsync(a => a.Date.Date == today && a.Status == "Present");
        //    var totalMarkedToday = await _context.Attendances.CountAsync(a => a.Date.Date == today);

        //    return Ok(new
        //    {
        //        totalStudents,
        //        activeEvents,
        //        pendingFees,
        //        todayAttendance = totalMarkedToday > 0 ? $"{(presentToday * 100) / totalMarkedToday}%" : "0%"
        //    });
        //}

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var today = DateTime.Now.Date;
            var threeDaysAgo = today.AddDays(-3);
            var nextWeek = today.AddDays(7);

            var notifications = new List<object>();

            // 1. BIRTHDAYS TODAY
            var birthdays = await _context.Students
                .Where(s => s.DateOfBirth.Month == today.Month && s.DateOfBirth.Day == today.Day)
                .Select(s => new {
                    id = "bday_" + s.Id,
                    type = "birthday",
                    title = "Birthday Today! 🎂",
                    desc = $"It's {s.Name}'s birthday ({s.StudentCustomId})",
                    time = "Today"
                }).ToListAsync();
            notifications.AddRange(birthdays);

            // 2. NEW JOINERS (Last 3 days)
            var newJoiners = await _context.Students
                .Where(s => s.EnrollDate >= threeDaysAgo)
                .OrderByDescending(s => s.EnrollDate)
                .Select(s => new {
                    id = "reg_" + s.Id,
                    type = "reg",
                    title = "New Warrior Joined",
                    desc = $"{s.Name} enrolled in {s.Branch}",
                    time = s.EnrollDate.ToString("dd MMM")
                }).ToListAsync();
            notifications.AddRange(newJoiners);

            // 3. FEE OVERDUE (Not Paid)
            var overdue = await _context.Students
                .Where(s => s.FeeStatus != "Paid" && s.FeeStatus != "Verification")
                .Take(5) // Limit to 5 most urgent
                .Select(s => new {
                    id = "fee_" + s.Id,
                    type = "fee",
                    title = "Fee Pending",
                    desc = $"{s.Name} ({s.StudentCustomId}) hasn't paid this month",
                    time = "Urgent"
                }).ToListAsync();
            notifications.AddRange(overdue);

            // 4. UPCOMING EVENTS (Next 7 days)
            var upcomingEvents = await _context.Events
                .Where(e => e.EventDate >= today && e.EventDate <= nextWeek)
                .Select(e => new {
                    id = "event_" + e.Id,
                    type = "system",
                    title = "Event Reminder",
                    desc = $"{e.Title} is scheduled for {e.EventDate:dd MMM}",
                    time = "Upcoming"
                }).ToListAsync();
            notifications.AddRange(upcomingEvents);

            return Ok(notifications);
        }


        [HttpGet("dashboard-summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var today = DateTime.Now.Date;
            var currentMonth = DateTime.Now.ToString("MMMM");
            var currentYear = DateTime.Now.Year;

            // 1. Stats Cards Data
            var totalStudents = await _context.Students.CountAsync();
            var activeStudents = await _context.Students.CountAsync(s => s.FeeStatus == "Paid");
            var pendingFeesCount = await _context.Payments.CountAsync(p => p.Status == "Verification");
            var totalRevenue = await _context.Payments
                .Where(p => p.Status == "Paid" && p.Month == currentMonth && p.Year == currentYear)
                .SumAsync(p => p.Amount);

            // 2. NEW: Registrations by Branch (Dynamic Grouping)
            var branchData = await _context.Students
                .GroupBy(s => s.Branch)
                .Select(g => new {
                    name = g.Key ?? "Unknown", // Branch Name
                    value = g.Count()           // Total Students in this branch
                })
                .ToListAsync();

            // Assign colors to branches dynamically for the UI
            string[] colors = { "bg-blue-500", "bg-emerald-500", "bg-purple-500", "bg-orange-500", "bg-pink-500" };
            var branchStats = branchData.Select((b, index) => new {
                b.name,
                b.value,
                color = colors[index % colors.Length] // Cycle through colors
            }).ToList();

            // 3. Attendance Percentage
            var totalMarkedToday = await _context.Attendances.CountAsync(a => a.Date.Date == today);
            var presentToday = await _context.Attendances.CountAsync(a => a.Date.Date == today && a.Status == "Present");
            var attendancePercent = totalMarkedToday > 0 ? (presentToday * 100) / totalMarkedToday : 0;

            // 4. New Enrollments
            var recentRegistrations = await _context.Students
                .OrderByDescending(s => s.Id)
                .Take(5)
                .Select(s => new {
                    s.Id,
                    s.Name,
                    Art = s.IsSilambam && s.IsYoga  ? "Both" : s.IsSilambam ? "Silambam" : "Yoga",
                    Time = s.EnrollDate.ToString("dd MMM yyyy"),
                    Status = s.FeeStatus ?? "Pending"
                })
                .ToListAsync();

            return Ok(new
            {
                stats = new
                {
                    totalStudents,
                    activeStudents,
                    pendingFeesCount,
                    totalRevenue = $"₹{totalRevenue:N0}"
                },
                branchStats, // Changed from artStats to branchStats
                attendancePercent,
                recentRegistrations
            });
        }
    }
}