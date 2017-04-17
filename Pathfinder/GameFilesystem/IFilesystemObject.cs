using Hacknet;

namespace Pathfinder.GameFilesystem
{
    public interface IFilesystemObject
    {
        FileType Vanilla { get; }
        IFilesystemObject Parent { get; }
        bool IsRoot { get; }
        string Name { get; }
    }
}
