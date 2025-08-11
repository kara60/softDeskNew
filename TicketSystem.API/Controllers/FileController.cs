using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketSystem.API.Services;

namespace TicketSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Sadece giriş yapmış kullanıcılar
    public class FileController : ControllerBase
    {
        private readonly IFileUploadService _fileService;
        private readonly ILogger<FileController> _logger;

        public FileController(IFileUploadService fileService, ILogger<FileController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Dosya yükleme endpoint'i
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string? folder = "uploads")
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("Dosya seçilmedi!");

                // Dosya validasyonu
                if (!_fileService.IsValidFileType(file))
                    return BadRequest("Bu dosya türü desteklenmiyor! Desteklenen türler: jpg, jpeg, png, gif, pdf, doc, docx, txt, xlsx, zip");

                if (!_fileService.IsValidFileSize(file))
                    return BadRequest("Dosya boyutu 10MB'dan büyük olamaz!");

                // Dosyayı yükle
                var filePath = await _fileService.UploadFileAsync(file, folder);

                _logger.LogInformation("Dosya yüklendi: {FileName} -> {FilePath}", file.FileName, filePath);

                return Ok(new
                {
                    message = "Dosya başarıyla yüklendi!",
                    fileName = file.FileName,
                    filePath = filePath,
                    fileSize = file.Length,
                    uploadTime = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dosya yükleme hatası: {FileName}", file?.FileName);
                return StatusCode(500, "Dosya yükleme sırasında hata oluştu!");
            }
        }

        /// <summary>
        /// Dosya indirme endpoint'i
        /// </summary>
        [HttpGet("download/{*filePath}")]
        public async Task<IActionResult> DownloadFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return BadRequest("Dosya yolu belirtilmedi!");

                var fileBytes = await _fileService.GetFileAsync(filePath);
                var fileName = Path.GetFileName(filePath);
                var contentType = GetContentType(fileName);

                return File(fileBytes, contentType, fileName);
            }
            catch (FileNotFoundException)
            {
                return NotFound("Dosya bulunamadı!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dosya indirme hatası: {FilePath}", filePath);
                return StatusCode(500, "Dosya indirme sırasında hata oluştu!");
            }
        }

        /// <summary>
        /// Dosya silme endpoint'i
        /// </summary>
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteFile([FromQuery] string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return BadRequest("Dosya yolu belirtilmedi!");

                var deleted = await _fileService.DeleteFileAsync(filePath);

                if (deleted)
                {
                    _logger.LogInformation("Dosya silindi: {FilePath}", filePath);
                    return Ok(new { message = "Dosya başarıyla silindi!" });
                }
                else
                {
                    return NotFound("Dosya bulunamadı!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dosya silme hatası: {FilePath}", filePath);
                return StatusCode(500, "Dosya silme sırasında hata oluştu!");
            }
        }

        /// <summary>
        /// Dosya bilgileri endpoint'i
        /// </summary>
        [HttpGet("info/{*filePath}")]
        public async Task<IActionResult> GetFileInfo(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return BadRequest("Dosya yolu belirtilmedi!");

                var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var fullPath = Path.Combine(webRootPath, filePath);

                if (!System.IO.File.Exists(fullPath))
                    return NotFound("Dosya bulunamadı!");

                var fileInfo = new FileInfo(fullPath);

                return Ok(new
                {
                    fileName = fileInfo.Name,
                    filePath = filePath,
                    fileSize = fileInfo.Length,
                    fileSizeFormatted = FormatFileSize(fileInfo.Length),
                    createdDate = fileInfo.CreationTime,
                    lastModified = fileInfo.LastWriteTime,
                    extension = fileInfo.Extension,
                    contentType = GetContentType(fileInfo.Name)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dosya bilgisi alma hatası: {FilePath}", filePath);
                return StatusCode(500, "Dosya bilgisi alınırken hata oluştu!");
            }
        }

        /// <summary>
        /// Yüklenen dosyaları listeleme endpoint'i
        /// </summary>
        [HttpGet("list")]
        public IActionResult ListFiles([FromQuery] string folder = "uploads")
        {
            try
            {
                var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadPath = Path.Combine(webRootPath, folder);

                if (!Directory.Exists(uploadPath))
                    return Ok(new { files = new List<object>(), message = "Klasör bulunamadı!" });

                var files = Directory.GetFiles(uploadPath)
                    .Select(filePath =>
                    {
                        var fileInfo = new FileInfo(filePath);
                        var relativePath = Path.GetRelativePath(webRootPath, filePath).Replace("\\", "/");

                        return new
                        {
                            fileName = fileInfo.Name,
                            filePath = relativePath,
                            fileSize = fileInfo.Length,
                            fileSizeFormatted = FormatFileSize(fileInfo.Length),
                            createdDate = fileInfo.CreationTime,
                            extension = fileInfo.Extension,
                            contentType = GetContentType(fileInfo.Name)
                        };
                    })
                    .OrderByDescending(f => f.createdDate)
                    .ToList();

                return Ok(new { files = files, totalCount = files.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dosya listeleme hatası: {Folder}", folder);
                return StatusCode(500, "Dosya listeleme sırasında hata oluştu!");
            }
        }

        /// <summary>
        /// Dosya türüne göre Content-Type belirleme
        /// </summary>
        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".xls" => "application/vnd.ms-excel",
                ".txt" => "text/plain",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                ".7z" => "application/x-7z-compressed",
                _ => "application/octet-stream"
            };
        }

        /// <summary>
        /// Dosya boyutunu human-readable formata çevirme
        /// </summary>
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}