using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly AppDbContext _context;

        public UsersController(
            UserManager<User> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: api/users
        [HttpGet]
        [Authorize(Roles = "Admin,Support")] // Sadece Admin ve Support görebilir
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var currentUserRole = GetCurrentUserRole();
                var currentUserCompanyId = GetCurrentUserCompanyId();

                IQueryable<User> query = _context.Users
                    .Include(u => u.Company)
                    .Where(u => u.IsActive);

                // Support sadece kendi şirketindeki kullanıcıları görebilir
                if (currentUserRole == "Support")
                {
                    query = query.Where(u => u.CompanyId == currentUserCompanyId);
                }
                // Admin tüm kullanıcıları görebilir

                var totalCount = await query.CountAsync();

                var users = await query
                    .OrderBy(u => u.FirstName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var userDtos = new List<UserListDto>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    userDtos.Add(new UserListDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        IsActive = user.IsActive,
                        CompanyName = user.Company?.Name ?? "Atanmamış",
                        Roles = roles.ToList(),
                        CreatedAt = user.CreatedAt
                    });
                }

                return Ok(new
                {
                    users = userDtos,
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

        // GET: api/users/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Support")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            try
            {
                var currentUserRole = GetCurrentUserRole();
                var currentUserCompanyId = GetCurrentUserCompanyId();

                var user = await _context.Users
                    .Include(u => u.Company)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı." });
                }

                // Support sadece kendi şirketindeki kullanıcıları görebilir
                if (currentUserRole == "Support" && user.CompanyId != currentUserCompanyId)
                {
                    return Forbid("Bu kullanıcıyı görüntüleme yetkiniz yok.");
                }

                var roles = await _userManager.GetRolesAsync(user);

                var userDto = new UserDetailDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    CompanyId = user.CompanyId,
                    CompanyName = user.Company?.Name ?? "Atanmamış",
                    Roles = roles.ToList(),
                    CreatedAt = user.CreatedAt
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/users
        [HttpPost]
        [Authorize(Roles = "Admin")] // Sadece Admin kullanıcı oluşturabilir
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                // Email kontrolü
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Bu email adresi zaten kullanılıyor." });
                }

                // Şirket kontrolü
                var company = await _context.Companies.FindAsync(dto.CompanyId);
                if (company == null)
                {
                    return BadRequest(new { message = "Geçersiz şirket ID." });
                }

                // Yeni kullanıcı oluştur
                var user = new User
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    CompanyId = dto.CompanyId,
                    EmailConfirmed = true,
                    IsActive = dto.IsActive
                };

                var result = await _userManager.CreateAsync(user, dto.Password);

                if (result.Succeeded)
                {
                    // Rolleri ata
                    if (dto.Roles != null && dto.Roles.Any())
                    {
                        await _userManager.AddToRolesAsync(user, dto.Roles);
                    }
                    else
                    {
                        // Default rol: Customer
                        await _userManager.AddToRoleAsync(user, "Customer");
                    }

                    return Ok(new
                    {
                        message = "Kullanıcı başarıyla oluşturuldu.",
                        userId = user.Id
                    });
                }

                return BadRequest(new
                {
                    message = "Kullanıcı oluşturulamadı.",
                    errors = result.Errors.Select(e => e.Description)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı." });
                }

                // Email değişikliği kontrolü
                if (user.Email != dto.Email)
                {
                    var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                    if (existingUser != null && existingUser.Id != user.Id)
                    {
                        return BadRequest(new { message = "Bu email adresi başka bir kullanıcı tarafından kullanılıyor." });
                    }
                }

                // Şirket kontrolü
                if (dto.CompanyId.HasValue)
                {
                    var company = await _context.Companies.FindAsync(dto.CompanyId.Value);
                    if (company == null)
                    {
                        return BadRequest(new { message = "Geçersiz şirket ID." });
                    }
                }

                // Kullanıcı bilgilerini güncelle
                user.FirstName = dto.FirstName;
                user.LastName = dto.LastName;
                user.Email = dto.Email;
                user.UserName = dto.Email;
                user.CompanyId = dto.CompanyId;
                user.IsActive = dto.IsActive;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // Rolleri güncelle
                    if (dto.Roles != null)
                    {
                        var currentRoles = await _userManager.GetRolesAsync(user);
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        await _userManager.AddToRolesAsync(user, dto.Roles);
                    }

                    return Ok(new { message = "Kullanıcı başarıyla güncellendi." });
                }

                return BadRequest(new
                {
                    message = "Kullanıcı güncellenemedi.",
                    errors = result.Errors.Select(e => e.Description)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı." });
                }

                // Soft delete (IsActive = false)
                user.IsActive = false;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return Ok(new { message = "Kullanıcı pasif hale getirildi." });
                }

                return BadRequest(new
                {
                    message = "Kullanıcı silinemedi.",
                    errors = result.Errors.Select(e => e.Description)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/users/roles
        [HttpGet("roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await _roleManager.Roles
                    .Select(r => new { id = r.Id, name = r.Name })
                    .ToListAsync();

                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // PUT: api/users/{id}/password
        [HttpPut("{id}/password")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordDto dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı." });
                }

                // Şifre sıfırlama token'ı oluştur
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Yeni şifreyi ata
                var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

                if (result.Succeeded)
                {
                    return Ok(new { message = "Şifre başarıyla değiştirildi." });
                }

                return BadRequest(new
                {
                    message = "Şifre değiştirilemedi.",
                    errors = result.Errors.Select(e => e.Description)
                });
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