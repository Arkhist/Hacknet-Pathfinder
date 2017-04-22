using System.Collections.Generic;
using System.Linq;

namespace Pathfinder.NetworkMap
{
    public static class Extensions
    {
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

        public static bool DiscoverNodeByName(this Hacknet.NetworkMap netmap, string name, float flashTime = 1f)
        {
            return netmap.DiscoverNode(netmap.GetComputerByName(name), flashTime);
        }

        public static bool DiscoverNodeByIp(this Hacknet.NetworkMap netmap, string ip, float flashTime = 1f)
        {
            return netmap.DiscoverNode(netmap.GetComputerByIp(ip), flashTime);
        }

        public static List<bool> DiscoverNodes(this Hacknet.NetworkMap netmap, IEnumerable<Hacknet.Computer> comps, float flashTime = 1f)
        {
            var result = new List<bool>();
            foreach (var c in comps)
                result.Add(netmap.DiscoverNode(c, flashTime));
            return result;
        }

        public static List<bool> DiscoverNodesByNames(this Hacknet.NetworkMap netmap, IEnumerable<string> names, float flashTime = 1f)
        {
            return netmap.DiscoverNodes(names.Select(s => netmap.GetComputerByName(s)), flashTime);
        }

        public static List<bool> DiscoverNodesByIps(this Hacknet.NetworkMap netmap, IEnumerable<string> ips, float flashTime = 1f)
        {
            return netmap.DiscoverNodes(ips.Select(s => netmap.GetComputerByIp(s)), flashTime);
        }

        public static Hacknet.Computer GetComputerByName(this Hacknet.NetworkMap netmap, string name)
        {
            return netmap.nodes.Find(c => c.idName == name);
        }

        public static Hacknet.Computer GetComputerByIp(this Hacknet.NetworkMap netmap, string ip)
        {
            return netmap.nodes.Find(n => n.ip == ip);
        }
    }
}
