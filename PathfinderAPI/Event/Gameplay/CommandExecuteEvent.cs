using Hacknet;
using HarmonyLib;

namespace Pathfinder.Event.Gameplay
{
    [HarmonyPatch]
    public class CommandExecuteEvent : PathfinderEvent
    {
        public OS Os { get; }
        public string[] Args { get; set; }
        private bool found = false;
        public bool Found
        {
            get => found;
            set => found |= value;
        }

        public CommandExecuteEvent(OS os, string[] args)
        {
            Os = os;
            Args = args;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProgramRunner), nameof(ProgramRunner.ExecuteProgram))]
        internal static bool OnCommandExecutePrefix(ref object os_object, ref string[] arguments, ref bool __result)
        {
            var commandExecuteEvent = new CommandExecuteEvent((OS)os_object, arguments);
            EventManager<CommandExecuteEvent>.InvokeAll(commandExecuteEvent);

            arguments = commandExecuteEvent.Args;
            __result = commandExecuteEvent.Found;
            return !commandExecuteEvent.Cancelled;
        }
    }
}
