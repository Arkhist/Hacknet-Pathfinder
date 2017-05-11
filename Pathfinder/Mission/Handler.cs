using System;
using System.Collections.Generic;
using Pathfinder.Util;

namespace Pathfinder.Mission
{
    public static class Handler
    {
        internal static Dictionary<string, IGoal> goals = new Dictionary<string, IGoal>();
        internal static Dictionary<string, IInterface> missions = new Dictionary<string, IInterface>();

        public static string RegisterMissionGoal(string id, IGoal inter)
        {
            id = Utility.GetId(id, throwFindingPeriod: true);
            Logger.Verbose("Mod {0} attempting to add mission goal interface {1} with id {2}",
                           Utility.ActiveModId,
                           inter.GetType().FullName,
                           id);
            if (goals.ContainsKey(id))
                return null;

            goals.Add(id, inter);
            return id;
        }

        [Obsolete("Use RegisterMissionGoal")]
        public static bool AddMissionGoal(string id, IGoal inter) => RegisterMissionGoal(id, inter) != null;

        internal static bool UnregisterMissionGoal(string id)
        {
            id = Utility.GetId(id);
            if (!goals.ContainsKey(id))
                return true;
            return goals.Remove(id);
        }

        public static string RegisterMission(string id, IInterface inter)
        {
            id = Utility.GetId(id, throwFindingPeriod: true);
            Logger.Verbose("Mod {0} attempting to add mission interface {1} with id {2}",
                           Utility.ActiveModId,
                           inter.GetType().FullName,
                           id);
            if (goals.ContainsKey(id))
                return null;

            missions.Add(id, inter);
            return id;
        }

        [Obsolete("Use RegisterMission")]
        public static bool AddMission(string id, IInterface inter) => RegisterMission(id, inter) != null;

        internal static bool UnregisterMission(string id)
        {
            id = Utility.GetId(id);
            if (!missions.ContainsKey(id))
                return true;
            return missions.Remove(id);
        }

        public static bool ContainsMission(string id) => missions.ContainsKey(Utility.GetId(id));
        public static bool ContainsMissionGoal(string id) => goals.ContainsKey(Utility.GetId(id));

        public static IInterface GetMissionById(string id) => GetMissionById(ref id);
        public static IInterface GetMissionById(ref string id)
        {
            id = Utility.GetId(id);
            IInterface i;
            if (missions.TryGetValue(id, out i))
                return i;
            return null;
        }

        public static IGoal GetMissionGoalById(string id) => GetMissionGoalById(ref id);
        public static IGoal GetMissionGoalById(ref string id)
        {
            id = Utility.GetId(id);
            IGoal i;
            if (goals.TryGetValue(id, out i))
                return i;
            return null;
        }
    }
}
