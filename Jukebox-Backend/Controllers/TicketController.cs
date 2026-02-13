using Jukebox_Backend.Models.Dto.Requests;
using Jukebox_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Jukebox_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly TicketService _ticketService;

        public TicketController(TicketService ticketService)
        {
            _ticketService = ticketService;
        }

        // search tickets with pagination 
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] SearchTicketRequest request)
        {
            try
            {
                var result = await _ticketService.SearchAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // get my tickets
        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyTickets()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId is null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                var tickets = await _ticketService.GetByUserAsync(userId);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // get ticket by id
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var ticket = await _ticketService.GetByIdAsync(id);

                if (ticket is null)
                {
                    return NotFound(new { message = "Ticket not found" });
                }

                return Ok(ticket);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // create ticket
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTicketRequest request)
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

                var ticket = await _ticketService.CreateAsync(request, userId);

                if (ticket is null)
                {
                    return BadRequest(new { message = "Failed to create ticket" });
                }

                return CreatedAtAction(nameof(GetById), new { id = ticket.TicketId }, ticket);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // response + status change
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("{id}/action")]
        public async Task<IActionResult> AddAction(int id, [FromBody] TicketActionRequest request)
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

                var result = await _ticketService.AddActionAsync(id, request, userId);

                if (result is null)
                {
                    return BadRequest(new { message = "Action failed. Check status transition rules." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // valid transitions for current status
        [Authorize]
        [HttpGet("{id}/transitions")]
        public async Task<IActionResult> GetTransitions(int id)
        {
            try
            {
                var ticket = await _ticketService.GetByIdAsync(id);
                if (ticket is null)
                    return NotFound(new { message = "Ticket not found" });

                var transitions = TicketService.GetValidTransitions(ticket.Status);
                return Ok(new { currentStatus = ticket.Status, allowedTransitions = transitions });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // update ticket fields 
        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTicketRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var ticket = await _ticketService.UpdateAsync(id, request);

                if (ticket is null)
                {
                    return NotFound(new { message = "Ticket not found" });
                }

                return Ok(ticket);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // soft delete ticket
        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _ticketService.DeleteAsync(id);

                if (!result)
                {
                    return NotFound(new { message = "Ticket not found" });
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