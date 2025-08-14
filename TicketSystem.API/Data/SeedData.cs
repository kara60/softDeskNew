// TicketSystem.API/Data/SeedData.cs
using Microsoft.AspNetCore.Identity;
using TicketSystem.API.Models;

namespace TicketSystem.API.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            // Database oluştur
            await context.Database.EnsureCreatedAsync();

            // Roller oluştur
            await CreateRolesAsync(roleManager);

            // Şirketler oluştur
            await CreateCompaniesAsync(context);

            // Kullanıcılar oluştur
            await CreateUsersAsync(userManager, context);

            // Ticket türleri oluştur
            await CreateTicketTypesAsync(context);

            // Kategoriler oluştur
            await CreateCategoriesAsync(context);

            await context.SaveChangesAsync();
        }

        private static async Task CreateRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
        {
            string[] roleNames = { "SuperAdmin", "Admin", "Support", "User" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid> { Name = roleName });
                }
            }
        }

        private static async Task CreateCompaniesAsync(AppDbContext context)
        {
            if (!context.Companies.Any())
            {
                var companies = new[]
                {
                    new Company
                    {
                        Id = Guid.NewGuid(),
                        Name = "BANAT FIRÇA VE PLASTİK SAN. A.Ş.",
                        DatabaseName = "banat_db",
                        Email = "info@banatfirca.com",
                        Phone = "0212 123 45 67",
                        ContactPerson = "Ayşe Banat",
                        IsActive = true,
                        MonthlyTicketLimit = 100,
                        PlanType = "PREMIUM"
                    },
                    new Company
                    {
                        Id = Guid.NewGuid(),
                        Name = "SOYBAŞ DEMİR SANAYİ VE TİCARET ANONİM ŞİRKETİ",
                        DatabaseName = "soybas_db",
                        Email = "info@soybas.com.tr",
                        Phone = "0212 987 65 43",
                        ContactPerson = "Mehmet Soybaş",
                        IsActive = true,
                        MonthlyTicketLimit = 50,
                        PlanType = "BASIC"
                    },
                    new Company
                    {
                        Id = Guid.NewGuid(),
                        Name = "TEKNOS MAKİNA SANAYİ LTD. ŞTİ.",
                        DatabaseName = "teknos_db",
                        Email = "info@teknos.com",
                        Phone = "0216 555 44 33",
                        ContactPerson = "Ahmet Teknik",
                        IsActive = true,
                        MonthlyTicketLimit = 75,
                        PlanType = "BASIC"
                    }
                };

                context.Companies.AddRange(companies);
                await context.SaveChangesAsync();
            }
        }

        private static async Task CreateUsersAsync(UserManager<User> userManager, AppDbContext context)
        {
            // Admin Kullanıcı
            if (await userManager.FindByEmailAsync("admin@softdesk.com") == null)
            {
                var adminUser = new User
                {
                    UserName = "admin",
                    Email = "admin@softdesk.com",
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true,
                    IsActive = true,
                    CompanyId = null // Admin şirket bağımsız
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Demo User 1 - BANAT FIRÇA
            var banatCompany = context.Companies.FirstOrDefault(c => c.Name.Contains("BANAT"));
            if (banatCompany != null && await userManager.FindByEmailAsync("user@banatfirca.com") == null)
            {
                var banatUser = new User
                {
                    UserName = "user",
                    Email = "user@banatfirca.com",
                    FirstName = "Test",
                    LastName = "Kullanıcı",
                    EmailConfirmed = true,
                    IsActive = true,
                    CompanyId = banatCompany.Id
                };

                var result = await userManager.CreateAsync(banatUser, "User123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(banatUser, "User");
                }
            }

            // Demo User 2 - SOYBAŞ
            var soybasCompany = context.Companies.FirstOrDefault(c => c.Name.Contains("SOYBAŞ"));
            if (soybasCompany != null && await userManager.FindByEmailAsync("demo@soybas.com") == null)
            {
                var soybasUser = new User
                {
                    UserName = "demo",
                    Email = "demo@soybas.com",
                    FirstName = "Demo",
                    LastName = "Soybaş",
                    EmailConfirmed = true,
                    IsActive = true,
                    CompanyId = soybasCompany.Id
                };

                var result = await userManager.CreateAsync(soybasUser, "Demo123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(soybasUser, "User");
                }
            }

            // Support Kullanıcı
            if (await userManager.FindByEmailAsync("support@softdesk.com") == null)
            {
                var supportUser = new User
                {
                    UserName = "support",
                    Email = "support@softdesk.com",
                    FirstName = "Support",
                    LastName = "Team",
                    EmailConfirmed = true,
                    IsActive = true,
                    CompanyId = null
                };

                var result = await userManager.CreateAsync(supportUser, "Support123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(supportUser, "Support");
                }
            }
        }

        private static async Task CreateTicketTypesAsync(AppDbContext context)
        {
            if (!context.TicketTypes.Any())
            {
                var ticketTypes = new[]
                {
                    new TicketType
                    {
                        Name = "Hata/Sorun Bildirimi",
                        Description = "Sistem hataları ve teknik sorunlar için",
                        Icon = "🐛",
                        Color = "#ef4444",
                        SortOrder = 1,
                        IsActive = true
                    },
                    new TicketType
                    {
                        Name = "Yeni Özellik Talebi",
                        Description = "Sistem geliştirme ve yeni özellik önerileri",
                        Icon = "💡",
                        Color = "#3b82f6",
                        SortOrder = 2,
                        IsActive = true
                    },
                    new TicketType
                    {
                        Name = "Genel Destek",
                        Description = "Kullanım desteği ve genel sorular",
                        Icon = "❓",
                        Color = "#8b5cf6",
                        SortOrder = 3,
                        IsActive = true
                    },
                    new TicketType
                    {
                        Name = "Eğitim Talebi",
                        Description = "Kullanıcı eğitimi ve dokümantasyon",
                        Icon = "🎓",
                        Color = "#10b981",
                        SortOrder = 4,
                        IsActive = true
                    }
                };

                context.TicketTypes.AddRange(ticketTypes);
                await context.SaveChangesAsync();
            }
        }

        private static async Task CreateCategoriesAsync(AppDbContext context)
        {
            if (!context.TicketCategories.Any())
            {
                var categories = new[]
                {
                    new TicketCategory
                    {
                        Name = "ERP Sistemi",
                        Description = "Kurumsal kaynak planlama modülleri",
                        Icon = "💼",
                        Color = "#f97316",
                        SortOrder = 1,
                        IsActive = true
                    },
                    new TicketCategory
                    {
                        Name = "CRM",
                        Description = "Müşteri ilişkileri yönetimi",
                        Icon = "🎯",
                        Color = "#10b981",
                        SortOrder = 2,
                        IsActive = true
                    },
                    new TicketCategory
                    {
                        Name = "Lopus",
                        Description = "Personel yönetim sistemi",
                        Icon = "👥",
                        Color = "#8b5cf6",
                        SortOrder = 3,
                        IsActive = true
                    }
                };

                context.TicketCategories.AddRange(categories);
                await context.SaveChangesAsync();

                // Modüller ekle
                var erpCategory = categories[0];
                var modules = new[]
                {
                    new TicketModule
                    {
                        CategoryId = erpCategory.Id,
                        Name = "Muhasebe",
                        Description = "Mali işlemler ve raporlama",
                        Icon = "💰",
                        Color = "#10b981",
                        SortOrder = 1,
                        IsActive = true
                    },
                    new TicketModule
                    {
                        CategoryId = erpCategory.Id,
                        Name = "Stok Yönetimi",
                        Description = "Depo ve stok takibi",
                        Icon = "📦",
                        Color = "#3b82f6",
                        SortOrder = 2,
                        IsActive = true
                    }
                };

                context.TicketModules.AddRange(modules);
                await context.SaveChangesAsync();
            }
        }
    }
}