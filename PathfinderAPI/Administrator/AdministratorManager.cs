using System.Reflection;
using Hacknet;
using Pathfinder.Event;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Administrator;

public static class AdministratorManager
{
    private static readonly Dictionary<string, Type> CustomAdministrators = new Dictionary<string, Type>();

    static AdministratorManager()
    {
        EventManager.onPluginUnload += onPluginUnload;
    }
        
    internal static void LoadAdministrator(ElementInfo info, Computer comp, OS os)
    {
        if (!info.Attributes.TryGetValue("type", out var adminTypeName))
            return;
            
        if (CustomAdministrators.TryGetValue(adminTypeName, out Type adminType))
        {
            BaseAdministrator admin = (BaseAdministrator)Activator.CreateInstance(adminType, [comp, os]);
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
        foreach (var name in CustomAdministrators.Where(x => x.Value.Assembly == pluginAsm).Select(x => x.Key).ToList())
            CustomAdministrators.Remove(name);
    }

    public static void RegisterAdministrator<T>() where T : BaseAdministrator => RegisterAdministrator(typeof(T));
    public static void RegisterAdministrator(Type adminType) => RegisterAdministrator(adminType, adminType.Name);
    public static void RegisterAdministrator<T>(string xmlName) where T : BaseAdministrator => RegisterAdministrator(typeof(T), xmlName);
    public static void RegisterAdministrator(Type adminType, string xmlName)
    {
        adminType.ThrowNotInherit<BaseAdministrator>(nameof(adminType));
        CustomAdministrators.Add(xmlName, adminType);
    }

    public static void UnregisterAdministrator<T>() where T : BaseAdministrator => UnregisterAdministrator(typeof(T));
    public static void UnregisterAdministrator(Type adminType)
    {
        var xmlName = CustomAdministrators.FirstOrDefault(x => x.Value == adminType).Key;
        if (xmlName != null)
            CustomAdministrators.Remove(xmlName);
    }
    public static void UnregisterAdministrator(string xmlName)
    {
        CustomAdministrators.Remove(xmlName);
    }
}
