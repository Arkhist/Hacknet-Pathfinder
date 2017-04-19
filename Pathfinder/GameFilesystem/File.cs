using Hacknet;

namespace Pathfinder.GameFilesystem
{
    public class File : IFilesystemObject
    {
        private string path;
        private Filesystem root;
        private int? pindex;

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

        public int ParentIndex
        {
            get
            {
                if (!pindex.HasValue)
                    pindex = (Parent.Vanilla as Folder)?.files.BinarySearch(this);
                return pindex.GetValueOrDefault(-1);
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

        public Filesystem Root
        {
            get
            {
                if (root == null)
                {
                    IFilesystemObject fso = this;
                    Filesystem.RunToRoot(this, (f) => fso = f);
                    root = fso.Parent as Filesystem;
                }
                return root;
            }
        }

        public File(IFilesystemObject parent, FileEntry fe)
        {
            Parent = parent;
            Vanilla = fe;
        }

        public bool MoveTo(Directory d, string changeName = null)
        {
            if (changeName == null) changeName = Name;
            Filesystem.LogOperation(this, Filesystem.FileOpLogType.MoveFile,
                                    Root.Parent.ip, Root.IpAccessor, Name, changeName, (Parent as Directory)?.Path, d.Path);
            Name = changeName;
            (Parent as Directory)?.Vanilla?.files?.RemoveAt(ParentIndex);
            Parent = d;
            d.Vanilla.files.Add(Vanilla);
            ResetIndex();
            return d.Vanilla.files[ParentIndex] == this.Parent;
        }

        internal void ResetIndex()
        {
            pindex = null;
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
