using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hacknet;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Util.XML;

namespace Pathfinder.Util
{
    public class CachedCustomTheme
    {
        private struct FastColorField
        {
            public AccessTools.FieldRef<OS, Color> Ref;
        }
        
        private static readonly Dictionary<string, FastColorField> OSFieldsFast = new Dictionary<string, FastColorField>();
        
        [Initialize]
        internal static void Initialize()
        {
            foreach (var field in AccessTools.GetDeclaredFields(typeof(OS)).Where(x => x.FieldType == typeof(Color)))
            {
                OSFieldsFast.Add(field.Name, new FastColorField
                {
                    Ref = AccessTools.FieldRefAccess<OS, Color>(field)
                });
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
                    if (imagePath.Content.HasContent() && imagePath.Content.ContentFileExists())
                    {
                        using (FileStream imageSteam = File.OpenRead(imagePath.Content.ContentFilePath()))
                        {
                            BackgroundImage = Texture2D.FromStream(GuiData.spriteBatch.GraphicsDevice, imageSteam);
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

        public void ApplyTo(OS os)
        {
            if (!Loaded)
                throw new InvalidOperationException("Can't apply a custom theme before it has finished loading!");
                
            foreach (var setting in ThemeInfo.Children)
            {
                if (OSFieldsFast.TryGetValue(setting.Name, out var field))
                {
                    ref Color fieldRef = ref field.Ref(os);
                    fieldRef = Utils.convertStringToColor(setting.Content);
                }
            }

            ThemeManager.backgroundImage = BackgroundImage;
            ThemeManager.LastLoadedCustomThemePath = Path;
        }
    }
}