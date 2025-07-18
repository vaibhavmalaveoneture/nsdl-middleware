using ReverseProxyDemo.Interfaces;
using ReverseProxyDemo.Models;
using System.Text;
using System.Text.Json;

namespace ReverseProxyDemo.Helper
{
    public class FileHelper:IFileHelper
    {
        private readonly ILogger<FileHelper> _logger;
        private readonly IConfiguration _configuration;
        private Exception exception;
        public FileHelper(ILogger<FileHelper> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> createFile(FvciKycDocument fvciKycDocument)
        {
            var filePath = "";
            try
            {
                var categoryFolder = fvciKycDocument.DocumentType.ToLowerInvariant();
                var uploadsFolder = Path.Combine(_configuration["DocumentPathNew"] ?? string.Empty, fvciKycDocument.FvciApplicationId, categoryFolder);

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                filePath = Path.Combine(uploadsFolder, fvciKycDocument.DocumentPath);


                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await fvciKycDocument.file.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating file {filePath}", filePath);
            }
            return true;
        }

        public async Task<bool> deleteFile(DeleteFileRequest deleteFileRequest)
        {
            try
            {
                deleteFileRequest.FilePath = Path.Combine(_configuration["DocumentPathNew"] ?? string.Empty, deleteFileRequest.ApplicationId, deleteFileRequest.docType, deleteFileRequest.FilePath);
                if (System.IO.File.Exists(deleteFileRequest.FilePath))
                {
                    System.IO.File.Delete(deleteFileRequest.FilePath);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete file: {ex.Message}");
                return false;
            }
        }

        public async Task<IResult> viewFile(DeleteFileRequest downloadFileRequest)
        {
            var fullPath = Path.Combine(
                _configuration["DocumentPathNew"] ?? string.Empty,
                downloadFileRequest.ApplicationId,
                downloadFileRequest.docType,
                downloadFileRequest.FilePath
            );
            if (string.IsNullOrEmpty(fullPath) || !System.IO.File.Exists(fullPath))
            {
                return Results.NotFound("File not found.");
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                await stream.CopyToAsync(memory);
            }

            memory.Position = 0;
            var contentType = "application/pdf";
            var fileName = Path.GetFileName(fullPath);

            return Results.File(memory, contentType, fileName);
        }

        public FileUploadRequestDto ToUploadFileRequest(IFormCollection form)
        {
            var model = new FileUploadRequestDto
            {
                applicationId = form["applicationId"],
                docType = form["docType"],
                documentIdentifier = form["documentIdentifier"],
                file = form.Files.FirstOrDefault()
            };

            return model;
        }

    }
}
