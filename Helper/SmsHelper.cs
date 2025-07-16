using NSDL.Middleware.Helpers;
using ReverseProxyDemo.Interfaces;
using System.Net;
using System.Net.Mail;

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
            if (string.Equals(message, "REGISTRATION", StringComparison.OrdinalIgnoreCase))
                return true;
            string url = string.Empty, ProxyAddress = string.Empty, UseProxy = string.Empty, ProxyUserName=string.Empty, ProxyPassword = string.Empty, ProxyDomain=string.Empty;
            try
            {
                url = _configuration["SmsSetting:endpoint"]; // Replace with your URL
                ProxyAddress = _configuration["SmsSetting:ProxyIP"] + ":" + _configuration["SmsSetting:ProxyPort"];
                UseProxy = _configuration["SmsSetting:UseProxy"];
                ProxyUserName = _configuration["SmsSetting:ProxyUserName"];
                ProxyPassword = _configuration["SmsSetting:ProxyPassword"];
                ProxyDomain = _configuration["SmsSetting:ProxyDomain"];
                if (string.IsNullOrEmpty(url))
                    throw new Exception("no sms endpoint found");
                url = url.Replace("@Phoneno", phoneno);
                url = url.Replace("@otp", otp);
                var handler = new HttpClientHandler();
                if (UseProxy=="Y")
                {
                    handler.Proxy = new WebProxy(ProxyAddress)
                    {
                        Credentials = new NetworkCredential(ProxyUserName, ProxyPassword,ProxyDomain)
                    };
                    handler.UseProxy = true;
                }
                else
                {
                    handler.UseProxy = false;
                }
                using (HttpClient client = new HttpClient(handler))
                {
                    _logger.LogInformation("sms send successfully to : {url} at {timestamp}", url, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode(); // Throws exception for 4xx/5xx
                    string responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("sms sent successfully at {Timestamp} to request: {ToAddress} response:  {response}",
                          DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                          url,
                          responseBody
                      );
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
