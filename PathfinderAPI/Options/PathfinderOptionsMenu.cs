using HarmonyLib;
using MonoMod.Cil;
using Hacknet;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Pathfinder.Event;
using Pathfinder.Event.Options;
using Pathfinder.GUI;

namespace Pathfinder.Options;

[HarmonyPatch]
internal static class PathfinderOptionsMenu
{
    private static bool isInPathfinderMenu = false;
    public static string CurrentTabId { get; private set; } = null;

    public static void SetCurrentTab(string tabId)
    {
        #pragma warning disable 618
        if(OptionsManager.PluginTabs.Any(pt => pt.Id == tabId) || OptionsManager.Tabs.ContainsKey(tabId))
        {
            CurrentTabId = tabId;
            return;
        }
        throw new InvalidOperationException($"Tab {tabId} not found in {typeof(OptionsManager).FullName}.{nameof(OptionsManager.PluginTabs)} or {typeof(OptionsManager).FullName}.{nameof(OptionsManager.Tabs)}");
        #pragma warning restore 618
    }

    public static PluginOptionTab SetCurrentTab(PluginOptionTab tab)
    {
        if(!OptionsManager.PluginTabs.Contains(tab))
            throw new InvalidOperationException($"Tab {tab.Id} not found in {typeof(OptionsManager).FullName}.{nameof(OptionsManager.PluginTabs)}");
        CurrentTabId = tab.Id;
        return tab;
    }

    #pragma warning disable 618
    public static OptionsTab SetCurrentTab(OptionsTab tab)
    {
        if(!OptionsManager.Tabs.TryGetValue(tab.Name, out var newTab) || tab != newTab)
            throw new InvalidOperationException($"Tab {tab.Name} not found in {typeof(OptionsManager).FullName}.{nameof(OptionsManager.Tabs)}");
        CurrentTabId = tab.Name;
        return tab;
    }
    #pragma warning restore 618

    private static PFButton ReturnButton = new PFButton(10, 10, 220, 54, "Back to Options", Color.Yellow);

    [HarmonyPrefix]
    [HarmonyPatch(typeof(OptionsMenu), nameof(OptionsMenu.Draw))]
    internal static bool Draw(ref OptionsMenu __instance, GameTime gameTime)
    {
        if (!isInPathfinderMenu)
            return true;

        PostProcessor.begin();
			GuiData.startDraw();
			PatternDrawer.draw(new Rectangle(0, 0, __instance.ScreenManager.GraphicsDevice.Viewport.Width, __instance.ScreenManager.GraphicsDevice.Viewport.Height), 0.5f, Color.Black, new Color(2, 2, 2), GuiData.spriteBatch);

        if (ReturnButton.Do())
        {
            CurrentTabId = null;
            isInPathfinderMenu = false;
            GuiData.endDraw();
            PostProcessor.end();
            var saveEvent = new CustomOptionsSaveEvent();
            EventManager<CustomOptionsSaveEvent>.InvokeAll(saveEvent);
            return false;
        }

        #pragma warning disable 618
        var tabs = OptionsManager.Tabs;
        #pragma warning restore 618

        int tabX = 10;

        foreach (var tab in OptionsManager.PluginTabs)
        {
            if(tab.ButtonData.X == null)
            {
                tab.ButtonData.Set(tabX);
                tabX += 10 + tab.ButtonData.Width.GetValueOrDefault();
            }
            tab.OnDraw(gameTime);
        }

        foreach (var tab in tabs.Values)
        {
            if (CurrentTabId == null)
                CurrentTabId = tab.Name;
            var active = CurrentTabId == tab.Name;
            // Display tab button
            if (Button.doButton(tab.ButtonID, tabX, 70, 128, 20, tab.Name, active ? Color.Green : Color.Gray))
            {
                CurrentTabId = tab.Name;
                break;
            }
            tabX += 128 + 10;

            if (CurrentTabId != tab.Name)
                continue;

            // Display options
            int optX = 80, optY = 110;
            foreach (var option in tab.Options)
            {
                option.Draw(optX, optY);
                optY += 10 + option.SizeY;
            }
        }

        GuiData.endDraw();
			PostProcessor.end();
        return false;
    }

    private static PFButton EnterButton = new PFButton(240, 10, 220, 54, "Pathfinder Options", Color.Yellow);

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(OptionsMenu), nameof(OptionsMenu.Draw))]
    internal static void BeforeEndDrawOptions(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        c.GotoNext(MoveType.AfterLabel, x => x.MatchCallOrCallvirt(AccessTools.Method(typeof(GuiData), nameof(GuiData.endDraw))));

        c.EmitDelegate<System.Action>(() =>
        {
            if (EnterButton.Do())
            {
                isInPathfinderMenu = true;
            }
        });
    }
}
