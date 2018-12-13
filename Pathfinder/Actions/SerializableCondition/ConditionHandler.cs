using System;
using System.Collections.Generic;
using System.Xml;
using Pathfinder.Util;

namespace Pathfinder.Actions.SerializableCondition
{
    public static class ConditionHandler
    {

        public delegate Interface Deserializer(XmlReader rdr);

        internal static Dictionary<string, Deserializer> Deserializers =
            new Dictionary<string, Deserializer>();


        /// <summary>
        /// Adds a serializable condition to Hacknet
        /// </summary>
        /// <returns><c>true</c> if added to the game, <c>false</c> otherwise</returns>
        /// <param name="condition">The serializable condition used in the XML.</param>
        /// <param name="deserializer">The function run when deserializing the condition.</param>
        public static bool RegisterCondition(string condition,
                                             Deserializer deserializer)
        {
            if (Pathfinder.CurrentMod == null && !Extension.Handler.CanRegister)
                throw new InvalidOperationException("RegisterCondition can not be called outside of mod or extension loading.");
            var id = Pathfinder.CurrentMod?.GetCleanId() ?? Extension.Handler.ActiveInfo.Id;
            Logger.Verbose("{0} {1} is attempting to add condition {2}",
                           Pathfinder.CurrentMod != null ? "Mod" : "Extension", id, condition);
            if (Deserializers.ContainsKey(condition))
                return false;
            Deserializers.Add(condition, deserializer);

            return true;
        }

        public static Dictionary<string, Deserializer> GetDeserializers()
        {
            return Deserializers;
        }
    }
}
