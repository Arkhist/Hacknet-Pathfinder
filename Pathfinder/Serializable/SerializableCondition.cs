using System.Collections.Generic;
using Hacknet;
using Pathfinder.Util;

namespace Pathfinder.Serializable.Condition
{
    public interface Interface
    {
        bool OnCheck(OS os);
        void OnRead(Instance instance, SaxProcessor.ElementInfo info);
    }

    public class Instance : Hacknet.SerializableCondition
    {
        private Dictionary<string, object> keyToObject = new Dictionary<string, object>();

        public Interface Interface { get; internal set; }

        public Instance(Interface i)
        {
            Interface = i;
        }

        public static Instance CreateInstance(string id, Dictionary<string, object> objects = null)
        {
            var inter = Handler.GetConditionById(ref id);
            if (inter == null)
                return null;
            var i = new Instance(inter)
            {
                keyToObject = objects ?? new Dictionary<string, object>()
            };
            return i;
        }

        public override bool Check(object os_obj) => Interface.OnCheck((OS)os_obj);
        public void OnRead(SaxProcessor.ElementInfo info) => Interface.OnRead(this, info);

        public object this[string key]
        {
            get
            {
                object o;
                if (keyToObject.TryGetValue(key, out o))
                    return o;
                return null;
            }
            set
            {
                keyToObject[key] = value;
            }
        }

        public object GetInstanceData(string key) => this[key];
        public T GetInstanceData<T>(string key) => (T)GetInstanceData(key);

        public bool SetInstanceData(string key, object val)
        {
            this[key] = val;
            return this[key] == val;
        }
    }
}
