using Jukebox_Backend.Models.Dto.Requests;
using Jukebox_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Jukebox_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaylistController : ControllerBase
    {
        private readonly PlaylistService _playlistService;

        public PlaylistController(PlaylistService playlistService)
        {
            _playlistService = playlistService;
        }

        // search playlists with pagination (admin)
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] SearchPlaylistRequest request)
        {
            try
            {
                var result = await _playlistService.SearchAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // get my playlists
        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyPlaylists()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId is null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                var playlists = await _playlistService.GetByUserAsync(userId);
                return Ok(playlists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // get playlist by id with songs
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var playlist = await _playlistService.GetByIdAsync(id);

                if (playlist is null)
                {
                    return NotFound(new { message = "Playlist not found" });
                }

                return Ok(playlist);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // create empty playlist
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePlaylistRequest request)
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

                var playlist = await _playlistService.CreateAsync(request, userId);

                if (playlist is null)
                {
                    return BadRequest(new { message = "Failed to create playlist" });
                }

                return CreatedAtAction(nameof(GetById), new { id = playlist.PlaylistId }, playlist);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // generate playlist from quiz (import from APIs)
        [Authorize]
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GeneratePlaylistRequest request)
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

                var playlist = await _playlistService.GenerateAsync(request, userId);

                if (playlist is null)
                {
                    return BadRequest(new { message = "Failed to generate playlist, no songs found" });
                }

                return Ok(playlist);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // update playlist
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePlaylistRequest request)
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

                var playlist = await _playlistService.UpdateAsync(id, request, userId);

                if (playlist is null)
                {
                    return NotFound(new { message = "Playlist not found" });
                }

                return Ok(playlist);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // add song to playlist
        [Authorize]
        [HttpPost("{playlistId}/songs/{songId}")]
        public async Task<IActionResult> AddSong(int playlistId, int songId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId is null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                var result = await _playlistService.AddSongAsync(playlistId, songId, userId);

                if (!result)
                {
                    return BadRequest(new { message = "Failed to add song, playlist/song not found or already added" });
                }

                return Ok(new { message = "Song added to playlist" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // remove song from playlist
        [Authorize]
        [HttpDelete("{playlistId}/songs/{songId}")]
        public async Task<IActionResult> RemoveSong(int playlistId, int songId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId is null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                var result = await _playlistService.RemoveSongAsync(playlistId, songId, userId);

                if (!result)
                {
                    return NotFound(new { message = "Song not found in playlist" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // soft delete playlist
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId is null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                var result = await _playlistService.DeleteAsync(id, userId);

                if (!result)
                {
                    return NotFound(new { message = "Playlist not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}