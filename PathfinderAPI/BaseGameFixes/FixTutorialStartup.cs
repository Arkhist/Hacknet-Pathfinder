using Hacknet;
using Hacknet.Extensions;
using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace Pathfinder.BaseGameFixes;

[HarmonyPatch]
public static class FixTutorialStartup
{
    [HarmonyILManipulator]
    [HarmonyPatch(typeof(Programs), nameof(Programs.firstTimeInit))]
    internal static void FirstTimeInitSetStartingMissionForTutorial(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        c.GotoNext(MoveType.After,
            x => x.MatchLdstr("Launching Tutorial...")
        );

        c.Emit(OpCodes.Ldarg_1);
        c.EmitDelegate<Action<OS>>(os =>
        {
            if (ExtensionLoader.ActiveExtensionInfo.StartingMissionPath != null)
                os.currentMission = (ActiveMission)ComputerLoader.readMission(ExtensionLoader.ActiveExtensionInfo.FolderPath + "/" + ExtensionLoader.ActiveExtensionInfo.StartingMissionPath);
        });
    }
    [HarmonyILManipulator]
    [HarmonyPatch(typeof(AdvancedTutorial), nameof(AdvancedTutorial.Killed))]
    internal static void AdvancedTutorialKilledNullMissionFix(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        // os.currentMission.sendEmail(os);
        c.GotoNext(MoveType.After,
            x => x.MatchNop(),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld(AccessTools.Field(typeof(Module), nameof(Module.os))),
            x => x.MatchLdfld(AccessTools.Field(typeof(OS), nameof(OS.currentMission))),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld(AccessTools.Field(typeof(Module), nameof(Module.os))),
            x => x.MatchCallvirt(AccessTools.Method(typeof(ActiveMission), nameof(ActiveMission.sendEmail)))
        );
        // os.Flags.AddFlag("TutorialComplete");
        ILLabel nextLine = c.MarkLabel();
        c.Index -= 3;
        ILLabel continueThisLine = c.MarkLabel();
        c.MoveBeforeLabels();

        c.Emit(OpCodes.Dup);
        c.Emit(OpCodes.Ldnull);
        c.Emit(OpCodes.Ceq);
        c.Emit(OpCodes.Brfalse, continueThisLine);
        c.Emit(OpCodes.Pop);
        c.Emit(OpCodes.Br, nextLine);
    }
}
