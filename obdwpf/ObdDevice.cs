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
      //while (true)
      //{
        state.Voltage = Convert.ToDouble(SendVoltageRequest());
        state.Mph = Convert.ToInt32(SendElmRequest("010D"));
        state.Rpm = Convert.ToInt32(SendElmRequest("010C"));
        //Thread.Sleep(1000);
        return state;
      //}
    }

    public bool Connect(string portName, int baudRate)
    {
      bool goodConnection = true;
      port = new SerialPort(portName, baudRate);
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
        }
        if (!SetLineFeed(false)) {
          goodConnection = false;
          errorMessage = "Error on Linefeed";
        }
        if (!SetHeader(false)) {
          goodConnection = false;
          errorMessage = "Error on Header";
        }
        if (!SetEcho(false)) {
          goodConnection = false;
          errorMessage = "Error on Echo";
        }
        if (!goodConnection) {
          connected = false;
          port.Close();
        }
        else {
          port.DiscardInBuffer();
          port.DiscardOutBuffer();
          processThread.Start();
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
      if (returnVal.Contains("ELM"))
        return true;
      else
        return false;
    }

    public string SendVoltageRequest()
    {
      port.DiscardOutBuffer();
      port.DiscardInBuffer();
      port.Write("ATRV" + 0x0D);
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
  }
}
