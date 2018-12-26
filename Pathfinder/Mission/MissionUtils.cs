#pragma warning disable CS0162 // Unreachable code detected
using System;
using System.Reflection;
using Hacknet.Mission;

namespace Pathfinder.Mission
{
    public static class MissionUtils
    {
        public static string GenerateSaveStringFor(MisisonGoal goal)
        {
            var modGoal = goal as GoalInstance;
            if (modGoal != null)
                return modGoal.SaveString;
            var name = goal.GetType().Name;
            var result = "<vanillaMissionGoal interfaceId=\"" + name.Remove(name.LastIndexOf("Mission")) + "\" ";
            foreach (var f in goal.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (f.FieldType == typeof(Hacknet.OS) || f.FieldType == typeof(Hacknet.Folder)) continue;
                const string t = "target";
                var val = f.GetValue(goal);
                var computer = val as Hacknet.Computer;
                name = f.Name != t && f.Name.Contains(t) ? f.Name.Substring(f.Name.IndexOf(t) + t.Length) : f.Name;
                result += name + "=\"" + computer?.ip ?? val;
            }
            return result + "/>";
        }

        public static MisisonGoal LoadFromSaveString(string str)
        {
            /*if (str.Contains("vanillaMissionGoal"))
            {
                const string id = "interfaceId=\"";
                var name = str.Substring(str.IndexOf(id) + id.Length) + "Mission";
                var type = Type.GetType("Hacknet.Mission." + name + ",HacknetPathfinder");
                var ctor = type.GetConstructors(BindingFlags.Public)[0];
                string[] strs = new string[ctor.GetParameters().Length];
                int i = 0;
                foreach (var p in ctor.GetParameters())
                {
                    strs[i] =
                }
                Activator.CreateInstance(type, type.GetConstructors
            }*/
#pragma warning disable RECS0083 // Shows NotImplementedException throws in the quick task bar
            throw new NotImplementedException();
#pragma warning restore RECS0083 // Shows NotImplementedException throws in the quick task bar
        }
    }
}
