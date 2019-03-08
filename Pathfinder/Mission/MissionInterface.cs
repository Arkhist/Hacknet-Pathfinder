using System;
using System.Collections.Generic;
using Hacknet;
using Hacknet.Mission;

namespace Pathfinder.Mission
{
    public interface Interface
    {
        List<MisisonGoal> DefaultGoals { get; }
        string InitialNextMission { get; }
        MailServer.EMailData IntialEmailData { get; }
        bool AutoLoadSaveables { get; }

        Tuple<string, int> OnStart(Instance instance);
        void OnLoadInstance(Instance instance, Dictionary<string, object> objects);
        void OnGoalAdd(Instance instance, MisisonGoal goal);
        void Update(Instance instance, float time);
        bool? IsComplete(Instance instance, List<string> details);
        bool SendEmail(Instance instance, OS os);
        Tuple<string, int> OnEnd(Instance instance);
    }

    public class Base : Interface
    {
        public virtual List<MisisonGoal> DefaultGoals { get { return null; } }
        public virtual string InitialNextMission { get { return null; } }
        public virtual MailServer.EMailData IntialEmailData { get { return default(MailServer.EMailData); } }
        public virtual bool AutoLoadSaveables => true;

        public virtual bool? IsComplete(Instance instance, List<string> details) { return true; }
        public virtual Tuple<string, int> OnEnd(Instance instance) { return null; }
        public virtual Tuple<string, int> OnStart(Instance instance) { return null; }
        public virtual bool SendEmail(Instance instance, OS os) { return true; }
        public virtual void Update(Instance instance, float time) {}
        public virtual void OnGoalAdd(Instance instance, MisisonGoal goal) {}
        public virtual void OnLoadInstance(Instance instance, Dictionary<string, object> objects) {}
    }
}
