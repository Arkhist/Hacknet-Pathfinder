using HarmonyLib;
using Hacknet;

namespace Pathfinder.Event.Loading
{
    [HarmonyPatch]
    public class TextReplaceEvent : PathfinderEvent
    {
        public string Original { get; private set; }
        public string Replacement { get; set; }

        public TextReplaceEvent(string original, string replacement)
        {
            Original = original;
            Replacement = replacement;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ComputerLoader), nameof(ComputerLoader.filter))]
        internal static void TextFilterPostfix(ref string s, ref string __result)
        {
            var textReplaceEvent = new TextReplaceEvent(s, __result);
            EventManager<TextReplaceEvent>.InvokeAll(textReplaceEvent);
            __result = textReplaceEvent.Replacement;
        }
    }
}
