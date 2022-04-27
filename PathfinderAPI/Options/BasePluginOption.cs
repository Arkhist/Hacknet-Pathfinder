using BepInEx.Configuration;
using Pathfinder.GUI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Hacknet;

namespace Pathfinder.Options;

public interface IPluginOption
{
    PluginOptionTab Tab { get; set; }
    string Id { get; }
    PluginOptionDrawData DrawData { get; set; }
    void LoadContent();
    void OnDraw(GameTime gameTime);
    void OnSave(ConfigFile config);
    void OnLoad(ConfigFile config);
}

public abstract class BasePluginOption<ValueT> : IPluginOption
{
    public PluginOptionTab Tab { get; set; }

    public PluginOptionDrawData DrawData { get; set; }
    public int HacknetGuiId { get; private set; }

    public virtual ValueT Value { get; set; }
    public virtual ValueT DefaultValue { get; set; } = default;
    public virtual string HeaderText { get; protected set; }
    public virtual string DescriptionText { get; protected set; }
    public virtual string ConfigDescription { get; protected set; }
    public virtual string Id { get; }

    public Color HeaderColor { get; set; } = Color.White;
    public Color DescriptionColor { get; set; } = Color.White;

    public SpriteFont HeaderFont { get; set; }
    public SpriteFont DescriptionFont { get; set; }

    public Vector2 HeaderTextSize => HeaderFont.MeasureString(HeaderText);
    public Vector2 DescriptionTextSize => DescriptionFont.MeasureString(DescriptionText);

    protected BasePluginOption(string headerText, string descriptionText = null, ValueT defaultValue = default, string configDesc = null, string id = null)
    {
        HeaderText = headerText;
        DescriptionText = descriptionText;
        DefaultValue = defaultValue;
        ConfigDescription = configDesc;
        Id = OptionsManager.GetIdFrom(HeaderText, id);
    }

    public bool TrySetHeaderText(string text)
    {
        HeaderText = text;
        return HeaderText == text;
    }

    public bool TrySetDescriptionText(string text)
    {
        DescriptionText = text;
        return DescriptionText == text;
    }

    public bool TrySetConfigDescription(string desc)
    {
        ConfigDescription = desc;
        return ConfigDescription == desc;
    }

    public virtual void LoadContent()
    {
        HacknetGuiId = PFButton.GetNextID();
        HeaderFont ??= GuiData.font;
        DescriptionFont ??= GuiData.smallfont;
        SetSize();
    }

    public virtual void SetSize()
    {
        var headerSize = HeaderTextSize;
        var descSize = DescriptionTextSize;
        if(DrawData.Width == null)
            DrawData = DrawData.Set(width: (int)(Math.Max(headerSize.X, descSize.X + 32)));
        if(DrawData.Height == null)
            DrawData = DrawData.Set(height: (int)(headerSize.Y + descSize.Y));
    }

    public abstract void OnDraw(GameTime gameTime);
    public virtual void OnSave(ConfigFile config)
    {
        config.Bind<ValueT>(Tab.Id, Id, DefaultValue, ConfigDescription ?? "");
    }
    public virtual void OnLoad(ConfigFile config)
    {
        if(config.TryGetEntry<ValueT>(Tab.Id, Id, out var entry))
            Value = entry.Value;
        else Value = DefaultValue;
    }

    protected void DrawString(Vector2 pos, string text, Color? color = null, SpriteFont font = null)
    {
        Tab.Batch.DrawString(font ?? GuiData.font, text, pos, color ?? Color.White);
    }
}