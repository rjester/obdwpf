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
    private bool headers = false;
    private bool echo = false;
    private bool linefeed = false;
    
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
      bool goodConnection = true;
      connected = true;
      headers = false;
      echo = false;
      linefeed = true;
      System.Diagnostics.Debug.WriteLine("Opening connection {0} {1}", portName, baudRate);
      System.Diagnostics.Debug.WriteLine("Port open");

      Init();

      
    
      return goodConnection;
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
        case "0100":
          response = "41 00 BE 1F A8 11 9A";
          break;
        case "0101":
          response = "41 01 81 07 65 00 F2";
          break;
        case "0103":
          response = "41 03 02 00 09";
          break;
        case "0104":
          response = "41 04 43 4B";
          break;
        case "0105":
          response = "41 05 41 48";
          break;
        case "0106":
          response = "41 06 79 83";
          break;
        case "0107":
          response = "41 07 7F 8A";
          break;
        case "010C":
          response = "41 0C 15 69 8E";
          break;
        case "010D":
          response = "41 0D 78 11";
          break;
        case "010E":
          response = "41 0E 93 A5";
          break;
        case "010F":
          response = "41 0F 34 47";
          break;
        case "0110":
          response = "41 10 04 CF E7";
          break;
        case "0111":
          response = "41 11 2C 41";
          break;
        //case "0113":
          //response = "41 13 15 69 8E";
          //break;
        case "0115":
          response = "41 15 80 FF 98";
          break;
        case "011C":
          response = "41 1C 01 21";
          break;
        case "0120":
          response = "41 20 10 00 00 00 34";
          break;
        case "0124":
          response = "41 24 7E 8C 67 C3 5C";
          break;
        default:
          break;
      }
      return response;
    }

    //public double GetStatus() {
    //  var msg = SendElmRequest("0101");
    //  if (msg.Contains("41") && msg.Contains("0D")) {
    //    byte[] bytes = HexStringToBytes(msg.Substring(6));
    //    }
    //  return kph;
    //}

    public double GetFuelSystemStatus() {
      var msg = SendElmRequest("0103");
      // temp: remove 41
      msg = msg.Substring(6);
      byte[] bytes = HexStringToBytes(msg);

      double temp = 0;
      for (int i = 0; i < bytes.Length - 1; i++) {
        temp += Convert.ToDouble(bytes[i]);
      }
      return ((9.0 / 5.0) * (temp - 40)) + 32;
    }

    public double GetCoolantTemp() {
      var msg = SendElmRequest("0105");
      // temp: remove 41
      msg = msg.Substring(6);
      byte[] bytes = HexStringToBytes(msg);

      double temp = 0;
      for (int i = 0; i < bytes.Length - 1; i++) {
        temp += Convert.ToDouble(bytes[i]);
      }
      return ((9.0 / 5.0) * (temp - 40)) + 32;
    }

    public double GetMilesPerHour() {
      var msg = SendElmRequest("010D");
      double result = 0;
      if (msg.Contains("41") && msg.Contains("0D")) {
        byte[] bytes = HexStringToBytes(msg.Substring(6));
        result = bytes[0] * 0.621371;  
      }
      
      return result;
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

    public void Init() {
      if (!Reset()) {
        //goodConnection = false;
        errorMessage = "Error on Reset";
        System.Diagnostics.Debug.WriteLine(errorMessage);
      }
      if (!SetEcho(false)) {
        //goodConnection = false;
        echo = false;
        errorMessage = "Error on Echo";
        System.Diagnostics.Debug.WriteLine(errorMessage);
      }
      if (!SetLineFeed(false)) {
        //goodConnection = false; connected = true;
        linefeed = true;
        errorMessage = "Error on Linefeed";
        System.Diagnostics.Debug.WriteLine(errorMessage);
      }
      if (!Identify()) {
        //goodConnection = false; connected = true;
        //linefeed = true;
        errorMessage = "Error on Identity";
        System.Diagnostics.Debug.WriteLine(errorMessage);
      }
      if (!At1()) {
        //goodConnection = false; connected = true;
        //linefeed = true;
        errorMessage = "Error on AT@1";
        System.Diagnostics.Debug.WriteLine(errorMessage);
      }
      if (!At2()) {
        //goodConnection = false; connected = true;
        //linefeed = true;
        errorMessage = "Error on AT@2";
        System.Diagnostics.Debug.WriteLine(errorMessage);
      }
      SendVoltageRequest();


      var protocol = 3;
      var protocolDescription = "ISO 9141-2 (5 baud init, 10.4 baud)";

      if (protocol == 0) {
        for (int i = 1; i < 10; i++) {
          System.Diagnostics.Debug.WriteLine(string.Format("Trying protocol {0}", i));
          if (Atp(i)) {
            var returnVal = "BUS INIT: ...OK\r41 00 BE 1F A8 11>";
            if (returnVal.Contains("BUS INIT"))
              System.Diagnostics.Debug.WriteLine(string.Format("Detected protocol {0}", i));
          } 
        }
      }
      else {
        System.Diagnostics.Debug.WriteLine(string.Format("Trying protocol {0}", protocol));
        if (Atp(protocol)) {
          var returnVal = "BUS INIT: ...OK\r41 00 BE 1F A8 11>";
          if (returnVal.Contains("BUS INIT"))
            System.Diagnostics.Debug.WriteLine(string.Format("Detected protocol {0}", protocol));
        } 
      }
      //ATP(3);

      //BusInit();
    }

    private bool Reset() {
      System.Diagnostics.Debug.WriteLine("Sending ATZ");

      bool continueReading = true;
      string returnVal = "ATZ\r\rELM327 v1.4b>";
      if (returnVal.Contains('>'))
        continueReading = false;
      System.Diagnostics.Debug.WriteLine(string.Format("Received: {0}", returnVal));
      if (returnVal.Contains("ELM"))
        return true;
      else
        return false;
    }

    private bool SetEcho(bool echoState) {
      System.Diagnostics.Debug.WriteLine("Sending ATE0");
     
      bool continueReading = true;
      string returnVal = "ATE0\rOK>";
      while (continueReading) {
        if (returnVal.Contains('>'))
          continueReading = false;
      }
      System.Diagnostics.Debug.WriteLine(string.Format("Received: {0}", returnVal));
      if (returnVal.Contains("OK"))
        return true;
      else
        return false;
    }

    private bool SetLineFeed(bool linefeedState) {
      System.Diagnostics.Debug.WriteLine("Sending ATL0");
      bool continueReading = true;
      string returnVal = "OK>";
      while (continueReading) {
        if (returnVal.Contains('>'))
          continueReading = false;
      }
      System.Diagnostics.Debug.WriteLine(string.Format("Received: {0}", returnVal));

      if (returnVal.Contains("OK"))
        return true;
      else
        return false;
    }

    private bool Identify() {
      System.Diagnostics.Debug.WriteLine("Sending ATI");

      bool continueReading = true;
      string returnVal = "ATI\r\rELM327 v1.4b>";
      if (returnVal.Contains('>'))
        continueReading = false;
      System.Diagnostics.Debug.WriteLine(string.Format("Received: {0}", returnVal));
      if (returnVal.Contains("ELM"))
        return true;
      else
        return false;
    }

    private bool At1() {
      System.Diagnostics.Debug.WriteLine("Sending AT@1");

      bool continueReading = true;
      string returnVal = "AT@1\r\rOBDII to RS232 Interpreter>";
      if (returnVal.Contains('>'))
        continueReading = false;
      System.Diagnostics.Debug.WriteLine(string.Format("Received: {0}", returnVal));
      if (returnVal.Contains("OBDII"))
        return true;
      else
        return false;
    }

    private bool At2() {
      System.Diagnostics.Debug.WriteLine("Sending AT@2");

      bool continueReading = true;
      string returnVal = "AT@2\r\rSCANTOOL.NET>";
      if (returnVal.Contains('>'))
        continueReading = false;
      System.Diagnostics.Debug.WriteLine(string.Format("Received: {0}", returnVal));
      if (returnVal.Contains("SCANTOOL"))
        return true;
      else
        return false;
    }

    private bool Atp(int protocol) {
      System.Diagnostics.Debug.WriteLine("Sending ATSP " + protocol);

      bool continueReading = true;
      string returnVal = "OK>";
      while (continueReading) {
        if (returnVal.Contains('>'))
          continueReading = false;
      }
      System.Diagnostics.Debug.WriteLine(string.Format("Received: {0}", returnVal));
      if (returnVal.Contains("OK"))
        return true;
      else
        return false;
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
