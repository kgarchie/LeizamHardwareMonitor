using LibreHardwareMonitor.Hardware;

namespace LeizamHardwareMonitor.Interfaces;

public interface ISendMail
{
    public void SendWarningMail(string subject, string body, HardwareType hardwareType);
}