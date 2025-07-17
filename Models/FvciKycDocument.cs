namespace ReverseProxyDemo.Models
{
    public class FvciKycDocument
    {
        public string FvciApplicationId { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentIdentifier { get; set; } = string.Empty;
        public string DocumentPath { get; set; } = string.Empty;
        public IFormFile file { get; set; }
    }

    public class FileUploadRequestDto
    {
        public required IFormFile file { get; set; }

        public required string applicationId { get; set; }

        public required string docType { get; set; }
        public string documentIdentifier { get; set; } = string.Empty;
    }


    public class DeleteFileRequest
    {
        public required string ApplicationId { get; set; }
        public required string FilePath { get; set; }
        public required string docType { get; set; }
    }

}
