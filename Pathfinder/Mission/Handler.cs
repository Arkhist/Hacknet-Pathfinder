using System.Collections.Generic;
using Pathfinder.Util;

namespace Pathfinder.Mission
{
    public static class Handler
    {
        private static Dictionary<string, IMissionGoal> goals = new Dictionary<string, IMissionGoal>();
        private static Dictionary<string, IInterface> missions = new Dictionary<string, IInterface>();

        public static bool AddMissionGoal(string goalId, IMissionGoal inter)
        {
            goalId = Utility.GetPreviousStackFrameIdentity() + "." + Utility.GetId(goalId, true);
            if (goals.ContainsKey(goalId))
                return false;
            
            goals.Add(goalId, inter);
            return true;
        }

        public static bool AddMission(string missionId, IInterface inter)
        {
            missionId = Utility.GetPreviousStackFrameIdentity() + "." + Utility.GetId(missionId, true);
            if (goals.ContainsKey(missionId))
                return false;

            missions.Add(missionId, inter);
            return true;
        }

        public static bool ContainsMission(string id)
        {
            return missions.ContainsKey(Utility.GetId(id));
        }

        public static bool ContainsMissionGoal(string id)
        {
            id = Utility.GetId(id);
            return goals.ContainsKey(id);
        }

        public static IInterface GetMissionById(string id)
        {
            id = Utility.GetId(id);
            IInterface i;
            if (missions.TryGetValue(id, out i))
                return i;
            return null;
        }


        public static IMissionGoal GetMissionGoalById(string id)
        {
            id = Utility.GetId(id);
            IMissionGoal i;
            if (goals.TryGetValue(id, out i))
                return i;
            return null;
        }
    }
}
