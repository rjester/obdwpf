using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace obdwpf {
  public class ObdDevice : IObdDevice
  {
    SerialPort port;
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
    
    public ObdDevice() {
      commandBuffer = new byte[125];
      state = new PollState();
      //processThread = new Thread(new ThreadStart(Poll));
    }

    public bool Connected
    {
      get
      {
        return this.connected;
      }
      set
      {
        this.connected = value;
      }
    }

    public PollState Poll()
    {
        var voltage = SendVoltageRequest();
        state.Voltage = Convert.ToDouble(voltage.Substring(0, voltage.Length - 4));
        //state.Mph = GetMilesPerHour();
        //state.Rpm = GetRpm();
        state.CoolantTemp = GetCoolantTemp();
        return state;
    }

    public bool Connect(string portName, int baudRate)
    {
      bool goodConnection = true;
      port = new SerialPort(portName, baudRate);
      port.ReadTimeout = 1000;
      try {
        port.Open();
      }
      catch (Exception ex) {
        return false;
      }

      if (port.IsOpen) {
        connected = true;
        port.DiscardInBuffer();
        port.DiscardOutBuffer();

        if (!Reset()) {
          goodConnection = false;
          errorMessage = "Error on Reset";
          System.Diagnostics.Debug.WriteLine(errorMessage);
        }
        if (!SetEcho(false)) {
          goodConnection = false;
          echo = false;
          errorMessage = "Error on Echo";
        }
        if (!SetLineFeed(false)) {
          goodConnection = false; connected = true;
          linefeed = true;
          errorMessage = "Error on Linefeed";
        }
        //STI
        //ATI
        var atiOutput = SendAtCommand("ATI");
        
        if (!atiOutput.Contains("ELM")) {
          goodConnection = false; connected = true;
          //linefeed = true;
          errorMessage = "Error on ATI";
          }
        //AT@1
        var at1Output = SendAtCommand("AT@1");

        if (!at1Output.Contains("OBDII to RS232 Interpreter")) {
          goodConnection = false; connected = true;
          //linefeed = true;
          errorMessage = "Error on AT@1";
        }
        //AT@2
        var at2Output = SendAtCommand("AT@2");

        if (!at2Output.Contains("SCANTOOL.NET")) {
          goodConnection = false; connected = true;
          //linefeed = true;
          errorMessage = "Error on AT@2";
        }
        //ATRV
        var atrvOutput = SendVoltageRequest();
        if (!atrvOutput.Contains("V")) {
          goodConnection = false; connected = true;
          //linefeed = true;
          errorMessage = "Error on ATRV";
        }
        //ATSP 3
        var atspOutput = SendAtCommand("ATSP 3");

        if (!atspOutput.Contains("OK")) {
          goodConnection = false; connected = true;
          //linefeed = true;
          errorMessage = "Error on ATSP 3";
        }
        //0100 - BUS INIT
        var busInitOutput = SendElmRequest("01 00");
        if (!busInitOutput.Contains("BUS INIT")) {
          goodConnection = false; connected = true;
          //linefeed = true;
          errorMessage = "Error on BUS INIT";
        }
        //ATH1
        if (!SetHeader(false)) {
          goodConnection = false;
          headers = false;
          errorMessage = "Error on Header";
        }
        //0100
        //0120



        
        if (!goodConnection) {
          connected = false;
          port.Close();
        }
        else {
          port.DiscardInBuffer();
          port.DiscardOutBuffer();
          //processThread.Start();
        }
      }
      return goodConnection;
    }

    public void Disconnect()
    {
      if (port.IsOpen)
      {
        port.Close();
      }
      connected = false;
    }

    private bool SetLineFeed(bool linefeedState) {
      port.DiscardOutBuffer();
      port.DiscardInBuffer();
      if (linefeedState)
        port.Write("ATL1\r");
      else
        port.Write("ATL0\r");
      bool continueReading = true;
      string returnVal = string.Empty;
      while (continueReading) {
        //count = port.Read(commandBuffer, 0, 1024);
        count = port.BytesToRead;
        commandBuffer = new byte[count];
        port.Read(commandBuffer, 0, count);
        returnVal += System.Text.Encoding.Default.GetString(commandBuffer, 0, count);
        if (returnVal.Contains('>'))
          continueReading = false;
      }
      if (returnVal.Contains("OK"))
        return true;
      else
        return false;
    }

    private bool SetHeader(bool headerState) {
      port.DiscardOutBuffer();
      port.DiscardInBuffer();
      if (headerState)
        port.Write("ATH1\r");
      else
        port.Write("ATH0\r");
      bool continueReading = true;
      string returnVal = string.Empty;
      while (continueReading) {
        //count = port.Read(commandBuffer, 0, 1024);
        count = port.BytesToRead;
        commandBuffer = new byte[count];
        port.Read(commandBuffer, 0, count);
        returnVal += System.Text.Encoding.Default.GetString(commandBuffer, 0, count);
        if (returnVal.Contains('>'))
          continueReading = false;
      }
      if (returnVal.Contains("OK"))
        return true;
      else
        return false;
    }

    private bool SetEcho(bool echoState) {
      port.DiscardOutBuffer();
      port.DiscardInBuffer();
      if (echoState)
        port.Write("ATE1\r");
      else
        port.Write("ATE0\r");
      bool continueReading = true;
      string returnVal = string.Empty;
      while (continueReading) {
        //count = port.Read(commandBuffer, 0, 1024);
        count = port.BytesToRead;
        commandBuffer = new byte[count];
        port.Read(commandBuffer, 0, count);
        returnVal += System.Text.Encoding.Default.GetString(commandBuffer, 0, count);
        if (returnVal.Contains('>'))
          continueReading = false;
      }
      if (returnVal.Contains("OK"))
        return true;
      else
        return false;
    }

    private bool Reset() {
      port.DiscardOutBuffer();
      port.DiscardInBuffer();
      System.Diagnostics.Debug.WriteLine("Sending ATZ");
      port.Write("ATZ\r");

      bool continueReading = true;
      string returnVal = string.Empty;
      while (continueReading) {
        //returnVal += port.ReadTo(">");
        count = port.BytesToRead;
        commandBuffer = new byte[count];
        port.Read(commandBuffer, 0, count);
        returnVal += System.Text.Encoding.Default.GetString(commandBuffer, 0, count);
        if (returnVal.Contains('>'))
          continueReading = false;
      }
      System.Diagnostics.Debug.WriteLine("Received: {0}", returnVal);
      if (returnVal.Contains("ELM"))
        return true;
      else
        return false;
    }

    public string SendVoltageRequest()
    {
      port.DiscardOutBuffer();
      port.DiscardInBuffer();
      port.Write("ATRV\r");
      bool continueReading = true;
      string returnVal = string.Empty;
      while (continueReading) {
        //count = port.Read(commandBuffer, 0, 1024);
        count = port.BytesToRead;
        commandBuffer = new byte[count];
        port.Read(commandBuffer, 0, count);
        returnVal += System.Text.Encoding.Default.GetString(commandBuffer, 0, count);
        if (returnVal.Contains('>'))
          continueReading = false;
      }
      return returnVal;
    }

    public string SendAtCommand(string command) {
      System.Diagnostics.Debug.WriteLine(string.Format("Sending {0}", command));
      port.Write(command + "\r");
      bool continueReading = true;
      string returnVal = string.Empty;
      while (continueReading) {
        //count = port.Read(commandBuffer, 0, 1024);
        count = port.BytesToRead;
        commandBuffer = new byte[count];
        port.Read(commandBuffer, 0, count);
        returnVal += System.Text.Encoding.Default.GetString(commandBuffer, 0, count);
        if (returnVal.Contains('>'))
          continueReading = false;
      }
      System.Diagnostics.Debug.WriteLine(string.Format("Received {0}", returnVal));
      return returnVal;
    }

    public string SendElmRequest(string command)
    {
      port.Write(command + "\r");
      bool continueReading = true;
      string returnVal = string.Empty;
      while (continueReading) {
        //count = port.Read(commandBuffer, 0, 1024);
        count = port.BytesToRead;
        commandBuffer = new byte[count];
        port.Read(commandBuffer, 0, count);
        returnVal += System.Text.Encoding.Default.GetString(commandBuffer, 0, count);
        if (returnVal.Contains('>'))
          continueReading = false;
      }
      return returnVal;
    }

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
      msg = msg.Substring(5).Replace("\r", "").Replace(">", "").Trim();
      System.Diagnostics.Debug.WriteLine(string.Format("Coolant Temp: {0}", msg));
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

      var rpm = ((bytes[0] * 256) + bytes[1]) / 4;
      return rpm;
    }

    private byte[] HexStringToBytes(string hex) {
      byte[] data = new byte[hex.Length / 2];
      int j = 0;
      for (int i = 0; i < hex.Length; i += 3) {
        data[j] = Convert.ToByte(hex.Substring(i, 2), 16);
        ++j;
      }
      return data;
    }
  }
}
