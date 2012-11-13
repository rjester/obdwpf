using System;
using System.ComponentModel;

namespace obdwpf {
  public class PollState {
    public PollState() {
    }

    public double Voltage { get; set; }
    public double Mph { get; set; }
    public double Rpm { get; set; }
    public double Maf { get; set; }
    public double Mpg { 
      get{
        return (14.7*6.17*4.54*Mph)/(3600*1/100);
      } 
    }
    public double CoolantTemp { get; set; }

  }
}
