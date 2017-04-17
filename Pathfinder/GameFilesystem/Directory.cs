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

        public Directory(IFilesystemObject parent, Folder folder)
        {
            Parent = parent;
            Vanilla = folder;
            IsRoot = false;
        }

        public Directory GetDirectoryByName(string name)
        {
            Directory dir;
            if (!directoryCache.TryGetValue(name, out dir)) {
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
            for (var p = pathList[i]; i<pathList.Length; p = pathList[++i])
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
            return Vanilla.folders.Remove(dir.Vanilla);
        }

        public bool DeleteFile(string name)
        {
            var f = GetFileByName(name);
            if (f == null)
                return true;
            fileCache.Remove(name);
            return Vanilla.files.Remove(f.Vanilla);
        }

        public void ReloadDirectory(string name)
        {
            Directory d;
            if (directoryCache.TryGetValue(name, out d)) {
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
            if (fileCache.TryGetValue(name, out file)) {
                var f = Vanilla.searchForFile(name);
                if (!file.Vanilla.Equals(f))
                    fileCache[name] = new File(this, Vanilla.searchForFile(name));
                else if (f == null)
                    fileCache.Remove(name);
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
