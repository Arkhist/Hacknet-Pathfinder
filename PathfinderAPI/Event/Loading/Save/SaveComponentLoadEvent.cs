using System;
using System.Xml;
using Hacknet;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Pathfinder.Util.XML;

namespace Pathfinder.Event.Loading.Save
{
    [Flags]
    public enum ComponentType
    {
        None = 0b0,
        Generic = 0b1,
        Daemon = 0b10,
        All = 0b11
    }
    
    public class SaveComponentLoadEvent : PathfinderEvent
    {
        public Computer Comp { get; }
        public ElementInfo Info { get; }
        public OS Os { get; }
        public ComponentType Type { get; }

        public SaveComponentLoadEvent(Computer comp, ElementInfo info, OS os, ComponentType type)
        {
            Comp = comp;
            Info = info;
            Os = os;
            Type = type;
        }
    }
}
