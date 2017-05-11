#pragma warning disable CS0162 // Unreachable code detected
using System;
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
            throw new NotImplementedException();
            var name = goal.GetType().Name;
            var result = "<vanillaMissionGoal interfaceId=\"" + name.Remove(name.LastIndexOf("Mission")) + "\" ";

            //TODO: write out the crap manually to allow vanilla goals

            return result + "/>";
        }
    }
}
