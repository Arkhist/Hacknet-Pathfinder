using Hacknet;

namespace Pathfinder.Filesystem
{
    public interface IFilesystemObject
    {
        FileType Vanilla { get; }
        IFilesystemObject Parent { get; }
        bool IsRoot { get; }
        string Name { get; }
    }
}
