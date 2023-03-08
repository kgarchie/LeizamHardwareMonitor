using LeizamHardwareMonitor.Interfaces;
using RestSharp;

namespace LeizamHardwareMonitor;

public class SendMail : ISendMail
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public SendMail(IConfiguration configuration, ILogger logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void SendWarningMail(string subject, string body)
    {
        body = $"{body}";
        var smtpFrom = _configuration["SmtpFrom"];
        var smtpTo = _configuration["SmtpTo"];
        var domain = _configuration["Domain"];

        var request = new MailRequest(
            smtpTo!,
            subject,
            body,
            smtpFrom!
        );

        var uri = new Uri(domain!);
        var client = new RestClient(uri);

        client.AddDefaultHeader("Content-Type", "application/json");
        client.AddDefaultHeader("Accept", "application/json");

        var restRequest = new RestRequest("/Email/Mail/SendMail", Method.Post);
        restRequest.AddJsonBody(request);

        var restClientResult = client.Execute<MailRequest>(restRequest);

        if (restClientResult.IsSuccessful)
        {
            _logger.LogInformation("Mail sent successfully");
        }
        else
        {
            _logger.LogError("Error while sending mail");
        }
    }
}

public record MailRequest(
    string ToEmail,
    string Subject,
    string Body,
    string From
);