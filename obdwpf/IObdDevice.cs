using System;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace obdwpf
{
  public interface IObdDevice
  {
    bool Connected { get; set; }
    
    PollState Poll();

    bool Connect(string portName, int baudRate);

    void Disconnect();

    string SendVoltageRequest();

    string SendElmRequest(string command);

    double GetFuelSystemStatus();
  }
}