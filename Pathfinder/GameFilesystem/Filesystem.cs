using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hacknet;
using Pathfinder.Util;

namespace Pathfinder.GameFilesystem
{
    public sealed class Filesystem : FileObject<FileSystem, Hacknet.Computer>, IEnumerable<IFileObject>
    {
        private Directory rootDirectory;

        public Filesystem(Hacknet.Computer parent) : base(parent.files, parent) { }

        public override string Name
        {
            get
            {
                return "";
            }

            set
            {
                throw new InvalidOperationException("A " + nameof(Filesystem) + " Instance Name can't be assigned");
            }
        }

        public override string Path => Name;

        public override int Index
        {
            get
            {
                return -1;
            }

            internal set
            {
                throw new InvalidOperationException("A " + nameof(Filesystem) + " Instance Index can't be assigned");
            }
        }

        public override Filesystem Root => this;
        public override FileType Type => FileType.Filesystem;
        public Directory Directory => rootDirectory ?? (rootDirectory = new Directory(Object.root, this));

        public Directory SearchForDirectory(string path)
        {
            var res = Directory;
            foreach (var p in path.Split('/').Skip(1))
                if ((res = res.FindDirectory(p)) == null)
                    break;
            return res;
        }

        public File SearchForFile(string path)
        {
            var pList = path.Split('/').Skip(1);
            string p = null;
            File res = null;
            var dir = Directory;
            for (int i = 0; i < pList.Count(); p = pList.ElementAt(i++))
            {
                if (i == pList.Count() - 1)
                    res = dir.FindFile(p);
                if ((dir = dir.FindDirectory(p)) == null)
                    break;
            }
            return res;
        }

        public IEnumerator<IFileObject> GetEnumerator() => Directory.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public string IPAccess { get; set; } = null;
        public bool ShouldLogMultiplayer { get; set; } = true;

        public static Filesystem PrimaryFilesystem => Utility.ClientComputer;

        public static implicit operator Filesystem(Hacknet.Computer c) => new Filesystem(c);
    }
}
