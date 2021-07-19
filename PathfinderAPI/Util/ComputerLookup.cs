using System;
using System.Collections.Generic;
using Hacknet;

namespace Pathfinder.Util
{
    [Flags]
    public enum SearchType
    {
        Id   = 0b001,
        Ip   = 0b010,
        Name = 0b100,
        Any  = 0b111
    }
    
    public static class ComputerLookup
    {
        #region Lookup Tables
        private static readonly Dictionary<string, Computer> idLookup = new Dictionary<string, Computer>();

        private static readonly Dictionary<string, Computer> ipLookup = new Dictionary<string, Computer>();

        private static readonly Dictionary<string, Computer> nameLookup = new Dictionary<string, Computer>();
        #endregion
        
        internal static void PopulateLookups(Computer node)
        {
            idLookup[node.idName] = node;
            ipLookup[node.ip]     = node;
            nameLookup[node.name] = node;
        }

        public static Computer Find(string target, SearchType type = SearchType.Any)
        {
            Computer node = null;
            if (target == null) return null;
            if ((type & SearchType.Id) != 0)
                node = FindById(target);
            if (node == null && (type & SearchType.Ip) != 0)
                node = FindByIp(target);
            if (node == null && (type & SearchType.Name) != 0)
                node = FindByName(target);
            return node;
        }

        public static Computer FindById(string id) => idLookup.GetOrDefault(id);

        public static Computer FindByIp(string ip) => ipLookup.GetOrDefault(ip.Filter());

        public static Computer FindByName(string name) => nameLookup.GetOrDefault(name.Filter());
    }
}