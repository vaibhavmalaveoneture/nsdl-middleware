using NSDL.Middleware.Helpers;
using ReverseProxyDemo.Interfaces;
using System.Net;

namespace ReverseProxyDemo.Helper
{
    public class SmsHelper:ISmsHelper
    {
        private readonly ILogger<EmailHelper> _logger;
        private readonly IConfiguration _configuration;
        private Exception exception;
        public SmsHelper(ILogger<EmailHelper> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> SendOtpSmsAsync(string phoneno, string otp, string message)
        {
            string url = string.Empty;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    url = _configuration["SmsSetting:endpoint"]; // Replace with your URL
                    if (string.IsNullOrEmpty(url))
                        throw new Exception("no sms endpoint found");
                    url=url.Replace("@Phoneno", phoneno);
                    url=url.Replace("@otp", otp);
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode(); // Throws exception for 4xx/5xx
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
            finally
            {
                if (exception != null)
                    _logger.LogError(exception, "An error occurred while sending OTP sms to: {url}", url);
                else
                    _logger.LogInformation("sms send successfully to : {url}",url);
            }
        }
    }
}
