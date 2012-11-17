using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace obdwpf {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window, INotifyPropertyChanged {
    private IObdDevice device;
    private PollState state = new PollState();
    DashboardViewModel vm = new DashboardViewModel();
    public MainWindow() {
      InitializeComponent();

      base.DataContext = vm;
      device = new TestDevice();
      //device = new ObdDevice();
      Task.Factory.StartNew(() => {
        while (true) {
          if (device.Connected) {
            recInterfaceConnectionStatus.Fill = new SolidColorBrush(Colors.Green);
            vm.State = device.Poll();

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {
              RpmReadout.Value = vm.State.Rpm;
              RpmNeedle.Value = vm.State.Rpm;
              MphReadout.Value = vm.State.Mph;
              MphNeedle.Value = vm.State.Mph;
              VoltageNeedle.Value = vm.State.Voltage;
              VoltageReadout.Value = vm.State.Voltage;
              CoolantNeedle.Value = vm.State.CoolantTemp;
              CoolantReadout.Value = vm.State.CoolantTemp;

            }));

          }
          Thread.Sleep(10000);
        }
      });

    }

    private void btnConnect_Click(object sender, RoutedEventArgs e) {
      if (!device.Connect("COM3", 115200))
        MessageBox.Show("Connection failed");
    }

    private void btnDisconnect_Click(object sender, RoutedEventArgs e) {
      device.Disconnect();
      ResetGauges();
    }

    private void ResetGauges() {
      Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {
        RpmReadout.Value = 0;
        RpmNeedle.Value = 0;
        MphReadout.Value = 0;
        MphNeedle.Value = 0;
        VoltageNeedle.Value = 0;
        VoltageReadout.Value = 0;
        CoolantNeedle.Value = 0;
        CoolantReadout.Value = 0;
      }));
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void Window_Closing_1(object sender, CancelEventArgs e) {
      device.Disconnect();
      ResetGauges();
    }
  }
}
