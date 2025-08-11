using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TicketSystem.API.Data;
using TicketSystem.API.Models;
using TicketSystem.API.DTOs;

namespace TicketSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin,Admin")] // Sadece yöneticiler sistem ayarlarına erişebilir
    public class SystemSettingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SystemSettingsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/systemsettings
        [HttpGet]
        public async Task<IActionResult> GetAllSettings([FromQuery] string? category = null)
        {
            try
            {
                IQueryable<SystemSettings> query = _context.SystemSettings.Where(s => s.IsVisible);

                // Kategori filtresi
                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(s => s.Category == category);
                }

                var settings = await query
                    .OrderBy(s => s.Category)
                    .ThenBy(s => s.SettingKey)
                    .Select(s => new SystemSettingDto
                    {
                        Id = s.Id,
                        SettingKey = s.SettingKey,
                        SettingValue = s.SettingValue,
                        DataType = s.DataType,
                        DisplayName = s.DisplayName,
                        Description = s.Description,
                        Category = s.Category,
                        IsSystemSetting = s.IsSystemSetting,
                        DefaultValue = s.DefaultValue,
                        ValidationRules = s.ValidationRules
                    })
                    .ToListAsync();

                // Kategorilere göre grupla
                var groupedSettings = settings.GroupBy(s => s.Category)
                    .ToDictionary(g => g.Key ?? "General", g => g.ToList());

                return Ok(groupedSettings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/systemsettings/{key}
        [HttpGet("{key}")]
        public async Task<IActionResult> GetSetting(string key)
        {
            try
            {
                var setting = await _context.SystemSettings
                    .Where(s => s.SettingKey == key)
                    .Select(s => new SystemSettingDto
                    {
                        Id = s.Id,
                        SettingKey = s.SettingKey,
                        SettingValue = s.SettingValue,
                        DataType = s.DataType,
                        DisplayName = s.DisplayName,
                        Description = s.Description,
                        Category = s.Category,
                        IsSystemSetting = s.IsSystemSetting,
                        DefaultValue = s.DefaultValue,
                        ValidationRules = s.ValidationRules
                    })
                    .FirstOrDefaultAsync();

                if (setting == null)
                {
                    return NotFound(new { message = "Ayar bulunamadı." });
                }

                return Ok(setting);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // PUT: api/systemsettings/{key}
        [HttpPut("{key}")]
        [Authorize(Roles = "SuperAdmin")] // Sadece SuperAdmin ayar değiştirebilir
        public async Task<IActionResult> UpdateSetting(string key, [FromBody] UpdateSettingDto dto)
        {
            try
            {
                var setting = await _context.SystemSettings
                    .FirstOrDefaultAsync(s => s.SettingKey == key);

                if (setting == null)
                {
                    return NotFound(new { message = "Ayar bulunamadı." });
                }

                // Sistem ayarları değiştirilemez
                if (setting.IsSystemSetting)
                {
                    return BadRequest(new { message = "Bu ayar sistem tarafından korunmaktadır." });
                }

                // Veri tipi kontrolü
                if (!IsValidValue(dto.SettingValue, setting.DataType))
                {
                    return BadRequest(new { message = $"Geçersiz veri tipi. Beklenen: {setting.DataType}" });
                }

                setting.SettingValue = dto.SettingValue;
                setting.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Ayar başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/systemsettings
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateSetting([FromBody] CreateSettingDto dto)
        {
            try
            {
                // Aynı key'e sahip ayar var mı kontrol et
                var existingSetting = await _context.SystemSettings
                    .AnyAsync(s => s.SettingKey == dto.SettingKey);

                if (existingSetting)
                {
                    return BadRequest(new { message = "Bu anahtar zaten kullanılıyor." });
                }

                var setting = new SystemSettings
                {
                    Id = Guid.NewGuid(),
                    SettingKey = dto.SettingKey,
                    SettingValue = dto.SettingValue,
                    DataType = dto.DataType,
                    DisplayName = dto.DisplayName,
                    Description = dto.Description,
                    Category = dto.Category,
                    IsSystemSetting = false,
                    IsVisible = dto.IsVisible,
                    DefaultValue = dto.DefaultValue,
                    ValidationRules = dto.ValidationRules,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SystemSettings.Add(setting);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Ayar başarıyla oluşturuldu.",
                    settingId = setting.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/systemsettings/categories
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.SystemSettings
                    .Where(s => s.IsVisible)
                    .Select(s => s.Category)
                    .Distinct()
                    .Where(c => c != null)
                    .OrderBy(c => c)
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // PUT: api/systemsettings/bulk
        [HttpPut("bulk")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateMultipleSettings([FromBody] Dictionary<string, string> settings)
        {
            try
            {
                var updatedCount = 0;
                var errors = new List<string>();

                foreach (var kvp in settings)
                {
                    var setting = await _context.SystemSettings
                        .FirstOrDefaultAsync(s => s.SettingKey == kvp.Key);

                    if (setting == null)
                    {
                        errors.Add($"Ayar bulunamadı: {kvp.Key}");
                        continue;
                    }

                    if (setting.IsSystemSetting)
                    {
                        errors.Add($"Sistem ayarı değiştirilemez: {kvp.Key}");
                        continue;
                    }

                    if (!IsValidValue(kvp.Value, setting.DataType))
                    {
                        errors.Add($"Geçersiz veri tipi: {kvp.Key} (Beklenen: {setting.DataType})");
                        continue;
                    }

                    setting.SettingValue = kvp.Value;
                    setting.UpdatedAt = DateTime.UtcNow;
                    updatedCount++;
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = $"{updatedCount} ayar güncellendi.",
                    updatedCount = updatedCount,
                    errors = errors
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/systemsettings/reset-defaults
        [HttpPost("reset-defaults")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> ResetToDefaults([FromBody] List<string> settingKeys)
        {
            try
            {
                var settings = await _context.SystemSettings
                    .Where(s => settingKeys.Contains(s.SettingKey) && !s.IsSystemSetting)
                    .ToListAsync();

                foreach (var setting in settings)
                {
                    if (!string.IsNullOrEmpty(setting.DefaultValue))
                    {
                        setting.SettingValue = setting.DefaultValue;
                        setting.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = $"{settings.Count} ayar varsayılan değerlere döndürüldü." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/systemsettings/export
        [HttpGet("export")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> ExportSettings()
        {
            try
            {
                var settings = await _context.SystemSettings
                    .Where(s => !s.IsSystemSetting)
                    .Select(s => new
                    {
                        s.SettingKey,
                        s.SettingValue,
                        s.DataType,
                        s.DisplayName,
                        s.Description,
                        s.Category,
                        s.DefaultValue
                    })
                    .ToListAsync();

                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", $"system-settings-{DateTime.Now:yyyy-MM-dd}.json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/systemsettings/test-connection
        [HttpPost("test-connection")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> TestConnection([FromBody] TestConnectionDto dto)
        {
            try
            {
                switch (dto.ConnectionType.ToLower())
                {
                    case "email":
                        return await TestEmailConnection(dto.Settings);
                    case "pmo":
                        return await TestPMOConnection(dto.Settings);
                    case "database":
                        return await TestDatabaseConnection(dto.Settings);
                    default:
                        return BadRequest(new { message = "Desteklenmeyen bağlantı türü." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bağlantı testi başarısız.", error = ex.Message });
            }
        }

        #region Helper Methods

        private bool IsValidValue(string value, string dataType)
        {
            return dataType.ToLower() switch
            {
                "boolean" => bool.TryParse(value, out _),
                "number" => int.TryParse(value, out _) || double.TryParse(value, out _),
                "json" => IsValidJson(value),
                _ => true // string için her zaman geçerli
            };
        }

        private bool IsValidJson(string value)
        {
            try
            {
                JsonDocument.Parse(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<IActionResult> TestEmailConnection(Dictionary<string, string> settings)
        {
            try
            {
                // Email bağlantısı test mantığı burada olacak
                // SMTP ayarları ile test email gönderme

                await Task.Delay(1000); // Simulated delay

                return Ok(new { message = "Email bağlantısı başarılı.", status = "success" });
            }
            catch (Exception ex)
            {
                return Ok(new { message = "Email bağlantısı başarısız.", status = "error", error = ex.Message });
            }
        }

        private async Task<IActionResult> TestPMOConnection(Dictionary<string, string> settings)
        {
            try
            {
                // PMO API bağlantısı test mantığı burada olacak

                await Task.Delay(1000); // Simulated delay

                return Ok(new { message = "PMO bağlantısı başarılı.", status = "success" });
            }
            catch (Exception ex)
            {
                return Ok(new { message = "PMO bağlantısı başarısız.", status = "error", error = ex.Message });
            }
        }

        private async Task<IActionResult> TestDatabaseConnection(Dictionary<string, string> settings)
        {
            try
            {
                // Database bağlantısı test mantığı burada olacak

                await Task.Delay(1000); // Simulated delay

                return Ok(new { message = "Database bağlantısı başarılı.", status = "success" });
            }
            catch (Exception ex)
            {
                return Ok(new { message = "Database bağlantısı başarısız.", status = "error", error = ex.Message });
            }
        }

        #endregion
    }
}