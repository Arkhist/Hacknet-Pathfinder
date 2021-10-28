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
        
        public static void RebuildLookups(List<Computer> nodes = null)
        {
            nodes ??= OS.currentInstance?.netMap?.nodes;
            nodes.ThrowNull(nameof(nodes), "No nodes passed in and OS hasn't finished loading netmap");
            ClearLookups();
            foreach (var node in nodes)
                Add(node);
        }

        public static void Add(Computer node)
        {
            if (!idLookup.ContainsKey(node.idName))
                idLookup[node.idName] = node;
            if (!ipLookup.ContainsKey(node.ip))
                ipLookup[node.ip] = node;
            if (!nameLookup.ContainsKey(node.name))
                nameLookup[node.name] = node;
        }

        internal static void ClearLookups()
        {
            idLookup.Clear();
            ipLookup.Clear();
            nameLookup.Clear();
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

        public static Computer FindByIp(string ip, bool filter = true) => ipLookup.GetOrDefault(filter ? ip.Filter() : ip);

        public static Computer FindByName(string name, bool filter = true) => nameLookup.GetOrDefault(filter ? name.Filter() : name);
    }
}
