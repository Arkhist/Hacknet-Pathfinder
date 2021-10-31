using System;
using System.Reflection;
using BepInEx.Hacknet;
using HarmonyLib;

namespace Pathfinder.Meta.Load
{
    [HarmonyPatch]
    internal static class AttributeManager
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HacknetPlugin), nameof(HacknetPlugin.Load))]
        private static void OnPluginLoad(ref HacknetPlugin __instance)
        {
            ReadAttributesFor(__instance);
        }

        public static void ReadAttributesFor(HacknetPlugin plugin)
        {
            var pluginType = plugin.GetType();
            if(pluginType.GetCustomAttribute<IgnorePluginAttribute>() != null)
                return;
            foreach(var type in pluginType.Assembly.GetTypes())
            {
                ReadAttributesOnType(plugin, type);
            }
        }

        private static void ReadAttributesOnType(HacknetPlugin plugin, Type type)
        {
            foreach (var attribute in type.GetCustomAttributes<BaseAttribute>())
            {
                attribute.CallOn(plugin, type);
            }
            foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
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
