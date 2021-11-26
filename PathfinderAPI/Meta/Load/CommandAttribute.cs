using System;
using System.Reflection;
using BepInEx.Hacknet;
using Hacknet;
using Pathfinder.Command;

namespace Pathfinder.Meta.Load
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class CommandAttribute : BaseAttribute
    {
        public string CommandName { get; }
        public bool AddAutocomplete { get; set; }
        public bool CaseSensitive { get; set; }

        public CommandAttribute(string commandName, bool addAutocomplete = true, bool caseSensitive = false)
        {
            CommandName = commandName;
            AddAutocomplete = addAutocomplete;
            CaseSensitive = caseSensitive;
        }

        protected internal override void CallOn(HacknetPlugin plugin, MemberInfo targettedInfo)
        {
            var methodInfo = (MethodInfo)targettedInfo;
            var commandAction = (Action<OS, string[]>)methodInfo.CreateDelegate(typeof(Action<OS, string[]>));
            CommandManager.RegisterCommand(CommandName, commandAction, AddAutocomplete, CaseSensitive);
        }
    }
}