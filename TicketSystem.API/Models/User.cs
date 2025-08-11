using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace TicketSystem.API.Models
{
    public class User : IdentityUser<Guid>
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        public Guid? CompanyId { get; set; }
        public Company? Company { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Ticket> CreatedTickets { get; set; } = new List<Ticket>();
        public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();
    }
}