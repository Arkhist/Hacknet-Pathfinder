using Hacknet;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Pathfinder.Event.Menu;

[HarmonyPatch]
public class DrawMainMenuEvent : MainMenuEvent
{
    public DrawMainMenuEvent(MainMenu mainMenu) : base(mainMenu)
    {
    }

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(MainMenu), nameof(MainMenu.Draw))]
    private static void AfterMainMenuDraw(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        c.GotoNext(MoveType.AfterLabel,
            x => x.MatchCallOrCallvirt(typeof(GuiData), nameof(GuiData.endDraw))
        );

        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Newobj, AccessTools.DeclaredConstructor(typeof(DrawMainMenuEvent), [typeof(MainMenu)]));
        c.Emit(OpCodes.Call, AccessTools.DeclaredMethod(typeof(EventManager<DrawMainMenuEvent>), nameof(EventManager<DrawMainMenuEvent>.InvokeAll)));
    }
}