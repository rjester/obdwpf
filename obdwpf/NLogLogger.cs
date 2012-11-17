using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace obdwpf {
  public class NLogLogger : ILogger {
    private Logger logger;

    public NLogLogger() {
      logger = LogManager.GetCurrentClassLogger();
    }

    public void Info(string message) {
      logger.Info(message);
    }

    public void Warn(string message) {
      logger.Warn(message);
    }

    public void Debug(string message) {
      logger.Debug(message);
    }

    public void Error(Exception ex) {
      logger.Error(this.BuildExceptionMessage(ex));
    }

    public void Error(string message) {
      logger.Error(message);
    }

    public void Fatal(Exception ex) {
      logger.Fatal(this.BuildExceptionMessage(ex));
    }

    public void Fatal(string message) {
      logger.Fatal(message);
    }
  }
}
