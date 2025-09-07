using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure;
public interface IEmailService
{
    void SendTestEmail(User user);
    Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
}