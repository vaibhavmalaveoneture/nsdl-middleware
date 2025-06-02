namespace ReverseProxyDemo.Models
{
    public class FileUploadData
    {
        public required IFormFile file { get; set; }

        public required string applicationId { get; set; }

        public required string docType { get; set; }

        public string documentIdentifier { get; set; } = string.Empty;

        public string filePath { get; set;} = string.Empty;
    }
}
