using EduShpere.Domain.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace EduShpere.Infrastructure;
public class EmailService : IEmailService
{
    private readonly EmailConfig _emailConfig;
    public EmailService(IOptions<EmailConfig> emailConfig)
    {
        _emailConfig = emailConfig.Value;
    }
    public void SendTestEmail(User user)
    {
        if (user.Email == null)
        {
            return;
        }

        MailMessage message = new MailMessage();
        message.From = new MailAddress(_emailConfig.AppEmail);
        message.Subject = "Test Email";
        message.To.Add(new MailAddress(user.Email));
        message.IsBodyHtml = true;
        message.Body = "<html><body>Test Content</body></html>";

        var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(_emailConfig.AppEmail, _emailConfig.AppPassword),
            EnableSsl = true
        };

        smtpClient.Send(message);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
    {
        if (string.IsNullOrEmpty(toEmail)) return;

        var message = new MailMessage
        {
            From = new MailAddress(_emailConfig.AppEmail),
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml
        };

        message.To.Add(new MailAddress(toEmail));

        using var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(_emailConfig.AppEmail, _emailConfig.AppPassword),
            EnableSsl = true
        };

        await smtpClient.SendMailAsync(message);
    }

}
