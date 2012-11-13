using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace obdwpf {
  public class DashboardViewModel : INotifyPropertyChanged {

    private PollState state;

    public PollState State {
      get {
        return this.state;
      }
      set {
        this.state = value;
        RaisePropertyChanged("State");
      }
    }

    public DashboardViewModel() {
      State = new PollState { Mph = 0, Rpm = 0, Voltage = 0 };
    }

    private void RaisePropertyChanged(string propertyName) {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) {
        handler(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    public double Voltage {
      get {
        return State.Voltage;
      }
      set {
        State.Voltage = value;
        RaisePropertyChanged("Voltage");
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;


  }
}
