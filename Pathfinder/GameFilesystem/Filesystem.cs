using System;
using System.Linq;
using System.Text;
using Hacknet;

namespace Pathfinder.GameFilesystem
{
    public class Filesystem : IFilesystemObject
    {
        private Directory root;

        public Hacknet.Computer Parent
        {
            get; private set;
        }

        public FileSystem Vanilla
        {
            get; private set;
        }

        IFilesystemObject IFilesystemObject.Parent
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        FileType IFilesystemObject.Vanilla
        {
            get
            {
                return Vanilla.root;
            }
        }

        public bool IsRoot
        {
            get
            {
                return true;
            }
        }

        public Filesystem Root => this;

        public int ParentIndex => -1;

        public string Name
        {
            get
            {
                return Root.Name;
            }
        }

        public string IpAccessor { get; set; }

        public Filesystem(Hacknet.Computer computer)
        {
            Parent = computer;
            Vanilla = computer.files;
        }

        public Directory RootDirectory
        {
            get
            {
                if (root == null)
                {
                    root = new Directory(this, Vanilla.root);
                    root.IsRoot = true;
                }
                return root;
            }
        }

        public static void RunToRoot(IFilesystemObject fso, Action<IFilesystemObject> funcToRun)
        {
            while (!fso.IsRoot)
            {
                funcToRun(fso);
                fso = fso.Parent;
            }
        }

        public static string GetPathFor(IFilesystemObject fso)
        {
            var path = new StringBuilder(" ");
            RunToRoot(fso, (f) => path = path.Insert(0, f.Name + '/'));
            return path.ToString().Trim();
        }

        public Directory GetDirectoryForPath(string path)
        {
            Directory result = RootDirectory, temp = result;
            var pathList = path.StartsWith("/", StringComparison.Ordinal) ? path.Split('/').Skip(1) : path.Split('/');
            foreach (var p in pathList)
            {
                if ((temp = temp.GetDirectoryByName(p)) != null)
                    result = temp;
                else
                    break;
            }
            return result;
        }

        public enum FileOpLogType
        {
            /// Input
            /// 0 = file owner's ip
            /// 1 = accessor's ip
            /// 2 = name
            /// 3 = data
            /// 4 = folderPath
            CreateFile,
            /// Input
            /// 0 = file owner's ip
            /// 1 = accessor's ip
            /// 2 = name
            /// 3 = folderPath
            CreateFolder,
            /// Input
            /// 0 = file owner's ip
            /// 1 = accessor's ip
            /// 2 = name
            /// 3 = parentIndex
            ReadFile,
            /// Input
            /// 0 = file owner's ip
            /// 1 = accessor's ip
            /// 2 = name
            CopyFile,
            /// Input
            /// 0 = file owner's ip
            /// 1 = accessor's ip
            /// 2 = name
            /// 3 = folderPath
            DeleteFile,
            /// Input
            /// 0 = file owner's ip
            /// 1 = accessor's ip
            /// 2 = name
            /// 3 = folderPath
            DeleteFolder,
            /// Input
            /// 0 = file owner's ip
            /// 1 = accessor's ip
            /// 2 = oldName
            /// 3 = newName
            /// 4 = oldFolderPath
            /// 5 = newFolderPath
            MoveFile,
            /// Input
            /// 0 = file owner's ip
            /// 1 = accessor's ip
            /// 2 = oldName
            /// 3 = newName
            /// 4 = oldFolderPath
            /// 5 = newFolderPath
            MoveFolder
        }

        public static bool LogOperation(IFilesystemObject fso, FileOpLogType t, params string[] input)
        {
            var root = fso.Root;
            string mpLog = "", gameLog = "";
            switch (t)
            {
                case FileOpLogType.CreateFile:
                    mpLog = "cMake #{0}#{1}#{2}#{3}#{4}";
                    gameLog = "FileCreated: by {1} - file:{2}";
                    input[4] = input[4].Replace('/', '#');
                    break;
                case FileOpLogType.CreateFolder:
                    mpLog = "cMkDir #{0}#{1}#{2}#{3}";
                    gameLog = "FolderCreated: by {1} - folder:{2}";
                    input[3] = input[3].Replace('/', '#');
                    break;
                case FileOpLogType.ReadFile:
                    mpLog = "cFile {0} {1} {2} {3}";
                    gameLog = "FileRead: by {1} - file:{2}";
                    break;
                case FileOpLogType.CopyFile:
                    mpLog = "cCopy {0} {1} {2}";
                    gameLog = "FileCopied: by {1} - file:{2}";
                    break;
                case FileOpLogType.DeleteFile:
                    mpLog = "cDelete #{0}#{1}#{2}#{3}";
                    gameLog = "FileDeleted: by {1} - file:{2}";
                    input[3] = input[3].Replace('/', '#');
                    break;
                case FileOpLogType.DeleteFolder:
                    mpLog = "cRmDir #{0}#{1}#{2}#{3}";
                    gameLog = "FolderDeleted: by {1} - file:{2}";
                    input[3] = input[3].Replace('/', '#');
                    break;
                case FileOpLogType.MoveFile:
                    mpLog = "cMove #{0}#{1}#{2}#{3}#{4}#{5}";
                    gameLog = "FileMoved: by {1} - file:{2} To: {3}";
                    input[4] = input[4].Replace('/', '%');
                    input[5] = input[5].Replace('/', '%');
                    break;
                case FileOpLogType.MoveFolder:
                    mpLog = "cMvDir #{0}#{1}#{2}#{3}#{4}#{5}";
                    gameLog = "FolderMoved: by {1} - file:{2} To: {3}";
                    input[4] = input[4].Replace('/', '%');
                    input[5] = input[5].Replace('/', '%');
                    break;
            }
            root.Parent.sendNetworkMessage(String.Format(mpLog, input));
            if (root.IpAccessor == null)
                return false;
            root.Parent.log(String.Format(gameLog, input));
            return true;
        }

        public static Filesystem GetPrimaryFilesystem()
        {
            return Hacknet.OS.currentInstance;
        }

        public static implicit operator Filesystem(Hacknet.OS os)
        {
            return os.thisComputer;
        }

        public static implicit operator Filesystem(Hacknet.Computer computer)
        {
            return new Filesystem(computer);
        }

        public static implicit operator FileSystem(Filesystem fs)
        {
            return fs.Vanilla;
        }
    }
}
