using System.Xml.Linq;
using System.Linq;
using System;
using System.Reflection;
using BepInEx.Hacknet;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Pathfinder.Meta.Load
{
    [HarmonyPatch]
    internal static class AttributeManager
    {
        [HarmonyILManipulator]
        [HarmonyPatch(typeof(HacknetChainloader), nameof(HacknetChainloader.LoadPlugin))]
        private static void OnPluginLoadIL(ILContext il)
        {
            var c = new ILCursor(il);

            c.GotoNext(
                x => x.MatchLdloc(1),
                x => x.MatchCallvirt(AccessTools.Method(typeof(HacknetPlugin), nameof(HacknetPlugin.Load)))
            );

            c.Emit(OpCodes.Ldloc, 1);
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(AttributeManager), nameof(ReadAttributesFor)));
        }

        public static void ReadAttributesFor(HacknetPlugin plugin)
        {
            var pluginType = plugin.GetType();
            if(pluginType.GetCustomAttribute<IgnorePluginAttribute>() != null)
                return;
            ReadAttributesOnType(plugin, pluginType);
            foreach(var type in pluginType.Assembly.GetTypes())
            {
                if(type == pluginType) continue;
                ReadAttributesOnType(plugin, type);
            }
        }

        private static void ReadAttributesOnType(HacknetPlugin plugin, Type type)
        {
            foreach (var attribute in type.GetCustomAttributes<BaseAttribute>())
            {
                attribute.CallOn(plugin, type);
            }
            foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                if (member.MemberType == MemberTypes.NestedType)
                {
                    ReadAttributesOnType(plugin, (Type)member);
                }
                else foreach (var attribute in member.GetCustomAttributes<BaseAttribute>())
                {
                    attribute.CallOn(plugin, member);
                }
            }
        }
    }
}
