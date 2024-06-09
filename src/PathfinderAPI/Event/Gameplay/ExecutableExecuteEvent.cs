using System.ComponentModel;
using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Hacknet;

namespace Pathfinder.Event.Gameplay;

[HarmonyPatch]
public class ExecutableExecuteEvent : PathfinderEvent
{
    public Computer Computer { get; private set; }
    public OS OS { get; private set; }
    public string ExecutableName
    {
        get { return ExeFile?.name; }
        set
        {
            if (ExeFile == null || value == null)
                return;
            ExeFile.name = value;
        }
    }
    public string ExecutableData
    {
        get { return ExeFile?.data; }
        set
        {
            if (ExeFile == null || value == null)
                return;
            ExeFile.data = value;
        }
    }
    public List<string> Arguments { get; private set; }
    public Folder ExeFolder { get; private set; }
    public int FileIndex { get; private set; }
    public FileEntry ExeFile { get; private set; }
    private ExecutionResult _result = ExecutionResult.NotFound;
    public ExecutionResult Result
    {
        get => _result;
        set
        {
            _result = value;
            if (value == ExecutionResult.Cancelled)
                Cancelled = true;
        }
    }

    public ExecutableExecuteEvent(Computer com, OS os, Folder fol, int finde, FileEntry file, string[] args)
    {
        Computer = com;
        OS = os;
        ExeFolder = fol;
        FileIndex = finde;
        ExeFile = file;
        Arguments = [..args ?? new string[0]];
    }

    public string this[int index]
    {
        get
        {
            if (Arguments.Count <= index)
                return "";
            return Arguments[index];
        }
    }

    delegate int InjectDelegate(ref Computer com, ref Folder fol, ref int founde, ref string exeData, ref OS os, ref string[] args);

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(ProgramRunner), nameof(ProgramRunner.AttemptExeProgramExecution))]
    private static void onExecutableExecuteIL(ILContext il, ILLabel retLabel)
    {
        ILCursor c = new ILCursor(il);

        c.GotoNext(MoveType.Before,
            x => x.MatchNop(),
            x => x.MatchLdloc(1),
            x => x.MatchLdfld(AccessTools.Field(typeof(Folder), nameof(Folder.files)))
        );

        c.Emit(OpCodes.Ldloca, 0);
        c.Emit(OpCodes.Ldloca, 1);
        c.Emit(OpCodes.Ldloca, 2);
        c.Emit(OpCodes.Ldloca, 6);
        c.Emit(OpCodes.Ldarga, 0);
        c.Emit(OpCodes.Ldarga, 1);

        c.EmitDelegate<InjectDelegate>((ref Computer com, ref Folder fol, ref int founde, ref string exeData, ref OS os, ref string[] args) =>
        {
            FileEntry f = fol.files[founde];

            var executableExecuteEvent = new ExecutableExecuteEvent(com, os, fol, founde, f, args);
            EventManager<ExecutableExecuteEvent>.InvokeAll(executableExecuteEvent);

            return (int)(executableExecuteEvent.Cancelled ? ExecutionResult.Cancelled : executableExecuteEvent.Result);
        });

        c.Emit(OpCodes.Dup);
        c.Emit(OpCodes.Ldc_I4_0);
        var label = c.DefineLabel();
        c.Emit(OpCodes.Blt, label);
        c.Emit(OpCodes.Br, retLabel);
        c.Emit(OpCodes.Pop);
        label.Target = c.Prev;
    }
}

[DefaultValue(NotFound)]
public enum ExecutionResult
{
    NotFound = -1,
    Error = 0,
    StartupSuccess = 1,
    Cancelled,
}
