using Hacknet.Extensions;
using HarmonyLib;
using MonoMod.Cil;

namespace Pathfinder.BaseGameFixes
{
    [HarmonyPatch]
    internal static class NoToLowerOnTheme
    {
        [HarmonyILManipulator]
        [HarmonyPatch(typeof(ExtensionInfo), nameof(ExtensionInfo.ReadExtensionInfo))]
        internal static void DontToLowerTheThemeFilename(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt(AccessTools.Method(typeof(string), nameof(string.ToLower))),
                x => x.MatchStloc(4)
            );

            c.Remove();
        }
    }
}
