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
            // Adds a local (index 8) of type 'string' to the method
            il.Body.Variables.Add(new VariableDefinition(
                il.Module.ImportReference(typeof(string))));
            
            var c = new ILCursor(il);
            
            c.GotoNext(x => x.MatchCallvirt(AccessTools.Method(typeof(string), nameof(string.ToLower))));
            
            var start = c.DefineLabel(); // IL_0123
            var end   = c.DefineLabel(); // IL_0132

            var tagLoc = 8; // The local in which AssignmentTag is stored
            
            c.Emit(OpCodes.Stloc, tagLoc);
            c.Emit(OpCodes.Ldloc, tagLoc);
            c.Emit(OpCodes.Ldnull);
            c.Emit(OpCodes.Ceq);
            c.Emit(OpCodes.Brfalse, start);
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.Br, end);

            // Target an emitted instruction that comes before the String::ToLower call
            start.Target = c.Emit(OpCodes.Ldloc, tagLoc).Prev;

            c.GotoNext(x => x.MatchCallvirt(AccessTools.Method(typeof(MissionListingServer), nameof(MissionListingServer.addMisison))));

            end.Target = c.Next;
        }
    }
}