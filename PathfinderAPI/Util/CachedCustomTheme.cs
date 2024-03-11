using Hacknet;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using Pathfinder.Util.XML;

namespace Pathfinder.Util;

[HarmonyPatch]
public class CachedCustomTheme : IDisposable
{
    public delegate ref Color RefColorFieldDelegate(OS instance);
        
    private static readonly Dictionary<string, RefColorFieldDelegate> OSColorFieldsFast = new Dictionary<string, RefColorFieldDelegate>();
        
    [Initialize]
    internal static void Initialize()
    {
        foreach (var field in AccessTools.GetDeclaredFields(typeof(OS)).Where(x => x.FieldType == typeof(Color)))
        {
            var dynMethod = new DynamicMethodDefinition(field.Name, typeof(Color).MakeByRefType(), [typeof(OS)]);
            var p = dynMethod.GetILProcessor();
            p.Emit(OpCodes.Ldarg_0);
            p.Emit(OpCodes.Ldflda, field);
            p.Emit(OpCodes.Ret);
            OSColorFieldsFast.Add(field.Name, dynMethod.Generate().CreateDelegate<RefColorFieldDelegate>());
        }
    }
        
    public ElementInfo ThemeInfo { get; }
    public Texture2D BackgroundImage { get; internal set; }
    private byte[] TextureData = null;
    public string Path { get; }
    public bool Loaded { get; private set; } = false;

    public CachedCustomTheme(string themeFileName)
    {
        Path = themeFileName;
        ElementInfo themeInfo = null;

        var executor = new EventExecutor(themeFileName.ContentFilePath(), true);
        executor.RegisterExecutor("CustomTheme", (exec, info) => themeInfo = info, ParseOption.ParseInterior);
        executor.Parse();

        ThemeInfo = themeInfo ?? throw new FormatException($"No CustomTheme element in {themeFileName}");
    }

    public void Load(bool isMainThread)
    {
        if (ThemeInfo.Children.All(x => x.Name != "backgroundImagePath"))
        {
            Loaded = true;
            return;
        }
            
        if (isMainThread && TextureData == null)
        {
            if (ThemeInfo.Children.TryGetElement("backgroundImagePath", out var imagePath))
            {
                if (imagePath.Content.HasContent())
                {
                    if (imagePath.Content.ContentFileExists())
                    {
                        using (FileStream imageSteam = File.OpenRead(imagePath.Content.ContentFilePath()))
                        {
                            BackgroundImage = Texture2D.FromStream(GuiData.spriteBatch.GraphicsDevice, imageSteam);
                        }
                    }
                    else
                    {
                        Logger.Log(BepInEx.Logging.LogLevel.Warning, "Could not find theme background image " + imagePath.Content + " for theme " + Path);
                    }

                    Loaded = true;
                }
            }
        }
        else if (isMainThread)
        {
            using (var ms = new MemoryStream(TextureData))
            {
                BackgroundImage = Texture2D.FromStream(GuiData.spriteBatch.GraphicsDevice, ms);
            }

            TextureData = null;
            Loaded = true;
        }
        else
        {
            if (ThemeInfo.Children.TryGetElement("backgroundImagePath", out var imagePath))
            {
                if (imagePath.Content.HasContent() && imagePath.Content.ContentFileExists())
                {
                    TextureData = File.ReadAllBytes(imagePath.Content.ContentFilePath());
                }
            }
        }
    }

    private static OSTheme GetThemeForLayout(string theme)
    {
        if (theme == null)
            return OSTheme.HacknetBlue;
        switch (theme.ToLower())
        {
            case "blue":
                return OSTheme.HacknetBlue;
            case "green":
                return OSTheme.HackerGreen;
            case "greencompact":
                return OSTheme.GreenCompact;
            case "white":
            case "csec":
                return OSTheme.HacknetWhite;
            case "mint":
            case "teal":
                return OSTheme.HacknetMint;
            case "colamaeleon":
            case "cola":
                return OSTheme.Colamaeleon;
            case "riptide":
                return OSTheme.Riptide;
            case "riptide2":
                return OSTheme.Riptide2;
            default:
                return OSTheme.HacknetPurple;
        }
    }

    public void ApplyTo(OS os)
    {
        if (!Loaded)
            throw new InvalidOperationException("Can't apply a custom theme before it has finished loading!");
            
        ThemeManager.switchTheme(os, OSTheme.HacknetBlue);
                
        foreach (var setting in ThemeInfo.Children)
        {
            if (OSColorFieldsFast.TryGetValue(setting.Name, out var field))
            {
                ref Color fieldRef = ref field(os);
                fieldRef = Utils.convertStringToColor(setting.Content);
            }
            else if (setting.Name == "UseAspectPreserveBackgroundScaling")
            {
                if (bool.TryParse(setting.Content, out var preserve))
                    os.UseAspectPreserveBackgroundScaling = preserve;
            }
            else if (setting.Name == "themeLayoutName")
            {
                ThemeManager.switchThemeLayout(os, GetThemeForLayout(setting.Content));
            }
        }

        ThemeManager.backgroundImage = BackgroundImage;
        ThemeManager.LastLoadedCustomThemePath = Path;
        ThemeManager.currentTheme = OSTheme.Custom;
        os.RefreshTheme();
    }

    public void Dispose()
    {
        BackgroundImage?.Dispose();
    }
}