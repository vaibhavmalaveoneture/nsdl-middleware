using ReverseProxyDemo.Interfaces;
using ReverseProxyDemo.Models;
using System.Net;

namespace ReverseProxyDemo.Helper
{
    public class FileHelper : IFileHelper
    {
        public string fileVlaidation(FileUploadData request)
        {
            try
            {
                if (request.file == null || request.file.Length == 0)
                {
                    return "No file uploaded.";
                }
                if (
                        string.IsNullOrWhiteSpace(request.docType)
                        || !(
                            request.docType.Equals("POA", StringComparison.OrdinalIgnoreCase)
                            || request.docType.Equals("POI", StringComparison.OrdinalIgnoreCase)
                            || request.docType.Equals("additional", StringComparison.OrdinalIgnoreCase)
                            || request.docType.Equals("formUpload", StringComparison.OrdinalIgnoreCase)
                            || request.docType.Equals("annexureUpload", StringComparison.OrdinalIgnoreCase)
                            || request.docType.Equals("disciplinaryHistory", StringComparison.OrdinalIgnoreCase)
                        )
                    )
                {
                    return "Invalid document category. Allowed values: POA, POI,formUpload,annexureUpload,disciplinaryHistory or additional.";

                }
                if (
                        request.docType.Equals("additional", StringComparison.OrdinalIgnoreCase)
                        && string.IsNullOrWhiteSpace(request.documentIdentifier)
                    )
                {
                    return "Document identifier is required for additional documents.";
                }

                var fileExtension = Path.GetExtension(request.file.FileName)?.ToLowerInvariant();
                if (fileExtension != ".pdf")
                {
                    return "Invalid file type. Only PDF files are allowed.";
                }

                if (
                       !string.Equals(
                           request.file.ContentType,
                           "application/pdf",
                           StringComparison.OrdinalIgnoreCase
                       )
                   )
                {
                    return "Invalid file content type. Only PDF files are allowed.";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "success";
        }

        public async Task<FileUploadData> fileCreation(FileUploadData request,string baseUploadsFolder)
        {
            try
            {
                var safeFileName = Path.GetFileName(request.file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}";


                var categoryFolder = request.docType.ToLowerInvariant();
                var uploadsFolder = Path.Combine(baseUploadsFolder ?? string.Empty, request.applicationId, categoryFolder);
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);


                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.file.CopyToAsync(stream);
                }
                request.filePath = filePath;

                return request;
            }
            catch(Exception ex) 
            {
                throw;
            }
        }
    }
}
