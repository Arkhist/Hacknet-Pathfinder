using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using Hacknet;
using Microsoft.Xna.Framework;
using System.Xml;
using Hacknet.Mission;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Util;
using Pathfinder.Daemon;
using Pathfinder.Mission;

namespace ExampleMod2
{
    [BepInPlugin("com.Windows10CE.Example", "Example", "1.0.0")]
    [BepInDependency(Pathfinder.PathfinderAPIPlugin.ModGUID, BepInDependency.DependencyFlags.HardDependency)]
    public class ExampleModPlugin2 : BepInEx.Hacknet.HacknetPlugin
    {
        public override bool Load()
        {
            base.HarmonyInstance.PatchAll(typeof(PatchClass2));

            Pathfinder.Executable.ExecutableManager.RegisterExecutable<TestExe>("#PF_TEST_EXE#");
            Pathfinder.Port.PortManager.RegisterPort("Example port", 50);
            Pathfinder.Daemon.DaemonManager.RegisterDaemon<TestDaemon>();
            Pathfinder.Command.CommandManager.RegisterCommand("pathfinder", TestCommand);
            Pathfinder.Mission.GoalManager.RegisterGoal<TestGoal>("resetIP");

            return true;
        }

        public static void TestCommand(OS os, string[] args)
        {
            os.write("pathfinder is here!");
            os.write("Arguments passed in: " + string.Join(" ", args));
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
            drawTarget();
            drawOutline();
            Hacknet.Gui.TextItem.doLabel(new Vector2(Bounds.Center.X, Bounds.Center.Y), "blue text", new Color(255, 0, 0));
        }

        private float total = 0f;
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

    public class TestDaemon : BaseDaemon
    {
        public TestDaemon(Computer computer, string serviceName, OS opSystem) : base(computer, serviceName, opSystem) { }

        public override string Identifier => "Test Daemon! :)";

        [XMLStorage]
        public string DisplayString;

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            base.draw(bounds, sb);

            var center = os.display.bounds.Center;
            Hacknet.Gui.TextItem.doLabel(new Vector2(center.X, center.Y), DisplayString, Color.Aquamarine);
        }
    }

    public class TestGoal : InitializableGoal
    {
        [XMLStorage]
        public string NodeID;

        public string OriginalIP;

        public override void Initialize()
        {
            OriginalIP = Programs.getComputer(OS.currentInstance, NodeID).ip;
        }

        public override bool isComplete(List<string> additionalDetails = null)
        {
            return Programs.getComputer(OS.currentInstance, NodeID).ip != OriginalIP;
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
    }
}
