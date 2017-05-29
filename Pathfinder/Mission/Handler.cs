using System;
using System.Collections.Generic;
using Pathfinder.Util;

namespace Pathfinder.Mission
{
    public static class Handler
    {
        internal static Dictionary<string, IGoal> ModGoals = new Dictionary<string, IGoal>();
        internal static Dictionary<string, IInterface> ModMissions = new Dictionary<string, IInterface>();

        public static string RegisterMissionGoal(string id, IGoal inter)
        {
            if (Pathfinder.CurrentMod == null && !Extension.Handler.CanRegister)
                throw new InvalidOperationException("RegisterMissionGoal can not be called outside of mod or extension loading.");
            id = Pathfinder.CurrentMod != null ? Utility.GetId(id, throwFindingPeriod: true) : Extension.Handler.ActiveInfo.Id+"."+id;
            Logger.Verbose("{0} {1} attempting to add mission goal interface {2} with id {3}",
                           Pathfinder.CurrentMod != null ? "Mod" : "Extension",
                           Pathfinder.CurrentMod?.GetCleanId() ?? Extension.Handler.ActiveInfo.Id,
                           inter.GetType().FullName,
                           id);
            if (ModGoals.ContainsKey(id))
                return null;

            ModGoals.Add(id, inter);
            return id;
        }

        [Obsolete("Use RegisterMissionGoal")]
        public static bool AddMissionGoal(string id, IGoal inter) => RegisterMissionGoal(id, inter) != null;

        internal static bool UnregisterMissionGoal(string id)
        {
            id = Utility.GetId(id);
            if (!ModGoals.ContainsKey(id))
                return true;
            return ModGoals.Remove(id);
        }

        public static string RegisterMission(string id, IInterface inter)
        {
            if (Pathfinder.CurrentMod == null && !Extension.Handler.CanRegister)
                throw new InvalidOperationException("RegisterMission can not be called outside of mod or extension loading.");
            id = Pathfinder.CurrentMod != null ? Utility.GetId(id, throwFindingPeriod: true) : Extension.Handler.ActiveInfo.Id+"."+id;
            Logger.Verbose("{0} {1} attempting to add mission interface {2} with id {3}",
                           Pathfinder.CurrentMod != null ? "Mod" : "Extension",
                           Pathfinder.CurrentMod?.GetCleanId() ?? Extension.Handler.ActiveInfo.Id,
                           inter.GetType().FullName,
                           id);
            if (ModGoals.ContainsKey(id))
                return null;

            ModMissions.Add(id, inter);
            return id;
        }

        [Obsolete("Use RegisterMission")]
        public static bool AddMission(string id, IInterface inter) => RegisterMission(id, inter) != null;

        internal static bool UnregisterMission(string id)
        {
            id = Utility.GetId(id);
            if (!ModMissions.ContainsKey(id))
                return true;
            return ModMissions.Remove(id);
        }

        public static bool ContainsMission(string id) => ModMissions.ContainsKey(Utility.GetId(id));
        public static bool ContainsMissionGoal(string id) => ModGoals.ContainsKey(Utility.GetId(id));

        public static IInterface GetMissionById(string id) => GetMissionById(ref id);
        public static IInterface GetMissionById(ref string id)
        {
            id = Utility.GetId(id);
            IInterface i;
            if (ModMissions.TryGetValue(id, out i))
                return i;
            return null;
        }

        public static IGoal GetMissionGoalById(string id) => GetMissionGoalById(ref id);
        public static IGoal GetMissionGoalById(ref string id)
        {
            id = Utility.GetId(id);
            IGoal i;
            if (ModGoals.TryGetValue(id, out i))
                return i;
            return null;
        }
    }
}
