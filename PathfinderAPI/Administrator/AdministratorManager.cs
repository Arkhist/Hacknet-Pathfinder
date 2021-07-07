using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Hacknet;
using Pathfinder.Event;
using Pathfinder.Event.Loading.Content;
using Pathfinder.Event.Loading.Save;

namespace Pathfinder.Administrator
{
    public static class AdministratorManager
    {
        private static readonly List<Type> CustomAdministrators = new List<Type>();

        static AdministratorManager()
        {
            EventManager<ComputerComponentLoadEvent>.AddHandler(OnComponentLoad);
            EventManager<SaveComponentLoadEvent>.AddHandler(OnSavedComponentLoad);
            EventManager.onPluginUnload += onPluginUnload;
        }

        private static void OnComponentLoad(ComputerComponentLoadEvent args)
        {
            // TODO : Content loading
        }
        private static void OnSavedComponentLoad(SaveComponentLoadEvent args)
        {
            Hacknet.Administrator admin = null;
            if ((args.Type & ComponentType.Administrator) != 0)
            {
                string adminTypeName = null;
                if (!args.Info.Attributes.TryGetValue("type", out adminTypeName))
                    return;
                switch (adminTypeName)
                {
                    case "fast":
                        admin = new FastBasicAdministrator();
                        args.Comp.admin = admin;
                        args.Cancelled = true;
                        break;
                    case "basic":
                        admin = new BasicAdministrator();
                        args.Comp.admin = admin;
                        args.Cancelled = true;
                        break;
                    case "progress":
                        admin = new FastProgressOnlyAdministrator();
                        args.Comp.admin = admin;
                        args.Cancelled = true;
                        break;
                    default:
                        var adminType = CustomAdministrators.FirstOrDefault(x => x.Name == adminTypeName);
                        if (adminType != null) {
                            admin = (BaseAdministrator)Activator.CreateInstance(adminType, new object[] { args.Comp, args.Os });
                            args.Comp.admin = admin;
                            args.Cancelled = true;
                        }
                        break;
                }
            }
        }

        private static void onPluginUnload(Assembly pluginAsm)
        {
            var allTypes = pluginAsm.GetTypes();
            CustomAdministrators.RemoveAll(x => allTypes.Contains(x));
        }

        public static void RegisterAdministrator<T>() where T : BaseAdministrator => RegisterAdministrator(typeof(T));
        public static void RegisterAdministrator(Type adminType)
        {
            if (!typeof(BaseAdministrator).IsAssignableFrom(adminType))
                throw new ArgumentException("Administrator type must inherit from BaseAdministrator!", nameof(adminType));
            
            CustomAdministrators.Add(adminType);
        }

        public static void UnregisterAdministrator<T>() where T : BaseAdministrator => UnregisterAdministrator(typeof(T));
        public static void UnregisterAdministrator(Type adminType)
        {
            CustomAdministrators.Remove(adminType);
        }
    }
}
