namespace NSDL.Middleware.Interfaces
{
    public interface IEmailHelper
    {
        Task<bool> SendOtpEmailAsync(string email, string otp, string purpose);
        Task<bool> SendEncryptedPdfEmailAsync(string base64Data,string email,string purpose);
    }
}
