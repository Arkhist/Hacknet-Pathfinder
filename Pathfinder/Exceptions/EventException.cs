using System;
using System.Collections.Generic;
using System.Text;

namespace Pathfinder.Exceptions
{
    public class EventException : Exception
    {
        public Dictionary<string, Exception> Exceptions { get; }

        private static string GenMessage(string message, Dictionary<string, Exception> excepts)
        {
            var builder = new StringBuilder();
            builder.Append(message);
            if (excepts.Count > 0)
            {
                builder.Append(": ");
                foreach (var entry in excepts)
                    builder.Append($"\n-----> Event Listener {entry.Key} threw: {entry.Value}");
            }
            else
                builder.Append(".");

            return builder.ToString();
        }
        public EventException(string message, Dictionary<string, Exception> excepts)
            : base(GenMessage(message, excepts))
        {
            Exceptions = excepts;
        }
    }
}