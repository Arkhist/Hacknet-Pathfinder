using System;
using System.Collections.Generic;
using System.Xml;
using Hacknet;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Pathfinder.Event.Loading.Content
{
    public class GetAdditionalConditionsEvent : PathfinderEvent
    {
        public struct ConditionInfo
        {
            public string XmlName;
            public Func<XmlReader, SerializableCondition> Callback;
        }

        public readonly List<ConditionInfo> AdditonalConditions = new List<ConditionInfo>();

        [HarmonyILManipulator]
        [HarmonyPatch(typeof(SerializableCondition), nameof(SerializableCondition.Deserialize))]
        internal static void GetAdditionalConditions(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.Before,
                x => x.MatchLdloc(1)
            );

            c.Emit(OpCodes.Ldloc_1);
            c.EmitDelegate<Action<Dictionary<string, Func<XmlReader, SerializableCondition>>>>(dict =>
            {
                foreach (var actionInfo in EventManager<GetAdditionalConditionsEvent>.InvokeAll(new GetAdditionalConditionsEvent()).AdditonalConditions)
                    dict.Add(actionInfo.XmlName, actionInfo.Callback);
            });
        }
    }
}
