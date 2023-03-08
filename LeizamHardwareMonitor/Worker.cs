using LeizamHardwareMonitor.Interfaces;

namespace LeizamHardwareMonitor;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly int _taskDelay;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        

        _configuration = new ConfigurationBuilder()
            .AddJsonFile("./Settings.json", true, true)
            .Build();

        // Check if all required settings are set and not string.Empty
        if (_configuration["CPU_Threshold_Usage"] != null &&
            _configuration["Virtual_Memory_Threshold_Usage"] != null &&
            _configuration["Physical_Memory_Threshold_Usage"] != null &&
            _configuration["Disk_Threshold_Usage"] != null &&
            _configuration["SmtpTo"] != string.Empty &&
            _configuration["SmtpTo"] != null &&
            _configuration["Domain"] != string.Empty &&
            _configuration["Domain"] != null &&
            _configuration["SmtpFrom"] != string.Empty &&
            _configuration["SmtpFrom"] != null &&
            _configuration["Task_Delay"] != string.Empty &&
            _configuration["Task_Delay"] != null
           )
        {
            Console.WriteLine("All required settings are set");
        }
        else
        {
            Console.WriteLine("Missing configuration for sending mail: Please check Settings.json, Exiting...");
            Environment.Exit(1);
        }
        
        _taskDelay = int.Parse(_configuration["Task_Delay"]!);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            // monitor hardware
            IUpdateVisitor updateVisitor = new UpdateVisitor(_configuration, _logger);
            try
            {
                await updateVisitor.Monitor();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while monitoring hardware");
            }

            await Task.Delay(_taskDelay, stoppingToken);
        }
    }
}