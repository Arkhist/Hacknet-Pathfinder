using System.Collections.Generic;
using System.Linq;
using Hacknet;
using Pathfinder.Game.Folder;
using Pathfinder.Util;

namespace Pathfinder.Internal
{
    public static class DatabaseDaemonHandler
    {
        static Dictionary<DatabaseDaemon, List<DataInfo>> databaseDatasets =
            new Dictionary<DatabaseDaemon, List<DataInfo>>();

        public class DataInfo
        {
            public DataInfo(string name, string value = "")
            {
                Name = name;
                Value = value;
            }
            public DataInfo(Util.SaxProcessor.ElementInfo info)
            {
                var data = CopyData(info);
                Name = data.Name;
                Value = info.Elements?.Count < 1 ? data.Value : null;
                elements = data.elements;
            }

            public string Name { get; set; }
            public string Value { get; set; }
            internal DataInfo[] elements;

            public DataInfo this[int index] => elements[index];

            static DataInfo CopyData(Util.SaxProcessor.ElementInfo info)
            {
                var data = new DataInfo(info.Name, info.Elements.Count < 1 ? info.Value : null);
                data.elements = new DataInfo[info.Elements.Count];
                for (var i = 0; i < info.Elements.Count; i++)
                {
                    if (info.Elements[i].Elements.Count < 1)
                    {
                        data.elements[i].Name = info.Elements[i].Name;
                        data.elements[i].Value = info.Elements[i].Value;
                    }
                    else data.elements[i] = CopyData(info.Elements[i]);
                }
                return data;
            }

            public override string ToString()
            {
                if (Value != null) return Name.ToXml(Value);
                var result = "";
                foreach (var data in elements)
                    result += data.Name.ToXml(data.ToString());
                return result;
            }
        }

        public static List<DataInfo> GetDataset(this DatabaseDaemon daemon)
        {
            if (!databaseDatasets.ContainsKey(daemon))
                databaseDatasets.Add(daemon, new List<DataInfo>());
            return databaseDatasets[daemon];
        }

        public static void InitDataset(this DatabaseDaemon daemon)
        {
            if (!databaseDatasets.ContainsKey(daemon))
                databaseDatasets.Add(daemon, new List<DataInfo>());

            bool isVehicleInfo;
            if (databaseDatasets[daemon]?.Count >= 1)
                foreach (var contents in databaseDatasets[daemon])
                {
                    var contentStr = contents.ToString();
                    daemon.DatasetFolder.AddFile(daemon.GetDatabaseFilename(contentStr), contentStr);
                }
            else if (People.all.Count < 1) { }
            else if (isVehicleInfo = daemon.DataTypeIdentifier.EndsWith("VehicleInfo") || daemon.DataTypeIdentifier.EndsWith("Person"))
            {
                daemon.FilenameIsPersonName = true;
                foreach (var p in People.all)
                    if (p.vehicles.Count > 0)
                        daemon.DatasetFolder.AddFile(daemon.GetDatabasePersonalFilename(p),
                                                     isVehicleInfo
                                                     ? string.Join("\n", p.vehicles.Select((v) => v.ToXml()))
                                                     : p.ToXml());
            }
            else if (daemon.DataTypeIdentifier.EndsWith("NeopalsAccount"))
            {
                daemon.FilenameIsPersonName = false;
                daemon.HasSpecialCaseDraw = true;
                foreach (var p in People.all)
                {
                    if (p.NeopalsAccount != null)
                    {
                        var contentStr = p.ToXml();
                        daemon.DatasetFolder.AddFile(daemon.GetDatabaseFilename(contentStr), contentStr);
                    }
                }
            }
        }

        public static string GetDatabaseFilename(this DatabaseDaemon daemon, string input)
            => Utils.GetNonRepeatingFilename(input.Replace(" ", "_").ToLower(), ".rec", daemon.DatasetFolder);

        public static string GetDatabasePersonalFilename(this DatabaseDaemon daemon, Person input)
            => Utils.GetNonRepeatingFilename((input.firstName + '_' + input.lastName).ToLower(), ".rec", daemon.DatasetFolder);
    }
}
