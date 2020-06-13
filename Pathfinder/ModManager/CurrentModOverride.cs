using System;

namespace Pathfinder.ModManager
{
    /* Helper class for scoped changes to Manager.CurrentMod */
    internal class CurrentModOverride : IDisposable
    {
        private IMod Previous;
        private bool Disposed = false;
        internal CurrentModOverride(IMod current)
        {
            Previous = Manager.CurrentMod;
            Manager.CurrentMod = current;
        }

        public void Dispose()
        {
            if(Disposed) return;
            Disposed = true;
            Manager.CurrentMod = Previous;
        }
    }
}
