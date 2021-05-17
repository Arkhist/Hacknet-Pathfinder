using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using BepInEx;
using HarmonyLib;

namespace ExampleMod2
{
    [BepInPlugin("com.Windows10CE.Example", "Example", "1.0.0")]
    public class ExampleModPlugin2 : BepInEx.Hacknet.HacknetPlugin
    {
        public override bool Load()
        {
            base.HarmonyInstance.PatchAll(typeof(PatchClass2));

            return true;
        }
    }

    [HarmonyPatch]
    public static class PatchClass2
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Hacknet.MainMenu), nameof(Hacknet.MainMenu.Draw))]
        public static void MainMenuTextPatch()
        {
            Hacknet.GuiData.startDraw();
            Hacknet.Gui.Button.doButton(3473249, 5, 5, 30, 600, "bruh", Microsoft.Xna.Framework.Color.BlueViolet);
            Hacknet.GuiData.endDraw();
        }
    }
}
