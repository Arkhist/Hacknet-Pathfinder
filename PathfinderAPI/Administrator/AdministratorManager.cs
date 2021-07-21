using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hacknet;
using Pathfinder.Event;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Administrator
{
    public static class AdministratorManager
    {
        private static readonly List<Type> CustomAdministrators = new List<Type>();

        static AdministratorManager()
        {
            EventManager.onPluginUnload += onPluginUnload;
        }
        
        internal static void LoadAdministrator(ElementInfo info, Computer comp, OS os)
        {
            if (!info.Attributes.TryGetValue("type", out var adminTypeName))
                return;
            
            var adminType = CustomAdministrators.FirstOrDefault(x => x.Name == adminTypeName);
            if (adminType != null) {
                BaseAdministrator admin = (BaseAdministrator)Activator.CreateInstance(adminType, new object[] { comp, os });
                admin.LoadFromXml(info);
                comp.admin = admin;
            }
            else
            {
                switch (adminTypeName)
                {
                    case "fast":
                        comp.admin = new FastBasicAdministrator();
                        break;
                    case "basic":
                        comp.admin = new BasicAdministrator();
                        break;
                    case "progress":
                        comp.admin = new FastProgressOnlyAdministrator();
                        break;
                    case "none":
                        comp.admin = null;
                        break;
                }

                if (comp.admin != null)
                {
                    comp.admin.ResetsPassword = info.Attributes.GetBool("resetPass", info.Attributes.GetBool("resetPassword", true));
                    comp.admin.IsSuper = info.Attributes.GetBool("isSuper");
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
