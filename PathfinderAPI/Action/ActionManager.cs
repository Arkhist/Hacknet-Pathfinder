using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Hacknet;
using Hacknet.Mission;
using HarmonyLib;
using Pathfinder.Event;
using Pathfinder.Event.Loading.Content;

namespace Pathfinder.Action
{
    [HarmonyPatch]
    public static class ActionManager
    {
        private static readonly Dictionary<string, Type> CustomActions = new Dictionary<string, Type>();

        static ActionManager()
        {
            EventManager<GetAdditionalActionsEvent>.AddHandler(OnGetAdditionalActions);
            EventManager.onPluginUnload += OnPluginUnload;
        }

        private static void OnGetAdditionalActions(GetAdditionalActionsEvent args)
        {
            args.AdditonalActions.AddRange(CustomActions.Keys.Select(x => new GetAdditionalActionsEvent.ActionInfo { XmlName = x, Callback = ActionLoadCallback}));
        }
        private static SerializableAction ActionLoadCallback(XmlReader reader)
        {
            var actionType = CustomActions[reader.Name];
            PathfinderAction action = (PathfinderAction)Activator.CreateInstance(actionType);
            action.LoadFromXml(reader);
            return action;
        }
        
        private static void OnPluginUnload(Assembly pluginAsm)
        {
            var allTypes = pluginAsm.GetTypes();
            foreach (var name in CustomActions.Where(x => allTypes.Contains(x.Value)).Select(x => x.Key))
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
                __result = pfAction.GetSaveStringOverridable();
                return false;
            }

            return true;
        }
    }
}
