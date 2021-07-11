using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hacknet.Mission;
using Pathfinder.Event;
using Pathfinder.Event.Loading.Content;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Mission
{
    public static class GoalManager
    {
        private static readonly Dictionary<string, Type> CustomGoals = new Dictionary<string, Type>();

        static GoalManager()
        {
            EventManager.onPluginUnload += OnPluginUnload;
        }

        internal static bool TryLoadCustomGoal(string type, out MisisonGoal customGoal)
        {
            if (CustomGoals.TryGetValue(type, out var goalType))
            {
                customGoal = (MisisonGoal)Activator.CreateInstance(goalType);
                return true;
            }

            customGoal = null;
            return false;
        }

        private static void OnPluginUnload(Assembly pluginAsm)
        {
            var allTypes = pluginAsm.GetTypes();
            foreach (var name in CustomGoals.Where(x => allTypes.Contains(x.Value)).Select(x => x.Key).ToList())
                CustomGoals.Remove(name);
        }
        
        public static void RegisterGoal<T>(string xmlName) where T : MisisonGoal => RegisterGoal(typeof(T), xmlName);
        public static void RegisterGoal(Type goalType, string xmlName)
        {
            if (!typeof(MisisonGoal).IsAssignableFrom(goalType))
                throw new ArgumentException("Goal type must inherit from Hacknet.Mission.MisisonGoal (yes that is spelled right)", nameof(goalType));
            CustomGoals.Add(xmlName.ToLower(), goalType);
        }

        public static void UnregisterGoal<T>() => UnregisterGoal(typeof(T));
        public static void UnregisterGoal(Type goalType)
        {
            var xmlName = CustomGoals.FirstOrDefault(x => x.Value == goalType).Key;
            if (xmlName != null)
                CustomGoals.Remove(xmlName);
        }
        public static void UnregisterGoal(string xmlName)
        {
            CustomGoals.Remove(xmlName.ToLower());
        }
    }
}
