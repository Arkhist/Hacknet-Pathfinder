using System.Xml.Linq;
using Hacknet;
using Pathfinder.Replacements;
using Pathfinder.Util.XML;

namespace Pathfinder.Action;

public sealed class ActionDelayDecorator : DelayablePathfinderAction
{
    public static SerializableAction Create(ElementInfo info, SerializableAction action)
    {
        if (info.Attributes.ContainsKey("Delay") && info.Attributes.ContainsKey("DelayHost"))
            return new ActionDelayDecorator(info, action);

        return action;
    }

    SerializableAction targetAction;

    public ActionDelayDecorator(ElementInfo info, SerializableAction action)
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