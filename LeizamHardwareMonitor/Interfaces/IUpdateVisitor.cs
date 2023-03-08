using LibreHardwareMonitor.Hardware;

namespace LeizamHardwareMonitor.Interfaces;

public interface IUpdateVisitor
{
    public Task Monitor();
    public void VisitComputer(IComputer computer);
    public void VisitHardware(IHardware hardware);
    public void VisitSensor(ISensor sensor);
    public void VisitParameter(IParameter parameter);
}