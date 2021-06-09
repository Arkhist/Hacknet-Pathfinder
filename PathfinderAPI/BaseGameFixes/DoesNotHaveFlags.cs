using Hacknet;
using Pathfinder.Event;
using Pathfinder.Event.Loading.Content;

namespace Pathfinder.BaseGameFixes
{
    internal static class DoesNotHaveFlags
    {
        [Util.Initialize]
        internal static void Initialize()
        {
            EventManager<GetAdditionalConditionsEvent>.AddHandler(args =>
            {
                args.AdditonalConditions.Add(new GetAdditionalConditionsEvent.ConditionInfo { XmlName = "DoesNotHaveFlags", Callback = SCDoesNotHaveFlags.DeserializeFromReader});
            });
        }
    }
}
