using Hacknet;
using Hacknet.Factions;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Pathfinder.Port;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Replacements;

[HarmonyPatch]
public static class ReplacementsCommon
{
    public static MemoryContents LoadMemoryContents(ElementInfo info)
    {
        if (info.Children.TryGetElement("Memory", out var internalInfo)) info = internalInfo;
        var memory = new MemoryContents();

        if (info.Children.TryGetElement("Commands", out var commands))
        {
            foreach (var command in commands.Children.Select(x => x.Content).Where(PFStringExtensions.HasContent))
            {
                if (command.Contains("\n"))
                {
                    foreach (var trueCommand in command.Split(Utils.robustNewlineDelim, StringSplitOptions.RemoveEmptyEntries))
                    {
                        memory.CommandsRun.Add(trueCommand.HasContent() ? trueCommand.Filter() : " ");
                    }
                }
                else
                    memory.CommandsRun.Add(command.Filter());
            }
        }

        if (info.Children.TryGetElement("Data", out var data))
        {
            foreach (var block in data.Children)
            {
                memory.DataBlocks.Add(block.Content.Filter());
            }
        }

        if (info.Children.TryGetElement("FileFragments", out var files))
        {
            foreach (var file in files.Children)
            {
                memory.FileFragments.Add(new KeyValuePair<string, string>(
                    file.Attributes.GetString("name", "UNKNOWN"),
                    file.Content
                ));
            }
        }

        if (info.Children.TryGetElement("Images", out var images))
        {
            foreach (var image in images.Children)
            {
                memory.Images.Add(image.Content);
            }
        }
            
        return memory;
    }
        
    public static Faction LoadFaction(ElementInfo info)
    {
        Faction ret;

        var name = info.Attributes.GetString("name", "UNKNOWN");
        var needed = info.Attributes.GetInt("neededVal");
        switch (info.Name)
        {
            case "HubFaction":
                ret = new HubFaction(name, needed);
                break;
            case "EntropyFaction":
                ret = new EntropyFaction(name, needed);
                break;
            case "CustomFaction":
                var actions = new List<CustomFactionAction>();
                foreach (var actionSetInfo in info.Children)
                {
                    actions.Add(new CustomFactionAction()
                    {
                        ValueRequiredForTrigger = actionSetInfo.Attributes.GetInt("ValueRequired"),
                        FlagsRequiredForTrigger = actionSetInfo.Attributes.GetString("Flags", null),
                        TriggerActions = actionSetInfo.Children.Select(ActionsLoader.ReadAction).ToList()
                    });
                }
                    
                ret = new CustomFaction(name, 100)
                {
                    CustomActions = actions
                };
                break;
            default:
                ret = new Faction(name, needed);
                break;
        }

        ret.playerValue = info.Attributes.GetInt("playerVal");
        ret.idName = info.Attributes.GetString("id");
        ret.playerHasPassedValue = info.Attributes.GetBool("playerHasPassed");

        return ret;
    }

    internal static bool isPathfinderComputer = false;

    internal static readonly List<ElementInfo> defaultPorts =
    [
        new ElementInfo() { Name = "web" },
        new ElementInfo() { Name = "smtp" },
        new ElementInfo() { Name = "ftp" },
        new ElementInfo() { Name = "ssh" }
    ];

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Computer), MethodType.Constructor, [typeof(string), typeof(string), typeof(Vector2), typeof(int), typeof(byte), typeof(OS)])]
    private static void LoadDefaultPortsIfReplacement(Computer __instance)
    {
        if (!isPathfinderComputer)
        {
            PortManager.LoadPortsFromChildren(__instance, defaultPorts, false);
        }
    }
}
