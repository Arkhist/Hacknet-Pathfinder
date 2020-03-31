using System;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinder.Util
{
    public static class Logger
    {
        /// <summary>
        /// Log Levels for the Logger
        /// </summary>
        [Flags]
        public enum LogLevel
        {
            VERBOSE = 1,
            DEBUG   = 1 << 1,
            INFO    = 1 << 2,
            WARN    = 1 << 3,
            ERROR   = 1 << 4,
            FATAL   = 1 << 5,
            All     = VERBOSE | DEBUG | INFO | WARN | ERROR | FATAL,
            None    = 0,
            Default = FATAL | ERROR | WARN | INFO
        }

        const int MAX_LOG_SIZE = 100;
        public static readonly List<Type> DEFAULT_IGNORE_EVENTS = new List<Type>(new Type[] { typeof(Event.GameUpdateEvent) });
        private static List<Tuple<LogLevel, string>> logHistory = new List<Tuple<LogLevel, string>>();
        private static LogLevel showFlags = LogLevel.Default;
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:Pathfinder.Util.Logger"/> should include mod identifier.
        /// </summary>
        /// <value><c>true</c> if mod identifier should be included; otherwise, <c>false</c>.</value>
        public static bool IncludeModId { get; set; } = true;
        /// <summary>
        /// Gets the log history.
        /// </summary>
        /// <value>The log history for the past 100 log calls.</value>
        public static IList<Tuple<LogLevel, string>> LogHistory => logHistory.AsReadOnly();
        public static List<Type> IgnoreEventTypes { get; set; } = DEFAULT_IGNORE_EVENTS;

        /// <summary>
        /// Adds flag(s) to this <see cref="T:Pathfinder.Util.Logger"/>'s flags.
        /// </summary>
        /// <param name="levels">Log Level(s) to add.</param>
        public static void AddFlag(LogLevel levels) => showFlags |= levels;

        /// <summary>
        /// Sets showFlags to exact levels.
        /// </summary>
        /// <param name="levels">Log Levels to set this <see cref="T:Pathfinder.Util.Logger"/>'s showFlags to.</param>
        public static void SetFlags(LogLevel levels) => showFlags = levels;

        /// <summary>
        /// Removes a flag(s) from this <see cref="T:Pathfinder.Util.Logger"/>'s flags.
        /// </summary>
        /// <param name="levels">Log Level(s) to remove.</param>
        public static void RemoveFlag(LogLevel levels) => showFlags &= ~levels;

        /// <summary>
        /// Determines whether this <see cref="T:Pathfinder.Util.Logger"/> has the flag(s).
        /// </summary>
        /// <returns><c>true</c>, has flag, <c>false</c> otherwise.</returns>
        /// <param name="level">Log Level(s) to test for.</param>
        public static bool HasFlag(LogLevel level) => showFlags.HasFlag(level);

        /// <summary>
        /// Logs the specified level and input.
        /// </summary>
        /// <param name="level">Log Level to log for.</param>
        /// <param name="input">Any stringable inputs. (if larger then one, must be in standard C# String.Format format)</param>
        public static void Log(LogLevel level, params object[] input)
        {
            if (!HasFlag(level)) return;
            Tuple<LogLevel, string> t;
            var prefix = IncludeModId ? Utility.ActiveModId + " " : "";
            switch (input.Length)
            {
                case 0: return;
                case 1:
                    t = new Tuple<LogLevel, string>(level, string.Format("{0}[{1}]: {2}", prefix, level, input[0]));
                    break;
                default:
                    t = new Tuple<LogLevel, string>(level, string.Format("{0}[{1}]: {2}", prefix, level,
                                                                         string.Format(input[0].ToString(),
                                                                                       input.Skip(1).ToArray())));
                    break;
            }
            if (logHistory.Count >= MAX_LOG_SIZE)
                logHistory.RemoveRange(0, logHistory.Count - MAX_LOG_SIZE);
            logHistory.Add(t);
            Console.WriteLine(t.Item2);
        }

        /// <summary>
        /// Logs on <see cref="T:Pathfinder.Util.Logger.LogLevel.VERBOSE"/> the specified input.
        /// </summary>
        /// <param name="input">Any stringable inputs. (if larger then one, must be in standard C# String.Format format)</param>
        public static void Verbose(params object[] input) => Log(LogLevel.VERBOSE, input);

        /// <summary>
        /// Logs on <see cref="T:Pathfinder.Util.Logger.LogLevel.DEBUG"/> the specified input.
        /// </summary>
        /// <param name="input">Any stringable inputs. (if larger then one, must be in standard C# String.Format format)</param>
        public static void Debug(params object[] input) => Log(LogLevel.DEBUG, input);

        /// <summary>
        /// Logs on <see cref="T:Pathfinder.Util.Logger.LogLevel.INFO"/> the specified input.
        /// </summary>
        /// <param name="input">Any stringable inputs. (if larger then one, must be in standard C# String.Format format)</param>
        public static void Info(params object[] input) => Log(LogLevel.INFO, input);

        /// <summary>
        /// Logs on <see cref="T:Pathfinder.Util.Logger.LogLevel.WARN"/> the specified input.
        /// </summary>
        /// <param name="input">Any stringable inputs. (if larger then one, must be in standard C# String.Format format)</param>
        public static void Warn(params object[] input) => Log(LogLevel.WARN, input);

        /// <summary>
        /// Logs on <see cref="T:Pathfinder.Util.Logger.LogLevel.ERROR"/> the specified input.
        /// </summary>
        /// <param name="input">Any stringable inputs. (if larger then one, must be in standard C# String.Format format)</param>
        public static void Error(params object[] input) => Log(LogLevel.ERROR, input);

        /// <summary>
        /// Logs on <see cref="T:Pathfinder.Util.Logger.LogLevel.FATAL"/> the specified input.
        /// </summary>
        /// <param name="input">Any stringable inputs. (if larger then one, must be in standard C# String.Format format)</param>
        public static void Fatal(params object[] input) => Log(LogLevel.FATAL, input);
    }
}
