using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hacknet.Mission;
using Pathfinder.Event;
using Pathfinder.Event.Loading.Content;
using Pathfinder.Util;

namespace Pathfinder.Mission
{
    public static class GoalManager
    {
        private static readonly Dictionary<string, Type> CustomGoals = new Dictionary<string, Type>();

        static GoalManager()
        {
            EventManager<LoadGoalEvent>.AddHandler(OnLoadGoal);
            EventManager.onPluginUnload += OnPluginUnload;
        }

        private static void OnLoadGoal(LoadGoalEvent args)
        {
            if (args.GoalName == null) return;
            if (CustomGoals.TryGetValue(args.GoalName, out Type goalType))
            {
                args.GoalFound = true;

                MisisonGoal goal = (MisisonGoal)Activator.CreateInstance(goalType);
                XMLStorageAttribute.ReadFromXml(args.Reader, goal);
                if (goal is InitializableGoal pathfinderGoal)
                    pathfinderGoal.Initialize();
                args.GoalList.Add(goal);
            }
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
            CustomGoals.Add(xmlName, goalType);
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
            CustomGoals.Remove(xmlName);
        }
    }
}
