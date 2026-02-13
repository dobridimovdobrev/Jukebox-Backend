using Jukebox_Backend.Models.Dto.Requests;
using Jukebox_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Jukebox_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserQuizHistoryController : ControllerBase
    {
        private readonly UserQuizHistoryService _historyService;

        public UserQuizHistoryController(UserQuizHistoryService historyService)
        {
            _historyService = historyService;
        }

        // get all history (admin)
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var history = await _historyService.GetAllAsync();
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // get my quiz history
        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyHistory()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId is null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                var history = await _historyService.GetByUserAsync(userId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // submit quiz result
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserQuizHistoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId is null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                var result = await _historyService.CreateAsync(request, userId);

                if (result is null)
                {
                    return BadRequest(new { message = "Failed to save quiz result" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}