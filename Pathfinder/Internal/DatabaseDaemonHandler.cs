using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Hacknet;
using Pathfinder.Game.Folder;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Internal
{
    public static class DatabaseDaemonHandler
    {
        static Dictionary<DatabaseDaemon, List<DataInfo>> databaseDatasets =
            new Dictionary<DatabaseDaemon, List<DataInfo>>();

        public class DataInfo
        {
            public DataInfo(string name = null, string value = null)
            {
                Name = name;
                Value = value;
            }
            public DataInfo(SaxProcessor.ElementInfo info)
                => CopyData(info);

            public DataInfo(ElementInfo info)
                => CopyData(info);

            public string Name { get; set; }
            public string Value { get; set; }
            internal DataInfo[] elements;

            public DataInfo this[int index] => elements[index];

            DataInfo CopyData(SaxProcessor.ElementInfo info)
            {
                Name = info.Name;
                Value = info.Elements.Count < 1 ? info.Value : null;
                elements = new DataInfo[info.Elements.Count];
                for (var i = 0; i < info.Elements.Count; i++)
                {
                    if (info.Elements[i].Elements.Count < 1)
                        elements[i] = new DataInfo(info.Elements[i].Name, info.Elements[i].Value);
                    else elements[i] = new DataInfo(info.Elements[i]);
                }
                return this;
            }

            DataInfo CopyData(ElementInfo info)
            {
                Name = info.Name;
                Value = info.Children.Count < 1 ? info.Value : null;
                elements = new DataInfo[info.Children.Count];
                for (var i = 0; i < elements.Length; i++)
                {
                    if (info.Children[i].Children.Count < 1)
                        elements[i] = new DataInfo(info.Children[i].Name, info.Children[i].Value);
                    else elements[i] = new DataInfo(info.Children[i]);
                }
                return this;
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
            else if (People.all.Count < 1) ;
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
