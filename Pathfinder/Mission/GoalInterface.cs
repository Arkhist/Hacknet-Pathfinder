using System.Collections.Generic;

namespace Pathfinder.Mission
{
    public interface IGoal
    {
        bool IsComplete(GoalInstance instance, List<string> details);
        void Reset(GoalInstance instance);
        string GetTestCompletableString(GoalInstance instance);
    }

    public class GoalImpl : IGoal
    {
        public string GetTestCompletableString(GoalInstance instance)
        {
            return "";
        }

        public bool IsComplete(GoalInstance instance, List<string> details)
        {
            return true;
        }

        public void Reset(GoalInstance instance)
        {}
    }
}
