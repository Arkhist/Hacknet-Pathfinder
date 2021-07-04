using System;
using System.Reflection;
using System.Xml;
using HarmonyLib;
using Hacknet;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Pathfinder.Util.XML;

namespace Pathfinder.Event.Loading.Content
{
    public class ComputerComponentLoadEvent : PathfinderEvent
    {
        public Computer Comp { get; }
        public ElementInfo Info { get; }
        public OS Os { get; }

        public ComputerComponentLoadEvent(Computer comp, ElementInfo info, OS os)
        {
            Comp = comp;
            Info = info;
            Os = os;
        }
    }
}
