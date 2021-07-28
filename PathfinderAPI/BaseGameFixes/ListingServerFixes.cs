using Hacknet;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Pathfinder.BaseGameFixes
{
    [HarmonyPatch]
    internal static class ListingServerFixes
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MissionListingServer), nameof(MissionListingServer.addMisison))]
        internal static bool SkipNullMissions(ActiveMission m) => m != null;

        [HarmonyILManipulator]
        [HarmonyPatch(typeof(SAAddMissionToHubServer), nameof(SAAddMissionToHubServer.Trigger))]
        internal static void NullCheckOnAssignmentTag(ILContext il)
        {
            var c = new ILCursor(il);
            
            c.GotoNext(x => x.MatchCallvirt(AccessTools.Method(typeof(string), nameof(string.ToLower))));
            
            var start = c.DefineLabel(); // IL_0123
            var end   = c.DefineLabel(); // IL_0132

            c.Emit(OpCodes.Dup);
            c.Emit(OpCodes.Brfalse, start);
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.Br, end);
            
            start.Target = c.Next;

            c.GotoNext(x => x.MatchCallvirt(AccessTools.Method(typeof(MissionListingServer), nameof(MissionListingServer.addMisison))));

            end.Target = c.Next;
        }
    }
}