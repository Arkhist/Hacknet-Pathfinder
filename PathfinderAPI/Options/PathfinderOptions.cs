namespace Pathfinder.Options;

internal static class PathfinderOptions
{
    private const string OPTION_TAG = "Pathfinder";

    internal static PluginCheckbox PreloadAllThemes = new PluginCheckbox("Preload All Themes",
        "Preload all themes at extension start\nimproves performance at the cost of memory");

    internal static PluginCheckbox DisableSteamCloudError = new PluginCheckbox("Disable Steam Cloud Message",
        "Disables the Steam Cloud disabled message on the main menu");

    [Util.Initialize]
    static void Initialize()
    {
        OptionsManager.RegisterTab(OPTION_TAG)
        .AddOption(PreloadAllThemes)
        .AddOption(DisableSteamCloudError);
    }
}
