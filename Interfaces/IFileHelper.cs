using ReverseProxyDemo.Models;

namespace ReverseProxyDemo.Interfaces
{
    public interface IFileHelper
    {
        Task<bool> createFile(FvciKycDocument fvciKycDocument);

        Task<bool> deleteFile(DeleteFileRequest deleteFileRequest);

        Task<IResult> viewFile(DeleteFileRequest downloadFileRequest);

        FileUploadRequestDto ToUploadFileRequest(IFormCollection form);
    }
}
