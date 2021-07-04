using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Pathfinder.Event;
using Pathfinder.Event.Loading.Content;
using Pathfinder.Event.Loading.Save;

namespace Pathfinder.Daemon
{
    public static class DaemonManager
    {
        private static readonly List<Type> CustomDaemons = new List<Type>();

        static DaemonManager()
        {
            EventManager<ComputerComponentLoadEvent>.AddHandler(OnComponentLoad);
            EventManager<SaveComponentLoadEvent>.AddHandler(OnSavedComponentLoad);
            EventManager.onPluginUnload += onPluginUnload;
        }

        private static void OnComponentLoad(ComputerComponentLoadEvent args)
        {
            var daemonType = CustomDaemons.FirstOrDefault(x => x.Name == args.Info.Name);
            if (daemonType != null)
            {
                BaseDaemon daemon = (BaseDaemon)Activator.CreateInstance(daemonType, new object[] { args.Comp, args.Info.Name, args.Os });
                daemon.LoadFromXml(args.Info);
                args.Comp.daemons.Add(daemon);
            }
        }
        private static void OnSavedComponentLoad(SaveComponentLoadEvent args)
        {
            if ((args.Type & ComponentType.Daemon) != 0)
            {
                var daemonType = CustomDaemons.FirstOrDefault(x => x.Name == args.Info.Name);
                if (daemonType != null)
                {
                    BaseDaemon daemon = (BaseDaemon)Activator.CreateInstance(daemonType, new object[] { args.Comp, args.Info.Name, args.Os });
                    daemon.LoadFromXml(args.Info);
                    args.Comp.daemons.Add(daemon);
                    args.Cancelled = true;
                }
            }
        }

        private static void onPluginUnload(Assembly pluginAsm)
        {
            var allTypes = pluginAsm.GetTypes();
            CustomDaemons.RemoveAll(x => allTypes.Contains(x));
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
