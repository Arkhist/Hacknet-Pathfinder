using System;
using System.Xml;
using Hacknet;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Action
{
    public abstract class DelayablePathfinderAction : PathfinderAction
    {
        [XMLStorage]
        public string DelayHost; 
        [XMLStorage]
        public string Delay;

        private DelayableActionSystem delayHost;
        private float delay = 0f;
        
        public sealed override void Trigger(object os_obj)
        {
            if (delayHost == null && DelayHost != null)
            {
                var delayComp = Programs.getComputer(OS.currentInstance, DelayHost);
                if (delayComp == null)
                    throw new FormatException($"{this.GetType().Name}: DelayHost could not be found");
                delayHost = DelayableActionSystem.FindDelayableActionSystemOnComputer(delayComp);
            }
            
            if (delay <= 0f || delayHost == null)
            {
                Trigger((OS)os_obj);
                return;
            }

            delayHost.AddAction(this, delay);
            delay = 0f;
        }

        public abstract void Trigger(OS os);

        public override void LoadFromXml(ElementInfo info)
        {
            base.LoadFromXml(info);
            
            if (Delay != null && !float.TryParse(Delay, out delay))
                throw new FormatException($"{this.GetType().Name}: Couldn't parse delay time!");
        }
    }
}
