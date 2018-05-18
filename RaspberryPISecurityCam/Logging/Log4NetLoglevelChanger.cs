using Microsoft.Extensions.Logging;
using RaspberryPISecurityCam.Models.HomeViewModels;
using RaspberryPISecurityCam.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaspberryPISecurityCam.Logging
{
    public static class Log4NetLoglevelChanger
    {
        public static void ChangeLogLevel()
        {
            LogLevel logLevel = LogLevel.None;
            var model = SettingsHandler.GetModelFromJSON<AdminSettings>();
            if (model != null)
            {
                logLevel = model.logLevel;
            }

            var logLevelString = GetLogLevelString(logLevel);
            log4net.Repository.ILoggerRepository[] repositories = log4net.LogManager.GetAllRepositories();

            //Configure all loggers to be at the debug level.
            foreach (log4net.Repository.ILoggerRepository repository in repositories)
            {
                repository.Threshold = repository.LevelMap[logLevelString];
                log4net.Repository.Hierarchy.Hierarchy hier = (log4net.Repository.Hierarchy.Hierarchy)repository;
                log4net.Core.ILogger[] loggers = hier.GetCurrentLoggers();
                foreach (log4net.Core.ILogger logger in loggers)
                {
                    ((log4net.Repository.Hierarchy.Logger)logger).Level = hier.LevelMap[logLevelString];
                }
            }
            //Configure the root logger.
            log4net.Repository.Hierarchy.Hierarchy h = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository("log4net-default-repository");
            log4net.Repository.Hierarchy.Logger rootLogger = h.Root;
            rootLogger.Level = h.LevelMap[logLevelString];
        }

        private static string GetLogLevelString(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Critical:
                    return "FATAL";
                case LogLevel.Debug:
                case LogLevel.None:
                case LogLevel.Trace:
                    return "DEBUG";
                case LogLevel.Error:
                    return "ERROR";
                case LogLevel.Information:
                    return "INFO";
                case LogLevel.Warning:
                    return "WARN";
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }
    }
}
