using System;
using System.Collections.Generic;
using System.Linq;
using Pathfinder.Util;

namespace Pathfinder.Attribute
{
    public class ModInfoAttribute : System.Attribute
    {
        public string PublicTitle { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public string Before { get; set; }
        public string After { get; set; }

        private List<string> beforeIds;
        private List<string> afterIds;

        public ModInfoAttribute(string publicTitle = null, string description = null, string version = null, string before = null, string after = null)
        {
            PublicTitle = publicTitle;
            Description = description;
            Version = version;
            Before = before;
            After = after;
        }

        public List<string> BeforeIds
        {
            get
            {
                if (beforeIds != null) return beforeIds;
                beforeIds = new List<string>(Before.Split(',') ?? Utility.Array<string>.Empty).Select(i => i.Trim()).Where(i => !string.IsNullOrEmpty(i)).ToList();
                return beforeIds;
            }
        }

        public List<string> AfterIds
        {
            get
            {
                if (afterIds != null) return afterIds;
                afterIds = new List<string>(After.Split(',') ?? Utility.Array<string>.Empty).Select(i => i.Trim()).Where(i => !string.IsNullOrEmpty(i)).ToList();
                return afterIds;
            }
        }
    }
}
