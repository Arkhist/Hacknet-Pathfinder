using System;
using System.Collections.Generic;
using Pathfinder.Util;

namespace Pathfinder.Mission
{
    public class GoalInstance : Hacknet.Mission.MisisonGoal
    {
        private Dictionary<string, Tuple<bool, object>> keyToObject = new Dictionary<string, Tuple<bool, object>>();

        public Hacknet.ActiveMission Mission { get; internal set; }
        public IGoal Interface { get; private set; }
        public string InterfaceId { get; private set; }

        internal GoalInstance(IGoal inter) { Interface = inter; }

        public static GoalInstance CreateInstance(string id)
        {
            var inter = Handler.GetMissionGoalById(ref id);
            if (inter == null)
                return null;
            var i = new GoalInstance(inter) { InterfaceId = id };
            return i;
        }

        public object this[string key]
        {
            get
            {
                key = Utility.ConvertToValidXmlAttributeName(key);
                Tuple<bool, object> t;
                if (keyToObject.TryGetValue(key, out t))
                    return t.Item2;
                return null;
            }
            set
            {
                key = Utility.ConvertToValidXmlAttributeName(key);
                keyToObject[key] = new Tuple<bool, object>(false, value);
            }
        }

        public object GetInstanceData(string key) => this[key];
        public T GetInstanceData<T>(string key) => (T)GetInstanceData(key);

        public bool? IsInstanceSaveable(string key)
        {
            key = Utility.ConvertToValidXmlAttributeName(key);
            Tuple<bool, object> t;
            if (keyToObject.TryGetValue(key, out t))
                return t.Item1;
            return null;
        }

        public bool SetInstanceData(string key, object val, bool shouldSave = false)
        {
            key = Utility.ConvertToValidXmlAttributeName(key);
            var t = new Tuple<bool, object>(shouldSave, val);
            keyToObject[key] = t;
            return keyToObject[key] == t;
        }

        public void SetInstanceDataSaveable(string key, bool shouldSave)
        {
            key = Utility.ConvertToValidXmlAttributeName(key);
            Tuple<bool, object> t;
            if (keyToObject.TryGetValue(key, out t))
            {
                t = new Tuple<bool, object>(shouldSave, t.Item2);
                keyToObject[key] = t;
            }
        }

        public override bool isComplete(List<string> additionalDetails = null) =>
            Interface.IsComplete(this, additionalDetails ?? new List<string>());
        public override void reset() => Interface.Reset(this);
        public override string TestCompletable() => Interface.GetTestCompletableString(this);
        public void OnAdd(Hacknet.ActiveMission mission) => Mission = mission;

        public string SaveString
        {
            get
            {
                var str = "<moddedMissionGoal interfaceId=\"" + InterfaceId + "\" storedObjects=\"";
                foreach (var o in keyToObject) if (o.Value.Item1)
                    str += " " + o.Key + "|" + o.Value.Item2;
                return str + "/>";
            }
        }
    }
}
