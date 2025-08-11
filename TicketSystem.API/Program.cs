using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TicketSystem.API.Data;
using TicketSystem.API.Models;
using TicketSystem.API.Services; // ?? YEN�: Services namespace

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero // Token expire s�resini kesin yap�yor
    };
});

// Authorization
builder.Services.AddAuthorization();

// ?? YEN�: CUSTOM SERV�SLER
// Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

// File Upload Service  
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

// ?? YEN�: HTTP CLIENT (Email i�in gerekli)
builder.Services.AddHttpClient();

// ?? YEN�: Static files zaten varsay�lan olarak desteklenir

// Controllers
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Swagger configuration with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Ticket System API",
        Version = "v1",
        Description = "Modern Ticket Management System with .NET 8" // ?? YEN�: A��klama
    });

    // JWT Authentication i�in Swagger'a security definition ekle
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", policy =>
    {
        policy.WithOrigins("https://localhost:5001", "http://localhost:5000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ?? YEN�: LOGGING CONFIGURATION
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ticket System API v1");
        c.RoutePrefix = "swagger"; // ?? YEN�: Swagger route
    });
}

app.UseHttpsRedirection();

// ?? YEN�: STATIC FILES MIDDLEWARE (File upload i�in)
app.UseStaticFiles();

app.UseCors("AllowBlazorApp");

// Authentication & Authorization - SIRASI �NEML�!
app.UseAuthentication();  // �nce kimlik do�rulama
app.UseAuthorization();   // Sonra yetkilendirme

app.MapControllers();

// Database migration ve seeding
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>(); // ?? YEN�: Logger

    try
    {
        // ?? YEN�: WWWROOT KLAS�R� OLU�TUR (File upload i�in)
        var webRootPath = app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");
        if (!Directory.Exists(webRootPath))
        {
            Directory.CreateDirectory(webRootPath);
            Directory.CreateDirectory(Path.Combine(webRootPath, "uploads"));
            logger.LogInformation("wwwroot ve uploads klas�rleri olu�turuldu");
        }

        // Database olu�tur
        context.Database.EnsureCreated();
        logger.LogInformation("Database ba�lant�s� ba�ar�l�");

        // Rolleri olu�tur
        await CreateRolesAsync(roleManager, logger);

        // Admin kullan�c� olu�tur
        await CreateAdminUserAsync(userManager, context, logger);

        logger.LogInformation("?? Database initialization completed successfully!");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "? Database initialization error: {Message}", ex.Message);
    }
}

app.Run();

// Rolleri olu�turma method'u
async Task CreateRolesAsync(RoleManager<IdentityRole<Guid>> roleManager, ILogger logger)
{
    string[] roles = { "Admin", "Customer", "Support" };

    foreach (string role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            logger.LogInformation("? Role created: {Role}", role);
        }
    }
}

// Admin kullan�c� olu�turma method'u
async Task CreateAdminUserAsync(UserManager<User> userManager, AppDbContext context, ILogger logger)
{
    var adminEmail = "admin@softticket.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        // Admin �irketi olu�tur
        var adminCompany = new Company
        {
            Id = Guid.NewGuid(),
            Name = "SoftTicket Admin",
            DatabaseName = "admin_db",
            Email = adminEmail,
            ContactPerson = "System Admin",
            IsActive = true
        };

        context.Companies.Add(adminCompany);
        await context.SaveChangesAsync();

        // Admin kullan�c� olu�tur
        adminUser = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "System",
            LastName = "Admin",
            CompanyId = adminCompany.Id,
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await userManager.CreateAsync(adminUser, "Admin123!");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            logger.LogInformation("?? Admin user created successfully!");
            logger.LogInformation("?? Email: {Email}", adminEmail);
            logger.LogInformation("?? Password: Admin123!");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                logger.LogError("Admin user creation error: {Error}", error.Description);
            }
        }
    }
}