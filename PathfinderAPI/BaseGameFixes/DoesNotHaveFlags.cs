using System;
using System.Collections.Generic;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Hacknet;

namespace Pathfinder.BaseGameFixes
{
    [HarmonyPatch]
    internal static class DoesNotHaveFlags
    {
        [HarmonyILManipulator]
        [HarmonyPatch(typeof(SerializableCondition), nameof(SerializableCondition.Deserialize))]
        internal static void AddDoesNotHaveFlagsIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            var add = AccessTools.Method(typeof(Dictionary<string, Func<System.Xml.XmlReader, SerializableCondition>>), "Add", new Type[] { typeof(string), typeof(Func<System.Xml.XmlReader, SerializableCondition>) });

            c.GotoNext(MoveType.After,
                x => x.MatchCallvirt(add)
            );

            c.Emit(OpCodes.Ldloc_0);
            c.Emit(OpCodes.Ldstr, "DoesNotHaveFlags");
            c.Emit(OpCodes.Ldnull);
            c.Emit(OpCodes.Ldftn, AccessTools.Method(typeof(SCDoesNotHaveFlags), nameof(SCDoesNotHaveFlags.DeserializeFromReader)));
            c.Emit(OpCodes.Newobj, AccessTools.Constructor(typeof(Func<System.Xml.XmlReader, SerializableCondition>), new Type[] { typeof(object), typeof(IntPtr) }));
            c.Emit(OpCodes.Callvirt, add);
        }
    }
}
