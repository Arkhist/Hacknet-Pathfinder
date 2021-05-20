using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.Logging;
using BepInEx.Configuration;

namespace BepInEx.Hacknet
{
    internal class ConsoleLogger : BepInEx.Logging.ILogListener
    {
        protected static readonly ConfigEntry<LogLevel> ConfigConsoleDisplayedLevel = ConfigFile.CoreConfig.Bind(
         "Logging.Console", "LogLevels",
         LogLevel.Fatal | LogLevel.Error | LogLevel.Warning | LogLevel.Message | LogLevel.Info,
         "Only displays the specified log levels in the console output.");

        public void Dispose() { }

        public void LogEvent(object sender, LogEventArgs eventArgs)
        {
            if ((eventArgs.Level & ConfigConsoleDisplayedLevel.Value) == 0)
                return;

            Console.ForegroundColor = eventArgs.Level.GetConsoleColor();
            Console.Out.Write(eventArgs.ToStringLine());
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
