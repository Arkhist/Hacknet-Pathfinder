using System;
using System.Collections.Generic;
using System.IO;
using Hacknet;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Pathfinder.Replacements;

namespace Pathfinder.BaseGameFixes
{
    [HarmonyPatch]
    internal static class MissionListingServerLoadTime
    {
        [HarmonyILManipulator]
        [HarmonyPatch(typeof(MissionListingServer), nameof(MissionListingServer.addListingsForGroup))]
        internal static void FixMissionLoadTimes(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            
            c.GotoNext(MoveType.Before, 
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(AccessTools.Field(typeof(MissionListingServer), nameof(MissionListingServer.CustomFolderLoadPath)))
            );

            c.Index += 1;
            var start = c.MarkLabel();
            
            c.GotoNext(MoveType.Before,
                x => x.MatchNop(),
                x => x.MatchNop(),
                x => x.MatchBr(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(AccessTools.Field(typeof(MissionListingServer), nameof(MissionListingServer.groupName)))
            );

            var end = c.Index;

            c.GotoLabel(start);

            c.RemoveRange(end - c.Index);

            c.Emit(OpCodes.Ldloc_1);
            c.EmitDelegate<Action<MissionListingServer, bool>>((listingDaemon, shouldGen) =>
            {
                ComputerLoader.postAllLoadedActions = (System.Action)Delegate.Combine(ComputerLoader.postAllLoadedActions, (System.Action)(
                    () =>
                    {
                        foreach (var file in new DirectoryInfo(listingDaemon.CustomFolderLoadPath).GetFiles("*.xml"))
                        {
                            OS.currentInstance.branchMissions = new List<ActiveMission>();
                            listingDaemon.addMisison(MissionLoader.LoadContentMission(listingDaemon.CustomFolderLoadPath + file));
                        }

                        if (shouldGen)
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                OS.currentInstance.branchMissions = new List<ActiveMission>();
                                listingDaemon.addMisison((ActiveMission)MissionGenerator.generate(2));
                            }
                        }
                    }));
            });
        }

        [HarmonyILManipulator]
        [HarmonyPatch(typeof(MissionHubServer), nameof(MissionHubServer.initFiles))]
        internal static void FixMissionLoadTimes2(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.Before, x => x.MatchLdfld(AccessTools.Field(typeof(OS), nameof(OS.delayer))));

            c.Index -= 2;

            c.RemoveRange(4);

            var postAllLoaded = AccessTools.Field(typeof(ComputerLoader), nameof(ComputerLoader.postAllLoadedActions));
            c.Emit(OpCodes.Ldsfld, postAllLoaded);
            c.Index += 3;
            c.Remove();
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(Delegate), nameof(Delegate.Combine), new Type[] { typeof(Delegate), typeof(Delegate) }));
            c.Emit(OpCodes.Stsfld, postAllLoaded);
        }
    }
}