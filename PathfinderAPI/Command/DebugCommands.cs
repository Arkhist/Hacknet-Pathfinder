using System.Linq;
using Hacknet;
using Pathfinder.Util;

namespace Pathfinder.Command
{
    internal static class DebugCommands
    {
        internal static void AddCommands()
        {
            CommandManager.RegisterCommand("loadmission", LoadMission);
            CommandManager.RegisterCommand("loadactions", LoadActions);
        }

        private static void LoadMission(OS os, string[] args)
        {
            os.currentMission = Replacements.MissionLoader.LoadContentMission(string.Join(" ", args.Skip(1)).ContentFilePath());
            os.currentMission.sendEmail(os);
        }

        private static void LoadActions(OS os, string[] args)
        {
            RunnableConditionalActions.LoadIntoOS(string.Join(" ", args.Skip(1)), os);
        }
    }
}