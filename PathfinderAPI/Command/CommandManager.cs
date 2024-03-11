using System.Reflection;
using System.Runtime.CompilerServices;
using Hacknet;
using HarmonyLib;
using Pathfinder.Event;
using Pathfinder.Event.Gameplay;
using Pathfinder.Event.Pathfinder;
using Pathfinder.Util;

namespace Pathfinder.Command;

[HarmonyPatch]
public static class CommandManager
{
    private struct CustomCommand
    {
        public string Name;
        public Action<OS, string[]> CommandAction;
        public bool Autocomplete;
        public bool CaseSensitive;
    }
        
    private static readonly AssemblyAssociatedList<CustomCommand> CustomCommands = new AssemblyAssociatedList<CustomCommand>();

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
            if (string.Equals(command.Name, args.Args[0], command.CaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase))
            {
                custom = command.CommandAction;
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

    [Initialize]
    private static void Initialize()
    {
        ProgramList.init();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ProgramList), nameof(ProgramList.init))]
    private static bool ProgramListInitPrefix()
    {
        RebuildAutoComplete();
        return false;
    }

    private static void RebuildAutoComplete()
    {
        foreach (var command in CustomCommands.AllItems)
        {
            if (command.Autocomplete && !ProgramList.programs.Contains(command.Name))
                ProgramList.programs.Add(command.Name);
        }
        ProgramList.programs = EventManager<BuildAutocompletesEvent>.InvokeAll(new BuildAutocompletesEvent(ProgramList.programs)).Autocompletes;
    }

    private static void OnPluginUnload(Assembly pluginAsm)
    {
        if (CustomCommands.RemoveAssembly(pluginAsm, out _))
            RebuildAutoComplete();
    }
        
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void RegisterCommand(string commandName, Action<OS, string[]> handler, bool addAutocomplete = true, bool caseSensitive = false)
    {
        var pluginAsm = Assembly.GetCallingAssembly();

        if (CustomCommands.AllItems.Any(x => x.Name == commandName))
            throw new ArgumentException($"Command {commandName} has already been registered!", nameof(commandName));
                
        CustomCommands.Add(new CustomCommand
        {
            Name = commandName,
            CommandAction = handler,
            Autocomplete = addAutocomplete,
            CaseSensitive = caseSensitive
        }, pluginAsm);
            
        if (addAutocomplete)
            RebuildAutoComplete();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void UnregisterCommand(string commandName, Assembly pluginAsm = null)
    {
        CustomCommands.RemoveAll(x => x.Name == commandName, pluginAsm ?? Assembly.GetCallingAssembly());
    }
}