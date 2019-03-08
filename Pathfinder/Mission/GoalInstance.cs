using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pathfinder.Mission
{
    public class GoalInstance : Hacknet.Mission.MisisonGoal
    {
        private Dictionary<string, Tuple<bool, object>> keyToObject = new Dictionary<string, Tuple<bool, object>>();

        public Hacknet.ActiveMission Mission { get; internal set; }
        public IGoal Interface { get; private set; }
        public string InterfaceId { get; private set; }

        internal GoalInstance(IGoal inter) { Interface = inter; }

        public static GoalInstance CreateInstance(string id, Dictionary<string, object> objects = null)
        {
            var inter = Handler.GetMissionGoalById(ref id);
            if (inter == null)
                return null;
            var i = new GoalInstance(inter)
            {
                InterfaceId = id,
                keyToObject = objects?.ToDictionary(p => p.Key, p => new Tuple<bool, object>(true, p.Value))
            };
            return i;
        }

        public object this[string key]
        {
            get
            {
                key = Util.Utility.ConvertToValidXmlAttributeName(key);
                Tuple<bool, object> t;
                if (keyToObject.TryGetValue(key, out t))
                    return t.Item2;
                return null;
            }
            set
            {
                key = Util.Utility.ConvertToValidXmlAttributeName(key);
                keyToObject[key] = new Tuple<bool, object>(false, value);
            }
        }

        public object GetInstanceData(string key) => this[key];
        public T GetInstanceData<T>(string key) => (T)GetInstanceData(key);

        public bool? IsInstanceSaveable(string key)
        {
            key = Util.Utility.ConvertToValidXmlAttributeName(key);
            Tuple<bool, object> t;
            if (keyToObject.TryGetValue(key, out t))
                return t.Item1;
            return null;
        }

        public bool SetInstanceData(string key, object val, bool shouldSave = false)
        {
            key = Util.Utility.ConvertToValidXmlAttributeName(key);
            var t = new Tuple<bool, object>(shouldSave, val);
            keyToObject[key] = t;
            return keyToObject[key] == t;
        }

        public void SetInstanceDataSaveable(string key, bool shouldSave)
        {
            key = Util.Utility.ConvertToValidXmlAttributeName(key);
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
                var str = new StringBuilder("<goal id=\"" + InterfaceId + "\">");
                foreach (var o in keyToObject) if (o.Value.Item1)
                    str.AppendLine("<"+o.Key+">" + o.Value.Item2 + "</" + o.Key + ">");
                return str.Append("</goal>").ToString();
            }
        }
    }
}
