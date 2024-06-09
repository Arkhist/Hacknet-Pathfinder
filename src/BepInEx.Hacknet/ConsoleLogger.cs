using BepInEx.Logging;
using BepInEx.Configuration;

namespace BepInEx.Hacknet;

internal class ConsoleLogger : ILogListener
{
    protected static readonly ConfigEntry<LogLevel> ConfigConsoleDisplayedLevel = ConfigFile.CoreConfig.Bind(
        "Logging.Console", "LogLevels",
        LogLevel.Fatal | LogLevel.Error | LogLevel.Warning | LogLevel.Message | LogLevel.Info,
        "Only displays the specified log levels in the console output.");

    protected static readonly ConfigEntry<bool> BackupConsoleEnabled = ConfigFile.CoreConfig.Bind(
        "Logging.Console", "BackupConsole",
        false,
        "Enables backup console output in case the normal one fails.");

    public void Dispose() { }

    public void LogEvent(object sender, LogEventArgs eventArgs)
    {
        if (!BackupConsoleEnabled.Value || (eventArgs.Level & ConfigConsoleDisplayedLevel.Value) == 0)
            return;

        Console.ForegroundColor = eventArgs.Level.GetConsoleColor();
        Console.Out.Write(eventArgs.ToStringLine());
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    public LogLevel LogLevelFilter => ConfigConsoleDisplayedLevel.Value;
}