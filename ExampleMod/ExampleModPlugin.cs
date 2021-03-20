using System;
using MonoMod.Cil;
using HarmonyLib;

namespace ExampleMod
{
    [BepInEx.BepInPlugin("com.Windows10CE.Example", "Example", "1.0.0")]
    public class ExampleModPlugin : BepInEx.Hacknet.HacknetPlugin
    {
        public override bool Load()
        {
            base.HarmonyInstance.PatchAll(typeof(ExampleModPlugin).Assembly);

            return true;
        }
    }

    [HarmonyPatch]
    public static class PatchClass
    {
        [HarmonyILManipulator]
        [HarmonyPatch(typeof(Hacknet.MainMenu), "DrawBackgroundAndTitle")]
        public static void MainMenuTextPatch(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.Before,
                x => x.MatchCall(AccessTools.Method(typeof(string), "Concat", new Type[] { typeof(string[]) })),
                x => x.MatchStloc(out _)
            );

            c.Index += 1;

            c.EmitDelegate<Func<string, string>>((subtitle) => subtitle += " AHAHAHAHAHA IT WORKSSSSS but for a second time");
        }
    }
}
