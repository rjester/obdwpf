using System;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace obdwpf
{
  public interface IObdDevice1
  {
    bool Connected { get; set; }
  }
}