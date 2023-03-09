using LeizamHardwareMonitor.Interfaces;

namespace LeizamHardwareMonitor;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly int _taskDelay;
    private IUpdateVisitor _updateVisitor;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        

        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("./Settings.json", true, true)
            .Build();

        // Check if all required settings are set and not string.Empty
        if (configuration["CPU_Threshold_Usage"] != null &&
            configuration["Virtual_Memory_Threshold_Usage"] != null &&
            configuration["Physical_Memory_Threshold_Usage"] != null &&
            configuration["Disk_Threshold_Usage"] != null &&
            configuration["SmtpTo"] != string.Empty &&
            configuration["SmtpTo"] != null &&
            configuration["Domain"] != string.Empty &&
            configuration["Domain"] != null &&
            configuration["SmtpFrom"] != string.Empty &&
            configuration["SmtpFrom"] != null &&
            configuration["Task_Delay"] != string.Empty &&
            configuration["Task_Delay"] != null
           )
        {
            Console.WriteLine("All required settings are set");
        }
        else
        {
            Console.WriteLine("Missing configuration for sending mail: Please check Settings.json, Exiting...");
            Environment.Exit(1);
        }
        
        ISendMail sendMail = new SendMail(configuration, _logger);
        
        _taskDelay = int.Parse(configuration["Task_Delay"]!);
        
        _updateVisitor = new UpdateVisitor(configuration, _logger, sendMail);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            // monitor hardware
            try
            {
                await _updateVisitor.Monitor();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while monitoring hardware");
            }
        }
    }
}