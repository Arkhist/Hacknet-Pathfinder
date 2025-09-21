namespace Pathfinder.Meta;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class PluginWebsiteAttribute : System.Attribute
{
    public string WebsiteName;
    public string WebsiteUrl;

    public PluginWebsiteAttribute(string websiteName, string websiteUrl)
    {
        WebsiteName = websiteName;
        WebsiteUrl = websiteUrl;
    }
}