using ReverseProxyDemo.Models;

namespace ReverseProxyDemo.Interfaces
{
    public interface IFileHelper
    {
        Task<bool> createFile(FvciKycDocument fvciKycDocument);
    }
}
