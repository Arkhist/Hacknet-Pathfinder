using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathfinder.Daemon
{
    public class Instance : Hacknet.Daemon
    {
        private Interface daeInterface;
        private Dictionary<string, object> keyToObject = new Dictionary<string, object>();

        public Interface Interface
        {
            get
            {
                return daeInterface;
            }
        }

        public Instance(Hacknet.Computer computer, string serviceName, Hacknet.OS os, Interface daeInterface) : base(computer, serviceName, os)
        {
            this.daeInterface = daeInterface;
            daeInterface.OnCreate(this);
        }

        public object GetInstanceData(string key)
        {
            return keyToObject[key];
        }

        public T GetInstanceData<T>(string key)
        {
            return (T)GetInstanceData(key);
        }

        public bool SetInstanceData(string key, object val)
        {
            keyToObject[key] = val;
            return keyToObject[key] == val;
        }

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            base.draw(bounds, sb);
            daeInterface.Draw(this, bounds, sb);
        }

        public override string getSaveString()
        {
            return daeInterface.GetSaveString(this);
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
