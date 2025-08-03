using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;

namespace NLog.Web.AspNetCore.Targets.Gelf
{
    public class GelfConverter : IConverter
    {
        private const int SHORT_MESSAGE_MAXLENGTH = 250;
        private const int EXCEPTION_MESSAGE_DEPTH = 10;

        public JObject GetGelfJson(LogEventInfo logEventInfo, string facility, string gelfVersion = "1.0")
        {
            //Retrieve the formatted message from LogEventInfo
            var logEventMessage = logEventInfo.FormattedMessage;
            if (logEventMessage == null) return null;

            //If we are dealing with an exception, pass exception properties to LogEventInfo properties
            if (logEventInfo.Exception != null)
            {
                string exceptionDetail;
                string stackDetail;

                GetExceptionMessages(logEventInfo.Exception, out exceptionDetail, out stackDetail);

                logEventInfo.Properties.Add("ExceptionSource", logEventInfo.Exception.Source);
                logEventInfo.Properties.Add("ExceptionMessage", exceptionDetail);
                logEventInfo.Properties.Add("StackTrace", stackDetail);
            }

            //Figure out the short message
            var shortMessage = logEventMessage;
            if (shortMessage.Length > SHORT_MESSAGE_MAXLENGTH)
            {
                shortMessage = shortMessage.Substring(0, SHORT_MESSAGE_MAXLENGTH);
            }

            //Spec says: facility must be set by the client to "GELF" if empty
            facility = (string.IsNullOrEmpty(facility) ? "GELF" : facility);
            string line = logEventInfo.CallerLineNumber.ToString(CultureInfo.InvariantCulture);
            string file = logEventInfo.CallerFilePath is null ? string.Empty : logEventInfo.CallerFilePath;

            JObject jsonObject;

            //Construct the instance of GelfMessage
            //See http://docs.graylog.org/en/3.0/pages/gelf.html#gelf-payload-specification "Specification (version 1.1)"
            if (gelfVersion == "1.1")
            {
                jsonObject = JObject.FromObject(new GelfMessageV1_1
                {
                    Version = gelfVersion,
                    Host = Dns.GetHostName(),
                    ShortMessage = shortMessage,
                    FullMessage = logEventMessage,
                    Timestamp = new DateTimeOffset(logEventInfo.TimeStamp).ToUnixTimeMilliseconds() / 1000.0,
                    Level = GetSeverityLevel(logEventInfo.Level),
                });

                //Spec says: facility, line and file fields are deprecated and should be sent as additional fields
                logEventInfo.Properties.Add("facility", facility);
                logEventInfo.Properties.Add("line", line);
                logEventInfo.Properties.Add("file", file);
            }
            else
            {
                jsonObject = JObject.FromObject(new GelfMessage
                {
                    Version = gelfVersion,
                    Host = Dns.GetHostName(),
                    ShortMessage = shortMessage,
                    FullMessage = logEventMessage,
                    Timestamp = logEventInfo.TimeStamp,
                    Level = GetSeverityLevel(logEventInfo.Level),
                    Facility = facility,
                    Line = line,
                    File = file,
                });
            }

            //Add any other interesting data to LogEventInfo properties
            logEventInfo.Properties.Add("LoggerName", logEventInfo.LoggerName);

            foreach (var property in ScopeContext.GetAllProperties())
            {
                if (logEventInfo.Properties.ContainsKey(property.Key) == false)
                {
                    logEventInfo.Properties.Add(property.Key, property.Value);
                }
            }

            //We will persist them "Additional Fields" according to Gelf spec
            foreach (var property in logEventInfo.Properties)
            {
                AddAdditionalField(jsonObject, property);
            }

            return jsonObject;
        }

        private static void AddAdditionalField(IDictionary<string, JToken> jObject, KeyValuePair<object, object> property)
        {
            if (property.Key == ConverterConstants.PromoteObjectPropertiesMarker)
            {
                if (property.Value != null && property.Value is object)
                {
                    try
                    {
                        var jo = JObject.FromObject(property.Value);
                        foreach (var joProp in jo)
                        {
                            AddAdditionalField(jObject, new KeyValuePair<object, object>(joProp.Key, joProp.Value));
                        }
                    }
                    catch { }
                }
                return;
            }

            var key = property.Key as string;
            if (key == null) return;

            //According to the GELF spec, libraries should NOT allow to send id as additional field (_id)
            //Server MUST skip the field because it could override the MongoDB _key field
            if (key.Equals("id", StringComparison.OrdinalIgnoreCase))
                key = "id_";

            //According to the GELF spec, additional field keys should start with '_' to avoid collision
            if (key.StartsWith("_", StringComparison.OrdinalIgnoreCase) == false)
                key = "_" + key;

            JToken value = null;
            if (property.Value != null)
                value = JToken.FromObject(property.Value);

            jObject.Add(key, value);
        }

        /// <summary>
        /// Values from SyslogSeverity enum here: http://marc.info/?l=log4net-dev&m=109519564630799
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        private static int GetSeverityLevel(LogLevel level)
        {
            if (level == LogLevel.Debug)
            {
                return 7;
            }
            if (level == LogLevel.Fatal)
            {
                return 2;
            }
            if (level == LogLevel.Info)
            {
                return 6;
            }
            if (level == LogLevel.Trace)
            {
                return 7;
            }
            if (level == LogLevel.Warn)
            {
                return 4;
            }

            return 3; //LogLevel.Error
        }

        /// <summary>
        /// Get the message details from all nested exceptions, up to EXCEPTION_MESSAGE_DEPTH in depth.
        /// </summary>
        /// <param name="ex">Exception to get details for</param>
        /// <param name="exceptionDetail">Exception message</param>
        /// <param name="stackDetail">Stacktrace with inner exceptions</param>
        private void GetExceptionMessages(Exception ex, out string exceptionDetail, out string stackDetail)
        {
            var exceptionSb = new StringBuilder();
            var stackSb = new StringBuilder();
            var nestedException = ex;
            stackDetail = null;

            int counter = 0;
            do
            {
                exceptionSb.Append(nestedException.Message + " - ");
                if (nestedException.StackTrace != null)
                    stackSb.Append(nestedException.StackTrace + "--- Inner exception stack trace ---");
                nestedException = nestedException.InnerException;
                counter++;
            }
            while (nestedException != null && counter <= EXCEPTION_MESSAGE_DEPTH);

            exceptionDetail = exceptionSb.ToString().Substring(0, exceptionSb.Length - 3);
            if (stackSb.Length > 0)
                stackDetail = stackSb.ToString().Substring(0, stackSb.Length - 35);
        }
    }
}