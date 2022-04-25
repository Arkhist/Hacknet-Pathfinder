using BepInEx.Configuration;
using Pathfinder.GUI;
using Microsoft.Xna.Framework;

namespace Pathfinder.Options;

public interface IPluginOption
{
    PluginOptionTab Tab { get; set; }
    string Id { get; }
    PluginOptionDrawData DrawData { get; set; }
    void OnRegistered();
    void OnDraw(GameTime gameTime);
    void OnSave(ConfigFile config);
    void OnLoad(ConfigFile config);
}

public abstract class BasePluginOption<ValueT> : IPluginOption
{
    public PluginOptionTab Tab { get; set; }

    public PluginOptionDrawData DrawDataField;
    public PluginOptionDrawData DrawData { get => DrawDataField; set => DrawDataField = value; }
    public int HacknetGuiId { get; private set; }

    public virtual ValueT Value { get; set; }
    public virtual ValueT DefaultValue { get; set; } = default;
    public virtual string HeaderText { get; protected set; }
    public virtual string DescriptionText { get; protected set; }
    public virtual string ConfigDescription { get; protected set; }
    public virtual string Id { get; private set; }

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

    public virtual void OnRegistered()
    {
        HacknetGuiId = PFButton.GetNextID();
        Id = string.Concat(HeaderText.Where(c => !char.IsWhiteSpace(c) && c != '='));
    }

    public abstract void OnDraw(GameTime gameTime);
    public virtual void OnSave(ConfigFile config)
    {
        config.Bind<ValueT>(Tab.Id, Id, DefaultValue, ConfigDescription);
    }
    public virtual void OnLoad(ConfigFile config)
    {
        if(config.TryGetEntry<ValueT>(Tab.Id, Id, out var entry))
            Value = entry.Value;
        else Value = DefaultValue;
    }
}