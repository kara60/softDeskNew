using System.ComponentModel.DataAnnotations;

namespace TicketSystem.API.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email adresi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre gereklidir.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
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
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$",
            ErrorMessage = "Şifre en az 1 büyük harf, 1 küçük harf ve 1 rakam içermelidir.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrarı gereklidir.")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public Guid? CompanyId { get; set; } // Admin tarafından atanacak
    }
}