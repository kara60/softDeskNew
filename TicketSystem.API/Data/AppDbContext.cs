using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TicketSystem.API.Models;

namespace TicketSystem.API.Data
{
    public class AppDbContext : IdentityDbContext<User, Microsoft.AspNetCore.Identity.IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets - Tablolarımız
        public DbSet<Company> Companies { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }
        public DbSet<TicketCategory> TicketCategories { get; set; }
        public DbSet<TicketModule> TicketModules { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<TicketAttachment> TicketAttachments { get; set; }
        public DbSet<FormField> FormFields { get; set; }
        public DbSet<TicketFormData> TicketFormData { get; set; }
        public DbSet<SystemSettings> SystemSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // User - Company ilişkisi
            builder.Entity<User>()
                .HasOne(u => u.Company)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CompanyId)
                .OnDelete(DeleteBehavior.SetNull);

            // Ticket ilişkileri
            builder.Entity<Ticket>()
                .HasOne(t => t.Company)
                .WithMany(c => c.Tickets)
                .HasForeignKey(t => t.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Ticket>()
                .HasOne(t => t.CreatedByUser)
                .WithMany(u => u.CreatedTickets)
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Ticket>()
                .HasOne(t => t.AssignedToUser)
                .WithMany(u => u.AssignedTickets)
                .HasForeignKey(t => t.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Ticket>()
                .HasOne(t => t.TicketType)
                .WithMany(tt => tt.Tickets)
                .HasForeignKey(t => t.TicketTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Ticket>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Tickets)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Ticket>()
                .HasOne(t => t.Module)
                .WithMany(m => m.Tickets)
                .HasForeignKey(t => t.ModuleId)
                .OnDelete(DeleteBehavior.SetNull);

            // TicketModule - TicketCategory ilişkisi
            builder.Entity<TicketModule>()
                .HasOne(tm => tm.Category)
                .WithMany(tc => tc.Modules)
                .HasForeignKey(tm => tm.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // FormField - TicketType ilişkisi
            builder.Entity<FormField>()
                .HasOne(ff => ff.TicketType)
                .WithMany(tt => tt.FormFields)
                .HasForeignKey(ff => ff.TicketTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            // TicketFormData ilişkileri
            builder.Entity<TicketFormData>()
                .HasOne(tfd => tfd.Ticket)
                .WithMany(t => t.FormData)
                .HasForeignKey(tfd => tfd.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TicketFormData>()
                .HasOne(tfd => tfd.FormField)
                .WithMany(ff => ff.FormData)
                .HasForeignKey(tfd => tfd.FormFieldId)
                .OnDelete(DeleteBehavior.Cascade);

            // TicketComment ilişkileri
            builder.Entity<TicketComment>()
                .HasOne(tc => tc.Ticket)
                .WithMany(t => t.Comments)
                .HasForeignKey(tc => tc.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TicketComment>()
                .HasOne(tc => tc.User)
                .WithMany()
                .HasForeignKey(tc => tc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // TicketAttachment ilişkileri
            builder.Entity<TicketAttachment>()
                .HasOne(ta => ta.Ticket)
                .WithMany(t => t.Attachments)
                .HasForeignKey(ta => ta.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TicketAttachment>()
                .HasOne(ta => ta.UploadedByUser)
                .WithMany()
                .HasForeignKey(ta => ta.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraints
            builder.Entity<Ticket>()
                .HasIndex(t => t.TicketNumber)
                .IsUnique();

            builder.Entity<Company>()
                .HasIndex(c => c.DatabaseName)
                .IsUnique();

            builder.Entity<SystemSettings>()
                .HasIndex(s => s.SettingKey)
                .IsUnique();

            // Default values ve seed data burada olacak
        }
    }
}