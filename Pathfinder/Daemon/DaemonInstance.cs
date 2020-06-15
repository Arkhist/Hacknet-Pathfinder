using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Util;

namespace Pathfinder.Daemon
{
    public class Instance : Hacknet.Daemon
    {
        private Dictionary<string, Tuple<bool, object>> keyToObject = new Dictionary<string, Tuple<bool, object>>();

        public Interface Interface {  get; private set; }

        public string InterfaceId { get; private set; }

        internal Instance(Hacknet.Computer computer, string serviceName, Hacknet.OS os, Interface daeInterface)
            : base(computer, serviceName, os)
        {
            Interface = daeInterface;
            Interface.OnCreate(this);
        }

        public static Instance CreateInstance(string id,
                                              Hacknet.Computer computer,
                                              Dictionary<string, string> objects)
        {
            var inter = Handler.GetDaemonById(ref id);
            if (inter == null)
                return null;
            var i = new Instance(computer, inter.InitialServiceName, computer.os, inter)
            {
                InterfaceId = id
            };
            i.Interface.LoadInstance(i, objects ?? new Dictionary<string, string>());
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

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            base.draw(bounds, sb);
            Interface.Draw(this, bounds, sb);
        }

        public override string getSaveString()
        {
            var str = "<moddedDamon interfaceId=\"" + InterfaceId + "\" storedObjects=\"";
            foreach (var o in keyToObject)
                if (o.Value.Item1)
                    str += " " + o.Key + "|" + o.Value.Item2;
            str += "\" />";
            return str;
        }

        public override void initFiles()
        {
            base.initFiles();
            Interface.InitFiles(this);
        }

        public override void loadInit()
        {
            base.loadInit();
            Interface.LoadInit(this);
        }

        public override void navigatedTo()
        {
            base.navigatedTo();
            Interface.OnNavigatedTo(this);
        }

        public override void userAdded(string name, string pass, byte type)
        {
            base.userAdded(name, pass, type);
            Interface.OnUserAdded(this, name, pass, type);
        }
    }
}
