using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace obdwpf {
  public static class LoggerExtensions {
    public static string BuildExceptionMessage(this ILogger logger, Exception x) {
        Exception logException = x;
        if (x.InnerException != null)
            logException = x.InnerException;
 
        string strErrorMsg = Environment.NewLine + "Message :" + logException.Message;
        strErrorMsg += Environment.NewLine + "Source :" + logException.Source;
        strErrorMsg += Environment.NewLine + "Stack Trace :" + logException.StackTrace;
        strErrorMsg += Environment.NewLine + "TargetSite :" + logException.TargetSite;

        return strErrorMsg;
    }
  }
}