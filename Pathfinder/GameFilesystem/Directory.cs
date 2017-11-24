using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hacknet;
using Pathfinder.Util;

namespace Pathfinder.GameFilesystem
{
    public class Directory : FileObject<Folder, IFileObject>, IEnumerable<IFileObject>
    {
        private string path;

        public Directory(Folder obj, IFileObject parent) : base(obj, parent)
        {
            var d = Parent as Directory;
            if (d == null)
                Index = -1;
            else
                Index = d.Object.folders.IndexOf(Object);
            path = Parent.Path + FilePath.SEPERATOR + Name;
        }

        /// <summary>
        /// Gets or sets the directory name.
        /// </summary>
        /// <value>The name of the directory.</value>
        public sealed override string Name
        {
            get
            {
                return Object.name;
            }

            set
            {
                LogOperation(FileOpLogType.MoveFolder, value, Path, Parent.Path + FilePath.SEPERATOR + Name);
                Object.name = value;
                path = Parent.Path + FilePath.SEPERATOR + Name;
            }
        }

        /// <summary>
        /// Gets or sets the directory's path, both renames and modifies the directory's location.
        /// </summary>
        /// <value>The directory's path.</value>
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
                LogOperation(FileOpLogType.MoveFolder, name, Path, Parent.Path + FilePath.SEPERATOR + Name);
                Object.name = name;
                path = Parent.Path + FilePath.SEPERATOR + Name;
            }
        }

        /// <summary>
        /// Gets the index inside its parent's respective list
        /// </summary>
        /// <value>The folder index in the parent or <c>-1</c> if root directory.</value>
        public sealed override int Index { get; internal set; }
        /// <summary>
        /// Gets the root Filesystem the Directory is within.
        /// </summary>
        public sealed override Filesystem Root => Parent.Root;
        /// <summary>
        /// Returns FileType.Directory
        /// </summary>
        public sealed override FileType Type => FileType.Directory;

        /// <summary>
        /// Casts the Parent FileObject to a Directory
        /// </summary>
        /// <value>The Parent as Directory</value>
        public Directory ParentDirectory => (Directory)Parent;

        /// <summary>
        /// Finds a File within the Directory based on its name.
        /// </summary>
        /// <returns>The File or <c>null</c> if not found.</returns>
        /// <param name="name">The name to find.</param>
        public File FindFile(string name)
        {
            var v = Object.searchForFile(name);
            if (v != null)
                return new File(v, this);
            return null;
        }

        /// <summary>
        /// Finds a Directory within the Directory based on its name.
        /// </summary>
        /// <returns>The Directory or <c>null</c> if not found.</returns>
        /// <param name="name">The name to find.</param>
        public Directory FindDirectory(string name)
        {
            var v = Object.searchForFolder(name);
            if (v != null)
                return new Directory(v, this);
            return null;
        }

        /// <summary>
        /// Searchs for a Directory's path as far as possible.
        /// </summary>
        /// <returns>The Directory to search for, or the deepest Directory found in the path, or <c>null</c> if nullOut is true.</returns>
        /// <param name="path">The path to search by.</param>
        /// <param name="ignoreRootSymbol">If set to <c>true</c> ignores the root (/) symbol at the start.</param>
        /// <param name="nullOut">If set to <c>true</c> then nulls out on failure, otherwise returns furthest depth</param>
        public Directory SearchForDirectory(string path, bool ignoreRootSymbol, bool nullOut = false)
        {
            if (!ignoreRootSymbol && path.StartsWith(FilePath.SEPERATOR))
                return Root.SearchForDirectory(path);
            var res = this;
            var last = res;
            var pList = ParsePath(path);
            foreach (var p in ParsePath(path))
            {
                if ((res = res.FindDirectory(p)) == null)
                {
                    if (!nullOut)
                        res = last;
                    break;
                }
                if(!nullOut) last = res;
            }
            return res;
        }
        /// <summary>
        /// Searchs for a Directory's path as far as possible.
        /// </summary>
        /// <returns>The Directory to search for, or the deepest Directory found in the path.</returns>
        /// <param name="path">The path to search by.</param>
        public Directory SearchForDirectory(string path) => SearchForDirectory(path, false);

        /// <summary>
        /// Searchs for a File's path as far as possible.
        /// </summary>
        /// <returns>The File to search for, or <c>null</c> if not found.</returns>
        /// <param name="path">The path to search by.</param>
        /// <param name="ignoreRootSymbol">If set to <c>true</c> ignores the root (/) symbol at the start.</param>
        public File SearchForFile(string path, bool ignoreRootSymbol)
        {
            if (!ignoreRootSymbol && path.StartsWith(FilePath.SEPERATOR))
                return Root.SearchForFile(path);
            var pList = ParsePath(path);
            string p = null;
            File res = null;
            var dir = this;
            for (int i = 0; i < pList.Count(); p = pList.ElementAt(i++))
            {
                if (i == pList.Count() - 1)
                    res = dir.FindFile(p);
                if ((dir = dir.FindDirectory(p)) == null)
                    break;
            }
            return res;
        }
        /// <summary>
        /// Searchs for a File's path as far as possible.
        /// </summary>
        /// <returns>The File to search for, or <c>null</c> if not found.</returns>
        /// <param name="path">The path to search by.</param>
        public File SearchForFile(string path) => SearchForFile(path, false);

        /// <summary>
        /// Gets a File based on its Index.
        /// </summary>
        /// <remarks>This method is the most bare and unsafe method of File retrieval</remarks>
        /// <returns>The File who contains said Index.</returns>
        /// <param name="index">The Index to find.</param>
        public File GetFile(int index) =>
            new File(Object.files[index], this);

        /// <summary>
        /// Gets a Directory based on its Index.
        /// </summary>
        /// <remarks>This method is the most bare and unsafe method of Directory retrieval</remarks>
        /// <returns>The Directory who contains said Index.</returns>
        /// <param name="index">The Index to find.</param>
        public Directory GetDirectory(int index) =>
            new Directory(Object.folders[index], this);

        /// <summary>
        /// Creates a new File based on the name and data.
        /// </summary>
        /// <returns>The File that was created.</returns>
        /// <param name="name">The name to assign to the File.</param>
        /// <param name="data">The data to assign to the File.</param>
        public File CreateFile(string name, string data = null)
        {
            if (data == null)
                data = "";
            var r = new File(new FileEntry(data, name), this);
            r.LogOperation(FileOpLogType.CreateFile, data, Path);
            Object.files.Add(r.Object);
            return r;
        }

        /// <summary>
        /// Creates a File based on the name and the Executable.IInterface generated file data.
        /// </summary>
        /// <returns>The File that was created.</returns>
        /// <param name="name">The name to assign to the File.</param>
        /// <param name="exeInterface">The Executable.IInterface whose file data is to be generated.</param>
        public File CreateFile(string name, Executable.IInterface exeInterface) =>
            CreateFile(name, Executable.Handler.GetStandardFileDataBy(exeInterface));

        /// <summary>
        /// Creates an executable File based on the name and the file data string id.
        /// </summary>
        /// <returns>The executable File that was created.</returns>
        /// <param name="name">The name to assign to the File.</param>
        /// <param name="exeId">The file data string id.</param>
        public File CreateExecutableFile(string name, string exeId) =>
            CreateFile(name, ExeInfoManager.GetExecutableInfo(exeId).Data ?? Executable.Handler.GetStandardFileDataBy(exeId, true));

        /// <summary>
        /// Creates a vanilla executable File based on the name and the vanilla data index.
        /// </summary>
        /// <returns>The executable File that was created.</returns>
        /// <param name="name">The name to assign to the File.</param>
        /// <param name="vanillaIndex">The vanilla data index.</param>
        public File CreateExecutableFile(string name, int vanillaIndex) =>
            CreateFile(name, ExeInfoManager.GetExecutableInfo(vanillaIndex).Data);

        /// <summary>
        /// Creates a new Directory based on the name.
        /// </summary>
        /// <returns>The Directory that was created.</returns>
        /// <param name="name">The name to assign to the Directory.</param>
        public Directory CreateDirectory(string name)
        {
            var r = new Directory(new Folder(name), this);
            r.LogOperation(FileOpLogType.CreateFolder, Path);
            Object.folders.Add(r.Object);
            return r;
        }

        /// <summary>
        /// Removes a File in the Directory by its name.
        /// </summary>
        /// <returns><c>true</c>, if File was found and removed, <c>false</c> otherwise.</returns>
        /// <param name="name">The name to find.</param>
        public bool RemoveFile(string name) => RemoveFile(FindFile(name));

        /// <summary>
        /// Removes a File in the Directory.
        /// </summary>
        /// <returns><c>true</c>, if File was found and removed, <c>false</c> otherwise.</returns>
        /// <param name="f">The File to find.</param>
        public bool RemoveFile(File f)
        {
            if (f == null)
                return false;
            f.LogOperation(FileOpLogType.DeleteFile, Path);
            return Object.files.Remove(f.Object);
        }

        /// <summary>
        /// Removes a Directory in the Directory by its name.
        /// </summary>
        /// <returns><c>true</c>, if Directory was found and removed, <c>false</c> otherwise.</returns>
        /// <param name="name">The name to find.</param>
        public bool RemoveDirectory(string name) => RemoveDirectory(FindDirectory(name));

        /// <summary>
        /// Removes a Directory in the Directory.
        /// </summary>
        /// <returns><c>true</c>, if Directory was found and removed, <c>false</c> otherwise.</returns>
        /// <param name="d">The Directory to find.</param>
        public bool RemoveDirectory(Directory d)
        {
            if (d == null)
                return false;
            d.LogOperation(FileOpLogType.DeleteFolder, Path);
            return Object.folders.Remove(d.Object);
        }

        /// <summary>
        /// Moves a File to a new Directory.
        /// </summary>
        /// <returns>The moved File.</returns>
        /// <param name="f">The File to move.</param>
        /// <param name="newDir">The new Directory.</param>
        public File MoveFile(File f, Directory newDir)
        {
            if (!Contains(f))
                return null;
            f.LogOperation(FileOpLogType.MoveFile, f.Name, Path, newDir.Path);
            Object.files.RemoveAt(f.Index);
            f.Parent = newDir;
            f.Index = newDir.Object.files.Count;
            newDir.Object.files.Add(f.Object);
            f.Name = f.Name;
            return f;
        }

        /// <summary>
        /// Moves a Directory to a new Directory.
        /// </summary>
        /// <returns>The moved Directory.</returns>
        /// <param name="d">The Directory to move.</param>
        /// <param name="newDir">The new Directory.</param>
        public Directory MoveDirectory(Directory d, Directory newDir)
        {
            if (!Contains(d))
                return null;
            d.LogOperation(FileOpLogType.MoveFolder, d.Name, Path, newDir.Path);
            Object.folders.RemoveAt(d.Index);
            d.Parent = newDir;
            d.Index = newDir.Object.folders.Count;
            newDir.Object.folders.Add(d.Object);
            d.Name = d.Name;
            return d;
        }

        /// <summary>
        /// Moves the Directory to a new Directory.
        /// </summary>
        /// <returns>The Directory to move to.</returns>
        /// <param name="to">The moved Directory.</param>
        public Directory MoveTo(Directory to) => ParentDirectory.MoveDirectory(this, to);
        /// <summary>
        /// Determines whether the specified File is contained within the Directory.
        /// </summary>
        /// <returns><c>true</c>, if the File isn't null and contained within the Directory, <c>false</c> otherwise.</returns>
        /// <param name="f">The File to determine is contained within.</param>
        public bool Contains(File f) => Object.files.Contains(f?.Object);
        /// <summary>
        /// Determines whether the specified Directory is contained within the Directory.
        /// </summary>
        /// <returns><c>true</c>, if the Directory isn't null and contained within the Directory, <c>false</c> otherwise.</returns>
        /// <param name="d">The Directory to determine is contained within.</param>
        public bool Contains(Directory d) => Object.folders.Contains(d?.Object);
        /// <summary>
        /// Determines whether a File with the specified name and/or data is contained within the Directory
        /// </summary>
        /// <returns><c>true</c>, if File is contained in the Directory, <c>false</c> otherwise.</returns>
        /// <param name="name">The name to search for or <c>null</c>.</param>
        /// <param name="data">The data to search for or <c>null</c>.</param>
        public bool ContainsFile(string name = null, string data = null)
        {
            if (name == null && data == null)
                return Object.files.Count > 1;
            if (name == null)
                return Object.containsFileWithData(data);
            if (data == null)
                return Object.containsFile(name);
            return Object.files.Exists(f => f.name == name && f.data == data);
        }
        /// <summary>
        // Determines whether a directory with the specified name, folderCount, and/or fileCount is contained within the Directory
        /// </summary>
        /// <returns><c>true</c>, if Directory is contained in the Directory, <c>false</c> otherwise.</returns>
        /// <param name="name">The name to search for or <c>null</c>.</param>
        /// <param name="folderCount">The amount of folders to look for or <c>null</c>.</param>
        /// <param name="fileCount">The amount of files to look for or <c>null</c>.</param>
        public bool ContainsDirectory(string name, int? folderCount, int? fileCount = null)
        {
            if (name == null)
                return Object.folders.Count < 1;
            return Object.folders.Exists(f =>
                                         (name == null || f.name == name) &&
                                         (!folderCount.HasValue || f.folders.Count == folderCount) &&
                                         (!fileCount.HasValue || f.files.Count == fileCount));
        }
        public bool ContainsDirectory(string name) => ContainsDirectory(name, null);

        /// <summary>
        /// Gets the first <see cref="T:Pathfinder.GameFilesystem.IFileObject"/> that contains the specified name,
        /// searching through files first.
        /// </summary>
        /// <param name="name">Name.</param>
        public IFileObject this[string name] => (IFileObject)FindFile(name) ?? FindDirectory(name);
        /// <summary>
        /// Retrieves a list of Files contained within the Directory.
        /// </summary>
        public List<File> Files => Object.files.Select(f => new File(f, this)).ToList();
        /// <summary>
        /// Retrieves a list of Directories contained within the Directory.
        /// </summary>
        /// <value>The directories.</value>
        public List<Directory> Directories => Object.folders.Select(f => new Directory(f, this)).ToList();
        /// <summary>
        /// Retrieves the amount of Files within the Directory.
        /// </summary>
        public int FileCount => Object.files.Count;
        /// <summary>
        /// Retrieves the amount of Directories within the Directory.
        /// </summary>
        public int DirectoryCount => Object.folders.Count;

        public IEnumerator<IFileObject> GetEnumerator()
        {
            foreach (var f in Object.files)
                yield return new File(f, this);

            foreach (var f in Object.folders)
                yield return new Directory(f, this);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
