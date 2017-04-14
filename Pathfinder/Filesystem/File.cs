using Hacknet;

namespace Pathfinder.Filesystem
{
    public class File : IFilesystemObject
    {
        private string path;

        public FileEntry Vanilla
        {
            get; private set;
        }

        FileType IFilesystemObject.Vanilla
        {
            get
            {
                return Vanilla;
            }
        }

        public IFilesystemObject Parent
        {
            get; private set;
        }

        public bool IsRoot
        {
            get
            {
                return false;
            }
        }

        public string Name
        {
            get
            {
                return Vanilla.getName();
            }
            set
            {
                Vanilla.name = value.Replace(" ", "_");
            }
        }

        public string Data
        {
            get
            {
                return Vanilla.data;
            }
            set
            {
                Vanilla.data = value;
                Vanilla.size = value.Length * 8;
            }
        }

        public int CreatedAt
        {
            get
            {
                return Vanilla.secondCreatedAt;
            }
        }

        public int Size
        {
            get
            {
                return Vanilla.size;
            }
        }

        public string Path
        {
            get
            {
                if (path == null)
                    path = Filesystem.GetPathFor(this);
                return path;
            }
        }

        public File(IFilesystemObject parent, FileEntry fe)
        {
            Parent = parent;
            Vanilla = fe;
        }

        public static implicit operator FileEntry(File f)
        {
            return f.Vanilla;
        }

        public static implicit operator string(File f)
        {
            return f.Data;
        }
    }
}
