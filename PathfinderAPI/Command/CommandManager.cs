using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Hacknet;
using Pathfinder.Event;
using Pathfinder.Event.Gameplay;
using Pathfinder.Util;

namespace Pathfinder.Command
{
    public static class CommandManager
    {
        private static readonly AssemblyAssociatedList<KeyValuePair<string, Action<OS, string[]>>> CustomCommands = new AssemblyAssociatedList<KeyValuePair<string, Action<OS, string[]>>>();

        static CommandManager()
        {
            EventManager<CommandExecuteEvent>.AddHandler(OnCommandExecute);
            EventManager.onPluginUnload += OnPluginUnload;
        }

        private static void OnCommandExecute(CommandExecuteEvent args)
        {
            Action<OS, string[]> custom = null;
            foreach (var command in CustomCommands.AllItems)
            {
                if (command.Key == args.Args[0])
                {
                    custom = command.Value;
                    break;
                }
            }

            if (custom != null)
            {
                args.Found = true;
                args.Cancelled = true;
                
                custom(args.Os, args.Args);
            }
        }

        private static void OnPluginUnload(Assembly pluginAsm)
        {
            CustomCommands.RemoveAssembly(pluginAsm, out _);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RegisterCommand(string commandName, Action<OS, string[]> handler)
        {
            var pluginAsm = Assembly.GetCallingAssembly();

            if (CustomCommands.AllItems.Any(x => x.Key == commandName))
                throw new ArgumentException($"Command {commandName} has already been registered!", nameof(commandName));
                
            CustomCommands.Add(new KeyValuePair<string, Action<OS, string[]>>(commandName, handler), pluginAsm);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void UnregisterCommand(string commandName, Assembly pluginAsm = null)
        {
            CustomCommands.RemoveAll(x => x.Key == commandName, pluginAsm ?? Assembly.GetCallingAssembly());
        }
    }
}
