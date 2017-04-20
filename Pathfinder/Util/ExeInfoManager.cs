using System.Collections.Generic;
using Hacknet;

namespace Pathfinder.Util
{
    public static class ExeInfoManager
    {
        public struct ExecutableInfo
        {
            public static ExecutableInfo Empty = new ExecutableInfo();

            public int PortNumber { get; private set; }
            public int Index { get; private set; }
            public int NumberIndex { get; private set; }
            public string Name { get; private set; }
            public string ServiceName { get; private set; }
            public bool NeedsPort { get; private set; }
            public string Data { get; private set; }
            public string LocalData { get; private set; }
            public bool IsEmpty
            {
                get
                {
                    return this.Equals(Empty);
                }
            }

            public ExecutableInfo(int pn, int i, int ni, string n, string sn, bool np, string d, string ld)
            {
                PortNumber = pn;
                Index = i;
                NumberIndex = ni;
                Name = n;
                ServiceName = sn;
                NeedsPort = np;
                Data = d;
                LocalData = ld;
            }

            public bool Equals(ExecutableInfo o)
            {
                return o.PortNumber == PortNumber
                        && o.Index == Index
                        && o.NumberIndex == NumberIndex
                        && o.Name == Name
                        && o.ServiceName == ServiceName
                        && o.NeedsPort == NeedsPort
                        && o.Data == Data
                        && o.LocalData == LocalData;
            }

            public override bool Equals(object obj)
            {
                if (obj is ExecutableInfo)
                    Equals((ExecutableInfo)obj);
                return false;
            }

            public override int GetHashCode()
            {
                int hash = Index;
                hash = (hash * 15) + NumberIndex;
                hash = (hash * 15) + Name.GetHashCode();
                hash = (hash * 15) + ServiceName.GetHashCode();
                hash = (hash * 15) + (NeedsPort ? 1 : 2);
                hash = (hash * 15) + Data.GetHashCode();
                hash = (hash * 15) + Data.GetHashCode();
                if (PortNumber != -1)
                    hash = (hash * 15) + PortNumber;
                return hash;
            }
        }

        private static Dictionary<string, ExecutableInfo> nameToExeStruct = new Dictionary<string, ExecutableInfo>();

        internal static void LoadExecutableStruct(Event.GameLoadContentEvent e)
        {
            int i = 0;
            string sn;
            foreach (var n in PortExploits.exeNums)
            {
                PortExploits.services.TryGetValue(n, out sn);
                nameToExeStruct.Add(PortExploits.cracks[n],
                                    new ExecutableInfo(PortExploits.portNums.Contains(n) ? n : -1, n, i++,
                                                       PortExploits.cracks[n],
                                                       sn,
                                                       PortExploits.needsPort[n], PortExploits.crackExeData[n],
                                                       PortExploits.crackExeDataLocalRNG[n]));
                sn = null;
            }
        }

        public static ExecutableInfo GetExecutableInfo(string name)
        {
            ExecutableInfo i;
            if (nameToExeStruct.TryGetValue(name, out i))
                return i;
            return i;
        }

        public static ExecutableInfo GetExecutableInfo(int index)
        {
            ExecutableInfo i = default(ExecutableInfo);
            string n;
            if (PortExploits.cracks.TryGetValue(index, out n) && nameToExeStruct.TryGetValue(n, out i))
                return i;
            return i;
        }

        public static List<ExecutableInfo> GetInfoList()
        {
            var result = new List<ExecutableInfo>();
            foreach (var pair in nameToExeStruct)
                result.Add(pair.Value);
            return result;
        }
    }
}
