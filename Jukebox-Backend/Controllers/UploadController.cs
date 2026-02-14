using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;



namespace Jukebox_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file provided" });

            // Max 5MB
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "File too large (max 5MB)" });

            // Only images
            var allowedTypes = new[] { "image/jpeg", "image/png" };
            if (!allowedTypes.Contains(file.ContentType))
                return BadRequest(new { message = "Only JPEG and PNG files are allowed" });

            // Create uploads directory
            var uploadsDir = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", "tickets");
            Directory.CreateDirectory(uploadsDir);

            // Unique filename
            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(ext)) ext = ".jpg";
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            // Save to disk
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return url
            var url = $"/uploads/tickets/{fileName}";
            return Ok(new { url });
        }

    }
}
