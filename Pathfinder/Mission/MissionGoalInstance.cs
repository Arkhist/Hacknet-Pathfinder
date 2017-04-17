using System.Collections.Generic;
using Pathfinder.Util;

namespace Pathfinder.Mission
{
    public class MissionGoalInstance : Hacknet.Mission.MisisonGoal
    {
        public Hacknet.ActiveMission Mission
        {
            get; internal set;
        }

        public IMissionGoal Interface
        {
            get; private set;
        }

        public string InterfaceId
        {
            get; private set;
        }

        internal MissionGoalInstance(IMissionGoal inter)
        {
            Interface = inter;
        }

        public static MissionGoalInstance CreateInstance(string id)
        {
            id = Utility.ConvertToValidXmlAttributeName(id);
            var inter = Handler.GetMissionGoalById(id);
            if (inter == null)
                return null;
            var i = new MissionGoalInstance(inter)
            {
                InterfaceId = id
            };
            return i;
        }

        public override bool isComplete(List<string> additionalDetails = null)
        {
            return Interface.IsComplete(this, additionalDetails ?? new List<string>());
        }

        public override void reset()
        {
            Interface.Reset(this);
        }

        public override string TestCompletable()
        {
            return Interface.GetTestCompletableString(this);
        }

        public void OnAdd(Hacknet.ActiveMission mission)
        {
            Mission = mission;
        }
    }
}
