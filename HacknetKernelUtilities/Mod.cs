using System;
using Pathfinder.ModManager;

using CommandHandler = Pathfinder.Command.Handler;

namespace KernelUtilities
{
    public class Mod : IMod
    {
        public string Identifier => "Hacknet Kernel Utilities";

        public void Load()
        {
            throw new NotImplementedException();
        }

        public void LoadContent()
        {
            CommandHandler.RegisterCommand("netmap", Commands.NetMap, "", true);
            CommandHandler.RegisterCommand("cp", Commands.Copy, "", true);

        }

        public void Unload()
        {
            throw new NotImplementedException();
        }
    }
}
