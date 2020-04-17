using System.Threading.Tasks;

namespace StaffPortal.Service.Message
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string to, string subject, string message);
    }
}
