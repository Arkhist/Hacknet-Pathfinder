using System;

namespace Pathfinder.GameFilesystem
{
    public class FilePath
    {
        public const string SEPERATOR = "/";

        private string path;
        public string Path
        {
            get
            {
                return path;
            }

            set
            {
                if (value.Contains(SEPERATOR + SEPERATOR))
                    throw new ArgumentException("value of " + nameof(Path) + " cannot contain two or more of '" + SEPERATOR + "' in conjuction");
                path = value.Trim();
            }
        }

        public FilePath(string path)
        {
            Path = path;
        }

        public static implicit operator FilePath(string s) => new FilePath(s);
        public static implicit operator string(FilePath p) => p.Path;

        public FilePath Append(FilePath source) => Append(source.Path);
        public FilePath Append(string source)
        {
            if (Path.EndsWith(SEPERATOR) || String.IsNullOrEmpty(Path) || source.StartsWith(SEPERATOR))
            {
                if (Path.EndsWith(SEPERATOR) && source.StartsWith(SEPERATOR)) source = source.Remove(0, 1);
                return new FilePath(Path + source);
            }
            return new FilePath(Path + SEPERATOR + source);
        }

        public static FilePath operator /(FilePath path, FilePath source) => path.Append(source);
        public static FilePath operator /(FilePath path, string source) => path.Append(source);

        public FilePath Concat(FilePath source) => Concat(source.Path);
        public FilePath Concat(string source) => new FilePath(Path + source);

        public static FilePath operator +(FilePath path, FilePath source) => path.Concat(source);
        public static FilePath operator +(FilePath path, string source) => path.Concat(source);

        public FilePath Root => Path.StartsWith(SEPERATOR) ? Path.Substring(1, Path.IndexOf(SEPERATOR, 1)) : "";
        public FilePath Relative => Path.IndexOf(Root) != -1 ? Path.Remove(Path.IndexOf(Root), Root.Path.Length) : Path;
        public FilePath Parent
        {
            get
            {
                if (Path.LastIndexOf(SEPERATOR) == -1 || Path.LastIndexOf(SEPERATOR) == Path.Length - 1)
                    return "";
                if (Path.LastIndexOf(SEPERATOR) == 0)
                    return SEPERATOR;
                return Path.Remove(Path.LastIndexOf(SEPERATOR));
            }
        }
        public FilePath Filename => Path.IndexOf(SEPERATOR) != Path.LastIndexOf(SEPERATOR)
                                        && Path.LastIndexOf(SEPERATOR) == Path.Length - 1
                                        ? "." : Path.Substring(Path.LastIndexOf(SEPERATOR)+1);

        public FilePath Stem => Filename.Path.LastIndexOf('.') != -1
                                        && Filename != "." && Filename != ".."
                                        ? Filename.Path.Remove(Filename.Path.LastIndexOf('.')) : Filename.Path;

        public FilePath Extension => Filename.Path.LastIndexOf('.') != -1
                                        && Filename != "." && Filename != ".."
                                        ? Filename.Path.Substring(Filename.Path.LastIndexOf('.')) : "";
    }
}
