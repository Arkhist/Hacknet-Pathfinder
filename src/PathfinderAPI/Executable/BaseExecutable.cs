using Hacknet;
using Microsoft.Xna.Framework;

namespace Pathfinder.Executable;

public abstract class BaseExecutable : ExeModule
{
    public string[] Args;

    public BaseExecutable(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem)
    {
        Args = args;
    }
}