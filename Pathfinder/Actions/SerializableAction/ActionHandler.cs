using Pathfinder.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Pathfinder.Actions.SerializableAction
{
    public static class ActionHandler
    {
        public delegate Interface Deserializer(XmlReader rdr);

        internal static Dictionary<string, Deserializer> Deserializers =
            new Dictionary<string, Deserializer>();


        /// <summary>
        /// Adds a serializable action to Hacknet
        /// </summary>
        /// <returns><c>true</c> if added to the game, <c>false</c> otherwise</returns>
        /// <param name="condition">The serializable action used in the XML.</param>
        /// <param name="deserializer">The function run when deserializing the action.</param>
        public static bool RegisterAction(string action,
                                             Deserializer deserializer)
        {
            if (Pathfinder.CurrentMod == null && !Extension.Handler.CanRegister)
                throw new InvalidOperationException("RegisterCondition can not be called outside of mod or extension loading.");
            var id = Pathfinder.CurrentMod?.GetCleanId() ?? Extension.Handler.ActiveInfo.Id;
            Logger.Verbose("{0} {1} is attempting to add condition {2}",
                           Pathfinder.CurrentMod != null ? "Mod" : "Extension", id, action);
            if (Deserializers.ContainsKey(action))
                return false;
            Deserializers.Add(action, deserializer);

            return true;
        }

        public static Dictionary<string, Deserializer> GetDeserializers()
        {
            return Deserializers;
        }
    }
}
