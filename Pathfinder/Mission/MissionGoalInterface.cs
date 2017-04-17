using System.Collections.Generic;

namespace Pathfinder.Mission
{
    public interface IMissionGoal
    {
        bool IsComplete(MissionGoalInstance instance, List<string> details);
        void Reset(MissionGoalInstance instance);
        string GetTestCompletableString(MissionGoalInstance instance);
    }

    public class MissionGoalImpl : IMissionGoal
    {
        public string GetTestCompletableString(MissionGoalInstance instance)
        {
            return "";
        }

        public bool IsComplete(MissionGoalInstance instance, List<string> details)
        {
            return true;
        }

        public void Reset(MissionGoalInstance instance)
        {}
    }
}
