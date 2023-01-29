using Hacknet;

namespace Pathfinder.Executable;

public static class ExeModuleExtensions
{
    public static bool CanKill(this ExeModule module) =>
        !(
            !module.os.exes.Contains(module) ||
            ((module is GameExecutable gameExe) && !gameExe.CanBeKilled) ||
            ((module is DLCIntroExe introExe) && !((introExe.State == DLCIntroExe.IntroState.NotStarted) || (introExe.State == DLCIntroExe.IntroState.Exiting))) ||
            ((module is ExtensionSequencerExe seqExe) && (seqExe.state == ExtensionSequencerExe.SequencerExeState.Active))
        );

    public static bool Kill(this ExeModule module)
    {
        if(!module.CanKill())
            return false;
        module.Killed();
        return module.os.exes.Remove(module);
    }
}