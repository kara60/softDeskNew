using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

namespace TicketSystem.API.Models
{
    public class Company
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string DatabaseName { get; set; } = string.Empty; // Her müşteri ayrı DB

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? ContactPerson { get; set; }

        // Kontör sistemi için (Faz 2)
        public int TicketCredits { get; set; } = 0;

        [MaxLength(20)]
        public string PlanType { get; set; } = "FREE"; // FREE, BASIC, PREMIUM

        public int MonthlyTicketLimit { get; set; } = 10;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}