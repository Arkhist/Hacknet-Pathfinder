using Hacknet;
using Microsoft.Xna.Framework;

namespace Pathfinder.Executable
{
    public abstract class BaseExecutable : ExeModule
    {
        public abstract string GetIdentifier();

        public readonly string[] Args;

        public BaseExecutable(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem)
        {
            Args = args;
        }
    }
}
