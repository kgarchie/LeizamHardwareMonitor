using LeizamHardwareMonitor.Interfaces;

namespace LeizamHardwareMonitor;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        
        _configuration = new ConfigurationBuilder()
            .AddJsonFile("./Settings.json", true, true)
            .Build();
        
        // Check if all required settings are set and not string.Empty
        if (_configuration["SmtpServer"] != null && _configuration["SmtpPort"] != null &&
            _configuration["SmtpUser"] != null && _configuration["SmtpPassword"] != null &&
            _configuration["SmtpFrom"] != null && _configuration["SmtpTo"] != null &&
            _configuration["CPU_Threshold_Usage"] != null &&
            _configuration["SmtpServer"] != string.Empty && _configuration["SmtpPort"] != string.Empty &&
            _configuration["SmtpUser"] != string.Empty && _configuration["SmtpPassword"] != string.Empty &&
            _configuration["SmtpFrom"] != string.Empty && _configuration["SmtpTo"] != string.Empty &&
            _configuration["CPU_Threshold_Usage"] != string.Empty)
        {
            Console.WriteLine("All required settings are set");
        }
        else
        {
            Console.WriteLine("Missing configuration for sending mail: Please check Settings.json, Exiting...");
            Environment.Exit(1);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
            
           // monitor hardware
           IUpdateVisitor updateVisitor = new UpdateVisitor(_configuration, _logger);
           try
           {
               updateVisitor.Monitor();
           } catch (Exception e)
           {
               _logger.LogError(e, "Error while monitoring hardware");
           }
        }
    }
}
