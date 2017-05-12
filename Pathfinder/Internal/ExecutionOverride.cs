using System;
using System.Threading;
using Hacknet;
using Pathfinder.Computer;
using Pathfinder.Event;
using Pathfinder.GUI;
using Pathfinder.OS;

namespace Pathfinder.Internal
{
    static class ExecutionOverride
    {
        public static void OverridePortHack(ExecutablePortExecuteEvent e)
        {
            if (e.Arguments[0].ToLower() == "porthack")
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

        public static void OverwriteProbe(CommandSentEvent e)
        {
            if (e.Arguments[0].ToLower() == "probe" || e.Arguments[0].ToLower() == "nmap")
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
                    os.WriteF("Port#: {0} - {1}{2}",ins.Port.PortDisplay, ins.Port.PortName, (ins.Unlocked ? "OPEN" : ""));
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
        }
    }
}
