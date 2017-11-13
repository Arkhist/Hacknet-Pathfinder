using System.Collections.Generic;
using Hacknet;

namespace Pathfinder.Util
{
    public static class ExeInfoManager
    {
        public struct ExecutableInfo
        {
            public static ExecutableInfo Empty = new ExecutableInfo();

            /// <summary>
            /// Gets the port number for the info.
            /// </summary>
            public int PortNumber { get; private set; }
            /// <summary>
            /// Gets the number within <see cref="PortExploits.exeNums"/>.
            /// </summary>
            public int Number { get; private set; }
            /// <summary>
            /// Gets the index within <see cref="PortExploits.exeNums"/>.
            /// </summary>
            public int NumberIndex { get; private set; }
            /// <summary>
            /// Gets the executable name.
            /// </summary>
            public string Name { get; private set; }
            /// <summary>
            /// Gets the name of the port service.
            /// </summary>
            public string ServiceName { get; private set; }
            /// <summary>
            /// Gets a value indicating whether this <see cref="T:Pathfinder.Util.ExeInfoManager.ExecutableInfo"/> needs port.
            /// </summary>
            /// <value><c>true</c> if needs port; otherwise, <c>false</c>.</value>
            public bool NeedsPort { get; private set; }
            /// <summary>
            /// Gets the standard game data for the executable.
            /// </summary>
            public string Data { get; private set; }
            /// <summary>
            /// Gets the local game data for the executable.
            /// </summary>
            public string LocalData { get; private set; }
            /// <summary>
            /// Gets a value indicating whether this <see cref="T:Pathfinder.Util.ExeInfoManager.ExecutableInfo"/> is empty.
            /// </summary>
            /// <value><c>true</c> if is empty; otherwise, <c>false</c>.</value>
            public bool IsEmpty => Equals(Empty);

            public ExecutableInfo(int pn, int n, int ni, string na, string sn, bool np, string d, string ld)
            {
                PortNumber = pn;
                Number = n;
                NumberIndex = ni;
                Name = na;
                ServiceName = sn;
                NeedsPort = np;
                Data = d;
                LocalData = ld;
            }

            public bool Equals(ExecutableInfo o)
            {
                return o.PortNumber == PortNumber
                        && o.Number == Number
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
                int hash = Number;
                hash = (hash * 16) + NumberIndex;
                hash = (hash * 16) + Name.GetHashCode();
                hash = (hash * 16) + ServiceName.GetHashCode();
                hash = (hash * 16) + (NeedsPort ? 1 : 2);
                hash = (hash * 16) + Data.GetHashCode();
                hash = (hash * 16) + Data.GetHashCode();
                if (PortNumber != -1)
                    hash = (hash * 16) + PortNumber;
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
