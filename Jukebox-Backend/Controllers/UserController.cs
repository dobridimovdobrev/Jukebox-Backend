using Jukebox_Backend.Models.Dto.Requests;
using Jukebox_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Jukebox_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        // search users with pagination (admin)
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] SearchUserRequest request)
        {
            try
            {
                var result = await _userService.SearchAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // get my profile
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId is null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                var user = await _userService.GetByIdAsync(userId);

                if (user is null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // get user by id (admin)
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);

                if (user is null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // update my profile
        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserRequest request)
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

                var user = await _userService.UpdateAsync(userId, request);

                if (user is null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // deactivate user (admin)
        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deactivate(string id)
        {
            try
            {
                var result = await _userService.DeactivateAsync(id);

                if (!result)
                {
                    return NotFound(new { message = "User not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // toggle user active (admin)
        [Authorize(Roles = "SuperAdmin")]
        [HttpPatch("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(string id)
        {
            try
            {
                var result = await _userService.ToggleActiveAsync(id);
                if (!result)
                    return NotFound(new { message = "User not found" });

                return Ok(new { message = "User status toggled" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // spend coins to play song
        [Authorize]
        [HttpPost("spend-coins")]
        public async Task<IActionResult> SpendCoins([FromQuery] int amount = 1)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId is null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                var result = await _userService.SpendCoinsAsync(userId, amount);

                if (!result)
                {
                    return BadRequest(new { message = "Not enough coins or user not found" });
                }

                return Ok(new { message = "Coins spent successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}