using System;
using System.Collections.Generic;
using System.Xml;
using Hacknet;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Pathfinder.Event.Loading.Content
{
    [HarmonyPatch]
    public class GetAdditionalActionsEvent : PathfinderEvent
    {
        public struct ActionInfo
        {
            public string XmlName;
            public Func<XmlReader, SerializableAction> Callback;
        }

        public readonly List<ActionInfo> AdditonalActions = new List<ActionInfo>();

        [HarmonyILManipulator]
        [HarmonyPatch(typeof(SerializableAction), nameof(SerializableAction.Deserialize))]
        internal static void GetAdditionalActionsIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.Before,
                x => x.MatchLdloc(1)
            );

            c.Emit(OpCodes.Ldloc_1);
            c.EmitDelegate<Action<Dictionary<string, Func<XmlReader, SerializableAction>>>>(dict =>
            {
                foreach (var actionInfo in EventManager<GetAdditionalActionsEvent>.InvokeAll(new GetAdditionalActionsEvent()).AdditonalActions)
                    dict.Add(actionInfo.XmlName, actionInfo.Callback);
            });
        }
    }
}
