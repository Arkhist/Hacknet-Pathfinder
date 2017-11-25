using System;
using System.Collections;
using System.Collections.Generic;
using Hacknet;
using Pathfinder.Util;

namespace Pathfinder.GameFilesystem
{
    public sealed class Filesystem : FileObject<FileSystem, Computer>, IEnumerable<IFileObject>
    {
        private Directory rootDirectory;

        public Filesystem(Computer parent) : base(parent.files, parent) { }

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

        /// <summary>
        /// Searchs for a Directory's path as far as possible.
        /// </summary>
        /// <returns>The Directory to search for, or the deepest Directory found in the path, or <c>null</c> if nullOut is true.</returns>
        /// <param name="path">The path to search by.</param>
        /// <param name="ignoreRootSymbol">If set to <c>true</c> ignores the root (/) symbol at the start.</param>
        /// <param name="nullOut">If set to <c>true</c> then nulls out on failure, otherwise returns furthest depth</param>
        public Directory SearchForDirectory(string path, bool ignoreRootSymbol = true, bool nullOut = false) =>
            Directory.SearchForDirectory(path, ignoreRootSymbol, nullOut);
        /// <summary>
        /// Searchs for a File's path as far as possible.
        /// </summary>
        /// <returns>The File to search for, or <c>null</c> if not found.</returns>
        /// <param name="path">The path to search by.</param>
        /// <param name="ignoreRootSymbol">If set to <c>true</c> ignores the root (/) symbol at the start.</param>
        public File SearchForFile(string path, bool ignoreRootSymbol = true) =>
            Directory.SearchForFile(path, ignoreRootSymbol);

        public IEnumerator<IFileObject> GetEnumerator() => Directory.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public string IPAccess { get; set; } = null;
        public bool ShouldLogMultiplayer { get; set; } = true;

        public static Filesystem PrimaryFilesystem => Utility.ClientComputer;
        public static Directory CurrentDirectory => GetCurrentDirectory();

        public static Directory GetCurrentDirectory(OS os = null) =>
            GetDirectoryAtDepth((os ?? Utility.ClientOS).navigationPath.Count, os);

        /// <summary>
        /// Gets the Directory at the specified depth.
        /// </summary>
        /// <returns>The Directory at the inputed depth.</returns>
        /// <param name="depth">The depth to retrieve.</param>
        /// <param name="os">The OS responsible for the data relating to the Directory's depth.</param>
        public static Directory GetDirectoryAtDepth(int depth, OS os = null)
        {
            os = os ?? Utility.ClientOS;
            var dir = Utility.GetCurrentComputer(os).GetFilesystem().Directory;
            if (os.navigationPath.Count > 0)
                for (var i = 0; i < depth && os.navigationPath.Count > i; i++)
                    if (dir.DirectoryCount > os.navigationPath[i]) dir = dir.GetDirectory(os.navigationPath[i]);
            return dir;
        }

        public static implicit operator Filesystem(Computer c) => new Filesystem(c);
    }
}
