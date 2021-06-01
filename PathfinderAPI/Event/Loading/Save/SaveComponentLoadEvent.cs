using System;
using System.Xml;
using Hacknet;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

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
    
    [HarmonyPatch]
    public class SaveComponentLoadEvent : PathfinderEvent
    {
        public Computer Comp { get; }
        public XmlReader Reader { get; }
        public OS Os { get; }
        public ComponentType Type { get; }

        public SaveComponentLoadEvent(Computer comp, XmlReader reader, OS os, ComponentType type)
        {
            Comp = comp;
            Reader = reader;
            Os = os;
            Type = type;
        }

        [HarmonyILManipulator]
        [HarmonyPatch(typeof(Computer), nameof(Computer.load))]
        internal static void LoadSavedComputerIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            
            c.GotoNext(MoveType.Before, 
                x => x.MatchLdarg(0),
                x => x.MatchCallvirt(AccessTools.PropertyGetter(typeof(XmlReader), nameof(XmlReader.Name))),
                x => x.MatchLdstr("MailServer"),
                x => x.MatchCall(AccessTools.Method(typeof(string), "op_Equality", new Type[] { typeof(string), typeof(string) })),
                x => x.MatchLdcI4(0),
                x => x.MatchCeq(),
                x => x.MatchStloc(out _)
            );

            c.MoveAfterLabels();
            c.Emit(OpCodes.Ldloc, 23);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<Action<Computer, XmlReader, OS>>((comp, reader, os) =>
            {
                var componentLoadSavedEvent = new SaveComponentLoadEvent(comp, reader, os, ComponentType.Daemon);
                EventManager<SaveComponentLoadEvent>.InvokeAll(componentLoadSavedEvent);
            });
        }
    }
}
