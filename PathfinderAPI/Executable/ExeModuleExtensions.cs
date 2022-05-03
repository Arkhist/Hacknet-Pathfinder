using Hacknet;

namespace Pathfinder.Executable;

public static class ExeModuleExtensions
{
    public static bool Kill(this ExeModule module)
    {
        if(!module.os.exes.Contains(module))
            return false;
        module.Killed();
        return module.os.exes.Remove(module);
    }
}