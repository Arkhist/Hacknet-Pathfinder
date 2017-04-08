using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Pathfinder.Executable
{
    public class Instance : Hacknet.ExeModule
    {
        private List<string> parameters;
        private Interface exeInterface;
        private Dictionary<string, object> keyToObject = new Dictionary<string, object>();

        public List<string> Parameters
        {
            get
            {
                return parameters;
            }
        }

        public Interface Interface
        {
            get
            {
                return exeInterface;
            }
        }

        internal Instance(Rectangle loc, Hacknet.OS os, List<string> parameters, Interface exeInterface) : base(loc, os)
        {
            this.parameters = parameters;
            this.exeInterface = exeInterface;
        }

        public object GetInstanceData(string key)
        {
            return keyToObject[key];
        }

        public T GetInstanceData<T>(string key)
        {
            return (T) GetInstanceData(key);
        }

        public bool SetInstanceData(string key, object val)
        {
            keyToObject[key] = val;
            return keyToObject[key] == val;
        }

        public override void LoadContent()
        {
            base.LoadContent();
            exeInterface.LoadContent(this);
        }

        public override void Completed()
        {
            base.Completed();
            exeInterface.OnComplete(this);
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            exeInterface.Draw(this, t);
        }

        public override void drawOutline()
        {
            if(exeInterface.DrawOutline(this))
                base.drawOutline();
        }

        public override void drawTarget(string typeName = "app:")
        {
            if(exeInterface.DrawTarget(this, typeName))
                base.drawTarget(typeName);
        }

        public override void Killed()
        {
            base.Killed();
            exeInterface.OnKilled(this);
        }

        public override void Update(float t)
        {
            this.IdentifierName = exeInterface.GetIdentifer(this);
            this.needsProxyAccess = exeInterface.NeedsProxyAccess(this);
            this.ramCost = exeInterface.GetRamCost(this);
            bool? result = exeInterface.Update(this, t);
            if(result.HasValue)
                this.isExiting = result.Value;
            base.Update(t);
        }

        public override void PreDrawStep()
        {
            base.PreDrawStep();
            exeInterface.PreDraw(this);
        }

        public override void PostDrawStep()
        {
            base.PostDrawStep();
            exeInterface.PostDraw(this);
        }
    }
}
