using Hacknet;

namespace Pathfinder.GameFilesystem
{
    public class File : FileObject<FileEntry, Directory>
    {
        public File(FileEntry vanila, Directory parent) : base(vanila, parent)
        {
            Index = Parent.Object.files.IndexOf(Object);
            path = Parent.Path + FilePath.SEPERATOR + Name;
        }

        /// <summary>
        /// Gets or sets the File's name. (logs as move if change is attempted)
        /// </summary>
        public sealed override string Name
        {
            get
            {
                return Object.name;
            }

            set
            {
                Object.name = value;
                CorrectPath();
            }
        }

        internal void CorrectPath()
        {
            if (!Path.Contains(Parent.Path))
            {
                LogOperation(FileOpLogType.MoveFile, Name, Path, Parent.Path + FilePath.SEPERATOR + Name);
                path = Parent.Path + FilePath.SEPERATOR + Name;
            }
        }

        /// <summary>
        /// Gets the index inside its parent's respective list
        /// </summary>
        public sealed override int Index { get; internal set; }
        /// <summary>
        /// Gets the root Filesystem the File is within.
        /// </summary>
        public sealed override Filesystem Root => Parent.Root;
        /// <summary>
        /// Returns FileType.File
        /// </summary>
        public sealed override FileType Type => FileType.File;

        public sealed override FileProperties Properties
        {
            get { return Object.Properties; }
            set { Object.Properties = value; }
        }

        /// <summary>
        /// Gets or sets the File's full path, setting changes the File's parent.
        /// </summary>
        public override string Path
        {
            get
            {
                return path;
            }

            set
            {
                var d = Root.SearchForDirectory(value.Remove(value.LastIndexOf(FilePath.SEPERATOR)));
                if (d != null)
                    Parent = d;
                var name = value.Substring(value.LastIndexOf(FilePath.SEPERATOR) + 1);
                name = name.Length > 0 ? name : Name;
                LogOperation(FileOpLogType.MoveFile, name, Path, Parent.Path + FilePath.SEPERATOR + Name);
                Object.name = name;
                path = Parent.Path + FilePath.SEPERATOR + Name;
            }
        }

        /// <summary>
        /// Gets or sets the File's data.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the File's size.
        /// </summary>
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

        public int TimeCreated
        {
            get
            {
                return Object.secondCreatedAt;
            }

            set
            {
                Object.secondCreatedAt = value;
            }
        }

        /// <summary>
        /// Updates the size of the File according to the standard set in Hacknet.
        /// </summary>
        public void UpdateFileSize() => Size = Data.Length * 8;
        /// <summary>
        /// Retrieves the head of the file, or the first 50 characters of the File's data before a newline.
        /// </summary>
        public string Head => Object.head();
        /// <summary>
        /// Moves the File to a different Directory.
        /// </summary>
        /// <returns>The File after its been moved.</returns>
        /// <param name="to">The Directory to move the File to.</param>
        public File MoveTo(Directory to) => Parent.MoveFile(this, to);
    }
}
