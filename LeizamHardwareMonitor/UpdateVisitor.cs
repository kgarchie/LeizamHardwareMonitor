using LeizamHardwareMonitor.Interfaces;
using LibreHardwareMonitor.Hardware;

namespace LeizamHardwareMonitor;

public class UpdateVisitor : IUpdateVisitor, IVisitor
{
    private readonly IConfiguration _configuration;
    private readonly ISendMail _sendMail;
    private readonly ILogger _logger;

    public UpdateVisitor(IConfiguration configuration, ILogger logger, ISendMail sendMail)
    {
        _configuration = configuration;
        _logger = logger;
        _sendMail = sendMail;
    }

    public Task Monitor()
    {
        try
        {
            Computer computer = new Computer
            {
                IsCpuEnabled = true,
                // IsGpuEnabled = true,
                IsMemoryEnabled = true,
                // IsMotherboardEnabled = true,
                // IsControllerEnabled = true,
                // IsNetworkEnabled = true,
                // IsStorageEnabled = true
            };

            computer.Open();
            computer.Accept(new UpdateVisitor(_configuration, _logger, _sendMail));

            foreach (IHardware hardware in computer.Hardware)
            {
                switch (hardware.HardwareType)
                {
                    case HardwareType.Memory:
                    {
                        // if memory
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType != SensorType.Load) continue;
                            switch (sensor.Name)
                            {
                                case "Memory":
                                {
                                    if (sensor.Value > int.Parse(_configuration["CPU_Threshold_Usage"]!))
                                    {
                                        _logger.LogWarning("Physical memory critical: {0} %", sensor.Value);
                                        _sendMail.SendWarningMail("Physical memory critical",
                                            $"Physical memory critical: {sensor.Value} %", HardwareType.Memory);
                                    }

                                    break;
                                }
                                case "Virtual Memory":
                                {
                                    if (sensor.Value > int.Parse(_configuration["Virtual_Memory_Threshold_Usage"]!))
                                    {
                                        _logger.LogWarning("Virtual memory critical: {0} %", sensor.Value);
                                        _sendMail.SendWarningMail("Virtual memory critical",
                                            $"Virtual memory critical: {sensor.Value} %", HardwareType.Memory);
                                    }

                                    break;
                                }
                            }
                        }

                        break;
                    }
                    // if cpu
                    case HardwareType.Cpu:
                    {
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Load)
                            {
                                // Console.WriteLine(sensor.Name + ": " + sensor.Value.ToString() + " %");
                                if (sensor.Name == "CPU Total")
                                {
                                    if (sensor.Value > int.Parse(_configuration["CPU_Threshold_Usage"]!))
                                    {
                                        _logger.LogWarning("CPU critical: {0} %", sensor.Value);
                                        _sendMail.SendWarningMail("CPU critical", $"CPU critical: {sensor.Value} %",
                                            HardwareType.Cpu);
                                    }
                                }
                            }
                        }

                        break;
                    }
                    // if HDD
                    case HardwareType.Storage:
                    {
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Load)
                            {
                                // Console.WriteLine(sensor.Name + ": " + sensor.Value.ToString() + " %");
                                if (sensor.Name == "HDD Total")
                                {
                                    if (sensor.Value > int.Parse(_configuration["HDD_Threshold_Usage"]!))
                                    {
                                        _logger.LogWarning("HDD critical: {0} %", sensor.Value);
                                        _sendMail.SendWarningMail("HDD critical", $"HDD critical: {sensor.Value} %",
                                            HardwareType.Storage);
                                    }
                                }
                            }
                        }

                        break;
                    }
                }

                computer.Close();
            }

            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while monitoring");
            return Task.CompletedTask;
        }
    }


    public void VisitComputer(IComputer computer)
    {
        computer.Traverse(this);
    }

    public void VisitHardware(IHardware hardware)
    {
        hardware.Update();
        foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
    }

    public void VisitSensor(ISensor sensor)
    {
    }

    public void VisitParameter(IParameter parameter)
    {
    }
}