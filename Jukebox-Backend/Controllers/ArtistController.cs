using Jukebox_Backend.Models.Dto.Requests;
using Jukebox_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jukebox_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtistController : ControllerBase
    {
        private readonly ArtistService _artistService;
        private readonly PlaylistGenerationService _playlistGenService;

        public ArtistController(ArtistService artistService, PlaylistGenerationService playlistGenService)
        {
            _artistService = artistService;
            _playlistGenService = playlistGenService;
        }

        // search artists with pagination
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] SearchArtistRequest request)
        {
            try
            {
                var result = await _artistService.SearchAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // search artists on MusicBrainz — NO auto-import
        [Authorize]
        [HttpGet("search-musicbrainz")]
        public async Task<IActionResult> SearchMusicBrainz(
            [FromQuery] string query,
            [FromQuery] int limit = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
                    return BadRequest(new { message = "Query must be at least 2 characters" });

                var results = await _playlistGenService.SearchMusicBrainzAsync(query.Trim(), limit);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // import ONE artist when user selects from search
        [Authorize]
        [HttpPost("import-musicbrainz")]
        public async Task<IActionResult> ImportByMusicBrainzId([FromBody] ImportMusicBrainzRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.MusicBrainzId))
                    return BadRequest(new { message = "MusicBrainzId is required" });

                var artist = await _playlistGenService.ImportByMusicBrainzIdAsync(request.MusicBrainzId.Trim());

                if (artist is null)
                    return NotFound(new { message = "Artist not found on MusicBrainz" });

                return Ok(new
                {
                    artistId = artist.ArtistId,
                    name = artist.Name,
                    photo = artist.Photo,
                    genre = artist.Genre,
                    countryCode = artist.CountryCode,
                    musicBrainzId = artist.MusicBrainzId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // get artist by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var artist = await _artistService.GetByIdAsync(id);

                if (artist is null)
                {
                    return NotFound(new { message = "Artist not found" });
                }

                return Ok(artist);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // create artist manually
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateArtistRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var artist = await _artistService.CreateAsync(request);

                if (artist is null)
                {
                    return BadRequest(new { message = "Artist already exists or creation failed" });
                }

                return CreatedAtAction(nameof(GetById), new { id = artist.ArtistId }, artist);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // import artist from external APIs (MusicBrainz + TheAudioDB)
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("import/{artistName}")]
        public async Task<IActionResult> Import(string artistName)
        {
            try
            {
                var artist = await _playlistGenService.ImportArtistAsync(artistName);

                if (artist is null)
                {
                    return NotFound(new { message = "Artist not found on MusicBrainz" });
                }

                return Ok(new
                {
                    artist.ArtistId,
                    artist.Name,
                    artist.Photo,
                    artist.CountryCode,
                    artist.Genre,
                    artist.Biography,
                    artist.MusicBrainzId,
                    artist.IsActive,
                    CareerStart = artist.CareerStart?.Year,
                    CareerEnd = artist.CareerEnd?.Year
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // update artist
        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateArtistRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var artist = await _artistService.UpdateAsync(id, request);

                if (artist is null)
                {
                    return NotFound(new { message = "Artist not found" });
                }

                return Ok(artist);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // soft delete artist
        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _artistService.DeleteAsync(id);

                if (!result)
                {
                    return NotFound(new { message = "Artist not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // test youtube api 
        /* [Authorize(Roles = "SuperAdmin")]
         [HttpGet("test-youtube")]
         public async Task<IActionResult> TestYoutube([FromQuery] string artist, [FromQuery] string song)
         {
             try
             {
                 var result = await _playlistGenService.TestYoutubeAsync(artist, song);
                 return Ok(result);
             }
             catch (Exception ex)
             {
                 return StatusCode(500, new { message = ex.Message });
             }
         }*/
    }
}