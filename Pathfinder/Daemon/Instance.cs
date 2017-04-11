using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathfinder.Daemon
{
    public class Instance : Hacknet.Daemon
    {
        private IInterface daeInterface;
        private Dictionary<string, Tuple<bool, object>> keyToObject = new Dictionary<string, Tuple<bool, object>>();

        public IInterface Interface
        {
            get
            {
                return daeInterface;
            }
        }

        public string InterfaceId
        {
            get; internal set;
        }

        public Instance(Hacknet.Computer computer, string serviceName, Hacknet.OS os, IInterface daeInterface) : base(computer, serviceName, os)
        {
            this.daeInterface = daeInterface;
            daeInterface.OnCreate(this);
        }

        public static Instance CreateInstance(Hacknet.Computer computer, string serviceName, Hacknet.OS os, IInterface daeInterface)
        {
            return new Instance(computer, serviceName, os, daeInterface);
        }

        public static Instance CreateInstance(Event.LoadComputerXmlReadEvent e, IInterface daeInterface)
        {
            var i = CreateInstance(e.Computer, daeInterface.InitialServiceName, e.Computer.os, daeInterface);
            daeInterface.LoadInstance(i, e.Reader);
            return i;
        }

        public object GetInstanceData(string key)
        {
            Tuple<bool, object> t;
            if (keyToObject.TryGetValue(key, out t))
               return t.Item2;
            return null;
        }

        public T GetInstanceData<T>(string key)
        {
            return (T)GetInstanceData(key);
        }

        public bool? IsInstanceSaveable(string key)
        {
            Tuple<bool, object> t;
            if (keyToObject.TryGetValue(key, out t))
               return t.Item1;
            return null;
        }

        public bool SetInstanceData(string key, object val, bool shouldSave = false)
        {
            var t = new Tuple<bool, object>(shouldSave, val);
            keyToObject[key] = t;
            return keyToObject[key] == t;
        }

        public void SetInstanceDataSaveable(string key, bool shouldSave)
        {
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
            daeInterface.Draw(this, bounds, sb);
        }

        public override string getSaveString()
        {
            var str = "<"+InterfaceId;
            foreach (var o in keyToObject)
            {
                if(o.Value.Item1)
                    str+= " " + o.Key + "=\"" + o.Value.Item2.ToString() + "\"";
            }
            str += " />";
            return str;
        }

        public override void initFiles()
        {
            base.initFiles();
            daeInterface.InitFiles(this);
        }

        public override void loadInit()
        {
            base.loadInit();
            daeInterface.LoadInit(this);
        }

        public override void navigatedTo()
        {
            base.navigatedTo();
            daeInterface.OnNavigatedTo(this);
        }

        public override void userAdded(string name, string pass, byte type)
        {
            base.userAdded(name, pass, type);
            daeInterface.OnUserAdded(this, name, pass, type);
        }
    }
}
