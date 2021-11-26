using System;
using Hacknet;
using Microsoft.Xna.Framework;

namespace Pathfinder.Executable
{
    public abstract class BaseExecutable : ExeModule
    {
        [Obsolete("To be removed in 6.0.0")]
        public abstract string GetIdentifier();

        public string[] Args;

        public BaseExecutable(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem)
        {
            Args = args;
        }
    }
}
