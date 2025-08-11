using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Text;
using TicketSystem.API.Data;
using TicketSystem.API.Models;

namespace TicketSystem.API.Services
{
    public interface IEmailService
    {
        Task<bool> SendTicketCreatedEmailAsync(Ticket ticket);
        Task<bool> SendTicketStatusChangedEmailAsync(Ticket ticket, string oldStatus, string newStatus);
        Task<bool> SendTicketCommentEmailAsync(Ticket ticket, TicketComment comment);
        Task<bool> SendUserCreatedEmailAsync(User user, string temporaryPassword);
        Task<bool> SendPasswordResetEmailAsync(User user, string resetToken);
        Task<bool> SendTestEmailAsync(string toEmail, string subject, string body);
        Task<bool> SendCustomEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
    }

    public class EmailService : IEmailService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EmailService> _logger;

        public EmailService(AppDbContext context, ILogger<EmailService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> SendTicketCreatedEmailAsync(Ticket ticket)
        {
            try
            {
                // Ticket bilgilerini yükle
                var ticketWithData = await _context.Tickets
                    .Include(t => t.Company)
                    .Include(t => t.CreatedByUser)
                    .Include(t => t.TicketType)
                    .FirstOrDefaultAsync(t => t.Id == ticket.Id);

                if (ticketWithData == null) return false;

                // Email ayarlarını kontrol et
                if (!await IsEmailEnabledAsync("NOTIFY_ON_TICKET_CREATE")) return true;

                // Email içeriği oluştur
                var subject = $"Yeni Ticket Oluşturuldu - {ticketWithData.TicketNumber}";
                var body = await BuildTicketCreatedEmailBodyAsync(ticketWithData);

                // Support ekibine gönder
                var supportEmails = await GetSupportEmailsAsync();
                var emailSent = false;

                foreach (var email in supportEmails)
                {
                    if (await SendEmailAsync(email, subject, body))
                    {
                        emailSent = true;
                    }
                }

                _logger.LogInformation($"Ticket oluşturma emaili gönderildi: {ticketWithData.TicketNumber}");
                return emailSent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ticket oluşturma emaili gönderilirken hata: {ticket.TicketNumber}");
                return false;
            }
        }

        public async Task<bool> SendTicketStatusChangedEmailAsync(Ticket ticket, string oldStatus, string newStatus)
        {
            try
            {
                // Email ayarlarını kontrol et
                if (!await IsEmailEnabledAsync("NOTIFY_ON_STATUS_CHANGE")) return true;

                // Ticket bilgilerini yükle
                var ticketWithData = await _context.Tickets
                    .Include(t => t.Company)
                    .Include(t => t.CreatedByUser)
                    .Include(t => t.AssignedToUser)
                    .Include(t => t.TicketType)
                    .FirstOrDefaultAsync(t => t.Id == ticket.Id);

                if (ticketWithData == null) return false;

                // Email içeriği oluştur
                var subject = $"Ticket Durumu Değişti - {ticketWithData.TicketNumber}";
                var body = await BuildTicketStatusChangedEmailBodyAsync(ticketWithData, oldStatus, newStatus);

                // Ticket sahibine gönder
                var emailSent = await SendEmailAsync(ticketWithData.CreatedByUser.Email, subject, body);

                // Atanan kişiye de gönder (farklıysa)
                if (ticketWithData.AssignedToUser != null &&
                    ticketWithData.AssignedToUser.Email != ticketWithData.CreatedByUser.Email)
                {
                    await SendEmailAsync(ticketWithData.AssignedToUser.Email, subject, body);
                }

                _logger.LogInformation($"Ticket durum değişikliği emaili gönderildi: {ticketWithData.TicketNumber}");
                return emailSent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ticket durum değişikliği emaili gönderilirken hata: {ticket.TicketNumber}");
                return false;
            }
        }

        public async Task<bool> SendTicketCommentEmailAsync(Ticket ticket, TicketComment comment)
        {
            try
            {
                // Email ayarlarını kontrol et
                if (!await IsEmailEnabledAsync("NOTIFY_ON_COMMENT")) return true;

                // Ticket ve yorum bilgilerini yükle
                var ticketWithData = await _context.Tickets
                    .Include(t => t.Company)
                    .Include(t => t.CreatedByUser)
                    .Include(t => t.AssignedToUser)
                    .FirstOrDefaultAsync(t => t.Id == ticket.Id);

                var commentWithUser = await _context.TicketComments
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == comment.Id);

                if (ticketWithData == null || commentWithUser == null) return false;

                // Internal yorumlar sadece support ekibine gönderilir
                if (commentWithUser.IsInternal)
                {
                    var supportEmails = await GetSupportEmailsAsync();
                    var subject = $"Internal Yorum Eklendi - {ticketWithData.TicketNumber}";
                    var body = await BuildTicketCommentEmailBodyAsync(ticketWithData, commentWithUser, true);

                    var emailSent = false;
                    foreach (var email in supportEmails)
                    {
                        if (await SendEmailAsync(email, subject, body))
                        {
                            emailSent = true;
                        }
                    }
                    return emailSent;
                }

                // Normal yorumlar herkese gönderilir
                var normalSubject = $"Yeni Yorum Eklendi - {ticketWithData.TicketNumber}";
                var normalBody = await BuildTicketCommentEmailBodyAsync(ticketWithData, commentWithUser, false);

                // Ticket sahibine gönder (yorum kendisinden değilse)
                var success = true;
                if (commentWithUser.UserId != ticketWithData.CreatedByUserId)
                {
                    success &= await SendEmailAsync(ticketWithData.CreatedByUser.Email, normalSubject, normalBody);
                }

                // Atanan kişiye gönder (farklıysa ve yorum kendisinden değilse)
                if (ticketWithData.AssignedToUser != null &&
                    commentWithUser.UserId != ticketWithData.AssignedToUserId &&
                    ticketWithData.AssignedToUser.Email != ticketWithData.CreatedByUser.Email)
                {
                    success &= await SendEmailAsync(ticketWithData.AssignedToUser.Email, normalSubject, normalBody);
                }

                _logger.LogInformation($"Ticket yorum emaili gönderildi: {ticketWithData.TicketNumber}");
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ticket yorum emaili gönderilirken hata: {ticket.TicketNumber}");
                return false;
            }
        }

        public async Task<bool> SendUserCreatedEmailAsync(User user, string temporaryPassword)
        {
            try
            {
                var userWithCompany = await _context.Users
                    .Include(u => u.Company)
                    .FirstOrDefaultAsync(u => u.Id == user.Id);

                if (userWithCompany == null) return false;

                var subject = "SoftTicket Hesabınız Oluşturuldu";
                var body = await BuildUserCreatedEmailBodyAsync(userWithCompany, temporaryPassword);

                var success = await SendEmailAsync(userWithCompany.Email, subject, body);

                _logger.LogInformation($"Kullanıcı oluşturma emaili gönderildi: {userWithCompany.Email}");
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Kullanıcı oluşturma emaili gönderilirken hata: {user.Email}");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(User user, string resetToken)
        {
            try
            {
                var subject = "Şifre Sıfırlama Talebi";
                var body = await BuildPasswordResetEmailBodyAsync(user, resetToken);

                var success = await SendEmailAsync(user.Email, subject, body);

                _logger.LogInformation($"Şifre sıfırlama emaili gönderildi: {user.Email}");
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Şifre sıfırlama emaili gönderilirken hata: {user.Email}");
                return false;
            }
        }

        public async Task<bool> SendTestEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var testSubject = $"[TEST] {subject}";
                var testBody = $@"
                    <h2>Bu bir test emailidir</h2>
                    <p><strong>Gönderilme Zamanı:</strong> {DateTime.Now:dd.MM.yyyy HH:mm:ss}</p>
                    <hr>
                    <h3>Test İçeriği:</h3>
                    {body}
                ";

                return await SendEmailAsync(toEmail, testSubject, testBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Test emaili gönderilirken hata: {toEmail}");
                return false;
            }
        }

        public async Task<bool> SendCustomEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            return await SendEmailAsync(toEmail, subject, body, isHtml);
        }

        #region Private Methods

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            try
            {
                // Email ayarlarını al
                var emailSettings = await GetEmailSettingsAsync();
                if (!emailSettings.Enabled)
                {
                    _logger.LogWarning("Email sistemi devre dışı");
                    return false;
                }

                using var client = new SmtpClient(emailSettings.SmtpHost, emailSettings.SmtpPort);
                client.Credentials = new NetworkCredential(emailSettings.Username, emailSettings.Password);
                client.EnableSsl = true;

                using var message = new MailMessage();
                message.From = new MailAddress(emailSettings.FromEmail, emailSettings.FromName);
                message.To.Add(toEmail);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isHtml;
                message.BodyEncoding = Encoding.UTF8;
                message.SubjectEncoding = Encoding.UTF8;

                await client.SendMailAsync(message);

                _logger.LogInformation($"Email başarıyla gönderildi: {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Email gönderilirken hata: {toEmail}");
                return false;
            }
        }

        private async Task<EmailSettings> GetEmailSettingsAsync()
        {
            var settings = await _context.SystemSettings
                .Where(s => s.Category == "Email")
                .ToDictionaryAsync(s => s.SettingKey, s => s.SettingValue);

            return new EmailSettings
            {
                Enabled = GetBoolValue(settings, "EMAIL_ENABLED", true),
                SmtpHost = GetStringValue(settings, "SMTP_HOST", "smtp.gmail.com"),
                SmtpPort = GetIntValue(settings, "SMTP_PORT", 587),
                Username = GetStringValue(settings, "SMTP_USERNAME", ""),
                Password = GetStringValue(settings, "SMTP_PASSWORD", ""),
                FromEmail = GetStringValue(settings, "EMAIL_FROM", "noreply@softticket.com"),
                FromName = GetStringValue(settings, "EMAIL_FROM_NAME", "SoftTicket Sistemi")
            };
        }

        private async Task<bool> IsEmailEnabledAsync(string notificationKey)
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == notificationKey);

            return setting != null && bool.TryParse(setting.SettingValue, out bool result) && result;
        }

        private async Task<List<string>> GetSupportEmailsAsync()
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .Where(u => _context.UserRoles
                    .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
                    .Any(x => x.UserId == u.Id && (x.Name == "Admin" || x.Name == "Support")))
                .Select(u => u.Email)
                .ToListAsync();
        }

        private string GetStringValue(Dictionary<string, string> settings, string key, string defaultValue)
        {
            return settings.TryGetValue(key, out string? value) ? value : defaultValue;
        }

        private int GetIntValue(Dictionary<string, string> settings, string key, int defaultValue)
        {
            return settings.TryGetValue(key, out string? value) && int.TryParse(value, out int result) ? result : defaultValue;
        }

        private bool GetBoolValue(Dictionary<string, string> settings, string key, bool defaultValue)
        {
            return settings.TryGetValue(key, out string? value) && bool.TryParse(value, out bool result) ? result : defaultValue;
        }

        #endregion

        #region Email Templates

        private async Task<string> BuildTicketCreatedEmailBodyAsync(Ticket ticket)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #6366f1; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9fafb; }}
        .ticket-info {{ background-color: white; padding: 15px; border-left: 4px solid #6366f1; margin: 15px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
        .button {{ background-color: #6366f1; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Yeni Ticket Oluşturuldu</h2>
        </div>
        <div class='content'>
            <p>Merhaba,</p>
            <p>Yeni bir destek talebi oluşturuldu:</p>
            
            <div class='ticket-info'>
                <h3>Ticket Bilgileri</h3>
                <p><strong>Ticket No:</strong> {ticket.TicketNumber}</p>
                <p><strong>Başlık:</strong> {ticket.Title}</p>
                <p><strong>Tür:</strong> {ticket.TicketType?.Name ?? "Belirtilmemiş"}</p>
                <p><strong>Öncelik:</strong> {ticket.Priority}</p>
                <p><strong>Durum:</strong> {ticket.Status}</p>
                <p><strong>Oluşturan:</strong> {ticket.CreatedByUser.FirstName} {ticket.CreatedByUser.LastName} ({ticket.CreatedByUser.Email})</p>
                <p><strong>Şirket:</strong> {ticket.Company.Name}</p>
                <p><strong>Oluşturulma Zamanı:</strong> {ticket.CreatedAt:dd.MM.yyyy HH:mm}</p>
            </div>
            
            <div class='ticket-info'>
                <h4>Açıklama:</h4>
                <p>{ticket.Description}</p>
            </div>
            
            <p style='text-align: center; margin-top: 30px;'>
                <a href='#' class='button'>Ticket'ı Görüntüle</a>
            </p>
        </div>
        <div class='footer'>
            <p>Bu email SoftTicket sistemi tarafından otomatik olarak gönderilmiştir.</p>
        </div>
    </div>
</body>
</html>";
        }

        private async Task<string> BuildTicketStatusChangedEmailBodyAsync(Ticket ticket, string oldStatus, string newStatus)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #059669; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9fafb; }}
        .status-change {{ background-color: white; padding: 15px; border-left: 4px solid #059669; margin: 15px 0; text-align: center; }}
        .old-status {{ color: #dc2626; }}
        .new-status {{ color: #059669; font-weight: bold; }}
        .ticket-info {{ background-color: white; padding: 15px; margin: 15px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Ticket Durumu Değişti</h2>
        </div>
        <div class='content'>
            <p>Merhaba,</p>
            <p>Ticket'ınızın durumu güncellenmiştir:</p>
            
            <div class='status-change'>
                <h3>Durum Değişikliği</h3>
                <p class='old-status'>Eski Durum: {oldStatus}</p>
                <p>⬇️</p>
                <p class='new-status'>Yeni Durum: {newStatus}</p>
            </div>
            
            <div class='ticket-info'>
                <h3>Ticket Bilgileri</h3>
                <p><strong>Ticket No:</strong> {ticket.TicketNumber}</p>
                <p><strong>Başlık:</strong> {ticket.Title}</p>
                <p><strong>Güncelleme Zamanı:</strong> {DateTime.Now:dd.MM.yyyy HH:mm}</p>
            </div>
        </div>
        <div class='footer'>
            <p>Bu email SoftTicket sistemi tarafından otomatik olarak gönderilmiştir.</p>
        </div>
    </div>
</body>
</html>";
        }

        private async Task<string> BuildTicketCommentEmailBodyAsync(Ticket ticket, TicketComment comment, bool isInternal)
        {
            var headerColor = isInternal ? "#f59e0b" : "#8b5cf6";
            var title = isInternal ? "Internal Yorum Eklendi" : "Yeni Yorum Eklendi";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: {headerColor}; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9fafb; }}
        .comment {{ background-color: white; padding: 15px; border-left: 4px solid {headerColor}; margin: 15px 0; }}
        .ticket-info {{ background-color: white; padding: 15px; margin: 15px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
        .internal-badge {{ background-color: #fbbf24; color: #92400e; padding: 2px 8px; border-radius: 12px; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>{title}</h2>
        </div>
        <div class='content'>
            <p>Merhaba,</p>
            <p>Ticket'a yeni bir yorum eklenmiştir:</p>
            
            <div class='comment'>
                <h3>Yorum {(isInternal ? "<span class='internal-badge'>INTERNAL</span>" : "")}</h3>
                <p><strong>Yazan:</strong> {comment.User.FirstName} {comment.User.LastName}</p>
                <p><strong>Tarih:</strong> {comment.CreatedAt:dd.MM.yyyy HH:mm}</p>
                <hr>
                <p>{comment.Comment}</p>
            </div>
            
            <div class='ticket-info'>
                <h3>Ticket Bilgileri</h3>
                <p><strong>Ticket No:</strong> {ticket.TicketNumber}</p>
                <p><strong>Başlık:</strong> {ticket.Title}</p>
                <p><strong>Durum:</strong> {ticket.Status}</p>
            </div>
        </div>
        <div class='footer'>
            <p>Bu email SoftTicket sistemi tarafından otomatik olarak gönderilmiştir.</p>
        </div>
    </div>
</body>
</html>";
        }

        private async Task<string> BuildUserCreatedEmailBodyAsync(User user, string temporaryPassword)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #10b981; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9fafb; }}
        .credentials {{ background-color: #fef3c7; padding: 15px; border-left: 4px solid #f59e0b; margin: 15px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
        .button {{ background-color: #10b981; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block; }}
        .warning {{ background-color: #fee2e2; padding: 10px; border-left: 4px solid #dc2626; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>SoftTicket Hesabınız Oluşturuldu</h2>
        </div>
        <div class='content'>
            <p>Merhaba {user.FirstName} {user.LastName},</p>
            <p>{user.Company?.Name ?? "Sistemimizdeki"} için SoftTicket hesabınız başarıyla oluşturulmuştur.</p>
            
            <div class='credentials'>
                <h3>Giriş Bilgileriniz</h3>
                <p><strong>Email:</strong> {user.Email}</p>
                <p><strong>Geçici Şifre:</strong> {temporaryPassword}</p>
            </div>
            
            <div class='warning'>
                <p><strong>⚠️ Önemli:</strong> Güvenlik nedeniyle ilk girişinizden sonra şifrenizi değiştirmenizi önemle tavsiye ederiz.</p>
            </div>
            
            <p style='text-align: center; margin-top: 30px;'>
                <a href='#' class='button'>Sisteme Giriş Yap</a>
            </p>
            
            <p>Herhangi bir sorunuz olursa lütfen destek ekibimiz ile iletişime geçin.</p>
            
            <p>İyi çalışmalar dileriz!</p>
        </div>
        <div class='footer'>
            <p>SoftTicket Ekibi<br>
            Bu email SoftTicket sistemi tarafından otomatik olarak gönderilmiştir.</p>
        </div>
    </div>
</body>
</html>";
        }

        private async Task<string> BuildPasswordResetEmailBodyAsync(User user, string resetToken)
        {
            var resetLink = $"https://softticket.com/reset-password?token={resetToken}";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc2626; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9fafb; }}
        .reset-info {{ background-color: white; padding: 15px; border-left: 4px solid #dc2626; margin: 15px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
        .button {{ background-color: #dc2626; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block; }}
        .warning {{ background-color: #fef3c7; padding: 10px; border-left: 4px solid #f59e0b; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Şifre Sıfırlama Talebi</h2>
        </div>
        <div class='content'>
            <p>Merhaba {user.FirstName} {user.LastName},</p>
            <p>SoftTicket hesabınız için şifre sıfırlama talebinde bulundunuz.</p>
            
            <div class='reset-info'>
                <h3>Şifre Sıfırlama</h3>
                <p>Şifrenizi sıfırlamak için aşağıdaki linke tıklayın:</p>
                <p style='text-align: center; margin: 20px 0;'>
                    <a href='{resetLink}' class='button'>Şifremi Sıfırla</a>
                </p>
                <p>Alternatif olarak şu linki tarayıcınıza kopyalayabilirsiniz:</p>
                <p style='word-break: break-all; background-color: #f3f4f6; padding: 10px; font-family: monospace;'>{resetLink}</p>
            </div>
            
            <div class='warning'>
                <p><strong>⚠️ Güvenlik Uyarısı:</strong></p>
                <ul>
                    <li>Bu link 1 saat içinde geçerliliğini yitirecektir</li>
                    <li>Eğer bu talebi siz yapmadıysanız, bu emaili görmezden gelin</li>
                    <li>Linki kimseyle paylaşmayın</li>
                </ul>
            </div>
            
            <p>Sorularınız için destek ekibimiz ile iletişime geçebilirsiniz.</p>
        </div>
        <div class='footer'>
            <p>SoftTicket Ekibi<br>
            Bu email SoftTicket sistemi tarafından otomatik olarak gönderilmiştir.</p>
        </div>
    </div>
</body>
</html>";
        }

        #endregion
    }

    // Email ayarları helper class
    public class EmailSettings
    {
        public bool Enabled { get; set; }
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }
}