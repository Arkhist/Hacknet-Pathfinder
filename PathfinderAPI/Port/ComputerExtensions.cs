using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Hacknet;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Pathfinder.Port
{
    [Obsolete("Use PortState or PortRecord instead")]
    public class PortData : IEquatable<PortData>
    {
        public string Protocol { get; }
        public string DisplayName { get; set; }
        public int Port { get; set; }
        public int OriginalPort { get; private set; }
        public bool Cracked { get; set; } = false;

        public PortData(string proto, int portNum, string displayName)
        {
            Protocol = proto;
            Port = portNum;
            OriginalPort = portNum;
            DisplayName = displayName;
        }

        internal PortData(string proto, int origPortNum, int portNum, string displayName)
        {
            Protocol = proto;
            Port = portNum;
            OriginalPort = origPortNum;
            DisplayName = displayName;
        }

        public PortData Clone()
        {
            var ret = new PortData(Protocol, Port, DisplayName)
            {
                OriginalPort = OriginalPort
            };
            return ret;
        }
            
        public bool Equals(PortData other)
        {
            return !object.ReferenceEquals(other, null) && Protocol == other.Protocol && Port == other.Port && OriginalPort == other.OriginalPort;
        }
        public static bool operator ==(PortData first, PortData second)
        {
            return first?.Equals(second) ?? second is null;
        }
        public static bool operator !=(PortData first, PortData second)
        {
            return !(first == second);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((PortData)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Protocol != null ? Protocol.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Port;
                hashCode = (hashCode * 397) ^ OriginalPort;
                return hashCode;
            }
        }
    }
    
    [HarmonyPatch]
    public static class ComputerExtensions
    {
        internal static readonly Dictionary<string, PortRecord> OGPorts;
        private static readonly Dictionary<int, string> OGPortToProto = new Dictionary<int, string>()
        {
            {22, "ssh"},
            {21, "ftp"},
            {25, "smtp"},
            {80, "web"},
            {1433, "sql"},
            {104, "medical"},
            {6881, "torrent"},
            {443, "ssl"},
            {192, "pacific"},
            {554, "rtsp"},
            // no cracker but still ports
            {211, "transfer"},
            {9418, "version"},
            {3724, "blizzard"},
            {3659, "eos"},
            // ...what the fuck?
            {32, "sigscramble"}
        };

        static ComputerExtensions()
        {
            OGPorts = new Dictionary<string, PortRecord>();
            PortExploits.populate();
            foreach (var port in PortExploits.portNums.Where(x => PortExploits.services.ContainsKey(x)))
            {
                OGPorts.Add(OGPortToProto[port], new PortRecord(OGPortToProto[port], PortExploits.services[port], port));
            }
        }

        private static readonly ConditionalWeakTable<Computer, Dictionary<string, PortState>> PortTable = new ConditionalWeakTable<Computer, Dictionary<string, PortState>>();
        
        public static void AddPort(this Computer comp, string protocol, int portNum, string displayName) => comp.AddPort(new PortState(comp, protocol, displayName, portNum));
        [Obsolete("Use AddPort(PortRecord) or AddPort(PortState) instead")]
        public static void AddPort(this Computer comp, PortData port)
        {
            var ports = PortTable.GetOrCreateValue(comp);
            if (ports.ContainsKey(port.Protocol))
                return;
            ports.Add(port.Protocol, (PortState)port);
        }
        public static void AddPort(this Computer comp, PortRecord record)
        {
            if(record == null)
                throw new ArgumentNullException(nameof(record));

            comp.AddPort(record.CreateState(comp));
        }
        public static void AddPort(this Computer comp, PortState port)
        {
            if(port == null)
                throw new ArgumentNullException(nameof(port));

            if(port.Computer != null && port.Computer != comp)
                throw new InvalidOperationException($"{nameof(port)} already a Computer assigned to it");

            var ports = PortTable.GetOrCreateValue(comp);
            if (ports.ContainsKey(port.Record.Protocol))
                return;

            ports.Add(port.Record.Protocol, port);
            port.Computer = comp;
        }

        public static bool RemovePort(this Computer comp, string protocol)
        {
            return PortTable.GetOrCreateValue(comp).Remove(protocol);
        }

        public static bool RemovePort(this Computer comp, PortRecord record)
        {
            return comp.RemovePort(record.Protocol);
        }

        public static PortState GetPortState(this Computer comp, string protocol)
        {
            PortTable.GetOrCreateValue(comp).TryGetValue(protocol, out var ret);
            return ret;
        }

        [Obsolete("Use GetPortState(string) instead")]
        public static PortData GetPort(this Computer comp, string protocol)
        {
            return (PortData)comp.GetPortState(protocol);
        }

        public static List<PortState> GetAllPortStates(this Computer comp)
        {
            return PortTable.GetOrCreateValue(comp).Values.ToList();
        }

        [Obsolete("Use GetAllPortStates() instead")]
        public static List<PortData> GetAllPorts(this Computer comp)
        {
            return PortTable.GetOrCreateValue(comp).Values.Select(record => (PortData)record).ToList();
        }

        public static Dictionary<string, PortState> GetPortStateDict(this Computer comp)
        {
            return PortTable.GetOrCreateValue(comp);
        }

        [Obsolete("Use GetPortStateDict() instead")]
        public static Dictionary<string, PortData> GetPortDict(this Computer comp)
        {
            return comp.GetPortStateDict()
                .Select(pair => new KeyValuePair<string, PortData>(pair.Key, (PortData)pair.Value))
                .ToDictionary(p => p.Key, p => p.Value);
        }

        public static bool HasInitializedPorts(Computer comp) => PortTable.TryGetValue(comp, out _);

        public static int CountOpenPorts(this Computer comp)
        {
            return comp.GetAllPortStates().Count(x => x.Cracked);
        }

        public static void openPort(this Computer comp, string protocol, string ipFrom)
        {
            if (PortTable.GetOrCreateValue(comp).TryGetValue(protocol, out var port))
            {
                port.Cracked = true;
            }
            comp.log($"{ipFrom} Opened Port#{port?.PortNumber ?? -1}");
            if (!comp.silent)
            {
                comp.sendNetworkMessage($"cPortOpen {comp.ip} {ipFrom} {port?.PortNumber ?? -1}");
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Computer), nameof(Computer.openPort))]
        private static bool OpenPortPrefix(Computer __instance, int portNum, string ipFrom)
        {
            var ports = __instance.GetAllPortStates();
            var port = ports.FirstOrDefault(x => x.Record.OriginalPortNumber == portNum);
            if (port != null)
            {
                port.Cracked = true;
            }
            __instance.log($"{ipFrom} Opened Port#{portNum}");
            if (!__instance.silent)
            {
                __instance.sendNetworkMessage($"cPortOpen {__instance.ip} {ipFrom} {portNum}");
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Computer), nameof(Computer.openPorts))]
        private static bool OpenPortsPrefix(Computer __instance)
        {
            return false;
        }

        public static void closePort(this Computer comp, string protocol, string ipFrom)
        {
            if (PortTable.GetOrCreateValue(comp).TryGetValue(protocol, out var port))
            {
                port.Cracked = false;
            }
            comp.log($"{ipFrom} Closed Port#{port?.PortNumber ?? -1}");
            if (!comp.silent)
            {
                comp.sendNetworkMessage($"cPortClose {comp.ip} {ipFrom} {port?.PortNumber ?? -1}");
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Computer), nameof(Computer.closePort))]
        private static bool ClosePortPrefix(Computer __instance, int portNum, string ipFrom)
        {
            var ports = __instance.GetAllPortStates();
            var port = ports.FirstOrDefault(x => x.Record.OriginalPortNumber == portNum);
            if (port != null)
            {
                port.Cracked = false;
            }
            __instance.log($"{ipFrom} Closed Port#{portNum}");
            if (!__instance.silent)
            {
                __instance.sendNetworkMessage($"cPortClose {__instance.ip} {ipFrom} {portNum}");
            }
            return false;
        }

        public static bool isPortOpen(this Computer comp, string protocol)
        {
            return comp.GetPortState(protocol)?.Cracked ?? false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Computer), nameof(Computer.isPortOpen))]
        private static bool IsPortOpenPrefix(Computer __instance, int portNum, out bool __result)
        {
            if (!OGPortToProto.TryGetValue(portNum, out var proto))
                throw new ArgumentException("Checking if a port is open by it's number is only supported for base game ports!", nameof(portNum));
            __result = __instance.GetPortState(proto)?.Cracked ?? false;
            return false;
        }

        public static void ClearPorts(this Computer comp)
        {
            PortTable.GetOrCreateValue(comp).Clear();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Computer), nameof(Computer.GetCodePortNumberFromDisplayPort))]
        private static bool DisplayToCodePrefix(Computer __instance, int displayPort, out int __result)
        {
            __result = __instance.GetAllPortStates().FirstOrDefault(x => x.PortNumber == displayPort)?.Record.OriginalPortNumber ?? displayPort;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Computer), nameof(Computer.GetDisplayPortNumberFromCodePort))]
        private static bool CodeToDisplayPrefix(Computer __instance, int codePort, out int __result)
        {
            __result = __instance.GetAllPortStates().FirstOrDefault(x => x.Record.OriginalPortNumber == codePort)?.PortNumber ?? codePort;
            return false;
        }

        [HarmonyILManipulator]
        [HarmonyPatch(typeof(Programs), nameof(Programs.probe))]
        private static void FixCommandProbe(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.Before,
                x => x.MatchNop(),
                x => x.MatchLdcI4(0),
                x => x.MatchStloc(2),
                x => x.MatchBr(out _),
                x => x.MatchNop(),
                x => x.MatchLdarg(1)
            );

            var begin = c.MarkLabel();

            c.GotoNext(MoveType.Before,
                x => x.MatchLdarg(1),
                x => x.MatchLdstr("---------------------------------"),
                x => x.MatchCallOrCallvirt(AccessTools.Method(typeof(OS), nameof(OS.write)))
            );

            var endIndex = c.Index;
            c.GotoLabel(begin);
            c.RemoveRange(endIndex - c.Index);

            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Ldloc_1);
            c.EmitDelegate<Action<OS, Computer>>((os, comp) =>
            {
                foreach (var port in comp.GetAllPortStates())
                {
                    os.write($"Port#: {port.PortNumber}  -  {port.DisplayName}{(port.Cracked ? " : OPEN" : "")}");
                    Thread.Sleep(120);
                }
            });
        }

        [HarmonyILManipulator]
        [HarmonyPatch(typeof(DisplayModule), nameof(DisplayModule.doProbeDisplay))]
        private static void FixDisplayProbe(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.Before,
                x => x.MatchLdcI4(0),
                x => x.MatchStloc(6),
                x => x.MatchBr(out _),
                x => x.MatchNop(),
                x => x.MatchLdloca(0),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(AccessTools.Field(typeof(DisplayModule), nameof(DisplayModule.y))),
                x => x.MatchLdcI4(4),
                x => x.MatchAdd(),
                x => x.MatchStfld(AccessTools.Field(typeof(Rectangle), nameof(Rectangle.Y)))
            );

            var begin = c.MarkLabel();
            c.GotoNext(MoveType.Before, x => x.MatchRet());
            var end = c.Index;
            c.GotoLabel(begin);
            c.RemoveRange(end - c.Index);

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc_1);
            c.Emit(OpCodes.Ldloc_0);
            c.Emit(OpCodes.Ldloc, 10);
            c.EmitDelegate<Action<DisplayModule, Computer, Rectangle, Vector2>>((display, comp, rect, pos) =>
            {
                var batch = display.spriteBatch;
                var os = display.os;
                foreach (var port in comp.GetAllPortStates())
                {
                    rect.Y = display.y + 4;
                    pos.Y = rect.Y + 4;
                    batch.Draw(Utils.white, rect, port.Cracked ? os.unlockedColor : os.lockedColor);
                    batch.Draw(port.Cracked ? display.openLockSprite : display.lockSprite, pos, Color.White);
                    string portNumText = $"Port#: {port.PortNumber}";
                    var start = GuiData.font.MeasureString(portNumText);
                    batch.DrawString(GuiData.font, portNumText, new Vector2(display.x, display.y + 3), Color.White);
                    string portNameText = $" - {port.DisplayName}";
                    var second = GuiData.smallfont.MeasureString(portNameText);
                    float num2 = rect.Width - start.X - 50f;
                    float scale = Math.Min(1f, num2 / second.X);
                    batch.DrawString(GuiData.smallfont, portNameText, new Vector2(display.x + start.X, display.y + 4), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0.8f);
                    display.y += 45;
                }
            });
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FastBasicAdministrator), nameof(FastBasicAdministrator.disconnectionDetected))]
        private static void ResetPortsFastPostfix(FastBasicAdministrator __instance, Computer c, OS os)
        {
            foreach (var port in c.GetAllPortStates().Where(x => x.Cracked))
                c.closePort(port.Record.Protocol, "LOCAL_ADMIN");
            if (!__instance.IsSuper)
                os.delayer.Post(os.delayer.nextPairs.Last().Condition, () =>
                {
                    foreach (var port in c.GetAllPortStates().Where(x => x.Cracked))
                        c.closePort(port.Record.Protocol, "LOCAL_ADMIN");
                });
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FastProgressOnlyAdministrator), nameof(FastProgressOnlyAdministrator.disconnectionDetected))]
        private static void ResetPortsProgressOnlyPostfix(Computer c)
        {
            foreach (var port in c.GetAllPortStates().Where(x => x.Cracked))
                c.closePort(port.Record.Protocol, "LOCAL_ADMIN");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BasicAdministrator), nameof(BasicAdministrator.disconnectionDetected))]
        private static void ResetPortsBasicPostfix(Computer c, OS os)
        {
            os.delayer.Post(os.delayer.nextPairs.Last().Condition, () =>
            {
                foreach (var port in c.GetAllPortStates().Where(x => x.Cracked))
                    c.closePort(port.Record.Protocol, "LOCAL_ADMIN");
            });
        }

        [HarmonyILManipulator]
        [HarmonyPatch(typeof(ProgramRunner), nameof(ProgramRunner.AttemptExeProgramExecution))]
        private static void FixPortCrackers(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.Before,
                x => x.MatchLdcI4(0),
                x => x.MatchStloc(9),
                x => x.MatchBr(out _),
                x => x.MatchNop(),
                x => x.MatchLdloc(0),
                x => x.MatchLdfld(AccessTools.Field(typeof(Computer), nameof(Computer.ports))),
                x => x.MatchLdloc(9),
                x => x.MatchCallOrCallvirt(AccessTools.Method(typeof(List<int>), "get_Item"))
            );

            c.RemoveRange(36);

            c.Emit(OpCodes.Ldloc_0);
            c.Emit(OpCodes.Ldloc, 11);
            c.EmitDelegate<Func<Computer, int, int>>((comp, port) =>
            {
                if (comp.GetAllPortStates().Any(x => x.Record.OriginalPortNumber == port))
                    return port;
                return -1;
            });
            c.Emit(OpCodes.Stloc, 10);
        }

        [HarmonyILManipulator]
        [HarmonyPatch(typeof(OS), nameof(OS.launchExecutable))]
        private static void FixPortHack(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.Before,
                x => x.MatchNop(),
                x => x.MatchLdcI4(0),
                x => x.MatchStloc(5),
                x => x.MatchLdcI4(0),
                x => x.MatchStloc(1),
                x => x.MatchBr(out _)
            );

            c.RemoveRange(30);

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(OS), nameof(OS.connectedComp)));
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(ComputerExtensions), nameof(CountOpenPorts)));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ComputerLoader), nameof(ComputerLoader.loadPortsIntoComputer))]
        private static bool LoadPortsIntoComputerReplacement(string portsList, object computer_obj)
        {
            PortManager.LoadPortsFromStringVanilla((Computer)computer_obj, portsList);
            return false;
        }
    }
}
