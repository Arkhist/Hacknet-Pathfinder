using System;
using System.Linq;
using Hacknet;

namespace Pathfinder.GameFilesystem
{
    public class Filesystem : FileObject<FileSystem, Hacknet.Computer>
    {
        public Filesystem(Hacknet.Computer parent) : base(parent.files, parent) { }

        public override string Name
        {
            get
            {
                return "";
            }

            set
            {
                throw new InvalidOperationException("A Pathfinder.GameFilesystem.Filesystem Instance Name can't be assigned");
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
                throw new InvalidOperationException("A Pathfinder.GameFilesystem.Filesystem Instance Index can't be assigned");
            }
        }

        public override Filesystem Root => this;

        public Directory Directory
        {
            get
            {
                return new Directory(Object.root, this as IFileObject<object>);
            }
        }

        public Directory SeacrhForDirectory(string path)
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

        public string IPAccess { get; set; } = null;
        public bool ShouldLogMultiplayer { get; set; } = true;

        public static Filesystem PrimaryFilesystem => Hacknet.OS.currentInstance.thisComputer;

        public static implicit operator Filesystem(Hacknet.Computer c)
        {
            return new Filesystem(c);
        }
    }
}
