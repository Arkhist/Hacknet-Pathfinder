using Hacknet;

namespace Pathfinder.GameFilesystem
{
    public static class Extensions
    {
        public static bool ContainsFolder(this Folder folder, string name)
        {
            return folder.searchForFolder(name) != null;
        }

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

        public static Folder MakeFolder(this FileSystem fs, string path)
        {
            return fs.root.MakeFolder(path);
        }

        public static Filesystem GetFilesystem(this Hacknet.Computer computer)
        {
            return new Filesystem(computer);
        }
    }
}
