using System.Collections.Generic;
using Hacknet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathfinder.Executable
{
    public class Instance : ExeModule, MainDisplayOverrideEXE
    {
        private Dictionary<string, object> keyToObject = new Dictionary<string, object>();

        public List<string> Arguments { get; private set; }
        public Interface Interface { get; private set; }
        public FileEntry ExecutionFile { get; private set; }

        public Instance(Rectangle loc, OS os, List<string> args, FileEntry executionFile, Interface exeInterface)
            : base(loc, os)
        {
            Arguments = args;
            Interface = exeInterface;
            ExecutionFile = executionFile;
            IdentifierName = Interface.Identifier;
            needsProxyAccess = Interface.NeedsProxyAccess;
            ramCost = Interface.RamCost;
            isInterfaceDisplayOverride = Interface is IMainDisplayOverride;
            Interface.OnConstruction(this);
        }

        public static Instance CreateInstance(Interface exeInterface,
                                              FileEntry executionFile,
                                              OS os,
                                              List<string> args,
                                              Rectangle loc)
        {
            return new Instance(loc, os, args, executionFile, exeInterface);
        }

        public static Instance CreateInstance(Interface exeInterface,
                                              FileEntry executionFile,
                                              OS os,
                                              List<string> args) =>
            CreateInstance(exeInterface, executionFile, os, args, Rectangle.Empty);

        private readonly bool isInterfaceDisplayOverride;
        private bool isOverrideable = true;
        public bool DisplayOverrideIsActive
        {
            get => isInterfaceDisplayOverride && isOverrideable && ((IMainDisplayOverride)Interface).IsOverrideActive(this);
            set => isOverrideable = value;
        }

        public object this[string key]
        {
            get
            {
                if (keyToObject.TryGetValue(key, out object o))
                    return o;
                return null;
            }
            set => keyToObject[key] = value;
        }

        public T GetInstanceData<T>(string key) => (T)this[key];

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
                isExiting = result.Value;
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

        public void RenderMainDisplay(Rectangle dest, SpriteBatch sb) =>
            ((IMainDisplayOverride)Interface).DrawMain(this, dest, sb);
    }
}
