using Hacknet;
using Hacknet.Extensions;
using Hacknet.Gui;
using Hacknet.Screens;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Pathfinder.Event;
using Pathfinder.Event.Menu;

namespace Pathfinder.GUI;

[HarmonyPatch]
internal static class ArbitraryCodeWarning
{
    private static ExtensionInfo needsApproval = null;
    private static ExtensionInfo approvedInfo = null;
    private static string messages = null;
    private static ExtensionsMenuScreen screen = null;

    [Util.Initialize]
    internal static void Initialize()
    {
        EventManager<DrawMainMenuEvent>.AddHandler(OnDrawMainMenu);
    }

    private static PFButton Continue = new PFButton(650, 0, 130, 20, "Continue", new Color(255, 110, 110));
    private static PFButton Cancel = new PFButton(800, 0, 130, 20, "Cancel", new Color(50, 50, 50));

    private static void OnDrawMainMenu(DrawMainMenuEvent args)
    {
        if (needsApproval == null)
            return;

        GuiData.spriteBatch.Draw(Utils.white,
            new Rectangle(0, 0, GuiData.spriteBatch.GraphicsDevice.Viewport.Width,
                GuiData.spriteBatch.GraphicsDevice.Viewport.Height), new Color(0, 0, 0, 0.75f));

        TextItem.doLabel(new Vector2(650, 230), "Arbitary Code Warning", new Color(255, 130, 130));
        var endPfMessage = (int)TextItem.doMeasuredSmallLabel(new Vector2(650, 270),
            Utils.SuperSmartTwimForWidth(
                $"The extension {needsApproval.Name} contains DLLs inside the plugin folder that Pathfinder will attempt to load.\nThis will allow whatever code is in that DLL to be ran on your machine.\nPlease confirm that you acknowledge this and are comfortable with loading these plugins.\n\nLoading from {needsApproval.GetFullFolderPath()}",
                GuiData.spriteBatch.GraphicsDevice.Viewport.Width - 660, GuiData.smallfont), Color.White).Y + 280;
        var messageTextEnd = (int)TextItem.doMeasuredTinyLabel(new Vector2(650, endPfMessage), messages, Color.White).Y +
            endPfMessage + 10;
        Continue.Y = messageTextEnd;
        Cancel.Y = messageTextEnd;
        if (Continue.Do())
        {
            approvedInfo = needsApproval;
            needsApproval = null;
            screen.ActivateExtensionPage(approvedInfo);
        }
        else if (Cancel.Do())
        {
            needsApproval = null;
        }
    }

    [HarmonyPrefix]
    [HarmonyBefore("BepInEx.Hacknet.Chainloader")]
    [HarmonyPatch(typeof(ExtensionsMenuScreen), nameof(ExtensionsMenuScreen.ActivateExtensionPage))]
    private static bool ShowArbitraryWarning(ExtensionsMenuScreen __instance, ExtensionInfo info)
    {
        screen = __instance;
        if (approvedInfo == info)
        {
            approvedInfo = null;
            return true;
        }

        approvedInfo = null;

        string[] dlls = null;
        try
        {
            dlls = Directory.GetFiles(Path.Combine(info.GetFullFolderPath(), "Plugins"), "*.dll",
                SearchOption.AllDirectories);
        }
        catch (DirectoryNotFoundException)
        {
            return true;
        }

        List<string> warnings = [];
        foreach (var dll in dlls)
        {
            try
            {
                using (var asm = AssemblyDefinition.ReadAssembly(dll))
                {
                    foreach (var plugin in asm.MainModule.Types)
                    {
                        if (!plugin.HasCustomAttributes)
                            continue;
                        var pluginInfo =
                            plugin.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "BepInPlugin");
                        if (pluginInfo != null)
                            warnings.Add(
                                $"Plugin with the name \"{pluginInfo.ConstructorArguments[1].Value}\" from {Path.GetFileName(dll)}");
                    }
                }
            }
            catch
            {
                warnings.Add($"Could not process DLL {dll}, it's possibly dangerous!");
            }
        }

        if (warnings.Count == 0)
            return true;

        needsApproval = info;
        messages = Utils.SuperSmartTwimForWidth(
            string.Join("\n", warnings),
            GuiData.spriteBatch.GraphicsDevice.Viewport.Width - 660, GuiData.tinyfont
        );
        return false;
    }
}