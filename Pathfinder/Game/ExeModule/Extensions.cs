using Pathfinder.Game.OS;

namespace Pathfinder.Game.ExeModule
{
    public static class Extensions
    {
        public static bool Kill(this Hacknet.ExeModule module, bool shouldWrite = false)
        {
            if (!module.os.exes.Contains(module))
                return false;
            if(shouldWrite) module.os.Write("Process {0} [{1}] Ended", module.PID, module.IdentifierName);
            module.Killed();
            module.os.exes.Remove(module);
            return true;
        }
    }
}
