using System.ComponentModel.DataAnnotations;

namespace TicketSystem.API.DTOs
{
    // Kullanıcı listesi için (Admin paneli)
    public class UserListDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
    }

    // Kullanıcı detayı için
    public class UserDetailDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public Guid? CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
    }

    // Admin'in yeni kullanıcı oluşturması için
    public class CreateUserDto
    {
        [Required(ErrorMessage = "Ad gereklidir.")]
        [MaxLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad gereklidir.")]
        [MaxLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email adresi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre gereklidir.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şirket seçimi gereklidir.")]
        public Guid CompanyId { get; set; }

        public List<string> Roles { get; set; } = new List<string> { "Customer" }; // Default Customer

        public bool IsActive { get; set; } = true; // Default aktif
    }

    // Kullanıcı güncelleme için
    public class UpdateUserDto
    {
        [Required(ErrorMessage = "Ad gereklidir.")]
        [MaxLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad gereklidir.")]
        [MaxLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email adresi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        public Guid? CompanyId { get; set; }

        public List<string> Roles { get; set; } = new List<string>();

        public bool IsActive { get; set; } = true;
    }

    // Admin'in şifre değiştirmesi için
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Yeni şifre gereklidir.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$",
            ErrorMessage = "Şifre en az 1 büyük harf, 1 küçük harf ve 1 rakam içermelidir.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrarı gereklidir.")]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    // Basit kullanıcı bilgisi (dropdown'lar için)
    public class SimpleUserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty; // FirstName + LastName
        public string Email { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
    }

    // Rol bilgisi
    public class RoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}