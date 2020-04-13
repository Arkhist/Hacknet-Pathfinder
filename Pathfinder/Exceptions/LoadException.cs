using System;
using Pathfinder.Util;

namespace Pathfinder.Exceptions
{
    public class LoadException : Exception
    {
        public LoadException(string thing, string message)
            : base("Failed to load " + thing + message.ToAppendix(" "))
        {
        }

        public LoadException(string thing, string message, Exception inner)
            : base("Failed to load " + thing + message.ToAppendix(" "), inner)
        {
        }

    }

    public class ActionLoadException : LoadException
    {
        public string ActionName
        {
            get;
        }

        public ActionLoadException(string actionName, string message)
            : base("Action", "'" + actionName + "'" + message.ToAppendix())
        {
            ActionName = actionName;
        }

        public ActionLoadException(string actionName, string message, Exception inner)
            : base("Action", "'" + actionName + "'" + message.ToAppendix(), inner)
        {
            ActionName = actionName;
        }
    }


    public class ConditionLoadException : LoadException
    {
        public string ConditionName
        {
            get;
        }

        public ConditionLoadException(string actionName, string message)
            : base("Condition", "'" + actionName + "'" + message.ToAppendix())
        {
            ConditionName = actionName;
        }

        public ConditionLoadException(string actionName, string message, Exception inner)
            : base("Condition", "'" + actionName + "'" + message.ToAppendix(), inner)
        {
            ConditionName = actionName;
        }
    }
}