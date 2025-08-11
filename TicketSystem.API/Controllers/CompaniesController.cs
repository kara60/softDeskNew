using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TicketSystem.API.Data;
using TicketSystem.API.Models;
using TicketSystem.API.DTOs;

namespace TicketSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Login gerekli
    public class CompaniesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CompaniesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/companies
        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")] // SuperAdmin tümünü, Admin kendi şirketini görebilir
        public async Task<IActionResult> GetCompanies([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var currentUserRole = GetCurrentUserRole();
                var currentUserCompanyId = GetCurrentUserCompanyId();

                IQueryable<Company> query = _context.Companies.Where(c => c.IsActive);

                // Admin sadece kendi şirketini görebilir
                if (currentUserRole == "Admin")
                {
                    query = query.Where(c => c.Id == currentUserCompanyId);
                }
                // SuperAdmin tüm şirketleri görebilir

                var totalCount = await query.CountAsync();

                var companies = await query
                    .OrderBy(c => c.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new CompanyListDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Email = c.Email,
                        Phone = c.Phone,
                        ContactPerson = c.ContactPerson,
                        IsActive = c.IsActive,
                        TicketCredits = c.TicketCredits,
                        PlanType = c.PlanType,
                        MonthlyTicketLimit = c.MonthlyTicketLimit,
                        CreatedAt = c.CreatedAt,
                        UserCount = _context.Users.Count(u => u.CompanyId == c.Id && u.IsActive),
                        TicketCount = _context.Tickets.Count(t => t.CompanyId == c.Id)
                    })
                    .ToListAsync();

                return Ok(new
                {
                    companies = companies,
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

        // GET: api/companies/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetCompany(Guid id)
        {
            try
            {
                var currentUserRole = GetCurrentUserRole();
                var currentUserCompanyId = GetCurrentUserCompanyId();

                // Admin sadece kendi şirketini görebilir
                if (currentUserRole == "Admin" && id != currentUserCompanyId)
                {
                    return Forbid("Bu şirketi görüntüleme yetkiniz yok.");
                }

                var company = await _context.Companies
                    .Where(c => c.Id == id)
                    .Select(c => new CompanyDetailDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        DatabaseName = c.DatabaseName,
                        Address = c.Address,
                        Phone = c.Phone,
                        Email = c.Email,
                        ContactPerson = c.ContactPerson,
                        IsActive = c.IsActive,
                        TicketCredits = c.TicketCredits,
                        PlanType = c.PlanType,
                        MonthlyTicketLimit = c.MonthlyTicketLimit,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        UserCount = _context.Users.Count(u => u.CompanyId == c.Id && u.IsActive),
                        TicketCount = _context.Tickets.Count(t => t.CompanyId == c.Id),
                        OpenTicketCount = _context.Tickets.Count(t => t.CompanyId == c.Id && t.Status == "Open"),
                        ResolvedTicketCount = _context.Tickets.Count(t => t.CompanyId == c.Id && t.Status == "Resolved")
                    })
                    .FirstOrDefaultAsync();

                if (company == null)
                {
                    return NotFound(new { message = "Şirket bulunamadı." });
                }

                return Ok(company);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/companies
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")] // Sadece SuperAdmin şirket oluşturabilir
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyDto dto)
        {
            try
            {
                // Database name uniqe olmalı
                var existingCompany = await _context.Companies
                    .AnyAsync(c => c.DatabaseName == dto.DatabaseName);

                if (existingCompany)
                {
                    return BadRequest(new { message = "Bu database adı zaten kullanılıyor." });
                }

                var company = new Company
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    DatabaseName = dto.DatabaseName,
                    Address = dto.Address,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    ContactPerson = dto.ContactPerson,
                    TicketCredits = dto.TicketCredits,
                    PlanType = dto.PlanType,
                    MonthlyTicketLimit = dto.MonthlyTicketLimit,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Şirket başarıyla oluşturuldu.",
                    companyId = company.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // PUT: api/companies/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] UpdateCompanyDto dto)
        {
            try
            {
                var currentUserRole = GetCurrentUserRole();
                var currentUserCompanyId = GetCurrentUserCompanyId();

                // Admin sadece kendi şirketini güncelleyebilir
                if (currentUserRole == "Admin" && id != currentUserCompanyId)
                {
                    return Forbid("Bu şirketi güncelleme yetkiniz yok.");
                }

                var company = await _context.Companies.FindAsync(id);
                if (company == null)
                {
                    return NotFound(new { message = "Şirket bulunamadı." });
                }

                // Basic bilgileri güncelle
                company.Name = dto.Name;
                company.Address = dto.Address;
                company.Phone = dto.Phone;
                company.Email = dto.Email;
                company.ContactPerson = dto.ContactPerson;
                company.UpdatedAt = DateTime.UtcNow;

                // SuperAdmin kontör ve plan ayarlarını değiştirebilir
                if (currentUserRole == "SuperAdmin")
                {
                    company.TicketCredits = dto.TicketCredits;
                    company.PlanType = dto.PlanType;
                    company.MonthlyTicketLimit = dto.MonthlyTicketLimit;
                    company.IsActive = dto.IsActive;
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Şirket bilgileri güncellendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // PUT: api/companies/{id}/credits
        [HttpPut("{id}/credits")]
        [Authorize(Roles = "SuperAdmin")] // Sadece SuperAdmin kontör ekleyebilir
        public async Task<IActionResult> AddCredits(Guid id, [FromBody] AddCreditsDto dto)
        {
            try
            {
                var company = await _context.Companies.FindAsync(id);
                if (company == null)
                {
                    return NotFound(new { message = "Şirket bulunamadı." });
                }

                company.TicketCredits += dto.Credits;
                company.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = $"{dto.Credits} kontör eklendi.",
                    totalCredits = company.TicketCredits
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/companies/{id}/users
        [HttpGet("{id}/users")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetCompanyUsers(Guid id)
        {
            try
            {
                var currentUserRole = GetCurrentUserRole();
                var currentUserCompanyId = GetCurrentUserCompanyId();

                // Admin sadece kendi şirketinin kullanıcılarını görebilir
                if (currentUserRole == "Admin" && id != currentUserCompanyId)
                {
                    return Forbid("Bu şirketin kullanıcılarını görüntüleme yetkiniz yok.");
                }

                var users = await _context.Users
                    .Where(u => u.CompanyId == id && u.IsActive)
                    .Select(u => new SimpleUserDto
                    {
                        Id = u.Id,
                        Name = u.FirstName + " " + u.LastName,
                        Email = u.Email,
                        CompanyName = u.Company.Name
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/companies/{id}/tickets
        [HttpGet("{id}/tickets")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetCompanyTickets(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var currentUserRole = GetCurrentUserRole();
                var currentUserCompanyId = GetCurrentUserCompanyId();

                // Admin sadece kendi şirketinin ticketlarını görebilir
                if (currentUserRole == "Admin" && id != currentUserCompanyId)
                {
                    return Forbid("Bu şirketin ticketlarını görüntüleme yetkiniz yok.");
                }

                var totalCount = await _context.Tickets.CountAsync(t => t.CompanyId == id);

                var tickets = await _context.Tickets
                    .Include(t => t.CreatedByUser)
                    .Include(t => t.TicketType)
                    .Where(t => t.CompanyId == id)
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new TicketListDto
                    {
                        Id = t.Id,
                        TicketNumber = t.TicketNumber,
                        Title = t.Title,
                        Status = t.Status,
                        Priority = t.Priority,
                        CreatedAt = t.CreatedAt,
                        CreatedByName = t.CreatedByUser.FirstName + " " + t.CreatedByUser.LastName,
                        TicketTypeName = t.TicketType.Name
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

        // DELETE: api/companies/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")] // Sadece SuperAdmin şirket silebilir
        public async Task<IActionResult> DeleteCompany(Guid id)
        {
            try
            {
                var company = await _context.Companies.FindAsync(id);
                if (company == null)
                {
                    return NotFound(new { message = "Şirket bulunamadı." });
                }

                // Soft delete
                company.IsActive = false;
                company.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Şirket pasif hale getirildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        #region Helper Methods

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

        #endregion
    }
}