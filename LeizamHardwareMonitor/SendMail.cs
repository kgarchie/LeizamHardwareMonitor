using System.Net.Mail;
using LeizamHardwareMonitor.Interfaces;
namespace LeizamHardwareMonitor;

public class SendMail : ISendMail
{
    private readonly IConfiguration _configuration;
    
    public SendMail(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public void SendWarningMail(string subject, string body)
    {
        
        body = $"<html><body>{body}</body></html>";
        
        var smtpServer = _configuration["SmtpServer"];
        var smtpPort = _configuration["SmtpPort"];
        var smtpUser = _configuration["SmtpUser"];
        var smtpPassword = _configuration["SmtpPassword"];
        var smtpFrom = _configuration["SmtpFrom"];
        var smtpTo = _configuration["SmtpTo"];
        
        SmtpClient smtpClient = new(smtpServer, int.Parse(smtpPort!))
        {
            Credentials = new System.Net.NetworkCredential(smtpUser, smtpPassword),
            EnableSsl = true
        };
        
        MailMessage mailMessage = new(smtpFrom!, smtpTo!, subject, body);
        smtpClient.Send(mailMessage);
    }
}