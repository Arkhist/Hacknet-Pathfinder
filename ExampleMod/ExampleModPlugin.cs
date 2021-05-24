using System;
using System.Linq;
using BepInEx;
using HarmonyLib;
using Hacknet;
using Microsoft.Xna.Framework;

namespace ExampleMod2
{
    [BepInPlugin("com.Windows10CE.Example", "Example", "1.0.0")]
    [BepInDependency(Pathfinder.PathfinderAPIPlugin.ModGUID, BepInDependency.DependencyFlags.HardDependency)]
    public class ExampleModPlugin2 : BepInEx.Hacknet.HacknetPlugin
    {
        public override bool Load()
        {
            base.HarmonyInstance.PatchAll(typeof(PatchClass2));

            Pathfinder.Executable.ExecutableHandler.RegisterExecutable(typeof(TestExe), "#A#");
            Pathfinder.Port.PortHandler.AddPort("sex2", 50);

            return true;
        }
    }

    public class TestExe : Pathfinder.Executable.BaseExecutable
    {
        public override string GetIdentifier() => "Some";

        public TestExe(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args) { this.ramCost = 761; }

        public override void LoadContent()
        {
            base.LoadContent();
            Programs.getComputer(os, targetIP).hostileActionTaken();
            os.write(string.Join(" ", Args));
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            //drawFrame();
            drawTarget();
            drawOutline();
            Hacknet.Gui.TextItem.doLabel(new Vector2(Bounds.Center.X, Bounds.Center.Y), "sex!", new Color(255, 0, 0));
        }

        float total = 0f;
        public override void Update(float t)
        {
            base.Update(t);
            
            total += t;
            if (total > 2.5f)
            {
                isExiting = true;
                Programs.getComputer(os, targetIP).openPort(50, os.thisComputer.ip);
            }
        }
    }

    [HarmonyPatch]
    public static class PatchClass2
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenu), nameof(MainMenu.Draw))]
        public static void MainMenuTextPatch()
        {
            GuiData.startDraw();
            Hacknet.Gui.Button.doButton(3473249, 5, 5, 30, 600, "bruh", Color.BlueViolet);
            GuiData.endDraw();
        }

        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Game1), MethodType.Constructor)]
        public static void Uncap(ref Game1 __instance)
        {
            __instance.graphics.SynchronizeWithVerticalRetrace = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(OS), nameof(OS.LoadContent))]
        public static void StopInit(OS __instance)
        {
            __instance.thisComputer.daemons.Add(new PorthackHeartDaemon(__instance.thisComputer, __instance));
            __instance.thisComputer.daemons.Last().initFiles();
            __instance.thisComputer.daemons.Last().registerAsDefaultBootDaemon();
            (__instance.thisComputer.daemons.Last() as PorthackHeartDaemon).BreakHeart();
            __instance.display.visible = true;
            Programs.connect(new string[] { __instance.thisComputer.ip }, __instance);
            var ph = new PortHackExe(new Rectangle(__instance.ram.bounds.X, __instance.ram.bounds.Y + RamModule.contentStartOffset, RamModule.MODULE_WIDTH, (int)OS.EXE_MODULE_HEIGHT), __instance);
            ph.hasCheckedForheart = true;
            ph.progress = 0.6f;
            __instance.addExe(ph);

            Console.WriteLine("PortHack done");
        }

        static bool first = true;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(OS), nameof(OS.Draw))]
        public static void ShowTime()
        {
            if (first)
                Console.WriteLine("first frame start");
            first = false;
        }
        */
    }
}
