using Jukebox_Backend.Models.Dto.Requests;
using Jukebox_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jukebox_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongController : ControllerBase
    {
        private readonly SongService _songService;

        public SongController(SongService songService)
        {
            _songService = songService;
        }

        // search songs with pagination
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] SearchSongRequest request)
        {
            try
            {
                var result = await _songService.SearchAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // get song by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var song = await _songService.GetByIdAsync(id);

                if (song is null)
                {
                    return NotFound(new { message = "Song not found" });
                }

                return Ok(song);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // create song
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSongRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var song = await _songService.CreateAsync(request);

                if (song is null)
                {
                    return BadRequest(new { message = "Artist not found or creation failed" });
                }

                return CreatedAtAction(nameof(GetById), new { id = song.SongId }, song);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // update song
        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSongRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var song = await _songService.UpdateAsync(id, request);

                if (song is null)
                {
                    return NotFound(new { message = "Song or artist not found" });
                }

                return Ok(song);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // increment play count (when user plays a song)
        [Authorize]
        [HttpPost("{id}/play")]
        public async Task<IActionResult> Play(int id)
        {
            try
            {
                var result = await _songService.IncrementPlayCountAsync(id);

                if (!result)
                {
                    return NotFound(new { message = "Song not found" });
                }

                return Ok(new { message = "Play count updated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // soft delete song
        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _songService.DeleteAsync(id);

                if (!result)
                {
                    return NotFound(new { message = "Song not found" });
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