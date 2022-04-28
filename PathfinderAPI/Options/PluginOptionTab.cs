using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Hacknet;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.GUI;

namespace Pathfinder.Options;

public class PluginOptionTab : IReadOnlyDictionary<string, IPluginOption>
{
    private IDictionary<string, IPluginOption> options = new Dictionary<string, IPluginOption>();
    public string Id { get; private set; }
    public string TabName { get; private set; }

    public SpriteBatch Batch { get; internal set; }
    public int HacknetGuiId { get; }
    public bool IsLoaded { get; internal set; }
    public Rectangle ButtonRect { get; set; } = new Rectangle
    {
        X = 0,
        Y = 0,
        Width = 128,
        Height = 20
    };

    public Point ButtonPosition => ButtonRect.Location + ButtonOffset;
    public Point ButtonOffset { get; protected set; }
    public virtual bool TrySetButtonOffset(Point offset)
    {
        ButtonOffset = offset;
        return ButtonOffset == offset;
    }

    public PluginOptionTab(string tabName, string tabId = null)
    {
        TabName = tabName;
        Id = OptionsManager.GetIdFrom(tabName, tabId);
        HacknetGuiId = PFButton.GetNextID();
    }

    public int Count => options.Count;
    public IEnumerable<string> Keys => options.Keys;
    public IEnumerable<IPluginOption> Values => options.Values;
    public IPluginOption this[string key] => options[key];

    public PluginOptionTab AddOption(IPluginOption option)
    {
        options.Add(option.Id, option);
        option.Tab = this;
        if(IsLoaded)
        {
            option.LoadContent();
        }
        return this;
    }

    public IPluginOption GetOption(string id)
    {
        if(TryGetValue(id, out var result))
            return result;
        return null;
    }

    public OptionT GetOption<OptionT>(string id) where OptionT : IPluginOption
        => (OptionT)GetOption(id);

    public OptionT GetOptionAs<OptionT>(string id) where OptionT : class, IPluginOption
        => GetOption(id) as OptionT;

    public bool RemoveOption(IPluginOption option)
    {
        return RemoveOption(option.Id);
    }

    public bool RemoveOption(string id)
    {
        return options.Remove(id);
    }

    public virtual void LoadContent()
    {
        Batch = GuiData.spriteBatch;
        foreach(var opt in options)
            opt.Value.LoadContent();
        IsLoaded = true;
    }

    public virtual void OnSave(ConfigFile config)
    {
        foreach(var opt in options)
            opt.Value.OnSave(config);
    }

    public virtual void OnLoad(ConfigFile config)
    {
        foreach(var opt in options)
            opt.Value.OnLoad(config);
    }

    public virtual void OnDraw(GameTime gameTime)
    {
        if (PathfinderOptionsMenu.CurrentTabId == null)
            PathfinderOptionsMenu.SetCurrentTab(this);
        var active = PathfinderOptionsMenu.CurrentTabId == Id;
        // Display tab button
        if (Button.doButton(HacknetGuiId,
            ButtonPosition.X,
            ButtonPosition.Y,
            ButtonRect.Width,
            ButtonRect.Height,
            TabName,
            active ? Color.Green : Color.Gray))
        {
            PathfinderOptionsMenu.SetCurrentTab(this);
            return;
        }

        if (PathfinderOptionsMenu.CurrentTabId != Id)
            return;

        // Display options
        int defOptionXPos = 80, defOptionYPos = 110;
        foreach (var option in this)
        {
            if(option.Value.TrySetOffset(new Vector2(defOptionXPos, defOptionYPos)))
                defOptionYPos += 10 + (int)option.Value.Size.Y;
            option.Value.OnDraw(gameTime);
        }
    }

    public IEnumerator<KeyValuePair<string, IPluginOption>> GetEnumerator()
        => options.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool ContainsKey(string key)
        => options.ContainsKey(key);

    public bool TryGetValue(string key, out IPluginOption value)
        => options.TryGetValue(key, out value);
}