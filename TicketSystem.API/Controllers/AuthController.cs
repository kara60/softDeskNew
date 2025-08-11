using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TicketSystem.API.DTOs;
using TicketSystem.API.Models;

namespace TicketSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> SignInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = SignInManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            return BadRequest(new
            {
                message = "Kayıt işlemi kapalıdır. Admin ile iletişime geçin.",
                contact = "admin@softticket.com"
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                // Kullanıcıyı bul
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    return BadRequest(new { message = "Email veya şifre hatalı." });
                }

                // Şifre kontrolü
                var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Email veya şifre hatalı." });
                }

                // Kullanıcı aktif mi?
                if (!user.IsActive)
                {
                    return BadRequest(new { message = "Hesabınız aktif değil." });
                }

                // JWT token oluştur
                var token = await GenerateJwtTokenAsync(user);

                return Ok(new
                {
                    message = "Giriş başarılı.",
                    token = token,
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        companyId = user.CompanyId
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Çıkış yapıldı." });
        }

        private async Task<string> GenerateJwtTokenAsync(User user)
        {
            // Kullanıcı rollerini al
            var roles = await _userManager.GetRolesAsync(user);

            // Token claims'leri oluştur
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim("CompanyId", user.CompanyId?.ToString() ?? "")
            };

            // Rolleri claims'e ekle
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // JWT ayarları
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expirationMinutes = int.Parse(jwtSettings["ExpirationInMinutes"]);

            // Security key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = credentials
            };

            // Token oluştur
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
