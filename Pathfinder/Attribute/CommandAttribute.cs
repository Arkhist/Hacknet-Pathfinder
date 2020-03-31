using System;
namespace Pathfinder.Attribute
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CommandAttribute : AbstractPathfinderAttribute
    {
        public string Key { get; set; }
        public string Description { get; set; }
        public bool Autocomplete { get; set; }

        public CommandAttribute(string key = null, string description = null, bool autocomplete = false, Type mod = null)
            : base(mod)
        {
            Key = key;
            Description = description;
            Autocomplete = autocomplete;
        }
    }
}
