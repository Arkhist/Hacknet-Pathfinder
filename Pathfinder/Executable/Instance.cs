using System;
using System.Collections.Generic;
using Hacknet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathfinder.Executable
{
    public class Instance : ExeModule
    {
        private FileEntry executingFile;
        private List<string> arguments;
        private IInterface exeInterface;
        private Dictionary<string, object> keyToObject = new Dictionary<string, object>();

        public List<string> Arguments
        {
            get
            {
                return arguments;
            }
        }

        public IInterface Interface
        {
            get
            {
                return exeInterface;
            }
        }

        public FileEntry ExecutionFile
        {
            get
            {
                return executingFile;
            }
        }

        public Instance(Rectangle loc, Hacknet.OS os, List<string> arguments, FileEntry executionFile, IInterface exeInterface)
            : base(loc, os)
        {
            this.arguments = arguments;
            this.exeInterface = exeInterface;
            this.executingFile = executionFile;
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

        public static Instance CreateInstance(IInterface exeInterface, FileEntry executionFile, Hacknet.OS os, List<string> args)
        {
            return CreateInstance(exeInterface, executionFile, os, args, Rectangle.Empty);
        }

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

        public object GetInstanceData(string key)
        {
            return this[key];
        }

        public T GetInstanceData<T>(string key)
        {
            return (T) GetInstanceData(key);
        }

        public bool SetInstanceData(string key, object val)
        {
            this[key] = val;
            return this[key] == val;
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
            this.IdentifierName = exeInterface.GetIdentifier(this);
            this.needsProxyAccess = exeInterface.NeedsProxyAccess(this);
            this.ramCost = exeInterface.GetRamCost(this);
            var result = exeInterface.Update(this, t);
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

        public class InstanceOverrideDisplay : Instance, MainDisplayOverrideEXE
        {
            private bool isOverrideAble = true;

            public bool DisplayOverrideIsActive
            {
                get
                {
                    return isOverrideAble && (exeInterface as IMainDisplayOverride).IsOverrideActive(this);
                }
                set
                {
                    isOverrideAble = value;
                }
            }

            public InstanceOverrideDisplay(Rectangle loc, Hacknet.OS os, List<string> arguments, FileEntry executionFile, IInterface exeInterface)
                : base(loc, os, arguments, executionFile, exeInterface)
            {
                if (!(exeInterface is IMainDisplayOverride))
                    throw new ArgumentException("exeInterface must be derived from IMainDisplayOverride");
            }

            public void RenderMainDisplay(Rectangle dest, SpriteBatch sb)
            {
                (exeInterface as IMainDisplayOverride).DrawMain(this, dest, sb);
            }
        }
    }
}
