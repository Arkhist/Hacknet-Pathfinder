using System.Collections.Generic;

namespace Pathfinder.Mission
{
    public interface IGoal
    {
        bool IsComplete(GoalInstance instance, List<string> details);
        void Reset(GoalInstance instance);
        string GetTestCompletableString(GoalInstance instance);
    }

    public class GoalBase : IGoal
    {
        public virtual string GetTestCompletableString(GoalInstance instance)
        {
            return "";
        }

        public virtual bool IsComplete(GoalInstance instance, List<string> details)
        {
            return true;
        }

        public virtual void Reset(GoalInstance instance)
        {}
    }
}
