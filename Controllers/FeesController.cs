using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Namaya.Model;

namespace Namaya.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public FeesController(ApplicationDbContext context) { _context = context; }

        // 1. STUDENT: Upload Receipt Image (Fixed with DTO for Swagger)
        [HttpPost("upload-receipt")]
        public async Task<IActionResult> UploadReceipt([FromForm] FeeUploadDto dto)
        {
            try
            {
                if (dto.Image == null) return BadRequest("No image uploaded bro");

                // A. Save the file to wwwroot/receipts
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/receipts");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                var fileName = $"STU_{dto.StudentId}_{dto.Month}_{Guid.NewGuid().ToString().Substring(0, 5)}{Path.GetExtension(dto.Image.FileName)}";
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }

                // B. Create payment record
                var payment = new Payment
                {
                    StudentId = dto.StudentId,
                    Month = dto.Month,
                    Year = DateTime.Now.Year,
                    Amount = 1300,
                    Status = "Verification",
                    ReceiptPath = "/receipts/" + fileName,
                    UploadDate = DateTime.Now
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Receipt uploaded! Master will verify soon." });
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("pending-approvals")]
        public async Task<IActionResult> GetPending()
        {
            var list = await _context.Payments
                .Where(p => p.Status == "Verification")
                .Join(_context.Students, p => p.StudentId, s => s.Id, (p, s) => new {
                    p.Id,
                    s.Name,
                    s.StudentCustomId,
                    p.Month,
                    p.Amount,
                    p.ReceiptPath,
                    p.UploadDate
                }).ToListAsync();
            return Ok(list);
        }

        [HttpPost("approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            // 1. Find the payment record
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound("Payment not found");

            // 2. Mark the payment as Paid
            payment.Status = "Paid";
            payment.VerifiedDate = DateTime.Now;

            // 3. IMPORTANT: Find the student and update their main status
            var student = await _context.Students.FindAsync(payment.StudentId);
            if (student != null)
            {
                student.FeeStatus = "Paid"; // This is what shows in your "Records" table
                _context.Students.Update(student); // Tell EF to update the student
            }

            // 4. Save everything
            await _context.SaveChangesAsync();

            return Ok(new { message = "Verified and Student Table Updated!" });
        }

        [HttpGet("monthly-records")]
        public async Task<IActionResult> GetMonthlyFeeRecords(string? month, int? year, [FromQuery] string? branch) // Added 'branch' parameter
        {
            var targetMonth = month ?? DateTime.Now.ToString("MMMM", System.Globalization.CultureInfo.InvariantCulture);
            var targetYear = year ?? DateTime.Now.Year;

            IQueryable<Payment> query = _context.Payments;

            // Filter by Month and Year
            query = query.Where(p => p.Month.Equals(targetMonth, StringComparison.OrdinalIgnoreCase) && p.Year == targetYear);

            // NEW: Filter by Branch if provided.
            // We need to join with the Students table to access the Branch property.
            if (!string.IsNullOrWhiteSpace(branch))
            {
                query = query.Where(p => _context.Students.Any(s => s.Id == p.StudentId && s.Branch == branch));
                // Alternatively, use a direct navigation property if you have one set up in Payment model:
                // query = query.Where(p => p.Student.Branch == branch);
                // (This would require `public Student Student { get; set; }` in your Payment model and `Include` for eager loading)
            }

            // Now, perform the join for selecting data.
            // Note: The anonymous object structure is what the frontend will receive.
            var records = await query
                .Join(_context.Students, p => p.StudentId, s => s.Id, (p, s) => new
                {
                    p.Id,
                    p.StudentId,
                    StudentName = s.Name, // Explicitly get student name
                    StudentCustomId = s.StudentCustomId, // Explicitly get custom ID
                    s.Branch, // Include student's branch in the result
                    PaymentMonth = p.Month,
                    PaymentYear = p.Year,
                    p.Amount,
                    PaymentStatus = p.Status, // Status of this specific payment
                    p.PaymentMethod,
                    p.ReceiptPath,
                    p.UploadDate,
                    p.VerifiedDate
                })
                .OrderByDescending(r => r.UploadDate)
                .ToListAsync();

            return Ok(records);
        }

        [HttpPost("submit-fee")]
        public async Task<IActionResult> SubmitFee([FromForm] FeeUploadDto dto)
        {
            try
            {
                // 1. Find Student
                var student = await _context.Students.FindAsync(dto.StudentId);
                if (student == null) return BadRequest("Student ID not found in database.");

                // 2. Calculate Fees based on your rules
                decimal finalAmount = 0;
                string branch = student.Branch?.ToUpper() ?? "";

                if (branch == "AKILA HEIGHTS")
                {
                    // Both = 2000, One = 1000
                    if (student.IsSilambam && student.IsYoga) finalAmount = 2000;
                    else finalAmount = 1000;
                }
                else
                {
                    // Samraj Nagar or VGP Ponnagar
                    finalAmount = 1300;
                }

                // 3. Handle Receipt Upload (Only for UPI)
                string? dbPath = null;
                if (dto.PaymentMethod == "UPI")
                {
                    if (dto.Image == null) return BadRequest("UPI requires a receipt image.");

                    var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var receiptsFolder = Path.Combine(wwwroot, "receipts");

                    // Ensure folders exist
                    if (!Directory.Exists(wwwroot)) Directory.CreateDirectory(wwwroot);
                    if (!Directory.Exists(receiptsFolder)) Directory.CreateDirectory(receiptsFolder);

                    var fileName = $"STU_{dto.StudentId}_{Guid.NewGuid().ToString().Substring(0, 5)}{Path.GetExtension(dto.Image.FileName)}";
                    var fullPath = Path.Combine(receiptsFolder, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await dto.Image.CopyToAsync(stream);
                    }
                    dbPath = "/receipts/" + fileName;
                }

                // 4. Create the Payment Record
                var payment = new Payment
                {
                    StudentId = dto.StudentId,
                    Month = dto.Month ?? DateTime.Now.ToString("MMMM"),
                    Year = DateTime.Now.Year,
                    Amount = finalAmount,
                    PaymentMethod = dto.PaymentMethod,
                    Status = "Verification", // Master must verify
                    ReceiptPath = dbPath,
                    UploadDate = DateTime.Now
                };

                _context.Payments.Add(payment);

                // 5. Update the Student Table status
                student.FeeStatus = "Verification";

                await _context.SaveChangesAsync();

                return Ok(new { message = "Successfully submitted for verification!" });
            }
            catch (Exception ex)
            {
                // This will print the real error in your Visual Studio console
                Console.WriteLine("FEE ERROR: " + ex.Message);
                return StatusCode(500, $"Internal Error: {ex.Message}");
            }
        }

        // Updated DTO to include PaymentMethod
        public class FeeUploadDto
        {
            public int StudentId { get; set; }
            public string Month { get; set; }
            public string PaymentMethod { get; set; } // "Cash" or "UPI"
            public IFormFile? Image { get; set; } // Optional for Cash
        }
    }

    // THIS CLASS FIXES THE SWAGGER 500 ERROR
    public class FeeUploadDto
    {
        public int StudentId { get; set; }
        public string Month { get; set; }
        public IFormFile Image { get; set; }
    }
}