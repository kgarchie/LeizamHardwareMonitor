using LeizamHardwareMonitor.Interfaces;
using LibreHardwareMonitor.Hardware;
using RestSharp;

namespace LeizamHardwareMonitor;

public class SendMail : ISendMail
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private bool _cpuIsSent;
    private bool _diskIsSent;
    private bool _memoryIsSent;

    public SendMail(IConfiguration configuration, ILogger logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void SendWarningMail(string subject, string body, HardwareType hardwareType)
    {
        switch (hardwareType)
        {
            case HardwareType.Cpu when _cpuIsSent:
                _logger.LogInformation("CPU mail is already sent | Spam protection is active");
                return;
            case HardwareType.Memory when _memoryIsSent:
                _logger.LogInformation("Memory mail is already sent | Spam protection is active");
                return;
            case HardwareType.Storage when _diskIsSent:
                _logger.LogInformation("Disk mail is already sent | Spam protection is active");
                return;
            default:
                _logger.LogInformation(hardwareType + " is calling SendWarningMail");
                break;
        }
        
        var now = DateTime.Now.ToUniversalTime();
        body = $"{body} at DateTime:{now}";
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
            SetEmailSendTimeout(hardwareType);
        }
        else
        {
            _logger.LogError("Error while sending mail");
        }
    }

    private async void SetEmailSendTimeout(HardwareType hardwareType)
    {
        // for spam protection
        switch (hardwareType)
        {
            case HardwareType.Cpu:
                _cpuIsSent = true;
                // Console.WriteLine("CPU is set to wait 60 seconds");
                await Task.Delay(int.Parse(_configuration["Task_Delay"]!));
                // Console.WriteLine("CPU delay is over");
                _cpuIsSent = false;
                break;
            case HardwareType.Memory:
                _memoryIsSent = true;
                // Console.WriteLine("Memory is set to wait 60 seconds");
                await Task.Delay(int.Parse(_configuration["Task_Delay"]!));
                // Console.WriteLine("Memory delay is over");
                _memoryIsSent = false;
                break;
            case HardwareType.Storage:
                _diskIsSent = true;
                // Console.WriteLine("Disk is set to wait 60 seconds");
                await Task.Delay(int.Parse(_configuration["Task_Delay"]!));
                // Console.WriteLine("Disk delay is over");
                _diskIsSent = false;
                break;
        }
    }
}

public record MailRequest(
    string ToEmail,
    string Subject,
    string Body,
    string From
);