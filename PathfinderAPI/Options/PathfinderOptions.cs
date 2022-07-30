namespace Pathfinder.Options;

internal static class PathfinderOptions
{
    internal static PluginCheckbox PreloadAllThemes = new PluginCheckbox("Preload All Themes",
        "Preload all themes at extension start\nimproves performance at the cost of memory");

    internal static PluginCheckbox DisableSteamCloudError = new PluginCheckbox("Disable Steam Cloud Message",
        "Disables the Steam Cloud disabled message on the main menu");

    [Util.Initialize]
    static void Initialize()
    {
        OptionsManager.GetOrRegisterTab(PathfinderAPIPlugin.Instance, "Pathfinder")
        .AddOption(PreloadAllThemes)
        .AddOption(DisableSteamCloudError);
    }
}
