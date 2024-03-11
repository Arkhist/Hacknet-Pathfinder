using System.Reflection;
using Hacknet;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Pathfinder.BaseGameFixes;

[HarmonyPatch]
internal static class HHBS
{
    [Util.Initialize]
    internal static void Initialize()
    {
        var a = HostileHackerBreakinSequence.BaseDirectory;
    }

    private static readonly ConstructorInfo HHBSCctor = typeof(HostileHackerBreakinSequence).TypeInitializer;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(OS), MethodType.Constructor, new Type[0])]
    internal static void ReloadHHBSOnOSCtorPrefix() => HHBSCctor.Invoke(null, null);

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(HostileHackerBreakinSequence), nameof(HostileHackerBreakinSequence.GetBaseDirectory))]
    internal static void FixPathCombineOrderIL(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        c.GotoNext(MoveType.AfterLabel,
            x => x.MatchLdsfld(AccessTools.Field(typeof(Settings), nameof(Settings.IsInExtensionMode)))
        );

        var combine = AccessTools.Method(typeof(Path), nameof(Path.Combine), [typeof(string), typeof(string)]);
            
        c.Emit(OpCodes.Ldloc_0);
        c.Emit(OpCodes.Ldstr, "HacknetPathfinder");
        c.Emit(OpCodes.Call, combine);
        c.Emit(OpCodes.Ldstr, "Accounts");
        c.Emit(OpCodes.Call, combine);
        c.Emit(OpCodes.Stloc_0);

        c.GotoNext(MoveType.Before,
            x => x.MatchLdstr("Hacknet")
        );

        c.Next.Operand = "HHBS";
    }
}
