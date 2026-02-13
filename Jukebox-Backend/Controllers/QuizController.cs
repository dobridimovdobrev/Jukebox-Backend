using Jukebox_Backend.Models.Dto.Requests;
using Jukebox_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jukebox_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly QuizService _quizService;

        public QuizController(QuizService quizService)
        {
            _quizService = quizService;
        }

        // search quizzes with pagination
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] SearchQuizRequest request)
        {
            try
            {
                var result = await _quizService.SearchAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // get quiz by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var quiz = await _quizService.GetByIdAsync(id);

                if (quiz is null)
                {
                    return NotFound(new { message = "Quiz not found" });
                }

                return Ok(quiz);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // get random quizzes by category and difficulty
        [Authorize]
        [HttpGet("random")]
        public async Task<IActionResult> GetRandom(
            [FromQuery] string category,
            [FromQuery] string difficulty,
            [FromQuery] int count = 10)
        {
            try
            {
                var quizzes = await _quizService.GetRandomAsync(category, difficulty, count);
                return Ok(quizzes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // get distinct quiz categories
        [Authorize]
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _quizService.GetCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // create quiz
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateQuizRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var quiz = await _quizService.CreateAsync(request);

                if (quiz is null)
                {
                    return BadRequest(new { message = "Failed to create quiz" });
                }

                return CreatedAtAction(nameof(GetById), new { id = quiz.QuizId }, quiz);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // update quiz
        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateQuizRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var quiz = await _quizService.UpdateAsync(id, request);

                if (quiz is null)
                {
                    return NotFound(new { message = "Quiz not found" });
                }

                return Ok(quiz);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // soft delete quiz
        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _quizService.DeleteAsync(id);

                if (!result)
                {
                    return NotFound(new { message = "Quiz not found" });
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