using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TicketSystem.API.Data;
using TicketSystem.API.DTOs;
using TicketSystem.API.Models;

namespace TicketSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TicketsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/tickets
        [HttpGet]
        public async Task<IActionResult> GetTickets([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // Kullanıcı bilgilerini token'dan al
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                var companyId = GetCurrentUserCompanyId();

                IQueryable<Ticket> query = _context.Tickets
                    .Include(t => t.Company)
                    .Include(t => t.CreatedByUser)
                    .Include(t => t.AssignedToUser)
                    .Include(t => t.TicketType)
                    .Include(t => t.Category)
                    .Include(t => t.Module);

                // Role bazlı filtreleme
                if (userRole == "Customer")
                {
                    // Müşteri sadece kendi şirketinin ticketlarını görebilir
                    query = query.Where(t => t.CompanyId == companyId);
                }
                // Admin tüm ticketları görebilir

                // Sayfalama
                var totalCount = await query.CountAsync();
                var tickets = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new TicketListDto
                    {
                        Id = t.Id,
                        TicketNumber = t.TicketNumber,
                        Title = t.Title,
                        Description = t.Description.Length > 100 ? t.Description.Substring(0, 100) + "..." : t.Description,
                        Status = t.Status,
                        Priority = t.Priority,
                        CreatedAt = t.CreatedAt,
                        CompanyName = t.Company.Name,
                        CreatedByName = t.CreatedByUser.FirstName + " " + t.CreatedByUser.LastName,
                        AssignedToName = t.AssignedToUser != null ? t.AssignedToUser.FirstName + " " + t.AssignedToUser.LastName : null,
                        TicketTypeName = t.TicketType != null ? t.TicketType.Name : null,
                        CategoryName = t.Category != null ? t.Category.Name : null,
                        ModuleName = t.Module != null ? t.Module.Name : null
                    })
                    .ToListAsync();

                return Ok(new
                {
                    tickets = tickets,
                    totalCount = totalCount,
                    page = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/tickets/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicket(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                var companyId = GetCurrentUserCompanyId();

                var query = _context.Tickets
                    .Include(t => t.Company)
                    .Include(t => t.CreatedByUser)
                    .Include(t => t.AssignedToUser)
                    .Include(t => t.TicketType)
                    .Include(t => t.Category)
                    .Include(t => t.Module)
                    .Include(t => t.Comments.OrderBy(c => c.CreatedAt))
                        .ThenInclude(c => c.User)
                    .Include(t => t.Attachments)
                    .Where(t => t.Id == id);

                // Role bazlı yetki kontrolü
                if (userRole == "Customer")
                {
                    query = query.Where(t => t.CompanyId == companyId);
                }

                var ticket = await query.FirstOrDefaultAsync();

                if (ticket == null)
                {
                    return NotFound(new { message = "Ticket bulunamadı." });
                }

                var result = new TicketDetailDto
                {
                    Id = ticket.Id,
                    TicketNumber = ticket.TicketNumber,
                    Title = ticket.Title,
                    Description = ticket.Description,
                    Status = ticket.Status,
                    Priority = ticket.Priority,
                    CreatedAt = ticket.CreatedAt,
                    UpdatedAt = ticket.UpdatedAt,
                    CompanyName = ticket.Company.Name,
                    CreatedByName = ticket.CreatedByUser.FirstName + " " + ticket.CreatedByUser.LastName,
                    AssignedToName = ticket.AssignedToUser?.FirstName + " " + ticket.AssignedToUser?.LastName,
                    TicketTypeName = ticket.TicketType?.Name,
                    CategoryName = ticket.Category?.Name,
                    ModuleName = ticket.Module?.Name,
                    Comments = ticket.Comments.Select(c => new TicketCommentDto
                    {
                        Id = c.Id,
                        Comment = c.Comment,
                        IsInternal = c.IsInternal,
                        CreatedAt = c.CreatedAt,
                        UserName = c.User.FirstName + " " + c.User.LastName
                    }).ToList(),
                    Attachments = ticket.Attachments.Select(a => new TicketAttachmentDto
                    {
                        Id = a.Id,
                        FileName = a.FileName,
                        FilePath = a.FilePath,
                        FileSize = a.FileSize,
                        CreatedAt = a.CreatedAt
                    }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/tickets
        [HttpPost]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var companyId = GetCurrentUserCompanyId();

                if (companyId == null)
                {
                    return BadRequest(new { message = "Şirket bilgisi bulunamadı." });
                }

                // Ticket numarası oluştur
                var ticketNumber = await GenerateTicketNumberAsync();

                var ticket = new Ticket
                {
                    Id = Guid.NewGuid(),
                    TicketNumber = ticketNumber,
                    Title = dto.Title,
                    Description = dto.Description,
                    Priority = dto.Priority ?? "Medium",
                    Status = "Open",
                    CompanyId = companyId.Value,
                    CreatedByUserId = userId,
                    TicketTypeId = dto.TicketTypeId,
                    CategoryId = dto.CategoryId,
                    ModuleId = dto.ModuleId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();

                // Başarı response'u
                return Ok(new
                {
                    message = "Ticket başarıyla oluşturuldu.",
                    ticketId = ticket.Id,
                    ticketNumber = ticket.TicketNumber
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // PUT: api/tickets/{id}/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Support")] // Sadece Admin ve Support durumu değiştirebilir
        public async Task<IActionResult> UpdateTicketStatus(Guid id, [FromBody] UpdateTicketStatusDto dto)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(id);
                if (ticket == null)
                {
                    return NotFound(new { message = "Ticket bulunamadı." });
                }

                ticket.Status = dto.Status;
                ticket.UpdatedAt = DateTime.UtcNow;

                if (dto.AssignedToUserId.HasValue)
                {
                    ticket.AssignedToUserId = dto.AssignedToUserId.Value;
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Ticket durumu güncellendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/tickets/{id}/comments
        [HttpPost("{id}/comments")]
        public async Task<IActionResult> AddComment(Guid id, [FromBody] AddCommentDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                var companyId = GetCurrentUserCompanyId();

                // Ticket'ın varlığını ve yetkilendirmeyi kontrol et
                var ticketQuery = _context.Tickets.Where(t => t.Id == id);

                if (userRole == "Customer")
                {
                    ticketQuery = ticketQuery.Where(t => t.CompanyId == companyId);
                }

                var ticket = await ticketQuery.FirstOrDefaultAsync();
                if (ticket == null)
                {
                    return NotFound(new { message = "Ticket bulunamadı." });
                }

                var comment = new TicketComment
                {
                    Id = Guid.NewGuid(),
                    TicketId = id,
                    UserId = userId,
                    Comment = dto.Comment,
                    IsInternal = dto.IsInternal && userRole != "Customer", // Müşteri internal yorum yapamaz
                    CreatedAt = DateTime.UtcNow
                };

                _context.TicketComments.Add(comment);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Yorum eklendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        #region Helper Methods

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return Guid.Parse(userIdClaim.Value);
        }

        private string GetCurrentUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value ?? "Customer";
        }

        private Guid? GetCurrentUserCompanyId()
        {
            var companyClaim = User.FindFirst("CompanyId");
            if (companyClaim != null && Guid.TryParse(companyClaim.Value, out Guid companyId))
            {
                return companyId;
            }
            return null;
        }

        private async Task<string> GenerateTicketNumberAsync()
        {
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month;

            // Bu ay oluşturulan ticket sayısını bul
            var monthlyCount = await _context.Tickets
                .Where(t => t.CreatedAt.Year == year && t.CreatedAt.Month == month)
                .CountAsync();

            // Format: TK-2024-08-001
            return $"TK-{year}-{month:D2}-{(monthlyCount + 1):D3}";
        }

        #endregion
    }
}
