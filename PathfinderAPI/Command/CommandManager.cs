using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Hacknet;
using Pathfinder.Event;
using Pathfinder.Event.Gameplay;

namespace Pathfinder.Command
{
    public static class CommandManager
    {
        private static readonly Dictionary<string, Action<OS, string[]>> handlers = new Dictionary<string, Action<OS, string[]>>();
        private static readonly Dictionary<Assembly, List<string>> asmToCommands = new Dictionary<Assembly, List<string>>();

        static CommandManager()
        {
            EventManager<CommandExecuteEvent>.AddHandler(OnCommandExecute);
            EventManager.onPluginUnload += OnPluginUnload;
        }

        private static void OnCommandExecute(CommandExecuteEvent args)
        {
            if (handlers.TryGetValue(args.Args[0], out Action<OS, string[]> command))
            {
                args.Found = true;
                args.Cancelled = true;

                command(args.Os, args.Args);
            }
        }

        private static void OnPluginUnload(Assembly pluginAsm)
        {
            if (asmToCommands.TryGetValue(pluginAsm, out List<string> commands))
            {
                foreach (var command in commands)
                    handlers.Remove(command);
                asmToCommands.Remove(pluginAsm);
            }
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RegisterCommand(string commandName, Action<OS, string[]> handler)
        {
            var pluginAsm = Assembly.GetCallingAssembly();

            if (handlers.ContainsKey(commandName))
                throw new ArgumentException($"Command {commandName} has already been registered!", nameof(commandName));
                
            if (!asmToCommands.ContainsKey(pluginAsm))
                asmToCommands.Add(pluginAsm, new List<string>());
            asmToCommands[pluginAsm].Add(commandName);
            
            handlers.Add(commandName, handler);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void UnregisterCommand(string commandName)
        {
            if (!handlers.ContainsKey(commandName))
                return;
            handlers.Remove(commandName);
            asmToCommands[Assembly.GetCallingAssembly()].Remove(commandName);
        }
    }
}
