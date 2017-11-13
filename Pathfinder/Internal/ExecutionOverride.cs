using System;
using System.Threading;
using Hacknet;
using Hacknet.Extensions;
using Microsoft.Xna.Framework.Input;
using Pathfinder.Event;
using Pathfinder.Game.Computer;
using Pathfinder.Game.OS;
using Pathfinder.GUI;
using Pathfinder.Util;

namespace Pathfinder.Internal
{
    static class ExecutionOverride
    {
        public static void OverridePortHack(ExecutablePortExecuteEvent e)
        {
            if (e[0].ToLower() == "porthack")
            {
                e.IsCancelled = true;
                var os = e.OS;
                var cComp = os.connectedComp;
                bool canRun = false;
                bool firewallActive = false;
                if (cComp != null)
                {
                    int num2 = 0;
                    for (int i = 0; i < cComp.portsOpen.Count; i++)
                        num2 += os.connectedComp.portsOpen[i];
                    foreach (var p in cComp.GetModdedPortList())
                        num2 += p.Unlocked ? 1 : 0;
                    canRun |= num2 > cComp.portsNeededForCrack;
                    if (cComp.firewall != null && !cComp.firewall.solved)
                    {
                        firewallActive |= canRun;
                        canRun = false;
                    }
                }
                if (canRun)
                    os.addExe(new PortHackExe(e.Destination, os));
                else if (firewallActive)
                    os.WriteF("{0} -\n{1}",
                              LocaleTerms.Loc("Target Machine Rejecting Syndicated UDP Traffic"),
                              LocaleTerms.Loc("Bypass Firewall to allow unrestricted traffic"));
                else
                    os.WriteF("{0} - \n{1}\n",
                              LocaleTerms.Loc("Too Few Open Ports to Run"),
                              LocaleTerms.Loc("Open Additional Ports on Target Machine"));
            }
        }

        public static void OverrideCommands(CommandSentEvent e)
        {
            if (e[0].ToLower() == "probe" || e[0].ToLower() == "nmap")
            {
                e.IsCancelled = true;
                e.StateChange = CommandDisplayStateChange.Probe;
                var os = e.OS;
                int i;
                var c = os.GetCurrentComputer();
                os.WriteF("Probing {0}...\n", c.ip);
                for (i = 0; i < 10; i++)
                {
                    Thread.Sleep(80);
                    os.WriteSingle(".");
                }
                os.Write("\nProbe Complete - Open ports:\n").Write("---------------------------------");
                if (Port.Instance.compToInst.ContainsKey(c)) foreach (var ins in Port.Instance.compToInst[c])
                    {
                        os.WriteF("Port#: {0} - {1}{2}", ins.Port.PortDisplay, ins.Port.PortName, (ins.Unlocked ? "OPEN" : ""));
                        Thread.Sleep(120);
                    }
                for (i = 0; i < c.ports.Count; i++)
                {
                    os.WriteF("Port#: {0} - {1}{2}", c.GetDisplayPortNumberFromCodePort(c.ports[i]),
                              PortExploits.services[c.ports[i]],
                              (c.portsOpen[i] > 0 ? " : OPEN" : ""));
                    Thread.Sleep(120);
                }
                os.Write("---------------------------------")
                  .WriteF("Open Ports Required for Crack : {0}", Math.Max(c.portsNeededForCrack + 1, 0));
                if (c.hasProxy)
                    os.WriteF("Proxy Detected : {0}", (c.proxyActive ? "ACTIVE" : "INACTIVE"));
                if (c.firewall != null)
                    os.WriteF("Firewall Detected : {0}", (c.firewall.solved ? "SOLVED" : "ACTIVE"));
            }
            if (e[0].ToLower() == "help" || e[0].ToLower() == "man" || e[0] == "?")
            {
                e.IsCancelled = true;
                int page = 0;
                if (e.Arguments.Count > 1)
                {
                    try
                    {
                        page = Convert.ToInt32(e[1]);
                        if (page > Command.Help.PageCount)
                        {
                            e.OS.Write("Invalid Page Number - Displaying First Page");
                            page = 0;
                        }
                    }
                    catch (FormatException)
                    {
                        e.OS.Write("Invalid Page Number");
                    }
                    catch (OverflowException)
                    {
                        e.OS.Write("Invalid Page Number");
                    }
                }
                e.OS.Write(Command.Help.GetPageString(page));
                e.Disconnects = false;
            }
            if (e[0] == "exe")
            {
                e.IsCancelled = true;
                e.Disconnects = false;
                var os = e.OS;
                var folder = os.thisComputer.files.root.searchForFolder("bin");
                os.write("Available Executables:\n");
                os.write("PortHack");
                os.write("ForkBomb");
                os.write("Shell");
                os.write("Tutorial");
                foreach (var file in folder.files)
                {
                    bool alreadyHandled = false;
                    var name = file.name.Contains(".") ? file.name.Remove(file.name.LastIndexOf('.')) : file.name;
                    foreach (var num in PortExploits.exeNums)
                        if (file.data == PortExploits.crackExeData[num]
                            || file.data == PortExploits.crackExeDataLocalRNG[num])
                        {
                            os.write(name);
                            alreadyHandled = true;
                            break;
                        }
                    if (!alreadyHandled && Executable.Handler.IsFileDataForModExe(file.data))
                        os.write(name);
                }
                os.write(" ");
            }
        }

        private static bool wasRecursed;
        public static void OverrideFirstTimeInit(CommandSentEvent e)
        {
            if (Extension.Handler.ActiveInfo != null
                && e[0] == "FirstTimeInitdswhupwnemfdsiuoewnmdsmffdjsklanfeebfjkalnbmsdakj")
            {
                var os = e.OS;
                var num = Settings.isConventionDemo ? 80 : 200;
                var num2 = Settings.isConventionDemo ? 150 : 300;
                var doTut = e[1] == "StartTutorial";
                if (doTut)
                {
                    os.display.visible = false;
                    os.ram.visible = false;
                    os.netMap.visible = false;
                    os.terminal.visible = true;
                    os.mailicon.isEnabled = false;
                    if (os.hubServerAlertsIcon != null)
                    {
                        os.hubServerAlertsIcon.IsEnabled = false;
                    }
                }
                if (Settings.debugCommandsEnabled && GuiData.getKeyboadState().IsKeyDown(Keys.LeftAlt))
                    num2 = (num = 1);
                Programs.typeOut("Initializing .", os, 50);
                Programs.doDots(7, num + 100, os);
                Programs.typeOut("Loading modules.", os, 50);
                Programs.doDots(5, num, os);
                os.writeSingle("Complete");
                Utility.HaltThread(num2);
                Programs.typeOut("Loading nodes.", os, 50);
                Programs.doDots(5, num, os);
                os.writeSingle("Complete");
                Utility.HaltThread(num2);
                Programs.typeOut("Reticulating splines.", os, 50);
                Programs.doDots(5, num - 50, os);
                os.writeSingle("Complete");
                Utility.HaltThread(num2);
                if (os.crashModule.BootLoadErrors.Length > 0)
                {
                    Programs.typeOut("\n------ " + LocaleTerms.Loc("BOOT ERRORS DETECTED") + " ------", os, 50);
                    Utility.HaltThread(200);
                    string[] array = os.crashModule.BootLoadErrors.Split(Utils.newlineDelim, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < array.Length; i++)
                    {
                        Programs.typeOut(array[i], os, 50);
                        Utility.HaltThread(100, true);
                    }
                    Programs.typeOut("---------------------------------\n", os, 50);
                    Utility.HaltThread(200, true);
                }
                Programs.typeOut("\n--Initialization Complete--\n", os, 50);
                GuiData.getFilteredKeys();
                os.inputEnabled = true;
                Utility.HaltThread(num2 + 100);
                if (!doTut)
                {
                    Programs.typeOut(LocaleTerms.Loc("For A Command List, type \"help\""), os, 50);
                    Utility.HaltThread(num2 + 100);
                }
                os.write("");
                Utility.HaltThread(num2);
                os.write("");
                Utility.HaltThread(num2);
                os.write("");
                Utility.HaltThread(num2);
                os.write("\n");
                if (doTut)
                {
                    os.write(LocaleTerms.Loc("Launching Tutorial..."));
                    os.launchExecutable("Tutorial.exe", PortExploits.crackExeData[1], -1, null, null);
                    Settings.initShowsTutorial = false;
                    AdvancedTutorial advancedTutorial = null;
                    for (int i = 0; i < os.exes.Count; i++)
                    {
                        advancedTutorial = (os.exes[i] as AdvancedTutorial);
                        if (advancedTutorial != null)
                            break;
                    }
                    if (advancedTutorial != null)
                        advancedTutorial.CanActivateFirstStep = false;
                    int num3 = 100;
                    for (int i = 0; i < num3; i++)
                    {
                        double num4 = (double)i / num3;
                        if (Utils.random.NextDouble() < num4)
                        {
                            os.ram.visible = true;
                            os.netMap.visible = false;
                            os.terminal.visible = false;
                        }
                        else
                        {
                            os.ram.visible = false;
                            os.netMap.visible = false;
                            os.terminal.visible = true;
                        }
                        Utility.HaltThread(16, true);
                    }
                    os.ram.visible = true;
                    os.netMap.visible = false;
                    os.terminal.visible = false;
                    if (advancedTutorial != null)
                        advancedTutorial.CanActivateFirstStep = true;
                }
                else
                {
                    os.runCommand("connect " + os.thisComputer.ip);
                    if (doTut && !OS.WillLoadSave && !os.Flags.HasFlag("ExtensionFirstBootComplete"))
                    {
                        ExtensionLoader.SendStartingEmailForActiveExtensionNextFrame(os);
                        float num5 = 2.2f;
                        int num3 = (int)(60f * num5);
                        for (int i = 0; i < num3; i++)
                        {
                            double num4 = (double)i / num3;
                            os.ram.visible = (Utils.random.NextDouble() < num4);
                            os.netMap.visible = (Utils.random.NextDouble() < num4);
                            os.display.visible = (Utils.random.NextDouble() < num4);
                            Utility.HaltThread(16, true);
                        }
                        os.terminal.visible = true;
                        os.display.visible = true;
                        os.netMap.visible = true;
                        os.ram.visible = true;
                        os.terminal.visible = true;
                        os.display.inputLocked = false;
                        os.netMap.inputLocked = false;
                        os.ram.inputLocked = false;
                        os.Flags.AddFlag("ExtensionFirstBootComplete");
                    }
                }
                Utility.HaltThread(500, true);
                if (wasRecursed)
                {
                    os.ram.visible = true;
                    os.ram.inputLocked = false;
                    os.display.visible = true;
                    os.display.inputLocked = false;
                    os.netMap.visible = true;
                    os.netMap.inputLocked = false;
                }
                else if (doTut)
                {
                    os.ram.visible = true;
                    os.ram.inputLocked = false;
                    os.display.visible = true;
                    os.display.inputLocked = false;
                    os.netMap.visible = true;
                    os.netMap.inputLocked = false;
                }
                else if (!os.ram.visible)
                {
                    wasRecursed = true;
                    OverrideFirstTimeInit(e);
                }
            }
        }
    }
}
