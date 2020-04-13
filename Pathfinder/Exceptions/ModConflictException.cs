using System;
using Pathfinder.Util;

namespace Pathfinder.Exceptions
{
    public class ModConflictException : Exception
    {
        public string OldModId { get; private set; }
        public string NewModId { get; private set; }
        public string BaseMessage { get; private set; }

        public static string GenMessage(string oldModId, string newModId, string message)
        {
            return $"Conflict between '{oldModId}' and '{newModId}': {message}";
        }

        public ModConflictException(string oldModId, string message) :
            base(GenMessage(oldModId, Utility.ActiveModId, message))
        {
            OldModId = oldModId;
            NewModId = Utility.ActiveModId;
            BaseMessage = message;
        }

        public ModConflictException(string oldModId, string message, Exception inner) :
            base(GenMessage(oldModId, Utility.ActiveModId, message), inner)
        {
            OldModId = oldModId;
            NewModId = Utility.ActiveModId;
            BaseMessage = message;
        }

        public ModConflictException(string oldModId, string newModId, string message) :
            base(GenMessage(oldModId, newModId, message))
        {
            OldModId = oldModId;
            NewModId = newModId;
            BaseMessage = message;
        }

        public ModConflictException(string oldModId, string newModId, string message, Exception inner) :
            base(GenMessage(oldModId, newModId, message), inner)
        {
            OldModId = oldModId;
            NewModId = newModId;
            BaseMessage = message;
        }
    }
}
