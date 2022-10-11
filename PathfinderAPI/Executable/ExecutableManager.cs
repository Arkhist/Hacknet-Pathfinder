using System.Reflection;
using System.Text;
using Hacknet;
using Pathfinder.Event;
using Pathfinder.Event.Loading;
using Pathfinder.Event.Gameplay;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using HarmonyLib;
using Mono.Cecil.Cil;
using Pathfinder.Util;

namespace Pathfinder.Executable;

[HarmonyPatch]
public static class ExecutableManager
{
    private struct CustomExeInfo
    {
        public string ExeData;
        public string XmlId;
        public Type ExeType;
    }

    private static readonly List<CustomExeInfo?> CustomExes = new List<CustomExeInfo?>();
        
    static ExecutableManager()
    {
        EventManager<TextReplaceEvent>.AddHandler(GetTextReplacementExe);
        EventManager<ExecutableExecuteEvent>.AddHandler(OnExeExecute);
        EventManager.onPluginUnload += OnPluginUnload;
    }

    private static void GetTextReplacementExe(TextReplaceEvent e)
    {
        var exe = CustomExes.FirstOrDefault(x => x.Value.XmlId == e.Original);
        if (!exe.HasValue)
            return;
        e.Replacement = exe.Value.ExeData;
    }
    private static void OnExeExecute(ExecutableExecuteEvent e)
    {
        var exe = CustomExes.FirstOrDefault(x => x.Value.ExeData == e.ExecutableData);
        if (!exe.HasValue)
            return;
        var location = new Rectangle(e.OS.ram.bounds.X, e.OS.ram.bounds.Y + RamModule.contentStartOffset, RamModule.MODULE_WIDTH, (int)OS.EXE_MODULE_HEIGHT);
        if(exe.Value.ExeType.IsSubclassOf(typeof(GameExecutable)))
            e.OS.AddGameExecutable(
                (GameExecutable)Activator.CreateInstance(exe.Value.ExeType, null),
                location,
                e.Arguments.ToArray()
            );
        else
            e.OS.addExe((BaseExecutable)Activator.CreateInstance(exe.Value.ExeType, new object[] { location, e.OS, e.Arguments.ToArray() }));
        e.Result = ExecutionResult.StartupSuccess;
    }

    private static void OnPluginUnload(Assembly pluginAsm)
    {
        CustomExes.RemoveAll(x => x.Value.ExeType.Assembly == pluginAsm);
    }

    public static void RegisterExecutable<T>(string xmlName) where T : BaseExecutable => RegisterExecutable(typeof(T), xmlName);
    public static void RegisterExecutable(Type executableType, string xmlName)
    {
        executableType.ThrowNotInherit<BaseExecutable>(nameof(executableType));
        var builder = new StringBuilder();
        foreach (var exeByte in Encoding.ASCII.GetBytes("PathfinderExe:" + executableType.FullName))
            builder.Append(Convert.ToString(exeByte, 2));
        CustomExes.Add(new CustomExeInfo
        {
            ExeData = builder.ToString(),
            XmlId = xmlName,
            ExeType = executableType
        });
    }

    public static string GetCustomExeData(string xmlName) => CustomExes.FirstOrDefault(x => x.Value.XmlId == xmlName)?.ExeData;

    public static void UnregisterExecutable(string xmlName)
    {
        CustomExes.RemoveAll(x => x.Value.XmlId == xmlName);
    }
    public static void UnregisterExecutable<T>() => UnregisterExecutable(typeof(T));
    public static void UnregisterExecutable(Type exeType)
    {
        CustomExes.RemoveAll(x => x.Value.ExeType == exeType);
    }

    public static void AddGameExecutable(this OS os, GameExecutable exe, Rectangle location, string[] args)
    {
        exe.Assign(location, os, args);
        os.AddGameExecutable(exe);
    }
    public static void AddGameExecutable(this OS os, GameExecutable exe)
    {
        var computer = os.connectedComp ?? os.thisComputer;
        try
        {
            if(exe.needsProxyAccess && computer.proxyActive)
            {
                exe.OnProxyBypassFailure();
                if(!exe.IgnoreProxyFailPrint)
                    os.write(LocaleTerms.Loc("Proxy Active -- Cannot Execute"));
                return;
            }

            if(os.ramAvaliable >= exe.ramCost)
            {
                exe.OnInitialize();
                if(exe.CanAddToSystem)
                    os.exes.Add(exe);
                return;
            }

            exe.OnNoAvailableRam();
            if(!exe.IgnoreMemoryBehaviorPrint)
            {
                os.ram.FlashMemoryWarning();
                os.write(LocaleTerms.Loc("Insufficient Memory"));
            }
        }
        catch(Exception e)
        {
            if(!exe.CatchException(e))
                throw e;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(OS), nameof(OS.addExe))]
    private static bool AddExePrefix(OS __instance, ExeModule exe)
    {
        exe.os = __instance;
        if (exe is GameExecutable gameExe)
        {
            __instance.AddGameExecutable(gameExe);
            return false;
        }
        return true;
    }
    
    [HarmonyILManipulator]
    [HarmonyPatch(typeof(Programs), nameof(Programs.execute))]
    private static void programsExecuteFix(ILContext il){
        var c = new ILCursor(il);

        ILLabel branchCondition = null;

        c.GotoNext(MoveType.After,
            // for (int i = 0; i < folder.files.Count; i++)
            // int i = 0;
            x => x.MatchNop(),
            x => x.MatchLdcI4(0),
            x => x.MatchStloc(2),
            x => x.MatchBr(out branchCondition),
            // for (int j = 0; j < PortExploits.exeNums.Count; j++)
            // int j = 0;
            x => x.MatchNop()
        );

        c.Emit(OpCodes.Ldloc_1);
        c.Emit(OpCodes.Ldloc_2);
        c.Emit(OpCodes.Ldarg_1);

        c.EmitDelegate<Func<Folder, int, OS, bool>>((folder, i, os) => {
            if(CustomExes.Any(x => x.Value.ExeData == folder.files[i].data)){
                os.write(folder.files[i].name.Replace(".exe", ""));
                return true;
            }
            return false;
        });

        int i = c.Index;

        c.GotoLabel(branchCondition, MoveType.Before);
        c.Index -= 5;
        ILLabel branchIncrement = c.MarkLabel();

        c.Index = i;
        c.Emit(OpCodes.Brtrue, branchIncrement);
    }

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(OS), nameof(OS.Update), typeof(GameTime), typeof(bool), typeof(bool))]
    private static void onOSUpdate(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(MoveType.Before,
            x => x.MatchNop(),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld(AccessTools.Field(typeof(OS), nameof(OS.exes))),
            x => x.MatchLdloc(1),
            x => x.MatchCallvirt(AccessTools.Method(typeof(List<ExeModule>), nameof(List<ExeModule>.RemoveAt), new[] {typeof(int)}))
        );

        c.Emit(OpCodes.Ldarg, 0);
        c.Emit(OpCodes.Ldloc, 1);

        c.EmitDelegate<Action<OS, int>>((os, index) =>
        {
            if(os.exes[index] is GameExecutable exe)
                exe.Completed();
        });
    }

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(Programs), nameof(Programs.kill))]
    private static void programsKillFix(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        ILLabel leaveLabel = null;
        c.GotoNext(
            x => x.MatchLeaveS(out leaveLabel)
        );

        // os.write("Process " + num + "[" + os.exes[num2].IdentifierName + "] Ended");
        c.GotoPrev(MoveType.Before,
            x => x.MatchLdstr("] Ended")
        );
        c.Remove();
        c.Emit(OpCodes.Ldarg_1);
        c.Emit(OpCodes.Ldloc_1);
        c.EmitDelegate<Func<OS, int, bool>>((os, num2) =>
            os.exes[num2].CanKill()
        );
        c.Emit(OpCodes.Stloc_3); // no longer in use
        c.Emit(OpCodes.Ldloc_3);
        c.EmitDelegate<Func<bool, string>>(canKill =>
            canKill ? "] Ended" : "] Did not Respond"
        );

        // (no C# code)
        c.GotoNext(MoveType.After,
            x => x.MatchCallvirt(AccessTools.Method(typeof(OS), nameof(OS.write)))
        );
        c.Emit(OpCodes.Ldloc_3);
        c.Emit(OpCodes.Brfalse, leaveLabel);
    }
}
