using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace obdwpf {
  public class TestDevice : IObdDevice {
    byte[] commandBuffer;
    Thread processThread;
    int errorCount, count;
    string errorMessage;
    PollState state;
    private bool connected = false;
    public event obdResponseEventHandler obdResponse;

    public delegate void obdResponseEventHandler(string response);

    public TestDevice() {
      commandBuffer = new byte[125];
      state = new PollState();
      //processThread = new Thread(new ThreadStart(Poll));
    }

    public bool Connected {
      get {
        return this.connected;
      }
      set {
        this.connected = value;
      }
    }

    public PollState Poll() {
        var voltage = SendVoltageRequest();
        state.Voltage = Convert.ToDouble(voltage.Substring(0, voltage.Length - 1));
        state.Mph = GetMilesPerHour();
        state.Rpm = GetRpm();
        state.CoolantTemp = GetCoolantTemp();
        return state;
    }

    public bool Connect(string portName, int baudRate) {
      //processThread.Start();
      connected = true;
      return true;
    }

    public void Disconnect() {
      connected = false;
    }

    public string SendVoltageRequest() {
      return "10.5V";
    }

    public string SendElmRequest(string command) {
      string response = string.Empty;
      switch (command.ToUpper()) {
        case "0105":
          response = "41 05 3C 45";
          break;
        case "0110":
          response = "10";
          break;
        case "010C":
          response = "41 0C 15 69 8E";
        break;
        case "010D":
          response = "120";
        break;
        default:
          break;
      }
      return response;
    }

    public double GetStatus() {
      var kph = Convert.ToInt32(SendElmRequest("0101")) * 0.621371;
      return kph;
    }

    public double GetMilesPerHour() {
      var kph = Convert.ToInt32(SendElmRequest("010D")) * 0.621371;
      return kph;
    }

    public double GetRpm() {
      var msg = SendElmRequest("010C");
      // temp: remove 41
      msg = msg.Substring(6);

      byte[] bytes = HexStringToBytes(msg);

      //double temp = 0;
      //for (int i = 0; i < bytes.Length - 1; i++) {
      //  temp += Convert.ToDouble(bytes[i]);
      //}
      var rpm = ((bytes[0] * 256) + bytes[1]) / 4;
      return rpm;
    }

    public double GetCoolantTemp() {
      var msg = SendElmRequest("0105");
      // temp: remove 41
      msg = msg.Substring(6);
      byte[] bytes = HexStringToBytes(msg);
      
      double temp = 0;
      for (int i = 0; i < bytes.Length-1; i++) {
        temp += Convert.ToDouble(bytes[i]);
      }
      return ((9.0/5.0) * (temp-40)) + 32;
    }

    private byte[] HexStringToBytes(string hex) {
      byte[] data = new byte[hex.Length/2];
      int j = 0;
      for (int i = 0; i < hex.Length; i+=3) {
        data[j] = Convert.ToByte(hex.Substring(i, 2), 16);
        ++j;
      }
      return data;
    }

    public double GetMaf() {
      var val = Convert.ToDouble(SendElmRequest("0110"));
      return val;
    }

        //    0x00 - SupportedPIDs0
        //0x01 - MonitorStatus
        //0x03 - FuelSystemStatus
        //0x04 - EngineLoad
        //0x05 - EngineCoolantTemp
        //0x06 - ShortTermFuelTrimBank1
        //0x07 - LongTermFuelTrimBank1
        //0x0C - EngineRPM
        //0x0D - VehicleSpeed
        //0x0E - TimingAdvance
        //0x0F - IntakeAirTemperature
        //0x10 - MassAirFlowRate
        //0x11 - ThrottlePosition
        //0x13 - OxygenSensorsPresent
        //0x15 - OxygenSensor2Voltage
        //0x15 - OxygenSensor2ShortTermFuelTrim
        //0x1C - OBDStandard
        //0x20 - SupportedPIDs1
        //0x24 - OxygenSensor1LambdaWideRange
        //0x24 - OxygenSensor1VoltageWideRange

  }
}
