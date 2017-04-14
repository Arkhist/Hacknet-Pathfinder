using System;
using Hacknet;

namespace Pathfinder.Filesystem
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

        public string Name
        {
            get
            {
                return Root.Name;
            }
        }

        public Filesystem(Hacknet.Computer computer)
        {
            Parent = computer;
            Vanilla = computer.files;
        }

        public Directory Root
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

        public static string GetPathFor(IFilesystemObject fso)
        {
            var path = fso.Name;
            while (!fso.IsRoot)
            {
                path = path.Insert(0, fso.Name + "/");
                fso = fso.Parent;
            }
            return path;
        }

        public static Filesystem GetPrimaryFilesystem()
        {
            return new Filesystem(Hacknet.OS.currentInstance.thisComputer);
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
