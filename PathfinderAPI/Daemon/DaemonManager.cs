using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hacknet;
using Pathfinder.Event;
using Pathfinder.Util.XML;

namespace Pathfinder.Daemon
{
    public static class DaemonManager
    {
        internal static readonly List<Type> CustomDaemons = new List<Type>();

        static DaemonManager()
        {
            EventManager.onPluginUnload += onPluginUnload;
        }
        
        internal static bool TryLoadCustomDaemon(ElementInfo info, Computer comp, OS os)
        {
            var daemonType = CustomDaemons.FirstOrDefault(x => x.Name == info.Name);
            if (daemonType != null)
            {
                BaseDaemon daemon = (BaseDaemon)Activator.CreateInstance(daemonType, new object[] { comp, info.Name, os });
                daemon.LoadFromXml(info);
                comp.daemons.Add(daemon);
                return true;
            }

            return false;
        }

        private static void onPluginUnload(Assembly pluginAsm)
        {
            CustomDaemons.RemoveAll(x => x.Assembly == pluginAsm);
        }

        public static void RegisterDaemon<T>() where T : BaseDaemon => RegisterDaemon(typeof(T));
        public static void RegisterDaemon(Type daemonType)
        {
            if (!typeof(BaseDaemon).IsAssignableFrom(daemonType))
                throw new ArgumentException("Daemon type must inherit from BaseDaemon!", nameof(daemonType));
            
            CustomDaemons.Add(daemonType);
        }

        public static void UnregisterDaemon<T>() where T : BaseDaemon => UnregisterDaemon(typeof(T));
        public static void UnregisterDaemon(Type daemonType)
        {
            CustomDaemons.Remove(daemonType);
        }
    }
}
