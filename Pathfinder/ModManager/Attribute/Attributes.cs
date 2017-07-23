using System;
using System.Collections.Generic;

namespace Pathfinder.ModManager.Attribute
{
    public class TitleAttribute : System.Attribute
    {
        public string Title { get; }
        public TitleAttribute(string title) { Title = title; }
        public override string ToString() => Title;
    }

    public class DescriptionAttribute : System.Attribute
    {
        public string Description { get; }
        public DescriptionAttribute(string description) { Description = description; }
        public override string ToString() => Description;
    }

    public class VersionAttribute : System.Attribute
    {
        public string Version { get; }
        public VersionAttribute(string version) { Version = version; }
        public override string ToString() => Version;
    }

    public class AdditionalInfoAttribute : System.Attribute
    {
        public IDictionary<string, string> Info { get; }
        public string this[string key]
        {
            get
            {
                string result;
                Info.TryGetValue(key, out result);
                return result;
            }
        }
        public AdditionalInfoAttribute(IDictionary<string, string> info) { Info = info; }
    }
}
