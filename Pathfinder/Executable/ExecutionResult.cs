using System.ComponentModel;

namespace Pathfinder.Executable
{
    [DefaultValue(NotFound)]
    public enum ExecutionResult
    {
        NotFound = -1,
        Error = 0,
        StartupSuccess = 1
    }
}
