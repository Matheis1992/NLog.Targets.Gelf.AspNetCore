using NLog.Config;

namespace NLog.Targets.Gelf.AspNetCore.ConsoleRunner
{
    class Program
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");

            _logger.Debug("---------------------Test .NET Core ---------------------");
        }
    }
}