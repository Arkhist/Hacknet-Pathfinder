using System;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinder.Util
{
    public static class Logger
    {
        [Flags]
        public enum LogLevel
        {
            VERBOSE = 1,
            DEBUG = 2,
            INFO = 4,
            WARN = 8,
            ERROR = 16,
            FATAL = 32,
            All = VERBOSE | DEBUG | INFO | WARN | ERROR | FATAL,
            None = 0,
            Default = FATAL | ERROR | WARN | INFO
        }

        const int MAX_LOG_SIZE = 100;

        private static List<Tuple<LogLevel, string>> logHistory = new List<Tuple<LogLevel, string>>();
        private static LogLevel showFlags = LogLevel.Default;

        public static bool IncludeModId { get; set; } = true;
        public static IList<Tuple<LogLevel, string>> LogHistory => logHistory.AsReadOnly();

        public static void SetFlag(LogLevel level)
        {
            showFlags |= level;
        }

        public static void RemoveFlag(LogLevel level)
        {
            showFlags &= ~level;
        }

        public static bool HasFlag(LogLevel level)
        {
            return showFlags.HasFlag(level);
        }

        public static void Log(LogLevel level, params object[] input)
        {
            Tuple<LogLevel, string> t;
            var prefix = IncludeModId ? Utility.GetPreviousStackFrameIdentity() + " " : "";
            if (input.Length > 1)
                t = new Tuple<LogLevel, string>(level,
                                                String.Format("{0}[{1}]: {2}",
                                                              prefix,
                                                              level,
                                                              String.Format(input[0].ToString(), input.Skip(1).ToArray())));
            else
                t = new Tuple<LogLevel, string>(level,
                                                String.Format("{0}[{1}]: {2}",
                                                              prefix,
                                                              level, input[0]));
            if (logHistory.Count >= MAX_LOG_SIZE)
                logHistory.RemoveRange(0, logHistory.Count - MAX_LOG_SIZE);
            logHistory.Add(t);
            if (HasFlag(level))
                Console.WriteLine(t.Item2);
        }

        public static void Verbose(params object[] input)
        {
            Log(LogLevel.VERBOSE, input);
        }

        public static void Debug(params object[] input)
        {
            Log(LogLevel.DEBUG, input);
        }

        public static void Info(params object[] input)
        {
            Log(LogLevel.INFO, input);
        }

        public static void Warn(params object[] input)
        {
            Log(LogLevel.WARN, input);
        }

        public static void Error(params object[] input)
        {
            Log(LogLevel.ERROR, input);
        }

        public static void Fatal(params object[] input)
        {
            Log(LogLevel.FATAL, input);
        }
    }
}
