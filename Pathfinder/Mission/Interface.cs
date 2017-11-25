using System;
using System.Collections.Generic;
using Hacknet;
using Hacknet.Mission;

namespace Pathfinder.Mission
{
    public interface IInterface
    {
        List<MisisonGoal> DefaultGoals { get; }
        string InitialNextMission { get; }
        MailServer.EMailData IntialEmailData { get; }

        Tuple<string, int> OnStart(Instance instance);
        void OnLoadInstance(Instance instance, Dictionary<string, string> objects);
        void OnGoalAdd(Instance instance, MisisonGoal goal);
        void Update(Instance instance, float time);
        bool? IsComplete(Instance instance, List<string> details);
        bool SendEmail(Instance instance, OS os);
        Tuple<string, int> OnEnd(Instance instance);
    }

    public class Interface : IInterface
    {
        public List<MisisonGoal> DefaultGoals { get { return null; } }
        public string InitialNextMission { get { return null; } }
        public MailServer.EMailData IntialEmailData { get { return default(MailServer.EMailData); } }

        public bool? IsComplete(Instance instance, List<string> details) { return true; }
        public Tuple<string, int> OnEnd(Instance instance) { return null; }
        public Tuple<string, int> OnStart(Instance instance) { return null; }
        public bool SendEmail(Instance instance, OS os) { return true; }
        public void Update(Instance instance, float time) {}
        public void OnGoalAdd(Instance instance, MisisonGoal goal) {}
        public void OnLoadInstance(Instance instance, Dictionary<string, string> objects) {}
    }
}
