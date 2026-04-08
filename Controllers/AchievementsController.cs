using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Namaya.Model;

namespace Namaya.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AchievementsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public AchievementsController(ApplicationDbContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetAchievements()
        {
            var list = await _context.Achievements.OrderByDescending(a => a.Year).ToListAsync();
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAchievement([FromForm] AchievementDTO dto)
        {
            try
            {
                string imagePath = "";
                if (dto.ImageFile != null)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/achievements", fileName);

                    // Auto-create folder if missing
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.ImageFile.CopyToAsync(stream);
                    }
                    imagePath = "/achievements/" + fileName;
                }

                var achievement = new Achievement
                {
                    Title = dto.Title,
                    Level = dto.Level,
                    Year = dto.Year,
                    Highlight = dto.Highlight,
                    ImageUrl = imagePath
                };

                _context.Achievements.Add(achievement);
                await _context.SaveChangesAsync();
                return Ok(achievement);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAchievement(int id)
        {
            var achievement = await _context.Achievements.FindAsync(id);
            if (achievement == null) return NotFound();

            // Delete physical file
            if (!string.IsNullOrEmpty(achievement.ImageUrl))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", achievement.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            }

            _context.Achievements.Remove(achievement);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}