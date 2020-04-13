using System;
using Pathfinder.Util;

namespace Pathfinder.Exceptions
{
    public class AttributeException : Exception
    {
        public string AttributeName { get; private set; }

        public static string GenNameForMessage(string name, string description)
            => string.IsNullOrEmpty(description)
                ? $"\"{name}\""
                : $"{description} (\"{name}\")";

        public AttributeException(string attrName, string message) :
            base(message)
        {
            AttributeName = attrName;
        }

        public AttributeException(string attrName, string message, Exception inner) :
            base(message, inner)
        {
            AttributeName = attrName;
        }
    }

    public class MissingAttributeException : AttributeException
    {
        public MissingAttributeException(string attrName, string attrDescription)
            : base(attrName, "Missing " + GenNameForMessage(attrName, attrDescription))
        { }
    }

    public class InvalidAttributeException : AttributeException
    {
        public string BaseMessage
        {
            get;
            private set;
        }

        public static string GenMessage(string attrName, string attrDescription, string message)
        {
            return "Invalid " + GenNameForMessage(attrName, attrDescription) + message.ToAppendix();
        }

        public InvalidAttributeException(string attrName, string attrDescription, string message)
            : base(attrName, GenMessage(attrName, attrDescription, message))
        {
            BaseMessage = message;
        }
    }
}