using Hacknet;

namespace Pathfinder.GameFilesystem
{
    public class File : FileObject<FileEntry, Directory>
    {
        private string path;

        public File(FileEntry vanila, Directory parent) : base(vanila, parent)
        {
            Index = Parent.Object.files.BinarySearch(Object);
            path = Parent.Path + '/' + Name;
            Cast = (FileObject<object>)(IFileObject<object>)this;
        }

        public sealed override string Name
        {
            get
            {
                return Object.name;
            }

            set
            {
                LogOperation(FileOpLogType.MoveFile, value, Path, Parent.Path + '/' + Name);
                Object.name = value;
                path = Parent.Path + '/' + Name;
            }
        }

        public sealed override int Index
        {
            get; internal set;
        }

        public sealed override Filesystem Root => Parent.Root;

        public override string Path
        {
            get
            {
                return path;
            }

            set
            {
                var d = Root.SeacrhForDirectory(value.Remove(value.LastIndexOf('/')));
                if (d != null)
                    Parent = d;
                var name = value.Substring(value.LastIndexOf('/') + 1);
                name = name.Length > 0 ? name : Name;
                LogOperation(FileOpLogType.MoveFile, name, Path, Parent.Path + '/' + Name);
                Object.name = name;
                path = Parent.Path + '/' + Name;
            }
        }

        public string Data
        {
            get
            {
                return Object.data;
            }

            set
            {
                Object.data = value;
            }
        }

        public int Size
        {
            get
            {
                return Object.size;
            }

            set
            {
                Object.size = value;
            }
        }

        public void UpdateFileSize()
        {
            Size = Data.Length * 8;
        }

        public string Head { get { return Object.head(); } }

        public FileObject<object> Cast
        {
            get; private set;
        }

        public File MoveTo(Directory to)
        {
            return Parent.MoveFile(this, to);
        }
    }
}
