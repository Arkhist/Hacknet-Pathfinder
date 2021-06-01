using System;
using System.Reflection;
using System.Xml;
using HarmonyLib;
using Hacknet;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Pathfinder.Event.Loading.Content
{    
    [HarmonyPatch]
    public class ComputerComponentLoadEvent : PathfinderEvent
    {
        public Computer Comp { get; }
        public XmlReader Reader { get; }
        public OS Os { get; }

        public ComputerComponentLoadEvent(Computer comp, XmlReader reader, OS os)
        {
            Comp = comp;
            Reader = reader;
            Os = os;
        }

        [HarmonyILManipulator]
        [HarmonyPatch(typeof(ComputerLoader), nameof(ComputerLoader.loadComputer))]
        internal static void ComputerComponentLoadIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.Before,
                x => x.MatchLdloc(1),
                x => x.MatchCallvirt(AccessTools.PropertyGetter(typeof(XmlReader), nameof(XmlReader.Name))),
                x => x.MatchCallvirt(AccessTools.Method(typeof(string), nameof(string.ToLower), new Type[0])),
                x => x.MatchLdstr("file")
            );

            c.Emit(OpCodes.Ldloc, 152);
            c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ComputerLoader).GetNestedType("<>c__DisplayClass4", (BindingFlags)(-1)), "c"));
            c.Emit(OpCodes.Ldloc, 1);
            c.Emit(OpCodes.Ldsfld, AccessTools.Field(typeof(ComputerLoader), nameof(ComputerLoader.os)));
            c.EmitDelegate<Action<Computer, XmlReader, OS>>((comp, reader, os) =>
            {
                var componentLoadEvent = new ComputerComponentLoadEvent(comp, reader, os);
                EventManager<ComputerComponentLoadEvent>.InvokeAll(componentLoadEvent);
            });
        }
    }
}
