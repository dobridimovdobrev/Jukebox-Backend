using Jukebox_Backend.Data;
using Jukebox_Backend.Models.Dto.Requests;
using Jukebox_Backend.Models.Dto.Responses;
using Jukebox_Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Jukebox_Backend.Services
{
    public class TicketService : ServiceBase
    {
        // rules
        private static readonly Dictionary<string, List<string>> ValidTransitions = new()
        {
            { "open", new List<string> { "in_progress", "closed" } },
            { "in_progress", new List<string> { "answered", "closed" } },
            { "answered", new List<string> { "in_progress", "closed" } },
            { "closed", new List<string>() }
        };

        public TicketService(ApplicationDbContext context) : base(context) { }

        // helpers
        private TicketResponse MapToResponse(Ticket t, List<TicketHistory>? history = null)
        {
            return new TicketResponse
            {
                TicketId = t.TicketId,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                Category = t.Category,
                CreatedAt = t.CreatedAt,
                ResolvedAt = t.ResolvedAt,
                AttachmentUrl = t.AttachmentUrl,
                UserId = t.UserId,
                UserFullName = t.User != null ? t.User.FirstName + " " + t.User.LastName : null,
                UserEmail = t.User != null ? t.User.Email : null,
                History = (history ?? new List<TicketHistory>())
                    .OrderBy(h => h.CreatedAt)
                    .Select(h => new TicketHistoryResponse
                    {
                        TicketHistoryId = h.TicketHistoryId,
                        Type = h.Type,
                        FromStatus = h.FromStatus,
                        ToStatus = h.ToStatus,
                        Message = h.Message,
                        Note = h.Note,
                        AttachmentUrl = h.AttachmentUrl,
                        By = h.User != null ? h.User.FirstName + " " + h.User.LastName : "System",
                        Date = h.CreatedAt
                    })
                    .ToList()
            };
        }

        // search tickets with pagination (list view — no attachment, no history)
        public async Task<PaginatedResponse<TicketResponse>> SearchAsync(SearchTicketRequest request)
        {
            try
            {
                var query = _context.Tickets
                    .AsNoTracking()
                    .Where(t => !t.IsDeleted)
                    .Include(t => t.User)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(request.Title))
                    query = query.Where(t => t.Title.Contains(request.Title));

                if (!string.IsNullOrEmpty(request.Status))
                    query = query.Where(t => t.Status == request.Status);

                if (!string.IsNullOrEmpty(request.Priority))
                    query = query.Where(t => t.Priority == request.Priority);

                if (!string.IsNullOrEmpty(request.Category))
                    query = query.Where(t => t.Category != null && t.Category == request.Category);

                var totalItems = await query.CountAsync();

                var items = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(t => new TicketResponse
                    {
                        TicketId = t.TicketId,
                        Title = t.Title,
                        Description = t.Description,
                        Status = t.Status,
                        Priority = t.Priority,
                        Category = t.Category,
                        CreatedAt = t.CreatedAt,
                        ResolvedAt = t.ResolvedAt,
                        // no AttachmentUrl — too heavy for list (base64 images)
                        UserId = t.UserId,
                        UserFullName = t.User != null ? t.User.FirstName + " " + t.User.LastName : null,
                        UserEmail = t.User != null ? t.User.Email : null
                        // no History — loaded only in GetByIdAsync
                    })
                    .ToListAsync();

                return new PaginatedResponse<TicketResponse>
                {
                    Items = items,
                    TotalItems = totalItems,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
            catch (Exception)
            {
                return new PaginatedResponse<TicketResponse>();
            }
        }

        // get tickets by user (list view — no attachment, no history)
        public async Task<List<TicketResponse>> GetByUserAsync(string userId)
        {
            try
            {
                return await _context.Tickets
                    .AsNoTracking()
                    .Where(t => t.UserId == userId && !t.IsDeleted)
                    .OrderByDescending(t => t.CreatedAt)
                    .Select(t => new TicketResponse
                    {
                        TicketId = t.TicketId,
                        Title = t.Title,
                        Description = t.Description,
                        Status = t.Status,
                        Priority = t.Priority,
                        Category = t.Category,
                        CreatedAt = t.CreatedAt,
                        ResolvedAt = t.ResolvedAt,
                        // no AttachmentUrl — too heavy for list
                        UserId = t.UserId
                        // no History — loaded only in GetByIdAsync
                    })
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<TicketResponse>();
            }
        }

        // get ticket by id (detail view — full data with attachment + history)
        public async Task<TicketResponse?> GetByIdAsync(int id)
        {
            try
            {
                var ticket = await _context.Tickets
                    .AsNoTracking()
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.TicketId == id && !t.IsDeleted);

                if (ticket is null) return null;

                var history = await _context.TicketHistories
                    .AsNoTracking()
                    .Where(h => h.TicketId == id)
                    .Include(h => h.User)
                    .ToListAsync();

                return MapToResponse(ticket, history);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // create ticket + initial history 
        public async Task<TicketResponse?> CreateAsync(CreateTicketRequest request, string userId)
        {
            try
            {
                var ticket = new Ticket
                {
                    Title = request.Title,
                    Description = request.Description,
                    Priority = request.Priority,
                    Category = request.Category,
                    AttachmentUrl = request.AttachmentUrl,
                    Status = "open",
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Tickets.Add(ticket);
                await SaveAsync();


                var historyEntry = new TicketHistory
                {
                    TicketId = ticket.TicketId,
                    Type = "created",
                    ToStatus = "open",
                    Note = "Ticket created",
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.TicketHistories.Add(historyEntry);
                await SaveAsync();

                Log.Information("TICKET CREATED: #{Id} by {User}", ticket.TicketId, userId);


                return await GetByIdAsync(ticket.TicketId);
            }
            catch (Exception ex)
            {
                Log.Error("TICKET CREATE ERROR: {Msg}", ex.Message);
                return null;
            }
        }

        // response + status change 
        public async Task<TicketResponse?> AddActionAsync(int ticketId, TicketActionRequest request, string adminUserId)
        {
            try
            {
                var ticket = await _context.Tickets
                    .FirstOrDefaultAsync(t => t.TicketId == ticketId && !t.IsDeleted);

                if (ticket is null) return null;


                if (!string.IsNullOrWhiteSpace(request.Message) || !string.IsNullOrEmpty(request.AttachmentUrl))
                {
                    _context.TicketHistories.Add(new TicketHistory
                    {
                        TicketId = ticketId,
                        Type = "response",
                        Message = request.Message?.Trim(),
                        AttachmentUrl = request.AttachmentUrl,
                        UserId = adminUserId,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // Validate and apply status transition
                if (!string.IsNullOrEmpty(request.NewStatus) && request.NewStatus != ticket.Status)
                {
                    if (!ValidTransitions.ContainsKey(ticket.Status))
                    {
                        Log.Warning("TICKET TRANSITION DENIED: Unknown current status {Status}", ticket.Status);
                        return null;
                    }

                    var allowed = ValidTransitions[ticket.Status];
                    if (!allowed.Contains(request.NewStatus))
                    {
                        Log.Warning("TICKET TRANSITION DENIED: {From} -> {To} not allowed for #{Id}",
                            ticket.Status, request.NewStatus, ticketId);
                        return null;
                    }

                    var fromStatus = ticket.Status;

                    _context.TicketHistories.Add(new TicketHistory
                    {
                        TicketId = ticketId,
                        Type = "transition",
                        FromStatus = fromStatus,
                        ToStatus = request.NewStatus,
                        Note = request.Note ?? "",
                        UserId = adminUserId,
                        CreatedAt = DateTime.UtcNow
                    });

                    ticket.Status = request.NewStatus;

                    if (request.NewStatus == "closed" || request.NewStatus == "answered")
                        ticket.ResolvedAt = DateTime.UtcNow;

                    Log.Information("TICKET #{Id} TRANSITION: {From} -> {To} by {User}",
                        ticketId, fromStatus, request.NewStatus, adminUserId);
                }

                await SaveAsync();
                return await GetByIdAsync(ticketId);
            }
            catch (Exception ex)
            {
                Log.Error("TICKET ACTION ERROR: {Msg}", ex.Message);
                return null;
            }
        }

        // user reply to ticket
        public async Task<TicketResponse?> UserReplyAsync(int ticketId, UserReplyRequest request, string userId)
        {
            try
            {
                var ticket = await _context.Tickets
                    .FirstOrDefaultAsync(t => t.TicketId == ticketId && !t.IsDeleted && t.UserId == userId);

                if (ticket is null) return null;

                if (ticket.Status == "closed") return null;

                // Add user reply to history
                if (!string.IsNullOrWhiteSpace(request.Message) || !string.IsNullOrEmpty(request.AttachmentUrl))
                {
                    _context.TicketHistories.Add(new TicketHistory
                    {
                        TicketId = ticketId,
                        Type = "user_reply",
                        Message = request.Message?.Trim(),
                        AttachmentUrl = request.AttachmentUrl,
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // If status was "answered", reopen to "in_progress" so admin knows user replied
                if (ticket.Status == "answered")
                {
                    _context.TicketHistories.Add(new TicketHistory
                    {
                        TicketId = ticketId,
                        Type = "transition",
                        FromStatus = "answered",
                        ToStatus = "in_progress",
                        Note = "Reopened by user reply",
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow
                    });

                    ticket.Status = "in_progress";
                    ticket.ResolvedAt = null;
                }

                await SaveAsync();

                Log.Information("TICKET #{Id} USER REPLY by {User}", ticketId, userId);

                return await GetByIdAsync(ticketId);
            }
            catch (Exception ex)
            {
                Log.Error("TICKET USER REPLY ERROR: {Msg}", ex.Message);
                return null;
            }
        }

        // update ticket 
        public async Task<TicketResponse?> UpdateAsync(int id, UpdateTicketRequest request)
        {
            try
            {
                var ticket = await _context.Tickets
                    .FirstOrDefaultAsync(t => t.TicketId == id && !t.IsDeleted);

                if (ticket is null) return null;

                if (request.Title is not null) ticket.Title = request.Title;
                if (request.Description is not null) ticket.Description = request.Description;
                if (request.Priority is not null) ticket.Priority = request.Priority;
                if (request.Category is not null) ticket.Category = request.Category;
                if (request.AttachmentUrl is not null) ticket.AttachmentUrl = request.AttachmentUrl;

                await SaveAsync();
                return await GetByIdAsync(id);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // soft delete ticket
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var ticket = await _context.Tickets
                    .FirstOrDefaultAsync(t => t.TicketId == id && !t.IsDeleted);

                if (ticket is null) return false;

                ticket.IsDeleted = true;
                ticket.DeletedAt = DateTime.UtcNow;

                return await SaveAsync();
            }
            catch (Exception)
            {
                return false;
            }
        }

        // valid transitions for a status
        public static List<string> GetValidTransitions(string currentStatus)
        {
            return ValidTransitions.GetValueOrDefault(currentStatus, new List<string>());
        }
    }
}