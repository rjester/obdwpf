using System;

namespace obdwpf {
  public class PollState {
    public PollState() {
    }

    public decimal Voltage { get; set; }
    public int Mph { get; set; }
    public int Rpm { get; set; }
  }
}
