using System;
using System.Collections.Generic;
using Hacknet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathfinder.Executable
{
    public class Instance : ExeModule
    {
        private Dictionary<string, object> keyToObject = new Dictionary<string, object>();

        public List<string> Arguments { get; private set; }
        public IInterface Interface { get; private set; }
        public FileEntry ExecutionFile { get; private set; }

        public Instance(Rectangle loc, Hacknet.OS os, List<string> args, FileEntry executionFile, IInterface exeInterface)
            : base(loc, os)
        {
            Arguments = args;
            Interface = exeInterface;
            ExecutionFile = executionFile;
            this.IdentifierName = Interface.Identifier;
            this.needsProxyAccess = Interface.NeedsProxyAccess;
            this.ramCost = Interface.RamCost;
            Interface.OnConstruction(this);
        }

        public static Instance CreateInstance(IInterface exeInterface,
                                              FileEntry executionFile,
                                              Hacknet.OS os,
                                              List<string> args,
                                              Rectangle loc)
        {
            if (exeInterface is IMainDisplayOverride)
                return new InstanceOverrideDisplay(loc, os, args, executionFile, exeInterface);
            return new Instance(loc, os, args, executionFile, exeInterface);
        }

        public static Instance CreateInstance(IInterface exeInterface, 
                                              FileEntry executionFile,
                                              Hacknet.OS os, 
                                              List<string> args) =>
            CreateInstance(exeInterface, executionFile, os, args, Rectangle.Empty);

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

        public override void LoadContent()
        {
            base.LoadContent();
            Interface.LoadContent(this);
        }

        public override void Completed()
        {
            base.Completed();
            Interface.OnComplete(this);
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            Interface.Draw(this, t);
        }

        public override void drawOutline()
        {
            if(Interface.DrawOutline(this))
                base.drawOutline();
        }

        public override void drawTarget(string typeName = "app:")
        {
            if(Interface.DrawTarget(this, typeName))
                base.drawTarget(typeName);
        }

        public override void Killed()
        {
            base.Killed();
            Interface.OnKilled(this);
        }

        public override void Update(float t)
        {
            var result = Interface.Update(this, t);
            if(result.HasValue)
                this.isExiting = result.Value;
            base.Update(t);
        }

        public override void PreDrawStep()
        {
            base.PreDrawStep();
            Interface.PreDraw(this);
        }

        public override void PostDrawStep()
        {
            base.PostDrawStep();
            Interface.PostDraw(this);
        }

        public class InstanceOverrideDisplay : Instance, MainDisplayOverrideEXE
        {
            private bool isOverrideable = true;

            public bool DisplayOverrideIsActive
            {
                get
                {
                    return isOverrideable && (Interface as IMainDisplayOverride).IsOverrideActive(this);
                }
                set
                {
                    isOverrideable = value;
                }
            }

            public InstanceOverrideDisplay(Rectangle loc, Hacknet.OS os, List<string> arguments, FileEntry executionFile, IInterface exeInterface)
                : base(loc, os, arguments, executionFile, exeInterface)
            {
                if (!(exeInterface is IMainDisplayOverride))
                    throw new ArgumentException("exeInterface must be derived from IMainDisplayOverride");
            }

            public void RenderMainDisplay(Rectangle dest, SpriteBatch sb) => 
                (Interface as IMainDisplayOverride).DrawMain(this, dest, sb);
        }
    }
}
