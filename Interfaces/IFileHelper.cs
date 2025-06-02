using ReverseProxyDemo.Models;

namespace ReverseProxyDemo.Interfaces
{
    public interface IFileHelper
    {
        public string fileVlaidation(FileUploadData request);
    
        public Task<FileUploadData> fileCreation(FileUploadData request, string baseUploadsFolder);

    }
}
