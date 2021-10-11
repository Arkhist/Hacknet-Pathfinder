using System;
using System.Xml.Linq;
using Hacknet;
using Pathfinder.Replacements;
using Pathfinder.Util.XML;

namespace Pathfinder.Action
{
    public sealed class DelayAction : DelayablePathfinderAction
    {
        public static SerializableAction Create(ElementInfo info, SerializableAction action)
        {
            if (info.Attributes.ContainsKey("Delay") && info.Attributes.ContainsKey("DelayHost"))
                return new DelayAction(info, action);

            return action;
        }

        SerializableAction targetAction;

        public DelayAction(ElementInfo info, SerializableAction action)
        {
            LoadFromXml(info);
            targetAction = action;
        }

        public override void Trigger(OS os)
        {
            targetAction.Trigger(os);
        }

        public override XElement GetSaveElement()
        {
            XElement element = SaveWriter.GetActionSaveElement(targetAction);
            element.SetAttributeValue("Delay", Delay);
            element.SetAttributeValue("DelayHost", DelayHost);
            return element;
        }
    }
}
