using System.Text;
using System.Diagnostics;
using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using BepInEx.Hacknet;
using BepInEx.Logging;
using Hacknet;
using Hacknet.PlatformAPI.Storage;
using Pathfinder.Command;
using Pathfinder.Options;

namespace Pathfinder;

[HarmonyPatch]
internal static class MiscPatches
{
    [HarmonyILManipulator]
    [HarmonyPatch(typeof(LocalDocumentsStorageMethod), nameof(LocalDocumentsStorageMethod.Load))]
    internal static void ChangeSaveDirIL(ILContext il)
    {
        ILCursor c = new ILCursor(il);
            
        string str = "./";

        while (c.TryGotoNext(MoveType.Before, x => x.MatchLdstr(out str)))
        {
            c.Next.Operand = str.Replace("Hacknet", "HacknetPathfinder");
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HacknetPlugin), nameof(HacknetPlugin.Unload))]
    internal static void OnPluginUnload(ref HacknetPlugin __instance) => Event.EventManager.InvokeOnPluginUnload(__instance.GetType().Assembly);
        
    [HarmonyILManipulator]
    [HarmonyPatch(typeof(MainMenu), "<HookUpCreationEvents>b__1")]
    internal static void StopCatchingExceptionsIL(ILContext il)
    {
        ILCursor c = new ILCursor(il);
            
        foreach (var exHandler in il.Body.ExceptionHandlers)
        {
            c.Goto(exHandler.HandlerEnd, MoveType.After);
            var end = c.Index;
            c.Goto(exHandler.HandlerStart, MoveType.Before);
            c.RemoveRange(end - c.Index);
        }
            
        il.Body.ExceptionHandlers.Clear();
            
        foreach (var brokenLabel in il.Labels.Where(x => !c.Instrs.Contains(x.Target))) brokenLabel.Target = c.Instrs.Last();
    }
        
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Utils), nameof(Utils.SendRealWorldEmail))]
    internal static bool FixSendRealWorldEmail(string body)
    {
        Logger.Log(LogLevel.Error, body.Substring(body.IndexOf("\r\n", StringComparison.Ordinal) + 2));
        return false;
    }

    private static readonly ManualLogSource HacknetLogger = BepInEx.Logging.Logger.CreateLogSource("Hacknet");
        
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Utils), nameof(Utils.AppendToErrorFile))]
    internal static bool RedirectErrorLog(string text)
    {
        HacknetLogger.LogError(text);
        return false;
    }

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(OS), nameof(OS.threadExecute))]
    internal static void LogThreadedExceptions(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        c.GotoNext(MoveType.After, x => x.MatchPop());

        c.Prev.OpCode = OpCodes.Nop;
        while (!c.Next!.MatchLeave(out _)) c.Remove();
        c.EmitDelegate<Action<Exception>>(ex =>
        {
            HacknetLogger.LogError(new Exception("Exception occurred during threaded execute", ex));
        });
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Program), nameof(Program.Main))]
    private static void CheckDebug(string[] args)
    {
        if (args.Any(x => x.Equals("-enabledebug", StringComparison.OrdinalIgnoreCase)))
            DebugCommands.AddCommands();
    }
}