using System.Collections.Generic;
using System.Linq;

namespace Pathfinder.Game.NetworkMap
{
    public static class Extensions
    {
        /// <summary>
        /// Discovers a Computer node if it exists.
        /// </summary>
        /// <returns><c>true</c>, if node exists and was discovered, <c>false</c> otherwise.</returns>
        /// <param name="netmap">The NetworkMap</param>
        /// <param name="comp">The Computer node to discover</param>
        /// <param name="flashTime">The flash time length for the discovery</param>
        public static bool DiscoverNode(this Hacknet.NetworkMap netmap, Hacknet.Computer comp, float flashTime = 1f)
        {
            var i = netmap.nodes.IndexOf(comp);
            if (i != -1)
            {
                if(!netmap.visibleNodes.Contains(i))
                    netmap.visibleNodes.Add(i);
                if (flashTime > 0)
                    comp.highlightFlashTime = flashTime;
                netmap.lastAddedNode = comp;
            }
            return i != -1 && netmap.visibleNodes.Contains(i);
        }

        /// <summary>
        /// Discovers a Computer node by name if it exists.
        /// </summary>
        /// <returns><c>true</c>, if node exists and was discovered, <c>false</c> otherwise.</returns>
        /// <param name="netmap">The NetworkMap</param>
        /// <param name="name">The Computer name to discover</param>
        /// <param name="flashTime">The flash time length for the discovery</param>
        public static bool DiscoverNodeByName(this Hacknet.NetworkMap netmap, string name, float flashTime = 1f) =>
            netmap.DiscoverNode(netmap.GetComputerByName(name), flashTime);

        /// <summary>
        /// Discovers a Computer node by ip if it exists.
        /// </summary>
        /// <returns><c>true</c>, if node exists and was discovered, <c>false</c> otherwise.</returns>
        /// <param name="netmap">The NetworkMap</param>
        /// <param name="ip">The Computer ip to discover</param>
        /// <param name="flashTime">The flash time length for the discovery</param>
        public static bool DiscoverNodeByIp(this Hacknet.NetworkMap netmap, string ip, float flashTime = 1f) =>
            netmap.DiscoverNode(netmap.GetComputerByIp(ip), flashTime);

        /// <summary>
        /// Discovers nodes by a Computer enumerable.
        /// </summary>
        /// <returns>A List of booleans for each discovered node</returns>
        /// <param name="netmap">The NetworkMap</param>
        /// <param name="comps">The Computer nodes to discover</param>
        /// <param name="flashTime">The flash time length for the discovery</param>
        /// <seealso cref="DiscoverNode"/>
        public static List<bool> DiscoverNodes(this Hacknet.NetworkMap netmap, IEnumerable<Hacknet.Computer> comps, float flashTime = 1f)
        {
            var result = new List<bool>();
            foreach (var c in comps)
                result.Add(netmap.DiscoverNode(c, flashTime));
            return result;
        }

        /// <summary>
        /// Discovers Computer nodes by a name enumerable.
        /// </summary>
        /// <returns>A List of booleans for each discovered node</returns>
        /// <param name="netmap">The NetworkMap</param>
        /// <param name="names">The Computer names to discover</param>
        /// <param name="flashTime">The flash time length for the discovery</param>
        /// <seealso cref="DiscoverNodeByName"/>
        public static List<bool> DiscoverNodesByNames(this Hacknet.NetworkMap netmap,
                                                      IEnumerable<string> names, 
                                                      float flashTime = 1f) =>
            netmap.DiscoverNodes(names.Select(s => netmap.GetComputerByName(s)), flashTime);

        /// <summary>
        /// Discovers Computer nodes by an ip enumerable.
        /// </summary>
        /// <returns>A List of booleans for each discovered node</returns>
        /// <param name="netmap">The NetworkMap</param>
        /// <param name="ips">The Computer ips to discover</param>
        /// <param name="flashTime">The flash time length for the discovery</param>
        /// <seealso cref="DiscoverNodeByIp"/>
        public static List<bool> DiscoverNodesByIps(this Hacknet.NetworkMap netmap,
                                                    IEnumerable<string> ips, 
                                                    float flashTime = 1f) =>
            netmap.DiscoverNodes(ips.Select(s => netmap.GetComputerByIp(s)), flashTime);

        /// <summary>
        /// Retrieves a Computer by index.
        /// </summary>
        /// <returns>The Computer or <c>null</c> if not found.</returns>
        /// <param name="netmap">The NetworkMap</param>
        /// <param name="index">The index to get.</param>
        public static Hacknet.Computer GetComputer(this Hacknet.NetworkMap netmap, int index) =>
            netmap.nodes.ElementAtOrDefault(index);

        /// <summary>
        /// Retrieves a Computer by name.
        /// </summary>
        /// <returns>The Computer or <c>null</c> if not found.</returns>
        /// <param name="netmap">The NetworkMap</param>
        /// <param name="name">The name to get.</param>
        public static Hacknet.Computer GetComputerByName(this Hacknet.NetworkMap netmap, string name) =>
            netmap.nodes.Find(c => c.idName == name);

        /// <summary>
        /// Retrieves a Computer by ip.
        /// </summary>
        /// <returns>The Computer or <c>null</c> if not found.</returns>
        /// <param name="netmap">The NetworkMap</param>
        /// <param name="ip">The ip to get.</param>
        public static Hacknet.Computer GetComputerByIp(this Hacknet.NetworkMap netmap, string ip) =>
            netmap.nodes.Find(n => n.ip == ip);

        /// <summary>
        /// Retrieves a Computer by identifier.
        /// </summary>
        /// <returns>The Computer or <c>null</c> if not found.</returns>
        /// <param name="netmap">The NetworkMap.</param>
        /// <param name="id">The id to get.</param>
        public static Hacknet.Computer GetComputerById(this Hacknet.NetworkMap netmap, string id) =>
            netmap.nodes.Find(n => n.idName == id);

        /// <summary>
        /// Adds a link to linkStart connecting to linkEnd.
        /// </summary>
        /// <returns><c>true</c>, if the link was added, <c>false</c> otherwise.</returns>
        /// <param name="netmap">The NetworkMap</param>
        /// <param name="linkStart">The link start.</param>
        /// <param name="linkEnd">The link end.</param>
        public static bool AddLink(this Hacknet.NetworkMap netmap, Hacknet.Computer linkStart, Hacknet.Computer linkEnd)
        {
            var i = netmap.nodes.IndexOf(linkEnd);
            if (i < 0)
                return false;
            linkStart.links.Add(i);
            return linkStart.links.Contains(i);
        }
    }
}
