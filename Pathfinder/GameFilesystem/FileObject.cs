using System;
using System.Collections.Generic;

namespace Pathfinder.GameFilesystem
{
    public abstract class FileObject<VanillaT, ParentT> : FileObject<VanillaT>
        where VanillaT : class
        where ParentT : class
    {
        protected FileObject(VanillaT vanilla, ParentT parent)
        {
            if (vanilla == null)
                throw new ArgumentNullException(nameof(vanilla));
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            Object = vanilla;
            Parent = parent;
        }

        public new ParentT Parent { get; internal set; }
    }

    public class FileObject<VanillaT> : IFileObject<VanillaT> where VanillaT : class
    {
        /// <summary>
        /// Gets or sets the object's full path, setting changes the object's parent
        /// </summary>
        public virtual string Path { get; set; }
        /// <summary>
        /// Gets or sets the object's name
        /// </summary>
        public virtual string Name { get; set; }
        /// <summary>
        /// Gets the vanilla object
        /// </summary>
        VanillaT IFileObject<VanillaT>.Object { get { return Object; } }
        public virtual VanillaT Object { get; internal set; }
        /// <summary>
        /// Gets the object's parent
        /// </summary>
        object IFileObject<VanillaT>.Parent { get { return Parent; } }
        public virtual object Parent { get; internal set; }
        /// <summary>
        /// Gets the index inside its parent's respective list
        /// </summary>
        int IFileObject<VanillaT>.Index { get { return Index; } }
        public virtual int Index { get; internal set; }
        /// <summary>
        /// Gets the root Filesystem the object is within
        /// </summary>
        public virtual Filesystem Root { get; }

        public bool LogOperation(FileOpLogType t, params string[] inputArr)
        {
            var input = new List<string>(inputArr);
            input.InsertRange(0, new string[] { Root.Parent.ip, Root.IPAccess, Name });
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
            if(Root.ShouldLogMultiplayer)
                Root.Parent.sendNetworkMessage(String.Format(mpLog, input.ToArray()));
            if (Root.IPAccess == null)
                return false;
            Root.Parent.log(String.Format(gameLog, input.ToArray()));
            return true;
        }
    }

    public interface IFileObject<VanillaT>
    {
        /// <summary>
        /// Gets or sets the object's full path, setting changes the object's parent
        /// </summary>
        string Path { get; set; }
        /// <summary>
        /// Gets or sets the object's name
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Gets the vanilla object
        /// </summary>
        VanillaT Object { get; }
        /// <summary>
        /// Gets the object's parent
        /// </summary>
        object Parent { get; }
        /// <summary>
        /// Gets the index inside its parent's respective list
        /// </summary>
        int Index { get; }
        /// <summary>
        /// Gets the root Filesystem the object is within
        /// </summary>
        Filesystem Root { get; }
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
}
