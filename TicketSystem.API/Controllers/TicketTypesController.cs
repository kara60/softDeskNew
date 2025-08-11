using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSystem.API.Data;
using TicketSystem.API.Models;
using TicketSystem.API.DTOs;

namespace TicketSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Login gerekli
    public class TicketTypesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TicketTypesController(AppDbContext context)
        {
            _context = context;
        }

        #region TicketTypes Management

        // GET: api/tickettypes
        [HttpGet]
        public async Task<IActionResult> GetTicketTypes()
        {
            try
            {
                var ticketTypes = await _context.TicketTypes
                    .Where(tt => tt.IsActive)
                    .OrderBy(tt => tt.SortOrder)
                    .Select(tt => new TicketTypeDto
                    {
                        Id = tt.Id,
                        Name = tt.Name,
                        Description = tt.Description,
                        Icon = tt.Icon,
                        Color = tt.Color,
                        SortOrder = tt.SortOrder,
                        IsActive = tt.IsActive,
                        FormFieldCount = tt.FormFields.Count(f => f.IsActive)
                    })
                    .ToListAsync();

                return Ok(ticketTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/tickettypes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketType(Guid id)
        {
            try
            {
                var ticketType = await _context.TicketTypes
                    .Include(tt => tt.FormFields.Where(f => f.IsActive))
                    .Where(tt => tt.Id == id)
                    .Select(tt => new TicketTypeDetailDto
                    {
                        Id = tt.Id,
                        Name = tt.Name,
                        Description = tt.Description,
                        Icon = tt.Icon,
                        Color = tt.Color,
                        SortOrder = tt.SortOrder,
                        IsActive = tt.IsActive,
                        FormFields = tt.FormFields
                            .Where(f => f.IsActive)
                            .OrderBy(f => f.SortOrder)
                            .Select(f => new FormFieldDto
                            {
                                Id = f.Id,
                                FieldName = f.FieldName,
                                DisplayName = f.DisplayName,
                                FieldType = f.FieldType,
                                DefaultValue = f.DefaultValue,
                                PlaceholderText = f.PlaceholderText,
                                HelpText = f.HelpText,
                                IsRequired = f.IsRequired,
                                SortOrder = f.SortOrder,
                                MinLength = f.MinLength,
                                MaxLength = f.MaxLength,
                                ValidationRules = f.ValidationRules,
                                Options = f.Options
                            }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (ticketType == null)
                {
                    return NotFound(new { message = "Ticket türü bulunamadı." });
                }

                return Ok(ticketType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/tickettypes
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateTicketType([FromBody] CreateTicketTypeDto dto)
        {
            try
            {
                var ticketType = new TicketType
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Description = dto.Description,
                    Icon = dto.Icon,
                    Color = dto.Color,
                    SortOrder = dto.SortOrder,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.TicketTypes.Add(ticketType);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Ticket türü başarıyla oluşturuldu.",
                    ticketTypeId = ticketType.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // PUT: api/tickettypes/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> UpdateTicketType(Guid id, [FromBody] UpdateTicketTypeDto dto)
        {
            try
            {
                var ticketType = await _context.TicketTypes.FindAsync(id);
                if (ticketType == null)
                {
                    return NotFound(new { message = "Ticket türü bulunamadı." });
                }

                ticketType.Name = dto.Name;
                ticketType.Description = dto.Description;
                ticketType.Icon = dto.Icon;
                ticketType.Color = dto.Color;
                ticketType.SortOrder = dto.SortOrder;
                ticketType.IsActive = dto.IsActive;
                ticketType.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Ticket türü güncellendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        #endregion

        #region Categories Management

        // GET: api/tickettypes/categories
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.TicketCategories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .Select(c => new TicketCategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        Icon = c.Icon,
                        Color = c.Color,
                        SortOrder = c.SortOrder,
                        ModuleCount = c.Modules.Count(m => m.IsActive)
                    })
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/tickettypes/categories/{id}/modules
        [HttpGet("categories/{id}/modules")]
        public async Task<IActionResult> GetCategoryModules(Guid id)
        {
            try
            {
                var modules = await _context.TicketModules
                    .Where(m => m.CategoryId == id && m.IsActive)
                    .OrderBy(m => m.SortOrder)
                    .Select(m => new TicketModuleDto
                    {
                        Id = m.Id,
                        CategoryId = m.CategoryId,
                        Name = m.Name,
                        Description = m.Description,
                        Icon = m.Icon,
                        Color = m.Color,
                        SortOrder = m.SortOrder
                    })
                    .ToListAsync();

                return Ok(modules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/tickettypes/categories
        [HttpPost("categories")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            try
            {
                var category = new TicketCategory
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Description = dto.Description,
                    Icon = dto.Icon,
                    Color = dto.Color,
                    SortOrder = dto.SortOrder,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.TicketCategories.Add(category);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Kategori başarıyla oluşturuldu.",
                    categoryId = category.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/tickettypes/categories/{categoryId}/modules
        [HttpPost("categories/{categoryId}/modules")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateModule(Guid categoryId, [FromBody] CreateModuleDto dto)
        {
            try
            {
                var category = await _context.TicketCategories.FindAsync(categoryId);
                if (category == null)
                {
                    return NotFound(new { message = "Kategori bulunamadı." });
                }

                var module = new TicketModule
                {
                    Id = Guid.NewGuid(),
                    CategoryId = categoryId,
                    Name = dto.Name,
                    Description = dto.Description,
                    Icon = dto.Icon,
                    Color = dto.Color,
                    SortOrder = dto.SortOrder,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.TicketModules.Add(module);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Modül başarıyla oluşturuldu.",
                    moduleId = module.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        #endregion

        #region Form Fields Management

        // GET: api/tickettypes/{ticketTypeId}/formfields
        [HttpGet("{ticketTypeId}/formfields")]
        public async Task<IActionResult> GetFormFields(Guid ticketTypeId)
        {
            try
            {
                var formFields = await _context.FormFields
                    .Where(f => f.TicketTypeId == ticketTypeId && f.IsActive)
                    .OrderBy(f => f.SortOrder)
                    .Select(f => new FormFieldDto
                    {
                        Id = f.Id,
                        FieldName = f.FieldName,
                        DisplayName = f.DisplayName,
                        FieldType = f.FieldType,
                        DefaultValue = f.DefaultValue,
                        PlaceholderText = f.PlaceholderText,
                        HelpText = f.HelpText,
                        IsRequired = f.IsRequired,
                        SortOrder = f.SortOrder,
                        MinLength = f.MinLength,
                        MaxLength = f.MaxLength,
                        ValidationRules = f.ValidationRules,
                        Options = f.Options
                    })
                    .ToListAsync();

                return Ok(formFields);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/tickettypes/{ticketTypeId}/formfields
        [HttpPost("{ticketTypeId}/formfields")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateFormField(Guid ticketTypeId, [FromBody] CreateFormFieldDto dto)
        {
            try
            {
                var ticketType = await _context.TicketTypes.FindAsync(ticketTypeId);
                if (ticketType == null)
                {
                    return NotFound(new { message = "Ticket türü bulunamadı." });
                }

                var formField = new FormField
                {
                    Id = Guid.NewGuid(),
                    TicketTypeId = ticketTypeId,
                    FieldName = dto.FieldName,
                    DisplayName = dto.DisplayName,
                    FieldType = dto.FieldType,
                    DefaultValue = dto.DefaultValue,
                    PlaceholderText = dto.PlaceholderText,
                    HelpText = dto.HelpText,
                    IsRequired = dto.IsRequired,
                    SortOrder = dto.SortOrder,
                    MinLength = dto.MinLength,
                    MaxLength = dto.MaxLength,
                    ValidationRules = dto.ValidationRules,
                    Options = dto.Options,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.FormFields.Add(formField);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Form alanı başarıyla oluşturuldu.",
                    formFieldId = formField.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // PUT: api/tickettypes/formfields/{id}
        [HttpPut("formfields/{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> UpdateFormField(Guid id, [FromBody] UpdateFormFieldDto dto)
        {
            try
            {
                var formField = await _context.FormFields.FindAsync(id);
                if (formField == null)
                {
                    return NotFound(new { message = "Form alanı bulunamadı." });
                }

                formField.FieldName = dto.FieldName;
                formField.DisplayName = dto.DisplayName;
                formField.FieldType = dto.FieldType;
                formField.DefaultValue = dto.DefaultValue;
                formField.PlaceholderText = dto.PlaceholderText;
                formField.HelpText = dto.HelpText;
                formField.IsRequired = dto.IsRequired;
                formField.SortOrder = dto.SortOrder;
                formField.MinLength = dto.MinLength;
                formField.MaxLength = dto.MaxLength;
                formField.ValidationRules = dto.ValidationRules;
                formField.Options = dto.Options;
                formField.IsActive = dto.IsActive;
                formField.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Form alanı güncellendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        // DELETE: api/tickettypes/formfields/{id}
        [HttpDelete("formfields/{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> DeleteFormField(Guid id)
        {
            try
            {
                var formField = await _context.FormFields.FindAsync(id);
                if (formField == null)
                {
                    return NotFound(new { message = "Form alanı bulunamadı." });
                }

                // Soft delete
                formField.IsActive = false;
                formField.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Form alanı silindi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

        #endregion

        #region Utility Endpoints

        // GET: api/tickettypes/field-types
        [HttpGet("field-types")]
        public IActionResult GetFieldTypes()
        {
            var fieldTypes = new[]
            {
                new { value = "text", label = "Metin", description = "Tek satır metin girişi" },
                new { value = "textarea", label = "Çok Satırlı Metin", description = "Uzun metin girişi" },
                new { value = "select", label = "Seçim Listesi", description = "Dropdown liste" },
                new { value = "radio", label = "Radyo Buton", description = "Tek seçenek" },
                new { value = "checkbox", label = "Onay Kutusu", description = "Evet/Hayır seçimi" },
                new { value = "email", label = "Email", description = "Email adresi girişi" },
                new { value = "number", label = "Sayı", description = "Numerik giriş" },
                new { value = "date", label = "Tarih", description = "Tarih seçici" },
                new { value = "file", label = "Dosya", description = "Dosya yükleme" }
            };

            return Ok(fieldTypes);
        }

        #endregion
    }
}