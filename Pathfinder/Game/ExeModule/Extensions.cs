using Pathfinder.Game.OS;

namespace Pathfinder.Game.ExeModule
{
    public static class Extensions
    {
        /// <summary>
        /// Kill the ExeModule.
        /// </summary>
        /// <returns><c>true</c>, if ExeModule was found, killed, and removed, <c>false</c> otherwise.</returns>
        /// <param name="module">The ExeModule to kill.</param>
        /// <param name="shouldWrite">If set to <c>true</c> then success will be written to the OS.</param>
        public static bool Kill(this Hacknet.ExeModule module, bool shouldWrite = false)
        {
            if (!module.os.exes.Contains(module))
                return false;
            if(shouldWrite) module.os.Write("Process {0} [{1}] Ended", module.PID, module.IdentifierName);
            // not localized normally in game, TODO: override game localization of kill (and other commands)
            module.Killed();
            return module.os.exes.Remove(module);
        }
    }
}
