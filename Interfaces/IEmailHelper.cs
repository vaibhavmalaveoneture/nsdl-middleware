namespace NSDL.Middleware.Interfaces
{
    public interface IEmailHelper
    {
        Task<bool> SendOtpEmailAsync(string email, string otp, string purpose);
    }
}
