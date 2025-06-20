using ReverseProxyDemo.Interfaces;
using ReverseProxyDemo.Models;

namespace ReverseProxyDemo.Helper
{
    public class FileHelper : IFileHelper
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
            var filePath="";
            try
            {
                var uniqueFileDetails = fvciKycDocument.DocumentPath;

                string fileName = Path.GetFileName(uniqueFileDetails);

                // Get the directory path only
                string directoryPath = Path.GetDirectoryName(uniqueFileDetails);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                filePath = Path.Combine(directoryPath, fileName);


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
    }
}
