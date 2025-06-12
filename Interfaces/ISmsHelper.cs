namespace ReverseProxyDemo.Interfaces
{
    public interface ISmsHelper
    {
        Task<bool> SendOtpSmsAsync(string phoneno, string otp, string message);
    }
}
