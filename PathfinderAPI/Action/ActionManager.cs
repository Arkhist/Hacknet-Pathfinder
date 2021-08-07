using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Hacknet;
using Hacknet.Mission;
using HarmonyLib;
using Pathfinder.Event;
using Pathfinder.Util.XML;

namespace Pathfinder.Action
{
    [HarmonyPatch]
    public static class ActionManager
    {
        private static readonly Dictionary<string, Type> CustomActions = new Dictionary<string, Type>();

        static ActionManager()
        {
            EventManager.onPluginUnload += OnPluginUnload;
        }

        internal static bool TryLoadCustomAction(ElementInfo info, out PathfinderAction action)
        {
            if (CustomActions.TryGetValue(info.Name, out var actionType))
            {
                action = (PathfinderAction) Activator.CreateInstance(actionType);
                action.LoadFromXml(info);
                return true;
            }

            action = null;
            return false;
        }
        
        private static void OnPluginUnload(Assembly pluginAsm)
        {
            var allTypes = pluginAsm.GetTypes();
            foreach (var name in CustomActions.Where(x => allTypes.Contains(x.Value)).Select(x => x.Key).ToList())
                CustomActions.Remove(name);
        }
        
        public static void RegisterAction<T>(string xmlName) where T : PathfinderAction => RegisterAction(typeof(T), xmlName);
        public static void RegisterAction(Type actionType, string xmlName)
        {
            if (!typeof(PathfinderAction).IsAssignableFrom(actionType))
                throw new ArgumentException("Action type must inherit from Pathfinder.Action.PathfinderAction!", nameof(actionType));
            CustomActions.Add(xmlName, actionType);
        }

        public static void UnregisterAction<T>() where T : PathfinderAction => UnregisterAction(typeof(T));
        public static void UnregisterAction(Type actionType)
        {
            var xmlName = CustomActions.FirstOrDefault(x => x.Value == actionType).Key;
            if (xmlName != null)
                CustomActions.Remove(xmlName);
        }
        public static void UnregisterAction(string xmlName)
        {
            CustomActions.Remove(xmlName);
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SerializableAction), nameof(SerializableAction.GetSaveString))]
        internal static bool GetSaveStringOverridePrefix(SerializableAction __instance, ref string __result)
        {
            if (__instance is PathfinderAction pfAction)
            {
                var builder = new StringBuilder();
                using (var writer = XmlWriter.Create(builder))
                    pfAction.GetSaveElement().WriteTo(writer);
                __result = builder.ToString();
                return false;
            }

            return true;
        }
    }
}
