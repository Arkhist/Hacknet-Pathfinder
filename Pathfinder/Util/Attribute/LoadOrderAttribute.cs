using System.Collections.Generic;
using System.Linq;

namespace Pathfinder.Util.Attribute
{
    /// <summary>
    /// Load order attribute for designating mod load orders. (in case of mod reliance or relation)
    /// </summary>
    public class LoadOrderAttribute : System.Attribute
    {
        internal List<string> beforeIds;
        internal List<string> afterIds;

        public LoadOrderAttribute(string before = null, string after = null)
        {
            beforeIds = new List<string>(before.Split(',') ?? Utility.Array<string>.Empty);
            beforeIds = beforeIds.Select(i => i.Trim()).ToList();
            afterIds = new List<string>(after.Split(',') ?? Utility.Array<string>.Empty);
            afterIds = afterIds.Select(i => i.Trim()).ToList();
        }
    }
}
