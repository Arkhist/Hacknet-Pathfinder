using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using BepInEx;
using BepInEx.Hacknet;
using Hacknet;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Meta;

namespace Pathfinder.GUI;

public class PluginInfo
{
    private static List<string> _loadOrderCache;
    private static ReadOnlyCollection<string> _readonlyLoadOrderCache;
    protected static ReadOnlyCollection<string> LoadOrderCache => _readonlyLoadOrderCache ??= new ReadOnlyCollection<string>(_loadOrderCache);

    public readonly HacknetPlugin Plugin;
    public readonly string Name;
    public readonly string Guid;
    public readonly SemanticVersioning.Version Version;
    public readonly bool Enabled;
    public readonly int LoadOrder;
    public readonly string ConfigPath;
    private bool _configPathExists;
    public bool ConfigPathExists { get => _configPathExists; protected set => _configPathExists = value; }
    public readonly string Description;
    public readonly ReadOnlyCollection<string> SoftDependencies;
    public readonly ReadOnlyCollection<string> HardDependencies;
    public readonly ReadOnlyCollection<string> Incompatibles;
    public readonly ReadOnlyCollection<string> Authors;
    public readonly string TeamName;
    private ReadOnlyDictionary<string, string> _readonlyWebsites;
    public ReadOnlyDictionary<string, string> Websites => _readonlyWebsites ??= new ReadOnlyDictionary<string, string>(_websites);
    private readonly SortedDictionary<string, string> _websites = new SortedDictionary<string, string>();
    private readonly Texture2D ImageTexture;

    private readonly PFButton CheckForUpdateButton;
    private readonly PFButton UpdateButton;
    private readonly Func<PFButton, PFButton, Task> CheckForUpdateAction;
    private readonly Func<GameScreen, PFButton, Task> UpdateAction;
    private readonly Func<bool> CanPerformUpdate;
    private readonly Func<bool> CanCheckForUpdate;

    public PluginInfo(PluginListScreen screen, HacknetPlugin plugin)
    {
        Plugin = plugin;
        var metadata = MetadataHelper.GetMetadata(Plugin);
        if(metadata == null)
            throw new InvalidOperationException($"Plugin {Plugin.GetType().FullName} lacks a BepInPlugin attribute.");
        Name = metadata.Name;
        Guid = metadata.GUID;
        Version = metadata.Version;
        ConfigPath = Plugin.Config.ConfigFilePath;
        ConfigPathExists = File.Exists(ConfigPath);
        var pluginDataAttrib = MetadataHelper.GetAttributes<PluginInfoAttribute>(Plugin).FirstOrDefault();
        if(pluginDataAttrib != null)
        {
            Description = pluginDataAttrib.Description;
            Authors = new ReadOnlyCollection<string>(pluginDataAttrib.Authors);
            TeamName = pluginDataAttrib.TeamName;
            if(pluginDataAttrib.ImageName != null)
                ImageTexture = LoadTexture(screen.ScreenManager.GraphicsDevice, Path.Combine(Paths.PluginPath, pluginDataAttrib.ImageName));
        }
        if(ImageTexture == null)
        {
            ImageTexture = LoadTexture(screen.ScreenManager.GraphicsDevice, Path.Combine(Paths.PluginPath, Guid));
        }
        foreach(var website in MetadataHelper.GetAttributes<PluginWebsiteAttribute>(Plugin))
            _websites[website.WebsiteName] = website.WebsiteUrl;
        var softDeps = new List<string>();
        var hardDeps = new List<string>();
        foreach(var deps in MetadataHelper.GetDependencies(Plugin.GetType()))
        {
            if(deps.Flags == BepInDependency.DependencyFlags.SoftDependency)
                softDeps.Add($"{deps.DependencyGUID}{(deps.VersionRange?.ToString()?.Length > 0 ? $" ({deps.VersionRange})" : "")}");
            else
                hardDeps.Add($"{deps.DependencyGUID}{(deps.VersionRange?.ToString()?.Length > 0 ? $" ({deps.VersionRange})" : "")}");
        }
        SoftDependencies = new ReadOnlyCollection<string>(softDeps);
        HardDependencies = new ReadOnlyCollection<string>(hardDeps);
        var incompats = new List<string>();
        foreach(var incomp in MetadataHelper.GetAttributes<BepInIncompatibility>(Plugin))
            incompats.Add(incomp.IncompatibilityGUID);
        Incompatibles = new ReadOnlyCollection<string>(incompats);

        if(_loadOrderCache == null)
        {
            _loadOrderCache = new List<string>();
            foreach(var pluginPair in HacknetChainloader.Instance.Plugins)
                _loadOrderCache.Add(pluginPair.Key);
        }

        LoadOrder = _loadOrderCache.IndexOf(Guid);
        if(LoadOrder != -1)
        {
            LoadOrder++;
            Enabled = true;
        }

        Button = new PFButton(0, 0, 0, 0, $"{(Enabled ? $"{LoadOrder}. " : "")}{Name} ({Guid} v{Version})")
        {
        };

        ConfigPathButton = new PFButton(0,0,0,0,"");

        var updaterAttrb = MetadataHelper.GetAttributes<UpdaterAttribute>(Plugin).FirstOrDefault();
        if(updaterAttrb != null && HacknetChainloader.Instance.Plugins.TryGetValue("com.Pathfinder.Updater", out var updaterInfo))
        {
            var mainMenuOverrideType = updaterInfo.Instance.GetType().Assembly.GetType("PathfinderUpdater.MainMenuOverride");
            var methInfo = mainMenuOverrideType.GetMethod("PerformCheckAndUpdateButtonAsync", BindingFlags.NonPublic | BindingFlags.Static);
            if(methInfo.GetParameters().FirstOrDefault() != null)
            {
                CheckForUpdateAction = (Func<PFButton, PFButton, Task>)methInfo.CreateDelegate(typeof(Func<PFButton, PFButton, Task>));
                UpdateAction = (Func<GameScreen, PFButton, Task>)mainMenuOverrideType.GetMethod("PerformUpdateAndUpdateButtonAsync", BindingFlags.NonPublic | BindingFlags.Static).CreateDelegate(typeof(Func<GameScreen, PFButton, Task>));
                CanPerformUpdate = (Func<bool>)mainMenuOverrideType.GetProperty("CanPerformUpdate", BindingFlags.NonPublic | BindingFlags.Static).GetGetMethod(true).CreateDelegate(typeof(Func<bool>));
                CanCheckForUpdate = (Func<bool>)mainMenuOverrideType.GetProperty("CanCheckForUpdate", BindingFlags.NonPublic | BindingFlags.Static).GetGetMethod(true).CreateDelegate(typeof(Func<bool>));
                CheckForUpdateButton = new PFButton(0,0,0,0, "Check For Update", new Color(255, 255, 87));
                UpdateButton = new PFButton(0,0,0,0, "Update");
            }
        }
    }

    public virtual int Width { get; } = 300;
    public virtual int Height { get; } = 100;

    protected PFButton Button;
    protected PFButton ConfigPathButton;

    public virtual bool DrawListElement(GameTime time, PluginListScreen screen, Point offset, bool isSelected)
    {
        var result = isSelected;
        Button.X = offset.X;
        Button.Y = offset.Y;
        Button.Width = Width;
        Button.Height = Height;

        if(isSelected)
            Button.Color = GuiData.Default_Trans_Grey_Strong;
        else
            Button.Color = null;

        if(Button.Do())
            result = true;

        if(isSelected)
            DrawData(time, screen, offset);

        return result;
    }

    private int _panelId = -1;

    public virtual void DrawData(GameTime time, PluginListScreen screen, Point offset)
    {
        var viewWidth = screen.ScreenManager.GraphicsDevice.Viewport.Width;
        var viewHeight = screen.ScreenManager.GraphicsDevice.Viewport.Height;
        var xPos = offset.X + Width + 10;
        var yPos = 10;
        var rect = new Rectangle(xPos, yPos, viewWidth - xPos - 10, viewHeight - (yPos*2));
        ScrollablePanel.beginPanel(
            _panelId > -1
                ? _panelId
                : _panelId = PFButton.GetNextID(),
            rect,
            Vector2.Zero
        );

        RenderedRectangle.doRectangle(0, 0, rect.Width, rect.Height, new Color(0, 0, 0, 175));
        RenderedRectangle.doRectangleOutline(0, 0, rect.Width, rect.Height, 2, Color.Gray);

        var labelYPos = 10;
        if(ImageTexture != null)
        {
            DrawTexture(new Vector2(10, labelYPos), ImageTexture);
            labelYPos += ImageTexture.Height + 10;
        }
        const int yPosAdd = 40;
        TextItem.doLabel(new Vector2(10, labelYPos), $"Name: {Name}", null);
        labelYPos += yPosAdd;
        TextItem.doLabel(new Vector2(10, labelYPos), $"Guid: {Guid}", null);
        labelYPos += yPosAdd;
        TextItem.doLabel(new Vector2(10, labelYPos), $"Version: {Version}", null);
        labelYPos += yPosAdd;
        if(CheckForUpdateButton != null && UpdateButton != null)
        {
            CheckForUpdateButton.X = 10;
            CheckForUpdateButton.Y = labelYPos;
            CheckForUpdateButton.Width = 160;
            CheckForUpdateButton.Height = 30;
            UpdateButton.X = CheckForUpdateButton.X + CheckForUpdateButton.Width + 10;
            UpdateButton.Y = labelYPos;
            UpdateButton.Width = 160;
            UpdateButton.Height = 30;
            labelYPos += CheckForUpdateButton.Height + 10;

            if(CheckForUpdateButton.Do() && CanCheckForUpdate())
                Task.Run(async () => await CheckForUpdateAction(UpdateButton, CheckForUpdateButton));

            if(UpdateButton.Do() && CanPerformUpdate())
                Task.Run(async () => await UpdateAction(screen, UpdateButton));

        }
        TextItem.doLabel(new Vector2(10, labelYPos), $"Enabled: {Enabled}", null);
        labelYPos += yPosAdd;
        if(Enabled)
        {
            TextItem.doLabel(new Vector2(10, labelYPos), $"Load Order: {LoadOrder}", null);
            labelYPos += yPosAdd;
        }
        const string configPathText = "Config Path: ";
        TextItem.doLabel(new Vector2(10, labelYPos), configPathText, null);
        var configTextSize = GuiData.font.MeasureString(configPathText);
        ConfigPathButton.X = (int)configTextSize.X + 10;
        ConfigPathButton.Y = labelYPos;
        ConfigPathButton.Width = 900;
        ConfigPathButton.Height = (int)configTextSize.Y;
        ConfigPathButton.Text = ConfigPath;
        if(ConfigPathButton.Do())
        {
            if(!ConfigPathExists) File.Create(ConfigPath);
            new Thread(() => new Process
            {
                StartInfo = new ProcessStartInfo(ConfigPath)
                {
                    UseShellExecute = true
                }
            }.Start()).Start();
        }
        labelYPos += yPosAdd;
        TextItem.doLabel(new Vector2(10, labelYPos), $"Config Exists: {ConfigPathExists}", null);
        labelYPos += yPosAdd;
        if(Description != null)
        {
            TextItem.doLabel(new Vector2(10, labelYPos), $"Description: {Description}", null);
            labelYPos += yPosAdd;
        }
        if(Authors != null)
        {
            TextItem.doLabel(new Vector2(10, labelYPos), $"Author{(Authors.Count > 1 ? "s" : "")}: {string.Join(", ", Authors)}", null);
            labelYPos += yPosAdd;
        }
        if(TeamName != null)
        {
            TextItem.doLabel(new Vector2(10, labelYPos), $"Team Name: {TeamName}", null);
            labelYPos += yPosAdd;
        }
        if(HardDependencies.Count > 0)
        {
            TextItem.doLabel(new Vector2(10, labelYPos), $"Hard Dependenc{(HardDependencies.Count > 1 ? "ies" : "y")}: {string.Join(", ", HardDependencies)}", null);
            labelYPos += yPosAdd;
        }
        if(SoftDependencies.Count > 0)
        {
            TextItem.doLabel(new Vector2(10, labelYPos), $"Soft Dependenc{(SoftDependencies.Count > 1 ? "ies" : "y")}: {string.Join(", ", SoftDependencies)}", null);
            labelYPos += yPosAdd;
        }
        if(Incompatibles.Count > 0)
        {
            TextItem.doLabel(new Vector2(10, labelYPos), $"Incompatible{(Incompatibles.Count > 1 ? "s" : "")}: {string.Join(", ", Incompatibles)}", null);
            labelYPos += yPosAdd;
        }
        if(Websites.Count > 1)
        {
            TextItem.doLabel(new Vector2(10, labelYPos), $"Websites: ", null);
            labelYPos += yPosAdd;
        }
        foreach(var webPair in Websites)
        {
            TextItem.doLabel(new Vector2(40, labelYPos), $"{webPair.Key}: {webPair.Value}", null);
            labelYPos += yPosAdd;
        }


        ScrollablePanel.endPanel(_panelId, Vector2.Zero, rect, 0);
    }

    protected void DrawTexture(Vector2 position, Texture2D texture, Color? color = null)
    {
        GuiData.spriteBatch.Draw(texture, position, color ?? Color.White);
    }

    protected Texture2D LoadTexture(GraphicsDevice device, string fileName)
    {
        // TODO: validate if file is actually an image file perhaps? https://stackoverflow.com/a/49683945
        if(!File.Exists(fileName)) foreach(var ext in new[] { ".bmp", ".gif", ".jpg", ".jpeg", ".png", ".tga", ".tif", ".tiff" })
        {
            var imgPath = Path.Combine(Paths.PluginPath, Guid) + ext;
            if(File.Exists(imgPath))
            {
                fileName = imgPath;
                break;
            }
        }
        if(!File.Exists(fileName)) return null;
        try
        {
            using(FileStream fs = File.Open(fileName, FileMode.Open))
                return Texture2D.FromStream(device, fs);
        }
        catch(Exception ex)
        {
            throw ex;
        }
    }
}