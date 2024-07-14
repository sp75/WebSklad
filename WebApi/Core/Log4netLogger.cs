using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Core
{
    public class Log4netLogger
    {
        private readonly ILog _log;

        public Log4netLogger(string name_log)
        {
            _log = LogManager.GetLogger(name_log);
        }

        public void LogInfo(string format, params object[] args)
        {
            var message = string.Format(
                format,
                args
            );
            LogInfo(message);
        }

        public void LogInfo(string message)
        {
            _log.Info(message);
        }

        public void LogException(Exception ex)
        {
            LogException(ex, null);
        }

        public void LogException(Exception ex, string message)
        {
            var full_message = BuildFullExceptionMessage(ex);

            var log_message = string.Format(
                "{0}Error: {1}\nStack: {2}",
                string.IsNullOrEmpty(message) ? "" : message + "\n",
                full_message,
                ex.StackTrace
            );

            _log.Info(log_message);
        }

        public static string BuildFullExceptionMessage(Exception exception)
        {
            var result = string.Format("{0} : {1}", exception.GetType().FullName, exception.Message);
            if (null != exception.InnerException)
            {
                result += string.Format("{0} -----> {1}",
                    Environment.NewLine,
                    BuildFullExceptionMessage(exception.InnerException));
            }

            return result;
        }
    }
}