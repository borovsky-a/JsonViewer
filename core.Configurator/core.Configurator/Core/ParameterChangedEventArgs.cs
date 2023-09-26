using mop.Configurator.Log;
using System;
using System.ComponentModel;

namespace mop.Configurator
{
    public class ParameterChangedEventArgs : EventArgs
    {
       
        public ParameterChangedEventArgs(object sender,  string message, LogLevel logLevel = LogLevel.None, Exception exception = null, params object[] messageParameters)          
        {
            Sender = sender;
            if (!string.IsNullOrEmpty(message) && messageParameters.Length > 0)
                Message = string.Format(message, messageParameters);
            else Message = message;
            LogLevel = (logLevel == LogLevel.None && exception != null) ? LogLevel.Error : logLevel;
        }

        object Sender { get; }
        public string Message { get; }
        public LogLevel LogLevel { get; }
        public Exception exception { get; }
    }
}
