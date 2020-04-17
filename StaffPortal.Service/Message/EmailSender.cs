using StaffPortal.Common;
using StaffPortal.Common.Settings;
using StaffPortal.Service.Errors;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace StaffPortal.Service.Message
{
    public class EmailSender : IEmailSender
    {
        private readonly IErrorService _errorService;
        private readonly EmailSettings _emailSettings;

        public EmailSender(
            IErrorService errorService,
            EmailSettings emailSettings)
        {
            _errorService = errorService;
            _emailSettings = emailSettings;
        }

        public Task SendEmailAsync(string to, string subject, string message)
        {
            try
            {
                MailMessage mailMessage = new MailMessage(_emailSettings.FromEmail, to);

                // Assign strings to the message object
                mailMessage.Subject = subject.ToString();
                mailMessage.Body = message.ToString();
                mailMessage.IsBodyHtml = true;
                // Create smtp and send message
                //if (_env.EnvironmentName == "Development")
                //SmtpClient smtp = new SmtpClient("localhost", 25);

                // TODO: On Production Set a proper SMTP CLIENT
                SmtpClient smtp = new SmtpClient(_emailSettings.Host);
                NetworkCredential nc = new NetworkCredential(_emailSettings.UserName, _emailSettings.Password);
                smtp.Credentials = nc;

                smtp.Send(mailMessage);
            }
            catch (Exception ex)
            {
                _errorService.Insert(new ErrorLog(ex.Message, ex.StackTrace));                
            }

            return Task.CompletedTask;
        }
    }
}
