using System;
using System.Collections.Generic;
using System.Xml;
using Hacknet;
using Hacknet.Mission;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Pathfinder.Event.Loading.Content
{
    [HarmonyPatch]
    public class LoadGoalEvent : PathfinderEvent
    {
        public XmlReader Reader { get; }
        public List<MisisonGoal> GoalList { get; }
        internal static string wtfGoalName = "";
        public string GoalName { get; }

        private bool goalFound = false;
        public bool GoalFound
        {
            get => goalFound;
            set => goalFound |= value;
        }

        public LoadGoalEvent(XmlReader reader, List<MisisonGoal> goalList, string goalName)
        {
            Reader = reader;
            GoalList = goalList;
            GoalName = goalName;
        }

        [HarmonyILManipulator]
        [HarmonyPatch(typeof(ComputerLoader), nameof(ComputerLoader.readMission))]
        internal static void GoalLoadIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.After,
                x => x.MatchLdstr("UNKNOWN")
            );
            c.GotoNext(MoveType.Before,
                x => x.MatchNop(),
                x => x.MatchNop(),
                x => x.MatchLdloc(1),
                x => x.MatchCallvirt(AccessTools.Method(typeof(XmlReader), nameof(XmlReader.Read)))
            );

            c.Emit(OpCodes.Ldloc_1);

            foreach (var label in c.IncomingLabels)
                label.Target = c.Prev;
            
            c.Emit(OpCodes.Ldloc_3);
            c.Emit(OpCodes.Ldloc, 14);
            c.Emit(OpCodes.Stsfld, AccessTools.Field(typeof(LoadGoalEvent), nameof(wtfGoalName)));
            c.EmitDelegate<Action<XmlReader, List<MisisonGoal>>>((reader, goalList) =>
            {
                var loadGoalEvent = new LoadGoalEvent(reader, goalList, wtfGoalName);
                EventManager<LoadGoalEvent>.InvokeAll(loadGoalEvent);
            });
        }
    }
}
