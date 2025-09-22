using Hacknet;
using Hacknet.Extensions;
using HarmonyLib;
using Pathfinder.Util;

namespace Pathfinder.BaseGameFixes;

[HarmonyPatch]
internal static class ReloadExtensionNodes
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ExtensionLoader), nameof(ExtensionLoader.ReloadExtensionNodes))]
    private static bool ReloadExtensionNodesReplacement(object osobj)
    {
        var os = (OS) osobj;
        if (OS.TestingPassOnly)
            return true;

        var nodesDir = ExtensionLoader.ActiveExtensionInfo.FolderPath + ExtensionInfo.NODES_FOLDER;
        if (!Directory.Exists(nodesDir))
            return false;
        Utils.ActOnAllFilesRevursivley(nodesDir, filename =>
        {
            if (!filename.EndsWith(".xml"))
                return;

            var computer = (Computer) ComputerLoader.loadComputer(filename, preventAddingToNetmap: true);
            if (computer is not null)
                UpdateNode(os, computer);
        });
        ComputerLookup.RebuildLookups();
        return false;
    }

    private static void UpdateNode(OS os, Computer node)
    {
        var oldNode = ComputerLookup.FindById(node.idName);
        if (oldNode is null)
            return;

        node.location = oldNode.location;
        node.adminIP  = oldNode.adminIP;
        node.ip       = oldNode.ip;
        node.highlightFlashTime = 1f;

        var i = os.netMap.nodes.IndexOf(oldNode);
        os.netMap.nodes[i] = node;
        ExtensionLoader.CheckAndAssignCoreServer(node, os);

        // bugfix: attempt to update old references
        void UpdateRef(ref Computer dst)
        {
            if (dst == oldNode)
                dst = node;
        }
        UpdateRef(ref os.connectedComp);

        foreach (var exeModule in os.exes)
        switch  (exeModule)
        {
            case MemoryDumpDownloader exe:
                UpdateRef(ref exe.target);
                break;
            case ExtensionSequencerExe exe:
                UpdateRef(ref exe.targetComp);
                break;
            case SequencerExe exe:
                UpdateRef(ref exe.targetComp);
                break;
            case DecypherTrackExe exe:
                UpdateRef(ref exe.targetComputer);
                // missing: update fs refs
                break;
            case DecypherExe exe:
                UpdateRef(ref exe.targetComputer);
                // missing: update fs refs
                break;
            case EOSDeviceScannerExe exe:
                UpdateRef(ref exe.targetComp);
                break;
            case PortHackExe exe:
                UpdateRef(ref exe.target);
                break;
            case ShellExe exe:
                UpdateRef(ref exe.destComp);
                UpdateRef(ref exe.compThisShellIsRunningOn);
                break;
        }
    }
}

