namespace Pathfinder.Meta;

public class PluginInfoAttribute : System.Attribute
{
    public string Description;
    public string ImageName;
    public string[] Authors;
    public string TeamName;

    public PluginInfoAttribute(string description, string teamName = null, string author = null)
    {
        Description = description;
        if(author != null) Authors = new []{ author };
        TeamName = teamName;
    }
}