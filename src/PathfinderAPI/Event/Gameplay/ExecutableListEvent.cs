using Hacknet;
using HarmonyLib;

namespace Pathfinder.Event.Gameplay;

[HarmonyPatch]
public class ExecutableListEvent : PathfinderEvent
{
    public OS OS { get; }

    public List<string> EmbeddedExes { get; } = ["PortHack", "ForkBomb", "Shell", "Tutorial"];
    public Dictionary<FileEntry, bool> BinExes { get; }

    public ExecutableListEvent(OS os, Dictionary<FileEntry, bool> binExes)
    {
        OS = os;
        BinExes = binExes;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Programs), nameof(Programs.execute))]
    private static bool ProgramsExecuteReplacement(string[] args, OS os){
        var binFiles = os.thisComputer.files.root.searchForFolder("bin").files; // folders[2].files;

        var binExes = new Dictionary<FileEntry, bool>();
        foreach (FileEntry exeFile in binFiles)
            binExes[exeFile] =
                PortExploits.crackExeData        .Any(x => x.Value == exeFile.data) ||
                PortExploits.crackExeDataLocalRNG.Any(x => x.Value == exeFile.data);

        var programsExecute = new ExecutableListEvent(os, binExes);
        EventManager<ExecutableListEvent>.InvokeAll(programsExecute);

        os.write("Available Executables:\n");
        
        foreach (string embedded in programsExecute.EmbeddedExes)
            os.write(embedded);
        foreach (FileEntry file in binFiles.Where(x => binExes[x]))
            os.write(file.name.Replace(".exe", ""));

        os.write(" ");
        return false;
    }
}