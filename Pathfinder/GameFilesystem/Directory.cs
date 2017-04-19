using System;
using System.Collections.Generic;
using Hacknet;
using Pathfinder.Util;

namespace Pathfinder.GameFilesystem
{
    public class Directory : IFilesystemObject
    {
        private Dictionary<string, Directory> directoryCache = new Dictionary<string, Directory>();
        private Dictionary<string, File> fileCache = new Dictionary<string, File>();
        private string path;
        private Filesystem root;
        private List<int> count = new List<int>();
        private int? pindex;

        public Folder Vanilla
        {
            get; private set;
        }

        public IFilesystemObject Parent
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

        public bool IsRoot
        {
            get; internal set;
        }

        public string Name
        {
            get
            {
                return Vanilla.getName();
            }
            set
            {
                Vanilla.name = value;
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

        public int ParentIndex
        {
            get
            {
                if (!pindex.HasValue)
                    pindex = (Parent.Vanilla as Folder)?.folders.BinarySearch(Vanilla);
                return pindex.GetValueOrDefault(-1);
            }
        }

        public Directory(IFilesystemObject parent, Folder folder)
        {
            Parent = parent;
            Vanilla = folder;
            IsRoot = false;
        }

        public Directory GetDirectoryByName(string name)
        {
            Directory dir;
            if (!directoryCache.TryGetValue(name, out dir))
            {
                var folder = Vanilla.searchForFolder(name);
                if (folder == null)
                    return null;
                dir = new Directory(this, folder);
                directoryCache.Add(name, dir);
            }
            return dir;
        }

        public File GetFileByName(string name)
        {
            File file;
            if (!fileCache.TryGetValue(name, out file))
            {
                var f = Vanilla.searchForFile(name);
                if (f == null)
                    return null;
                file = new File(this, f);
                fileCache.Add(name, file);
            }
            return file;
        }

        public bool ContainsFile(string name, string data = null)
        {
            if (data == null)
                return Vanilla.containsFile(name);
            return Vanilla.containsFile(name, data);
        }

        public bool ContainsFileWithData(string data)
        {
            return Vanilla.containsFileWithData(data);
        }

        public bool ContainsDirectory(string name)
        {
            return Vanilla.ContainsFolder(name);
        }

        public Directory CreateDirectory(string name)
        {
            var dir = GetDirectoryByName(name);
            if (dir == null)
            {
                Filesystem.LogOperation(this, Filesystem.FileOpLogType.CreateFolder,
                                        Root.Parent.ip, Root.IpAccessor, name, (Parent as Directory)?.Path);
                dir = new Directory(this, new Folder(name));
                directoryCache.Add(name, dir);
                Vanilla.folders.Add(dir.Vanilla);
            }
            return dir;
        }

        public Directory MakeDirectory(string path)
        {
            var current = this;
            var pathList = path.Split('/');
            var i = 0;
            for (var p = pathList[i]; i < pathList.Length; p = pathList[++i])
            {
                if (current.GetDirectoryByName(p) == null)
                    current = current.CreateDirectory(p);
            }
            return current;
        }

        public File CreateFile(string name, string data = null)
        {
            var f = GetFileByName(name);
            if (f == null)
            {
                Filesystem.LogOperation(this, Filesystem.FileOpLogType.CreateFile,
                                        Root.Parent.ip, Root.IpAccessor, name, data ?? "", (Parent as Directory)?.Path);
                f = new File(this, new FileEntry(name, data ?? ""));
                fileCache.Add(name, f);
                Vanilla.files.Add(f.Vanilla);
            }
            return f;
        }

        public File CreateExecutableFile(string name, Executable.IInterface modInterface)
        {
            var data = Executable.Handler.GetStandardFileDataBy(modInterface);
            if (data == null)
                return null;
            return CreateFile(name, data);
        }

        public File CreateExecutableFile(string name, string exeId)
        {
            var data = Executable.Handler.GetStandardFileDataBy(exeId);
            if (data == null)
            {
                data = ExeInfoManager.GetExecutableInfo(exeId).Data;
                if (data == null)
                    return null;
            }
            return CreateFile(name, data);
        }

        public File CreateExecutableFile(string name, int vanillaIndex)
        {
            string data;
            PortExploits.crackExeData.TryGetValue(vanillaIndex, out data);
            if (data == null)
                return null;
            return CreateFile(name, data);
        }

        public bool DeleteDirectory(string name)
        {
            var dir = GetDirectoryByName(name);
            if (dir == null)
                return true;
            directoryCache.Remove(name);
            Filesystem.LogOperation(this, Filesystem.FileOpLogType.DeleteFolder,
                                    Root.Parent.ip, Root.IpAccessor, name, (Parent as Directory)?.Path);
            return Vanilla.folders.Remove(dir.Vanilla);
        }

        public bool DeleteFile(string name)
        {
            var f = GetFileByName(name);
            if (f == null)
                return true;
            fileCache.Remove(name);
            Filesystem.LogOperation(this, Filesystem.FileOpLogType.DeleteFile,
                                    Root.Parent.ip, Root.IpAccessor, name, (Parent as Directory)?.Path);
            return Vanilla.files.Remove(f.Vanilla);
        }

        public File MoveFile(string name, string path)
        {
            var f = GetFileByName(name);
            if (f == null)
                return null;
            var dir = Root.GetDirectoryForPath(path.Remove(path.LastIndexOf('/')));
            f.MoveTo(dir, path.Substring(path.LastIndexOf('/') + 1));
            return f;
        }

        public Directory MoveFolder(string name, string path)
        {
            var d = GetDirectoryByName(name);
            if (d == null)
                return null;
            var dir = Root.GetDirectoryForPath(path.Remove(path.LastIndexOf('/')));
            d.MoveTo(dir, path.Substring(path.LastIndexOf('/') + 1));
            return d;
        }

        public bool MoveTo(Directory d, string changeName = null)
        {
            if (IsRoot) return false;
            if (changeName == null) changeName = Name;
            Filesystem.LogOperation(this, Filesystem.FileOpLogType.MoveFolder,
                                    Root.Parent.ip, Root.IpAccessor, Name, changeName, (Parent as Directory)?.Path, d.Path);
            Name = changeName;
            (Parent as Directory)?.Vanilla?.folders?.RemoveAt(ParentIndex);
            Parent = d;
            d.Vanilla.folders.Add(Vanilla);
            ResetIndex();
            return d.Vanilla.files[ParentIndex] == this.Parent;
        }

        internal void ResetIndex()
        {
            pindex = null;
        }

        public void ReloadDirectory(string name)
        {
            Directory d;
            if (directoryCache.TryGetValue(name, out d))
            {
                var f = Vanilla.searchForFolder(name);
                if (!d.Vanilla.Equals(f))
                    directoryCache[name] = new Directory(this, Vanilla.searchForFolder(name));
                else if (f == null)
                    directoryCache.Remove(name);
            }
        }

        public void ReloadFile(string name)
        {
            File file;
            if (fileCache.TryGetValue(name, out file))
            {
                var f = Vanilla.searchForFile(name);
                if (!file.Vanilla.Equals(f))
                    fileCache[name] = new File(this, Vanilla.searchForFile(name));
                else if (f == null)
                    fileCache.Remove(name);
            }
        }

        public IList<int> FolderCount
        {
            get
            {
                if (count.Count == 0)
                    Filesystem.RunToRoot(this, (f) => count.Add(f.ParentIndex));
                return count.AsReadOnly();
            }
        }

        public Directory this[string name]
        {
            get
            {
                return GetDirectoryByName(name);
            }
        }

        public Directory this[int index]
        {
            get
            {
                if (Vanilla.folders.Count <= index)
                    return null;
                if (!directoryCache.ContainsKey(Vanilla.folders[index].name))
                    directoryCache.Add(Vanilla.folders[index].name, new Directory(this, Vanilla.folders[index]));
                return directoryCache[Vanilla.folders[index].name];
            }
        }

        public static implicit operator Folder(Directory d)
        {
            return d.Vanilla;
        }
    }
}
