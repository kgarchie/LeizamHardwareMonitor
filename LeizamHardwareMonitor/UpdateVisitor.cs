using LeizamHardwareMonitor.Interfaces;
using LibreHardwareMonitor.Hardware;

namespace LeizamHardwareMonitor;

public class UpdateVisitor : IUpdateVisitor, IVisitor
{
    private readonly IConfiguration _configuration;
    private readonly ISendMail _sendMail;
    private readonly ILogger _logger;

    public UpdateVisitor(IConfiguration configuration, ILogger logger)
    {
        _configuration = configuration;
        _logger = logger;
        _sendMail = new SendMail(_configuration, _logger);
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
            computer.Accept(new UpdateVisitor(_configuration, _logger));

            foreach (IHardware hardware in computer.Hardware)
            {
                switch (hardware.HardwareType)
                {
                    // hardware.Update();
                    case HardwareType.Memory:
                    {
                        // if memory
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType != SensorType.Load) continue;
                            switch (sensor.Name)
                            {
                                // Console.WriteLine(sensor.Name + ": " + sensor.Value.ToString() + " %");
                                // Please note: There is both physical memory and virtual memory. The physical memory is the RAM, the virtual memory is the swap file.
                                case "Memory":
                                {
                                    if (sensor.Value > int.Parse(_configuration["CPU_Threshold_Usage"]!))
                                    {
                                        _logger.LogWarning("Physical memory critical: {0} %", sensor.Value);
                                        _sendMail.SendWarningMail("Physical memory critical",
                                            $"Physical memory critical: {sensor.Value} %");
                                    }

                                    break;
                                }
                                case "Virtual Memory":
                                {
                                    if (sensor.Value > int.Parse(_configuration["Virtual_Memory_Threshold_Usage"]!))
                                    {
                                        _logger.LogWarning("Virtual memory critical: {0} %", sensor.Value);
                                        _sendMail.SendWarningMail("Virtual memory critical",
                                            $"Virtual memory critical: {sensor.Value} %");
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
                                        _sendMail.SendWarningMail("CPU critical", $"CPU critical: {sensor.Value} %");
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
                                    }
                                }
                            }
                        }

                        break;
                    }
                }

                computer.Close();

                // foreach (IHardware subhardware in hardware.SubHardware)
                // {
                //     Console.WriteLine("\tSubhardware: {0}", subhardware.Name);
                //
                //     foreach (ISensor sensor in subhardware.Sensors)
                //     {
                //         Console.WriteLine("\t\tSensor: {0}, value: {1}", sensor.Name, sensor.Value);
                //     }
                // }
                //
                // foreach (ISensor sensor in hardware.Sensors)
                // {
                //     Console.WriteLine("\tSensor: {0}, value: {1}", sensor.Name, sensor.Value);
                // }
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