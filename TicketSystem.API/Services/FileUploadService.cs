using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;

namespace TicketSystem.API.Services
{
    public interface IFileUploadService
    {
        Task<string> UploadFileAsync(IFormFile file, string folderName = "uploads");
        Task<bool> DeleteFileAsync(string filePath);
        Task<byte[]> GetFileAsync(string filePath);
        bool IsValidFileType(IFormFile file, string[] allowedExtensions = null);
        bool IsValidFileSize(IFormFile file, long maxSizeInBytes = 10 * 1024 * 1024); // 10MB default
        string GenerateUniqueFileName(string originalFileName);
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;

        // Güvenli dosya türleri
        private readonly string[] _defaultAllowedExtensions = new[]
        {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", // Resimler
            ".pdf", ".doc", ".docx", ".txt", ".rtf", // Belgeler
            ".xlsx", ".xls", ".csv", // Excel
            ".zip", ".rar", ".7z" // Arşivler
        };

        public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Dosya yükleme işlemi
        /// </summary>
        public async Task<string> UploadFileAsync(IFormFile file, string folderName = "uploads")
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("Dosya seçilmedi veya boş!");

                // Güvenlik kontrolleri
                if (!IsValidFileType(file))
                    throw new ArgumentException("Bu dosya türü desteklenmiyor!");

                if (!IsValidFileSize(file))
                    throw new ArgumentException("Dosya boyutu 10MB'dan büyük olamaz!");

                // Upload klasörünü oluştur
                var uploadPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath,
                                            "wwwroot", folderName);

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                // Unique dosya adı oluştur
                var uniqueFileName = GenerateUniqueFileName(file.FileName);
                var filePath = Path.Combine(uploadPath, uniqueFileName);

                // Dosyayı kaydet
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation($"Dosya başarıyla yüklendi: {uniqueFileName}");

                // Relatif path döndür (database için)
                return Path.Combine(folderName, uniqueFileName).Replace("\\", "/");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dosya yükleme hatası: {FileName}", file?.FileName);
                throw;
            }
        }

        /// <summary>
        /// Dosya silme işlemi
        /// </summary>
        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return false;

                var fullPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath,
                                          "wwwroot", filePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation($"Dosya silindi: {filePath}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dosya silme hatası: {FilePath}", filePath);
                return false;
            }
        }

        /// <summary>
        /// Dosya okuma işlemi
        /// </summary>
        public async Task<byte[]> GetFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath,
                                          "wwwroot", filePath);

                if (!File.Exists(fullPath))
                    throw new FileNotFoundException("Dosya bulunamadı!");

                return await File.ReadAllBytesAsync(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dosya okuma hatası: {FilePath}", filePath);
                throw;
            }
        }

        /// <summary>
        /// Dosya türü kontrolü
        /// </summary>
        public bool IsValidFileType(IFormFile file, string[] allowedExtensions = null)
        {
            if (file == null) return false;

            var extensions = allowedExtensions ?? _defaultAllowedExtensions;
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            return extensions.Contains(fileExtension);
        }

        /// <summary>
        /// Dosya boyutu kontrolü
        /// </summary>
        public bool IsValidFileSize(IFormFile file, long maxSizeInBytes = 10 * 1024 * 1024)
        {
            if (file == null) return false;
            return file.Length <= maxSizeInBytes;
        }

        /// <summary>
        /// Unique dosya adı oluşturucu
        /// </summary>
        public string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);

            // Türkçe karakterleri temizle
            nameWithoutExtension = CleanFileName(nameWithoutExtension);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var randomSuffix = Guid.NewGuid().ToString("N")[..8]; // İlk 8 karakter

            return $"{nameWithoutExtension}_{timestamp}_{randomSuffix}{extension}";
        }

        /// <summary>
        /// Dosya adını temizle (Türkçe karakter vs)
        /// </summary>
        private string CleanFileName(string fileName)
        {
            // Türkçe karakterleri değiştir
            var turkishChars = "çğıöşüÇĞIÖŞÜ";
            var englishChars = "cgiosuCGIOSU";

            var cleanName = new StringBuilder(fileName);
            for (int i = 0; i < turkishChars.Length; i++)
            {
                cleanName.Replace(turkishChars[i], englishChars[i]);
            }

            // Özel karakterleri temizle
            var invalidChars = Path.GetInvalidFileNameChars().Concat(new[] { ' ', '-', '(', ')', '[', ']' });
            foreach (var c in invalidChars)
            {
                cleanName.Replace(c, '_');
            }

            return cleanName.ToString();
        }
    }
}