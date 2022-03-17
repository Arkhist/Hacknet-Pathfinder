using BepInEx.Logging;

namespace Pathfinder;

internal static class Logger
{
    internal static ManualLogSource LogSource;

    internal static void Log(LogLevel severity, object msg) => LogSource.Log(severity, msg); 
}