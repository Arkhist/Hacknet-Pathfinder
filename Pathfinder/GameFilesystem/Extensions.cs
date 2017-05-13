using Hacknet;

namespace Pathfinder.GameFilesystem
{
    public static class Extensions
    {
        public static bool ContainsFolder(this Folder folder, string name) =>
            folder.searchForFolder(name) != null;

        public static Folder MakeFolder(this Folder folder, string path)
        {
            Folder fholder = folder;
            var pathList = path.Split('/');
            var i = 0;
            for (string p = pathList[i]; i < pathList.Length; p = pathList[++i])
            {
                if (fholder.searchForFolder(p) == null)
                    fholder.folders.Add(fholder = new Folder(p));
            }
            return fholder;
        }

        public static Folder MakeFolder(this FileSystem fs, string path) =>
            fs.root.MakeFolder(path);

        public static Filesystem GetFilesystem(this Hacknet.Computer computer) => computer;

        public static Directory GetDirectoryAtDepth(this Hacknet.OS os, int depth)
        {
            Filesystem fs = os.connectedComp ?? os.thisComputer;
            var dir = fs.Directory;
            if (os.navigationPath.Count > 0)
                try
                {
                    for (int i = 0; i < depth; i++)
                        if (dir.DirectoryCount > os.navigationPath[i])
                            dir = dir.GetDirectory(os.navigationPath[i]);
                }
                catch {}
            return dir;
        }

        public static Directory GetCurrentDirectory(this Hacknet.OS os) =>
            GetDirectoryAtDepth(os, os.navigationPath.Count);
    }
}
