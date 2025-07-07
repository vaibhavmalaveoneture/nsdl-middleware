using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using NSDL.Middleware.Interfaces;

namespace NSDL.Middleware.Helpers
{
    public class EmailHelper : IEmailHelper
    {
        private readonly ILogger<EmailHelper> _logger;
        private readonly IConfiguration _configuration;

        public EmailHelper(ILogger<EmailHelper> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> SendOtpEmailAsync(string email, string otp, string purpose)
        {
            try
            {
                bool useGupShup = Convert.ToBoolean(_configuration["EmailSettingsGupShup:UseGupShup"]);


                var fromEmail = _configuration[useGupShup ? "EmailSettingsGupShup:FromEmail" : "EmailSettings:FromEmail"];
                var fromPassword = _configuration["EmailSettings:FromEmailPassword"];
                var smtpServer = _configuration[useGupShup ? "EmailSettingsGupShup:HostName" : "EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]!);
                var mailUserID = useGupShup ? _configuration["EmailSettingsGupShup:MailUserID"] : fromEmail;
                var mailPass = useGupShup ? _configuration["EmailSettingsGupShup:MailPass"] : fromPassword;

                var smtpClient = new SmtpClient(smtpServer)
                {
                    Port = smtpPort,
                    Credentials = new NetworkCredential(mailUserID, mailPass),
                    EnableSsl = false,
                    UseDefaultCredentials = false,
                };

                /*string subject = purpose switch
                {
                    "FORGOT_PASSWORD" => "Your OTP for Password Reset | NSDL",
                    "LOGIN" => "Your OTP for Login | NSDL",
                    _ => "Your OTP for User Registration | NSDL",
                };*/

                /*string message = purpose switch
                {
                    "FORGOT_PASSWORD" => $@"
                <p>We received a request to reset your password. Please use the OTP below to proceed:</p>
                <p class='otp-text'>{otp}</p>
                <p>This OTP will expire in 5 minutes. If you did not request this, please ignore this email.</p>",
                "LOGIN" => $@"
                <p>Please use the OTP below to proceed with login:</p>
                <p class='otp-text'>{otp}</p>
                <p>This OTP will expire in 5 minutes. If you did not request this, please ignore this email.</p>",
                    _ => $@"
                <p>We have received a request to verify your registration. Please use the OTP below to complete your process:</p>
                <p class='otp-text'>{otp}</p>
                <p>This OTP will expire in 5 minutes. If you didn't request this, please ignore this email.</p>",
                };*/

                /*var body =
                   $@"
           <html>
           <head>
               <style>
                   body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; }}
                   .email-container {{ width: 100%; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ccc; border-radius: 8px; background-color: #f9f9f9; }}
                   .email-header {{ text-align: center; margin-bottom: 20px; }}
                   .otp-text {{ font-size: 18px; font-weight: bold; color: #333; }}
                   .footer {{ font-size: 12px; text-align: center; color: #888; margin-top: 20px; }}
               </style>
           </head>
           <body>
               <div class='email-container'>
                   <div class='email-header'>
                       <h2>NSDL - OTP Verification</h2>
                   </div>
                   <p>Dear User,</p>
                   {message}
                   <div class='footer'>
                       <p>NSDL Team</p>
                       <p><small>This email was sent to {email}. Please do not reply directly to this email.</small></p>
                   </div>
               </div>
           </body>
           </html>";*/
               /* string subject = "OTP - NSDL FPI Monitor";*/
                string subject = purpose switch
                {
                    "FORGOT_PASSWORD" or "LOGIN" => "OTP - NSDL FPI Monitor",
                    _ => "NSDL FPI Portal: Email Verification for User Registration",
                };
                string message = purpose switch
                {
                    "FORGOT_PASSWORD" => $@"Your One Time Password (OTP) for Password Reset to NSDL FPI Monitor is {otp}. This OTP is valid for 30 minutes and one login session.",
                    "LOGIN" => $@"Your One Time Password (OTP) for Login to NSDL FPI Monitor is {otp}. This OTP is valid for 30 minutes and one login session.",
                    _ => $@"<p>Welcome to the <strong>NSDL FPI Portal</strong>! (<a href=""https://www.fpi.nsdl.co.in"">www.fpi.nsdl.co.in</a>)</p>
                            <p>Please find below the email verification code for user registration of the Common Application Form on the NSDL FPI Portal.</p>
                            <p>Enter the following code on the portal to continue your registration process:</p>
                            <p>Verification Code: {otp}</p>
                            <p><strong>Email ID (as user ID):</strong> {email}</p>
                            <p>If you have any queries or need assistance, feel free to contact us at <a href=""mailto:fpiassist@nsdl.com"">fpiassist@nsdl.com</a>.</p>"
                };
               
                var body =
                    $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; }}
                    .email-container {{ width: 100%; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ccc; border-radius: 8px; background-color: #f9f9f9; }}
                    .email-header {{ text-align: center; margin-bottom: 20px; }}
                    .otp-text {{ font-size: 18px; font-weight: bold; color: #333; }}
                    .footer {{ font-size: 12px; text-align: center; color: #888; margin-top: 20px; }}
                </style>
            </head>
            <body>
                <div class='email-container'>
                    <div class='email-header'>
                        <h2>NSDL - OTP Verification</h2>
                    </div>
                    <p>Dear User,</p>
                        {message}      
                    <div>
                        <p>Please do not share this with anyone.</p> 
                    </div>
                    <div>
                        Regards,<br>
                        NSDL FPI Monitor.
                    </div>
                    <div class='footer'>
                        <p>NSDL Team</p>
                        <p><small>This email was sent to {email}. Please do not reply directly to this email.</small></p>
                    </div>
                </div>
            </body>
            </html>";

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail!),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(email);
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("mail send succesfully to {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending OTP email to: {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendEncryptedPdfEmailAsync(string base64Data,string email,string purpose)
        {
            try {
                if (base64Data == "")
                    return true;
                bool useGupShup = Convert.ToBoolean(_configuration["EmailSettingsGupShup:UseGupShup"]);
                var fromEmail = _configuration[useGupShup ? "EmailSettingsGupShup:FromEmail" : "EmailSettings:FromEmail"];
                var fromPassword = _configuration["EmailSettings:FromEmailPassword"];
                var smtpServer = _configuration[useGupShup ? "EmailSettingsGupShup:HostName" : "EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]!);
                var mailUserID = useGupShup ? _configuration["EmailSettingsGupShup:MailUserID"] : fromEmail;
                var mailPass = useGupShup ? _configuration["EmailSettingsGupShup:MailPass"] : fromPassword;

                var smtpClient = new SmtpClient(smtpServer)
                {
                    Port = smtpPort,
                    Credentials = new NetworkCredential(mailUserID, mailPass),
                    EnableSsl = false,
                    UseDefaultCredentials = false,
                };
                string emailBody =
                       @"
                <html>
                <head>
                    <style>
                        body { font-family: Arial, sans-serif; background-color: #f2f2f2; margin: 0; padding: 0; }
                        .container { max-width: 600px; margin: 40px auto; background: #fff; padding: 20px; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }
                        .header { font-size: 20px; font-weight: bold; margin-bottom: 15px; }
                        .content { font-size: 14px; line-height: 1.6; }
                        .footer { margin-top: 20px; font-size: 12px; color: #777; }
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>Password Reset Request on FPI Monitor</div>
                        <div class='content'>
                            <p>Dear Sir / Madam,</p>
                            <p>This has reference to your password reset request on FPI Monitor Portal. In this regard, please find enclosed a 'PDF' file containing your new password. The code for opening the 'PDF' file will be the first 4 letters of your organization name in lower case followed by the first 4 letters of your name in lower case. The code is case sensitive.</p>
                            <p>For example,<br/>
                               If the name of your organization is <strong>Soma Stock Exchange</strong> and your name is <strong>Steffy Graph</strong>, then your code will be <strong>somastef</strong>.<br/>
                               If the name of your organization is <strong>Indian Securities Depository Limited</strong> and your name is <strong>Om Prakash</strong>, then your code will be <strong>indiompr</strong>.<br/>
                               If the name of your organization is <strong>IN & FS Securities Services Limited</strong> and your name is <strong>Om</strong>, then your code will be <strong>in&fom--</strong>.<br/>
                               If the name of your organization is <strong>PQ</strong> and your name is <strong>Om</strong>, then your code will be <strong>pq--om--</strong>.
                            </p>
                            <p>In case you need any clarification or assistance, please feel free to contact us on 2499 4785 / 2499 4877 or email us at <a href='mailto:shivkumarj@nsdl.com'>shivkumarj@nsdl.com</a> / <a href='mailto:somac@nsdl.com'>somac@nsdl.com</a>.</p>
                            <p>Regards,<br/>FPI Monitor</p>
                            <p style='font-style: italic;'>This is an auto-generated email. Please do not reply to this email.</p>
                        </div>
                        <div class='footer'>NSDL FPI Portal</div>
                    </div>
                </body>
                </html>";

                byte[] pdfBytes = Convert.FromBase64String(base64Data);
                var attachment = new Attachment(new MemoryStream(pdfBytes), "ResetPassword.pdf", MediaTypeNames.Application.Pdf);
                string subject = "Password Reset Request on FPI Monitor";
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail!),
                    Subject = subject,
                    Body = emailBody,
                    IsBodyHtml = true,
                };
                mailMessage.Attachments.Add(attachment);
                mailMessage.To.Add(email);
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("mail send succesfully to {Email}", email);
                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending reset pdf email to: {Email}", email);
                return false;
            }
        }
    }
}
